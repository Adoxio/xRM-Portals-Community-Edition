/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class DateRangeWebsiteBlogAggregationDataAdapter : WebsiteBlogAggregationDataAdapter
	{
		public DateRangeWebsiteBlogAggregationDataAdapter(DateTime min, DateTime max, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			Min = min;
			Max = max;
		}

		public DateRangeWebsiteBlogAggregationDataAdapter(DateTime min, DateTime max, string portalName = null) : this(min, max, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected DateTime Max { get; private set; }

		protected DateTime Min { get; private set; }

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
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<Guid>("adx_blogid") equals blog.GetAttributeValue<Guid>("adx_blogid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					where blog.GetAttributeValue<EntityReference>("adx_websitelanguageid") == null || blog.GetAttributeValue<EntityReference>("adx_websitelanguageid").Id == contextLanguageInfo.ContextLanguage.EntityReference.Id
					where post.GetAttributeValue<bool?>("adx_published") == true
					where post.GetAttributeValue<DateTime?>("adx_date") >= Min.Date && post.GetAttributeValue<DateTime?>("adx_date") <= Max.Date
					orderby post.GetAttributeValue<DateTime?>("adx_date") descending 
					select post
				:
				from post in serviceContext.CreateQuery("adx_blogpost")
					join blog in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<Guid>("adx_blogid") equals blog.GetAttributeValue<Guid>("adx_blogid")
					where blog.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					where post.GetAttributeValue<bool?>("adx_published") == true
					where post.GetAttributeValue<DateTime?>("adx_date") >= Min.Date && post.GetAttributeValue<DateTime?>("adx_date") <= Max.Date
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

			return serviceContext.FetchBlogPostCountForWebsite(Website.Id, addCondition =>
			{
				addCondition("adx_date", "ge", Min.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				addCondition("adx_date", "le", Max.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			});
		}
	}
}
