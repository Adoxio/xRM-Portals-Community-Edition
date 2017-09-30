/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/



namespace Adxstudio.Xrm.Blogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Tagging;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;

	public class BlogDataAdapter : RatingDataAdapter, IBlogDataAdapter
	{
		private BlogDataAdapter(EntityReference blog, IDataAdapterDependencies dependencies, BlogSecurityInfo security) : base(blog, dependencies)
		{
			if (blog == null) throw new ArgumentNullException("blog");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (security == null) throw new ArgumentNullException("security");

			if (blog.LogicalName != "adx_blog")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blog.LogicalName), "blog");
			}

			Blog = blog;
			BlogDependencies = dependencies;
			Security = security;

		}

		public BlogDataAdapter(EntityReference blog, IDataAdapterDependencies dependencies) : this(blog, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public BlogDataAdapter(Entity blog, IDataAdapterDependencies dependencies) : this(blog.ToEntityReference(), dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public BlogDataAdapter(IBlog blog, IDataAdapterDependencies dependencies) : this(blog.Entity, dependencies) { }

		public BlogDataAdapter(EntityReference blog, string portalName = null) : this(blog, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public BlogDataAdapter(Entity blog, string portalName = null) : this(blog, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public BlogDataAdapter(IBlog blog, string portalName = null) : this(blog, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference Blog { get; private set; }

		protected IDataAdapterDependencies BlogDependencies { get; private set; }

		protected BlogSecurityInfo Security { get; private set; }

		public IBlog Select()
		{
			var serviceContext = BlogDependencies.GetServiceContext();
			var security = BlogDependencies.GetSecurityProvider();
			var urlProvider = BlogDependencies.GetUrlProvider();

			var entity = serviceContext.RetrieveSingle("adx_blog", "adx_blogid", this.Blog.Id, FetchAttribute.All);

			if (entity == null || !security.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				return null;
			}

			var path = urlProvider.GetApplicationPath(serviceContext, entity);

			return path == null ? null : new Blog(entity, path, BlogDependencies.GetBlogFeedPath(entity.Id));
		}

		public virtual IEnumerable<IBlogPost> SelectPosts()
		{
			return SelectPosts(0, -1);
		}

		public virtual IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IBlogPost[] { };
			}

			var serviceContext = BlogDependencies.GetServiceContext();

			var query = serviceContext.CreateQuery("adx_blogpost")
				.Where(post => post.GetAttributeValue<EntityReference>("adx_blogid") == Blog);

			if (!Security.UserHasAuthorPermission)
			{
				query = query.Where(post => post.GetAttributeValue<bool?>("adx_published") == true);
				}

			query = query.OrderByDescending(post => post.GetAttributeValue<DateTime?>("adx_date"));

			if (startRowIndex > 0)
			{
				query = query.Skip(startRowIndex);
			}

			if (maximumRows > 0)
			{
				query = query.Take(maximumRows);
			}

			var urlProvider = BlogDependencies.GetUrlProvider();
			var tagPathGenerator = new BlogArchiveApplicationPathGenerator(BlogDependencies);

			return new BlogPostFactory(serviceContext, urlProvider, BlogDependencies.GetWebsite(), tagPathGenerator).Create(query);
		}

		public virtual int SelectPostCount()
		{
			var serviceContext = BlogDependencies.GetServiceContext();

			return serviceContext.FetchBlogPostCount(Blog.Id, !Security.UserHasAuthorPermission);
		}

		public IEnumerable<IBlogArchiveMonth> SelectArchiveMonths()
		{
			var serviceContext = BlogDependencies.GetServiceContext();

			var counts = serviceContext.FetchBlogPostCountsGroupedByMonth(Blog.Id);
			var archivePathGenerator = new BlogArchiveApplicationPathGenerator(BlogDependencies);

			return counts.Select(c =>
			{
				var month = new DateTime(c.Item1, c.Item2, 1, 0, 0, 0, DateTimeKind.Utc);

				return new BlogArchiveMonth(month, c.Item3, archivePathGenerator.GetMonthPath(month, Blog));
			}).OrderByDescending(e => e.Month);
		}

		public IEnumerable<IBlogPostWeightedTag> SelectWeightedTags(int weights)
		{
			var serviceContext = BlogDependencies.GetServiceContext();

			var infos = serviceContext.FetchBlogPostTagCounts(Blog.Id)
				.Select(c => new BlogPostTagInfo(c.Item1, c.Item2));

			var tagCloudData = new TagCloudData(weights, TagInfo.TagComparer, infos);
			var archivePathGenerator = new BlogArchiveApplicationPathGenerator(BlogDependencies);

			return tagCloudData.Select(e => new BlogPostWeightedTag(e.Name, archivePathGenerator.GetTagPath(e.Name, Blog), e.TaggedItemCount, e.Weight));
		}
	}
}
