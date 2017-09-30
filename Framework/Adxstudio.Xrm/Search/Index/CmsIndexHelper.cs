/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CmsIndexHelper.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Index
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Forums.Security;
	using Adxstudio.Xrm.Web.Providers;

	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Helps get the cms information for specific entities
	/// </summary>
	public static class CmsIndexHelper
	{
		/// <summary>
		/// The allow access default value which is 'F1158253-71CB-4063-BBC5-B3CFE27CA3EB'.
		/// </summary>
		private static readonly string AllowAccessDefaultValue = "F1158253-71CB-4063-BBC5-B3CFE27CA3EB";

		#region Forums

		/// <summary>
		/// Gets the given forums web roles.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="forumid">
		/// The forumid.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of web roles associated to the forums.
		/// </returns>
		public static IEnumerable<string> GetForumsWebRoles(IContentMapProvider contentMapProvider, Guid forumid)
		{
			return contentMapProvider.Using(contentMap => SelectAllForumsWebRoles(forumid, contentMap));
		}

		/// <summary>
		/// Select all the forums web roles.
		/// </summary>
		/// <param name="entityId">
		/// The entity id.
		/// </param>
		/// <param name="contentMap">
		/// The content map.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of web roles associated to the forum.
		/// </returns>
		private static IEnumerable<string> SelectAllForumsWebRoles(Guid entityId, ContentMap contentMap)
		{
			EntityNode entity;

			// Get the Forums from the content map
			if (!contentMap.TryGetValue(new EntityReference("adx_communityforum", entityId), out entity))
			{
				return Enumerable.Empty<string>();
			}

			var forum = entity as ForumNode;
			if (forum == null)
			{
				return Enumerable.Empty<string>();
			}

			var changeRules =
				forum.ForumAccessPermissions.Where(fa => fa.Right == ForumAccessPermissionNode.RightOption.GrantChange)
					.SelectMany(fa => fa.WebRoles.Select(wr => wr.Name));

			var readRules =
				forum.ForumAccessPermissions.Where(fa => fa.Right == ForumAccessPermissionNode.RightOption.RestrictRead)
					.SelectMany(fa => fa.WebRoles.Select(wr => wr.Name)).ToList();

			bool anyInheritedReadRestrictRules = false;

			// If it has a parent page we will need to inspect to see if they have different read rules.
			if (forum.ParentPage != null)
			{
				var parentPageWebRoles = GetRulesForPage(forum.ParentPage).Distinct().ToList();
				anyInheritedReadRestrictRules =
				parentPageWebRoles.Any(
					rule =>
					{
						if (rule.Right == null)
						{
							return false;
						}
						return rule.Right.Value.ToEnum<ForumAccessPermissionProvider.RightOption>()
						   == ForumAccessPermissionProvider.RightOption.RestrictRead;
					});

				// If Both the webpage tree do not have read restrict rules then give access to all.  
				var parentPageWebRoleNames = anyInheritedReadRestrictRules || readRules.Any()
						? parentPageWebRoles.SelectMany(
							webPageAccessControlRuleNode => webPageAccessControlRuleNode.WebRoles,
								(webPageAccessControlRuleNode, webRole) => webRole.Name).Distinct()
						: new[] { AllowAccessDefaultValue };

				// If there are no read restrict rules then we just follow the parents roles and change roles
				if (!readRules.Any() && !anyInheritedReadRestrictRules)
				{
					return changeRules.Concat(parentPageWebRoleNames).Distinct();
				}

				readRules = parentPageWebRoleNames.Union(readRules).ToList();
			}

			// Since it didn't have a parent page make sure there isn't a read restrict rule if no then give access to all. 
			return readRules.Any() || anyInheritedReadRestrictRules ? changeRules.Concat(readRules).Distinct() : new[] { AllowAccessDefaultValue };
		}

		/// <summary>
		/// Checks if the forum url is defined.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="entityId">
		/// The entity id.
		/// </param>
		/// <returns>
		/// <see cref="bool"/>If the forum has a URL defined or not.
		/// </returns>
		public static bool IsForumUrlDefined(IContentMapProvider contentMapProvider, Guid entityId)
		{
			return contentMapProvider.Using(
				contentMap =>
					{
						EntityNode entity;
						if (!contentMap.TryGetValue(new EntityReference("adx_communityforum", entityId), out entity))
						{
							return false;
						}

						var forum = entity as ForumNode;

						if (forum == null)
						{
							return false;
						}
						
						var partialUrl = forum.PartialUrl;

						return IsWebPageUrlDefined(forum.ParentPage, partialUrl);
					});
		}

		#endregion

		#region Ideas

		/// <summary>
		/// Gets idea forum web roles.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="ideaForumId">
		/// The idea forum id.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of web roles associated to the Idea Forum.
		/// </returns>
		public static IEnumerable<string> GetIdeaForumWebRoles(IContentMapProvider contentMapProvider, Guid ideaForumId)
		{
			return contentMapProvider.Using(contentMap => SelectAllIdeaForumWebRoles(ideaForumId, contentMap));
		}

		/// <summary>
		/// Select all idea forum web roles.
		/// </summary>
		/// <param name="entityId">
		/// The entity id.
		/// </param>
		/// <param name="contentMap">
		/// The content map.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of web roles associated to the Idea Forum.
		/// </returns>
		private static IEnumerable<string> SelectAllIdeaForumWebRoles(Guid entityId, ContentMap contentMap)
		{
			EntityNode entity;

			// Get the idea forum from the content map
			if (!contentMap.TryGetValue(new EntityReference("adx_ideaforum", entityId), out entity))
			{
				return Enumerable.Empty<string>();
			}

			var idea = entity as IdeaForumNode;

			if (idea == null)
			{
				return Enumerable.Empty<string>();
			}

			// In IdeaSecurityProvider.cs the idea is readable if the user is in the given roles or if there is no roles 
			// given then all can read. Thus if WebRolesRead has none in there we need to add all the roles. 
			if (idea.WebRolesRead.Any())
			{
				// But if there are some Read rules then add them as well as the write rules as if they have any of them then they
				// Should be allowed to read it.
				return idea.WebRolesRead.Select(w => w.Name).Concat(idea.WebRolesWrite.Select(w => w.Name));
			}

			// If it doesn't have any specific read roles then add all zero guid as a sign that everyone should have access.
			return new[] { AllowAccessDefaultValue };
		}

		#endregion

		#region Site Markers

		/// <summary>
		/// Checks if the site maker has a url defined.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="siteMakerName">
		/// The site maker name.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/> if the site marker has a url defined.
		/// </returns>
		public static bool IsSiteMakerUrlDefined(IContentMapProvider contentMapProvider, string siteMakerName)
		{

			if (string.IsNullOrEmpty(siteMakerName))
			{
				return false;
			}

			return contentMapProvider.Using(
				contentMap =>
					{
						IDictionary<EntityReference, EntityNode> lookup;
						if (!contentMap.TryGetValue("adx_sitemarker", out lookup))
						{
							return false;
						}

						var siteMarker = lookup.Values.Cast<SiteMarkerNode>().FirstOrDefault(x => x.Name == siteMakerName);
						
						if (siteMarker == null)
						{
							return false;
						}

						var webpage = siteMarker.WebPage;
						if (webpage == null)
						{
							return false;
						}

						return IsWebPageUrlDefined(webpage);
					});

		}

		#endregion

		#region WebPages

		/// <summary>
		/// Gets the given web pages web roles.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="entityId">
		/// The webpage Id.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of the associated webroles name for the given Webpage.
		/// </returns>
		public static IEnumerable<string> GetWebPageWebRoles(IContentMapProvider contentMapProvider, Guid entityId)
		{
			return contentMapProvider.Using(contentMap => SelectAllWebRolesForWebpage(entityId, contentMap));
		}

		/// <summary>
		/// Gets guids of descendant localized web pages for the given web page.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="entityId">
		/// The webpage Id.
		/// </param>
		/// <param name="lcid">
		/// The lcid for the page if it's a content page.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of the web page GUIDs for descendant web pages.
		/// </returns>
		public static IEnumerable<Guid> GetDescendantLocalizedWebpagesForWebpage(IContentMapProvider contentMapProvider, Guid entityId, int? lcid = null)
		{
			var predicates = new List<Predicate<WebPageNode>>() { IsContentPage };

			if (lcid.HasValue)
			{
				Predicate<WebPageNode> isLocalized = new Predicate<WebPageNode>((WebPageNode webPageNode) =>
				{
					return webPageNode.WebPageLanguage.PortalLanguage.Lcid == lcid.Value;
				});
				predicates.Add(isLocalized);
			}

			return contentMapProvider.Using(contentMap => SelectAllDescendantWebpagesWithPredicates(entityId, contentMap, predicates));
		}

		/// <summary>
		/// Gets guids of root localized web pages for the given web page.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="entityId">
		/// The webpage Id.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of the web page GUIDs for descendant web pages.
		/// </returns>
		public static IEnumerable<Guid> GetDescendantRootWebpagesForWebpage(IContentMapProvider contentMapProvider, Guid entityId)
		{
			return contentMapProvider.Using(contentMap => SelectAllDescendantWebpagesWithPredicates(entityId, contentMap, new List<Predicate<WebPageNode>>() { IsRootWebPage }));
		}

        /// <summary>
        /// Selects all guids of web roles for the given entity.
        /// </summary>
        /// <param name="entityId">
        /// The webpage Id.
        /// </param>
        /// <param name="contentMap">
        /// The content map.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>. of the WebRole name associated to the webpage.
        /// </returns>
        private static IEnumerable<string> SelectAllWebRolesForWebpage(Guid entityId, ContentMap contentMap)
		{
			EntityNode entity;
			if (!contentMap.TryGetValue(new EntityReference("adx_webpage", entityId), out entity))
			{
				return Enumerable.Empty<string>();
			}

			var webpage = entity as WebPageNode;
			if (webpage == null)
			{
				return Enumerable.Empty<string>();
			}
			var webAccessRules = GetRulesForPage(webpage).ToList();

			// If the rule doesn't have a right associated to it then allow access 
			var anyReadRestrictRules =
				webAccessRules.Any(
					rule =>
						{
							if (rule.Right == null)
							{
								return false;
							}
							return rule.Right.Value.ToEnum<ForumAccessPermissionProvider.RightOption>()
							   == ForumAccessPermissionProvider.RightOption.RestrictRead;
						});

			// If there is not read restrict rules specified then allow READ access. 
			return anyReadRestrictRules || (webpage.PublishingState.IsVisible == null || webpage.PublishingState.IsVisible.Value == false) 
					? webAccessRules.SelectMany(webPageAccessControlRuleNode => webPageAccessControlRuleNode.WebRoles,
						(webPageAccessControlRuleNode, webRole) => webRole.Name).Distinct() 
					: new[] { AllowAccessDefaultValue };
		}

		/// <summary>
		/// Get the rules for the page.
		/// </summary>
		/// <param name="webPage">
		/// The web page.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of <see cref="WebPageAccessControlRuleNode"/>.
		/// </returns>
		private static IEnumerable<WebPageAccessControlRuleNode> GetRulesForPage(WebPageNode webPage)
		{
			if (webPage.Parent != null)
			{
				foreach (var rule in GetRulesForPage(webPage.Parent))
				{
					yield return rule;
				}
			}

			if (webPage.IsReference)
			{
				yield break;
			}

			foreach (var rule in webPage.WebPageAccessControlRules)
			{
				if (rule.PublishingStates.Any())
				{
					if (rule.PublishingStates.Any(publishingState => publishingState.Name == webPage.PublishingState.Name))
					{
						yield return rule;
					}
				}
				else
				{
					yield return rule;
				}
			}
		}

		/// <summary>
		/// Checks if the web page url defined.
		/// </summary>
		/// <param name="contentMapProvider">
		/// The content map provider.
		/// </param>
		/// <param name="entityId">
		/// The entity id.
		/// </param>
		/// <param name="additionalPartialUrl">
		/// OPTIONAL: additional Partial Url to be added to the webpages url.
		/// </param>
		/// <returns>
		/// If the webpage has a URL defined or not.
		/// </returns>
		public static bool IsWebPageUrlDefined(IContentMapProvider contentMapProvider, Guid entityId, string additionalPartialUrl = null)
		{
			return contentMapProvider.Using(
				delegate(ContentMap contentMap)
					{
						EntityNode entity;
						if (!contentMap.TryGetValue(new EntityReference("adx_webpage", entityId), out entity))
						{
							ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, string.Format("Web Page url is not defined. EntityId: {0}", entityId));
							return false;
						}

						var webPage = entity as WebPageNode;

						return IsWebPageUrlDefined(webPage, additionalPartialUrl);
					});
		}

		/// <summary>
		/// Is the web page url defined.
		/// </summary>
		/// <param name="webPage">
		/// The web page.
		/// </param>
		/// <param name="additionalPartialUrl">
		/// Optional additional Partial Url to be added to the webpage url.
		/// </param>
		/// <returns>
		/// If the webpage has a URL defined or not.
		/// </returns>
		private static bool IsWebPageUrlDefined(WebPageNode webPage, string additionalPartialUrl = null)
		{
			if (webPage == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, "Web Page url is not defined. Web Page is null");
				return false;
			}
			try
			{
				var applicationPath = GetApplicationPath(webPage, additionalPartialUrl);
				var newPath = ApplicationPath.FromPartialPath(WebsitePathUtility.ToAbsolute(new Entity("adx_website", webPage.Website.Id), applicationPath.PartialPath));
				return newPath != null;
			}
			catch (Exception e)
			{
				// if the application path wasn't a real path then it will throw an exception so just return false
				ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, string.Format("IsWebPageUrlDefined caught exception, returning false - {0}", e));
				return false;
			}
		}

		/// <summary>
		/// Gets the application path for the webpage.
		/// </summary>
		/// <param name="webPage">
		/// The web page.
		/// </param>
		/// <param name="additionalPartialUrl">
		/// Optional additional Partial Url to be added to the webpage url.
		/// </param>
		/// <returns>
		/// The <see cref="ApplicationPath"/>.
		/// </returns>
		private static ApplicationPath GetApplicationPath(WebPageNode webPage, string additionalPartialUrl = null)
		{
			var partialUrl = webPage.PartialUrl;
			if (!string.IsNullOrEmpty(additionalPartialUrl))
			{
				partialUrl = string.Format("{0}/{1}", partialUrl, additionalPartialUrl);
			}
			if (webPage.Parent != null)
			{
				return AdxEntityUrlProvider.JoinApplicationPath(GetApplicationPath(webPage.Parent).PartialPath, partialUrl);
			}
			return ApplicationPath.FromPartialPath(partialUrl);
		}

		/// <summary>
		/// Selects all guids of descendant web pages for the given web page.
		/// </summary>
		/// <param name="entityId">
		/// The webpage Id.
		/// </param>
		/// <param name="contentMap">
		/// The content map.
		/// </param>
		/// <param name="predicates">
		/// <see cref="IEnumerable"/> of predicates to determine whether a web page should be included in the results. Executed in order with short circuiting.
		/// </param>
		/// <returns>
		/// The <see cref="IEnumerable"/> of the web page GUIDs for descendant web pages.
		/// </returns>
		private static IEnumerable<Guid> SelectAllDescendantWebpagesWithPredicates(Guid entityId, ContentMap contentMap, IEnumerable<Predicate<WebPageNode>> predicates)
		{
			EntityNode entity;
			if (!contentMap.TryGetValue(new EntityReference("adx_webpage", entityId), out entity))
			{
				return Enumerable.Empty<Guid>();
			}

			var rootWebpage = entity as WebPageNode;
			if (rootWebpage == null)
			{
				return Enumerable.Empty<Guid>();
			}

			// if it's a content page, we want to start at it's root page so we can navigate down the web page hierarchy
			if (rootWebpage.IsRoot == false)
			{
				if (rootWebpage.RootWebPage == null)
				{
					// just return this web page, can't reach any others
					return new List<Guid>() { rootWebpage.Id };
				}

				rootWebpage = rootWebpage.RootWebPage;
			}

			var unprocessedNodes = new Queue<WebPageNode>();
			var webPageGuids = new List<Guid>();
			
			unprocessedNodes.Enqueue(rootWebpage);
			while (unprocessedNodes.Count > 0)
			{
				WebPageNode currWebPage = unprocessedNodes.Dequeue();

				foreach (var childWebPage in currWebPage.WebPages)
				{
					unprocessedNodes.Enqueue(childWebPage);
				}

				if (currWebPage.LanguageContentPages != null)
				{
					foreach (var contentPage in currWebPage.LanguageContentPages)
					{
						unprocessedNodes.Enqueue(contentPage);
					}
				}

				if (predicates.All(predicate => predicate(currWebPage)))
				{
					webPageGuids.Add(currWebPage.Id);
				}
			}

			return webPageGuids;
		}

		/// <summary>
		/// Checks if a web page is localized.
		/// </summary>
		/// <param name="webPageNode">The web page node.</param>
		/// <returns>Whether the web page is localized.</returns>
        private static bool IsContentPage(WebPageNode webPageNode)
        {
			// if IsRoot == null, MLP is disabled and root pages are the same as content pages
			return webPageNode.IsRoot == false || webPageNode.IsRoot == null;
        }

		/// <summary>
		/// Checks if the web page is a root page.
		/// </summary>
		/// <param name="webPageNode">The web page node.</param>
		/// <returns>Whether the web page is a root web page.</returns>
		private static bool IsRootWebPage(WebPageNode webPageNode)
		{
			// if IsRoot == null, MLP is disabled and root pages are the same as content pages
			return webPageNode.IsRoot == true || webPageNode.IsRoot == null;
		}

        #endregion

    }
}
