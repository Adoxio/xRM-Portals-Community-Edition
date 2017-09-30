/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Security
{
	public class CrmRoleProvider : RoleProvider
	{
		private string _applicationName;
		private string _attributeMapIsAuthenticatedUsersRole;
		private string _attributeMapRoleName;
		private string _attributeMapRoleWebsiteId;
		private string _attributeMapUsername;
		private string _attributeMapStateCode;
		private string _attributeMapIsDisabled;
		private bool _authenticatedUsersRolesEnabled;
		private bool _initialized;
		private string _roleEntityName;
		private string _roleToUserRelationshipName;
		private string _userEntityName;
		private string _roleEntityId;
		private string _userEntityId;

		protected static List<string> RequiredCustomAttributes = new List<string>
		{
			"attributeMapStateCode",
			"attributeMapRoleName",
			"attributeMapRoleWebsiteId",
			"attributeMapUsername",
			"roleEntityName",
			"roleToUserRelationshipName",
			"userEntityName",
			"userEntityId",
			"roleEntityId"
		};

		protected OrganizationServiceContext CreateContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		/// <summary>
		/// Initializes the provider with the property values specified in the ASP.NET application's configuration file.
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			if (_initialized)
			{
				return;
			}

			config.ThrowOnNull("config");

			if (string.IsNullOrEmpty(name))
			{
				name = GetType().FullName;
			}

			if (string.IsNullOrEmpty(config["description"]))
			{
				config["description"] = "XRM Role Provider";
			}

			base.Initialize(name, config);

			AssertRequiredCustomAttributes(config);

			ApplicationName = config["applicationName"] ?? Utility.GetDefaultApplicationName();

			_attributeMapIsAuthenticatedUsersRole = config["attributeMapIsAuthenticatedUsersRole"];

			bool authenticatedUsersRolesEnabledConfigurationValue;

			// Whether "Authenticated Users" role is supported is determine by a config switch, and whether
			// or not an attribute map for that boolean has been supplied. The feature is enabled by default,
			// provided that an attribute map has been supplied.
			_authenticatedUsersRolesEnabled = !string.IsNullOrWhiteSpace(_attributeMapIsAuthenticatedUsersRole)
				&& bool.TryParse(config["authenticatedUsersRolesEnabled"], out authenticatedUsersRolesEnabledConfigurationValue)
					? authenticatedUsersRolesEnabledConfigurationValue
					: true;

			_attributeMapStateCode = config["attributeMapStateCode"];

			_attributeMapIsDisabled = config["attributeMapIsDisabled"];

			_attributeMapRoleName = config["attributeMapRoleName"];

			_attributeMapRoleWebsiteId = config["attributeMapRoleWebsiteId"];

			_attributeMapUsername = config["attributeMapUsername"];

			PortalName = config["portalName"];

			_roleEntityName = config["roleEntityName"];

			_roleToUserRelationshipName = config["roleToUserRelationshipName"];

			_userEntityName = config["userEntityName"];

			_roleEntityId = config["roleEntityId"];

			_userEntityId = config["userEntityId"];

			var recognizedAttributes = new List<string>
			{
				"name",
				"applicationName",
				"attributeMapStateCode",
				"attributeMapIsDisabled",
				"attributeMapIsAuthenticatedUsersRole",
				"attributeMapRoleName",
				"attributeMapRoleWebsiteId",
				"attributeMapUsername",
				"portalName",
				"roleEntityName",
				"roleToUserRelationshipName",
				"userEntityName",
				"roleEntityId",
				"userEntityId"
			};

			// Remove all of the known configuration values. If there are any left over, they are unrecognized.
			recognizedAttributes.ForEach(config.Remove);

			if (config.Count > 0)
			{
				var unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ConfigurationErrorsException("The {0} doesn't recognize or support the attribute {1}.".FormatWith(name, unrecognizedAttribute));
				}
			}

			_initialized = true;
		}

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve role information for.
		/// </summary>
		/// <returns>
		/// The name of the application to store and retrieve role information for.
		/// </returns>
		public override string ApplicationName
		{
			get { return _applicationName; }

			set
			{
				if (string.IsNullOrEmpty(value)) throw new ArgumentException("{0} - ApplicationName can't be null or empty.".FormatWith(ToString()));

				if (value.Length > 0x100) throw new ProviderException("{0} - ApplicationName is too long.".FormatWith(ToString()));

				_applicationName = value;
			}
		}

		/// <summary>
		/// The portal name to use to connect to Microsoft Dynamics CRM.
		/// </summary>
		public string PortalName { get; private set; }

		/// <summary>
		/// Gets the configured adx_website ID to which the operations of this provider are scoped.
		/// </summary>
		protected virtual EntityReference WebsiteID
		{
			get { return HttpContext.Current.GetWebsite().Entity.ToEntityReference(); }
		}

		/// <summary>
		/// Adds the specified user names to the specified roles for the configured applicationName.
		/// </summary>
		/// <param name="roleNames">A string array of the role names to add the specified user names to. </param>
		/// <param name="usernames">A string array of user names to be added to the specified roles. </param>
		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			using (var context = CreateContext())
			{
				ForEachUserAndRole(context, usernames, roleNames, (user, role) => context.AddLink(user, _roleToUserRelationshipName.ToRelationship(), role));

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Adds a new role to the data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to create.</param>
		public override void CreateRole(string roleName)
		{
			if (RoleExists(roleName))
			{
				throw new ProviderException("A role with the name {0} already exists.".FormatWith(roleName));
			}

			var webrole = new Entity(_roleEntityName);

			webrole.SetAttributeValue(_attributeMapRoleName, roleName);

			webrole.SetAttributeValue(_attributeMapRoleWebsiteId, WebsiteID);

			using (var context = CreateContext())
			{
				context.AddObject(webrole);

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Removes a role from the data source for the configured applicationName.
		/// </summary>
		/// <returns>
		/// True if the role was successfully deleted; otherwise, false.
		/// </returns>
		/// <param name="throwOnPopulatedRole">If true, then an exception will be thrown if the role has one or more members and the role will not be deleted.</param>
		/// <param name="roleName">The name of the role to delete.</param>
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			using (var context = CreateContext())
			{
				var roles = GetRolesInWebsiteByName(context, roleName).ToList();

				if (throwOnPopulatedRole)
				{
					if (roles.Where(role => role.GetRelatedEntities(context, _roleToUserRelationshipName).Any()).Any())
					{
						throw new ProviderException("The role {0} can't be deleted because it has one or more members.".FormatWith(roleName));
					}
				}

				foreach (var role in roles)
				{
					context.DeleteObject(role);
				}

				context.SaveChanges();

				return true;
			}
		}

		/// <summary>
		/// Gets an array of user names in a role where the user name contains the specified user name to match.
		/// </summary>
		/// <returns>
		/// A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
		/// </returns>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <param name="roleName">The role to search in.</param>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			var usersInRole = GetUsersInRole(roleName);

			return usersInRole.Where(username => username.Contains(usernameToMatch)).ToArray();
		}

		/// <summary>
		/// Gets an array of all the roles for the configured applicationName.
		/// </summary>
		/// <returns>
		/// A string array containing the names of all the roles stored in the data source for the configured applicationName.
		/// </returns>
		public override string[] GetAllRoles()
		{
			using (var context = CreateContext())
			{
				var roleNames = (
					from role in context.CreateQuery(_roleEntityName)
					where role.GetAttributeValue<int>(_attributeMapStateCode) == 0
						&& role.GetAttributeValue<EntityReference>(_attributeMapRoleWebsiteId) == WebsiteID
					select role)
					.ToArray().Select(e => e.GetAttributeValue<string>(_attributeMapRoleName));

				return roleNames.Distinct().ToArray();
			}
		}

		/// <summary>
		/// Gets an array of the roles that a specified user is in for the configured applicationName.
		/// </summary>
		/// <returns>
		/// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
		/// </returns>
		/// <param name="username">The user to return a list of roles for.</param>
		public override string[] GetRolesForUser(string username)
		{
			var fetch = new Fetch { Entity = new FetchEntity(_roleEntityName)
			{
				Attributes = new[] { new FetchAttribute(_attributeMapRoleName) },
				Filters = new[] { new Filter { Conditions = new[]
				{
					new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
					new Condition(_attributeMapRoleWebsiteId, ConditionOperator.Equal, WebsiteID.Id)
				} } },
				Links = new[] { new Link
				{
					Name = _roleToUserRelationshipName,
					ToAttribute = _roleEntityId,
					FromAttribute = _roleEntityId,
					Links = new[] { new Link
					{
						IsUnique = true,
						Name = _userEntityName,
						ToAttribute = _userEntityId,
						FromAttribute = _userEntityId,
						Attributes = new[] { new FetchAttribute(_userEntityId) },
						Filters = new[] { new Filter
						{
							Conditions = GetUserNamePredicate(username)
						} }
					} }
				} }
			} };

			var service = HttpContext.Current.GetOrganizationService();
			var entities = service.RetrieveMultiple(fetch).Entities;
			var roleNames = entities.Select(e => e.GetAttributeValue<string>(_attributeMapRoleName)).ToList();

			// Merge in the authenticated users roles, if that option is defined.
			if (_authenticatedUsersRolesEnabled)
			{
				fetch = new Fetch { Entity = new FetchEntity(_roleEntityName)
				{
					Attributes = new[] { new FetchAttribute(_attributeMapRoleName) },
					Filters = new[] { new Filter { Conditions = new[]
					{
						new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
						new Condition(_attributeMapRoleWebsiteId, ConditionOperator.Equal, WebsiteID.Id),
						new Condition(_attributeMapIsAuthenticatedUsersRole, ConditionOperator.Equal, true)
					} } }
				} };

				entities = service.RetrieveMultiple(fetch).Entities;
				var authenticatedUsersRoleNames = entities.Select(e => e.GetAttributeValue<string>(_attributeMapRoleName)).ToList();
				return roleNames.Union(authenticatedUsersRoleNames).Distinct().ToArray();
			}

			return roleNames.Distinct().ToArray();
		}

		/// <summary>
		/// Gets the conditions based on _attributeMapIsDisabled value
		/// </summary>
		/// <param name="userName">The username</param>
		/// <returns>The conditions.</returns>
		private Condition[] GetUserNamePredicate(string userName)
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return new[]
				{
					new Condition(_attributeMapIsDisabled, ConditionOperator.Equal, false),
					new Condition(_attributeMapUsername, ConditionOperator.Equal, userName)
				};
			}
			else
			{
				return new[]
				{
					new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
					new Condition(_attributeMapUsername, ConditionOperator.Equal, userName)
				};
			}
		}

		/// <summary>
		/// Gets an array of users in the specified role for the configured applicationName.
		/// </summary>
		/// <returns>
		/// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
		/// </returns>
		/// <param name="roleName">The name of the role to get the list of users for. </param>
		public override string[] GetUsersInRole(string roleName)
		{
			if (!RoleExists(roleName))
			{
				return new string[0];
			}
			
			var roles = GetRolesInWebsiteByName(roleName).ToList();

			if (!roles.Any())
			{
				return new string[0];
			}

			// If any of the role entities in question have the Authenticated Users flag switched on, return the names of all users with
			// a non-null username.
			if (_authenticatedUsersRolesEnabled && roles.Any(role => role.GetAttributeValue<bool?>(_attributeMapIsAuthenticatedUsersRole) == true))
			{
				var authenticatedUsers = HttpContext.Current.GetOrganizationService()
					.RetrieveAll(_userEntityName, new[] { _attributeMapUsername }, GetAllUsersPredicate())
					.Select(user => user.GetAttributeValue<string>(_attributeMapUsername))
					.Distinct()
					.ToArray();

				return authenticatedUsers;
			}

			var roleIds = roles.Select(e => e.Id).Cast<object>().ToList();

			var fetch = new Fetch
			{
				Entity = new FetchEntity(_userEntityName)
				{
					Attributes = new[] { new FetchAttribute(_attributeMapUsername) },
					Filters = new[] { new Filter { Conditions = GetAllUsersPredicate() } },
					Links = new[] { new Link
					{
						Name = _roleToUserRelationshipName,
						ToAttribute = _userEntityId,
						FromAttribute = _userEntityId,
						Links = new[] { new Link
						{
							Name = _roleEntityName,
							ToAttribute = _roleEntityId,
							FromAttribute = _roleEntityId,
							Filters = new[] { new Filter
							{
								Conditions = new[] { new Condition(_roleEntityId, ConditionOperator.In, roleIds)  }
							} }
						} }
					} }
				}
			};

			var users = HttpContext.Current.GetOrganizationService()
				.RetrieveAll(fetch)
				.Select(e => e.GetAttributeValue<string>(_attributeMapUsername))
				.Distinct()
				.ToArray();

			return users;
		}

		private Condition[] GetAllUsersPredicate()
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return new[]
				{
					new Condition(_attributeMapIsDisabled, ConditionOperator.Equal, false),
					new Condition(_attributeMapUsername, ConditionOperator.NotNull)
				};
			}
			else
			{
				return new[]
				{
					new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
					new Condition(_attributeMapUsername, ConditionOperator.NotNull)
				};
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
		/// </summary>
		/// <returns>
		/// True if the specified user is in the specified role for the configured applicationName; otherwise, false.
		/// </returns>
		/// <param name="username">The user name to search for.</param>
		/// <param name="roleName">The role to search in.</param>
		public override bool IsUserInRole(string username, string roleName)
		{
			var rolesForUser = GetRolesForUser(username);

			return rolesForUser.Contains(roleName);
		}

		/// <summary>
		/// Removes the specified user names from the specified roles for the configured applicationName.
		/// </summary>
		/// <param name="roleNames">A string array of role names to remove the specified user names from. </param>
		/// <param name="usernames">A string array of user names to be removed from the specified roles. </param>
		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			using (var context = CreateContext())
			{
				ForEachUserAndRole(context, usernames, roleNames, (user, role) => context.DeleteLink(user, _roleToUserRelationshipName.ToRelationship(), role));

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
		/// </summary>
		/// <returns>
		/// True if the role name already exists in the data source for the configured applicationName; otherwise, false.
		/// </returns>
		/// <param name="roleName">The name of the role to search for in the data source. </param>
		public override bool RoleExists(string roleName)
		{
			return GetRolesInWebsiteByName(roleName).Any();
		}

		private void AssertRequiredCustomAttributes(NameValueCollection config)
		{
			var requiredCustomAttributesNotFound = RequiredCustomAttributes.Where(attribute => string.IsNullOrEmpty(config[attribute]));

			if (requiredCustomAttributesNotFound.Any())
			{
				throw new ConfigurationErrorsException("The {0} requires the following attribute(s) to be specified:\n{1}".FormatWith(Name, string.Join("\n", requiredCustomAttributesNotFound.ToArray())));
			}
		}

		/// <summary>
		/// Finds each user and role entity specified by the usernames and role names provided, respectively, and calls a delegate for each possible pairing.
		/// </summary>
		/// <exception cref="ProviderException">
		/// Thrown if any of the specified users or roles are not found.
		/// </exception>
		private void ForEachUserAndRole(OrganizationServiceContext context, string[] usernames, string[] roleNames, Action<Entity, Entity> action)
		{
			// If there are no usernames or no roles, there's nothing to be done, so exit.
			if (!(usernames.Any() && roleNames.Any()))
			{
				return;
			}

			var users = context.CreateQuery(_userEntityName)
				.Where(ContainsPropertyValueEqual<Entity>(_attributeMapUsername, usernames))
				.Where(GetDeactivatedPredicate())
				.ToList();

			var usersNotFound = usernames.Except(users.Select(contact => contact.GetAttributeValue<string>(_attributeMapUsername)));

			if (usersNotFound.Any())
			{
				throw new ProviderException("The users {0} weren't found.".FormatWith(string.Join(", ", usersNotFound.ToArray())));
			}

			var roles = context.CreateQuery(_roleEntityName)
				.Where(ContainsPropertyValueEqual<Entity>(_attributeMapRoleName, roleNames))
				.Where(role => role.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& role.GetAttributeValue<EntityReference>(_attributeMapRoleWebsiteId) == WebsiteID)
				.ToList();

			var rolesNotFound = roleNames.Except(roles.Select(role => role.GetAttributeValue<string>(_attributeMapRoleName)));

			if (rolesNotFound.Any())
			{
				throw new ProviderException("The role(s) {0} was/were not found.".FormatWith(string.Join(", ", rolesNotFound.ToArray())));
			}

			foreach (var user in users)
			{
				foreach (var role in roles)
				{
					action(user, role);
				}
			}
		}

		private Expression<Func<Entity, bool>> GetDeactivatedPredicate()
		{
			if (!string.IsNullOrWhiteSpace(_attributeMapIsDisabled))
			{
				return user => user.GetAttributeValue<bool>(_attributeMapIsDisabled) == false;
			}
			else
			{
				return user => user.GetAttributeValue<int>(_attributeMapStateCode) == 0;
			}
		}

		private IEnumerable<Entity> GetRolesInWebsiteByName(OrganizationServiceContext context, string roleName)
		{
			return context.CreateQuery(_roleEntityName)
				.Where(role => role.GetAttributeValue<int>(_attributeMapStateCode) == 0
					&& role.GetAttributeValue<string>(_attributeMapRoleName) == roleName
					&& role.GetAttributeValue<EntityReference>(_attributeMapRoleWebsiteId) == WebsiteID);
		}

		private IEnumerable<Entity> GetRolesInWebsiteByName(string roleName)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(_roleEntityName)
				{
					Attributes = new[] { new FetchAttribute(_attributeMapRoleName), new FetchAttribute(_attributeMapIsAuthenticatedUsersRole) },
					Filters = new[] { new Filter { Conditions = new[]
					{
						new Condition(_attributeMapStateCode, ConditionOperator.Equal, 0),
						new Condition(_attributeMapRoleName, ConditionOperator.Equal, roleName),
						new Condition(_attributeMapRoleWebsiteId, ConditionOperator.Equal, WebsiteID.Id)
					} } }
				}
			};

			var service = HttpContext.Current.GetOrganizationService();
			var entities = service.RetrieveMultiple(fetch).Entities;
			return entities;
		}

		private static Expression<Func<TParameter, bool>> ContainsPropertyValueEqual<TParameter>(string crmPropertyName, IEnumerable<object> values)
		{
			var parameterType = typeof(TParameter);

			var parameter = Expression.Parameter(parameterType, parameterType.Name.ToLowerInvariant());

			var expression = ContainsPropertyValueEqual(crmPropertyName, values, parameter);

			return Expression.Lambda<Func<TParameter, bool>>(expression, parameter);
		}

		private static Expression ContainsPropertyValueEqual(string crmPropertyName, IEnumerable<object> values, ParameterExpression parameter)
		{
			var left = PropertyValueEqual(parameter, crmPropertyName, values.First());

			return ContainsPropertyValueEqual(crmPropertyName, values.Skip(1), parameter, left);
		}

		private static Expression ContainsPropertyValueEqual(string crmPropertyName, IEnumerable<object> values, ParameterExpression parameter, Expression expression)
		{
			if (!values.Any())
			{
				return expression;
			}

			var orElse = Expression.OrElse(expression, PropertyValueEqual(parameter, crmPropertyName, values.First()));

			return ContainsPropertyValueEqual(crmPropertyName, values.Skip(1), parameter, orElse);
		}

		private static Expression PropertyValueEqual(Expression parameter, string crmPropertyName, object value)
		{
			var methodCall = Expression.Call(parameter, "GetAttributeValue", new[] { typeof(string) }, Expression.Constant(crmPropertyName));

			return Expression.Equal(methodCall, Expression.Constant(value));
		}

		internal class Utility
		{
			public static string GetDefaultApplicationName()
			{
				try
				{
					string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;

					if (!string.IsNullOrEmpty(applicationVirtualPath)) return applicationVirtualPath;

					applicationVirtualPath = Process.GetCurrentProcess().MainModule.ModuleName;

					int index = applicationVirtualPath.IndexOf('.');

					if (index != -1)
					{
						applicationVirtualPath = applicationVirtualPath.Remove(index);
					}

					if (!string.IsNullOrEmpty(applicationVirtualPath)) return applicationVirtualPath;

					return "/";
				}
				catch
				{
					return "/";
				}
			}
		}
	}

	/// <summary>
	/// A <see cref="CrmRoleProvider"/> that validates 'contact' entities (users) against 'adx_webrole' entitles (roles).
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <roleManager enabled="true" defaultProvider="Xrm">
	///    <providers>
	///     <add
	///      name="Xrm"
	///      type="Microsoft.Xrm.Portal.Web.Security.CrmContactRoleProvider"
	///      portalName="Xrm" [Microsoft.Xrm.Portal.Configuration.PortalContextElement]
	///      attributeMapIsAuthenticatedUsersRole="adx_authenticatedusersrole"
	///      attributeMapRoleName="adx_name"
	///      attributeMapRoleWebsiteId="adx_websiteid"
	///      attributeMapUsername="adx_identity_username"
	///      roleEntityName="adx_webrole"
	///      roleToUserRelationshipName="adx_webrole_contact"
	///      userEntityName="contact"
	///      userEntityId="contactid"
	///      roleEntityId="adx_webroleid"
	///     />
	///    </providers>
	///   </roleManager>
	///  </system.web>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class CrmContactRoleProvider : CrmRoleProvider
	{
		public override void Initialize(string name, NameValueCollection config)
		{
			config["attributeMapStateCode"] = config["attributeMapStateCode"] ?? "statecode";

			config["attributeMapIsAuthenticatedUsersRole"] = config["attributeMapIsAuthenticatedUsersRole"] ?? "adx_authenticatedusersrole";

			config["attributeMapRoleName"] = config["attributeMapRoleName"] ?? "adx_name";

			config["attributeMapRoleWebsiteId"] = config["attributeMapRoleWebsiteId"] ?? "adx_websiteid";

			config["attributeMapUsername"] = config["attributeMapUsername"] ?? "adx_identity_username";

			config["roleEntityName"] = config["roleEntityName"] ?? "adx_webrole";

			config["roleToUserRelationshipName"] = config["roleToUserRelationshipName"] ?? "adx_webrole_contact";

			config["userEntityName"] = config["userEntityName"] ?? "contact";

			config["userEntityId"] = config["userEntityId"] ?? "contactid";

			config["roleEntityId"] = "adx_webroleid";

			base.Initialize(name, config);
		}
	}
}
