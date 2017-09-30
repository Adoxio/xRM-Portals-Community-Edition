/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Blogs
{
	internal class BlogPostFactory
	{
		private readonly OrganizationServiceContext _serviceContext;
		private readonly IBlogArchiveApplicationPathGenerator _archiveApplicationPathGenerator;
		private readonly IEntityUrlProvider _urlProvider;
		private readonly EntityReference _website;

		public BlogPostFactory(OrganizationServiceContext serviceContext, IEntityUrlProvider urlProvider, EntityReference website, IBlogArchiveApplicationPathGenerator archiveApplicationPathGenerator)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (urlProvider == null)
			{
				throw new ArgumentNullException("urlProvider");
			}

			if (website == null)
			{
				throw new ArgumentNullException("website");
			}

			if (website.LogicalName != "adx_website")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", website.LogicalName), "website");
			}

			if (archiveApplicationPathGenerator == null)
			{
				throw new ArgumentNullException("archiveApplicationPathGenerator");
			}

			_serviceContext = serviceContext;
			_urlProvider = urlProvider;
			_website = website;
			_archiveApplicationPathGenerator = archiveApplicationPathGenerator;
		}

		public IEnumerable<IBlogPost> Create(IEnumerable<Entity> blogPostEntities)
		{
			var posts = blogPostEntities.ToArray();
			var postIds = posts.Select(e => e.Id).ToArray();

			var extendedDatas = _serviceContext.FetchBlogPostExtendedData(postIds, BlogCommentPolicy.Closed, _website.Id);
			var commentCounts = _serviceContext.FetchBlogPostCommentCounts(postIds);

			return posts.Select(e =>
			{
				var path = _urlProvider.GetApplicationPath(_serviceContext, e);

				if (path == null)
				{
					return null;
				}

				Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo> extendedDataValue;
				var extendedData = extendedDatas.TryGetValue(e.Id, out extendedDataValue)
					? extendedDataValue
					: new Tuple<string, string, BlogCommentPolicy, string[], IRatingInfo>(null, null, BlogCommentPolicy.Closed, new string[] { }, null);

				var authorReference = e.GetAttributeValue<EntityReference>("adx_authorid");
				var author = authorReference != null
					? new BlogAuthor(authorReference.Id, extendedData.Item1, extendedData.Item2, _archiveApplicationPathGenerator.GetAuthorPath(authorReference.Id, e.GetAttributeValue<EntityReference>("adx_blogid")))
					: new NullBlogAuthor() as IBlogAuthor;

				int commentCountValue;
				var commentCount = commentCounts.TryGetValue(e.Id, out commentCountValue) ? commentCountValue : 0;

				return new BlogPost(e, path, author, extendedData.Item3, commentCount, GetTags(e, extendedData.Item4), extendedData.Item5);
			})
			.Where(e => e != null)
			.ToArray();
		}

		private IEnumerable<IBlogPostTag> GetTags(Entity post, IEnumerable<string> tagNames)
		{
			if (post == null) throw new ArgumentNullException("post");

			return tagNames == null
				? new IBlogPostTag[] { }
				: tagNames
					.Distinct(TagInfo.TagComparer)
					.OrderBy(name => name)
					.Select(name => new BlogPostTag(name, _archiveApplicationPathGenerator.GetTagPath(name, post.GetAttributeValue<EntityReference>("adx_blogid"))))
					.ToArray();
		}
	}
}
