/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Performance;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Services;

namespace Adxstudio.Xrm.Security
{
	/// <summary>
	/// Assertion of privileges for a given entity record or set of entities as determined by the set of entity permissions defined in CRM that are applicable to the current user.
	/// </summary>
	public class CrmEntityPermissionProvider
	{
		private const string PermissionsFetchXmlFormat = @"
			<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
				<entity name=""adx_entitypermission"">
					<attribute name=""adx_entitypermissionid"" />
					<attribute name=""adx_entityname"" />
					<attribute name=""adx_entitylogicalname"" />
					<attribute name=""adx_create"" />
					<attribute name=""adx_read"" />
					<attribute name=""adx_write"" />
					<attribute name=""adx_delete"" />
					<attribute name=""adx_appendto"" />
					<attribute name=""adx_append"" />
					<attribute name=""adx_scope"" />
					<attribute name=""adx_accountrelationship"" />
					<attribute name=""adx_contactrelationship"" />
					<attribute name=""adx_parentrelationship"" />
					<attribute name=""adx_parententitypermission"" />
					<order attribute=""adx_entityname"" descending=""false"" />
					<filter type=""and"">
						<condition attribute=""statecode"" operator=""eq"" value=""0"" />
						<condition attribute=""adx_entitylogicalname"" operator=""not-null"" />
					</filter>
				</entity>
			</fetch>";

		/// <summary>
		/// Loads the tree of entity permissions applicable to the current website and a tree of entity permissions applicable to the current user.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="website"></param>
		protected void BuildEntityPermissionTrees(OrganizationServiceContext serviceContext, EntityReference website)
		{
			// Retrieve all entity permissions
			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.BuildEntityPermissionTrees))
			{
				var fetchPermissions = Fetch.Parse(PermissionsFetchXmlFormat);

				var responsePermissions = (serviceContext as IOrganizationService).RetrieveMultiple(fetchPermissions);

				// Retrieve all entity permission web roles - cannot use the content map as that filters by current website and we need all roles. Once the tree has been built, non website related web role's permissions will be pruned.
				var fetchPermissionRolesIntersect = new Fetch
				{
					Entity = new FetchEntity("adx_webrole")
					{
						Attributes = FetchAttribute.All,
						Links = new[] { new Link
						{
							Name = "adx_entitypermission_webrole",
							ToAttribute = "adx_webroleid",
							FromAttribute = "adx_webroleid",
							Alias = "epw",
							Attributes = new[] { new FetchAttribute("adx_entitypermissionid") }
						} }
					}
				};

				var entityPermissionWebRoleIntersects =
					(serviceContext as IOrganizationService).RetrieveMultiple(fetchPermissionRolesIntersect)
						.Entities.Select(e => new EntityPermissionWebRoleIntersect(e.GetAttributeAliasedValue<Guid>("epw.adx_entitypermissionid"), e)).ToList();

				PermissionTree = BuildEntityPermissionTree(serviceContext, website, responsePermissions.Entities, entityPermissionWebRoleIntersects);

				CurrentUserPermissionTree = BuildEntityPermissionTree(serviceContext, website, responsePermissions.Entities, entityPermissionWebRoleIntersects, true);
			}
		}

		/// <summary>
		/// Create a tree of entity permissions
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="website"></param>
		/// <param name="permissionEntities"></param>
		/// <param name="permissionWebRoleIntersects"></param>
		/// <param name="trimTreeForCurrentUser">If true, permissions that do not belong to web roles associated with the user will be pruned from the tree.</param>
		/// <returns>Tree of entity permissions</returns>
		protected Tree<EntityPermission> BuildEntityPermissionTree(OrganizationServiceContext serviceContext, EntityReference website, DataCollection<Entity> permissionEntities, List<EntityPermissionWebRoleIntersect> permissionWebRoleIntersects, bool trimTreeForCurrentUser = false)
		{
			var entityPermissions = permissionEntities.Select(p => new EntityPermission(p)).ToList();

			// Assign the web roles to each entity permission

			foreach (var entityPermission in entityPermissions)
			{
				var permission = entityPermission;
				var webroles = permissionWebRoleIntersects.Where(e => e.EntityPermissionId == permission.EntityReference.Id).Select(e => e.WebRole).ToList();
				entityPermission.WebRoles = webroles;
			}

			// create nodes of the tree

			var entityPermissionNodes = entityPermissions.Select(p => new TreeNode<EntityPermission>(p)).ToList();
			var removeNodesFromRoot = new List<TreeNode<EntityPermission>>();
			foreach (var node in entityPermissionNodes)
			{
				var parent = node.Value.ParentEntityPermission == null
					? null
					: entityPermissionNodes.FirstOrDefault(
						n => n.Value.EntityReference.Id == node.Value.ParentEntityPermission.Id);

				if (parent == null)
				{
					continue;
				}

				node.Parent = parent;
				removeNodesFromRoot.Add(node);
			}

			foreach (var node in removeNodesFromRoot)
			{
				entityPermissionNodes.Remove(node);
			}

			// Remove permission nodes that are exclusively related to web roles that are not associated with the current website.
			// Child permissions are not directly related to the web role so by removing this parent node, it's children and grandchildren are also eliminated.

			var removeNonWebsiteWebRoleRelatedNodes = (from node in entityPermissionNodes
				where node != null && node.Value != null
				where
				node.Value.WebRoles == null ||
				node.Value.WebRoles.All(
					role =>
						role.GetAttributeValue<EntityReference>("adx_websiteid") == null ||
						role.GetAttributeValue<EntityReference>("adx_websiteid").Id != website?.Id)
				select node).ToList();

			foreach (var node in removeNonWebsiteWebRoleRelatedNodes)
			{
				entityPermissionNodes.Remove(node);
			}

			if (trimTreeForCurrentUser)
			{
				// remove entity permission nodes that do not belong to web roles associated with the current user.

				var removeNonUserWebRoleRelatedNodes = (from node in entityPermissionNodes
														where node != null && node.Value != null
														where node.Value.WebRoles == null || !node.Value.WebRoles.Any(
																role => CurrentUserRoleNames.Contains(role.GetAttributeValue<string>("adx_name")))
														select node).ToList();

				foreach (var node in removeNonUserWebRoleRelatedNodes)
				{
					entityPermissionNodes.Remove(node);
				}
			}

			// Create tree and add the nodes

			var permissionTree = new Tree<EntityPermission>(new EntityPermission());

			permissionTree.Children.AddRange(entityPermissionNodes);

			return permissionTree;
		}

		/// <summary>
		/// Indicates the result of an entity permission evalutation.
		/// </summary>
		public class EntityPermissionEvaluation
		{
			/// <summary>
			/// Indicates whether global permission was granted or not
			/// </summary>
			public bool GlobalPermissionGranted { get; private set; }

			/// <summary>
			/// Indicates whether permission was granted or not
			/// </summary>
			public bool PermissionGranted { get; private set; }

			/// <summary>
			/// Indicates whether permission rules exist or not
			/// </summary>
			public bool RulesExist { get; private set; }

			/// <summary>
			/// Permission Tree Nodes applicable for the given entity name.
			/// </summary>
			public List<ITreeNode<EntityPermission>> Permissions { get; private set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="permissionGranted"></param>
			/// <param name="globalPermissionGranted"></param>
			/// <param name="rulesExist"></param>
			/// <param name="permissions"></param>
			public EntityPermissionEvaluation(bool permissionGranted = false, bool globalPermissionGranted = false,
				bool rulesExist = false, List<ITreeNode<EntityPermission>> permissions = null)
			{
				GlobalPermissionGranted = globalPermissionGranted;
				PermissionGranted = permissionGranted;
				RulesExist = rulesExist;
				Permissions = permissions ?? new List<ITreeNode<EntityPermission>>();
			}
		}

		/// <summary>
		/// Indicates the result of an entity permission right assertion.
		/// </summary>
		public class EntityPermissionRightResult
		{
			/// <summary>
			/// Indicates whether global permission was granted or not
			/// </summary>
			public bool GlobalPermissionGranted { get; private set; }

			/// <summary>
			/// Indicates whether permission was granted or not
			/// </summary>
			public bool PermissionGranted { get; private set; }

			/// <summary>
			/// Indicates whether permission rules exist or not
			/// </summary>
			public bool RulesExist { get; private set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="permissionGranted"></param>
			/// <param name="globalPermissionGranted"></param>
			/// <param name="rulesExist"></param>
			public EntityPermissionRightResult(bool permissionGranted = false, bool globalPermissionGranted = false,
				bool rulesExist = false)
			{
				GlobalPermissionGranted = globalPermissionGranted;
				PermissionGranted = permissionGranted;
				RulesExist = rulesExist;
			}
		}

		/// <summary>
		/// Indicates the resulting privileges of an entity permission assertion.
		/// </summary>
		public class EntityPermissionResult
		{
			/// <summary>
			/// User can read record
			/// </summary>
			public bool CanRead { get; private set; }
			/// <summary>
			/// User can update record
			/// </summary>
			public bool CanWrite { get; private set; }
			/// <summary>
			/// User can create records of this type
			/// </summary>
			public bool CanCreate { get; private set; }
			/// <summary>
			/// User can delete record
			/// </summary>
			public bool CanDelete { get; private set; }
			/// <summary>
			/// User can attach another record to the specified record. The Append and Append To rights work in combination.
			/// </summary>
			public bool CanAppend { get; private set; }
			/// <summary>
			/// User can append this record to another record. The Append and Append To rights work in combination.
			/// </summary>
			public bool CanAppendTo { get; private set; }
			/// <summary>
			/// Indicates whether permission rules exist or not
			/// </summary>
			public bool RulesExist { get; private set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="read"></param>
			/// <param name="write"></param>
			/// <param name="create"></param>
			/// <param name="delete"></param>
			/// <param name="append"></param>
			/// <param name="appendTo"></param>
			/// <param name="rulesExist"></param>
			public EntityPermissionResult(bool rulesExist = true, bool read = false, bool write = false, bool create = false, bool delete = false, bool append = false, bool appendTo = false)
			{
				RulesExist = rulesExist;
				CanRead = read;
				CanWrite = write;
				CanCreate = create;
				CanDelete = delete;
				CanAppend = append;
				CanAppendTo = appendTo;
			}
		}

		/// <summary>
		/// Information to aid in building the link-entity elements in FetchXml.
		/// </summary>
		protected class LinkDetails
		{
			/// <summary>
			/// Primary entity logical name
			/// </summary>
			public string Entity1Name { get; private set; }
			/// <summary>
			/// Secondary entity logical name
			/// </summary>
			public string Entity2Name { get; private set; }
			/// <summary>
			/// Schema name of the relationship between the two entities.
			/// </summary>
			public string RelationshipName { get; private set; }

			/// <summary>
			/// Scope of the permission rule.
			/// </summary>
			public EntityPermissionScope? Scope { get; set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="entity1Name"></param>
			/// <param name="entity2Name"></param>
			/// <param name="relationshipName"></param>
			/// <param name="scope"></param>
			public LinkDetails(string entity1Name, string entity2Name, string relationshipName, EntityPermissionScope? scope = null)
			{
				Entity1Name = entity1Name;
				Entity2Name = entity2Name;
				RelationshipName = relationshipName;
				Scope = scope;
			}
		}

		/// <summary>
		/// Represent the web role associated with an entity permission
		/// </summary>
		public class EntityPermissionWebRoleIntersect
		{
			/// <summary>
			/// Id of the entity permission
			/// </summary>
			public Guid EntityPermissionId { get; private set; }
			/// <summary>
			/// Web Role entity associated with the entity permission
			/// </summary>
			public Entity WebRole { get; private set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="entityPermissionId"></param>
			/// <param name="webRole"></param>
			public EntityPermissionWebRoleIntersect(Guid entityPermissionId, Entity webRole)
			{
				EntityPermissionId = entityPermissionId;
				WebRole = webRole;
			}
		}

		/// <summary>
		/// ContentMapCrmEntityPermissionProvider constructor
		/// </summary>
		/// <param name="portalName">The portal configuration that the control binds to.</param>
		public CrmEntityPermissionProvider(string portalName = null)
		{
			PortalName = portalName;

			Portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			CurrentUserRoleNames = GetRolesForUser(Portal.ServiceContext, Portal.Website.ToEntityReference());

			BuildEntityPermissionTrees(Portal.ServiceContext, Portal.Website.ToEntityReference());

			UseUnionHint = FeatureCheckHelper.IsFeatureEnabled(FeatureNames.EntityPermissionFetchUnionHint)
				&& "EntityPermissions.UseUnionHint".ResolveAppSetting().ToBoolean().GetValueOrDefault(true);
		}

		/// <summary>
		/// Tree of permissions that have been loaded when the provider is initialized.
		/// </summary>
		public Tree<EntityPermission> PermissionTree { get; private set; }

		/// <summary>
		/// Tree of permissions that have been loaded for the current user when the provider is initialized.
		/// </summary>
		public Tree<EntityPermission> CurrentUserPermissionTree { get; private set; }

		/// <summary>
		/// Indicates if entity permission records exist or not.
		/// </summary>
		public bool PermissionsExist
		{
			get { return PermissionTree != null && PermissionTree.Children.Any(); }
		}

		/// <summary>
		/// Indicates if the current user has entity permission records.
		/// </summary>
		public bool CurrentUserHasPermissions
		{
			get { return CurrentUserPermissionTree != null && CurrentUserPermissionTree.Children.Any(); }
		}

		/// <summary>
		/// An array of role names for the current user.
		/// </summary>
		public string[] CurrentUserRoleNames { get; private set; }

		/// <summary>
		/// Indicates whether the current user belongs to any roles.
		/// </summary>
		public bool CurrentUserHasRole
		{
			get { return CurrentUserRoleNames.Any(); }
		}

		/// <summary>
		/// The portal configuration that the control binds to.
		/// </summary>
		protected string PortalName { get; private set; }

		/// <summary>
		/// The portal context that contains the current <see cref="Entity"/>, Website and User.
		/// </summary>
		protected IPortalContext Portal { get; private set; }

		/// <summary>
		/// Add the hint="union" attribute to the main permission filter, allowing supporting CRM servers
		/// to further optimize the query.
		/// </summary>
		protected bool UseUnionHint { get; private set; }

		protected virtual List<ITreeNode<EntityPermission>> GetEntitySpecificPermissions(string entityName, EntityReference regarding = null, bool trimTreeForCurrentUser = true)
		{
			List<ITreeNode<EntityPermission>> entitySpecificPermissionNodes;

			var entityPermissionTree = trimTreeForCurrentUser ? CurrentUserPermissionTree : PermissionTree;

			if (regarding == null)
			{
				entitySpecificPermissionNodes =
					entityPermissionTree.Descendants.Where(d => d.Value != null && d.Value.EntityName == entityName)
						.ToList();
			}
			else
			{
				entitySpecificPermissionNodes =
					entityPermissionTree.Descendants.Where(
						d => (d.Value != null && d.Value.EntityName == entityName && d.Value.Scope == EntityPermissionScope.Global) ||
							(d.Value != null && d.Value.EntityName == entityName &&
							(d.Parent == null || d.Parent.Value == null || d.Parent.Value.EntityName == regarding.LogicalName))).ToList();
			}

			return entitySpecificPermissionNodes;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for the given entity type set and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="entityName">Logica name of the entity to assert privilege</param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>True if <see cref="CrmEntityPermissionRight"/> is granted, otherwise false.</returns>
		public virtual bool TryAssert(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, string entityName, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: entityName={0} right={1}", EntityNamePrivacy.GetEntityName(entityName), right.ToString()));

			var assert = Assert(serviceContext, right, entityName, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: entityName={0} right={1}", EntityNamePrivacy.GetEntityName(entityName), right.ToString()));

			return assert;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for the given entity type set and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="entityName"></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>True if <see cref="CrmEntityPermissionRight"/> is granted, otherwise false.</returns>
		protected virtual bool Assert(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, string entityName, EntityReference regarding = null)
		{
			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.Assert))
			{
				entityName.ThrowOnNullOrWhitespace("entityName");

				if (!PermissionsExist)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. There are no entity permission records defined in the system associated to valid web roles.");

					return false;
				}

				if (!CurrentUserHasRole)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. The current user does not belong to any web roles.");

					return false;
				}

				if (!CurrentUserHasPermissions)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. No entity permissions apply to the current user.");

					return false;
				}

				var entitySpecificPermissionNodes = GetEntitySpecificPermissions(entityName, regarding);

				if (!entitySpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied. There are no entity permission records defined in the system associated to web roles for the current user where entity name is '{0}'.", EntityNamePrivacy.GetEntityName(entityName)));

					return false;
				}

				List<ITreeNode<EntityPermission>> globalPermissions;
				List<ITreeNode<EntityPermission>> rightSpecificPermissions;

				switch (right)
				{
					case CrmEntityPermissionRight.Read:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Write:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Create:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Delete:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Append:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.AppendTo:
						globalPermissions = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissions = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					default:
						throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
				}

				var globalPermissionGranted = globalPermissions.Any();

				if (globalPermissionGranted)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));

					return true;
				}

				if (!rightSpecificPermissions.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity for '{1}' right. There are no entity permissions for entity type {0} where '{1}' right is true.", EntityNamePrivacy.GetEntityName(entityName), right));

					return false;
				}

				// note: record level filters must be applied to the fetch in order to assert privileges at the row level.

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right. note: record level filters must be applied to the fetch in order to assert privileges at the row level.", EntityNamePrivacy.GetEntityName(entityName), right));
			}

			return true;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for the given entity type set and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="fetchIn"></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if privilege is granted. <see cref="EntityPermissionRightResult"/></returns>
		public virtual EntityPermissionRightResult TryApplyRecordLevelFiltersToFetch(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, Fetch fetchIn, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: right={0}", right.ToString()));

			var result = ApplyRecordLevelFiltersToFetch(serviceContext, right, fetchIn, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: right={0}", right.ToString()));

			return result;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for the given entity type set and the current user. Applies the appropriate contact/account scoped filters to an existing <see cref="Fetch"/> to achieve record level security trimming.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="fetchIn"></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if privilege is granted. <see cref="EntityPermissionRightResult"/></returns>
		protected virtual EntityPermissionRightResult ApplyRecordLevelFiltersToFetch(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, Fetch fetchIn, EntityReference regarding = null)
		{
			fetchIn.ThrowOnNull("fetchIn");

			var entityName = fetchIn.Entity.Name;

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.RecordLevelFiltersToFetch))
			{
				if (string.IsNullOrWhiteSpace(entityName))
				{
					throw new ApplicationException("Fetch must contain and entity element with a name property value.");
				}

				if (!PermissionsExist)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. There are no entity permission records defined in the system associated to valid web roles.");

					return new EntityPermissionRightResult();
				}

				if (!CurrentUserHasRole)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. The current user does not belong to any web roles.");

					return new EntityPermissionRightResult(false, false, true);
				}

				if (!CurrentUserHasPermissions)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. No entity permissions apply to the current user.");

					return new EntityPermissionRightResult(false, false, true);
				}

				var entitySpecificPermissionNodes = GetEntitySpecificPermissions(entityName, regarding);

				if (!entitySpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied. There are no entity permission records defined in the system associated to web roles for the current user where entity name is '{0}'.", EntityNamePrivacy.GetEntityName(entityName)));

					return new EntityPermissionRightResult(false, false, true);
				}

				List<ITreeNode<EntityPermission>> globalPermissionNodes;
				List<ITreeNode<EntityPermission>> rightSpecificPermissionNodes;

				switch (right)
				{
					case CrmEntityPermissionRight.Read:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Write:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Create:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Delete:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Append:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.AppendTo:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					default:
						throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
				}

				var globalPermissionGranted = globalPermissionNodes.Any();

				if (globalPermissionGranted)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Global permission granted on entity set '{0}' for '{1}' right. Fetch not altered.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new EntityPermissionRightResult(true, true, true);
				}

				if (!rightSpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity for '{1}' right. There are no entity permissions for entity type {0} where '{1}' right is true.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new EntityPermissionRightResult(false, false, true);
				}

				if (right == CrmEntityPermissionRight.Create)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new EntityPermissionRightResult(true, false, true);
				}

				var contactAndAccount = GetContactAndAccount();

				// Modify the fetch to include the link-entity(s) and filter conditions.
				var filter = new Filter { Type = LogicalOperator.Or, Filters = new List<Filter>(), Conditions = new List<Condition>() };

				if (UseUnionHint)
				{
					filter.Hint = Filter.Hints.Union;
				}

				var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance(fetchIn);
				foreach (var permissionNode in rightSpecificPermissionNodes)
				{
					ApplyEntityLinksAndFilterToFetch(serviceContext, permissionNode, fetchIn, contactAndAccount.Item1, contactAndAccount.Item2, filter, linkEntityAliasGenerator);
				}
				AddFilterToFetch(fetchIn, filter);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permissions applicable on '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));
			}

			return new EntityPermissionRightResult(true, false, true);
		}

		/// <summary>
		/// Generate a separate fetch query for each record-level entity permission filter defined in the portal configuration.
		/// </summary>
		/// <param name="serviceContext">The <see cref="OrganizationServiceContext"/> used to communicate to CRM.</param>
		/// <param name="right">The <see cref="CrmEntityPermissionRight"/> to be asserted.</param>
		/// <param name="fetchIn">The base query to modify with record-level permission filters.</param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if privilege is granted. <see cref="EntityPermissionRightResult"/></returns>
		public virtual Tuple<EntityPermissionRightResult, IEnumerable<Fetch>> GenerateFetchForEachRecordLevelFilter(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, Fetch fetchIn, EntityReference regarding = null)
		{
			fetchIn.ThrowOnNull("fetchIn");

			var entityName = fetchIn.Entity.Name;

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.RecordLevelFiltersToFetch))
			{
				if (string.IsNullOrWhiteSpace(entityName))
				{
					throw new ApplicationException("Fetch must contain and entity element with a name property value.");
				}

				if (!PermissionsExist)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. There are no entity permission records defined in the system associated to valid web roles.");

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(), new[] { fetchIn });
				}

				if (!CurrentUserHasRole)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. The current user does not belong to any web roles.");

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(false, false, true), new[] { fetchIn });
				}

				if (!CurrentUserHasPermissions)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. No entity permissions apply to the current user.");

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(false, false, true), new[] { fetchIn });
				}

				var entitySpecificPermissionNodes = GetEntitySpecificPermissions(entityName, regarding);

				if (!entitySpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied. There are no entity permission records defined in the system associated to web roles for the current user where entity name is '{0}'.", EntityNamePrivacy.GetEntityName(entityName)));

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(false, false, true), new[] { fetchIn });
				}

				List<ITreeNode<EntityPermission>> globalPermissionNodes;
				List<ITreeNode<EntityPermission>> rightSpecificPermissionNodes;

				switch (right)
				{
					case CrmEntityPermissionRight.Read:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Write:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Create:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Delete:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Append:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.AppendTo:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					default:
						throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
				}

				var globalPermissionGranted = globalPermissionNodes.Any();

				if (globalPermissionGranted)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Global permission granted on entity set '{0}' for '{1}' right. Fetch not altered.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(true, true, true), new[] { fetchIn });
				}

				if (!rightSpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity for '{1}' right. There are no entity permissions for entity type {0} where '{1}' right is true.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(false, false, true), new[] { fetchIn });
				}

				if (right == CrmEntityPermissionRight.Create)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));

					return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(true, false, true), new[] { fetchIn });
				}

				var contactAndAccount = GetContactAndAccount();

				var queries = rightSpecificPermissionNodes.Select(permissionNode =>
				{
					var fetch = Fetch.Parse(fetchIn.ToXml());

					// Modify the fetch to include the link-entity(s) and filter conditions.
					var filter = new Filter { Type = LogicalOperator.Or, Filters = new List<Filter>(), Conditions = new List<Condition>() };

					if (UseUnionHint)
					{
						filter.Hint = Filter.Hints.Union;
					}

					var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance(fetch);

					ApplyEntityLinksAndFilterToFetch(serviceContext, permissionNode, fetch, contactAndAccount.Item1, contactAndAccount.Item2, filter, linkEntityAliasGenerator);
					AddFilterToFetch(fetch, filter);

					return fetch;
				}).ToArray();
				
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permissions applicable on '{0}' for '{1}' right. Fetch modified.", EntityNamePrivacy.GetEntityName(entityName), right));

				return new Tuple<EntityPermissionRightResult, IEnumerable<Fetch>>(new EntityPermissionRightResult(true, false, true), queries);
			}
		}

		/// <summary>
		/// Add a filter to the fetch
		/// </summary>
		/// <param name="fetchIn"></param>
		/// <param name="filter"></param>
		protected virtual void AddFilterToFetch(Fetch fetchIn, Filter filter)
		{
			if (fetchIn.Entity.Filters == null || !fetchIn.Entity.Filters.Any())
			{
				fetchIn.Entity.Filters = new List<Filter>
				{
					new Filter { Type = LogicalOperator.And, Filters = new List<Filter> { filter } }
				};
			}
			else
			{
				fetchIn.Entity.Filters.Add(new Filter { Type = LogicalOperator.And, Filters = new List<Filter> { filter } });
			}
		}

		/// <summary>
		///  Applies the appropriate link-entity(s) to an existing <see cref="Fetch"/> to and builds the filter to achieve record level security trimming.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="permissionNode"></param>
		/// <param name="fetchIn"></param>
		/// <param name="contact"></param>
		/// <param name="account"></param>
		/// <param name="filter"></param>
		/// <param name="linkEntityAliasGenerator"></param>
		protected virtual void ApplyEntityLinksAndFilterToFetch(OrganizationServiceContext serviceContext, ITreeNode<EntityPermission> permissionNode, Fetch fetchIn, EntityReference contact, EntityReference account, Filter filter, LinkEntityAliasGenerator linkEntityAliasGenerator)
		{
			fetchIn.ThrowOnNull("fetchIn");

			var entityName = fetchIn.Entity.Name;

			if (string.IsNullOrWhiteSpace(entityName))
			{
				throw new ApplicationException("Fetch must contain and entity element with a name property value.");
			}

			// Modify the fetch to include the link-entity(s) and filter conditions.

			var linkDetails = new List<LinkDetails>();

			// Walk the permission ancestral chain to collect the relationship link information.

			BuildLinkDetails(permissionNode, linkDetails);

			// Generate the link-entity(s) and filter appropriate for the current permission's ancestral chain of relationships

			var link = new Link();

			for (var index = 0; index < linkDetails.Count; index++)
			{
				var linkDetail = linkDetails[index];

				BuildLinksAndFilter(serviceContext, linkDetail, fetchIn, link, filter, contact, account, index + 1 == linkDetails.Count, linkEntityAliasGenerator);
			}

			// apply the link-entity(s) and filter to the fetch

			if (link.Name == null) return;

			if (fetchIn.Entity.Links == null)
			{
				fetchIn.Entity.Links = new List<Link> { link };
			}
			else
			{
				fetchIn.Entity.Links.Add(link);
			}
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entityReference"><see cref="EntityReference"/> to assert permission on</param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>True if <see cref="CrmEntityPermissionRight"/> is granted, otherwise false.</returns>
		public virtual bool TryAssert(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, EntityReference entityReference, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: entityName={0} right={1}", entityReference != null ? EntityNamePrivacy.GetEntityName(entityReference.LogicalName) : "unknown", right.ToString()));

			if (entityReference == null)
			{
				throw new NullReferenceException("entityReference");
			}

			if (entityMetadata == null) entityMetadata = GetEntityMetadata(serviceContext, entityReference.LogicalName);
			var primaryKeyName = entityMetadata.PrimaryIdAttribute;

			var fetch = new Fetch
			{
				Entity = new FetchEntity(entityReference.LogicalName)
				{
					Filters = new[] { new Filter { Conditions = new[] { new Condition(primaryKeyName, ConditionOperator.Equal, entityReference.Id) } } }
				}
			};

			var entity = serviceContext.RetrieveSingle(fetch, enforceFirst: true);

			var assert = TryAssert(serviceContext, right, entity, entityMetadata, readGranted, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: entityName={0} right={1}", EntityNamePrivacy.GetEntityName(entityReference.LogicalName), right.ToString()));

			return assert;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entity"><see cref="Entity"/> to assert permission on</param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>True if <see cref="CrmEntityPermissionRight"/> is granted, otherwise false.</returns>
		public virtual bool TryAssert(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, Entity entity, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: entityName={0} right={1}", entity != null ? EntityNamePrivacy.GetEntityName(entity.LogicalName) : "unknown", right.ToString()));

			var assert = Assert(serviceContext, right, entity, entityMetadata, readGranted, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: entityName={0} right={1}", entity != null ? EntityNamePrivacy.GetEntityName(entity.LogicalName) : "unknown", right.ToString()));

			return assert;
		}

		/// <summary>
		/// Assertion of the <see cref="CrmEntityPermissionRight"/> for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entity"><see cref="Entity"/> to assert permission on</param>
		/// <param name="right"><see cref="CrmEntityPermissionRight"/></param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>True if <see cref="CrmEntityPermissionRight"/> is granted, otherwise false.</returns>
		protected virtual bool Assert(OrganizationServiceContext serviceContext, CrmEntityPermissionRight right, Entity entity, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			entity.ThrowOnNull("entity");

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.Assert))
			{
				var entityName = entity.LogicalName;

				if (!PermissionsExist)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. There are no entity permission records defined in the system associated to valid web roles.");

					return false;
				}

				if (!CurrentUserHasRole)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. The current user does not belong to any web roles.");

					return false;
				}

				if (!CurrentUserHasPermissions)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. No entity permissions apply to the current user.");

					return false;
				}

				var entitySpecificPermissionNodes = GetEntitySpecificPermissions(entityName, regarding);

				if (!entitySpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied. There are no entity permission records defined in the system associated to web roles for the current user where entity name is '{0}'.", EntityNamePrivacy.GetEntityName(entityName)));

					return false;
				}

				List<ITreeNode<EntityPermission>> globalPermissionNodes;
				List<ITreeNode<EntityPermission>> rightSpecificPermissionNodes;

				switch (right)
				{
					case CrmEntityPermissionRight.Read:
						if (readGranted) return true;
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Write:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Create:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Delete:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.Append:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					case CrmEntityPermissionRight.AppendTo:
						globalPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
						rightSpecificPermissionNodes = entitySpecificPermissionNodes.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
						break;
					default:
						throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
				}

				var globalPermissionGranted = globalPermissionNodes.Any();

				if (globalPermissionGranted)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Global permission granted on entity set '{0}' for '{1}' right. Fetch not altered.", EntityNamePrivacy.GetEntityName(entityName), right));

					return true;
				}

				if (!rightSpecificPermissionNodes.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity for '{1}' right. There are no entity permissions for entity type {0} where '{1}' right is true.", EntityNamePrivacy.GetEntityName(entityName), right));

					return false;
				}

				if (right == CrmEntityPermissionRight.Create)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));

					return true;
				}

				var contactAndAccount = GetContactAndAccount();

				// To test right for a single entity record, build a fetch for the entity with a filter equality condition specifying the entity record id, then inject the link-entity(s) and filter applicable for each rule.
				// Execute fetch, if the one record is returned then privilege granted.

				var fetch = BuildRecordFetch(serviceContext, entity);
				var filter = new Filter { Type = LogicalOperator.Or, Filters = new List<Filter>(), Conditions = new List<Condition>() };

				if (UseUnionHint)
				{
					filter.Hint = Filter.Hints.Union;
				}

				var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance(fetch);

				foreach (var permissionNode in rightSpecificPermissionNodes)
				{
					ApplyEntityLinksAndFilterToFetch(serviceContext, permissionNode, fetch, contactAndAccount.Item1, contactAndAccount.Item2, filter, linkEntityAliasGenerator);
				}

				AddFilterToFetch(fetch, filter);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permissions applicable on '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entityName), right));

				var response = serviceContext.RetrieveSingle(fetch);

				return response != null;
			}
		}

		/// <summary>
		/// Assertion of the complete set of rights for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entityReference"><see cref="EntityReference"/> to assert permission on</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if user can read, write, create, delete, append, appendTo. <see cref="EntityPermissionResult"/></returns>
		public virtual EntityPermissionResult TryAssert(OrganizationServiceContext serviceContext, EntityReference entityReference, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: entityName={0}", entityReference != null ? EntityNamePrivacy.GetEntityName(entityReference.LogicalName) : "unknown"));

			if (entityReference == null)
			{
				throw new NullReferenceException("entityReference");
			}

			if (entityMetadata == null) entityMetadata = GetEntityMetadata(serviceContext, entityReference.LogicalName);
			var primaryKeyName = entityMetadata.PrimaryIdAttribute;

			var fetch = new Fetch
			{
				Entity = new FetchEntity(entityReference.LogicalName)
				{
					Filters = new[] { new Filter { Conditions = new[] { new Condition(primaryKeyName, ConditionOperator.Equal, entityReference.Id) } } }
				}
			};

			var entity = serviceContext.RetrieveSingle(fetch, enforceFirst: true);

			var result = TryAssert(serviceContext, entity, entityMetadata, readGranted, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: entityName={0}", EntityNamePrivacy.GetEntityName(entityReference.LogicalName)));

			return result;
		}

		/// <summary>
		/// Assertion of the complete set of rights for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entity"><see cref="Entity"/> to assert permission on</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if user can read, write, create, delete, append, appendTo. <see cref="EntityPermissionResult"/></returns>
		public virtual EntityPermissionResult TryAssert(OrganizationServiceContext serviceContext, Entity entity, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: entityName={0}", entity != null ? EntityNamePrivacy.GetEntityName(entity.LogicalName) : "unknown"));

			var result = Assert(serviceContext, entity, entityMetadata, readGranted, regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: entityName={0}", entity != null ? EntityNamePrivacy.GetEntityName(entity.LogicalName) : "unknown"));

			return result;
		}

		/// <summary>
		/// Assertion of the complete set of rights for a single entity record and the current user.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entity"><see cref="Entity"/> to assert permission on</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <param name="regarding">An optional <see cref="EntityReference"/> "parent" regarding the related "child" entity record(s) being tested.</param>
		/// <returns>Result of permission test indicating if user can read, write, create, delete, append, appendTo. <see cref="EntityPermissionResult"/></returns>
		protected virtual EntityPermissionResult Assert(OrganizationServiceContext serviceContext, Entity entity, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			entity.ThrowOnNull("entity");

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.Assert))
			{
				if (!PermissionsExist)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. There are no entity permission records defined in the system associated to valid web roles.");

					return new EntityPermissionResult(false);
				}

				if (!CurrentUserHasRole)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. The current user does not belong to any web roles.");

					return new EntityPermissionResult();
				}

				if (!CurrentUserHasPermissions)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission denied. No entity permissions apply to the current user.");

					return new EntityPermissionResult();
				}

				var entitySpecificPermissions = GetEntitySpecificPermissions(entity.LogicalName, regarding);

				if (!entitySpecificPermissions.Any())
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied. There are no entity permission records defined in the system associated to web roles for the current user where entity name is '{0}'.", EntityNamePrivacy.GetEntityName(entity.LogicalName)));

					return new EntityPermissionResult();
				}

				var permissionsTested = new List<PermissionTest>();
				var canRead = TryTestRight(serviceContext, entity, CrmEntityPermissionRight.Read, entitySpecificPermissions, permissionsTested, entityMetadata, readGranted, regarding);
				var canWrite = TryTestRight(serviceContext, entity, CrmEntityPermissionRight.Write, entitySpecificPermissions, permissionsTested, entityMetadata, readGranted, regarding);
				var canDelete = TryTestRight(serviceContext, entity, CrmEntityPermissionRight.Delete, entitySpecificPermissions, permissionsTested, entityMetadata, readGranted, regarding);
				var canAppend = TryTestRight(serviceContext, entity, CrmEntityPermissionRight.Append, entitySpecificPermissions, permissionsTested, entityMetadata, readGranted, regarding);
				var canAppendTo = TryTestRight(serviceContext, entity, CrmEntityPermissionRight.AppendTo, entitySpecificPermissions, permissionsTested, entityMetadata, readGranted, regarding);
				var canCreate = TestRight(serviceContext, entity, CrmEntityPermissionRight.Create, entitySpecificPermissions, entityMetadata);

				return new EntityPermissionResult(true, canRead, canWrite, canCreate, canDelete, canAppend, canAppendTo);
			}
		}

		protected virtual bool TryTestRight(OrganizationServiceContext serviceContext, Entity entity, CrmEntityPermissionRight right, List<ITreeNode<EntityPermission>> entitySpecificPermissions, List<PermissionTest> permissionsTested, EntityMetadata entityMetadata = null, bool readGranted = false, EntityReference regarding = null)
		{
			var granted = false;
			var tested = false;
			bool globalPermission;
			List<Guid> scopedPermissionIds;

			switch (right)
			{
				case CrmEntityPermissionRight.Read:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				case CrmEntityPermissionRight.Write:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				case CrmEntityPermissionRight.Create:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				case CrmEntityPermissionRight.Delete:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				case CrmEntityPermissionRight.Append:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				case CrmEntityPermissionRight.AppendTo:
					globalPermission = entitySpecificPermissions.Any(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global);
					scopedPermissionIds = entitySpecificPermissions.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).Select(e => e.Value.EntityReference.Id).ToList();
					break;
				default:
					throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
			}

			if (!globalPermission && scopedPermissionIds.Any())
			{
				// Check if the exact set of scoped permissions have already been tested then we don't need to redundantly test the right again.
				foreach (var test in permissionsTested.Where(test => !test.GlobalGranted && test.ScopedPermissionIds.Any()))
				{
					tested = !test.ScopedPermissionIds.Except(scopedPermissionIds).Union(scopedPermissionIds.Except(test.ScopedPermissionIds)).Any();
					if (!tested) continue;
					granted = test.Granted;
					break;
				}
			}

			var rightGranted = tested ? globalPermission || granted : right == CrmEntityPermissionRight.Read && readGranted || TestRight(serviceContext, entity, right, entitySpecificPermissions, entityMetadata, regarding);

			if (!tested)
			{
				permissionsTested.Add(new PermissionTest(right, globalPermission, scopedPermissionIds, rightGranted));
			}

			return rightGranted;
		}

		protected class PermissionTest
		{
			public CrmEntityPermissionRight Right { get; private set; }

			public bool GlobalGranted { get; private set; }

			public List<Guid> ScopedPermissionIds { get; private set; }

			public bool Granted { get; private set; }

			public PermissionTest()
			{
				ScopedPermissionIds = new List<Guid>();
			}

			public PermissionTest(CrmEntityPermissionRight right, bool globalGranted, List<Guid> scopedPermissionIds, bool granted)
			{
				Right = right;
				GlobalGranted = globalGranted;
				ScopedPermissionIds = scopedPermissionIds;
				Granted = granted;
			}
		}

		/// <summary>
		/// Test a specific right <see cref="CrmEntityPermissionRight"/>
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="entity"></param>
		/// <param name="right"></param>
		/// <param name="permissions"></param>
		/// <returns>True if right is granted, otherwise false.</returns>
		protected virtual bool TestRight(OrganizationServiceContext serviceContext, Entity entity, CrmEntityPermissionRight right, List<ITreeNode<EntityPermission>> permissions, EntityMetadata entityMetadata = null, EntityReference regarding = null)
		{
			List<ITreeNode<EntityPermission>> globalPermissions;
			List<ITreeNode<EntityPermission>> rightSpecificPermissions;

			switch (right)
			{
				case CrmEntityPermissionRight.Read:
					globalPermissions = permissions.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.Read && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				case CrmEntityPermissionRight.Write:
					globalPermissions = permissions.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.Write && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				case CrmEntityPermissionRight.Create:
					globalPermissions = permissions.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.Create && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				case CrmEntityPermissionRight.Delete:
					globalPermissions = permissions.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.Delete && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				case CrmEntityPermissionRight.Append:
					globalPermissions = permissions.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.Append && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				case CrmEntityPermissionRight.AppendTo:
					globalPermissions = permissions.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global).ToList();
					rightSpecificPermissions = permissions.Where(e => e.Value.AppendTo && e.Value.Scope.GetValueOrDefault() != EntityPermissionScope.Global).ToList();
					break;
				default:
					throw new ApplicationException(string.Format("CrmEntityPermissionRight specified {0} is not supported.", right));
			}

			var globalPermissionGranted = globalPermissions.Any();

			if (globalPermissionGranted)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Global permission granted on entity set '{0}' for '{1}' right. Fetch not altered.", EntityNamePrivacy.GetEntityName(entity.LogicalName), right));

				return true;
			}

			if (!rightSpecificPermissions.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity for '{1}' right. There are no entity permissions for entity type {0} where '{1}' right is true.", EntityNamePrivacy.GetEntityName(entity.LogicalName), right));

				return false;
			}

			if (right == CrmEntityPermissionRight.Create)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on entity set '{0}' for '{1}' right.", EntityNamePrivacy.GetEntityName(entity.LogicalName), right));

				return true;
			}

			var result = TryAssert(serviceContext, right, entity, entityMetadata, regarding: regarding);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application,
				result
					? string.Format("Permission granted on '{0}' entity with id ({1}) for '{2}' right.", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id,
						right)
					: string.Format("Permission denied on '{0}' entity with id ({1}) for '{2}' right.", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id,
						right));

			return result;
		}

		/// <summary>
		/// Determine if we need to assert the association of the relationship during insert/create of the entity type.
		/// </summary>
		/// <param name="entityLogicalName">The logical name of the primary entity being inserted.</param>
		/// <param name="relationshipName">The schema name of the relationship for the lookup association.</param>
		/// <returns>Returns true if there are entity permission rules for the specific entity type and relationship for create privilege.</returns>
		public virtual bool IsAssociateAssertRequiredOnInsert(string entityLogicalName, string relationshipName)
		{
			return IsRelationshipAssociateAssertRequiredOnInsertOrUpdate(CrmEntityPermissionRight.Create, entityLogicalName, relationshipName);
		}

		/// <summary>
		/// Determine if we need to assert the association of the relationship during update/write of the entity type.
		/// </summary>
		/// <param name="entityLogicalName">The logical name of the primary entity being updated.</param>
		/// <param name="relationshipName">The schema name of the relationship for the lookup association.</param>
		/// <returns>Returns true if there are entity permission rules for the specific entity type and relationship for write privilege.</returns>
		public virtual bool IsAssociateAssertRequiredOnUpdate(string entityLogicalName, string relationshipName)
		{
			return IsRelationshipAssociateAssertRequiredOnInsertOrUpdate(CrmEntityPermissionRight.Write, entityLogicalName, relationshipName);
		}

		/// <summary>
		/// Determine if we need to assert the association of the relationship during insert/create or update/write of the entity type.
		/// </summary>
		/// <param name="right">The <see cref="CrmEntityPermissionRight"/> to check for permission rules.</param>
		/// <param name="entityLogicalName">The logical name of the primary entity being inserted or updated.</param>
		/// <param name="relationshipName">The schema name of the relationship for the lookup association.</param>
		/// <returns>Returns true if there are entity permission rules for the specific entity type and relationship for insert or update.</returns>
		protected virtual bool IsRelationshipAssociateAssertRequiredOnInsertOrUpdate(CrmEntityPermissionRight right, string entityLogicalName, string relationshipName)
		{
			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentNullException(paramName: entityLogicalName);
			}

			if (string.IsNullOrWhiteSpace(relationshipName))
			{
				throw new ArgumentNullException(paramName: relationshipName);
			}

			if (!PermissionsExist)
			{
				return false;
			}

			var entitySpecificPermissions = GetEntitySpecificPermissions(entityLogicalName, trimTreeForCurrentUser: false);

			if (!entitySpecificPermissions.Any())
			{
				return false;
			}

			var rightSpecificPermissions = new ITreeNode<EntityPermission>[0];

			switch (right)
			{
				case CrmEntityPermissionRight.Write:
					rightSpecificPermissions = entitySpecificPermissions.Where(e => e.Value.Write).ToArray();
					break;
				case CrmEntityPermissionRight.Create:
					rightSpecificPermissions = entitySpecificPermissions.Where(e => e.Value.Create).ToArray();
					break;
			}

			if (!rightSpecificPermissions.Any())
			{
				return false;
			}

			var applicablePermissions =
				rightSpecificPermissions.Where(
					e =>
						e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Global ||
						(e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Account &&
						 !string.IsNullOrWhiteSpace(e.Value.AccountRelationshipName) &&
						 e.Value.AccountRelationshipName.Equals(relationshipName, StringComparison.InvariantCulture)) ||
						(e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Contact &&
						 !string.IsNullOrWhiteSpace(e.Value.ContactRelationshipName) &&
						 e.Value.ContactRelationshipName.Equals(relationshipName, StringComparison.InvariantCulture)) ||
						(e.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Parent &&
						 !string.IsNullOrWhiteSpace(e.Value.ParentRelationshipName) &&
						 e.Value.ParentRelationshipName.Equals(relationshipName, StringComparison.InvariantCulture))).ToArray();

			return applicablePermissions.Any();
		}

		/// <summary>
		/// Assertion of an Associate Request. The current user must have <see cref="CrmEntityPermissionRight.Append"/> on the target entity and <see cref="CrmEntityPermissionRight.AppendTo"/> on the other entity.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="target"><see cref="EntityReference"/> of the target to associate to.</param>
		/// <param name="relationship"><see cref="Relationship"/> between the target and entity.</param>
		/// <param name="entity"><see cref="EntityReference"/> that is requested to be associated to the target.</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <returns>True if permission is granted, otherwise false.</returns>
		public virtual bool TryAssertAssociation(OrganizationServiceContext serviceContext, EntityReference target, Relationship relationship, EntityReference entity, EntityMetadata entityMetadata = null)
		{
			target.ThrowOnNull("target");
			entity.ThrowOnNull("entity");

			return TryAssertAssociation(serviceContext, Retrieve(serviceContext, target), relationship, Retrieve(serviceContext, entity), entityMetadata);
		}

		/// <summary>
		/// Assertion of an Associate Request. The current user must have <see cref="CrmEntityPermissionRight.Append"/> on the target entity and <see cref="CrmEntityPermissionRight.AppendTo"/> on the other entity.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="target"><see cref="Entity"/> of the target to associate to.</param>
		/// <param name="relationship"><see cref="Relationship"/> between the target and entity.</param>
		/// <param name="entity"><see cref="Entity"/> that is requested to be associated to the target.</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <returns>True if permission is granted, otherwise false.</returns>
		public virtual bool TryAssertAssociation(OrganizationServiceContext serviceContext, Entity target, Relationship relationship, Entity entity, EntityMetadata entityMetadata = null)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var result = AssertAssociation(serviceContext, target, relationship, entity, entityMetadata);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: result={0}", result));

			return result;
		}

		/// <summary>
		/// Assertion of an Associate Request. The current user must have <see cref="CrmEntityPermissionRight.Append"/> on the target entity and <see cref="CrmEntityPermissionRight.AppendTo"/> on the other entity.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="target"><see cref="EntityReference"/> of the target to associated to.</param>
		/// <param name="relationship"><see cref="Relationship"/> between the target and entity</param>
		/// <param name="entity"><see cref="EntityReference"/> that is requested to be associated to the target.</param>
		/// <param name="targetEntityMetadata"><see cref="EntityMetadata"/> for the target entity.</param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/> for the entity associated to target.</param>
		/// <returns>True if permission is granted, otherwise false.</returns>
		protected virtual bool AssertAssociation(OrganizationServiceContext serviceContext, Entity target, Relationship relationship, Entity entity, EntityMetadata targetEntityMetadata = null, EntityMetadata entityMetadata = null)
		{
			relationship.ThrowOnNull("relationship");
			entity.ThrowOnNull("entity");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: SchemaName={0}, PrimaryEntityRole={1}, LogicalName={2}, Id={3}", relationship.SchemaName, relationship.PrimaryEntityRole, EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id));

			var parentTest = TryAssert(serviceContext, CrmEntityPermissionRight.AppendTo, target, targetEntityMetadata);

			if (!parentTest)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity with id ({1}) for '{2}' right.", EntityNamePrivacy.GetEntityName(target.LogicalName), target.Id, CrmEntityPermissionRight.Append));

				return false;
			}

			var entityTest = TryAssert(serviceContext, CrmEntityPermissionRight.Append, entity, entityMetadata);

			if (entityTest)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission granted on '{0}' entity with id ({1}) for '{2}' right.", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id, CrmEntityPermissionRight.AppendTo));

				return true;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission denied on '{0}' entity with id ({1}) for '{2}' right.", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id, CrmEntityPermissionRight.AppendTo));

			return false;
		}

		private static Entity Retrieve(OrganizationServiceContext serviceContext, EntityReference target)
		{
			var request = new RetrieveRequest { Target = target, ColumnSet = new ColumnSet(true) };
			var response = serviceContext.Execute(request) as RetrieveResponse;
			return response == null ? null : response.Entity;
		}

		/// <summary>
		/// Gets the names of the roles the current user belongs to.
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="website"></param>
		/// <returns>Returns a string array of the names of the roles the user belongs to.</returns>
		public static string[] GetRolesForUser(OrganizationServiceContext serviceContext, EntityReference website)
		{
			string[] roles;

			if (HttpContext.Current.Request.IsAuthenticated)
			{
				// Windows Live ID Server decided to return null for an unauthenticated user's name
				// A null username, however, breaks the Roles.GetRolesForUser() because it expects an empty string.
				var currentUsername = (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
					? HttpContext.Current.User.Identity.Name ?? string.Empty
					: string.Empty;
				using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.EntityPermissionProvider, PerformanceMarkerArea.Security, PerformanceMarkerTagName.GetRolesForUser))
				{
					roles = Roles.GetRolesForUser(currentUsername);
				}
			}
			else
			{
				// Anonymous users roles

				const string fetchXmlFormat = @"<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
				  <entity name=""adx_webrole"">
					<attribute name=""adx_webroleid"" />
					<attribute name=""adx_name"" />
					<attribute name=""createdon"" />
					<order attribute=""adx_name"" descending=""false"" />
					<filter type=""and"">
					  <condition attribute=""statecode"" operator=""eq"" value=""0"" />
					  <condition attribute=""adx_websiteid"" operator=""eq"" value=""{0}"" />
					  <condition attribute=""adx_anonymoususersrole"" operator=""eq"" value=""1"" />
					</filter>
				  </entity>
				</fetch>";

				var fetchXml = string.Format(fetchXmlFormat, website.Id);
				var fetch = Fetch.Parse(fetchXml);
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());
				if (response == null || response.EntityCollection == null || response.EntityCollection.Entities == null ||
					!response.EntityCollection.Entities.Any())
				{
					return new string[0];
				}

				roles = response.EntityCollection.Entities.Select(e => e.GetAttributeValue<string>("adx_name")).ToArray();
			}

			return roles;
		}

		/// <summary>
		/// Retrieve the entity metadata for the specified entity type name
		/// </summary>
		/// <param name="serviceContext"><see cref="OrganizationServiceContext"/></param>
		/// <param name="entityLogicalName">Logical name of the entity for which the metadata should be retrieved.</param>
		/// <returns><see cref="EntityMetadata"/></returns>
		protected static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			var retrieveEntityRequest = new RetrieveEntityRequest
			{
				LogicalName = entityLogicalName,
				EntityFilters = EntityFilters.All
			};

			var response = (RetrieveEntityResponse)serviceContext.Execute(retrieveEntityRequest);

			if (response == null)
			{
				throw new ApplicationException(string.Format("RetrieveEntityRequest failed for entity type {0}.", entityLogicalName));
			}

			return response.EntityMetadata;
		}


		/// <summary>
		/// Generate the collection of relationship link details that will be used to produce link-entity expressions.
		/// </summary>
		/// <param name="permissionNode"></param>
		/// <param name="linkDetails"></param>
		/// <exception cref="ApplicationException"></exception>
		protected static void BuildLinkDetails(ITreeNode<EntityPermission> permissionNode, List<LinkDetails> linkDetails)
		{
			if (permissionNode.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Contact)
			{
				if (string.IsNullOrEmpty(permissionNode.Value.ContactRelationshipName))
				{
					throw new ApplicationException(string.Format("Entity Permission {0} {1} Contact Relationship Name field is null.", permissionNode.Value.Name, permissionNode.Value.EntityReference.Id));
				}

				linkDetails.Add(new LinkDetails(permissionNode.Value.EntityName, "contact", permissionNode.Value.ContactRelationshipName, EntityPermissionScope.Contact));
			}
			else if (permissionNode.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Account)
			{
				if (string.IsNullOrEmpty(permissionNode.Value.AccountRelationshipName))
				{
					throw new ApplicationException(string.Format("Entity Permission {0} {1} Contact Relationship Name field is null.", permissionNode.Value.Name, permissionNode.Value.EntityReference.Id));
				}

				linkDetails.Add(new LinkDetails(permissionNode.Value.EntityName, "account", permissionNode.Value.AccountRelationshipName, EntityPermissionScope.Account));
			}
			else if (permissionNode.Value.Scope.GetValueOrDefault() == EntityPermissionScope.Self)
			{
				if (permissionNode.Value.EntityName != "contact")
				{
					throw new ApplicationException(string.Format("Entity Permission {0} {1} Scope value Self is not applicable to entity {2}.", permissionNode.Value.Name, permissionNode.Value.EntityReference.Id, permissionNode.Value.EntityName));
				}

				linkDetails.Add(new LinkDetails("contact", "contact", null, EntityPermissionScope.Self));
			}
			else
			{
				var parentPermission = permissionNode.Parent;

				if (parentPermission.IsRoot)
				{
					return;
				}

				linkDetails.Add(new LinkDetails(permissionNode.Value.EntityName, parentPermission.Value.EntityName, permissionNode.Value.ParentRelationshipName, EntityPermissionScope.Parent));

				BuildLinkDetails(parentPermission, linkDetails);
			}
		}

		/// <summary>
		/// Modify a fetch and add necessary link entity elements and filter conditions to satisfy record level security trimming based on the relationship definitions on entity permission records associated with the current user's roles.
		/// </summary>
		/// <param name="linkDetails"></param>
		/// <param name="fetch"></param>
		/// <param name="serviceContext"></param>
		/// <param name="link"></param>
		/// <param name="filter"></param>
		/// <param name="contact"></param>
		/// <param name="account"></param>
		/// <param name="addCondition"></param>
		/// <param name="linkEntityAliasGenerator"></param>
		protected static void BuildLinksAndFilter(OrganizationServiceContext serviceContext, LinkDetails linkDetails, Fetch fetch, Link link, Filter filter, EntityReference contact, EntityReference account, bool addCondition, LinkEntityAliasGenerator linkEntityAliasGenerator)
		{
			var entity1Metadata = GetEntityMetadata(serviceContext, linkDetails.Entity1Name);
			var entity2Metadata = linkDetails.Entity2Name == linkDetails.Entity1Name ? entity1Metadata : GetEntityMetadata(serviceContext, linkDetails.Entity2Name);
			var linkEntityPrimaryIdAttribute = entity2Metadata.PrimaryIdAttribute;
			var linkEntityName = linkDetails.Entity2Name;
			var alias = linkEntityAliasGenerator.CreateUniqueAlias(linkEntityName);

			Link newLink = null;
			Condition newCondition = null;

			if (linkDetails.Scope.GetValueOrDefault() != EntityPermissionScope.Self)
			{
				var relationshipManyToMany = entity1Metadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);
				var relationshipManyToOne = entity1Metadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);
				var relationshipOneToMany = entity1Metadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);


				if (relationshipManyToMany != null)
				{
					var intersectLinkEntityName = relationshipManyToMany.IntersectEntityName;
					string linkTargetFromAttribute;
					string linkTargetToAttribute;
					string linkIntersectFromAttribute;
					string linkIntersectToAttribute;
					if (relationshipManyToMany.Entity1LogicalName == relationshipManyToMany.Entity2LogicalName)
					{
						linkIntersectFromAttribute = relationshipManyToMany.Entity2IntersectAttribute;
						linkIntersectToAttribute = entity1Metadata.PrimaryIdAttribute;
						linkTargetFromAttribute = entity1Metadata.PrimaryIdAttribute;
						linkTargetToAttribute = relationshipManyToMany.Entity1IntersectAttribute;
					}
					else
					{
						linkIntersectFromAttribute =
							linkIntersectToAttribute = relationshipManyToMany.Entity1LogicalName == linkDetails.Entity1Name
								? relationshipManyToMany.Entity1IntersectAttribute
								: relationshipManyToMany.Entity2IntersectAttribute;
						linkTargetFromAttribute =
							linkTargetToAttribute = relationshipManyToMany.Entity2LogicalName == linkDetails.Entity2Name
								? relationshipManyToMany.Entity2IntersectAttribute
								: relationshipManyToMany.Entity1IntersectAttribute;
					}

					newLink = new Link
					{
						Name = intersectLinkEntityName,
						FromAttribute = linkIntersectFromAttribute,
						ToAttribute = linkIntersectToAttribute,
						Intersect = true,
						Visible = false,
						Type = JoinOperator.LeftOuter,
						Links = new List<Link>
						{
							new Link
							{
								Name = linkEntityName,
								FromAttribute = linkTargetFromAttribute,
								ToAttribute = linkTargetToAttribute,
								Alias = alias,
								Type = JoinOperator.LeftOuter
							}
						}
					};

					// If we're at the end of a link chain that is Account or Contact scope, we can optimize N:N
					// into just one link, with a filter condition on the intersect entity. We also apply the
					// relevant account or contact filter at this level, to reduce impact of outer joins on top-
					// level filters and distinct.
					if (addCondition)
					{
						if (IsAccountScope(linkDetails, linkEntityName))
						{
							TerminateManyToManyLinkWithTargetFilter(newLink, linkTargetToAttribute, alias, account);

							linkEntityPrimaryIdAttribute = linkTargetToAttribute;
						}
						else if (IsContactScope(linkDetails, linkEntityName))
						{
							TerminateManyToManyLinkWithTargetFilter(newLink, linkTargetToAttribute, alias, contact);

							linkEntityPrimaryIdAttribute = linkTargetToAttribute;
						}
					}
				}
				else if (relationshipManyToOne != null)
				{
					var linkFromAttribute = relationshipManyToOne.ReferencedEntity == linkEntityName
						? relationshipManyToOne.ReferencedAttribute
						: relationshipManyToOne.ReferencingAttribute;

					var linkToAttribute = relationshipManyToOne.ReferencedEntity == linkEntityName
						? relationshipManyToOne.ReferencingAttribute
						: relationshipManyToOne.ReferencedAttribute;

					// If we're at the end of a link chain, we can optimize N:1 into just a condition, sans link.
					if (addCondition)
					{
						newCondition = new Condition
						{
							EntityName = GetDeepestNestedLinkAlias(link),
							Attribute = linkToAttribute
						};

						if (IsAccountScope(linkDetails, linkEntityName))
						{
							newCondition.Operator = ConditionOperator.Equal;
							newCondition.Value = account.Id;
						}
						else if (IsContactScope(linkDetails, linkEntityName))
						{
							newCondition.Operator = ConditionOperator.Equal;
							newCondition.Value = contact.Id;
						}
						else
						{
							newCondition.Operator = ConditionOperator.NotNull;
						}
					}
					else
					{
						newLink = new Link
						{
							Name = linkEntityName,
							FromAttribute = linkFromAttribute,
							ToAttribute = linkToAttribute,
							Type = JoinOperator.LeftOuter,
							Alias = alias
						};
					}
				}
				else if (relationshipOneToMany != null)
				{
					var linkFromAttribute = relationshipOneToMany.ReferencedEntity == linkEntityName
						? relationshipOneToMany.ReferencedAttribute
						: relationshipOneToMany.ReferencingAttribute;

					var linkToAttribute = relationshipOneToMany.ReferencedEntity == linkEntityName
						? relationshipOneToMany.ReferencingAttribute
						: relationshipOneToMany.ReferencedAttribute;

					newLink = new Link
					{
						Name = linkEntityName,
						FromAttribute = linkFromAttribute,
						ToAttribute = linkToAttribute,
						Type = JoinOperator.LeftOuter,
						Alias = alias
					};
				}
				else
				{
					throw new ApplicationException(string.Format("Retrieve relationship request failed for relationship name {0}",
						linkDetails.RelationshipName));
				}

				if (newLink != null)
				{
					AddLink(link, newLink);
				}
			}

			if (addCondition) // Only add the condition if we are at the end of the chain
			{
				if (newCondition == null)
				{
					var condition = new Condition { Attribute = linkEntityPrimaryIdAttribute };

					if (linkDetails.Scope.HasValue && (linkDetails.Scope.Value == EntityPermissionScope.Contact || linkDetails.Scope.Value == EntityPermissionScope.Self))
					{
						if (linkDetails.Scope.Value == EntityPermissionScope.Self && newLink == null)
						{
							if (link.Alias != null)
							{
								condition.EntityName = link.Alias;
							}
						}
						else
						{
							condition.EntityName = alias;
						}
						condition.Operator = ConditionOperator.Equal;
						condition.Value = contact.Id;
					}
					else if (linkDetails.Scope.HasValue && linkDetails.Scope.Value == EntityPermissionScope.Account)
					{
						condition.EntityName = alias;
						condition.Operator = ConditionOperator.Equal;
						condition.Value = account.Id;
					}
					else
					{
						condition.EntityName = alias;
						condition.Operator = ConditionOperator.NotNull;
					}

					filter.Conditions.Add(condition);
				}
				else
				{
					filter.Conditions.Add(newCondition);
				}
			}

			fetch.Distinct = true;
		}

		private static void AddLink(Link link, Link newLink)
		{
			if (string.IsNullOrEmpty(link.Name))
			{
				link.Name = newLink.Name;
				link.FromAttribute = newLink.FromAttribute;
				link.ToAttribute = newLink.ToAttribute;
				link.Intersect = newLink.Intersect;
				link.Visible = newLink.Visible;
				link.Type = newLink.Type;
				link.Links = newLink.Links;
				link.Alias = newLink.Alias;
				link.Filters = newLink.Filters;
			}
			else if (link.Links == null || !link.Links.Any())
			{
				link.Links = new List<Link> { newLink };
			}
			else
			{
				AddLink(link.Links.First(), newLink);
			}
		}

		private static bool IsAccountScope(LinkDetails linkDetails, string linkEntityName = "account")
		{
			return linkDetails.Scope.HasValue
				&& linkDetails.Scope.Value == EntityPermissionScope.Account
				&& string.Equals(linkEntityName, "account", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsContactScope(LinkDetails linkDetails, string linkEntityName = "contact")
		{
			return linkDetails.Scope.HasValue
				&& linkDetails.Scope.Value == EntityPermissionScope.Contact
				&& string.Equals(linkEntityName, "contact", StringComparison.OrdinalIgnoreCase);
		}

		private static string GetDeepestNestedLinkAlias(Link link)
		{
			if (string.IsNullOrEmpty(link.Name))
			{
				return null;
			}

			while (link.Links != null && link.Links.Any())
			{
				link = link.Links.First();
			}

			return link.Alias;
		}

		private static void TerminateManyToManyLinkWithTargetFilter(Link link, string linkTargetToAttribute, string alias, EntityReference target)
		{
			link.Filters = new[]
			{
				new Filter
				{
					Type = LogicalOperator.And,
					Conditions = new List<Condition>
					{
						new Condition(linkTargetToAttribute, ConditionOperator.Equal, target.Id)
					}
				}
			};

			link.Alias = alias;
			link.Links = null;
			link.IsUnique = true;
		}

		/// <summary>
		/// Generate minimal fetch for a given entity that will be used to test if access is granted for a single entity record.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="record"></param>
		/// <param name="entityMetadata"><see cref="EntityMetadata"/></param>
		/// <returns>Fetch for the entity of the record with a filter equality condition specifying the entity record id.</returns>
		protected static Fetch BuildRecordFetch(OrganizationServiceContext serviceContext, Entity record, EntityMetadata entityMetadata = null)
		{
			if (entityMetadata == null) entityMetadata = GetEntityMetadata(serviceContext, record.LogicalName);
			var fetch = new Fetch { Entity = new FetchEntity() };
			fetch.Entity.Name = record.LogicalName;
			fetch.MappingType = MappingType.Logical;
			fetch.Entity.Attributes = new List<FetchAttribute> { new FetchAttribute(entityMetadata.PrimaryIdAttribute) };
			AddRecordFilter(fetch, record, entityMetadata);
			return fetch;
		}

		private static void AddRecordFilter(Fetch fetch, Entity record, EntityMetadata entityMetadata)
		{
			var attribute = entityMetadata.PrimaryIdAttribute;
			var filter = new Filter
			{
				Type = LogicalOperator.And,
				Filters = new List<Filter>(),
				Conditions = new List<Condition> { new Condition(attribute, ConditionOperator.Equal, record.Id) }
			};

			if (fetch.Entity.Filters == null || !fetch.Entity.Filters.Any())
			{
				fetch.Entity.Filters = new List<Filter>
				{
					filter
				};
			}
			else
			{
				fetch.Entity.Filters.Add(filter);
			}
		}

		private Tuple<EntityReference, EntityReference> GetContactAndAccount()
		{
			if (!HttpContext.Current.Request.IsAuthenticated || Portal.User == null || Portal.User.LogicalName != "contact")
			{
				return new Tuple<EntityReference, EntityReference>(
					new EntityReference("contact", Guid.Empty),
					new EntityReference("account", Guid.Empty));
			}

			var contact = Portal.User.ToEntityReference();
			var account = Portal.User.GetAttributeValue<EntityReference>("parentcustomerid");

			if (account == null || account.LogicalName != "account")
			{
				account = new EntityReference("account", Guid.Empty);
			}

			return new Tuple<EntityReference, EntityReference>(contact, account);
		}
	}
}
