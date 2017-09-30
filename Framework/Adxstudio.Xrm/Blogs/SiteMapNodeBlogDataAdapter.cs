/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class SiteMapNodeBlogDataAdapter : IBlogDataAdapter
	{
		private readonly IBlogDataAdapter _blogDataAdapter;

		public SiteMapNodeBlogDataAdapter(SiteMapNode node, IDataAdapterDependencies dependencies)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}

			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			_blogDataAdapter = CreateBlogDataAdapter(node, dependencies);
		}

		public SiteMapNodeBlogDataAdapter(SiteMapNode node, string portalName = null) : this(node, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public IBlog Select()
		{
			return _blogDataAdapter.Select();
		}

		public IEnumerable<IBlogPost> SelectPosts()
		{
			return _blogDataAdapter.SelectPosts();
		}

		public IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows = -1)
		{
			return _blogDataAdapter.SelectPosts(startRowIndex, maximumRows);
		}

		public int SelectPostCount()
		{
			return _blogDataAdapter.SelectPostCount();
		}

		public IEnumerable<IBlogArchiveMonth> SelectArchiveMonths()
		{
			return _blogDataAdapter.SelectArchiveMonths();
		}

		public IEnumerable<IBlogPostWeightedTag> SelectWeightedTags(int weights)
		{
			return _blogDataAdapter.SelectWeightedTags(weights);
		}

		private static IBlogDataAdapter CreateBlogDataAdapter(SiteMapNode node, IDataAdapterDependencies dependencies)
		{
			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null)
			{
				return new WebsiteBlogAggregationDataAdapter(dependencies);
			}

			if (entityNode.Entity.LogicalName == "adx_blogpost")
			{
				var blog = entityNode.Entity.GetAttributeValue<EntityReference>("adx_blogid");

				if (blog != null)
				{
					return new BlogDataAdapter(blog, dependencies);
				}
			}

			if (entityNode.Entity.LogicalName == "adx_blog")
			{
				return CreateBlogDataAdapter(node, dependencies, entityNode.Entity.ToEntityReference());
			}

			return CreateBlogAggregationDataAdapter(node, dependencies);
		}

		private static IBlogDataAdapter CreateBlogDataAdapter(SiteMapNode node, IDataAdapterDependencies dependencies, EntityReference blog)
		{
			return CreateBlogDataAdapter(
				node,
				authorId => new AuthorBlogDataAdapter(blog, authorId, dependencies),
				(min, max) => new DateRangeBlogDataAdapter(blog, min, max, dependencies),
				tag => new TagBlogDataAdapter(blog, tag, dependencies),
				() => new BlogDataAdapter(blog, dependencies));
		}

		private static IBlogDataAdapter CreateBlogAggregationDataAdapter(SiteMapNode node, IDataAdapterDependencies dependencies)
		{
			return CreateBlogDataAdapter(
				node,
				authorId => new AuthorWebsiteBlogAggregationDataAdapter(authorId, dependencies),
				(min, max) => new DateRangeWebsiteBlogAggregationDataAdapter(min, max, dependencies),
				tag => new TagWebsiteBlogAggregationDataAdapter(tag, dependencies),
				() => new WebsiteBlogAggregationDataAdapter(dependencies));
		}

		private static IBlogDataAdapter CreateBlogDataAdapter(
			SiteMapNode node,
			Func<Guid, IBlogDataAdapter> createAuthorArchiveAdapter,
			Func<DateTime, DateTime, IBlogDataAdapter> createMonthArchiveAdapter,
			Func<string, IBlogDataAdapter> createTagArchiveAdapter,
			Func<IBlogDataAdapter> createDefaultAdapter)
		{
			Guid authorId;

			if (BlogSiteMapProvider.TryGetAuthorArchiveNodeAttribute(node, out authorId))
			{
				return createAuthorArchiveAdapter(authorId);
			}

			DateTime monthArchiveDate;

			if (BlogSiteMapProvider.TryGetMonthArchiveNodeAttribute(node, out monthArchiveDate))
			{
				return createMonthArchiveAdapter(monthArchiveDate.Date, monthArchiveDate.Date.AddMonths(1));
			}

			string tag;

			if (BlogSiteMapProvider.TryGetTagArchiveNodeAttribute(node, out tag))
			{
				return createTagArchiveAdapter(tag);
			}

			return createDefaultAdapter();
		}
	}
}
