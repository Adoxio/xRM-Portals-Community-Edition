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
	public class TagBlogDataAdapter : IBlogDataAdapter
	{
		private readonly IBlogDataAdapter _dataAdapter;

		private TagBlogDataAdapter(EntityReference blog, string tag, IDataAdapterDependencies dependencies, BlogSecurityInfo security)
		{
			if (blog == null) throw new ArgumentNullException("blog");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (security == null) throw new ArgumentNullException("security");

			if (blog.LogicalName != "adx_blog")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blog.LogicalName), "blog");
			}

			Blog = blog;
			Tag = tag;
			Dependencies = dependencies;
			Security = security;

			_dataAdapter = new BlogDataAdapter(blog, dependencies);
		}

		public TagBlogDataAdapter(EntityReference blog, string tag, IDataAdapterDependencies dependencies) : this(blog, tag, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public TagBlogDataAdapter(Entity blog, string tag, IDataAdapterDependencies dependencies) : this(blog.ToEntityReference(), tag, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public TagBlogDataAdapter(IBlog blog, string tag, IDataAdapterDependencies dependencies) : this(blog.Entity, tag, dependencies) { }

		public TagBlogDataAdapter(EntityReference blog, string tag, string portalName = null) : this(blog, tag, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public TagBlogDataAdapter(Entity blog, string tag, string portalName = null) : this(blog, tag, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public TagBlogDataAdapter(IBlog blog, string tag, string portalName = null) : this(blog, tag, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference Blog { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected BlogSecurityInfo Security { get; private set; }

		protected string Tag { get; private set; }

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
				.Join(serviceContext.CreateQuery("adx_blogpost_tag"), post => post.GetAttributeValue<Guid>("adx_blogpostid"), postTag => postTag.GetAttributeValue<Guid>("adx_blogpostid"), (post, postTag) => new { Post = post, PostTag = postTag })
				.Join(serviceContext.CreateQuery("adx_tag"), e => e.PostTag.GetAttributeValue<Guid>("adx_tagid"), tag => tag.GetAttributeValue<Guid>("adx_tagid"), (e, tag) => new { PostPostTag = e, Tag = tag })
				.Where(e => e.Tag.GetAttributeValue<string>("adx_name") == Tag)
				.Where(e => e.PostPostTag.Post.GetAttributeValue<EntityReference>("adx_blogid") == Blog);

			if (!Security.UserHasAuthorPermission)
			{
				query = query.Where(e => e.PostPostTag.Post.GetAttributeValue<bool?>("adx_published") == true);
			}

			query = query.OrderByDescending(e => e.PostPostTag.Post.GetAttributeValue<DateTime?>("adx_date"));

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

			return new BlogPostFactory(serviceContext, urlProvider, Dependencies.GetWebsite(), tagPathGenerator).Create(query.Select(e => e.PostPostTag.Post));
		}

		public int SelectPostCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchBlogPostCount(Blog.Id, Tag, !Security.UserHasAuthorPermission);
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
