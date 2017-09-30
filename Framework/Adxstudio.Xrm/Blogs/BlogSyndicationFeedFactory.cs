/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	internal class BlogSyndicationFeedFactory
	{
		private readonly IBlogDataAdapter _dataAdapter;

		public BlogSyndicationFeedFactory(IBlogDataAdapter dataAdapter)
		{
			if (dataAdapter == null)
			{
				throw new ArgumentNullException("dataAdapter");
			}

			_dataAdapter = dataAdapter;
		}

		public SyndicationFeed CreateFeed(IPortalContext portal, HttpContext context, string selfRouteName, int maximumItems)
		{
			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var blog = _dataAdapter.Select();

			if (blog == null)
			{
				throw new InvalidOperationException("Blog not found.");
			}

			var posts = _dataAdapter.SelectPosts(0, maximumItems).ToArray();

			var feedLastUpdatedTime = posts.Any() ? new DateTimeOffset(posts.First().PublishDate) : DateTimeOffset.UtcNow;
			var blogHtmlUri = new Uri(context.Request.Url, blog.ApplicationPath.AbsolutePath);

			var feed = new SyndicationFeed(posts.Select(p => GetFeedItem(p, context)))
			{
				Id = "uuid:{0};{1}".FormatWith(blog.Id, feedLastUpdatedTime.Ticks),
				Title = SyndicationContent.CreatePlaintextContent(blog.Title),
				Description = SyndicationContent.CreateHtmlContent(blog.Summary.ToString()),
				LastUpdatedTime = feedLastUpdatedTime,
				BaseUri = new Uri(context.Request.Url, "/")
			};

			var selfPath = RouteTable.Routes.GetVirtualPath(context.Request.RequestContext, selfRouteName, new RouteValueDictionary
			{
				{ "__portalScopeId__", portal.Website.Id },
				{ "id", blog.Id }
			});

			if (selfPath != null)
			{
				feed.Links.Add(SyndicationLink.CreateSelfLink(new Uri(context.Request.Url, ApplicationPath.FromPartialPath(selfPath.VirtualPath).AbsolutePath), "application/atom+xml"));
			}

			feed.Links.Add(SyndicationLink.CreateAlternateLink(blogHtmlUri, "text/html"));

			return feed;
		}

		private static SyndicationItem GetFeedItem(IBlogPost post, HttpContext context)
		{
			if (post == null)
			{
				throw new ArgumentNullException("post");
			}

			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var item = new SyndicationItem
			{
				Id = "uuid:{0};{1}".FormatWith(post.Id, post.LastUpdatedTime.Ticks),
				Title = SyndicationContent.CreatePlaintextContent(post.Title),
				LastUpdatedTime = post.LastUpdatedTime,
				PublishDate = post.PublishDate
			};

			var postHtmlUri = new Uri(context.Request.Url, post.ApplicationPath.AbsolutePath);

			item.Authors.Add(new SyndicationPerson { Name = post.Author.Name });
			item.Links.Add(SyndicationLink.CreateAlternateLink(postHtmlUri, "text/html"));

			if (post.HasExcerpt)
			{
				item.Summary = SyndicationContent.CreateHtmlContent(post.Summary.ToString());
			}
			else
			{
				item.Content = SyndicationContent.CreateHtmlContent(post.Content.ToString());
			}

			return item;
		}
	}
}
