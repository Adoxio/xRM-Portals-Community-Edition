/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	internal class BlogArchiveApplicationPathGenerator : IBlogArchiveApplicationPathGenerator
	{
		private readonly IDictionary<Guid, ApplicationPath> _blogPathCache = new Dictionary<Guid, ApplicationPath>();

		public BlogArchiveApplicationPathGenerator(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{
				throw new ArgumentNullException("dependencies");
			}

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public ApplicationPath GetAuthorPath(Guid authorId, EntityReference blog)
		{
			var blogPath = GetBlogApplicationPath(blog);

			if (blogPath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}author/{2}/".FormatWith(
					blogPath.AppRelativePath,
					blogPath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					authorId));
		}

		public ApplicationPath GetMonthPath(DateTime month, EntityReference blog)
		{
			var blogPath = GetBlogApplicationPath(blog);

			if (blogPath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}{2:yyyy}/{2:MM}/".FormatWith(
					blogPath.AppRelativePath,
					blogPath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					month));
		}

		public ApplicationPath GetTagPath(string tag, EntityReference blog)
		{
			var blogPath = GetBlogApplicationPath(blog);

			if (blogPath == null)
			{
				return null;
			}

			return ApplicationPath.FromAppRelativePath(
				"{0}{1}tags/{2}".FormatWith(
					blogPath.AppRelativePath,
					blogPath.AppRelativePath.EndsWith("/") ? string.Empty : "/",
					HttpUtility.UrlPathEncode(tag)));
		}

		private ApplicationPath GetBlogApplicationPath(EntityReference blog)
		{
			if (blog == null)
			{
				return null;
			}

			if (blog.LogicalName != "adx_blog")
			{
				return null;
			}

			ApplicationPath blogPath;

			if (_blogPathCache.TryGetValue(blog.Id, out blogPath))
			{
				return blogPath;
			}

			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var entity = serviceContext.CreateQuery("adx_blog")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogid") == blog.Id
					&& e.GetAttributeValue<EntityReference>("adx_websiteid") == website);

			if (entity == null)
			{
				return null;
			}

			var urlProvider = Dependencies.GetUrlProvider();

			blogPath = urlProvider.GetApplicationPath(serviceContext, entity);

			_blogPathCache[blog.Id] = blogPath;

			return blogPath;
		}
	}
}
