/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// </summary>
	public class CmsEntityRelationshipHandler : CmsEntityHandler
	{
		private static readonly Regex _relationshipSchemaNameRegex = new Regex(@"^(?<schemaName>[^\.]+)(\.(?<entityRole>.+))?$");

		public CmsEntityRelationshipHandler() : this(null, null, null, null, null) { }

		public CmsEntityRelationshipHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id, string relationshipSchemaName) : base(portalName, portalScopeId, entityLogicalName, id)
		{
			RelationshipSchemaName = relationshipSchemaName;
		}

		/// <summary>
		/// The CRM schema name of the relationship being requested.
		/// </summary>
		protected virtual string RelationshipSchemaName { get; private set; }

		protected override void AssertRequestEntitySecurity(IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider security)
		{
			// If the current request entity is the current website, leave security handling for later, once we have info about
			// the relationship being requested.
			if (entity.ToEntityReference().Equals(portal.Website.ToEntityReference()))
			{
				return;
			}

			base.AssertRequestEntitySecurity(portal, serviceContext, entity, security);
		}

		protected virtual void AssertRequestEntitySecurity(IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider security, IWebsiteAccessPermissionProvider websiteAccess, CmsEntityRelationshipInfo relationshipInfo)
		{
			if (!entity.ToEntityReference().Equals(portal.Website.ToEntityReference()))
			{
				return;
			}

			var otherEntity = relationshipInfo.IsCollection
				? relationshipInfo.ReferencingEntity
				: relationshipInfo.ReferencedEntity;

			if (string.Equals(otherEntity, "adx_contentsnippet", StringComparison.OrdinalIgnoreCase))
			{
				if (!websiteAccess.TryAssert(serviceContext, WebsiteRight.ManageContentSnippets))
				{
					throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Manage Content Snippets permission denied.");
				}

				return;
			}

			if (string.Equals(otherEntity, "adx_sitemarker", StringComparison.OrdinalIgnoreCase))
			{
				if (!websiteAccess.TryAssert(serviceContext, WebsiteRight.ManageSiteMarkers))
				{
					throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Manage Site Markers permission denied.");
				}

				return;
			}

			if (string.Equals(otherEntity, "adx_weblinkset", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(otherEntity, "adx_weblink", StringComparison.OrdinalIgnoreCase))
			{
				if (!websiteAccess.TryAssert(serviceContext, WebsiteRight.ManageWebLinkSets))
				{
					throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Manage Web Link Sets permission denied.");
				}

				return;
			}

			base.AssertRequestEntitySecurity(portal, serviceContext, entity, security);
		}

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			var relationshipSchemaName = string.IsNullOrWhiteSpace(RelationshipSchemaName) ? context.Request.Params["relationshipSchemaName"] : RelationshipSchemaName;

			if (string.IsNullOrWhiteSpace(relationshipSchemaName))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity relationship schema name from request.");
			}

			var match = _relationshipSchemaNameRegex.Match(relationshipSchemaName);

			if (!match.Success)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity relationship schema name from request."); 
			}

			var schemaName = match.Groups["schemaName"].Value;

			if (string.IsNullOrWhiteSpace(schemaName))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity relationship schema name from request."); 
			}

			var entityRole = match.Groups["entityRole"].Value;

			EntityRole parsedRole;

			var relationship = new Relationship(schemaName)
			{
				PrimaryEntityRole = Enum.TryParse(entityRole, true, out parsedRole) ? new EntityRole?(parsedRole) : null
			};

			CmsEntityRelationshipInfo relationshipInfo;

			if (!entityMetadata.TryGetRelationshipInfo(relationship, out relationshipInfo))
			{
				throw new CmsEntityServiceException(HttpStatusCode.NotFound, "Entity relationship not found.");
			}

			// If the current request entity is the current website, do security handling here, since we skipped it earlier.
			if (entity.ToEntityReference().Equals(portal.Website.ToEntityReference()))
			{
				AssertRequestEntitySecurity(portal, serviceContext, entity, security, CreateWebsiteAccessPermissionProvider(portal), relationshipInfo);
			}

			if (IsRequestMethod(context.Request, "GET"))
			{
				if (relationshipInfo.IsCollection)
				{
					var readableRelatedEntities = entity.GetRelatedEntities(serviceContext, relationship)
						.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read));

					var entityMetadataLookup = new Dictionary<string, CmsEntityMetadata>();

					var entityJsonObjects = readableRelatedEntities.Select(e =>
					{
						CmsEntityMetadata relatedEntityMetadata;

						if (!entityMetadataLookup.TryGetValue(e.LogicalName, out relatedEntityMetadata))
						{
							relatedEntityMetadata = new CmsEntityMetadata(serviceContext, e.LogicalName);

							entityMetadataLookup[e.LogicalName] = relatedEntityMetadata;
						}

						return GetEntityJson(context, serviceProvider, portalScopeId, portal, serviceContext, e, relatedEntityMetadata);
					});

					WriteResponse(context.Response, new JObject
					{
						{ "d", new JArray(entityJsonObjects) }
					});
				}
				else
				{
					var relatedEntity = entity.GetRelatedEntity(serviceContext, relationship);

					if (relatedEntity == null)
					{
						throw new CmsEntityServiceException(HttpStatusCode.NotFound, "Related entity not found.");
					}

					if (!security.TryAssert(serviceContext, relatedEntity, CrmEntityRight.Read))
					{
						throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Related entity access denied.");
					}

					WriteResponse(context.Response, new JObject
					{
						{ "d", GetEntityJson(context, serviceProvider, portalScopeId, portal, serviceContext, relatedEntity, new CmsEntityMetadata(serviceContext, relatedEntity.LogicalName)) }
					});
				}

				return;
			}

			if (IsRequestMethod(context.Request, "POST"))
			{
				if (relationshipInfo.IsCollection)
				{
					OneToManyRelationshipMetadata relationshipMetadata;

					if (!entityMetadata.TryGetOneToManyRelationshipMetadata(relationship, out relationshipMetadata))
					{
						throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the one-to-many relationship metadata for relationship {0} on entity type".FormatWith(relationship.ToSchemaName("."), entity.LogicalName));
					}

					var relatedEntity = CreateEntityOfType(serviceContext, relationshipMetadata.ReferencingEntity);
					var relatedEntityMetadata = new CmsEntityMetadata(serviceContext, relatedEntity.LogicalName);

					var extensions = UpdateEntityFromJsonRequestBody(context.Request, serviceContext, relatedEntity, relatedEntityMetadata);

					var preImage = relatedEntity.Clone(false);

					// Ensure the reference to the target entity is set.
					relatedEntity.SetAttributeValue(relationshipMetadata.ReferencingAttribute, new EntityReference(entity.LogicalName, entity.GetAttributeValue<Guid>(relationshipMetadata.ReferencedAttribute)));

					serviceProvider.InterceptChange(context, portal, serviceContext, relatedEntity, relatedEntityMetadata, CmsEntityOperation.Create, preImage);

					serviceContext.AddObject(relatedEntity);

					serviceProvider.InterceptExtensionChange(context, portal, serviceContext, relatedEntity, relatedEntityMetadata, extensions, CmsEntityOperation.Create);

					serviceContext.SaveChanges();

					var refetchedEntity = serviceContext.CreateQuery(relatedEntity.LogicalName)
						.FirstOrDefault(e => e.GetAttributeValue<Guid>(relatedEntityMetadata.PrimaryIdAttribute) == relatedEntity.Id);

					if (refetchedEntity == null)
					{
						throw new CmsEntityServiceException(HttpStatusCode.InternalServerError, "Unable to retrieve the created entity.");
					}

					WriteResponse(context.Response, new JObject
					{
						{ "d", GetEntityJson(context, serviceProvider, portalScopeId, portal, serviceContext, refetchedEntity, relatedEntityMetadata) }
					}, HttpStatusCode.Created);
				}
				else
				{
					OneToManyRelationshipMetadata relationshipMetadata;

					if (!entityMetadata.TryGetManyToOneRelationshipMetadata(relationship, out relationshipMetadata))
					{
						throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to retrieve the many-to-one relationship metadata for relationship {0} on entity type".FormatWith(relationship.ToSchemaName("."), entity.LogicalName));
					}

					var relatedEntity = CreateEntityOfType(serviceContext, relationshipMetadata.ReferencedEntity);
					var relatedEntityMetadata = new CmsEntityMetadata(serviceContext, relatedEntity.LogicalName);

					var extensions = UpdateEntityFromJsonRequestBody(context.Request, serviceContext, relatedEntity, relatedEntityMetadata);

					serviceProvider.InterceptChange(context, portal, serviceContext, relatedEntity, relatedEntityMetadata, CmsEntityOperation.Create);

					serviceContext.AddObject(relatedEntity);
					serviceContext.AddLink(relatedEntity, relationship, entity);

					serviceProvider.InterceptExtensionChange(context, portal, serviceContext, relatedEntity, relatedEntityMetadata, extensions, CmsEntityOperation.Create);

					serviceContext.SaveChanges();

					var refetchedEntity = serviceContext.CreateQuery(relatedEntity.LogicalName)
						.FirstOrDefault(e => e.GetAttributeValue<Guid>(relatedEntityMetadata.PrimaryIdAttribute) == relatedEntity.Id);

					if (refetchedEntity == null)
					{
						throw new CmsEntityServiceException(HttpStatusCode.InternalServerError, "Unable to retrieve the created entity.");
					}

					WriteResponse(context.Response, new JObject
					{
						{ "d", GetEntityJson(context, serviceProvider, portalScopeId, portal, serviceContext, refetchedEntity, relatedEntityMetadata) }
					}, HttpStatusCode.Created);
				}

				return;
			}

			throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
		}

		private static Entity CreateEntityOfType(OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			EntitySetInfo entitySetInfo;

			if (OrganizationServiceContextInfo.TryGet(serviceContext.GetType(), entityLogicalName, out entitySetInfo))
			{
				try
				{
					return (Entity)Activator.CreateInstance(entitySetInfo.Entity.EntityType);
				}
				catch
				{
					return new Entity(entityLogicalName);
				}
			}

			return new Entity(entityLogicalName);
		}
	}
}
