/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogFeedHandler : BlogDataAdapterFeedHandler
	{
		public BlogFeedHandler() : this(null, null) { }

		public BlogFeedHandler(string portalName, Guid? id) : base(portalName, typeof(BlogFeedRouteHandler).FullName)
		{
			Id = id;
		}

		protected virtual Guid? Id { get; private set; }

		protected override IBlogDataAdapter GetDataAdapter(IPortalContext portal, HttpContext context)
		{
			Guid parsedId;
			var id = Id ?? (Guid.TryParse(context.Request.Params["id"], out parsedId) ? new Guid?(parsedId) : null);

			if (id == null)
			{
				throw new HttpException(400, "Unable to determine blog ID from request.");
			}

			return new BlogDataAdapter(
				new EntityReference("adx_blog", id.Value),
				new PortalContextDataAdapterDependencies(portal, PortalName, context.Request.RequestContext));
		}
	}
}
