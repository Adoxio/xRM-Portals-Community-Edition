/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Events.Security
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using Adxstudio.Xrm.Cms.Security;
	using Adxstudio.Xrm.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;

	internal class EventAccessPermissionProvider : CacheSupportingCrmEntitySecurityProvider
	{
		protected const int RestrictRead = 1;

		private readonly WebPageAccessControlSecurityProvider _webPageAccessControlProvider = new WebPageAccessControlSecurityProvider(HttpContext.Current);

		public override bool TryAssert(OrganizationServiceContext context, Entity currentEvent, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			currentEvent.AssertEntityName("adx_event");

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Testing right {0} on event ({1}).", right, currentEvent.Id));

			dependencies.AddEntityDependency(currentEvent);
			dependencies.AddEntitySetDependency("adx_webrole");
			dependencies.AddEntitySetDependency("adx_eventaccesspermission");

			if (!Roles.Enabled)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Roles are not enabled for this application. Allowing Read, but not Change.");

                // If roles are not enabled on the site, grant Read, deny Change.
                return (right == CrmEntityRight.Read);
			}

			// Windows Live ID Server decided to return null for an unauthenticated user's name
			// A null username, however, breaks the Roles.GetRolesForUser() because it expects an empty string.
			var currentUsername = (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
				? HttpContext.Current.User.Identity.Name ?? string.Empty
				: string.Empty;
			

			var userRoles = Roles.GetRolesForUser(currentUsername);

			// Get all rules applicable to the event, grouping equivalent rules. (Rules that
			// target the same event and confer the same right are equivalent.)
			var ruleGroupings = from rule in GetRulesApplicableToEvent(context, currentEvent, dependencies)
								let eventReference = rule.GetAttributeValue<EntityReference>("adx_eventid")
								let rightOption = rule.GetAttributeValue<OptionSetValue>("adx_right")
								where eventReference != null && rightOption != null
								group rule by new { EventID = eventReference.Id, Right = rightOption.Value } into ruleGrouping
								select ruleGrouping;

			var websiteReference = currentEvent.GetAttributeValue<EntityReference>("adx_websiteid");

			foreach (var ruleGrouping in ruleGroupings)
			{
				// Collect the names of all the roles that apply to this rule grouping
				var ruleGroupingRoles = ruleGrouping.SelectMany(rule => rule.GetRelatedEntities(context, "adx_eventaccesspermission_webrole").ToList()
					.Where(role => BelongsToWebsite(websiteReference, role))
					.Select(role => role.GetAttributeValue<string>("adx_name")));

				// Determine if the user belongs to any of the roles that apply to this rule grouping
				var userIsInAnyRoleForThisRule = ruleGroupingRoles.Any(role => userRoles.Any(userRole => userRole == role));

				// If the user belongs to one of the roles...
				if (userIsInAnyRoleForThisRule)
				{
					if (ruleGrouping.Key.Right != RestrictRead)
					{
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User has right Change on forum ({0}). Permission granted.", ruleGrouping.Key.EventID));

						// ...the user has all rights.
						return true;
					}
				}
				// If the user does not belong to any of the roles, the rule restricts read, and the desired right
				// is read...
				else if (ruleGrouping.Key.Right == RestrictRead && right == CrmEntityRight.Read)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("User does not have right Read due to read restriction on event ({0}). Permission denied.", ruleGrouping.Key.EventID));

					// ...the user has no right.
					return false;
				}
			}
			
			//Get all membership type applicable to the event
			var membershipTypes = currentEvent.GetRelatedEntities(context, "adx_event_membershiptype");

			//If the event has membership types, specific user has right to access it. If there is no membership type, every user has right.
			if (membershipTypes.Any())
			{
				var contact = PortalContext.Current.User;

				//Anonymouse user has no right.
				if (contact == null) return false;

				foreach (var membershipType in membershipTypes)
				{
					if (contact.GetRelatedEntities(context, "adx_membership_contact").Any(m => m.GetAttributeValue<EntityReference>("adx_membershiptypeid") == membershipType.ToEntityReference()))                   
					{
						return true;
					}
				}

				return false;
			}

			// If none of the above rules apply, assert on parent webpage.
			var parentWebPage = currentEvent.GetRelatedEntity(context, "adx_webpage_event");

			// If there is no parent web page, grant Read by default, and deny Change.
			if (parentWebPage == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No access control rules apply to the current user and event. Allowing Read, but not Change.");

				return (right == CrmEntityRight.Read);
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No access control rules apply to the current user and event. Asserting right on parent web page.");

			return _webPageAccessControlProvider.TryAssert(context, parentWebPage, right, dependencies);
		}

		private static bool BelongsToWebsite(EntityReference websiteReference, Entity webRole)
		{
			webRole.AssertEntityName("adx_webrole");

			if (websiteReference == null)
			{
				return false;
			}

			var roleWebsiteReference = webRole.GetAttributeValue<EntityReference>("adx_websiteid");

			if (roleWebsiteReference == null)
			{
				return false;
			}

			return websiteReference.Id == roleWebsiteReference.Id;
		}

		protected virtual IEnumerable<Entity> GetRulesApplicableToEvent(OrganizationServiceContext context, Entity crmEvent, CrmEntityCacheDependencyTrace dependencies)
		{
			crmEvent.AssertEntityName("adx_event");

			var rules = new List<Entity>();

			var currentEvent = crmEvent;

			if (currentEvent != null)
			{
				var rulesForCurrentEvent = currentEvent.GetRelatedEntities(context, "adx_event_eventaccesspermission");

				if (rulesForCurrentEvent != null)
				{
					rules.AddRange(rulesForCurrentEvent);
				}
			}

			dependencies.AddEntityDependencies(rules);

			return rules;
		}
	}
}
