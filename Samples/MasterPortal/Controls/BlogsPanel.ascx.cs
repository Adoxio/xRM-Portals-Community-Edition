/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Controls
{
	public partial class BlogsPanel : PortalUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void CreateBlogDataAdapter(object sender, ObjectDataSourceEventArgs args)
		{
			var newsBlogName = Portal.ServiceContext.GetSiteSettingValueByName(Website, "News Blog Name");

			args.ObjectInstance = 
				new WebsiteBlogAggregationDataAdapter(
					new PortalContextDataAdapterDependencies(Portal, requestContext:Request.RequestContext),
					null,
					serviceContext => GetAllBlogPostsInWebsiteExceptNews(ServiceContext, Website.Id, newsBlogName));
		}

		protected IQueryable<Entity> GetAllBlogPostsInWebsiteExceptNews(OrganizationServiceContext serviceContext, Guid websiteId, string newsBlogName)
		{
			var query = from post in serviceContext.CreateQuery("adx_blogpost")
						join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<Guid>("adx_blogid") equals blog.GetAttributeValue<Guid>("adx_blogid")
						where blog.GetAttributeValue<EntityReference>("adx_websiteid").Id == websiteId
						where blog.GetAttributeValue<string>("adx_name") != newsBlogName
						where post.GetAttributeValue<bool?>("adx_published") == true
						orderby post.GetAttributeValue<DateTime?>("adx_date") descending
						select post;

			return query;
		}
	}
}
