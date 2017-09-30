/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.UI;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;
using IDataAdapterDependencies = Adxstudio.Xrm.Cms.IDataAdapterDependencies;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Cms.PortalConfigurationDataAdapterDependencies;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Default implementation of <see cref="IPortalViewContext"/>, using default dependency APIs.
	/// </summary>
	public class PortalViewContext : IPortalViewContext
	{
		private readonly Lazy<SiteMapNode> _currentSiteMapNode;
		private readonly Lazy<SiteMapNode[]> _currentSiteMapNodeAncestors;
		private bool _enableEditing;
		private readonly Lazy<IPortalViewEntity> _entity;
		private readonly IDictionary<string, IPortalViewEntity> _entities = new Dictionary<string, IPortalViewEntity>();
		private readonly Lazy<IPortalViewEntity> _user;
		private readonly Lazy<IPortalViewEntity> _website;
		private readonly Lazy<IWebsiteAccessPermissionProvider> _websiteAccessPermissionProvider;

		public PortalViewContext(SiteMapProvider siteMapProvider = null, string portalName = null, RequestContext requestContext = null)
			: this(new PortalConfigurationDataAdapterDependencies(portalName, requestContext), siteMapProvider, portalName, requestContext) { }

		public PortalViewContext(IDataAdapterDependencies dependencies, SiteMapProvider siteMapProvider = null, string portalName = null, 
            RequestContext requestContext = null)
			: this(
				GetSettingDataAdapter(dependencies),
				GetSiteMarkerDataAdapter(dependencies), 
				GetSnippetDataAdapter(dependencies),
				GetWebLinkSetDataAdapter(dependencies),
				GetAdDataAdapter(dependencies),
				GetPollDataAdapter(dependencies),
				dependencies.GetUrlProvider(),
				siteMapProvider, 
				portalName,
				requestContext) { }

		public PortalViewContext(ISettingDataAdapter settings, ISiteMarkerDataAdapter siteMarkers, ISnippetDataAdapter snippets,
			IWebLinkSetDataAdapter webLinks, IAdDataAdapter ads, IPollDataAdapter polls, IEntityUrlProvider urlProvider, 
			SiteMapProvider siteMapProvider = null, string portalName = null, RequestContext requestContext = null)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			if (siteMarkers == null)
			{
				throw new ArgumentNullException("siteMarkers");
			}

			if (snippets == null)
			{
				throw new ArgumentNullException("snippets");
			}

			if (urlProvider == null)
			{
				throw new ArgumentNullException("urlProvider");
			}

            if (webLinks == null)
            {
                throw new ArgumentNullException("webLinks");
            }

			if (ads == null)
			{
				throw new ArgumentNullException("ads");
			}

			if (polls == null)
			{
				throw new ArgumentNullException("polls");
			}
			
			SiteMapProvider = siteMapProvider ?? (SiteMap.Enabled ? SiteMap.Provider : null);
			PortalName = portalName;
			RequestContext = requestContext;

			Settings = settings;
			SiteMarkers = siteMarkers;
			Snippets = snippets;
			UrlProvider = urlProvider;
			WebLinks = webLinks;

			Ads = ads;
			
			Polls = polls;

			_currentSiteMapNode = new Lazy<SiteMapNode>(GetCurrentSiteMapNode, LazyThreadSafetyMode.None);
			_currentSiteMapNodeAncestors = new Lazy<SiteMapNode[]>(GetCurrentSiteMapNodeAncestors, LazyThreadSafetyMode.None);
			_entity = new Lazy<IPortalViewEntity>(GetEntity, LazyThreadSafetyMode.None);
			_user = new Lazy<IPortalViewEntity>(GetUser, LazyThreadSafetyMode.None);
			_website = new Lazy<IPortalViewEntity>(GetWebsite, LazyThreadSafetyMode.None);
			_websiteAccessPermissionProvider = new Lazy<IWebsiteAccessPermissionProvider>(GetWebsiteAccessPermissionProvider, LazyThreadSafetyMode.None);
		}

		public IPortalViewEntity this[string key]
		{
			get
			{
				IPortalViewEntity entity;

				return _entities.TryGetValue(key, out entity) ? entity : null;
			}
			set { _entities[key] = value; }
		}

		public SiteMapNode CurrentSiteMapNode
		{
			get { return _currentSiteMapNode.Value; }
		}

		public SiteMapNode[] CurrentSiteMapNodeAncestors
		{
			get { return _currentSiteMapNodeAncestors.Value; }
		}

		public bool EnableEditing
		{
			get { return _enableEditing || (_enableEditing = _entities.Any(e => e.Value.Editable)); }
		}

		public IPortalViewEntity Entity
		{
			get { return _entity.Value; }
		}

		public string PortalName { get; private set; }

		public ISettingDataAdapter Settings { get; private set; }

		public SiteMapProvider SiteMapProvider { get; private set; }

		public ISiteMarkerDataAdapter SiteMarkers { get; private set; }

		public ISnippetDataAdapter Snippets { get; private set; }

		public IEntityUrlProvider UrlProvider { get; private set; }

		public IPortalViewEntity User
		{
			get { return _user.Value; }
		}

		public IWebLinkSetDataAdapter WebLinks { get; private set; }

		public IAdDataAdapter Ads { get; private set; }

		public IPollDataAdapter Polls { get; private set; }

		public IPortalViewEntity Website
		{
			get { return _website.Value; }
		}

		public IWebsiteAccessPermissionProvider WebsiteAccessPermissionProvider
		{
			get { return _websiteAccessPermissionProvider.Value; }
		}

		protected RequestContext RequestContext { get; private set; }

		public ICrmEntitySecurityProvider CreateCrmEntitySecurityProvider()
		{
			return PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
		}

		public OrganizationServiceContext CreateServiceContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public IPortalContext CreatePortalContext()
		{
			return PortalCrmConfigurationManager.CreatePortalContext(PortalName, RequestContext);
		}

		public bool IsAncestorSiteMapNode(string url, bool excludeRootNodes = false)
		{
			if (string.IsNullOrWhiteSpace(url) || !(VirtualPathUtility.IsAbsolute(url) || VirtualPathUtility.IsAppRelative(url)))
			{
				return false;
			}

			var currentNodeAncestors = CurrentSiteMapNodeAncestors;

			if (currentNodeAncestors.Length < 1)
			{
				return false;
			}

			var node = FindSiteMapNode(url);

			if (node == null)
			{
				return false;
			}

			var root = currentNodeAncestors.Last();

			foreach (var ancestor in currentNodeAncestors)
			{
				if (ancestor.Equals(node))
				{
					return !(excludeRootNodes && ancestor.Equals(root));
				}
			}

			return false;
		}

		public bool IsAncestorSiteMapNode(SiteMapNode siteMapNode, bool excludeRootNodes = false)
		{
			if (siteMapNode == null)
			{
				return false;
			}

			var entityNode = siteMapNode as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null)
			{
				return IsAncestorSiteMapNode(siteMapNode.Url);
			}

			var currentNodeAncestors = CurrentSiteMapNodeAncestors;

			if (currentNodeAncestors.Length < 1)
			{
				return false;
			}

			var nodeEntityReference = entityNode.Entity.ToEntityReference();

			var root = currentNodeAncestors.Last();

			foreach (var ancestor in currentNodeAncestors.OfType<CrmSiteMapNode>())
			{
				if (ancestor.Entity == null)
				{
					continue;
				}

				if (ancestor.Entity.ToEntityReference().Equals(nodeEntityReference))
				{
					return !(excludeRootNodes && ancestor.Equals(root));
				}
			}

			return false;
		}

		public bool IsAncestorSiteMapNode(EntityReference entityReference, bool excludeRootNodes = false)
		{
			if (entityReference == null)
			{
				return false;
			}

			var currentNodeAncestors = CurrentSiteMapNodeAncestors;

			if (currentNodeAncestors.Length < 1)
			{
				return false;
			}

			var root = currentNodeAncestors.Last();

			foreach (var ancestor in currentNodeAncestors.OfType<CrmSiteMapNode>())
			{
				if (ancestor.Entity == null)
				{
					continue;
				}

				if (ancestor.Entity.ToEntityReference().Equals(entityReference))
				{
					return !(excludeRootNodes && ancestor.Equals(root));
				}
			}

			return false;
		}

		public bool IsCurrentSiteMapNode(string url)
		{
			if (string.IsNullOrWhiteSpace(url) || !(VirtualPathUtility.IsAbsolute(url) || VirtualPathUtility.IsAppRelative(url)))
			{
				return false;
			}

			var currentNode = CurrentSiteMapNode;

			if (currentNode == null)
			{
				return false;
			}

			var node = FindSiteMapNode(url);

			return node != null && currentNode.Equals(node);
		}

		public bool IsCurrentSiteMapNode(IPortalViewEntity entity)
		{
			var currentNode = CurrentSiteMapNode as CrmSiteMapNode;

			return currentNode != null && currentNode.Entity != null && entity.EntityReference.Equals(currentNode.Entity.ToEntityReference());
		}

		public bool IsCurrentSiteMapNode(SiteMapNode siteMapNode)
		{
			if (siteMapNode == null)
			{
				return false;
			}

			var entityNode = siteMapNode as CrmSiteMapNode;
			var currentNode = CurrentSiteMapNode as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null || currentNode == null || currentNode.Entity == null)
			{
				return IsCurrentSiteMapNode(siteMapNode.Url);
			}

			return entityNode.Entity.ToEntityReference().Equals(currentNode.Entity.ToEntityReference());
		}

		public bool IsCurrentSiteMapNode(EntityReference entityReference)
		{
			if (entityReference == null)
			{
				return false;
			}

			var currentNode = CurrentSiteMapNode as CrmSiteMapNode;

			return currentNode != null && currentNode.Entity != null && entityReference.Equals(currentNode.Entity.ToEntityReference());
		}

		public IPortalViewEntity GetEntity(OrganizationServiceContext serviceContext, Entity entity)
		{
			if (entity == null)
			{
				return null;
			}

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>();

			return new PortalViewEntity(serviceContext, entity, securityProvider, urlProvider);
		}

		public void AddEntity(string key, OrganizationServiceContext serviceContext, Entity entity)
		{
			this[key] = GetEntity(serviceContext, entity);
		}

		public void RenderEditingMetadata(IPortalViewAttribute attribute, TagBuilder tag, string description = null)
		{
			var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICmsEntityEditingMetadataProvider>();

			metadataProvider.AddAttributeMetadata(new TagBuilderCmsEntityEditingMetadataContainer(tag), attribute.EntityReference, attribute.LogicalName, description ?? attribute.Description, PortalName);

			_enableEditing = true;
		}

		public void RenderEditingMetadata(IPortalViewEntity entity, TagBuilder tag)
 		{
			var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICmsEntityEditingMetadataProvider>();

			// If the entity is Weblinkset and a display name was given, show that otherwise use entity description (name)
			string displayName = entity is IWebLinkSet && ((IWebLinkSet)entity).DisplayName != null ? ((IWebLinkSet)entity).DisplayName : entity.Description;
            metadataProvider.AddEntityMetadata(new TagBuilderCmsEntityEditingMetadataContainer(tag), entity.EntityReference, PortalName, displayName);

			_enableEditing = true;
		}

		public void RenderEditingMetadata(string entityLogicalName, TagBuilder tag, string description = null, JObject initialValues = null)
		{
			var metadataProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICmsEntityEditingMetadataProvider>();

			metadataProvider.AddEntityMetadata(new TagBuilderCmsEntityEditingMetadataContainer(tag), entityLogicalName, PortalName, description, initialValues);

			_enableEditing = true;
		}

		private readonly IDictionary<string, SiteMapNode> _findSiteMapNodeCache = new Dictionary<string, SiteMapNode>();
		
		private SiteMapNode FindSiteMapNode(string url)
		{
			if (SiteMapProvider == null)
			{
				return null;
			}

			SiteMapNode node;

			if (_findSiteMapNodeCache.TryGetValue(url, out node))
			{
				return node;
			}

			node = SiteMapProvider.FindSiteMapNode(url);

			_findSiteMapNodeCache[url] = node;

			return node;
		}

		private SiteMapNode GetCurrentSiteMapNode()
		{
			return SiteMapProvider == null ? null : SiteMapProvider.CurrentNode;
		}

		private SiteMapNode[] GetCurrentSiteMapNodeAncestors()
		{
			var currentNode = CurrentSiteMapNode;

			if (currentNode == null)
			{
				return new SiteMapNode[] { };
			}

			var path = new List<SiteMapNode>();

			for (var parentNode = currentNode.ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
			{
				path.Add(parentNode);
			}

			return path.ToArray();
		}

		private IPortalViewEntity GetEntity()
		{
			var portalContext = CreatePortalContext();

			return GetEntity(portalContext.ServiceContext, portalContext.Entity);
		}

		private IPortalViewEntity GetUser()
		{
			var portalContext = CreatePortalContext();

			return GetEntity(portalContext.ServiceContext, portalContext.User);
		}

		private IPortalViewEntity GetWebsite()
		{
			var portalContext = CreatePortalContext();

			return GetEntity(portalContext.ServiceContext, portalContext.Website);
		}

		private IWebsiteAccessPermissionProvider GetWebsiteAccessPermissionProvider()
		{
			var portalContext = CreatePortalContext();
			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

			return new RequestCachingWebsiteAccessPermissionProvider(new WebsiteAccessPermissionProvider(portalContext.Website, contentMapProvider), portalContext.Website.ToEntityReference());
		}

		private static ISettingDataAdapter GetSettingDataAdapter(IDataAdapterDependencies dependencies)
		{
			var website = HttpContext.Current.GetWebsite();

			return new RequestCachingSettingDataAdapter(new SettingDataAdapter(dependencies, website), website.Entity.ToEntityReference());
		}

		private static ISiteMarkerDataAdapter GetSiteMarkerDataAdapter(IDataAdapterDependencies dependencies)
		{
			var website = HttpContext.Current.GetWebsite();

			return new RequestCachingSiteMarkerDataAdapter(new SiteMarkerDataAdapter(dependencies), website.Entity.ToEntityReference());
		}

		private static ISnippetDataAdapter GetSnippetDataAdapter(IDataAdapterDependencies dependencies)
		{
			var website = HttpContext.Current.GetWebsite();

			return new RequestCachingSnippetDataAdapter(new SnippetDataAdapter(dependencies), website.Entity.ToEntityReference());
		}

		private static IWebLinkSetDataAdapter GetWebLinkSetDataAdapter(IDataAdapterDependencies dependencies)
		{
			return new WebLinkSetDataAdapter(dependencies);
		}

		private static IAdDataAdapter GetAdDataAdapter(IDataAdapterDependencies dependencies)
		{
			return new AdDataAdapter(dependencies);
		}

		private static IPollDataAdapter GetPollDataAdapter(IDataAdapterDependencies dependencies)
		{
			return new PollDataAdapter(dependencies);
		}
	}
}
