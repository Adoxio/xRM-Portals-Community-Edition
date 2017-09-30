/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Site.Pages;

namespace Site.Areas.Blogs.Pages
{
	public partial class BlogArchive : PortalPage
	{
		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);

		protected void Page_Load(object sender, EventArgs e)
		{
			Guid blogId;

			if (TryGetCurrentBlog(out blogId))
			{
				SearchData.SelectParameters.Add("Blog", blogId.ToString());
				SearchData.Query = "+(@Query) +adx_blogid:@Blog";
			}
		}

		protected void CreateBlogDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = new SiteMapNodeBlogDataAdapter(System.Web.SiteMap.CurrentNode, new PortalContextDataAdapterDependencies(_portal.Value, requestContext: Request.RequestContext));
		}

		protected string GetDisplayUrl(object urlData)
		{
			if (urlData == null)
			{
				return string.Empty;
			}

			try
			{
				return new UrlBuilder(urlData.ToString()).ToString();
			}
			catch (FormatException)
			{
				return string.Empty;
			}
		}

		protected void SearchData_OnSelected(object sender, SearchDataSourceStatusEventArgs args)
		{
			// If the SearchDataSource reports that the index was not found, try to build the index.
			if (args.Exception is IndexNotFoundException)
			{
				using (var builder = args.Provider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}

				args.ExceptionHandled = true;

				// Redirect/refresh to let the user continue their search.
				Response.Redirect(Request.Url.PathAndQuery);
			}
		}

		private bool TryGetCurrentBlog(out Guid blogId)
		{
			blogId = default(Guid);

			var entity = _portal.Value.Entity;

			if (entity == null || entity.LogicalName != "adx_blog")
			{
				return false;
			}

			blogId = entity.Id;

			return true;
		}
	}
}
