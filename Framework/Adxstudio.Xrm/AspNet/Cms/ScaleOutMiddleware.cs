/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

/* this is how a scaleout notification look like. In the Invoke method below we parse this data and send it to telemetry.
{
	"version":"1.0",
	"status":"Activated",
	"operation":"Scale Out",	
	"context":{
		"timestamp":"2017-02-07T21:39:53.9926414Z",
		"id":"/subscriptions/ec279228-ce39-4966-b83b-b65d8e08a653/resourceGroups/resourcegroup-66cd1446-8b0d-4d86-91ca-32e144812f90/providers/microsoft.insights/autoscalesettings/autoscale-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw",
		"name":"autoscale-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw",
		"details":"Autoscale successfully started scale operation for resource 'sf-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw' from capacity '1' to capacity '8'",
		"subscriptionId":"ec279228-ce39-4966-b83b-b65d8e08a653",
		"resourceGroupName":"resourcegroup-66cd1446-8b0d-4d86-91ca-32e144812f90",
		"resourceName":"sf-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw",
		"resourceType":"microsoft.web/serverfarms",
		"resourceId":"/subscriptions/ec279228-ce39-4966-b83b-b65d8e08a653/resourceGroups/resourcegroup-66cd1446-8b0d-4d86-91ca-32e144812f90/providers/Microsoft.Web/serverfarms/sf-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw",
		"portalLink":"https://portal.azure.com/#resource/subscriptions/ec279228-ce39-4966-b83b-b65d8e08a653/resourceGroups/resourcegroup-66cd1446-8b0d-4d86-91ca-32e144812f90/providers/Microsoft.Web/serverfarms/sf-10b6e637-e9fb-447e-b1f2-b1285b8ff3e3-USw",
		"resourceRegion":"West US",
		"oldCapacity":"1",
		"newCapacity":"8"
	}
}
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Threading.Tasks;
	using Adxstudio.Xrm.Cms;
	using Microsoft.Owin;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// ScaleOut middleware to log jarvis logs whenever a scale-out/in operation happens.
	/// </summary>
	public class ScaleOutMiddleware : OwinMiddleware
	{
		/// <summary>
		/// path string for scaleout telemetry webhook.
		/// </summary>
		private readonly PathString callbackPath = new PathString("/_services/about/scaleout");

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaleOutMiddleware"/> class.
		/// </summary>
		/// <param name="next">middleware next in the queue.</param>
		public ScaleOutMiddleware(OwinMiddleware next) : base(next)
		{
		}

		/// <summary>
		/// ScaleOut middleware to add telemetry for scale-out events.
		/// </summary>
		/// <param name="context">Owin context</param>
		/// <returns>Returns task</returns>
		public override async Task Invoke(IOwinContext context)
		{
			if (object.Equals(context.Request.Path, this.callbackPath))
			{
				string body = context.GetRequestBody();

				if (!string.IsNullOrEmpty(body))
				{
					try
					{
						JObject payload = JsonConvert.DeserializeObject(body) as JObject;
						if (payload != null)
						{
							var operation = payload["operation"] != null ? payload["operation"].Value<string>() : null;
							var status = payload["status"] != null ? payload["status"].Value<string>() : null;
							var contextObject = payload["context"] != null ? payload["context"] as JObject : null;
							if (operation != null && status != null && contextObject != null)
							{
								var timestamp = contextObject["timestamp"] != null ? contextObject["timestamp"].Value<string>() : null;
								var details = contextObject["details"] != null ? contextObject["details"].Value<string>() : null;
								var resourceId = contextObject["resourceId"] != null ? contextObject["resourceId"].Value<string>() : null;
								var oldCapacity = contextObject["oldCapacity"] != null ? contextObject["oldCapacity"].Value<string>() : null;
								var newCapacity = contextObject["newCapacity"] != null ? contextObject["newCapacity"].Value<string>() : null;

								// Temp changes till this jarvis event this is not fixed
								ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("ScaleOut webhook notification. Status: {0}, Operation: {1}, TimeStamp: {2}, Details: {3}, ResourceId: {4}, OldCapacity: {5}, NewCapacity: {6}",
									status, operation, timestamp, details, resourceId, oldCapacity, newCapacity));
								CmsEventSource.Log.ScaleOutNotification(status, operation, timestamp, details, oldCapacity, newCapacity, resourceId);
							}
							else
							{
								ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("The payload for scaleout webhook notification is missing essential data to log data. Request.Body: {0}", body));
							}
						}
						else
						{
							ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Something is wrong in the scaleout webhook notification payload. {0}", body));
						}
					}
					catch (Exception ex)
					{
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Could not deserialize ScaleOut webhook notification payload. Request.Body: {0} Exception: {1}", body, ex.Message));
					}
				}
			}
			else
			{
				await Next.Invoke(context);
			}
		}
	}
}
