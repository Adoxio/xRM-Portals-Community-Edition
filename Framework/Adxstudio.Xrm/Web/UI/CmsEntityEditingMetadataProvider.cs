/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Web.Handlers.ElFinder;
using Adxstudio.Xrm.Web.Mvc.Controllers;
using Adxstudio.Xrm.Web.Routing;
using Adxstudio.Xrm.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI
{
	/// <summary>
	/// Default implementation of <see cref="ICmsEntityEditingMetadataProvider"/>. Supports standard CMS entities.
	/// </summary>
	public class CmsEntityEditingMetadataProvider : ICmsEntityEditingMetadataProvider
	{
		private static readonly Dictionary<string, IEnumerable<Relationship>> ChildRelationshipsByEntityName = new Dictionary<string, IEnumerable<Relationship>>
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
			{ "adx_blog",
				new[]
				{
					"adx_blog_blogpost".ToRelationship(),
				}
			},
			{ "adx_blogpost",
				new[]
				{
					"adx_blogpost_webfile".ToRelationship(),
				}
			}
		};

		private static readonly Dictionary<string, IEnumerable<string>> SiteMapChildEntitiesByEntityName = new Dictionary<string, IEnumerable<string>>
		{
			{ "adx_webpage",
				new[]
				{
					"adx_communityforum",
					"adx_event",
					"adx_webfile",
					"adx_webpage",
					"adx_shortcut",
					"adx_blog",
				}
			},
			{ "adx_blogpost",
				new[]
				{
					"adx_webfile",
				}
			},
			{ "adx_event",
				new[]
				{
					"adx_eventschedule"
				}
			},
		};

		private static readonly List<string> DeletableEntityNames = new List<string>
		{
			"adx_communityforum",
			"adx_communityforumthread",
			"adx_event",
			"adx_eventschedule",
			"adx_webfile",
			"adx_weblink",
			"adx_webpage",
			"adx_shortcut",
			"adx_blog",
			"adx_blogpost",
		};

		private static readonly List<string> DependencyEntityNames = new List<string>
		{
			"adx_pagetemplate",
			"adx_publishingstate",
		};

		private static readonly List<string> FileAttachmentEntityNames = new List<string>
		{
			"adx_webfile"
		};

		private static readonly List<string> FileBrowserDirectoryEntityNames = new List<string>
		{
			"adx_webpage",
			"adx_blog",
			"adx_blogpost"
		};

		private static readonly List<string> UrlEntityNames = new List<string>
		{
			"adx_communityforum",
			"adx_communityforumthread",
			"adx_event",
			"adx_webfile",
			"adx_webpage",
			"adx_blog",
			"adx_blogpost"
		};

		private static readonly IDictionary<string, Relationship> ParentalRelationshipsByEntityName = new Dictionary<string, Relationship>
		{
			{ "adx_communityforum", "adx_webpage_communityforum".ToRelationship() },
			{ "adx_communityforumthread", "adx_communityforum_communityforumthread".ToRelationship() },
			{ "adx_event", "adx_webpage_event".ToRelationship() },
			{ "adx_webfile", "adx_webpage_webfile".ToRelationship() },
			{ "adx_webpage", "adx_webpage_webpage".ToRelationship(EntityRole.Referencing) },
			{ "adx_shortcut", "adx_webpage_shortcut".ToRelationship() },
			{ "adx_blogpost", "adx_blog_blogpost".ToRelationship() }
		};

		public CmsEntityEditingMetadataProvider(string portalName)
		{
			PortalName = portalName;
		}

		protected string PortalName { get; private set; }

		public virtual void AddAttributeMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entity, string attributeLogicalName, string attributeDisplayName, string portalName = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName ?? PortalName);
			var servicePath = CmsEntityAttributeRouteHandler.GetAppRelativePath(portal.Website.Id, entity, attributeLogicalName);

			container.AddLabel(attributeDisplayName);
			container.AddAttribute("data-xrm-base", VirtualPathUtility.ToAbsolute("~/xrm-adx/"));
			container.AddAttribute("data-editable-url", VirtualPathUtility.ToAbsolute(servicePath));
			container.AddAttribute("data-editable-title", attributeDisplayName ?? attributeLogicalName);

			AddEntityTemplateServiceReference(container, portal, entity);
			AddEntityTemplateRenderServiceReference(container, portal, entity);
			AddFileBrowserServiceReference(container, portal, FileBrowserDirectoryEntityNames.Contains(entity.LogicalName));
		}

		public virtual void AddEntityMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entity, string portalName = null, string entityDisplayName = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			AddEntityMetadata(container, entity, portal, true, portalName, entityDisplayName);
		}

		public void AddEntityMetadata(ICmsEntityEditingMetadataContainer container, string entityLogicalName, string portalName = null, string entityDisplayName = null, JObject initialValues = null)
		{
			if (container == null) throw new ArgumentNullException("container");

			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName ?? PortalName);
			var website = portal.Website.ToEntityReference();

			container.AddLabel(entityDisplayName);
			container.AddAttribute("data-xrm-base", VirtualPathUtility.ToAbsolute("~/xrm-adx/"));
			container.AddAttribute("data-logicalname", entityLogicalName);

			AddFileBrowserServiceReference(container, portal, FileBrowserDirectoryEntityNames.Contains(entityLogicalName));
			AddEntityTemplateServiceReference(container, portal, website, entityLogicalName);
			AddEntityTemplateRenderServiceReference(container, portal, website, entityLogicalName);

			if (initialValues != null)
			{
				container.AddAttribute("data-create-initial", initialValues.ToString(Formatting.None));
			}

			if (entityLogicalName == "adx_contentsnippet")
			{
				container.AddAttribute("data-create-url", VirtualPathUtility.ToAbsolute(CmsEntityRelationshipRouteHandler.GetAppRelativePath(website.Id, website, new Relationship("adx_website_contentsnippet"))));
				container.AddAttribute("data-create-attribute", "adx_value");
				container.AddAttribute("data-editable-uritemplate", VirtualPathUtility.ToAbsolute(CmsEntityAttributeRouteHandler.GetAppRelativePathTemplate(website.Id, entityLogicalName, "Id", "adx_value")));
			}
		}

		protected virtual void AddEntityMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entityReference, IPortalContext portal, bool addSiteMapNodeMetadata, string portalName = null, string entityDisplayName = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entityReference == null)
			{
				throw new ArgumentNullException("entityReference");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			container.AddLabel(entityDisplayName ?? entityReference.Name);
			container.AddAttribute("data-xrm-base", VirtualPathUtility.ToAbsolute("~/xrm-adx/"));
			container.AddAttribute("data-logicalname", entityReference.LogicalName);
			container.AddAttribute("data-id", entityReference.Id.ToString());

			AddRouteServiceReferenceAttribute(container, "data-parentoptions", "CmsParent_GetParentOptions", new
			{
				__portalScopeId__ = portal.Website.Id
			});

			container.AddAttribute("data-parentoptions-uritemplate", VirtualPathUtility.ToAbsolute(CmsParentController.GetAppRelativePathTemplate(portal.Website.Id, "LogicalName", "Id")));

			portalName = portalName ?? PortalName;

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName);
			var entity = GetEntity(serviceContext, entityReference);

			AddEntityTemplateServiceReference(container, portal, entityReference);
			AddFileBrowserServiceReference(container, portal, FileBrowserDirectoryEntityNames.Contains(entity.LogicalName));

			if (entityReference.LogicalName == "adx_weblinkset")
			{
				// Output the service reference for the web link set itself.
				AddEntityServiceReference(container, portal, entityReference, entityDisplayName ?? entity.GetAttributeValue<string>("adx_name"));

				// Output the service reference for the child web links of the set.
				AddEntityRelationshipServiceReference(container, portal, entityReference, new Relationship("adx_weblinkset_weblink"));
				AddEntityRelationshipServiceReference(container, portal, entityReference, new Relationship("adx_weblinkset_weblink"), "xrm-entity-{0}-update-ref");

				AddEntityDeleteServiceReferenceTemplate(container, portal, "adx_weblink");

				// Output the service reference and schema map for site web pages (required to create new web links).
				AddEntitySetServiceReference(container, portal, "adx_webpage");

				// Output the service reference and schema map for site publishing states (required to create new web links).
				AddEntitySetServiceReference(container, portal, "adx_publishingstate");

				return;
			}

			var allEntities = GetEntityDictionary(serviceContext);

			AddEntityServiceReference(container, portal, entityReference);
			AddEntityUrlServiceReference(container, portal, entityReference);

			// If the entity is deletable, add a service reference for delete of the entity. 
			if (DeletableEntityNames.Contains(entityReference.LogicalName))
			{
				AddEntityDeleteServiceReference(container, portal, entityReference);
			}

			if (FileAttachmentEntityNames.Contains(entityReference.LogicalName))
			{
				AddEntityFileAttachmentServiceReference(container, portal, entityReference);
			}

			// Add the service references on which the creation of various entities are dependent.
			foreach (var dependencyEntityName in DependencyEntityNames)
			{
				AddEntitySetServiceReference(container, portal, dependencyEntityName);
			}

			// Add the service reference URI Templates for the notes associated with given entity types.
			foreach (var fileAttachmentEntity in FileAttachmentEntityNames)
			{
				AddEntityFileAttachmentServiceReferenceTemplate(container, portal, fileAttachmentEntity);
			}

			// Add the service reference URI Templates for getting URLs for specific entity types.
			foreach (var urlEntityName in UrlEntityNames.Where(entityName => EntityNameExistsInSchema(entityName, allEntities)))
			{
				AddEntityUrlServiceReferenceTemplate(container, portal, urlEntityName);
			}

			IEnumerable<Relationship> childRelationships;

			if (ChildRelationshipsByEntityName.TryGetValue(entityReference.LogicalName, out childRelationships))
			{
				foreach (var relationship in childRelationships.Where(relationship => RelationshipExistsInSchema(relationship, allEntities)))
				{
					AddEntityRelationshipServiceReference(container, portal, entityReference, relationship);
				}
			}

			var previewPermission = new PreviewPermission(portal.ServiceContext, portal.Website);

			if (previewPermission.IsPermitted)
			{
				container.AddPreviewPermittedMetadata();
			}

			Relationship parentalRelationship;

			// Output the URL path of parent entity to the DOM (mostly to be read if the entity is deleted--the user
			// will then be redirected to the parent).
			if (ParentalRelationshipsByEntityName.TryGetValue(entityReference.LogicalName, out parentalRelationship))
			{
				var parent = entity.GetRelatedEntity(serviceContext, parentalRelationship);

				var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

				var parentPath = urlProvider.GetApplicationPath(serviceContext, parent ?? entity);

				if (parentPath != null)
				{
					AddServiceReference(container, parentPath.AbsolutePath, "xrm-adx-entity-parent-url-ref");
				}
			}

			// Output the sitemarkers of the current web page into the DOM.
			if (entityReference.LogicalName == "adx_webpage")
			{
				foreach (var siteMarker in entity.GetRelatedEntities(serviceContext, "adx_webpage_sitemarker"))
				{
					container.AddSiteMarkerMetadata(entityReference.LogicalName, siteMarker.GetAttributeValue<string>("adx_name"));
				}

				if (EntityNameExistsInSchema("adx_communityforum", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_communityforum");
				}

				if (EntityNameExistsInSchema("adx_event", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_event");
				}

				if (EntityNameExistsInSchema("adx_entityform", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_entityform");
				}

				if (EntityNameExistsInSchema("adx_entitylist", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_entitylist");
				}

				if (EntityNameExistsInSchema("adx_webform", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_webform");
				}

				if (EntityNameExistsInSchema("adx_weblinkset", allEntities))
				{
					AddEntitySetServiceReference(container, portal, "adx_weblinkset");
				}

				AddEntitySetServiceReference(container, portal, "adx_webpage");
				AddEntitySetServiceReference(container, portal, "adx_webfile");
				
				AddPublishingStateSetServiceReference(container, portal);

				AddPicklistMetadata(container, serviceContext, "adx_webpage", "adx_feedbackpolicy");

				AddEntityRelationshipServiceReferenceTemplate(container, portal, "adx_webpage", "adx_webpage_navigation_weblinkset".ToRelationship());

				if (entity.GetAttributeValue<EntityReference>("adx_parentpageid") == null && string.Equals(entity.GetAttributeValue<string>("adx_partialurl"), "/", StringComparison.OrdinalIgnoreCase))
				{
					container.AddAttribute("data-root", "true");
				}

				var langContext = HttpContext.Current.GetContextLanguageInfo();

				// For multi language portals, add root webpage id to the dom
				if (langContext.IsCrmMultiLanguageEnabled)
				{
					//add language information
					container.AddAttribute("data-languagename", langContext.ContextLanguage.Name);
					container.AddAttribute("data-languageid", langContext.ContextLanguage.EntityReference.Id.ToString());
					
					var rootPageReference = portal.Entity.GetAttributeValue<EntityReference>("adx_rootwebpageid");
					
					if (rootPageReference != null)
					{
						container.AddAttribute("data-rootwebpageid", rootPageReference.Id.ToString());
					}
					else
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Root page for current content page is null, id ={0}", entityReference.Id));
					}
				}
			}

			if (entityReference.LogicalName == "adx_event")
			{
				AddPicklistMetadata(container, serviceContext, "adx_eventschedule", "adx_recurrence");
				AddPicklistMetadata(container, serviceContext, "adx_eventschedule", "adx_week");
			}

			if (addSiteMapNodeMetadata && SiteMapChildEntitiesByEntityName.ContainsKey(entityReference.LogicalName))
			{
				container.AddCssClass("xrm-editable-sitemapchildren");

				AddEntityChildrenServiceReference(container, portal, entityReference, "xrm-entity-ref-sitemapchildren", GetEntityName(entity));

				AddSiteMapNodeMetadata(container, entityReference, portal, portalName);
			}

			if (entityReference.LogicalName == "adx_blog" || entityReference.LogicalName == "adx_webpage")
			{
				AddPicklistMetadata(container, serviceContext, "adx_blog", "adx_commentpolicy");
			}

			if (entityReference.LogicalName == "adx_blog" || entityReference.LogicalName == "adx_blogpost")
			{
				AddPicklistMetadata(container, serviceContext, "adx_blogpost", "adx_commentpolicy");

				var tags = GetWebsiteTags(portal, serviceContext);

				AddTagMetadata(container, "adx_blogpost", tags);
			}

			if (entityReference.LogicalName == "adx_communityforumthread")
			{
				AddEntitySetServiceReference(container, portal, "adx_forumthreadtype");

				var tags = GetWebsiteTags(portal, serviceContext);

				AddTagMetadata(container, "adx_communityforumthread", tags);
			}

			AddPicklistMetadata(container, serviceContext, "adx_webfile", "adx_contentdisposition");
			AddEntitySetServiceReference(container, portal, "subject");
		}

		public virtual void AddSiteMapNodeMetadata(ICmsEntityEditingMetadataContainer container, SiteMapNode node, string portalName = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (node == null)
			{
				throw new ArgumentNullException("node");
			}

			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null)
			{
				return;
			}

			var entityReference = entityNode.Entity.ToEntityReference();

			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName ?? PortalName);

			AddEntityChildrenServiceReference(container, portal, entityReference, "xrm-entity-ref xrm-entity-ref-sitemapchildren", node.Title);

			AddSiteMapNodeMetadata(container, entityReference, portal, portalName);

			AddEntityMetadata(container, entityReference, portal, false);
		}

		protected virtual void AddSiteMapNodeMetadata(ICmsEntityEditingMetadataContainer container, EntityReference entity, IPortalContext portal, string portalName = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			IEnumerable<string> siteMapChildEntityNames;

			if (!SiteMapChildEntitiesByEntityName.TryGetValue(entity.LogicalName, out siteMapChildEntityNames))
			{
				return;
			}
			
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(portalName ?? PortalName);
			var allEntities = GetEntityDictionary(serviceContext);

			foreach (var entityName in siteMapChildEntityNames.Where(entityName => EntityNameExistsInSchema(entityName, allEntities)))
			{
				AddEntityServiceReferenceTemplate(container, portal, entityName);

				if (DeletableEntityNames.Contains(entityName))
				{
					AddEntityDeleteServiceReferenceTemplate(container, portal, entityName);
				}
			}
		}

		protected virtual void AddEntityChildrenServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string cssClass, string title = null)
		{
			var servicePath = CmsEntityChildrenRouteHandler.GetAppRelativePath(portal.Website.Id, entity);

			AddServiceReference(container, servicePath, cssClass, title);
		}

		protected virtual void AddEntityDeleteServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string title = null)
		{
			var servicePath = CmsEntityDeleteRouteHandler.GetAppRelativePath(portal.Website.Id, entity);

			AddServiceReference(container, servicePath, "xrm-entity-delete-ref", title);
		}

		protected virtual void AddEntityDeleteServiceReferenceTemplate(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, string idTemplateVariableName = "Id")
		{
			var servicePath = CmsEntityDeleteRouteHandler.GetAppRelativePathTemplate(portal.Website.Id, entityLogicalName, idTemplateVariableName);

			AddServiceReference(container, servicePath, "xrm-uri-template xrm-entity-{0}-delete-ref".FormatWith(entityLogicalName));
		}

		protected virtual void AddEntityFileAttachmentServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string title = null)
		{
			var servicePath = CmsEntityFileAttachmentRouteHandler.GetAppRelativePath(portal.Website.Id, entity);

			AddServiceReference(container, servicePath, "xrm-entity-attachment-ref", title);
		}

		protected virtual void AddEntityFileAttachmentServiceReferenceTemplate(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, string idTemplateVariableName = "Id")
		{
			var servicePath = CmsEntityFileAttachmentRouteHandler.GetAppRelativePathTemplate(portal.Website.Id, entityLogicalName, idTemplateVariableName);

			AddServiceReference(container, servicePath, "xrm-uri-template xrm-entity-{0}-attachment-ref".FormatWith(entityLogicalName));
		}

		protected virtual void AddEntityServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string title = null)
		{
			var servicePath = CmsEntityRouteHandler.GetAppRelativePath(portal.Website.Id, entity);

			AddServiceReference(container, servicePath, "xrm-entity-ref", title);
		}

		protected virtual void AddEntityServiceReferenceTemplate(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, string idTemplateVariableName = "Id")
		{
			var servicePath = CmsEntityRouteHandler.GetAppRelativePathTemplate(portal.Website.Id, entityLogicalName, idTemplateVariableName);

			AddServiceReference(container, servicePath, "xrm-uri-template xrm-entity-{0}-ref".FormatWith(entityLogicalName));
		}

		protected virtual void AddEntityRelationshipServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, Relationship relationship, string cssClassFormat = "xrm-entity-{0}-ref")
		{
			var servicePath = CmsEntityRelationshipRouteHandler.GetAppRelativePath(portal.Website.Id, entity, relationship);

			AddServiceReference(container, servicePath, cssClassFormat.FormatWith(relationship.ToSchemaName("_")));
		}

		protected virtual void AddEntityRelationshipServiceReferenceTemplate(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, Relationship relationship, string idTemplateVariableName = "Id", string cssClassFormat = "xrm-entity-{0}-ref")
		{
			var servicePath = CmsEntityRelationshipRouteHandler.GetAppRelativePathTemplate(portal.Website.Id, entityLogicalName, idTemplateVariableName, relationship);

			AddServiceReference(container, servicePath, "xrm-uri-template " + cssClassFormat.FormatWith(relationship.ToSchemaName("_")));
		}

		protected virtual void AddEntitySetServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, string title = null)
		{
			var servicePath = CmsEntitySetRouteHandler.GetAppRelativePath(portal.Website.Id, entityLogicalName);

			AddServiceReference(container, servicePath, "xrm-entity-{0}-ref".FormatWith(entityLogicalName), title);
		}

		protected virtual void AddEntityTemplateServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string context = null)
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				return;
			}

			AddRouteServiceReferenceAttribute(container, "data-cmstemplate-url", "CmsTemplate_GetAll", new
			{
				__portalScopeId__ = portal.Website.Id,
				entityLogicalName = entity.LogicalName,
				id = entity.Id,
				currentSiteMapNodeUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(current.Request.Url.AbsolutePath)),
				context
			});
		}

		protected virtual void AddEntityTemplateRenderServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity, string context = null)
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				return;
			}

			AddRouteServiceReferenceAttribute(container, "data-cmstemplate-render-url", "CmsTemplate_GetLivePreview", new
			{
				__portalScopeId__ = portal.Website.Id,
				entityLogicalName = entity.LogicalName,
				id = entity.Id,
				__currentSiteMapNodeUrl__ = Convert.ToBase64String(Encoding.UTF8.GetBytes(current.Request.Url.AbsolutePath)),
				context
			});
		}

		protected virtual void AddEntityUrlServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, EntityReference entity)
		{
			var servicePath = CmsEntityUrlRouteHandler.GetAppRelativePath(portal.Website.Id, entity);

			AddServiceReference(container, servicePath, "xrm-entity-url-ref", "Url");
		}

		protected virtual void AddEntityUrlServiceReferenceTemplate(ICmsEntityEditingMetadataContainer container, IPortalContext portal, string entityLogicalName, string idTemplateVariableName = "Id")
		{
			var servicePath = CmsEntityUrlRouteHandler.GetAppRelativePathTemplate(portal.Website.Id, entityLogicalName, idTemplateVariableName);

			AddServiceReference(container, servicePath, "xrm-uri-template xrm-entity-{0}-url-ref".FormatWith(entityLogicalName), "Url");
		}

		protected virtual void AddFileBrowserServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal, bool setWorkingDirectory = false)
		{
			var elFinderConnectorPath = VirtualPathUtility.ToAbsolute(ElFinderRouteHandler.GetAppRelativePath(portal.Website.Id));
			var elFinderConnectorUrl = new UrlBuilder(elFinderConnectorPath);

			if (setWorkingDirectory && portal.Entity != null)
			{
				elFinderConnectorUrl.QueryString.Set("working", new DirectoryContentHash(portal.Entity.ToLanguageContainerEntityReference()).ToString());
			}

			container.AddAttribute("data-filebrowser-url", elFinderConnectorUrl.PathWithQueryString);
			container.AddAttribute("data-filebrowser-dialog-url", ElFinderRouteHandler.DialogPath);
		}

		protected virtual void AddPicklistMetadata(ICmsEntityEditingMetadataContainer container, OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			Dictionary<int, string> options;

			if (!TryGetPicklistOptions(serviceContext, entityLogicalName, attributeLogicalName, out options))
			{
				return;
			}

			container.AddPicklistMetadata(entityLogicalName, attributeLogicalName, options);
		}

		protected virtual void AddTagMetadata(ICmsEntityEditingMetadataContainer container, string entityLogicalName, IEnumerable<string> tags)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			container.AddTagMetadata(entityLogicalName, tags);
		}

		protected virtual void AddPublishingStateSetServiceReference(ICmsEntityEditingMetadataContainer container, IPortalContext portal)
		{
			var servicePath = VirtualPathUtility.ToAbsolute(CmsEntitySetRouteHandler.GetAppRelativePath(portal.Website.Id, "adx_publishingstate"));

			servicePath = "{0}{1}FromStateID={{adx_publishingstateid.Id}}".FormatWith(servicePath, servicePath.Contains("?") ? "&" : "?");

			AddServiceReference(container, servicePath, "xrm-uri-template xrm-entity-editadx_publishingstate-ref");
		}

		protected virtual void AddServiceReference(ICmsEntityEditingMetadataContainer container, string servicePath, string cssClass, string title = null)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			container.AddServiceReference(servicePath, cssClass, title);
		}

		protected virtual IEnumerable<string> GetWebsiteTags(IPortalContext portal, OrganizationServiceContext serviceContext)
		{
			return serviceContext.CreateQuery("adx_tag")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == portal.Website.ToEntityReference())
				.Select(e => new { Tag = e.GetAttributeValue<string>("adx_name") })
				.ToArray()
				.Select(t => t.Tag);
		}

		private static void AddRouteServiceReferenceAttribute(ICmsEntityEditingMetadataContainer container, string name, string routeName, object routeValues)
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				return;
			}

			var http = new HttpContextWrapper(current);
			var urlHelper = new UrlHelper(new RequestContext(http, RouteTable.Routes.GetRouteData(http) ?? new RouteData()), RouteTable.Routes);

			try
			{
				var url = urlHelper.RouteUrl(routeName, routeValues);

				if (string.IsNullOrEmpty(url))
				{
					return;
				}

				container.AddAttribute(name, url);
			}
			// If the route isn't found, just silently fail to add the data attribute.
			catch (ArgumentException) { }
		}

		private static bool EntityNameExistsInSchema(string entityLogicalName, IDictionary<string, EntityMetadata> allEntities)
		{
			return allEntities.ContainsKey(entityLogicalName);
		}

		/// <summary>
		/// If the relationship is associated with one of the special expansion entities, then it will test whether or not the known entity is in the schema; otherwise, true is returned.
		/// </summary>
		private static bool RelationshipExistsInSchema(Relationship relationship, IDictionary<string, EntityMetadata> allEntities)
		{
			if (relationship.SchemaName == "adx_webpage_communityforum")
			{
				return EntityNameExistsInSchema("adx_communityforum", allEntities);
			}

			if (relationship.SchemaName == "adx_webpage_event")
			{
				return EntityNameExistsInSchema("adx_event", allEntities);
			}

			return true;
		}

		protected static bool TryGetPicklistOptions(OrganizationServiceContext serviceContext, string entityLogicalName, string attributeLogicalName, out Dictionary<int, string> options)
		{
			options = null;

			try
			{
				var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
				entityFilter.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityLogicalName));

				var attributeFilter = new MetadataFilterExpression(LogicalOperator.And);
				attributeFilter.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, attributeLogicalName));

				var response = (RetrieveMetadataChangesResponse)serviceContext.Execute(new RetrieveMetadataChangesRequest
				{
					Query = new EntityQueryExpression
					{
						Criteria = entityFilter,
						Properties = new MetadataPropertiesExpression("LogicalName", "Attributes"),
						AttributeQuery = new AttributeQueryExpression
						{
							Criteria = attributeFilter,
							Properties = new MetadataPropertiesExpression("LogicalName", "OptionSet")
						}
					}
				});

				var entity = response.EntityMetadata.FirstOrDefault(e => e.LogicalName == entityLogicalName);

				if (entity == null)
				{
					return false;
				}

				var attribute = entity.Attributes.FirstOrDefault(a => a.LogicalName == attributeLogicalName) as PicklistAttributeMetadata;

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

		private static string GetEntityName(Entity entity)
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

		private static Entity GetEntity(OrganizationServiceContext serviceContext, EntityReference entity)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var primaryIdAttribute = GetPrimaryIdAttributeLogicalName(serviceContext, entity.LogicalName);

			return serviceContext.CreateQuery(entity.LogicalName)
				.FirstOrDefault(e => e.GetAttributeValue<Guid>(primaryIdAttribute) == entity.Id);
		}

		private static IDictionary<string, EntityMetadata> GetEntityDictionary(OrganizationServiceContext serviceContext)
		{
			var response = (RetrieveMetadataChangesResponse)serviceContext.Execute(new RetrieveMetadataChangesRequest
			{
				Query = new EntityQueryExpression
				{
					Properties = new MetadataPropertiesExpression("LogicalName"),
				}
			});

			return response.EntityMetadata.ToDictionary(e => e.LogicalName, StringComparer.OrdinalIgnoreCase);
		}

		private static string GetPrimaryIdAttributeLogicalName(OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
			entityFilter.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityLogicalName));

			var response = (RetrieveMetadataChangesResponse)serviceContext.Execute(new RetrieveMetadataChangesRequest
			{
				Query = new EntityQueryExpression
				{
					Criteria = entityFilter,
					Properties = new MetadataPropertiesExpression("LogicalName", "PrimaryIdAttribute")
				}
			});

			var entity = response.EntityMetadata.FirstOrDefault(e => e.LogicalName == entityLogicalName);

			if (entity == null)
			{
                throw new InvalidOperationException(ResourceManager.GetString("PrimaryIdAttribute_For_Entity_Retrieve_Exception".FormatWith(entityLogicalName)));
			}

			return entity.PrimaryIdAttribute;
		}
	}
}
