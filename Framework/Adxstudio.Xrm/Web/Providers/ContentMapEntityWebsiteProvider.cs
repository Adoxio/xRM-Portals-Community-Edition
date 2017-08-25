/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Providers
{
	using System;
	using System.Web;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Services;

	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Client;

	internal class ContentMapEntityWebsiteProvider : AdxEntityWebsiteProvider
	{
		private readonly IContentMapProvider _contentMapProvider;

		public ContentMapEntityWebsiteProvider(IContentMapProvider contentMapProvider)
		{
			_contentMapProvider = contentMapProvider;
		}

		public override Entity GetWebsite(OrganizationServiceContext context, Entity entity)
		{
			if (HttpContext.Current != null)
			{
				var website = HttpContext.Current.GetWebsite();
				if (website != null)
				{
					return context.MergeClone(website.Entity);
				}
			}

			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				return null;
			}

			return _contentMapProvider.Using(map => GetWebsite(context, entity, map));
		}

		private Entity GetWebsite(OrganizationServiceContext context, Entity entity, ContentMap map)
		{
			switch (entity.LogicalName)
			{
				case "adx_weblink":
					WebLinkNode link;
					if (map.TryGetValue(entity, out link))
					{
						return ToWebsite(context, link.WebLinkSet.Website);
					}
					break;
				case "adx_webpage":
					WebPageNode page;
					if (map.TryGetValue(entity, out page))
					{
						return ToWebsite(context, page.Website);
					}
					break;
				case "adx_weblinkset":
					WebLinkSetNode linkset;
					if (map.TryGetValue(entity, out linkset))
					{
						return ToWebsite(context, linkset.Website);
					}
					break;
				case "adx_webfile":
					WebFileNode file;
					if (map.TryGetValue(entity, out file))
					{
						return ToWebsite(context, file.Website);
					}
					break;
				case "adx_sitemarker":
					SiteMarkerNode marker;
					if (map.TryGetValue(entity, out marker))
					{
						return ToWebsite(context, marker.Website);
					}
					break;
				case "adx_pagetemplate":
					PageTemplateNode template;
					if (map.TryGetValue(entity, out template))
					{
						return ToWebsite(context, template.Website);
					}
					break;
				case "adx_contentsnippet":
					ContentSnippetNode snippet;
					if (map.TryGetValue(entity, out snippet))
					{
						return ToWebsite(context, snippet.Website);
					}
					break;
				case "adx_websitelanguage":
					WebsiteLanguageNode websiteLanguageNode;
					if (map.TryGetValue(entity, out websiteLanguageNode))
					{
						return ToWebsite(context, websiteLanguageNode.Website);
					}
					break;
				case "adx_ideaforum":
					IdeaForumNode ideaForumNode;
					if (map.TryGetValue(entity, out ideaForumNode))
					{
						return ToWebsite(context, ideaForumNode.Website);
					}
					break;
				case "adx_communityforum":
					ForumNode forumNode;
					if (map.TryGetValue(entity, out forumNode))
					{
						return ToWebsite(context, forumNode.Website);
					}
					break;
				case "adx_blog":
					BlogNode blogNode;
					if (map.TryGetValue(entity, out blogNode))
					{
						return ToWebsite(context, blogNode.Website);
					}
					break;
				case "adx_publishingstate":
					PublishingStateNode publishingStateNode;
					if (map.TryGetValue(entity, out publishingStateNode))
					{
						return ToWebsite(context, publishingStateNode.Website);
					}
					break;
					}

			return base.GetWebsite(context, entity);
		}

		private static Entity ToWebsite(OrganizationServiceContext context, WebsiteNode node)
		{
			var website = node.AttachTo(context);

			return website;
		}
	}
}
