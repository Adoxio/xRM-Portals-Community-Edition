/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web.Data.Services
{
	public class AdxCmsDataServiceProvider : CmsDataServiceProvider
	{
		private static readonly Dictionary<string, EntitySetRights> _entitySetRightsByLogicalName = new Dictionary<string, EntitySetRights>
		{
			{ "adx_communityforum",  EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_event",           EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_eventschedule",   EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
			{ "adx_publishingstate", EntitySetRights.AllRead },
			{ "adx_shortcut",        EntitySetRights.AllRead | EntitySetRights.WriteAppend | EntitySetRights.WriteMerge | EntitySetRights.WriteReplace },
		};

		public AdxCmsDataServiceProvider(string portalName) : base(portalName) { }

		public override void InitializeService<TDataContext>(IDataServiceConfiguration config)
		{
			base.InitializeService<TDataContext>(config);

			foreach (var entitySet in GetEntitySetsWithRights<TDataContext>(_entitySetRightsByLogicalName))
			{
				config.SetEntitySetAccessRule(entitySet.Key, entitySet.Value);
			}

			config.RegisterKnownType(typeof(ExtendedSiteMapChildInfo));
		}

		public override void DeleteEntity(OrganizationServiceContext context, string entitySet, Guid entityID)
		{
			var entity = GetServiceOperationEntityByID(context, entitySet, entityID);

			AssertCrmEntityChangeAccess(context, entity);

			CrmEntityInactiveInfo inactiveInfo;

			if (CrmEntityInactiveInfo.TryGetInfo(entity.LogicalName, out inactiveInfo))
			{
				context.SetState(inactiveInfo.InactiveState, inactiveInfo.InactiveStatus, entity);

				return;
			}

			if (entity.LogicalName == "adx_communityforum" || entity.LogicalName == "adx_event" || entity.LogicalName == "adx_shortcut")
			{
				context.DeleteObject(entity);

				context.SaveChanges();

				return;
			}

			throw new DataServiceException(403, "This operation cannot be performed entities of type {0}.".FormatWith(entity.LogicalName));
		}

		public override IEnumerable<SiteMapChildInfo> GetSiteMapChildren(OrganizationServiceContext context, string siteMapProvider, string startingNodeUrl, string cmsServiceBaseUri)
		{
			if (string.IsNullOrEmpty(siteMapProvider))
			{
				throw new DataServiceException(400, "siteMapProvider cannot be null or empty.");
			}

			if (startingNodeUrl == null)
			{
				throw new DataServiceException(400, "startingNodeUrl cannot be null.");
			}

			var provider = SiteMap.Providers[siteMapProvider];

			if (provider == null)
			{
				throw new DataServiceException(404, "Site map provider with name {0} not found.".FormatWith(siteMapProvider));
			}

			var startingNode = provider.FindSiteMapNode(startingNodeUrl);

			if (startingNode == null)
			{
				throw new DataServiceException(404, "Starting site map node with URL {0} not found.".FormatWith(startingNodeUrl));
			}

			var entityStartingNode = startingNode as CrmSiteMapNode;

			if (entityStartingNode == null || entityStartingNode.Entity == null)
			{
				return new List<SiteMapChildInfo>();
			}

			var childEntities = GetChildEntities(context, context.MergeClone(entityStartingNode.Entity));

			var validChildEntities = childEntities
				.Select(e => context.MergeClone(e))
				.Where(e => TryAssertCrmEntityRight(context, e, CrmEntityRight.Read))
				.ToList();

			validChildEntities.Sort(new EntitySiteMapDisplayOrderComparer());

			var childInfos = validChildEntities.Select(e =>
			{
				var info = new ExtendedSiteMapChildInfo
				{
					Title = GetEntityTitle(e),
					EntityUri = null,
					HasPermission = TryAssertCrmEntityRight(context, e, CrmEntityRight.Change),
					Id = e.Id,
					LogicalName = e.LogicalName,
					HiddenFromSiteMap = e.Attributes.Contains("adx_hiddenfromsitemap") && e.GetAttributeValue<bool?>("adx_hiddenfromsitemap").GetValueOrDefault(),
				};

				EntitySetInfo entitySetInfo;
				AttributeInfo propertyInfo;

				if (OrganizationServiceContextInfo.TryGet(context, e, out entitySetInfo)
					&& entitySetInfo.Entity.AttributesByLogicalName.TryGetValue("adx_displayorder", out propertyInfo)
						&& propertyInfo.Property.PropertyType == typeof(int?))
				{
					info.DisplayOrder = (int?)propertyInfo.GetValue(e);
					info.DisplayOrderPropertyName = propertyInfo.Property.Name;
				}

				return info;
			});

			return childInfos.ToArray();
		}

		private static IEnumerable<Entity> GetChildEntities(OrganizationServiceContext context, Entity entity)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName == "adx_webpage")
			{
				Func<string, IEnumerable<Entity>> safeGetRelatedEntities = relationshipName =>
				{
					try
					{
						return entity.GetRelatedEntities(context, relationshipName);
					}
					catch
					{
						return new List<Entity>();
					}
				};

				return context.GetChildPages(entity)
					.Union(context.GetChildFiles(entity))
					.Union(context.GetChildShortcuts(entity))
					.Union(safeGetRelatedEntities("adx_webpage_event"))
					.Union(safeGetRelatedEntities("adx_webpage_communityforum"))
					.Union(safeGetRelatedEntities("adx_webpage_survey"));
			}

			return new List<Entity>();
		}

		private static string GetEntityTitle(Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.Attributes.Contains("adx_name"))
			{
				return entity.GetAttributeValue<string>("adx_name");
			}

			if (entity.Attributes.Contains("name"))
			{
				return entity.GetAttributeValue<string>("name");
			}

			return null;
		}

		public override void InterceptChange<TEntity>(OrganizationServiceContext context, TEntity entity, UpdateOperations operations)
		{
			var entityName = entity.LogicalName;

			try
			{
				switch (entityName)
				{
					case "adx_communityforum":
						InterceptForumUpdate(context, entity, operations);
						break;

					case "adx_event":
						InterceptEventUpdate(context, entity, operations);
						break;

					case "adx_eventschedule":
						InterceptEventScheduleUpdate(context, entity, operations);
						break;

					case "adx_shortcut":
						InterceptShortcutUpdate(context, entity, operations);
						break;
				}
			}
			catch (SecurityException)
			{
				throw new DataServiceException(403, "Write permission on entity type {0} is denied.".FormatWith(entity.GetType().FullName));
			}

			base.InterceptChange(context, entity, operations);
		}

		protected virtual void InterceptEventUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new DataServiceException(403, "Events cannot have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new DataServiceException(403, "Events cannot have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_pagetemplateid") == null)
			{
				throw new DataServiceException(403, "Events must have a page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Events must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		private void InterceptEventScheduleUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent event link is being added.
				GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_event");
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new DataServiceException(403, "Event schedules can't have an empty name property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Event schedules must have a publishing state ID.");
			}

			if (entity.GetAttributeValue<DateTime?>("adx_starttime") == null)
			{
				throw new DataServiceException(403, "Event schedules must have a start time value.");
			}

			if (entity.GetAttributeValue<DateTime?>("adx_endtime") == null)
			{
				throw new DataServiceException(403, "Event schedules must have an end time value.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		protected virtual void InterceptForumUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new DataServiceException(403, "Forums can't have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_forumpagetemplateid") == null)
			{
				throw new DataServiceException(403, "Forums must have a forum page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_threadpagetemplateid") == null)
			{
				throw new DataServiceException(403, "Forums must have a thread page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Forums must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		protected override void InterceptWebFileUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			base.InterceptWebFileUpdate(context, entity, operations);

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Web files must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		protected override void InterceptWebLinkUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			base.InterceptWebLinkUpdate(context, entity, operations);

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Web links must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		protected override void InterceptWebPageUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);

				// Make the current user the author of the new web page, if the author ID is not yet set.
				if (entity.GetAttributeValue<EntityReference>("adx_authorid") == null)
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
				throw new DataServiceException(403, "Webpages can't have an empty name property.");
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_partialurl")))
			{
				throw new DataServiceException(403, "Web pages cannot have an empty partial URL property.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_pagetemplateid") == null)
			{
				throw new DataServiceException(403, "Webpages must have a page template ID.");
			}

			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null)
			{
				throw new DataServiceException(403, "Web pages must have a publishing state ID.");
			}

			AssertStateTransitionIsValid(context, entity, operations);
		}

		private void InterceptShortcutUpdate(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			if (operations == UpdateOperations.Add)
			{
				// Ensure parent page link is being added.
				var websiteID = GetWebsiteIDFromParentLinkForEntityInPendingChanges(context, entity, "adx_webpage");

				EnsureAssociationWithWebsite(entity, websiteID);
			}
			else
			{
				AssertCrmEntityChangeAccess(context, entity);
			}

			if (string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_name")))
			{
				throw new DataServiceException(403, "Shortcuts can't have an empty name property.");
			}
		}

		protected void AssertStateTransitionIsValid(OrganizationServiceContext context, Entity entity, UpdateOperations operations)
		{
			// This is not an update of an existing entity (may be Add, or Delete) -- nothing to check.
			if (operations != UpdateOperations.Change)
			{
				return;
			}

			var response = (RetrieveResponse)context.Execute(new RetrieveRequest
			{
				Target = new EntityReference(entity.LogicalName, entity.Id),
				ColumnSet = new ColumnSet("adx_publishingstateid")
			});

			var preUpdateEntity = response.Entity;

			// Publishing state has not changed -- nothing to check.
			if (entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == preUpdateEntity.GetAttributeValue<EntityReference>("adx_publishingstateid"))
			{
				return;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var transitionSecurityProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IPublishingStateTransitionSecurityProvider>();

			transitionSecurityProvider.Assert(
				context,
				portalContext.Website,
				preUpdateEntity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null ? Guid.Empty : preUpdateEntity.GetAttributeValue<EntityReference>("adx_publishingstateid").Id,
				entity.GetAttributeValue<EntityReference>("adx_publishingstateid") == null ? Guid.Empty : entity.GetAttributeValue<EntityReference>("adx_publishingstateid").Id);
		}

		protected string AddWebsitePathToQuerystring(string uri)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var website = portalContext.Website;

			if (website != null)
			{
				var partialUrl = website.GetAttributeValue<string>("adx_partialurl");

				if (!string.IsNullOrWhiteSpace(partialUrl))
				{
					return "{0}{1}websitepath={2}".FormatWith(uri, uri.Contains("?") ? "&" : "?", System.Web.Security.AntiXss.AntiXssEncoder.UrlEncode(partialUrl));
				}
			}

			return uri;
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

			var config = PortalCrmConfigurationManager.GetPortalContextElement(PortalName);
			var attributeMapUsername = config.Parameters["attributeMapUsername"] ?? "adx_identity_username";

			var username = GetCurrentIdentity();

			var findContact = context.RetrieveSingle("contact",
				new[] { "contactid" },
				new[] {
					new Condition("statecode", ConditionOperator.Equal, 0),
					new Condition(attributeMapUsername, ConditionOperator.Equal, username)
				});

			return findContact;
		}

		private void AssertCrmEntityChangeAccess(OrganizationServiceContext context, Entity entity)
		{
			AssertCrmEntityRight(context, entity, CrmEntityRight.Change);
		}
	}
}
