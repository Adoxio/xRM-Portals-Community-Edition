/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering Site Marker (adx_sitemarker) data in Adxstudio Portals applications.
	/// </summary>
	public static class SiteMarkerExtensions
	{
		/// <summary>
		/// Gets the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// The <see cref="ISiteMarkerTarget">target</see> of the given site marker. If <paramref name="requireTargetReadAccess"/> is set
		/// to true, and the current user does not have read access to the target entity, returns null.
		/// </returns>
		public static ISiteMarkerTarget SiteMarker(this HtmlHelper html, string siteMarkerName, bool requireTargetReadAccess = false)
		{
			var siteMarkers = PortalExtensions.GetPortalViewContext(html).SiteMarkers;

			return requireTargetReadAccess
				? siteMarkers.SelectWithReadAccess(siteMarkerName)
				: siteMarkers.Select(siteMarkerName);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, new { }, linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, object queryStringParameters, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, queryStringParameters, new { }, linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, NameValueCollection queryStringParameters, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, queryStringParameters, new { }, linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, object queryStringParameters, IDictionary<string, object> htmlAttributes, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes, linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, object queryStringParameters, object htmlAttributes, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), htmlAttributes, linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, NameValueCollection queryStringParameters, object htmlAttributes, string linkText = null, bool requireTargetReadAccess = false)
		{
			return SiteMarkerLink(html, siteMarkerName, queryStringParameters, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), linkText, requireTargetReadAccess);
		}

		/// <summary>
		/// Renders a link the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the link URL.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="linkText">The text of the link.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns an HTML A tag linking to the target of a given site marker. Returns an empty string if a target for
		/// <paramref name="siteMarkerName"/> is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user
		/// does not have read access to the target entity, returns an empty string.
		/// </returns>
		public static IHtmlString SiteMarkerLink(this HtmlHelper html, string siteMarkerName, NameValueCollection queryStringParameters, IDictionary<string, object> htmlAttributes, string linkText = null, bool requireTargetReadAccess = false)
		{
			var target = SiteMarker(html, siteMarkerName, requireTargetReadAccess);

			if (target == null)
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder("a");

			var url = SiteMarkerUrl(html, target, queryStringParameters);

			if (url != null)
			{
				tag.Attributes["href"] = url;
			}

			if (htmlAttributes != null)
			{
				tag.MergeAttributes(htmlAttributes, true);
			}
			
			tag.SetInnerText(linkText ?? target.Description);

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns a URL for the target of a given site marker. Returns an empty string if a target for <paramref name="siteMarkerName"/>
		/// is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user does not have read access to
		/// the target entity, returns null.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, string siteMarkerName, bool requireTargetReadAccess = false)
		{
			return SiteMarkerUrl(html, siteMarkerName, new { }, requireTargetReadAccess);
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the URL.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns a URL for the target of a given site marker. Returns an empty string if a target for <paramref name="siteMarkerName"/>
		/// is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user does not have read access to
		/// the target entity, returns null.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, string siteMarkerName, object queryStringParameters, bool requireTargetReadAccess = false)
		{
			return SiteMarkerUrl(html, siteMarkerName, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters), requireTargetReadAccess);
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker), by site marker name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="siteMarkerName">The name of the site marker to retrieve.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the URL.</param>
		/// <param name="requireTargetReadAccess">
		/// Whether the target of the named site marker should be tested for security read access. This is false by default, but if
		/// set to true, and the current user does not have read access to the target entity, this method will return null.
		/// </param>
		/// <returns>
		/// Returns a URL for the target of a given site marker. Returns an empty string if a target for <paramref name="siteMarkerName"/>
		/// is not found. If <paramref name="requireTargetReadAccess"/> is set to true, and the current user does not have read access to
		/// the target entity, returns null.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, string siteMarkerName, NameValueCollection queryStringParameters, bool requireTargetReadAccess = false)
		{
			var target = SiteMarker(html, siteMarkerName, requireTargetReadAccess);

			return target == null ? null : SiteMarkerUrl(html, target, queryStringParameters);
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker).
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="target">The <see cref="ISiteMarkerTarget"/> whose URL will be returned.</param>
		/// <returns>
		/// Returns a URL for a <paramref name="target"/>. Returns null if <paramref name="target"/> is null.
		/// is not found.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, ISiteMarkerTarget target)
		{
			return SiteMarkerUrl(html, target, new { });
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker).
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="target">The <see cref="ISiteMarkerTarget"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the URL.</param>
		/// <returns>
		/// Returns a URL for a <paramref name="target"/>. Returns null if <paramref name="target"/> is null.
		/// is not found.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, ISiteMarkerTarget target, object queryStringParameters)
		{
			return SiteMarkerUrl(html, target, PortalExtensions.AnonymousObjectToQueryStringParameters(queryStringParameters));
		}

		/// <summary>
		/// Returns a URL for the target of a given Site Marker (adx_sitemarker).
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="target">The <see cref="ISiteMarkerTarget"/> whose URL will be returned.</param>
		/// <param name="queryStringParameters">Query string parameter values that will be appended to the URL.</param>
		/// <returns>
		/// Returns a URL for a <paramref name="target"/>. Returns null if <paramref name="target"/> is null.
		/// is not found.
		/// </returns>
		public static string SiteMarkerUrl(this HtmlHelper html, ISiteMarkerTarget target, NameValueCollection queryStringParameters)
		{
			return html.EntityUrl(target, queryStringParameters);
		}
	}
}
