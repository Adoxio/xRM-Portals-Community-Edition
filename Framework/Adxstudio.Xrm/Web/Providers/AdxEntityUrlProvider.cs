/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Providers
{
	public class AdxEntityUrlProvider : EntityUrlProvider
	{
		public AdxEntityUrlProvider(IEntityWebsiteProvider websiteProvider, string portalName = null) : base(websiteProvider)
		{
			PortalName = portalName;
		}

		protected string PortalName { get; private set; }

		public override ApplicationPath GetApplicationPath(OrganizationServiceContext context, Entity entity)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				return null;
			}

			if (entity.LogicalName == "adx_communityforumpost")
			{
				var thread = entity.GetRelatedEntity(context, "adx_communityforumthread_communityforumpost".ToRelationship());

				if (thread != null)
				{
					return GetForumPostApplicationPath(context, entity, thread);
				}
			}

			if (entity.LogicalName == "adx_blogpostcomment")
			{
				var post = entity.GetRelatedEntity(context, "adx_blogpost_blogpostcomment".ToRelationship());

				if (post != null)
				{
					return GetBlogPostCommentApplicationPath(context, entity, post);
				}
			}

			if (entity.LogicalName == "adx_shortcut")
			{
				return GetShortcutApplicationPath(context, entity);
			}

			if (entity.LogicalName == "adx_ideaforum")
			{
				return GetIdeaForumApplicationPath(context, entity);
			}

			if (entity.LogicalName == "adx_idea")
			{
				return GetIdeaApplicationPath(context, entity);
			}

			if (entity.LogicalName == "adx_issue")
			{
				return GetIssueApplicationPath(context, entity);
			}

			if (entity.LogicalName == "incident")
			{
				return GetIncidentApplicationPath(context, entity);
			}

			if (entity.LogicalName == "kbarticle")
			{
				return GetKbArticleApplicationPath(context, entity);
			}

			if (entity.LogicalName == "knowledgearticle")
			{
				return GetKnowledgeArticleApplicationPath(context, entity);
			}

			if (entity.LogicalName == "category")
			{
				return GetCategoryApplicationPath(context, entity);
			}

			// We want new behaviour for adx_webpages -- paths for this entity will now have a trailing slash ('/').
			if (entity.LogicalName == "adx_webpage")
			{
				var path = base.GetApplicationPath(context, entity);

				// If the path is an external URL (it shouldn't be, but just in case), return the original path untouched.
				if (path.ExternalUrl != null)
				{
					return path;
				}

				// If the path does not already have a trailing slash (it shouldn't), append one.
				return path.AppRelativePath.EndsWith("/")
					? path
					: ApplicationPath.FromAppRelativePath("{0}/".FormatWith(path.AppRelativePath));
			}

			// Support adx_webfiles with a parent adx_blogpost, instead of adx_webpage.
			if (entity.LogicalName == "adx_webfile" && entity.GetAttributeValue<EntityReference>("adx_blogpostid") != null)
			{
				var post = entity.GetRelatedEntity(context, "adx_blogpost_webfile".ToRelationship());

				if (post != null)
				{
					var postPath = GetApplicationPath(context, post);
					var filePartialUrl = entity.GetAttributeValue<string>("adx_partialurl");

					if (postPath != null && filePartialUrl != null)
					{
						return ApplicationPath.FromAppRelativePath("{0}/{1}".FormatWith(postPath.AppRelativePath.TrimEnd('/'), filePartialUrl));
					}
				}
			}

			var lookup = new Dictionary<string, Tuple<string[], Relationship, string, string, bool>>
			{
				{
					"adx_communityforumthread",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_communityforumthreadid" },
						"adx_communityforum_communityforumthread".ToRelationship(),
						"adx_communityforum",
						null,
						false)
				},
				{
					"adx_communityforum",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_partialurl" },
						"adx_webpage_communityforum".ToRelationship(),
						"adx_webpage",
						"Forums",
						false)
				},
				{
					"adx_event",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_partialurl" },
						"adx_webpage_event".ToRelationship(),
						"adx_webpage",
						"Events",
						false)
				},
				{
					"adx_survey",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_partialurl" },
						"adx_webpage_survey".ToRelationship(),
						"adx_webpage",
						"Surveys",
						false)
				},
				{
					"adx_blog",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_partialurl" },
						"adx_webpage_blog".ToRelationship(),
						"adx_webpage",
						null,
						true)
				},
				{
					"adx_blogpost",
					new Tuple<string[], Relationship, string, string, bool>(
						new[] { "adx_partialurl", "adx_blogpostid" },
						"adx_blog_blogpost".ToRelationship(),
						"adx_blog",
						null,
						true)
				},
			};

			Tuple<string[], Relationship, string, string, bool> urlData;

			if (lookup.TryGetValue(entity.LogicalName, out urlData))
			{
				var partialUrlLogicalName = urlData.Item1.FirstOrDefault(logicalName =>
				{
					var partialUrlValue = entity.GetAttributeValue(logicalName);

					return partialUrlValue != null && !string.IsNullOrWhiteSpace(partialUrlValue.ToString());
				});

				if (partialUrlLogicalName == null)
				{
					return null;
				}

				var relationship = urlData.Item2;
				var siteMarker = urlData.Item4;
				var addTrailingSlash = urlData.Item5;

				var websiteRelativeUrl = GetApplicationPath(context, entity, partialUrlLogicalName, relationship, GetApplicationPath, siteMarker);

				if (websiteRelativeUrl != null)
				{
					if (addTrailingSlash && websiteRelativeUrl.PartialPath != null && !websiteRelativeUrl.PartialPath.EndsWith("/"))
					{
						websiteRelativeUrl = ApplicationPath.FromPartialPath("{0}/".FormatWith(websiteRelativeUrl.PartialPath));
					}

					var website = WebsiteProvider.GetWebsite(context, entity);

					var path = WebsitePathUtility.ToAbsolute(website, websiteRelativeUrl.PartialPath);

					return ApplicationPath.FromPartialPath(path);
				}
			}

			return base.GetApplicationPath(context, entity);
		}

		private ApplicationPath GetBlogPostCommentApplicationPath(OrganizationServiceContext context, Entity comment, Entity post)
		{
			var postPath = GetApplicationPath(context, post);

			if (postPath == null || postPath.AppRelativePath == null || postPath.AppRelativePath.Contains("#"))
			{
				return postPath;
			}

			return ApplicationPath.FromAppRelativePath("{0}#{1}".FormatWith(postPath.AppRelativePath, BlogPostComment.GetAnchorName(comment.Id)));
		}

		private ApplicationPath GetForumPostApplicationPath(OrganizationServiceContext context, Entity post, Entity thread)
		{
			var threadPath = GetApplicationPath(context, thread);

			if (threadPath == null || threadPath.AppRelativePath == null || threadPath.AppRelativePath.Contains("#"))
			{
				return threadPath;
			}

			return ApplicationPath.FromAppRelativePath("{0}#{1}".FormatWith(threadPath.AppRelativePath, post.Id));
		}

		private ApplicationPath GetShortcutApplicationPath(OrganizationServiceContext context, Entity shortcut)
		{
			shortcut.AssertEntityName("adx_shortcut");

			var relatedWebPage = shortcut.GetRelatedEntity(context, "adx_webpage_shortcut".ToRelationship());

			if (relatedWebPage != null)
			{
				return GetApplicationPath(context, relatedWebPage);
			}

			var relatedWebFile = shortcut.GetRelatedEntity(context, "adx_webfile_shortcut".ToRelationship());

			if (relatedWebFile != null)
			{
				return GetApplicationPath(context, relatedWebFile);
			}

			var relatedEvent = shortcut.GetRelatedEntity(context, "adx_event_shortcut".ToRelationship());

			if (relatedEvent != null)
			{
				return GetApplicationPath(context, relatedEvent);
			}

			var relatedForum = shortcut.GetRelatedEntity(context, "adx_communityforum_shortcut".ToRelationship());

			if (relatedForum != null)
			{
				return GetApplicationPath(context, relatedForum);
			}

			var externalUrl = shortcut.GetAttributeValue<string>("adx_externalurl");

			if (!string.IsNullOrEmpty(externalUrl))
			{
				return ApplicationPath.FromExternalUrl(externalUrl);
			}
			
			return null;
		}

		private ApplicationPath GetIdeaForumApplicationPath(OrganizationServiceContext context, Entity ideaForum)
		{
			ideaForum.AssertEntityName("adx_ideaforum");

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var partialUrl = ideaForum.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(partialUrl))
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			// If multi-language is enabled, return URL using in approriate multi-language URL format.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string url = string.Empty;

			if (contextLanguageInfo.IsCrmMultiLanguageEnabled && ContextLanguageInfo.DisplayLanguageCodeInUrl)
			{
				url = string.Format("{0}{1}", httpContext.Request.Url.GetLeftPart(UriPartial.Authority), urlHelper.Action("Ideas", "Ideas", new { ideaForumPartialUrl = partialUrl, area = "Ideas" }));
				url = contextLanguageInfo.FormatUrlWithLanguage(false, contextLanguageInfo.ContextLanguage.Code, new Uri(url));
			}
			else
			{
				url = urlHelper.Action("Ideas", "Ideas", new { ideaForumPartialUrl = partialUrl, area = "Ideas" });
			}

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private ApplicationPath GetIdeaApplicationPath(OrganizationServiceContext context, Entity idea)
		{
			idea.AssertEntityName("adx_idea");

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var partialUrl = idea.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(partialUrl))
			{
				return null;
			}

			var ideaForumEntityReference = idea.GetAttributeValue<EntityReference>("adx_ideaforumid");

			if (ideaForumEntityReference == null)
			{
				return null;
			}

			var forum = context.CreateQuery("adx_ideaforum").FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_ideaforumid").Id == ideaForumEntityReference.Id);

			if (forum == null)
			{
				return null;
			}

			var forumPartialUrl = forum.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(forumPartialUrl))
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			// If multi-language is enabled, return URL using in approriate multi-language URL format.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string url = string.Empty;

			if (contextLanguageInfo.IsCrmMultiLanguageEnabled && ContextLanguageInfo.DisplayLanguageCodeInUrl)
			{
				url = string.Format("{0}{1}", httpContext.Request.Url.GetLeftPart(UriPartial.Authority), urlHelper.Action("Ideas", "Ideas", new { ideaForumPartialUrl = forumPartialUrl, ideaPartialUrl = partialUrl, area = "Ideas" }));
				url = contextLanguageInfo.FormatUrlWithLanguage(false, contextLanguageInfo.ContextLanguage.Code, new Uri(url));
			}
			else
			{
				url = urlHelper.Action("Ideas", "Ideas", new { ideaForumPartialUrl = forumPartialUrl, ideaPartialUrl = partialUrl, area = "Ideas" });
			}

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private ApplicationPath GetIssueApplicationPath(OrganizationServiceContext context, Entity issue)
		{
			issue.AssertEntityName("adx_issue");

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var partialUrl = issue.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(partialUrl))
			{
				return null;
			}

			var forum = issue.GetRelatedEntity(context, new Relationship("adx_issueforum_issue"));

			if (forum == null)
			{
				return null;
			}

			var forumPartialUrl = forum.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(forumPartialUrl))
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			var url = urlHelper.Action("Issues", "Issues", new { issueForumPartialUrl = forumPartialUrl, issuePartialUrl = partialUrl, area = "Issues" });

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private ApplicationPath GetIncidentApplicationPath(OrganizationServiceContext context, Entity incident)
		{
			incident.AssertEntityName("incident");

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			if (portalContext == null || portalContext.Website == null)
			{
				return null;
			}

			var website = context.CreateQuery("adx_website")
				.FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == portalContext.Website.Id);

			if (website == null)
			{
				return null;
			}

			var page = context.GetPageBySiteMarkerName(website, "Case");

			if (page == null)
			{
				return null;
			}

			var pagePath = GetApplicationPath(context, page);

			if (pagePath == null)
			{
				return null;
			}

			var incidentUrl = new UrlBuilder(pagePath.AbsolutePath);

			incidentUrl.QueryString.Set("caseid", incident.Id.ToString());

			return ApplicationPath.FromAbsolutePath(incidentUrl.PathWithQueryString);
		}

		private ApplicationPath GetKbArticleApplicationPath(OrganizationServiceContext context, Entity kbarticle)
		{
			kbarticle.AssertEntityName("kbarticle");

			var number = kbarticle.GetAttributeValue<string>("number");

			if (string.IsNullOrEmpty(number))
			{
				return null;
			}

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			var url = urlHelper.Action("Index", "Article", new { number = number, area = "KnowledgeBase" });

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private ApplicationPath GetKnowledgeArticleApplicationPath(OrganizationServiceContext context, Entity article)
		{
			article.AssertEntityName("knowledgearticle");

			var number = article.GetAttributeValue<string>("articlepublicnumber");

			if (string.IsNullOrEmpty(number))
			{
				return null;
			}

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			var languageLocaleCode = article.Contains("language_locale.code") ? article.GetAttributeValue<AliasedValue>("language_locale.code").Value as string : null;
			if (string.IsNullOrWhiteSpace(languageLocaleCode))
			{
				var localeid = article.GetAttributeValue<EntityReference>("languagelocaleid");
				if (localeid != null)
				{
					var locale = context.CreateQuery("languagelocale").FirstOrDefault(lang => lang.GetAttributeValue<Guid>("languagelocaleid") == localeid.Id);
					if (locale != null)
					{
						languageLocaleCode = locale.GetAttributeValue<string>("code");
					}
				}
			}

			// If multi-language is enabled, return URL using in approriate multi-language URL format.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();
			string url = string.Empty;
			if (contextLanguageInfo.IsCrmMultiLanguageEnabled && ContextLanguageInfo.DisplayLanguageCodeInUrl)
			{
				var actionUrl = urlHelper.Action("Article", "Article", new { number = number, area = "KnowledgeManagement" });

				// if actionUrl is null, ex: deactivated root page.
				if (actionUrl == null)
				{
					return null;
				}

				url = string.Format("{0}{1}", httpContext.Request.Url.GetLeftPart(UriPartial.Authority), actionUrl);
				url = contextLanguageInfo.FormatUrlWithLanguage(false, languageLocaleCode, new Uri(url));
			}
			else
			{
				url = urlHelper.Action("Article", "Article", new { number = number, lang = languageLocaleCode, area = "KnowledgeManagement" });
			}

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private ApplicationPath GetCategoryApplicationPath(OrganizationServiceContext context, Entity article)
		{
			article.AssertEntityName("category");

			var number = article.GetAttributeValue<string>("categorynumber");

			if (string.IsNullOrEmpty(number))
			{
				return null;
			}

			var httpContext = HttpContext.Current;

			if (httpContext == null)
			{
				return null;
			}

			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				return null;
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			var url = urlHelper.Action("Index", "Category", new { number = number, area = "Category" });

			return url == null ? null : ApplicationPath.FromAbsolutePath(url);
		}

		private static ApplicationPath GetApplicationPath(
			OrganizationServiceContext context,
			Entity entity,
			string partialUrlLogicalName,
			Relationship parentEntityRelationship,
			Func<OrganizationServiceContext, Entity, ApplicationPath> getParentApplicationPath,
			string siteMarker = null)
		{
			var parentEntity = entity.GetRelatedEntity(context, parentEntityRelationship);

			var partialUrlAttributeValue = entity.GetAttributeValue<object>(partialUrlLogicalName);
			var partialUrl = partialUrlAttributeValue == null ? null : partialUrlAttributeValue.ToString();

			if (parentEntity == null)
			{
				if (siteMarker == null)
				{
					return ApplicationPath.FromPartialPath(partialUrl);
				}
				
				var siteMarkerPage = context.GetPageBySiteMarkerName(context.GetWebsite(entity), siteMarker);

				if (siteMarkerPage == null)
				{
					return null;
				}

				var siteMarkerUrl = context.GetApplicationPath(siteMarkerPage);

				if (siteMarkerUrl == null)
				{
					return null;
				}

				return JoinApplicationPath(siteMarkerUrl.PartialPath, partialUrl);
			}

			var parentUrl = getParentApplicationPath(context, parentEntity);

			if (parentUrl == null)
			{
				return null;
			}

			var url = JoinApplicationPath(parentUrl.PartialPath, partialUrl);

			return url;
		}

		internal static ApplicationPath JoinApplicationPath(string basePath, string extendedPath)
		{
			if (basePath.Contains("?")
				|| basePath.Contains(":")
				|| basePath.Contains("//")
				|| basePath.Contains("&")
				|| basePath.Contains("%3f")
				|| basePath.Contains("%2f%2f")
				|| basePath.Contains("%26"))
			{
				throw new ApplicationException("Invalid base path");
			}

			if (extendedPath.Contains("?")
				|| extendedPath.Contains("&")
				|| extendedPath.Contains("//")
				|| extendedPath.Contains(":")
				|| extendedPath.Contains("%3f")
				|| extendedPath.Contains("%2f%2f")
				|| extendedPath.Contains("%26"))
			{
				throw new ApplicationException("Invalid extendedPath");
			}

			var path = "{0}/{1}".FormatWith(basePath.TrimEnd('/'), extendedPath.TrimStart('/'));

			return ApplicationPath.FromPartialPath(path);
		}
	}
}
