/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums.Security
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Cms.Security;
	using Adxstudio.Xrm.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;

	internal class ForumAccessPermissionProvider : ContentMapAccessProvider
	{
		private readonly WebPageAccessControlSecurityProvider _webPageAccessControlProvider = new WebPageAccessControlSecurityProvider(HttpContext.Current);

		public ForumAccessPermissionProvider(HttpContext context)
			: base(context)
		{
		}

		public ForumAccessPermissionProvider(IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
		}

		/// <summary>
		///  (Enumeration for adx_right)
		/// </summary>
		/// <remarks>
		/// The custom entity adx_communityforumaccesspermission has a picklist attribute adx_right
		/// and the options must match the following enumeration
		/// </remarks>
		public enum RightOption { GrantChange = 1, RestrictRead = 2 }

		protected override bool TryAssert(OrganizationServiceContext context, Entity forum, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			forum.AssertEntityName("adx_communityforum");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Testing right {0} on forum '{1}' ({2}).", right, forum.GetAttributeValue<string>("adx_name"), forum.Id));

			this.AddDependencies(dependencies, forum, new[] { "adx_webrole", "adx_communityforumaccesspermission" });

			if (!Roles.Enabled)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Roles are not enabled for this application. Allowing Read, but not Change.");

				// If roles are not enabled on the site, grant Read, deny Change.
				return (right == CrmEntityRight.Read);
			}

			var userRoles = this.GetUserRoles();

			// Get all rules applicable to the forum, grouping equivalent rules. (Rules that
			// target the same forum and confer the same right are equivalent.)
			var ruleGroupings = from rule in this.GetRulesApplicableToForum(forum, dependencies, map)
								let forumReference = rule.GetAttributeValue<EntityReference>("adx_forumid")
								let rightOption = rule.GetAttributeValue<OptionSetValue>("adx_right")
								where forumReference != null && rightOption != null
								group rule by new { ForumID = forumReference.Id, Right = ParseRightOption(rightOption.Value) } into ruleGrouping
								select ruleGrouping;

			var websiteReference = forum.GetAttributeValue<EntityReference>("adx_websiteid");

			// Order the rule groupings so that all GrantChange rules will be evaluated first.
			ruleGroupings = ruleGroupings.OrderByDescending(grouping => grouping.Key.Right, new RightOptionComparer());

			foreach (var ruleGrouping in ruleGroupings)
			{
				// Collect the names of all the roles that apply to this rule grouping
				var ruleGroupingRoles = ruleGrouping.SelectMany(rule => GetRolesForGrouping(map, rule, websiteReference));

				// Determine if the user belongs to any of the roles that apply to this rule grouping
				var userIsInAnyRoleForThisRule = ruleGroupingRoles.Any(role => userRoles.Any(userRole => userRole == role));

				// If the user belongs to one of the roles...
				if (userIsInAnyRoleForThisRule)
				{
					// ...and the rule is GrantChange...
					if (ruleGrouping.Key.Right == RightOption.GrantChange)
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User has right Change on forum ({0}). Permission granted.", ruleGrouping.Key.ForumID));

						// ...the user has all rights.
						return true;
					}
				}
				// If the user does not belong to any of the roles, the rule restricts read, and the desired right
				// is read...
				else if (ruleGrouping.Key.Right == RightOption.RestrictRead && right == CrmEntityRight.Read)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User does not have right Read due to read restriction on forum ({0}). Permission denied.", ruleGrouping.Key.ForumID));

					// ...the user has no right.
					return false;
				}
			}

			// If none of the above rules apply, assert on parent webpage.
			var parentWebPage = forum.GetAttributeValue<EntityReference>("adx_parentpageid");
			WebPageNode parentPageNode;
			map.TryGetValue(parentWebPage, out parentPageNode);

			// If there is no parent web page, grant Read by default, and deny Change.
			if (parentWebPage == null || parentPageNode == null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No access control rules apply to the current user and forum. Allowing Read, but not Change.");

				return (right == CrmEntityRight.Read);
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No access control rules apply to the current user and forum. Asserting right on parent web page.");

			return this._webPageAccessControlProvider.TryAssert(context, parentPageNode.ToEntity(), right, dependencies);
		}

		private static bool BelongsToWebsite(EntityReference websiteReference, WebRoleNode webRole)
		{
			webRole.ToEntity().AssertEntityName("adx_webrole");

			if (websiteReference == null)
			{
				return false;
			}

			var roleWebsiteReference = webRole.Website;

			if (roleWebsiteReference == null)
			{
				return false;
			}

			return websiteReference.Id == roleWebsiteReference.Id;
		}

		private static IEnumerable<string> GetRolesForGrouping(ContentMap map, Entity rule, EntityReference websiteReference)
		{
			IDictionary<EntityReference, EntityNode> intersectNodes;
			if (map.TryGetValue("adx_communityforumaccesspermission_webrole", out intersectNodes))
			{
				var ruleGroupingRoles =
					intersectNodes.Values.Cast<ForumAccessPermissionsToWebRoleNode>()
						.Where(i => i.ForumAccessPermission.Id == rule.Id && BelongsToWebsite(websiteReference, i.WebRole))
						.Select(r => r.WebRole.Name);

				return ruleGroupingRoles;
			}

			return Enumerable.Empty<string>();
		}

		protected virtual IEnumerable<Entity> GetRulesApplicableToForum(Entity forum, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			forum.AssertEntityName("adx_communityforum");

			var rules = new List<Entity>();

			ForumNode currentForumNode;
			if (map.TryGetValue(forum, out currentForumNode))
			{
				var rulesForCurrentForum = currentForumNode.ForumAccessPermissions.Where(fr => fr.StateCode == 0);
				rules.AddRange(rulesForCurrentForum.Select(r => r.ToEntity()));
			}

			dependencies.AddEntityDependencies(rules);

			return rules;
		}

		private static RightOption ParseRightOption(int rightValue)
		{
			return rightValue.ToEnum<RightOption>();
		}

		private class RightOptionComparer : IComparer<RightOption>
		{
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
	}
}
