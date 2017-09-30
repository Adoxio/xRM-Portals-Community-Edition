/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Blogs
{
	public class DateRangeBlogDataAdapter : IBlogDataAdapter
	{
		private readonly IBlogDataAdapter _dataAdapter;

		private DateRangeBlogDataAdapter(EntityReference blog, DateTime min, DateTime max, IDataAdapterDependencies dependencies, BlogSecurityInfo security)
		{
			if (blog == null) throw new ArgumentNullException("blog");
			if (dependencies == null) throw new ArgumentNullException("dependencies");
			if (security == null) throw new ArgumentNullException("security");

			if (blog.LogicalName != "adx_blog")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", blog.LogicalName), "blog");
			}

			Blog = blog;
			Dependencies = dependencies;
			Security = security;
			Min = min;
			Max = max;

			_dataAdapter = new BlogDataAdapter(blog, dependencies);
		}

		public DateRangeBlogDataAdapter(EntityReference blog, DateTime min, DateTime max, IDataAdapterDependencies dependencies) : this(blog, min, max, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public DateRangeBlogDataAdapter(Entity blog, DateTime min, DateTime max, IDataAdapterDependencies dependencies) : this(blog.ToEntityReference(), min, max, dependencies, new BlogSecurityInfo(blog, dependencies)) { }

		public DateRangeBlogDataAdapter(IBlog blog, DateTime min, DateTime max, IDataAdapterDependencies dependencies) : this(blog.Entity, min, max, dependencies) { }

		public DateRangeBlogDataAdapter(EntityReference blog, DateTime min, DateTime max, string portalName = null) : this(blog, min, max, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public DateRangeBlogDataAdapter(Entity blog, DateTime min, DateTime max, string portalName = null) : this(blog, min, max, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		public DateRangeBlogDataAdapter(IBlog blog, DateTime min, DateTime max, string portalName = null) : this(blog, min, max, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected EntityReference Blog { get; private set; }

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected DateTime Max { get; private set; }

		protected DateTime Min { get; private set; }

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
				.Where(post => post.GetAttributeValue<DateTime?>("adx_date") >= Min.Date && post.GetAttributeValue<DateTime?>("adx_date") <= Max.Date);

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

			return serviceContext.FetchBlogPostCount(Blog.Id, addCondition =>
			{
				addCondition("adx_date", "ge", Min.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				addCondition("adx_date", "le", Max.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			}, !Security.UserHasAuthorPermission);
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
