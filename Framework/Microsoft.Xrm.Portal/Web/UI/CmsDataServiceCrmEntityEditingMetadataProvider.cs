/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.UI
{
	public class CmsDataServiceCrmEntityEditingMetadataProvider : ICrmEntityEditingMetadataProvider
	{
		private static readonly Dictionary<string, IEnumerable<Relationship>> _defaultChildAssociationsByEntityName = new Dictionary<string, IEnumerable<Relationship>>
		{
			{ "adx_webpage", new[] { "adx_webpage_webfile".ToRelationship(), "adx_webpage_webpage".ToRelationship(EntityRole.Referenced) } },
		};

		private static readonly List<string> _defaultDeletableEntityNames = new List<string>
		{
			"adx_webfile",
			"adx_weblink",
			"adx_webpage",
		};

		private static readonly List<string> _defaultDependencyEntityNames = new List<string>
		{
			"adx_pagetemplate",
		};

		private static readonly List<string> _defaultFileAttachmentEntityNames = new List<string>
		{
			"adx_webfile", "adx_webpage"
		};

		private static readonly List<string> _defaultUrlEntityNames = new List<string>
		{
			"adx_webfile",
			"adx_webpage",
		};

		private static readonly IDictionary<string, Relationship> _parentalRelationshipsByEntityName = new Dictionary<string, Relationship>
		{
			{ "adx_webfile", "adx_webpage_webfile".ToRelationship() },
			{ "adx_webpage", "adx_webpage_webpage".ToRelationship(EntityRole.Referencing) },
		};

		public CmsDataServiceCrmEntityEditingMetadataProvider()
			: this(
				_defaultDependencyEntityNames,
				_defaultDeletableEntityNames,
				_defaultUrlEntityNames,
				_defaultChildAssociationsByEntityName,
				_defaultFileAttachmentEntityNames)
		{ }

		public CmsDataServiceCrmEntityEditingMetadataProvider(
			IEnumerable<string> dependencyEntityNames,
			IEnumerable<string> deletableEntityNames,
			IEnumerable<string> urlEntityNames,
			IDictionary<string, IEnumerable<Relationship>> childAssociationsByEntityName,
			IEnumerable<string> fileAttachmentEntityNames,
			string portalName = null)
		{
			dependencyEntityNames.ThrowOnNull("dependencyEntityNames");
			deletableEntityNames.ThrowOnNull("deletableEntityNames");
			urlEntityNames.ThrowOnNull("urlEntityNames");
			childAssociationsByEntityName.ThrowOnNull("childAssociationsByEntityName");
			fileAttachmentEntityNames.ThrowOnNull("fileAttachmentEntityNames");

			DependencyEntityNames = dependencyEntityNames;
			DeletableEntityNames = deletableEntityNames;
			UrlEntityNames = urlEntityNames;
			ChildAssociationsByEntityName = childAssociationsByEntityName;
			FileAttachmentEntityNames = fileAttachmentEntityNames;
			PortalName = portalName;
		}

		protected IDictionary<string, IEnumerable<Relationship>> ChildAssociationsByEntityName { get; private set; }

		protected IEnumerable<string> DeletableEntityNames { get; private set; }

		protected IEnumerable<string> DependencyEntityNames { get; private set; }

		protected IEnumerable<string> FileAttachmentEntityNames { get; private set; }

		protected IEnumerable<string> UrlEntityNames { get; private set; }

		protected string PortalName { get; private set; }

		public virtual void AddAttributeMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity, string propertyName, string propertyDisplayName)
		{
			string serviceUri;

			if (TryGetDataServicePropertyUri(control, entity, propertyName, out serviceUri))
			{
				AddServiceReference(control, serviceUri, "xrm-attribute-ref", container, propertyDisplayName);
			}
		}

		public virtual void AddEntityMetadata(string portalName, IEditableCrmEntityControl control, Control container, Entity entity)
		{
			if (control == null || container == null || entity == null)
			{
				return;
			}

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

				return;
			}

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

			Relationship parentalRelationship;

			// Output the URL path of parent entity to the DOM (mostly to be read if the entity is deleted--the user
			// will then be redirected to the parent).
			if (_parentalRelationshipsByEntityName.TryGetValue(crmEntityName, out parentalRelationship))
			{
				var context = PortalCrmConfigurationManager.GetServiceContext(PortalName);
				entity = context.MergeClone(entity);

				var parent = entity.GetRelatedEntity(context, parentalRelationship);

				var dependencyProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName);

				if (dependencyProvider == null)
				{
					throw new InvalidOperationException("Unable to create {0} for current portal configuration.".FormatWith(typeof(IDependencyProvider).FullName));
				}

				var urlProvider = dependencyProvider.GetDependency<IEntityUrlProvider>();

				if (urlProvider == null)
				{
					throw new InvalidOperationException("Unable to create {0} for current portal configuration.".FormatWith(typeof(IEntityUrlProvider).FullName));
				}

				var parentPath = urlProvider.GetApplicationPath(context, parent ?? entity);

				if (parentPath != null)
				{
					AddServiceReference(control, parentPath.AbsolutePath, "xrm-entity-parent-url-ref", container);
				}
			}

			// Output the sitemarkers of the current web page into the DOM.
			if (crmEntityName == "adx_webpage")
			{
				var context = PortalCrmConfigurationManager.GetServiceContext(PortalName);
				entity = context.MergeClone(entity);

				foreach (var siteMarker in entity.GetRelatedEntities(context, "adx_webpage_sitemarker"))
				{
					var siteMarkerRef = new HtmlGenericControl("span");

					siteMarkerRef.Attributes["class"] = "xrm-entity-adx_webpage_sitemarker";
					siteMarkerRef.Attributes["title"] = siteMarker.GetAttributeValue<string>("adx_name");

					container.Controls.Add(siteMarkerRef);
				}

				AddEntitySetSchemaMap(control, "adx_webfile", container);
			}
		}

		public void AddSiteMapNodeMetadata(string portalName, IEditableCrmEntityControl control, Control container, SiteMapNode node)
		{
			if (control == null || container == null || node == null)
			{
				return;
			}

			var entityRef = new HyperLink
			{
				CssClass = "xrm-entity-ref xrm-entity-ref-sitemapchildren",
				NavigateUrl = VirtualPathUtility.ToAbsolute(GetSiteMapChildrenDataServiceUri(control, node)),
				ToolTip = node.Title
			};

			entityRef.Attributes["style"] = "display:none;";

			container.Controls.Add(entityRef);
		}

		protected virtual string GetSiteMapChildrenDataServiceUri(IEditableCrmEntityControl control, SiteMapNode startingNode)
		{
			var serviceBaseUri = string.IsNullOrEmpty(control.CmsServiceBaseUri) ? PortalCrmConfigurationManager.GetCmsServiceBaseUri(PortalName) : control.CmsServiceBaseUri;

			// MSBug #120121: No need to URL encode--encoding is handled by webcontrol rendering layer.
			return "{0}/GetSiteMapChildren?siteMapProvider='{1}'&startingNodeUrl='{2}'&cmsServiceBaseUri='{3}'".FormatWith(serviceBaseUri.TrimEnd('/'), startingNode.Provider.Name, startingNode.Url, serviceBaseUri);
		}

		protected virtual void AddEntityServiceReference(IEditableCrmEntityControl control, Entity entity, string title, Control container)
		{
			string serviceUri;

			if (TryGetDataServiceEntityUri(control, entity, out serviceUri))
			{
				AddServiceReference(control, serviceUri, "xrm-entity-ref", container, title);
			}
		}

		protected virtual void AddEntityAssocationSetServiceReference(string portalName, IEditableCrmEntityControl control, Entity entity, Relationship relationship, Control container)
		{
			AddEntityAssocationSetServiceReference(portalName, control, entity, relationship, container, "xrm-entity-{0}-ref");
		}

		protected virtual void AddEntityAssocationSetServiceReference(string portalName, IEditableCrmEntityControl control, Entity entity, Relationship relationship, Control container, string cssClassFormat)
		{
			string serviceUri;

			if (TryGetDataServiceCrmAssocationSetUri(portalName, control, entity, relationship, out serviceUri))
			{
				AddServiceReference(control, serviceUri, cssClassFormat.FormatWith(relationship.ToSchemaName("_")), container);
			}
		}

		protected virtual void AddEntityAssocationSetServiceReferenceForWebLinkSet(IEditableCrmEntityControl control, Entity entity, Relationship relationship, Control container)
		{
			var serviceBaseUri = string.IsNullOrEmpty(control.CmsServiceBaseUri) ? PortalCrmConfigurationManager.GetCmsServiceBaseUri(PortalName) : control.CmsServiceBaseUri;
			var context = PortalCrmConfigurationManager.GetServiceContext(PortalName);

			var serviceUri = context.GetType().GetCrmEntitySetDataServiceUri("adx_weblink", serviceBaseUri, "adx_weblinksetid", entity.Id);

			AddServiceReference(control, serviceUri, "xrm-entity-{0}-ref".FormatWith(relationship.ToSchemaName("_")), container);
		}

		protected virtual void AddEntitySetServiceReference(IEditableCrmEntityControl control, string crmEntityName, Control container)
		{
			string serviceUri;

			if (TryGetDataServiceCrmEntitySetUri(control, crmEntityName, out serviceUri))
			{
				AddServiceReference(control, serviceUri, "xrm-entity-{0}-ref".FormatWith(crmEntityName), container);
			}
		}

		protected virtual void AddEntitySetSchemaMap(IEditableCrmEntityControl control, string crmEntityName, Control container)
		{
			var schemaMap = new HtmlGenericControl("span")
			{
				InnerText = GetCrmDataContextType().GetCrmEntitySetSchemaMap(crmEntityName)
			};

			schemaMap.Attributes["class"] = "xrm-entity-schema-map";
			schemaMap.Attributes["title"] = crmEntityName;
			schemaMap.Attributes["style"] = "display:none;";

			container.Controls.Add(schemaMap);
		}

		protected virtual void AddServiceReference(IEditableCrmEntityControl control, string serviceUri, string cssClass, Control container)
		{
			AddServiceReference(control, serviceUri, cssClass, container, null);
		}

		protected virtual void AddServiceReference(IEditableCrmEntityControl control, string serviceUri, string cssClass, Control container, string title)
		{
			// MSBug #120121: No need to HTML encode--encoding is handled by webcontrol rendering layer.
			var serviceRef = new HyperLink { CssClass = cssClass, NavigateUrl = VirtualPathUtility.ToAbsolute(serviceUri), ToolTip = title };

			serviceRef.Attributes["style"] = "display:none;";

			container.Controls.Add(serviceRef);
		}

		protected virtual bool TryGetDataServiceEntityUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetDataServiceUri()
				: entity.GetDataServiceUri(control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetDataServicePropertyUri(IEditableCrmEntityControl control, Entity entity, string propertyName, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetDataServicePropertyUri(propertyName)
				: entity.GetDataServicePropertyUri(propertyName, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetDataServiceCrmAssocationSetUri(string portalName, IEditableCrmEntityControl control, Entity entity, Relationship relationship, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetDataServiceCrmAssociationSetUri(portalName, relationship)
				: entity.GetDataServiceCrmAssociationSetUri(portalName, relationship, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetDataServiceCrmEntitySetUri(IEditableCrmEntityControl control, string crmEntityName, out string serviceUri)
		{
			var dataContextType = GetCrmDataContextType();

			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? dataContextType.GetCrmEntitySetDataServiceUri(crmEntityName)
				: dataContextType.GetCrmEntitySetDataServiceUri(crmEntityName, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetCrmEntityDeleteDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetEntityDeleteDataServiceUri()
				: entity.GetEntityDeleteDataServiceUri(control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetCrmEntityDeleteDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
		{
			var dataContextType = GetCrmDataContextType();

			uriTemplate = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? dataContextType.GetCrmEntityDeleteDataServiceUriTemplate(crmEntityName)
				: dataContextType.GetCrmEntityDeleteDataServiceUriTemplate(crmEntityName, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(uriTemplate);
		}

		protected virtual bool TryGetCrmEntityFileAttachmentDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetEntityFileAttachmentDataServiceUri()
				: entity.GetEntityFileAttachmentDataServiceUri(control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetCrmEntityFileAttachmentDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
		{
			var dataContextType = GetCrmDataContextType();

			uriTemplate = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? dataContextType.GetCrmEntityFileAttachmentDataServiceUriTemplate(crmEntityName)
				: dataContextType.GetCrmEntityFileAttachmentDataServiceUriTemplate(crmEntityName, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(uriTemplate);
		}

		protected virtual bool TryGetCrmEntityUrlDataServiceUri(IEditableCrmEntityControl control, Entity entity, out string serviceUri)
		{
			serviceUri = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? entity.GetEntityUrlDataServiceUri()
				: entity.GetEntityUrlDataServiceUri(control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(serviceUri);
		}

		protected virtual bool TryGetCrmEntityUrlDataServiceUriTemplate(IEditableCrmEntityControl control, string crmEntityName, out string uriTemplate)
		{
			var dataContextType = GetCrmDataContextType();

			uriTemplate = string.IsNullOrEmpty(control.CmsServiceBaseUri)
				? dataContextType.GetCrmEntityUrlDataServiceUriTemplate(crmEntityName)
				: dataContextType.GetCrmEntityUrlDataServiceUriTemplate(crmEntityName, control.CmsServiceBaseUri);

			return !string.IsNullOrEmpty(uriTemplate);
		}

		protected virtual Type GetCrmDataContextType()
		{
			try
			{
				return CrmConfigurationManager.GetCrmSection().Contexts.Current.DependencyType ?? typeof(OrganizationServiceContext);
			}
			catch
			{
				return typeof(OrganizationServiceContext);
			}
		}
	}
}
