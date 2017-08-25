/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Provides various portal/CMS-centric services to MVC views, to be consumed by view helpers.
	/// </summary>
	public interface IPortalViewContext
	{
		/// <summary>
		/// Gets and sets <see cref="IPortalViewEntity">view entities</see> that are registered with
		/// this context and can be acccessed in view helpers by a string key.
		/// </summary>
		/// <param name="key">A string key that identifies a particular view entity.</param>
		IPortalViewEntity this[string key] { get; set; }

		/// <summary>
		/// Gets the current <see cref="SiteMapNode"/>.
		/// </summary>
		SiteMapNode CurrentSiteMapNode { get; }

		/// <summary>
		/// Gets the ancestors of the current <see cref="SiteMapNode"/>
		/// </summary>
		SiteMapNode[] CurrentSiteMapNodeAncestors { get; }

		/// <summary>
		/// Gets an indication of whether CMS editing features are to be enabled for the current view.
		/// </summary>
		bool EnableEditing { get; }
		
		/// <summary>
		/// The current "context" entity, as determined by the CMS system. For example, the current
		/// Web Page (adx_webpage) entity, determined by the URL of the current HTTP request.
		/// </summary>
		IPortalViewEntity Entity { get; }

		/// <summary>
		/// Gets the name of the microsoft.xrm.portal configuration to be used by this instance.
		/// </summary>
		string PortalName { get; }

		/// <summary>
		/// Access to Site Setting (adx_sitesetting) data for the current portal context.
		/// </summary>
		ISettingDataAdapter Settings { get; }

		/// <summary>
		/// Gets the <see cref="SiteMapProvider"/> for the current portal context.
		/// </summary>
		SiteMapProvider SiteMapProvider { get; }

		/// <summary>
		/// Access to Site Marker (adx_sitemarker) data for the current portal context.
		/// </summary>
		ISiteMarkerDataAdapter SiteMarkers { get; }

		/// <summary>
		/// Access to Content Snippet (adx_contentsnippet) data for the current portal context.
		/// </summary>
		ISnippetDataAdapter Snippets { get; }

		/// <summary>
		/// Gets the <see cref="IEntityUrlProvider"/> for the current portal context.
		/// </summary>
		IEntityUrlProvider UrlProvider { get; }

		/// <summary>
		/// The current context user entity, as determined by the CMS system. In most configurations,
		/// a Contact (contact) record, associated with the current ASP.NET request identity through
		/// the Username (adx_username) attribute.
		/// </summary>
		/// <remarks>
		/// This value may be null -- most commonly when the current request context is anonymous/not
		/// authenticated.
		/// </remarks>
		IPortalViewEntity User { get; }

		/// <summary>
		/// Access to Web Link (adx_weblinkset, adx_weblink) data for the current portal context.
		/// </summary>
		IWebLinkSetDataAdapter WebLinks { get; }

		/// <summary>
		/// Access to Ad data for the current portal context.
		/// </summary>
		IAdDataAdapter Ads { get; }

		/// <summary>
		/// Access to Poll data for the current portal context.
		/// </summary>
		IPollDataAdapter Polls { get; }

		/// <summary>
		/// The current context Website (adx_website) entity, as determined by the CMS system. In most
		/// configurations, determined by the microsoft.xrm.portal/portals configuration associated
		/// with <see cref="PortalName"/>.
		/// </summary>
		IPortalViewEntity Website { get; }

		/// <summary>
		/// Gets the <see cref="IWebsiteAccessPermissionProvider"/> for the current website.
		/// </summary>
		IWebsiteAccessPermissionProvider WebsiteAccessPermissionProvider { get; }

		/// <summary>
		/// Adds a given <see cref="Entity"/> to this context, so that it can later be accessed as a
		/// <see cref="IPortalViewEntity"/> by key in portal view helpers.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="serviceContext"></param>
		/// <param name="entity"></param>
		void AddEntity(string key, OrganizationServiceContext serviceContext, Entity entity);

		/// <summary>
		/// Creates a new <see cref="ICrmEntitySecurityProvider"/> for the current portal context.
		/// </summary>
		ICrmEntitySecurityProvider CreateCrmEntitySecurityProvider();

		/// <summary>
		/// Creates a new <see cref="OrganizationServiceContext"/> for the current portal context.
		/// </summary>
		OrganizationServiceContext CreateServiceContext();

		IPortalContext CreatePortalContext();

		/// <summary>
		/// Determines whether a given URL represents an ancestor node of the current site map node.
		/// </summary>
		bool IsAncestorSiteMapNode(string url, bool excludeRootNodes = false);

		/// <summary>
		/// Determines whether a given <see cref="SiteMapNode"/> represents an ancestor node of the current site map node.
		/// </summary>
		bool IsAncestorSiteMapNode(SiteMapNode siteMapNode, bool excludeRootNodes = false);

		/// <summary>
		/// Determines whether a given <see cref="EntityReference"/> represents an ancestor node of the current site map node.
		/// </summary>
		bool IsAncestorSiteMapNode(EntityReference entityReference, bool excludeRootNodes = false);

		/// <summary>
		/// Determines whether a given URL corresponds to the same entity as the current site map node.
		/// </summary>
		bool IsCurrentSiteMapNode(string url);

		/// <summary>
		/// Determines whether a given <see cref="IPortalViewEntity"/> corresponds to the same entity
		/// as the current site map node.
		/// </summary>
		bool IsCurrentSiteMapNode(IPortalViewEntity entity);

		/// <summary>
		/// Determines whether a given <see cref="SiteMapNode"/> corresponds to the same entity as the current site map node.
		/// </summary>
		bool IsCurrentSiteMapNode(SiteMapNode siteMapNode);

		/// <summary>
		/// Determines whether a given <see cref="EntityReference"/> corresponds to the same entity as the current site map node.
		/// </summary>
		bool IsCurrentSiteMapNode(EntityReference entityReference);

		/// <summary>
		/// Factory method to construct a valid <see cref="IPortalViewEntity"/> for this context.
		/// </summary>
		/// <param name="serviceContext"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		IPortalViewEntity GetEntity(OrganizationServiceContext serviceContext, Entity entity);

		/// <summary>
		/// Renders CMS editing HTML metadata for a given <see cref="IPortalViewAttribute"/>.
		/// </summary>
		/// <param name="attribute">The attribute to render metadata for.</param>
		/// <param name="tag">The HTML container tag into which the metadata will be rendered.</param>
		/// <param name="description">
		/// An optional description of the attribute, which the CMS editing system may display to the user, to
		/// better identify the attribute being edited.
		/// </param>
		void RenderEditingMetadata(IPortalViewAttribute attribute, TagBuilder tag, string description = null);

		/// <summary>
		/// Renders CMS editing HTML metadata for a given <see cref="IPortalViewEntity"/>.
		/// </summary>
		/// <param name="entity">The entity to render metadata for.</param>
		/// <param name="tag">The HTML container tag into which the metadata will be rendered.</param>
		void RenderEditingMetadata(IPortalViewEntity entity, TagBuilder tag);

		/// <summary>
		/// Renders CMS editing HTML metadata to allow creation of a new entity record.
		/// </summary>
		/// <param name="entityLogicalName">The logical name of the entity record to be created.</param>
		/// <param name="tag">The HTML container tag into which the metadata will be rendered.</param>
		/// <param name="description">
		/// An optional description of the attribute, which the CMS editing system may display to the user, to
		/// better identify the record being created.
		/// </param>
		void RenderEditingMetadata(string entityLogicalName, TagBuilder tag, string description = null, JObject initialValues = null);
	}
}
