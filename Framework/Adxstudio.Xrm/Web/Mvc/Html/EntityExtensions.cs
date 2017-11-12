/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering portal view entities in Adxstudio Portals applications.
	/// </summary>
	public static class EntityExtensions
	{
		/// <summary>
		/// Gets the current portal context <see cref="IPortalViewEntity"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <returns>The current portal context <see cref="IPortalViewEntity"/>.</returns>
		public static IPortalViewEntity Entity(this HtmlHelper html)
		{
			return PortalExtensions.GetPortalViewContext(html).Entity;
		}

		/// <summary>
		/// Gets a <see cref="IPortalViewEntity"/> associated with a given key, from the portal view context.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entityKey">The key with which the target entity is associated, in the current view context.</param>
		/// <returns>
		/// The <see cref="IPortalViewEntity"/> corresponding to <paramref name="entityKey"/>. If an entity with that key is not found in
		/// the current view context, returns null.
		/// </returns>
		public static IPortalViewEntity Entity(this HtmlHelper html, string entityKey)
		{
			return PortalExtensions.GetPortalViewContext(html)[entityKey];
		}

		/// <summary>
		/// Renders hidden metadata to the HTML DOM to support client-side editing of a given portal entity, for users with
		/// permission. Implicitly uses the current portal context entity as its source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="cssClass">An optional class attribute value to be added to the root element rendered by this method.</param>
		/// <returns>Editing metadata, as HTML.</returns>
		public static IHtmlString EntityEditingMetadata(this HtmlHelper html, string cssClass = null)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return EntityEditingMetadata(portalViewContext.Entity, portalViewContext, cssClass);
		}

		/// <summary>
		/// Renders hidden metadata to the HTML DOM to support client-side editing of a given portal entity, for users with
		/// permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The entity whose metadata will be rendered.</param>
		/// <param name="cssClass">An optional class attribute value to be added to the root element rendered by this method.</param>
		/// <returns>Editing metadata, as HTML.</returns>
		public static IHtmlString EntityEditingMetadata(this HtmlHelper html, IPortalViewEntity entity, string cssClass = null)
		{
			return EntityEditingMetadata(entity, PortalExtensions.GetPortalViewContext(html), cssClass);
		}

		private static IHtmlString EntityEditingMetadata(IPortalViewEntity entity, IPortalViewContext portalViewContext, string cssClass = null)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			if (!entity.Editable)
			{
				return new HtmlString(string.Empty);
			}

			if (portalViewContext == null)
			{
				throw new ArgumentNullException("portalViewContext");
			}

			var tag = new TagBuilder("div");

			tag.Attributes["style"] = "display:none;";
			tag.AddCssClass("xrm-entity");
			tag.AddCssClass("xrm-editable-{0}".FormatWith(entity.EntityReference.LogicalName));

			if (portalViewContext.IsCurrentSiteMapNode(entity))
			{
				tag.AddCssClass("xrm-entity-current");
			}

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			portalViewContext.RenderEditingMetadata(entity, tag);

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText)
		{
			return EntityLink(html, serviceContext, entity, linkText, new { });
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, object queryStringParameters)
		{
			return EntityLink(html, serviceContext, entity, linkText, queryStringParameters, new { });
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, NameValueCollection queryStringParameters)
		{
			return EntityLink(html, serviceContext, entity, linkText, queryStringParameters, new { });
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, object queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			return EntityLink(html, serviceContext, entity, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, object queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, serviceContext, entity, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, NameValueCollection queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, serviceContext, entity, linkText, queryStringParameters, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, string linkText, NameValueCollection queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return EntityLink(html, portalViewContext.GetEntity(serviceContext, entity), linkText, queryStringParameters, htmlAttributes);
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText)
		{
			return EntityLink(html, linkText, new { });
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, object queryStringParameters)
		{
			return EntityLink(html, linkText, queryStringParameters, new { });
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, NameValueCollection queryStringParameters)
		{
			return EntityLink(html, linkText, queryStringParameters, new { });
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, object queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, object queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			return EntityLink(html, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, NameValueCollection queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, linkText, queryStringParameters, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		/// <summary>
		/// Renders a link to the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the current portal context entity.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, string linkText, NameValueCollection queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return EntityLink(html, portalViewContext.Entity, linkText, queryStringParameters, htmlAttributes);
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText)
		{
			return EntityLink(html, entity, linkText, new { });
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, object queryStringParameters)
		{
			return EntityLink(html, entity, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters));
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, NameValueCollection queryStringParameters)
		{
			return EntityLink(html, entity, linkText, queryStringParameters, new { });
		}
		
		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, object queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, entity, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, object queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			return EntityLink(html, entity, linkText, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes);
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, NameValueCollection queryStringParameters, object htmlAttributes)
		{
			return EntityLink(html, entity, linkText, queryStringParameters, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		/// <summary>
		/// Renders a link to a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> for which a link will be rendered.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <returns>
		/// Returns an HTML A tag linking to the given entity. Returns an empty string if <paramref name="entity"/> is null.
		/// </returns>
		public static IHtmlString EntityLink(this HtmlHelper html, IPortalViewEntity entity, string linkText, NameValueCollection queryStringParameters, IDictionary<string, object> htmlAttributes)
		{
			if (entity == null)
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder("a");

			var url = EntityUrl(html, entity, queryStringParameters);

			if (url != null)
			{
				tag.Attributes["href"] = url;
			}

			if (htmlAttributes != null)
			{
				tag.MergeAttributes(htmlAttributes, true);
			}
			
			tag.SetInnerText(linkText);

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose URL will be returned.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity)
		{
			return EntityUrl(html, serviceContext, entity, new { });
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, object queryStringParameters)
		{
			return EntityUrl(html, serviceContext, entity, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters));
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceContext">
		/// The <see cref="OrganizationServiceContext"/> to which <paramref name="entity"/> is attached, and which will be used to load
		/// any additional data required to render the attribute (e.g., performing security assertions).
		/// </param>
		/// <param name="entity">The <see cref="Microsoft.Xrm.Sdk.Entity"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, OrganizationServiceContext serviceContext, Entity entity, NameValueCollection queryStringParameters)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return EntityUrl(html, portalViewContext.GetEntity(serviceContext, entity), queryStringParameters);
		}

		/// <summary>
		/// Return a URL for the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <returns>
		/// Returns a URL for the current portal context entity.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html)
		{
			return EntityUrl(html, new { });
		}

		/// <summary>
		/// Return a URL for the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the current portal context entity.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, object queryStringParameters)
		{
			return EntityUrl(html, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters));
		}

		/// <summary>
		/// Return a URL for the current portal context entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the current portal context entity.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, NameValueCollection queryStringParameters)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return EntityUrl(html, portalViewContext.Entity, queryStringParameters);
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose URL will be returned.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, IPortalViewEntity entity)
		{
			return EntityUrl(html, entity, new { });
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, IPortalViewEntity entity, object queryStringParameters)
		{
			return EntityUrl(html, entity, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters));
		}

		/// <summary>
		/// Return a URL for a given entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="entity">The <see cref="IPortalViewEntity"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <returns>
		/// Returns a URL for the given entity. Returns a null if <paramref name="entity"/> is null, or <paramref name="entity"/> does not have
		/// a URL.
		/// </returns>
		public static string EntityUrl(this HtmlHelper html, IPortalViewEntity entity, NameValueCollection queryStringParameters)
		{
			if (entity == null)
			{
				return null;
			}

			if (entity.Url == null)
			{
				return null;
			}

			if (queryStringParameters == null || !queryStringParameters.HasKeys())
			{
				return entity.Url;
			}

			return entity.Url.AppendQueryString(queryStringParameters);
		}

		internal static HtmlHelper GetHtmlHelper(string portalName, RequestContext requestContext, HttpResponse response)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName, requestContext);
			var controllerContext = new ControllerContext(requestContext, new MockController());
			var portalViewContext = new PortalViewContext(new PortalContextDataAdapterDependencies(portal));

			var htmlHelper = new HtmlHelper(new ViewContext(controllerContext, new MockView(), new ViewDataDictionary(), new TempDataDictionary(), response.Output)
			{
				ViewData = new ViewDataDictionary
				{
					{ PortalExtensions.PortalViewContextKey, portalViewContext }
				}
			}, new ViewPage());

			htmlHelper.ViewData[PortalExtensions.PortalViewContextKey] = portalViewContext;

			return htmlHelper;
		}

		internal class MockController : Controller { }

		internal class MockView : IView { public void Render(ViewContext viewContext, TextWriter writer) { } }
	}
}
