/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Security
{
	using System.Web;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Security;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;

	/// <summary> The web page access control security provider. </summary>
	internal class WebPageAccessControlSecurityProvider : ContentMapAccessProvider
	{
		/// <summary>
		///  (Enumeration for adx_right)
		/// </summary>
		public enum RightOption
		{
			GrantChange = 1,
			RestrictRead = 2
		}

		/// <summary>
		/// Enumeration for adx_scoope
		/// </summary>
		public enum ScopeOption
		{
			AllContent = 1,
			ExcludeDirectChildWebFiles = 2
		}

		/// <summary> Initializes a new instance of the <see cref="WebPageAccessControlSecurityProvider"/> class. </summary>
		/// <param name="context"> The context. </param>
		public WebPageAccessControlSecurityProvider(HttpContext context)
			: this(context != null ? context.GetContentMapProvider() : AdxstudioCrmConfigurationManager.CreateContentMapProvider())
		{
		}

		/// <summary> Initializes a new instance of the <see cref="WebPageAccessControlSecurityProvider"/> class. </summary>
		/// <param name="contentMapProvider"> The content map provider. </param>
		public WebPageAccessControlSecurityProvider(IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
		}

		/// <summary> The try assert. </summary>
		/// <param name="context"> The context. </param>
		/// <param name="entity"> The entity. </param>
		/// <param name="right"> The right. </param>
		/// <param name="dependencies"> The dependencies. </param>
		/// <param name="map"> The map. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		protected override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			return this.TryAssert(context, entity, right, dependencies, map, false);
		}

		/// <summary> The try assert. </summary>
		/// <param name="context"> The context. </param>
		/// <param name="entity"> The entity. </param>
		/// <param name="right"> The right. </param>
		/// <param name="dependencies"> The dependencies. </param>
		/// <param name="map"> The map. </param>
		/// <param name="useScope">Pass true if you need to determine web file permissions throught parent web page</param>
		/// <returns> The <see cref="bool"/>. </returns>
		public virtual bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right,
			CrmEntityCacheDependencyTrace dependencies, ContentMap map, bool useScope)
		{
			entity.AssertEntityName("adx_webpage");
			this.AddDependencies(
				dependencies,
				entity,
				new[] { "adx_webrole", "adx_webrole_contact", "adx_webrole_account", "adx_webpageaccesscontrolrule" });

			WebPageNode page;

			if (!map.TryGetValue(entity, out page))
			{
				// the website context is missing data
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to lookup the web page '{0}' ({1}).", entity.GetAttributeValue<string>("adx_name"), entity.Id));

				return false;
			}

			dependencies.IsCacheable = false;

			return this.TryAssert(page, right, useScope);
		}

		/// <summary> The try assert. </summary>
		/// <param name="page"> The page. </param>
		/// <param name="right"> The right. </param>
		/// <param name="useScope">Pass true if you need to determine web file permissions throught parent web page</param>
		/// <returns> The <see cref="bool"/>. </returns>
		public virtual bool TryAssert(WebPageNode page, CrmEntityRight right, bool useScope)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Testing right {0} on web page '{1}' ({2}).", right, page.Name, page.Id));

			if (!Roles.Enabled)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Roles are not enabled for this application. Allowing Read, but not Change.");

				// If roles are not enabled on the site, grant Read, deny Change.
				return right == CrmEntityRight.Read;
			}

			// Ignore access control rules for service pages like not found or access denied
			if (right == CrmEntityRight.Read && IsServicePage(page))
			{
				return true;
			}

			// If the chain of pages from the current page up to the root page contains an inactive page, deny the assertion
			if (IsInactivePath(page))
			{
				return false;
			}

			var userRoles = this.GetUserRoles();

			// when we use rule scope we're checking permissions for parent page of web file
			//	and we need to check permissions only for one level
			var useInheritance = !useScope || right == CrmEntityRight.Change;

			// Get all rules applicable to the page and its parent path, grouping equivalent rules. (Rules that
			// target the same web page and confer the same right are equivalent.)
			var ruleGroupings =
				from rule in this.GetRulesApplicableToWebPage(page, useInheritance)
				where rule.WebPage != null && rule.Right != null
				group rule by new { WebPageId = rule.WebPage.Id, Right = ParseRightOption(rule.Right.Value) } into ruleGrouping
				select ruleGrouping;

			// Order the rule groupings so that all GrantChange rules will be evaluated first.
			ruleGroupings = ruleGroupings.OrderByDescending(grouping => grouping.Key.Right, new RightOptionComparer());

			foreach (var ruleGrouping in ruleGroupings)
			{
				// Collect the names of all the roles that apply to this rule grouping
				var ruleGroupingRoles = ruleGrouping.SelectMany(
					rule => rule.WebRoles.Where(role => BelongsToWebsite(page.Website, role))
					.Select(role => role.Name));

				// Determine if the user belongs to any of the roles that apply to this rule grouping
				var userIsInAnyRoleForThisRule = ruleGroupingRoles.Any(role => userRoles.Any(userRole => userRole == role));

				// If the user belongs to one of the roles...
				if (userIsInAnyRoleForThisRule)
				{
					// ...and the rule is GrantChange...
					if (ruleGrouping.Key.Right == RightOption.GrantChange)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User has right Change on web page ({0}). Permission granted.", ruleGrouping.Key.WebPageId));

						// ...the user has all rights.
						return true;
					}
				}

				// If the user does not belong to any of the roles, the rule restricts read, and the desired right
				// is read...
				else if (ruleGrouping.Key.Right == RightOption.RestrictRead && right == CrmEntityRight.Read)
				{
					if (useScope && ruleGrouping.Any(rule => rule.Scope.HasValue && (ScopeOption)rule.Scope.Value == ScopeOption.ExcludeDirectChildWebFiles))
					{
						// Ignore read restriction for web files where scope is ExcludeDirectChildWebFiles
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Ignoring web page ({0}) read restriction due to ExcludeDirectChildWebFiles", ruleGrouping.Key.WebPageId));
					}
					else
					{
						if (page.Parent == null && page.PartialUrl == "/")
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("\"Restrict Read\" right on web page({0}) ({1}).", ruleGrouping.Key.WebPageId, page.Name));
						}
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User does not have right Read due to read restriction on web page ({0}). Permission denied.", ruleGrouping.Key.WebPageId));

						// ...the user has no right.
						return false;
					}
				}
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No access control rules apply to the current user and page. Allowing Read, but not Change.");

			// If none of the above rules apply, grant Read by default, and deny Change by default.
			return right == CrmEntityRight.Read;
		}

		/// <summary> The belongs to website. </summary>
		/// <param name="website"> The website. </param>
		/// <param name="webRole"> The web role. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		private static bool BelongsToWebsite(WebsiteNode website, WebRoleNode webRole)
		{
			if (website == null || webRole == null || webRole.Website == null)
			{
				return false;
			}

			return website.Id == webRole.Website.Id;
		}

		/// <summary> The get rules applicable to web page. </summary>
		/// <param name="webPage"> The web page. </param>
		/// <param name="useInheritance">true to aggregate rules from parent pages</param>
		/// <returns> The rules. </returns>
		protected virtual IEnumerable<WebPageAccessControlRuleNode> GetRulesApplicableToWebPage(WebPageNode webPage, bool useInheritance)
		{
			var rules = GetRulesForPage(webPage, useInheritance);

			var stateSpecificRules = new List<WebPageAccessControlRuleNode>();
			var statelessRules = new List<WebPageAccessControlRuleNode>();
			var restrictReadRules = new List<WebPageAccessControlRuleNode>();

			foreach (var rule in rules)
			{
				if (rule.Right == (int)RightOption.GrantChange)
				{
					var webPagePublishingState = webPage.PublishingState != null ? webPage.PublishingState.Name : null;

					if (rule.PublishingStates.Any(ps => ps.Name == webPagePublishingState))
					{
						stateSpecificRules.Add(rule);
					}
					else
					{
						statelessRules.Add(rule);
					}
				}
				else
				{
					restrictReadRules.Add(rule);
				}
			}

			// if any state specific rules exist, ignore the stateless rules

			if (stateSpecificRules.Any())
			{
				foreach (var rule in stateSpecificRules)
				{
					yield return rule;
				}
			}
			else
			{
				foreach (var rule in statelessRules)
				{
					yield return rule;
				}
			}

			foreach (var rule in restrictReadRules)
			{
				yield return rule;
			}
		}

		/// <summary> The get rules for page. </summary>
		/// <param name="webPage"> The web page. </param>
		/// <param name="useInheritance">true to aggregate rules from parent pages</param>
		/// <returns> The rules. </returns>
		private static IEnumerable<WebPageAccessControlRuleNode> GetRulesForPage(WebPageNode webPage, bool useInheritance)
		{
			if (useInheritance && webPage.Parent != null)
			{
				foreach (var rule in GetRulesForPage(webPage.Parent, true))
				{
					yield return rule;
				}
			}

			// Guard against null reference exception. This might happen if WebPage is a translated content, but for whatever reason has no root web page.
			if (webPage.WebPageAccessControlRules != null)
			{
				foreach (var rule in webPage.WebPageAccessControlRules)
				{
					yield return rule;
				}
			}
		}

		/// <summary> The parse right option. </summary>
		/// <param name="rightValue"> The right value. </param>
		/// <returns> The <see cref="RightOption"/>. </returns>
		private static RightOption ParseRightOption(int rightValue)
		{
			return rightValue.ToEnum<RightOption>();
		}

		/// <summary> The right option comparer. </summary>
		private class RightOptionComparer : IComparer<RightOption>
		{
			/// <summary> The compare. </summary>
			/// <param name="x"> The first value to compare. </param>
			/// <param name="y"> The second value to compare. </param>
			/// <returns> The compare result>. </returns>
			public int Compare(RightOption x, RightOption y)
			{
				if (x == y)
				{
					return 0;
				}

				if (x == RightOption.GrantChange)
				{
					return 1;
				}

				if (y == RightOption.GrantChange)
				{
					return -1;
				}

				return 0;
			}
		}

		/// <summary> Determines whether page is service and should be ignored by access control rules. </summary>
		/// <param name="page"> The page. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		private static bool IsServicePage(WebPageNode page)
		{
			if (page?.Website == null)
			{
				return false;
			}

			var servicePages = page.Website.SiteMarkers
				.Where(marker => marker.Name == ContentMapCrmSiteMapProvider.AccessDeniedPageSiteMarkerName
							  || marker.Name == ContentMapCrmSiteMapProvider.NotFoundPageSiteMarkerName)
				.Select(marker => marker.WebPage);

			var isRoot = page.IsRoot.GetValueOrDefault(); // MLP

			return servicePages.Any(servicePage => isRoot || page.RootWebPage == null ? page.Id == servicePage.Id : page.RootWebPage.Id == servicePage.Id);
		}

		/// <summary> The is inactive path. </summary>
		/// <param name="page"> The page. </param>
		/// <returns> The <see cref="bool"/>. </returns>
		private static bool IsInactivePath(WebPageNode page)
		{
			if (page == null)
			{
				return false;
			}
			if (page.IsCircularReference == true)
			{
				return true;
			}

			return page.IsReference || IsInactivePath(page.Parent);
		}
	}
}
