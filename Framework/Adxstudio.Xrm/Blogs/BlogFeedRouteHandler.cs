/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogFeedRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/feeds/portal/{__portalScopeId__}/blogs/{id}";

		public BlogFeedRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		public virtual string PortalName { get; private set; }

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			Guid parsedId;
			var id = Guid.TryParse(requestContext.RouteData.Values["id"] as string, out parsedId) ? new Guid?(parsedId) : null;

			return new BlogFeedHandler(PortalName, id);
		}
	}
}
