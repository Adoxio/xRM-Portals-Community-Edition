/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Site.Pages;

namespace Site.Areas.Blogs.Pages
{
	public partial class Blog : PortalPage
	{
		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext());

		protected void Page_Load(object sender, EventArgs e) { }

		protected void CreateBlogDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = new BlogDataAdapter(_portal.Value.Entity, new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext));
		}
	}
}
