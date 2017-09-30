/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Security
{
	/// <summary>
	/// A <see cref="CrmContactRoleProvider"/> that adds the additional capability for contact users to inherit
	/// role membership through any adx_webroles associated with their parent customer account.
	/// </summary>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class CrmContactAccountRoleProvider : CrmContactRoleProvider
	{
		private string _attributeMapParentId;
		private string _attributeMapRoleId;
		private string _attributeMapRoleName;
		private string _attributeMapRoleWebsiteId;
		private string _attributeMapUsername;
		private string _attributeMapUserParentId;
		private string _attributeMapStateCode;
		private string _parentEntityName;
		private string _roleEntityName;
		private string _roleToParentRelationshipName;
		private string _roleToParentRelationshipEntityName;
		private string _userEntityName;

		public override void Initialize(string name, NameValueCollection config)
		{
			_attributeMapParentId = config["attributeMapParentId"] ?? "accountid";
			_attributeMapRoleId = config["attributeMapRoleId"] ?? "adx_webroleid";
			_attributeMapUserParentId = config["attributeMapUserParentId"] ?? "parentcustomerid";
			_parentEntityName = config["parentEntityName"] ?? "account";
			_roleToParentRelationshipName = config["roleToParentRelationshipName"] ?? "adx_webrole_account";
			_roleToParentRelationshipEntityName = config["roleToParentRelationshipEntityName"] ?? _roleToParentRelationshipName;

			var privateRecognizedConfigurationAttributes = new List<string>
			{
				"attributeMapParentId",
				"attributeMapUserParentId",
				"parentEntityName",
				"roleToParentRelationshipName",
				"roleToParentRelationshipEntityName",
			};

			// Remove all of the known configuration values recognized by this provider, but not the base one.
			privateRecognizedConfigurationAttributes.ForEach(config.Remove);

			// Add default attribute values for base provider, and capture them as private member variables for internal use.
			_attributeMapStateCode = config["attributeMapStateCode"] = config["attributeMapStateCode"] ?? "statecode";
			_attributeMapRoleName = config["attributeMapRoleName"] = config["attributeMapRoleName"] ?? "adx_name";
			_attributeMapRoleWebsiteId = config["attributeMapRoleWebsiteId"] = config["attributeMapRoleWebsiteId"] ?? "adx_websiteid";
			_attributeMapUsername = config["attributeMapUsername"] = config["attributeMapUsername"] ?? "adx_identity_username";
			_roleEntityName = config["roleEntityName"] = config["roleEntityName"] ?? "adx_webrole";
			_userEntityName = config["userEntityName"] = config["userEntityName"] ?? "contact";
			config["userEntityId"] = config["userEntityId"] ?? "contactid";
			config["roleEntityId"] = config["roleEntityId"] ?? "adx_webroleid";

			base.Initialize(name, config);
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			if (throwOnPopulatedRole)
			{
				var usernames = GetUsersInRoleThroughParent(roleName);

				if (usernames.Any())
				{
					throw new ProviderException("The role {0} can't be deleted because it has one or more members.".FormatWith(roleName));
				}
			}

			return base.DeleteRole(roleName, throwOnPopulatedRole);
		}

		public override string[] GetRolesForUser(string username)
		{
			// Get all role names associated with the matching user name, by joining through the N:N relationship between
			// the user parent account, and role entities associated with that account.
			var fetch = new Fetch
			{
				Entity = new FetchEntity(_roleEntityName)
				{
					Attributes = new[] { new FetchAttribute(_attributeMapRoleName) },
					Filters = new[] { new Filter { Conditions = new[]
					{
						new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
						new Condition(_attributeMapRoleWebsiteId, ConditionOperator.Equal, WebsiteID.Id)
					} } },
					Links = new[] { new Link
					{
						Name = _roleToParentRelationshipEntityName,
						ToAttribute = _attributeMapRoleId,
						FromAttribute = _attributeMapRoleId,
						Links = new[] { new Link
						{
							Name = _parentEntityName,
							ToAttribute = _attributeMapParentId,
							FromAttribute = _attributeMapParentId,
							Filters = new[] { new Filter { Conditions = new[]
							{
								new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0)
							} } },
							Links = new[] { new Link
							{
								IsUnique = true,
								Name = _userEntityName,
								ToAttribute = _attributeMapParentId,
								FromAttribute = _attributeMapUserParentId,
								Filters = new[] { new Filter { Conditions = new[]
								{
									new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
									new Condition(_attributeMapUsername, ConditionOperator.Equal, username)
								} } }
							} }
						} }
					} }
				}
			};

			var service = HttpContext.Current.GetOrganizationService();
			var entities = service.RetrieveMultiple(fetch).Entities;
			var roleNames = entities.Select(e => e.GetAttributeValue<string>(_attributeMapRoleName)).ToList();

			var baseRoleNames = base.GetRolesForUser(username);

			return roleNames
				.Where(roleName => !string.IsNullOrEmpty(roleName))
				.Union(baseRoleNames)
				.ToArray();
		}

		public override string[] GetUsersInRole(string roleName)
		{
			if (!RoleExists(roleName))
			{
				return new string[0];
			}

			var usernames = GetUsersInRoleThroughParent(roleName);

			var baseUsernames = base.GetUsersInRole(roleName);

			return usernames.Union(baseUsernames).ToArray();
		}

		private string[] GetUsersInRoleThroughParent(string roleName)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(_userEntityName)
				{
					Attributes = new[] { new FetchAttribute(_attributeMapUsername) },
					Filters = new[] { new Filter { Conditions = new[]
					{
						new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
						new Condition(_attributeMapUsername, ConditionOperator.NotNull)
					} } },
					Links = new[] { new Link
					{
						Name = _parentEntityName,
						ToAttribute = _attributeMapUserParentId,
						FromAttribute = _attributeMapParentId,
						Links = new[] { new Link
						{
							Name = _roleToParentRelationshipEntityName,
							ToAttribute = _attributeMapParentId,
							FromAttribute = _attributeMapParentId,
							Links = new[] { new Link
							{
								Name = _roleEntityName,
								ToAttribute = _attributeMapRoleId,
								FromAttribute = _attributeMapRoleId,
								Filters = new[] { new Filter { Conditions = new[]
								{
									new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
									new Condition(_attributeMapRoleName, ConditionOperator.Equal, roleName),
									new Condition(_attributeMapRoleWebsiteId, ConditionOperator.Equal, WebsiteID.Id)
								} } }
							} }
						} }
					} }
				}
			};

			var entities = HttpContext.Current.GetOrganizationService().RetrieveAll(fetch);
			var usernames = entities
				.Select(e => e.GetAttributeValue<string>(_attributeMapUsername))
				.Where(username => !string.IsNullOrEmpty(username))
				.Distinct()
				.ToArray();

			return usernames;
		}
	}
}
