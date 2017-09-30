/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.IO;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Routing
{
	public class OrganizationServiceCachePluginMessageRouteHandler : IRouteHandler
	{
		public const string RoutePath = "{area}/cache/internal";

		public OrganizationServiceCachePluginMessageRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var body = GetRequestBody(requestContext);

			if (!string.IsNullOrWhiteSpace(body))
			{
				var message = body.DeserializeByJson(typeof(OrganizationServiceCachePluginMessage), null) as OrganizationServiceCachePluginMessage;
				return new OrganizationServiceCachePluginMessageHandler(PortalName, message);
			}

			return null;
		}

		private static string GetRequestBody(RequestContext requestContext)
		{
			using (var reader = new StreamReader(requestContext.HttpContext.Request.InputStream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
