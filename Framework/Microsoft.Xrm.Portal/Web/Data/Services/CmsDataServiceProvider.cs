/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.ServiceModel;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public class CmsDataServiceProvider : ICmsDataServiceProvider // MSBug #120015: Won't seal, inheritance is used extension point.
	{
		private static readonly Dictionary<string, EntitySetRights> _entitySetRightsByLogicalName = new Dictionary<string, EntitySetRights>
		{
			{ "adx_contentsnippet",  EntitySetRights.AllRead | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_pagetemplate",    EntitySetRights.AllRead },
			{ "adx_webfile",         EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_weblink",         EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_weblinkset",      EntitySetRights.AllRead | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_webpage",         EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
		};

		public string PortalName { get; private set; }

		public CmsDataServiceProvider(string portalName)
		{
			PortalName = portalName;
		}

		public virtual void InitializeService<TDataContext>(IDataServiceConfiguration config) where TDataContext : OrganizationServiceContext
		{
			config.UseVerboseErrors = true;

			foreach (var entitySet in GetEntitySetsWithRights<TDataContext>())
			{
				config.SetEntitySetAccessRule(entitySet.Key, entitySet.Value);
			}
		}

		public virtual void AttachFilesToEntity(OrganizationServiceContext context, string entitySet, Guid entityID, IEnumerable<HttpPostedFile> files)
		{
			var entity = GetServiceOperationEntityByID(context, entitySet, entityID);

			AssertCrmEntityChangeAccess(context, entity);

			var fileAttachmentProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityFileAttachmentProvider>();

			if (fileAttachmentProvider == null)
			{
				throw new DataServiceException(500, "Unable to retrieve configured ICrmEntityFileAttachmentProvider dependency.");
			}

			foreach (var file in files)
			{
				fileAttachmentProvider.AttachFile(context, entity, file);
			}
		}

		/// <summary>
		/// Service operation to soft-delete an entity. ("Soft" in that it may not actually delete the entity,
		/// but may instead change it to a different state, archive it, etc.)
		/// </summary>
		public virtual void DeleteEntity(OrganizationServiceContext context, string entitySet, Guid entityID)
		{
			var entity = GetServiceOperationEntityByID(context, entitySet, entityID);

			AssertCrmEntityChangeAccess(context, entity);

			CrmEntityInactiveInfo inactiveInfo;

			if (!CrmEntityInactiveInfo.TryGetInfo(entity.LogicalName, out inactiveInfo))
			{
				throw new DataServiceException(403, "This operation cannot be performed entities of type {0}.".FormatWith(entity.LogicalName));
			}

			context.SetState(inactiveInfo.InactiveState, inactiveInfo.InactiveStatus, entity);
		}

		public virtual string GetEntityUrl(OrganizationServiceContext context, string entitySet, Guid entityID)
		{
			var entity = GetServiceOperationEntityByID(context, entitySet, entityID);

			var url = context.GetUrl(entity);

			if (url == null)
			{
				throw new DataServiceException(404, "URL for entity not found.");
			}

			return url;
		}

		public virtual IEnumerable<SiteMapChildInfo> GetSiteMapChildren(OrganizationServiceContext context, string siteMapProvider, string startingNodeUrl, string cmsServiceBaseUri)
		{
			if (string.IsNullOrEmpty(siteMapProvider))
			{
				throw new DataServiceException(400, "siteMapProvider cannot be null or empty");
			}

			if (startingNodeUrl == null)
			{
				throw new DataServiceException(400, "startingNodeUrl cannot be null");
			}

			var provider = SiteMap.Providers[siteMapProvider];

			if (provider == null)
			{
				throw new DataServiceException(404, @"Site map provider with name ""{0}"" not found.".FormatWith(siteMapProvider));
			}

			var startingNode = provider.FindSiteMapNode(startingNodeUrl);

			if (startingNode == null)
			{
				throw new DataServiceException(404, @"Starting site map node with URL ""{0}"" not found.".FormatWith(startingNodeUrl));
			}

			var childInfos = new List<SiteMapChildInfo>();

			foreach (SiteMapNode childNode in startingNode.ChildNodes)
			{
				var crmNode = childNode as CrmSiteMapNode;

				if (crmNode == null || crmNode.Entity == null)
				{
					continue;
				}

				var entity = context.MergeClone(crmNode.Entity);

				var info = new SiteMapChildInfo
				{
					Title = crmNode.Title,
					EntityUri = string.IsNullOrEmpty(cmsServiceBaseUri) ? entity.GetDataServiceUri() : entity.GetDataServiceUri(cmsServiceBaseUri),
					HasPermission = TryAssertCrmEntityRight(context, entity, CrmEntityRight.Change)
				};

				EntitySetInfo entitySetInfo;
				AttributeInfo propertyInfo;

				if (!OrganizationServiceContextInfo.TryGet(context, entity, out entitySetInfo)
					|| !entitySetInfo.Entity.AttributesByLogicalName.TryGetValue("adx_displayorder", out propertyInfo)
						|| propertyInfo.Property.PropertyType != typeof(int?))
				{
					continue;
				}

				info.DisplayOrder = (int?)propertyInfo.GetValue(entity);
				info.DisplayOrderPropertyName = propertyInfo.Property.Name;

				childInfos.Add(info);
			}

			return childInfos;
		}

		public virtual void InterceptChange<TEntity>(OrganizationServiceContext context, TEntity entity, UpdateOperations operations) where TEntity : Entity
		{
			var entityName = entity.LogicalName;

			try
			{
				switch (entityName)
				{
				case "adx_contentsnippet":
					InterceptContentSnippetUpdate(context, entity, operations);
					break;

				case "adx_webfile":
					InterceptWebFileUpdate(context, entity, operations);
					break;

				case "adx_weblink":
					InterceptWebLinkUpdate(context, entity, operations);
					break;

				case "adx_weblinkset":
					InterceptWebLinkSetUpdate(context, entity, operations);
					break;

				case "adx_webpage":
					InterceptWebPageUpdate(context, entity, operations);
					break;
				default:
					// Let other change interceptors worry about other entity types.
					break;
				}
			}
			catch (SecurityException)
			{
				throw new DataServiceException(403, "Write permission on entity type {0} is denied.".FormatWith(entity.GetType().FullName));
			}
		}

		public virtual Expression<Func<TEntity, bool>> InterceptQuery<TEntity>(OrganizationServiceContext context) where TEntity : Entity
		{
			return entity => TryAssertCrmEntityRight(context, entity, CrmEntityRight.Read);
		}

		protected virtual void AssertCrmEntityRight(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			securityProvider.Assert(context, entity, right);
		}

		protected virtual IEnumerable<KeyValuePair<string, EntitySetRights>> GetEntitySetsWithRights<TDataContext>() where TDataContext : OrganizationServiceContext
		{
			return GetEntitySetsWithRights<TDataContext>(_entitySetRightsByLogicalName);
		}

		protected virtual void InterceptContentSnippetUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			AssertCrmEntityChangeAccess(context, entity);

			SetUpdateTrackingAttributes(entity);
		}

		protected virtual void InterceptWebFileUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);

				SetCreateTrackingAttributes(entity);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			SetUpdateTrackingAttributes(entity);

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new DataServiceException(403, "Web files cannot have an empty partial URL property.");
			}
		}

		protected virtual void InterceptWebLinkUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				EntityReference websiteID;

				if (!TryGetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_weblinkset", out websiteID))
				{
					throw new DataServiceException(403, "Change operation on type {0} requires AddLink to entity of type {1} to be present in pending changes.".FormatWith(entity.GetType().FullName, "adx_weblinkset"));
				}

				SetCreateTrackingAttributes(entity);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			SetUpdateTrackingAttributes(entity);

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new DataServiceException(403, "Web links cannot have an empty name property.");
			}
		}

		protected virtual void InterceptWebLinkSetUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			AssertCrmEntityChangeAccess(context, entity);
		}

		protected virtual void InterceptWebPageUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);

				// Make the current user the author of the new web page, if the author ID is not yet set.
				if (entity.GetAttributeValue<Guid?>("adx_authorid") == null)
				{
					var currentContact = GetUser(context);

					if (currentContact != null)
					{
						entity.SetAttributeValue("adx_authorid", currentContact.ToEntityReference());
					}
				}

				SetCreateTrackingAttributes(entity);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			SetUpdateTrackingAttributes(entity);

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new DataServiceException(403, "Web pages cannot have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new DataServiceException(403, "Web pages cannot have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<Guid?>("adx_pagetemplateid") == null)
			{
				throw new DataServiceException(403, "Web pages must have a page template ID.");
			}
		}

		protected virtual bool TryAssertCrmEntityRight(OrganizationServiceContext context, Entity entity, CrmEntityRight right)
		{
			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			return securityProvider.TryAssert(context, entity, right);
		}

		private void AssertCrmEntityChangeAccess(OrganizationServiceContext context, Entity entity)
		{
			AssertCrmEntityRight(context, entity, CrmEntityRight.Change);
		}

		protected static void EnsureAssociationWithWebsite(Entity entity, EntityReference websiteID)
		{
			// If we're changing an entity, make sure it is associated with the given website.
			if (entity.GetAttributeValue<EntityReference>("adx_websiteid") != websiteID)
			{
				entity.SetAttributeValue("adx_websiteid", websiteID);
			}
		}

		/// <summary>
		/// Gets mappings of <see cref="IQueryable{T}"/> entity set names exposed by TDataContext and
		/// the <see cref="EntitySetRights"/> to assign to those sets at service initialization.
		/// </summary>
		protected static IEnumerable<KeyValuePair<string, EntitySetRights>> GetEntitySetsWithRights<TDataContext>(Dictionary<string, EntitySetRights> entitySetRightsByLogicalName) where TDataContext : OrganizationServiceContext
		{
			return (typeof(TDataContext)).GetEntitySetProperties().Select(property =>
			{
				// Get the first generic type argument of the generic. This is potentially our CRM entity
				// mapping class.
				var genericArgumentType = property.PropertyType.GetGenericArguments().FirstOrDefault();

				if (genericArgumentType == null)
				{
					return null;
				}

				var entityName = genericArgumentType.GetEntityLogicalName();

				EntitySetRights rights;

				// If the properties generic type CRM entity name is not in our dictionary of relevant
				// entities, discard it.
				if (!entitySetRightsByLogicalName.TryGetValue(entityName, out rights))
				{
					return null;
				}

				return new { property.Name, Rights = rights };

			}).Where(set => set != null).Select(set => new KeyValuePair<string, EntitySetRights>(set.Name, set.Rights));
		}

		protected static Entity GetServiceOperationEntityByID(OrganizationServiceContext context, string entitySet, Guid entityID)
		{
			OrganizationServiceContextInfo contextInfo;
			EntitySetInfo entitySetInfo;

			if (!OrganizationServiceContextInfo.TryGet(context.GetType(), out contextInfo)
				|| !contextInfo.EntitySetsByPropertyName.TryGetValue(entitySet, out entitySetInfo))
			{
				throw new DataServiceException(404, @"Entity set ""{0}"" is not exposed by this service.".FormatWith(entitySet));
			}

			var entityType = entitySetInfo.Entity.EntityType;

			if (entityType == null)
			{
				throw new DataServiceException(404, @"Unable to retrieve data type for entity set ""{0}"".".FormatWith(entitySet));
			}

			var entityName = entitySetInfo.Entity.EntityLogicalName.LogicalName;
			var entityPrimaryKeyName = entitySetInfo.Entity.PrimaryKeyProperty.CrmPropertyAttribute.LogicalName;

			var dynamicEntityWrapper = context.CreateQuery(entityName)
				.Where(e => e.GetAttributeValue<Guid>(entityPrimaryKeyName) == entityID)
				.FirstOrDefault();

			if (dynamicEntityWrapper == null)
			{
				throw new DataServiceException(404, @"Entity with ID ""{0}"" not found in entity set ""{1}"".".FormatWith(entityID, entitySet));
			}

			var entity = dynamicEntityWrapper;

			if (entity == null)
			{
				throw new DataServiceException(404, @"Entity with ID ""{0}"" not found in entity set ""{1}"".".FormatWith(entityID, entitySet));
			}

			return entity;
		}

		protected static EntityReference GetWebsiteIDFromParentLinkForEntityInPendingChanges(
			OrganizationServiceContext context,
			Entity entity,
			string sourceEntityName)
		{
			EntityReference websiteID;

			if (!TryGetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, sourceEntityName, out websiteID))
			{
				throw new SecurityException("Change operation on type {0} requires AddLink to entity of type {1} to be present in pending changes.".FormatWith(entity.GetType().FullName, sourceEntityName));
			}

			return websiteID;
		}

		protected static void SetCreateTrackingAttributes(Entity entity)
		{
			entity.SetAttributeValue("adx_createdbyusername", GetCurrentIdentity());
			// entity.SetAttributeValue("adx_createdbyipaddress", HttpContext.Current.Request.UserHostAddress);
		}

		protected static void SetUpdateTrackingAttributes(Entity entity)
		{
			entity.SetAttributeValue("adx_modifiedbyusername", GetCurrentIdentity());
			// entity.SetAttributeValue("adx_modifiedbyipaddress", HttpContext.Current.Request.UserHostAddress);
		}

		protected static bool TryGetWebsiteIDFromParentLinkForEntityInPendingChanges(OrganizationServiceContext context, Entity entity, string sourceEntityName, out EntityReference websiteID)
		{
			return OrganizationServiceContextUtility.TryGetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, sourceEntityName, out websiteID);
		}

		private static string GetCurrentIdentity()
		{
			if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
			{
				return HttpContext.Current.User.Identity.Name;
			}

			if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.PrimaryIdentity != null)
			{
				return ServiceSecurityContext.Current.PrimaryIdentity.Name;
			}

			throw new DataServiceException(403, "Unable to determine identity from current context.");
		}

		private Entity GetUser(OrganizationServiceContext context)
		{
			// retrieve the username attribute from the portal configuration

			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName) as IUserResolutionSettings;
			var attributeMapUsername = (portal != null ? portal.AttributeMapUsername : null) ?? "adx_username";
			var memberEntityName = (portal != null ? portal.MemberEntityName : null) ?? "contact";

			var username = GetCurrentIdentity();

			var findContact =
				from c in context.CreateQuery(memberEntityName)
				where c.GetAttributeValue<string>(attributeMapUsername) == username
				select c;

			return findContact.FirstOrDefault();
		}
	}
}
