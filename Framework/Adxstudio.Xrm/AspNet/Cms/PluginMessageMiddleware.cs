/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System.Threading.Tasks;
	using Adxstudio.Xrm.Security;
	using Microsoft.Owin;
	using Microsoft.Xrm.Client.Runtime.Serialization;
	using Microsoft.Xrm.Client.Services.Messages;
	using global::Owin;

	/// <summary>
	/// Settings related to the <see cref="PluginMessageMiddleware"/>.
	/// </summary>
	public class PluginMessageOptions
	{
		public PathString CallbackPath { get; set; }

		public PluginMessageOptions()
		{
			this.CallbackPath = new PathString("/WebNotification.axd");
		}
	}

	public class PluginMessageRequest
	{
		public string Authorization { get; set; }
		public string ContentType { get; set; }
		public string Body { get; set; }
	}

	/// <summary>
	/// Middleware that handles incoming messages posted from a CRM plugin.
	/// </summary>
	public abstract class PluginMessageMiddleware : OwinMiddleware
	{
		protected PluginMessageOptions Options { get; }

		protected PluginMessageMiddleware(OwinMiddleware next, IAppBuilder app, PluginMessageOptions options)
			: base(next)
		{
			this.Options = options;
		}

		public override async Task Invoke(IOwinContext context)
		{
			if (Equals(context.Request.Path, this.Options.CallbackPath))
			{
				var request = new PluginMessageRequest
				{
					Authorization = context.Request.Headers["Authorization"],
					ContentType = context.Request.ContentType,
					Body = context.GetRequestBody()
				};

				var message = this.GetPluginMessage(request.Body);

				if (message != null && WebNotificationCryptography.ValidateRequest(request.Authorization))
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Posting message");

					await this.PostAsync(context, message).WithCurrentCulture();
				}
			}

			await this.Next.Invoke(context);
		}

		protected abstract Task PostAsync(IOwinContext context, PluginMessage message);

		protected virtual PluginMessage GetPluginMessage(string body)
		{
			if (string.IsNullOrWhiteSpace(body)) return null;

			return body.DeserializeByJson(typeof(PluginMessage), null) as PluginMessage;
		}
	}
}
