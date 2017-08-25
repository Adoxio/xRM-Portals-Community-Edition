/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.Modules
{
	/// <summary>
	/// Sends application error details (using a <see cref="SmtpClient"/>) to configured recipients.
	/// </summary>
	/// <remarks>
	/// Configuration is done through a set of application settings. The only required application setting is the "Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.To" setting.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	///  <appSettings>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.Host" value="localhost"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.From" value="webmaster@contoso.com"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.To" value="recipient1@contoso.com,recipient2@contoso.com"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.StatusCodesExcluded" value="400,404"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.MaximumNotificationsPerMinute" value="100"/>
	///  </appSettings>
	///  <system.net>
	///   <mailSettings>
	///    <smtp from="webmaster@contoso.com">
	///     <network host="localhost"/>
	///    </smtp>
	///   </mailSettings>
	///  </system.net>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	public class ErrorNotifierModule : IHttpModule
	{
		private static readonly int _maximumNotificationsPerMinute = 100;
		internal static readonly ConcurrentDictionary<string, ErrorInfo> _errors = new ConcurrentDictionary<string, ErrorInfo>();

		internal class ErrorInfo
		{
			public readonly Exception Error;
			public readonly ConcurrentQueue<DateTimeOffset> Buffer;
			public int DropCount;
			public DateTimeOffset Timestamp;

			public ErrorInfo(Exception error)
			{
				Error = error;
				Buffer = new ConcurrentQueue<DateTimeOffset>();
				DropCount = 0;
				Timestamp = DateTimeOffset.UtcNow;
			}
		}

		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			application.Error += OnError;
		}

		protected virtual void OnError(object sender, EventArgs e)
		{
			try
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

				var application = sender as HttpApplication;
				var context = application.Context;

				if (!string.IsNullOrWhiteSpace(GetTo(context)))
				{
					// prefer Server.GetLastError() over HttpContext.Error

					var error = application.Server.GetLastError();
					var statusCode = GetStatusCode(error, context);

					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("statusCode={0}", statusCode));
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("error={0}", error.Message));

					if (!GetStatusCodesExcluded().Contains(statusCode))
					{
						context.Response.StatusCode = statusCode;

						Send(application, error);
					}
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		protected virtual void Send(HttpApplication application, Exception error)
		{
			// collect the errors in the buffer for throttling keyed by exception type

			var ei = _errors.GetOrAdd(error.Message, _ => new ErrorInfo(error));

			lock (ei.Buffer)
			{
				// clear expired items from buffer

				var now = DateTimeOffset.UtcNow;

				DateTimeOffset result;

				while (ei.Buffer.TryPeek(out result) && result.AddMinutes(1) < now)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Dequeue: {0}", result));
					ei.Buffer.TryDequeue(out result);
				}

				if (ei.Buffer.Count < GetMaximumNotificationsPerMinute())
				{
					// add a new item to buffer and reset the drop count

					ei.Buffer.Enqueue(now);
					ei.Timestamp = now;
					var dropCount = ei.DropCount;
					ei.DropCount = 0;

					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Enqueue: {0}", now));

					// continue with notification

					Send(application, error, dropCount);
				}
				else
				{
					// drop this error

					ei.DropCount++;

					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Dropped: {0}", ei.DropCount));
				}
			}
		}

		protected virtual void Send(HttpApplication application, Exception error, int dropped)
		{
			var context = application.Context;
			var trace = application.Context.Trace;
			var mail = GetMailMessage(error, context, trace, dropped);

			Send(mail);
		}

		protected virtual IEnumerable<int> GetStatusCodesExcluded()
		{
			var excluded = GetConfigurationSettingValue(typeof(ErrorNotifierModule).FullName + ".StatusCodesExcluded");

			if (!string.IsNullOrWhiteSpace(excluded))
			{
				var parts = excluded.Split(new[] { ',', ';' });

				foreach (var part in parts)
				{
					int statusCode;

					if (int.TryParse(part, out statusCode))
					{
						yield return statusCode;
					}
				}
			}
		}

		protected virtual int GetMaximumNotificationsPerMinute()
		{
			var mepm = GetConfigurationSettingValue(typeof(ErrorNotifierModule).FullName + ".MaximumNotificationsPerMinute");

			if (!string.IsNullOrWhiteSpace(mepm))
			{
				int value;

				if (int.TryParse(mepm, out value))
				{
					return value;
				}
			}

			return _maximumNotificationsPerMinute;
		}

		protected virtual MailMessage GetMailMessage(Exception error, HttpContext context, TraceContext trace, int dropped)
		{
			var from = GetFrom(context);
			var to = GetTo(context);
			var subject = GetSubject(error, context);
			var body = GetBody(error, context, trace);

			// ensure that e-mail follows SMTP standards

			if (!body.EndsWith("\r\n"))
			{
				body += "\r\n";
			}

			var message = new MailMessage { IsBodyHtml = true, Subject = subject, Body = body };

			if (from != null) message.From = from;
			if (!string.IsNullOrWhiteSpace(to)) message.To.Add(to.Trim(new[] { ';', ',' }).Replace(';', ','));

			foreach (var header in GetHeaders(error, context, dropped))
			{
				if (!string.IsNullOrEmpty(header.Value))
				{
					message.Headers.Add(header.Key, header.Value);
				}
			}

			foreach (var attachment in GetAttachments(error, context))
			{
				message.Attachments.Add(attachment);
			}

			return message;
		}

		protected virtual void Send(MailMessage message)
		{
			var host = GetConfigurationSettingValue(typeof(ErrorNotifierModule).FullName + ".SmtpClient.Host") ?? GetConfigurationSettingValue("ADX_SMTP");
			using (var client = !string.IsNullOrWhiteSpace(host) ? new SmtpClient(host) : new SmtpClient())
			{
				client.Send(message);
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Message sent.");
		}

		protected virtual IEnumerable<KeyValuePair<string, string>> GetHeaders(Exception error, HttpContext context, int dropped)
		{
			yield return CreateHeader("X-ADXSTUDIO-SERVER_NAME", context.Request.ServerVariables["SERVER_NAME"]);
			yield return CreateHeader("X-ADXSTUDIO-SCRIPT_NAME", context.Request.ServerVariables["SCRIPT_NAME"]);
			yield return CreateHeader("X-ADXSTUDIO-QUERY_STRING", context.Request.ServerVariables["QUERY_STRING"]);
			yield return CreateHeader("X-ADXSTUDIO-STATUS_CODE", context.Response.StatusCode.ToString());
			yield return CreateHeader("X-ADXSTUDIO-STATUS", context.Response.Status);
			yield return CreateHeader("X-ADXSTUDIO-CONTENT-TYPE", context.Response.ContentType);
			yield return CreateHeader("X-ADXSTUDIO-CONTENT-ENCODING", context.Response.ContentEncoding.EncodingName);
			yield return CreateHeader("X-ADXSTUDIO-EXCEPTION-TYPE", error.GetType().FullName);
			yield return CreateHeader("X-ADXSTUDIO-EXCEPTION-DROPPED", dropped.ToString(CultureInfo.InvariantCulture));
		}

		private static KeyValuePair<string, string> CreateHeader(string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}

		protected virtual IEnumerable<Attachment> GetAttachments(Exception error, HttpContext context)
		{
			var unhandled = error as HttpUnhandledException;

			if (unhandled != null)
			{
				var message = unhandled.GetHtmlErrorMessage();

				if (!string.IsNullOrWhiteSpace(message))
				{
					var stream = new MemoryStream(Encoding.UTF8.GetBytes(message));
					yield return new Attachment(stream, "Error.html", MediaTypeNames.Text.Html);
				}
			}
		}

		protected virtual string GetTo(HttpContext context)
		{
			var to = GetConfigurationSettingValue(typeof(ErrorNotifierModule).FullName + ".SmtpClient.To") ?? GetConfigurationSettingValue("ADX_ERR_MAIL_TO");
			return to;
		}

		protected virtual MailAddress GetFrom(HttpContext context)
		{
			var setting = GetConfigurationSettingValue(typeof(ErrorNotifierModule).FullName + ".SmtpClient.From") ?? GetConfigurationSettingValue("ADX_SMTP_MAIL_FROM");
			var from = setting ?? "error@{0}".FormatWith(context.Request.ServerVariables["SERVER_NAME"]);
			return from != null ? new MailAddress(from) : null;
		}

		protected virtual string GetSubject(Exception error, HttpContext context)
		{
			return "{0} on {1}".FormatWith(context.Response.Status, context.Request.ServerVariables["SERVER_NAME"]);
		}

		protected virtual string GetBody(Exception error, HttpContext context, TraceContext trace)
		{
			const string body =
@"<html>
<body>
{0}
<hr/>
{1}
<hr/>
{2}
<br/>
{3}
<hr/>
{4}
</body>
</html>
";

			return body.FormatWith(GetException(error), GetRequest(context.Request), GetResponse(context.Response), GetTrace(trace), GetAssemblies());
		}

		protected virtual string GetException(Exception error)
		{
			if (error == null) return null;

			const string message =
@"<h3>{0}</h3>
<h5>{1}</h5>
<pre>{2}

{3}{4}
</pre>
";

			var sb = new StringBuilder();

			var current = error;

			while (current != null)
			{
				var errorMessage = HttpUtility.HtmlEncode(current.Message);
				var errorStackTrace = HttpUtility.HtmlEncode(current.StackTrace);
				var errorSource = HttpUtility.HtmlEncode(current.Source);

				var help = !string.IsNullOrWhiteSpace(current.HelpLink) ? "\r\n" + current.HelpLink : null;
				sb.Append(message.FormatWith(errorMessage, current.GetType().FullName, errorStackTrace, errorSource, help));
				current = current.InnerException;
			}

			return sb.ToString();
		}

		protected virtual string GetRequest(HttpRequest request)
		{
			var text = "<pre>\r\n{0} {1}{2} {3}\r\n".FormatWith(
				request.HttpMethod,
				request.Url.GetLeftPart(UriPartial.Authority),
				request.RawUrl,
				request.ServerVariables["SERVER_PROTOCOL"]);

			var sb = new StringBuilder(text);

			foreach (var key in request.Headers.AllKeys)
			{
				sb.Append("{0}: {1}\r\n".FormatWith(key, request.Headers[key]));
			}

			var position = request.InputStream.Position;
			request.InputStream.Position = 0;

			try
			{
				using (var reader = new StreamReader(request.InputStream))
				{
					sb.Append(reader.ReadToEnd());
				}
			}
			finally
			{
				request.InputStream.Position = position;
			}

			sb.Append("</pre>");

			return sb.ToString();
		}

		protected virtual string GetResponse(HttpResponse response)
		{
			var text = "<pre>\r\nHTTP/1.1 {0}\r\n".FormatWith(response.Status);

			var sb = new StringBuilder(text);

			sb.Append("Content-Type: {0}\r\n".FormatWith(response.ContentType));
			sb.Append("Content-Encoding: {0}\r\n".FormatWith(response.ContentEncoding.EncodingName));

			foreach (var key in response.Headers.AllKeys)
			{
				sb.Append("{0}: {1}\r\n".FormatWith(key, response.Headers[key]));
			}

			sb.Append("</pre>");

			return sb.ToString();
		}

		protected virtual string GetTrace(TraceContext trace)
		{
			try
			{
				return GetTraceByReflection(trace);
			}
			catch
			{
				return string.Empty;
			}
		}

		protected virtual string GetAssemblies()
		{
			const string message =
@"<pre>
{0}
</pre>
";

			var sb = new StringBuilder();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(assembly => assembly.FullName);

			foreach (var assembly in assemblies)
			{
				sb.Append(assembly.FullName + "\r\n");
			}

			return message.FormatWith(sb.ToString());
		}

		protected virtual int GetStatusCode(Exception error, HttpContext context)
		{
			var httpException = error as HttpException;

			if (httpException != null) return httpException.GetHttpCode();
			if (error != null) return 500;

			return context.Response.StatusCode;
		}

		protected virtual string GetConfigurationSettingValue(string configName)
		{
			return ConfigurationManager.AppSettings[configName];
		}

		private static string GetTraceByReflection(TraceContext trace)
		{
			// temporarily enable PageOutput (via the _isEnabled field) so that calling Render produces output

			var isEnabledField = typeof(TraceContext).GetField("_isEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
			var originalIsEnabledValue = isEnabledField.GetValue(trace);

			trace.IsEnabled = true;

			try
			{
				var sb = new StringBuilder();

				using (var htw = new Html32TextWriter(new StringWriter(sb, CultureInfo.InvariantCulture)))
				{
					typeof(TraceContext)
						.GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(trace, new object[] { htw });
				}

				return sb.ToString();
			}
			finally
			{
				// reset the _isEnabled field

				isEnabledField.SetValue(trace, originalIsEnabledValue);
			}
		}
	}
}
