/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Handlers
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Hosting;
	using System.Web.Routing;
	using Adxstudio.Xrm.AspNet;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.Caching;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Search;
	using Adxstudio.Xrm.Security;
	using Microsoft.Xrm.Client.Configuration;
	using Microsoft.Xrm.Client.Runtime.Serialization;
	using Microsoft.Xrm.Client.Services.Messages;

	/// <summary>
	/// HTTP Handler that processes web notification requests containing valid authorization token to invalidate the cache and rebuild the search index.
	/// </summary>
	public class WebNotificationHandler : IHttpHandler, IRouteHandler
	{
		private static readonly string[] SearchIndexApplicableMessages = new[] { "Build", "Publish", "PublishAll", "Delete", "Create", "Update", "Associate", "Disassociate" };
		private static readonly IEqualityComparer<string> MessageComparer = StringComparer.InvariantCultureIgnoreCase;

		public void ProcessRequest(HttpContext context)
		{
			var portalConfigType = ConfigurationManager.AppSettings["PortalConfigType"];
			var response = context.Response;

			if (string.IsNullOrWhiteSpace(context.Response.Headers.Get("Access-Control-Allow-Origin")))
			{
				response.AppendHeader("Access-Control-Allow-Origin", "*");
			}
			else
			{
				response.Headers["Access-Control-Allow-Origin"] = "*";
			}

			if (string.IsNullOrWhiteSpace(context.Response.Headers.Get("Access-Control-Allow-Methods")))
			{
				response.AppendHeader("Access-Control-Allow-Methods", "GET");
			}
			else
			{
				response.Headers["Access-Control-Allow-Methods"] = "GET";
			}

			if (portalConfigType == "online")
			{
				var onlineEnabled = "WebNotifications.Enabled".ResolveAppSetting().ToBoolean().GetValueOrDefault();

				if (!onlineEnabled)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Web Notifications are not enabled.");

					response.StatusCode = 404;
					response.StatusDescription = "Not Found";
					response.ContentType = "text/plain";
					response.Write("Not Found");
					response.End();

					return;
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Web Notifications are enabled with cloud application setting 'WebNotifications.Enabled'.");
			}

			if (context.Request.HttpMethod == "GET")
			{
				response.StatusCode = 200;
				response.StatusDescription = "OK";
				response.ContentType = "text/plain";
				response.Write("OK");
				response.End();

				return;
			}

			var request = new PluginMessageRequest
			{
				Authorization = context.Request.Headers["Authorization"],
				ContentType = context.Request.ContentType,
				Body = context.GetOwinContext().GetRequestBody()
			};

			HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => ProcessNotification(cancellationToken, request));
		}

		public bool IsReusable { get { return true; } }

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new WebNotificationHandler();
		}

		protected virtual OrganizationServiceCachePluginMessage GetPluginMessageFromJsonRequest(string body)
		{
			if (!string.IsNullOrWhiteSpace(body))
			{
				var message =
					body.DeserializeByJson(typeof(OrganizationServiceCachePluginMessage), null) as
						OrganizationServiceCachePluginMessage;

				if (message == null)
				{
					WebNotificationEventSource.Log.MessageDeserializationFailed();

					throw new Exception("The plug-in message is unspecified.");
				}

				return message;
			}

			WebNotificationEventSource.Log.MessageIsNull();
			
			ThrowOnNullOrWhiteSpace(body, ResourceManager.GetString("Unspecified_Request_Body_Exception"));

			return null;
		}

		private OrganizationServiceCachePluginMessage GetMessage(PluginMessageRequest request)
		{
			if (!request.ContentType.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase))
			{
				WebNotificationEventSource.Log.ContentTypeInvalid();

				return null;
			}

			return this.GetPluginMessageFromJsonRequest(request.Body);
		}

		private static void ThrowOnNullOrWhiteSpace(string text, string message)
		{
			if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException(message);
		}

		private void ProcessNotification(CancellationToken cancellationToken, PluginMessageRequest request)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				if (!WebNotificationCryptography.ValidateRequest(request.Authorization))
				{
					WebNotificationEventSource.Log.AuthorizationValidationFailed();

					return;
				}

				var message = this.GetMessage(request);

				if (message == null)
				{
					WebNotificationEventSource.Log.MessageInvalid();

					return;
				}

				CacheInvalidation.ProcessMessage(message);

				if (SearchIndexApplicableMessages.Contains(message.MessageName, MessageComparer))
				{
					var serviceContext = CrmConfigurationManager.CreateContext();
					SearchIndexBuildRequest.ProcessMessage(message, serviceContext: serviceContext);
				}
			}
			catch (TaskCanceledException e)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, e.Message);
			}
		}
	}
}
