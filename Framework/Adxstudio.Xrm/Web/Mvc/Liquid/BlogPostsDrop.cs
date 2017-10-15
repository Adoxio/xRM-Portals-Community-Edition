/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class BlogPostsDrop : PortalDrop
	{
		private readonly IBlogAggregationDataAdapter _adapter;
		private readonly Lazy<BlogPostDrop[]> _posts;
		
		public BlogPostsDrop(IPortalLiquidContext portalLiquidContext, 
									IDataAdapterDependencies dependencies,
									int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_date", string sortDirection = "desc") : base(portalLiquidContext)
		{
			Dependencies = dependencies;

			PortalLiquidContext = portalLiquidContext;

			SetParams(startRowIndex, pageSize, orderBy, sortDirection);

			_adapter = new WebsiteBlogAggregationDataAdapter(Dependencies, null, serviceContext => GetBlogPosts(serviceContext));

			_posts = new Lazy<BlogPostDrop[]>(() => _adapter.SelectPosts(StartRowIndex, PageSize).Select(e => new BlogPostDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public BlogPostsDrop(IPortalLiquidContext portalLiquidContext,
									IDataAdapterDependencies dependencies,
									IBlog blog,
									int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_date", string sortDirection = "desc")
			: base(portalLiquidContext)
		{
			Dependencies = dependencies;

			Blog = blog;

			PortalLiquidContext = portalLiquidContext;

			SetParams(startRowIndex, pageSize, orderBy, sortDirection);

			var blogAggregationDataAdapter = new WebsiteBlogAggregationDataAdapter(Dependencies, null, serviceContext => GetBlogPosts(serviceContext, Blog));

			_adapter = blogAggregationDataAdapter;

			_posts = new Lazy<BlogPostDrop[]>(() => _adapter.SelectPosts(StartRowIndex, PageSize).Select(e => new BlogPostDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public void SetParams(int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_date", string sortDirection = "desc")
		{
			StartRowIndex = startRowIndex;  PageSize = pageSize; OrderByKey = orderBy; SortDirection = sortDirection;
		}

		internal IBlog Blog { get; private set; }

		internal IPortalLiquidContext PortalLiquidContext { get; private set; }
		internal IDataAdapterDependencies Dependencies { get; private set; }

		public int StartRowIndex { get; private set; }
		public int PageSize { get; private set; }
		public string OrderByKey { get; private set; }
		public string SortDirection { get; private set; }

		public IEnumerable<BlogPostDrop> All
		{
			get
			{
				return _posts.Value.AsEnumerable();
			}
		}

		protected IQueryable<Entity> GetBlogPosts(OrganizationServiceContext serviceContext, IBlog blog = null)
		{
			IQueryable<Entity> query;

			if (blog != null)
			{
				query = string.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(SortDirection, "descending", StringComparison.InvariantCultureIgnoreCase)
							? from post in serviceContext.CreateQuery("adx_blogpost")
							  where post.GetAttributeValue<Guid>("adx_blogid") == blog.Id
							  orderby post[OrderByKey] descending
							  select post
							: from post in serviceContext.CreateQuery("adx_blogpost")
							  where post.GetAttributeValue<Guid>("adx_forumid") == blog.Id
							  orderby post[OrderByKey]
							  select post;
			}
			else
			{
				query = string.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(SortDirection, "descending", StringComparison.InvariantCultureIgnoreCase)
						? from post in serviceContext.CreateQuery("adx_blogpost")
							join b in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<Guid>("adx_blogid")
							equals b.GetAttributeValue<Guid>("adx_blogid")
							where b.GetAttributeValue<EntityReference>("adx_websiteid") == Dependencies.GetWebsite()
							where post.GetAttributeValue<bool?>("adx_published") == true
							orderby post[OrderByKey] descending
							select post
						: from post in serviceContext.CreateQuery("adx_blogpost")
							join b in serviceContext.CreateQuery("adx_blog") on post.GetAttributeValue<Guid>("adx_blogid")
								equals b.GetAttributeValue<Guid>("adx_blogid")
							where b.GetAttributeValue<EntityReference>("adx_websiteid") == Dependencies.GetWebsite()
							where post.GetAttributeValue<bool?>("adx_published") == true
							orderby post[OrderByKey]
							select post;
			}

			return query;
		}
	}
}
