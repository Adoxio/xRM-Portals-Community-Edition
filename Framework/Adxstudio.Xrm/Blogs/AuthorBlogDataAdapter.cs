/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Blogs
{
	public class AuthorBlogDataAdapter : IBlogDataAdapter
	{
		private readonly IBlogDataAdapter _dataAdapter;

		private AuthorBlogDataAdapter(EntityReference blog, Guid authorId, IDataAdapterDependencies dependencies, BlogSecurityInfo security)
		{
			if (blog == null) throw new ArgumentNullException("blog");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (security == null) throw new ArgumentNullException("security");

			if (blog.LogicalName != "adx_blog")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blog.LogicalName), "blog");
			}

			Blog = blog;
			AuthorId = authorId;
			Dependencies = dependencies;
			Security = security;

			_dataAdapter = new BlogDataAdapter(blog, dependencies);
		}

		public AuthorBlogDataAdapter(EntityReference blog, Guid authorId, IDataAdapterDependencies dependencies) : this(blog, authorId, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public AuthorBlogDataAdapter(Entity blog, Guid authorId, IDataAdapterDependencies dependencies) : this(blog.ToEntityReference(), authorId, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public AuthorBlogDataAdapter(IBlog blog, Guid authorId, IDataAdapterDependencies dependencies) : this(blog.Entity, authorId, dependencies) { }

		public AuthorBlogDataAdapter(EntityReference blog, Guid authorId, string portalName = null) : this(blog, authorId, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public AuthorBlogDataAdapter(Entity blog, Guid authorId, string portalName = null) : this(blog, authorId, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public AuthorBlogDataAdapter(IBlog blog, Guid authorId, string portalName = null) : this(blog, authorId, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected Guid AuthorId { get; private set; }

		protected EntityReference Blog { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected BlogSecurityInfo Security { get; private set; }

		public IBlog Select()
		{
			return _dataAdapter.Select();
		}

		public IEnumerable<IBlogPost> SelectPosts()
		{
			return SelectPosts(0);
		}

		public IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows = -1)
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

			var query = serviceContext.CreateQuery("adx_blogpost")
				.Where(post => post.GetAttributeValue<EntityReference>("adx_blogid") == Blog)
				.Where(post => post.GetAttributeValue<EntityReference>("adx_authorid") == new EntityReference("contact", AuthorId));

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

			var urlProvider = Dependencies.GetUrlProvider();
			var tagPathGenerator = new BlogArchiveApplicationPathGenerator(Dependencies);

			return new BlogPostFactory(serviceContext, urlProvider, Dependencies.GetWebsite(), tagPathGenerator).Create(query);
		}

		public int SelectPostCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchBlogPostCount(Blog.Id, addCondition => addCondition("adx_authorid", "eq", AuthorId.ToString()), !Security.UserHasAuthorPermission);
		}

		public IEnumerable<IBlogArchiveMonth> SelectArchiveMonths()
		{
			return _dataAdapter.SelectArchiveMonths();
		}

		public IEnumerable<IBlogPostWeightedTag> SelectWeightedTags(int weights)
		{
			return _dataAdapter.SelectWeightedTags(weights);
		}
	}
}
