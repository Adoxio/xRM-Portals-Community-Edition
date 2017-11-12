/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Web.Handlers.ElFinder;
using Adxstudio.Xrm.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Encoder = System.Web.Security.AntiXss.AntiXssEncoder;

namespace Adxstudio.Xrm.Web.UI
{
	public class AdxCmsDataServiceCrmEntityEditingMetadataProvider : ICrmEntityEditingMetadataProvider
	{
		private static readonly Dictionary<string, IEnumerable<Relationship>> _childAssociationsByEntityName = new Dictionary<string, IEnumerable<Relationship>>
		{
			{ "adx_event",
				new[]
				{
					"adx_event_eventschedule".ToRelationship()
				}
			},
			{ "adx_webpage",
				new[]
				{
					"adx_webpage_webfile".ToRelationship(),
					"adx_webpage_webpage".ToRelationship(EntityRole.Referenced),
					"adx_webpage_communityforum".ToRelationship(),
					"adx_webpage_event".ToRelationship(),
					"adx_parentwebpage_shortcut".ToRelationship(),
				}
			},
		};

		private static readonly Dictionary<string, IEnumerable<string>> _siteMapChildEntitiesByEntityName = new Dictionary<string, IEnumerable<string>>
		{
			{ "adx_webpage",
				new[]
				{
					"adx_communityforum",
					"adx_event",
					"adx_webfile",
					"adx_webpage",
					"adx_shortcut",
				}
			},
		};

		private static readonly List<string> _deletableEntityNames = new List<string>
		{
			"adx_communityforum",
			"adx_event",
			"adx_webfile",
			"adx_weblink",
			"adx_webpage",
			"adx_shortcut",
		};

		private static readonly List<string> _dependencyEntityNames = new List<string>
		{
			"adx_pagetemplate",
			"adx_publishingstate",
		};

		private static readonly List<string> _fileAttachmentEntityNames = new List<string>
		{
			"adx_webfile"
		};

		private static readonly List<string> _urlEntityNames = new List<string>
		{
			"adx_communityforum",
			"adx_event",
			"adx_webfile",
			"adx_webpage",
		};

		private static readonly IDictionary<string, Relationship> _parentalRelationshipsByEntityName = new Dictionary<string, Relationship>
		{
			{ "adx_communityforum", "adx_webpage_communityforum".ToRelationship() },
			{ "adx_event", "adx_webpage_event".ToRelationship() },
			{ "adx_webfile", "adx_webpage_webfile".ToRelationship() },
			{ "adx_webpage", "adx_webpage_webpage".ToRelationship(EntityRole.Referencing) },
			{ "adx_shortcut", "adx_webpage_shortcut".ToRelationship() },
		};

		public virtual void AddAttributeMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity, string propertyName, string propertyDisplayName)
		{
			GetOverrideProvider(portalName).AddAttributeMetadata(portalName, control, container, entity, propertyName, propertyDisplayName);
		}

		public virtual void AddEntityMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity)
		{
			GetOverrideProvider(portalName).AddEntityMetadata(portalName, control, container, entity);

			AddSiteMapNodeMetadataForEntity(portalName, control, container, entity);
		}

		public virtual void AddSiteMapNodeMetadata(string portalName, IEditableCrmEntityControl control, Control container, SiteMapNode node)
		{
			AddSiteMapNodeMetadata(portalName, control, container, node, true);
		}

		protected virtual void AddSiteMapNodeMetadata(string portalName, IEditableCrmEntityControl control, Control container, SiteMapNode node, bool includeEntityMetadata)
		{
			var overrideProvider = GetOverrideProvider(portalName);
			
			overrideProvider.AddSiteMapNodeMetadata(portalName, control, container, node);

			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null)
			{
				return;
			}

			if (includeEntityMetadata)
			{
				AddEntityMetadata(portalName, control, container, entityNode.Entity);
			}

			IEnumerable<string> siteMapChildEntityNames;

			if (_siteMapChildEntitiesByEntityName.TryGetValue(entityNode.Entity.LogicalName, out siteMapChildEntityNames))
			{
				foreach (var entityName in siteMapChildEntityNames)
				{
					overrideProvider.AddEntityUriTemplateReference(control, container, entityName);

					if (_deletableEntityNames.Contains(entityName))
					{
						overrideProvider.AddEntityDeleteUriTemplateReference(control, container, entityName);
					}
				}
			}
		}

		protected void AddSiteMapNodeMetadataForEntity(string portalName, IEditableCrmEntityControl control, Control container, Entity entity)
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName);

			var metadata = ((RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
			{
				EntityFilters = EntityFilters.Entity, LogicalName = entity.LogicalName
			})).EntityMetadata;

			var refetchedEntity = serviceContext.CreateQuery(entity.LogicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>(metadata.PrimaryIdAttribute) == entity.Id);

			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

			if (refetchedEntity != null)
			{
				var entityPath = urlProvider.GetApplicationPath(serviceContext, refetchedEntity);

				if (entityPath != null && entityPath.AbsolutePath != null)
				{
					var node = SiteMap.Provider.FindSiteMapNode(entityPath.AbsolutePath);

					if (node != null)
					{
						AddCssClass(container, "xrm-editable-sitemapchildren");

						AddSiteMapNodeMetadata(portalName, control, container, node, false);
					}
				}
			}
		}

		private static void AddCssClass(Control control, string cssClass)
		{
			var htmlControl = control as HtmlControl;

			if (htmlControl != null)
			{
				var existingClasses = htmlControl.Attributes["class"];

				htmlControl.Attributes["class"] = string.IsNullOrEmpty(existingClasses)
					? cssClass
					: "{0} {1}".FormatWith(existingClasses, cssClass);

				return;
			}

			var webControl = control as WebControl;

			if (webControl != null)
			{
				webControl.CssClass = string.IsNullOrEmpty(webControl.CssClass)
					? cssClass
					: "{0} {1}".FormatWith(webControl.CssClass, cssClass);

				return;
			}
		}

		private static OverrideProvider GetOverrideProvider(string portalName)
		{
			return new OverrideProvider(_dependencyEntityNames, _deletableEntityNames, _urlEntityNames, _childAssociationsByEntityName, _fileAttachmentEntityNames, portalName);
		}

		internal class OverrideProvider : CmsDataServiceCrmEntityEditingMetadataProvider
		{
			public OverrideProvider(
				IEnumerable<string> dependencyEntityNames,
				IEnumerable<string> deletableEntityNames,
				IEnumerable<string> urlEntityNames,
				IDictionary<string, IEnumerable<Relationship>> childAssociationsByEntityName,
				IEnumerable<string> fileAttachmentEntityNames,
				string portalName)
					: base(dependencyEntityNames, deletableEntityNames, urlEntityNames, childAssociationsByEntityName, fileAttachmentEntityNames, portalName) { }

			public override void AddAttributeMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity, string propertyName, string propertyDisplayName)
			{
				base.AddAttributeMetadata(portalName, control, container, entity, propertyName, propertyDisplayName);

				var portal = PortalContext.Current;

				var elFinderConnectorPath = VirtualPathUtility.ToAbsolute(ElFinderRouteHandler.GetAppRelativePath(portal.Website.Id));

				var elFinderConnectorUrl = new UrlBuilder(elFinderConnectorPath);

				if (portal.Entity != null)
				{
					elFinderConnectorUrl.QueryString.Set("working", new DirectoryContentHash(portal.Entity.ToLanguageContainerEntityReference()).ToString());
				}

				AddServiceReference(control, elFinderConnectorUrl.PathWithQueryString, "xrm-filebrowser-ref", container);
				AddServiceReference(control, ElFinderRouteHandler.DialogPath, "xrm-filebrowser-dialog-ref", container);
			}

			public override void AddEntityMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity)
			{
				if (control == null || container == null || entity == null)
				{
					return;
				}

				var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName);
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext(portalName);

				var metadata = ((RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
				{
					EntityFilters = EntityFilters.Entity,
					LogicalName = entity.LogicalName
				})).EntityMetadata;

				var refetchedEntity = serviceContext.CreateQuery(entity.LogicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>(metadata.PrimaryIdAttribute) == entity.Id);

				if (entity.LogicalName == "adx_weblinkset")
				{
					// Output the service reference for the web link set itself.
					AddEntityServiceReference(control, entity, entity.GetAttributeValue<string>("adx_name"), container);

					// Output the service reference for the child web links of the set.
					AddEntityAssocationSetServiceReferenceForWebLinkSet(control, entity, "adx_weblinkset_weblink".ToRelationship(), container);
					AddEntityAssocationSetServiceReference(portalName, control, entity, "adx_weblinkset_weblink".ToRelationship(), container, "xrm-entity-{0}-update-ref");
					AddEntitySetSchemaMap(control, "adx_weblink", container);

					// Output the service reference and schema map for site web pages (required to create new web links).
					AddEntitySetServiceReference(control, "adx_webpage", container);
					AddEntitySetSchemaMap(control, "adx_webpage", container);

					string weblinkDeleteUriTemplate;

					if (TryGetCrmEntityDeleteDataServiceUriTemplate(control, "adx_weblink", out weblinkDeleteUriTemplate))
					{
						AddServiceReference(control, weblinkDeleteUriTemplate, "xrm-uri-template xrm-entity-adx_weblink-delete-ref", container);
					}

					// Output the service reference and schema map for site publishing states (required to create new web links).
					AddEntitySetServiceReference(control, "adx_publishingstate", container);
					AddEntitySetSchemaMap(control, "adx_publishingstate", container);
				}
				else
				{
					string serviceUri;

					if (!TryGetDataServiceEntityUri(control, entity, out serviceUri))
					{
						return;
					}

					// Add the service reference to the bound entity.
					container.Controls.Add(new HyperLink { CssClass = "xrm-entity-ref", NavigateUrl = VirtualPathUtility.ToAbsolute(serviceUri), Text = string.Empty });

					string entityUrlServiceUri;

					// Add the service reference for getting the URL of the bound entity.
					if (TryGetCrmEntityUrlDataServiceUri(control, entity, out entityUrlServiceUri))
					{
						AddServiceReference(control, entityUrlServiceUri, "xrm-entity-url-ref", container, "GetEntityUrl");
					}

					var crmEntityName = entity.LogicalName;

					AddEntitySetSchemaMap(control, crmEntityName, container);

					// If the entity is "deletable", add a service reference for soft-delete of the entity.
					if (DeletableEntityNames.Contains(crmEntityName))
					{
						string deleteServiceUri;

						if (TryGetCrmEntityDeleteDataServiceUri(control, entity, out deleteServiceUri))
						{
							AddServiceReference(control, deleteServiceUri, "xrm-entity-delete-ref", container);
						}
					}

					if (FileAttachmentEntityNames.Contains(crmEntityName))
					{
						string fileAttachmentServiceUri;

						if (TryGetCrmEntityFileAttachmentDataServiceUri(control, entity, out fileAttachmentServiceUri))
						{
							AddServiceReference(control, fileAttachmentServiceUri, "xrm-entity-attachment-ref", container);
						}
					}

					// Add the service references on which the creation of various entities are dependent.
					foreach (var dependencyEntityName in DependencyEntityNames)
					{
						AddEntitySetServiceReference(control, dependencyEntityName, container);
						AddEntitySetSchemaMap(control, dependencyEntityName, container);
					}

					// Add the service reference URI Templates for the notes associated with given entity types.
					foreach (var fileAttachmentEntity in FileAttachmentEntityNames)
					{
						string uriTemplate;

						if (TryGetCrmEntityFileAttachmentDataServiceUriTemplate(control, fileAttachmentEntity, out uriTemplate))
						{
							AddServiceReference(control, uriTemplate, "xrm-uri-template xrm-entity-{0}-attachment-ref".FormatWith(fileAttachmentEntity), container);
						}
					}

					// Add the service reference URI Templates for getting URLs for specific entity types.
					foreach (var urlEntityName in UrlEntityNames)
					{
						string uriTemplate;

						if (TryGetCrmEntityUrlDataServiceUriTemplate(control, urlEntityName, out uriTemplate))
						{
							AddServiceReference(control, uriTemplate, "xrm-uri-template xrm-entity-{0}-url-ref".FormatWith(urlEntityName), container, "GetEntityUrl");
						}
					}

					IEnumerable<Relationship> childAssociations;

					if (ChildAssociationsByEntityName.TryGetValue(crmEntityName, out childAssociations))
					{
						foreach (var childAssociation in childAssociations)
						{
							AddEntityAssocationSetServiceReference(portalName, control, entity, childAssociation, container);
						}
					}

					Relationship parentalRelationship2;

					// Output the URL path of parent entity to the DOM (mostly to be read if the entity is deleted--the user
					// will then be redirected to the parent).
					if (_parentalRelationshipsByEntityName.TryGetValue(crmEntityName, out parentalRelationship2))
					{
						var parent = refetchedEntity.GetRelatedEntity(serviceContext, parentalRelationship2);

						var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>();

						var parentPath = urlProvider.GetApplicationPath(serviceContext, parent ?? refetchedEntity);

						if (parentPath != null)
						{
							AddServiceReference(control, parentPath.AbsolutePath, "xrm-entity-parent-url-ref", container);
						}
					}

					// Output the sitemarkers of the current web page into the DOM.
					if (crmEntityName == "adx_webpage")
					{
						foreach (var siteMarker in refetchedEntity.GetRelatedEntities(serviceContext, "adx_webpage_sitemarker"))
						{
							var siteMarkerRef = new HtmlGenericControl("span");

							siteMarkerRef.Attributes["class"] = "xrm-entity-adx_webpage_sitemarker";
							siteMarkerRef.Attributes["title"] = siteMarker.GetAttributeValue<string>("adx_name");

							container.Controls.Add(siteMarkerRef);
						}

						AddEntitySetSchemaMap(control, "adx_webfile", container);

						EntitySetInfo entitySetInfo;

						if (OrganizationServiceContextInfo.TryGet(GetCrmDataContextType(), "adx_communityforum", out entitySetInfo))
						{
							AddEntitySetSchemaMap(control, "adx_communityforum", container);
							AddEntitySetServiceReference(control, "adx_communityforum", container);
						}

						if (OrganizationServiceContextInfo.TryGet(GetCrmDataContextType(), "adx_event", out entitySetInfo))
						{
							AddEntitySetSchemaMap(control, "adx_event", container);
							AddEntitySetServiceReference(control, "adx_event", container);
						}

						AddEntitySetSchemaMap(control, "adx_shortcut", container);

						AddEntitySetServiceReference(control, "adx_webpage", container);
						AddEntitySetServiceReference(control, "adx_webfile", container);
					
						AddPublishingTransitionSetServiceReference(control, container);
					}

					if (entity.LogicalName == "adx_event")
					{
						AddEntitySetSchemaMap(control, "adx_eventschedule", container);
						AddPicklistMetadata(serviceContext, control, "adx_eventschedule", "adx_recurrence", container);
					}
				}

				var previewPermission = new PreviewPermission(portalContext.ServiceContext, portalContext.Website);

				if (previewPermission.IsPermitted)
				{
					var previewPermittedMetadata = new HtmlGenericControl("span");

					previewPermittedMetadata.Attributes["class"] = "xrm-preview-permitted";
					previewPermittedMetadata.Attributes["style"] = "display:none;";

					container.Controls.Add(previewPermittedMetadata);
				}

				Relationship parentalRelationship;

				// Output the URL path of parent entity to the DOM (mostly to be read if the entity is deleted--the user
				// will then be redirected to the parent).
				if (_parentalRelationshipsByEntityName.TryGetValue(entity.LogicalName, out parentalRelationship))
				{
					var parent = refetchedEntity.GetRelatedEntity(serviceContext, parentalRelationship);

					var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>();

					var parentPath = urlProvider.GetApplicationPath(serviceContext, parent ?? refetchedEntity);

					if (parentPath != null)
					{
						AddServiceReference(control, parentPath.AbsolutePath, "xrm-adx-entity-parent-url-ref", container);
					}
				}
			}

			public void AddEntityUriTemplateReference(IEditableCrmEntityControl control, Control container, string entityName)
			{
				string uriTemplate;

				if (TryGetCrmEntityDataServiceUriTemplate(control, entityName, out uriTemplate))
				{
					AddServiceReference(control, uriTemplate, string.Format("xrm-uri-template xrm-entity-{0}-ref", entityName), container);
				}
			}

			public void AddEntityDeleteUriTemplateReference(IEditableCrmEntityControl control, Control container, string entityName)
			{
				string uriTemplate;

				if (TryGetCrmEntityDeleteDataServiceUriTemplate(control, entityName, out uriTemplate))
				{
					AddServiceReference(control, uriTemplate, string.Format("xrm-uri-template xrm-entity-{0}-delete-ref", entityName), container);
				}
			}

			protected virtual void AddPicklistMetadata(OrganizationServiceContext serviceContext, IEditableCrmEntityControl control, string entityName, string propertyName, Control container)
			{
				Dictionary<int, string> options;

				if (!TryGetPicklistOptions(serviceContext, entityName, propertyName, out options))
				{
					return;
				}

				var json = options.SerializeByJson(new Type[] { });

				var schemaMap = new HtmlGenericControl("span") { InnerText = json };

				schemaMap.Attributes["class"] = "xrm-entity-picklist";
				schemaMap.Attributes["title"] = "{0}.{1}".FormatWith(entityName, propertyName);
				schemaMap.Attributes["style"] = "display:none;";

				container.Controls.Add(schemaMap);
			}

			protected static bool TryGetPicklistOptions(OrganizationServiceContext serviceContext, string entityName, string propertyName, out Dictionary<int, string> options)
			{
				options = null;

				try
				{
					var response  = (RetrieveEntityResponse)serviceContext.Execute(new RetrieveEntityRequest
					{
						LogicalName = entityName, EntityFilters = EntityFilters.Attributes
					});

					var attribute = response.EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == propertyName) as PicklistAttributeMetadata;

					if (attribute == null)
					{
						return false;
					}

					options = attribute.OptionSet.Options
						.Where(o => o.Value.HasValue)
						.ToDictionary(o => o.Value.Value, o => o.Label.GetLocalizedLabelString());

					return true;
				}
				catch
				{
					return false;
				}
			}

			protected override bool TryGetCrmEntityDeleteDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityDeleteDataServiceUri(control, entity, out uri), uri, out serviceUri);
			}

			protected override bool TryGetCrmEntityDeleteDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityDeleteDataServiceUriTemplate(control, crmEntityName, out uri), uri, out uriTemplate);
			}

			protected override bool TryGetCrmEntityFileAttachmentDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityFileAttachmentDataServiceUri(control, entity, out uri), uri, out serviceUri);
			}

			protected override bool TryGetCrmEntityFileAttachmentDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityFileAttachmentDataServiceUriTemplate(control, crmEntityName, out uri), uri, out uriTemplate);
			}

			protected override bool TryGetCrmEntityUrlDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityUrlDataServiceUri(control, entity, out uri), uri, out serviceUri);
			}

			protected override bool TryGetCrmEntityUrlDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetCrmEntityUrlDataServiceUriTemplate(control, crmEntityName, out uri), uri, out uriTemplate);
			}

			protected override bool TryGetDataServiceCrmAssocationSetUri(string portalName, IEditableCrmEntityControl control, Entity entity, Relationship relationship, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetDataServiceCrmAssocationSetUri(portalName, control, entity, relationship, out uri), uri, out serviceUri);
			}

			protected override bool TryGetDataServiceCrmEntitySetUri(IEditableCrmEntityControl control, string crmEntityName, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetDataServiceCrmEntitySetUri(control, crmEntityName, out uri), uri, out serviceUri);
			}

			protected override bool TryGetDataServiceEntityUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetDataServiceEntityUri(control, entity, out uri), uri, out serviceUri);
			}

			protected override bool TryGetDataServicePropertyUri(IEditableCrmEntityControl control, Entity entity, string propertyName, out string serviceUri)
			{
				string uri;
				return TryGetServiceUriWithWebsitePath(base.TryGetDataServicePropertyUri(control, entity, propertyName, out uri), uri, out serviceUri);
			}

			protected virtual bool TryGetCrmEntityWebLinkSetDataServiceUri(IEditableCrmEntityControl control, Entity entity, Relationship relationship, out string serviceUri)
			{
				var serviceBaseUri = string.IsNullOrEmpty(control.CmsServiceBaseUri) ? GetCmsServiceBaseUri(PortalName) : control.CmsServiceBaseUri;
				var context = GetServiceContext(PortalName);

				var uri = GetCrmEntitySetDataServiceUri(context.GetType(), "adx_weblink", serviceBaseUri, "adx_weblinksetid", entity.Id);
				serviceUri = AddWebsitePathToQuerystring(uri);

				return !string.IsNullOrEmpty(serviceUri);
			}

			protected virtual bool TryGetCrmEntityDataServiceUriTemplate(IEditableCrmEntityControl control, string entityName, out string uriTemplate)
			{
				var serviceBaseUri = string.IsNullOrEmpty(control.CmsServiceBaseUri) ? GetCmsServiceBaseUri(PortalName) : control.CmsServiceBaseUri;
				var context = GetServiceContext(PortalName);

				var uri = GetCrmEntityDataServiceUriTemplate(context.GetType(), entityName, serviceBaseUri);
				uriTemplate = AddWebsitePathToQuerystring(uri);

				return !string.IsNullOrEmpty(uriTemplate);
			}

			protected override void AddEntityAssocationSetServiceReferenceForWebLinkSet(IEditableCrmEntityControl control, Entity entity, Relationship relationship, Control container)
			{
				string serviceUri;

				if (TryGetCrmEntityWebLinkSetDataServiceUri(control, entity, relationship, out serviceUri))
				{
					AddServiceReference(control, serviceUri, "xrm-entity-{0}-ref".FormatWith(relationship.ToSchemaName("_")), container);
				}
			}

			private bool TryGetServiceUriWithWebsitePath(bool result, string serviceUri, out string extendedServiceUri)
			{
				extendedServiceUri = result ? AddWebsitePathToQuerystring(serviceUri) : null;

				return result;
			}

			private string AddWebsitePathToQuerystring(string uri)
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var website = portalContext.Website;

				if (website != null)
				{
					var partialUrl = website.GetAttributeValue<string>("adx_partialurl");

					if (!string.IsNullOrWhiteSpace(partialUrl))
					{
						return "{0}{1}websitepath={2}".FormatWith(uri, uri.Contains("?") ? "&" : "?", Encoder.UrlEncode(partialUrl));
					}
				}

				return uri;
			}

			private static string GetCrmEntityDataServiceUriTemplate(Type crmDataContextType, string crmEntityName, string serviceBaseUri)
			{
				if (string.IsNullOrEmpty(serviceBaseUri))
				{
					return null;
				}

				var entitySetInfo = GetEntitySetInfo(crmDataContextType, crmEntityName);

				if (entitySetInfo == null)
				{
					return null;
				}

				return "{0}/{1}(guid'{{Id}}')".FormatWith(serviceBaseUri.TrimEnd('/'), UrlEncode(entitySetInfo.Property.Name));
			}


			protected void AddPublishingTransitionSetServiceReference(IEditableCrmEntityControl control, Control container)
			{
				string serviceUri;

				if (TryGetDataServiceCrmEntitySetUri(control, "adx_publishingstate", out serviceUri))
				{
					serviceUri = serviceUri + "&FromStateID={adx_publishingstateid.Id}";
					AddServiceReference(control, serviceUri, "xrm-entity-edit{0}-ref".FormatWith("adx_publishingstate"), container);
				}

			}

			private static string GetCrmEntitySetDataServiceUri(
				Type crmDataContextType,
				string crmEntityName,
				string serviceBaseUri,
				string foreignKeyAttributeName,
				Guid foreignKeyValue)
			{
				if (string.IsNullOrEmpty(serviceBaseUri))
				{
					return null;
				}

				var entitySetInfo = GetEntitySetInfo(crmDataContextType, crmEntityName);

				if (entitySetInfo == null)
				{
					return null;
				}

				var filter = string.Empty;
				AttributeInfo propertyInfo;

				if (entitySetInfo.Entity.AttributesByLogicalName.TryGetValue(foreignKeyAttributeName, out propertyInfo))
				{
					filter = "?$filter={0}/Id eq guid'{1}'".FormatWith(UrlEncode(propertyInfo.Property.Name), UrlEncode(foreignKeyValue.ToString()));
				}

				return "{0}/{1}{2}".FormatWith(serviceBaseUri.TrimEnd('/'), UrlEncode(entitySetInfo.Property.Name), filter);
			}

			private static EntitySetInfo GetEntitySetInfo(Type crmDataContextType, string crmEntityName)
			{
				EntitySetInfo entitySetInfo;

				OrganizationServiceContextInfo.TryGet(crmDataContextType, crmEntityName, out entitySetInfo);

				return entitySetInfo;
			}

			private static string UrlEncode(string s)
			{
				return Encoder.UrlEncode(s);
			}

			private static string GetCmsServiceBaseUri(string portalName = null)
			{
				var element = PortalCrmConfigurationManager.GetPortalContextElement(portalName);
				return element.CmsServiceBaseUri;
			}

			private static OrganizationServiceContext GetServiceContext(string portalName = null, RequestContext request = null)
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName, request);
				return portal.ServiceContext;
			}

			protected override string GetSiteMapChildrenDataServiceUri(IEditableCrmEntityControl control, SiteMapNode startingNode)
			{
				var serviceBaseUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
					? PortalCrmConfigurationManager.GetPortalContextElement(PortalName).CmsServiceBaseUri
					: control.CmsServiceBaseUri;

				var serviceUri = "{0}/GetSiteMapChildren?siteMapProvider='{1}'&startingNodeUrl='{2}'".FormatWith(
					serviceBaseUri.TrimEnd('/'),
					UrlEncode(startingNode.Provider.Name),
					UrlEncode(startingNode.Url));

				return AddWebsitePathToQuerystring(serviceUri);
			}
		}
	}
}
