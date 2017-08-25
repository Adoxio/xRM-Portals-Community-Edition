/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Routing
{
	public class WebResourceRouteHandler : IRouteHandler
	{
		public virtual string PortalName { get; private set; }

		public WebResourceRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var webResourceName = requestContext.RouteData.Values["name"] as string;

			var entity = context.CreateQuery("webresource").FirstOrDefault(e => e.GetAttributeValue<string>("name") == webResourceName);

			if (entity == null)
			{
				var response = requestContext.HttpContext.Response;

				response.StatusCode = 404;
				response.ContentType = "text/plain";
				response.Write("Not Found");
				response.End();

				return null;
			}
			
			var handler = new WebResourceHandler(entity);
			return handler;
		}
	}
}
