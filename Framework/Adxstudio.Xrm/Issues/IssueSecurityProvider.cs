/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Issues
{
	internal class IssueSecurityProvider : CacheSupportingCrmEntitySecurityProvider
	{
		public IssueSecurityProvider(string portalName = null)
		{
			PortalName = portalName;
		}

		protected string PortalName { get; private set; }

		public override bool TryAssert(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			entity.ThrowOnNull("entity");

			if (entity.LogicalName == "adx_issueforum")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_issueforum ({1}).", right, entity.Id));

				dependencies.AddEntityDependency(entity);
				
				dependencies.AddEntitySetDependency("adx_webrole");

				return right == CrmEntityRight.Change
					? UserInRole("adx_webrole_issueforum_write", false, serviceContext, entity, dependencies)
					: UserInRole("adx_webrole_issueforum_read", true, serviceContext, entity, dependencies)
						|| UserInRole("adx_webrole_issueforum_write", false, serviceContext, entity, dependencies);
			}

			if (entity.LogicalName == "adx_issue")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_issue ({1}).", right, entity.Id));

				dependencies.AddEntityDependency(entity);

				var issueForum = entity.GetRelatedEntity(serviceContext, new Relationship("adx_issueforum_issue"));

				if (issueForum == null)
				{
					return false;
				}

				var approved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

				// If the right being asserted is Read, and the issue is approved, assert whether the issue forum is readable.
				if (right == CrmEntityRight.Read && approved)
				{
					return TryAssert(serviceContext, issueForum, right, dependencies);
				}

				return TryAssert(serviceContext, issueForum, CrmEntityRight.Change, dependencies);
			}

			if (entity.LogicalName == "adx_issuecomment")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_issuecomment ({1}).", right, entity.Id));

				dependencies.AddEntityDependency(entity);

				var issue = entity.GetRelatedEntity(serviceContext, new Relationship("adx_issue_issuecomment"));

				if (issue == null)
				{
					return false;
				}

				var approved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

				// If the right being asserted is Read, and the comment is approved, assert whether the issue is readable.
				if (right == CrmEntityRight.Read && approved)
				{
					return TryAssert(serviceContext, issue, right, dependencies);
				}

				return TryAssert(serviceContext, issue, CrmEntityRight.Change, dependencies);
			}

			if (entity.LogicalName == "adx_issuevote")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on adx_issuevote ({1}).", entity.Id));

				dependencies.AddEntityDependency(entity);

				var issue = entity.GetRelatedEntity(serviceContext, new Relationship("adx_issue_issuevote"));

				return issue != null && TryAssert(serviceContext, issue, right, dependencies);
			}

			throw new NotSupportedException("Entities of type {0} are not supported by this provider.".FormatWith(entity.LogicalName));
		}

		private bool UserInRole(string roleRelationship, bool defaultIfNoRoles, OrganizationServiceContext serviceContext, Entity entity, CrmEntityCacheDependencyTrace dependencies)
		{
			if (!Roles.Enabled)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Roles are not enabled for this application. Returning {0}.", defaultIfNoRoles));

                return defaultIfNoRoles;
			}

			var website = entity.GetAttributeValue<EntityReference>("adx_websiteid");

			if (website == null)
			{
				return false;
			}

			var roles = entity.GetRelatedEntities(serviceContext, new Relationship(roleRelationship))
				.Where(e => Equals(e.GetAttributeValue<EntityReference>("adx_websiteid"), website))
				.ToArray();

			if (!roles.Any())
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Read is not restricted to any particular roles. Returning {0}.", defaultIfNoRoles));

                return defaultIfNoRoles;
			}

			dependencies.AddEntityDependencies(roles);

			// Windows Live ID Server decided to return null for an unauthenticated user's name
			// A null username, however, breaks the Roles.GetRolesForUser() because it expects an empty string.
			var currentUsername = HttpContext.Current.User != null && HttpContext.Current.User.Identity != null
				? HttpContext.Current.User.Identity.Name ?? string.Empty
				: string.Empty;

			var userRoles = Roles.GetRolesForUser(currentUsername);

			return roles.Select(e => e.GetAttributeValue<string>("adx_name")).Intersect(userRoles, StringComparer.InvariantCulture).Any();
		}
	}
}
