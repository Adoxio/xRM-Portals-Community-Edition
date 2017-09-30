/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Collections.Generic;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Blogs
{
	public class TagWebsiteBlogAggregationDataAdapter : WebsiteBlogAggregationDataAdapter
	{
		public TagWebsiteBlogAggregationDataAdapter(string tag, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			if (string.IsNullOrWhiteSpace(tag))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "tag");
			}

			Tag = tag;
		}

		public TagWebsiteBlogAggregationDataAdapter(string tag, string portalName = null) : this(tag, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected string Tag { get; private set; }

		public override IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IBlogPost[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();
			var urlProvider = Dependencies.GetUrlProvider();

			// If multi-language is enabled, only select blogs that are language-agnostic or match the current language.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			var query = contextLanguageInfo.IsCrmMultiLanguageEnabled ?
				from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid").Id equals blog.GetAttributeValue<Guid>("adx_blogid")
					join postTag in serviceContext.CreateQuery("adx_blogpost_tag") on post.GetAttributeValue<Guid>("adx_blogpostid") equals postTag.GetAttributeValue<EntityReference>("adx_blogpostid").Id
					join tag in serviceContext.CreateQuery("adx_tag") on postTag.GetAttributeValue<EntityReference>("adx_tagid").Id equals tag.GetAttributeValue<Guid>("adx_tagid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					where blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null || blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id
					where postTag.GetAttributeValue<EntityReference>("adx_blogpostid") != null && postTag.GetAttributeValue<EntityReference>("adx_tagid") != null
					where tag.GetAttributeValue<string>("adx_name") == Tag
					where post.GetAttributeValue<EntityReference>("adx_blogid") != null && post.GetAttributeValue<bool?>("adx_published") == true
					orderby post.GetAttributeValue<DateTime?>("adx_date") descending
					select post
				:
				from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<EntityReference>("adx_blogid").Id equals blog.GetAttributeValue<Guid>("adx_blogid")
					join postTag in serviceContext.CreateQuery("adx_blogpost_tag") on post.GetAttributeValue<Guid>("adx_blogpostid") equals postTag.GetAttributeValue<EntityReference>("adx_blogpostid").Id
					join tag in serviceContext.CreateQuery("adx_tag") on postTag.GetAttributeValue<EntityReference>("adx_tagid").Id equals tag.GetAttributeValue<Guid>("adx_tagid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					where postTag.GetAttributeValue<EntityReference>("adx_blogpostid") != null && postTag.GetAttributeValue<EntityReference>("adx_tagid") != null
					where tag.GetAttributeValue<string>("adx_name") == Tag
					where post.GetAttributeValue<EntityReference>("adx_blogid") != null && post.GetAttributeValue<bool?>("adx_published") == true
					orderby post.GetAttributeValue<DateTime?>("adx_date") descending 
					select post;
			
			var blogPostFactory = new BlogPostFactory(serviceContext, urlProvider, Website, new WebsiteBlogAggregationArchiveApplicationPathGenerator(Dependencies));
			var blogReadPermissionCache = new Dictionary<Guid, bool>();

			if (maximumRows < 0)
			{
				return blogPostFactory.Create(query.ToArray()
					.Where(e => TryAssertBlogPostRight(serviceContext, security, e, CrmEntityRight.Read, blogReadPermissionCache))
					.Skip(startRowIndex));
			}

			var pagedQuery = query;

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => TryAssertBlogPostRight(serviceContext, security, e, CrmEntityRight.Read, blogReadPermissionCache),
				2);

			return blogPostFactory.Create(paginator.Select(startRowIndex, maximumRows));
		}

		public override int SelectPostCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchBlogPostCountForWebsite(Website.Id, Tag);
		}
	}
}
