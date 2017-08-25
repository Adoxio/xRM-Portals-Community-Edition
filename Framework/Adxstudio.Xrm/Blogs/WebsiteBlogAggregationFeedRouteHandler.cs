/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.Routing;

namespace Adxstudio.Xrm.Blogs
{
	public class WebsiteBlogAggregationFeedRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/feeds/portal/{__portalScopeId__}/blogs";

		public WebsiteBlogAggregationFeedRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		public virtual string PortalName { get; private set; }

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new WebsiteBlogAggregationFeedHandler(PortalName);
		}
	}
}
