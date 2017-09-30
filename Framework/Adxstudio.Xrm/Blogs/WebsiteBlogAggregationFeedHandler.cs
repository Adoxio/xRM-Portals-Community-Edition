/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Blogs
{
	public class WebsiteBlogAggregationFeedHandler : BlogDataAdapterFeedHandler
	{
		public WebsiteBlogAggregationFeedHandler() : this(null) { }

		public WebsiteBlogAggregationFeedHandler(string portalName) : base(portalName, typeof(WebsiteBlogAggregationFeedRouteHandler).FullName) { }

		protected override IBlogDataAdapter GetDataAdapter(IPortalContext portal, HttpContext context)
		{
			return new WebsiteBlogAggregationDataAdapter(
				new PortalContextDataAdapterDependencies(portal, PortalName, context.Request.RequestContext));
		}
	}
}
