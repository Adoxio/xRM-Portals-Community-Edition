/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Cms.Security
{
	internal class PublishingStateTransitionSecurityProvider : IPublishingStateTransitionSecurityProvider
	{

		public void Assert(OrganizationServiceContext context, Entity website, Entity fromState, Entity toState)
		{
			if (!TryAssert(context, website, fromState, toState))
			{
				throw new SecurityException(string.Format("Security assertion for transition from state {0} to {1} failed.",
					fromState.GetAttributeValue<string>("adx_name"), toState.GetAttributeValue<string>("adx_name")));
			}
		}

		public void Assert(OrganizationServiceContext context, Entity website, Guid fromStateId, Guid toStateId)
		{
			if (!TryAssert(context, website, fromStateId, toStateId))
			{
				throw new SecurityException(string.Format("Security assertion for transition from state {0} to {1} failed.",
					fromStateId, toStateId));
			}
		}

		public bool TryAssert(OrganizationServiceContext context, Entity website, Entity fromState, Entity toState)
		{
			var fromStateId = fromState.GetAttributeValue<Guid?>("adx_publishingstateid") ?? Guid.Empty;
			var toStateId = toState.GetAttributeValue<Guid?>("adx_publishingstateid") ?? Guid.Empty;

			return TryAssert(context, website, fromStateId, toStateId);
		}

		public bool TryAssert(OrganizationServiceContext context, Entity website, Guid fromStateId, Guid toStateId)
		{
			// Windows Live ID Server decided to return null for an unauthenticated user's name
			// A null username, however, breaks the Roles.GetRolesForUser() because it expects an empty string.
			var currentUsername = (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
				? HttpContext.Current.User.Identity.Name ?? string.Empty
				: string.Empty;

			var userRoles = Roles.GetRolesForUser(currentUsername).ToLookup(role => role);

			//Get publshing state transitional rules 
			var publishingStateRulesApplicable = context.CreateQuery("adx_publishingstatetransitionrule").Where(psr =>
				psr.GetAttributeValue<EntityReference>("adx_fromstate_publishingstateid") == new EntityReference("adx_publishingstate", fromStateId) &&
				psr.GetAttributeValue<EntityReference>("adx_tostate_publishingstateid") == new EntityReference("adx_publishingstate", toStateId) &&
				psr.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()).ToList();

			var webRoles = publishingStateRulesApplicable.SelectMany(rule => rule.GetRelatedEntities(context, "adx_publishingstatetransitionrule_webrole")
				.Where(role => role.GetAttributeValue<EntityReference>("adx_websiteid") != null && role.GetAttributeValue<EntityReference>("adx_websiteid").Id == website.Id))
					.ToList().Select(role => role.GetAttributeValue<string>("adx_name"));

			// Determine if the user belongs to any of the roles that apply to this rule grouping
			// If the user belongs to one of the roles... or if there are no rules applicable
			return (!publishingStateRulesApplicable.Any()) || webRoles.Any(role => userRoles.Contains(role));
		}

	}
}
