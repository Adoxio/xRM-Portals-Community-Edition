/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Resources;
using DotLiquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering Content Snippets (adx_contentsnippet) in Adxstudio Portals applications.
	/// </summary>
	public static class SnippetExtensions
	{
		internal const bool AllowCreateDefault = true;

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippetName">The name (adx_name) of the Snippet (adx_contentsnippet) to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this snippet. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <param name="allowCreate">
		/// Enable portal editing support for creating a snippet that does not exist, upon edit.
		/// </param>
		/// <returns>
		/// An HTML structure containing a the snippet value (if found), with support for inline CMS editing for users with permission.
		/// </returns>
		public static IHtmlString Snippet(this HtmlHelper html, string snippetName, string editType = "html", bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null, bool allowCreate = AllowCreateDefault)
		{
			return SnippetInternal(html, snippetName, editType, htmlEncode, tagName, cssClass, liquidEnabled, defaultValue: defaultValue, allowCreate: allowCreate);
		}

		internal static IHtmlString SnippetInternal(this HtmlHelper html, string snippetName, string editType = "html", bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, Context liquidContext = null, string defaultValue = null, bool allowCreate = AllowCreateDefault, string displayName = null)
		{
			var snippet = PortalExtensions.GetPortalViewContext(html).Snippets.Select(snippetName);

			if (snippet != null)
			{
				return SnippetInternal(html, snippet, editType, htmlEncode, tagName, cssClass, liquidEnabled, liquidContext, defaultValue);
			}

			if (allowCreate)
			{
				return SnippetPlaceHolder(html, snippetName, editType, htmlEncode, tagName, cssClass, liquidEnabled, liquidContext, defaultValue, displayName);
			}

			if (defaultValue == null)
			{
				return null;
			}

			if (liquidEnabled)
			{
				return new HtmlString(liquidContext == null
					? html.Liquid(defaultValue)
					: html.Liquid(defaultValue, liquidContext));
			}

			return new HtmlString(defaultValue);
		}

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippet">The <see cref="ISnippet"/> to be rendered.</param>
		/// <param name="editType">
		/// The type of CMS editing interface to use for this snippet. The values "text" and "html" are supported by default.
		/// </param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the attribute should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal attribute value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="ArgumentNullException"/> is null.</exception>
		public static IHtmlString Snippet(this HtmlHelper html, ISnippet snippet, string editType = "html", bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null)
		{
			return html.SnippetInternal(snippet, editType, htmlEncode, tagName, cssClass, liquidEnabled, defaultValue: defaultValue);
		}

		internal static IHtmlString SnippetInternal(this HtmlHelper html, ISnippet snippet, string editType = "html", bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, Context liquidContext = null, string defaultValue = null)
		{
			if (snippet == null)
			{
				throw new ArgumentNullException("snippet");
			}

			string snippetDisplayName = snippet.DisplayName ?? snippet.Name;
			return html.AttributeInternal(snippet.Value, editType, snippetDisplayName, htmlEncode, tagName, cssClass, liquidEnabled, liquidContext, defaultValue, snippet.LanguageName);
		}

		/// <summary>
		/// Renders a a Snippet (adx_contentsnippet) value, with no encoding or other modification.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippetName">The name (adx_name) of the Snippet (adx_contentsnippet) to be rendered.</param>
		/// <param name="defaultValue">An optional default value to be returned if the snippet does not exist or has no value.</param>
		/// <returns>
		/// A literal content snippet value, or <paramref name="defaultValue"/>, if the snippet is not found or has no value. If
        /// <paramref name="defaultValue"/> is also null, returns an empty string.
		/// </returns>
		public static string SnippetLiteral(this HtmlHelper html, string snippetName, string defaultValue = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var snippet = PortalExtensions.GetPortalViewContext(html).Snippets.Select(snippetName);

			return snippet == null
				? liquidEnabled && defaultValue != null
					? html.Liquid(defaultValue)
					: defaultValue
				: SnippetLiteral(html, snippet, defaultValue, liquidEnabled);
		}

		/// <summary>
		/// Renders a a Snippet (adx_contentsnippet) value, with no encoding or other modification.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippetName">The name (adx_name) of the Snippet (adx_contentsnippet) to be rendered.</param>
		/// <param name="defaultValue">An optional default value to be returned if the snippet does not exist or has no value.</param>
		/// <returns>
		/// A literal content snippet value, or <paramref name="defaultValue"/>, if the snippet is not found or has no value. If
        /// <paramref name="defaultValue"/> is also null, returns an empty string.
		/// </returns>
		public static string SnippetLiteral(this HtmlHelper html, string snippetName, IHtmlString defaultValue, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			var snippet = PortalExtensions.GetPortalViewContext(html).Snippets.Select(snippetName);

			if (snippet != null)
			{
				return SnippetLiteral(html, snippet, defaultValue == null ? null : defaultValue.ToString(), liquidEnabled);
			}

			if (defaultValue == null)
			{
				return null;
			}
				
			return liquidEnabled
				? html.Liquid(defaultValue)
				: defaultValue.ToString();
		}

		/// <summary>
		/// Renders a a Snippet (adx_contentsnippet) value, with no encoding or other modification.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippet">The <see cref="ISnippet"/> to be rendered.</param>
		/// <param name="defaultValue">An optional default value to be returned if the snippet has no value.</param>
		/// <returns>
        /// A literal content snippet value, or <paramref name="defaultValue"/>, if the snippet has no value. If <paramref name="defaultValue"/>
		/// is also null, returns an empty string.
		/// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="snippet"/> is null.</exception>
		public static string SnippetLiteral(this HtmlHelper html, ISnippet snippet, string defaultValue = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault)
		{
			if (snippet == null)
			{
				throw new ArgumentNullException("snippet");
			}

			var snippetValue = snippet.Value.Value;

			if (snippetValue == null)
			{
				return liquidEnabled && defaultValue != null
					? html.Liquid(defaultValue)
					: defaultValue;
			}

			return liquidEnabled
				? html.Liquid(snippetValue.ToString())
				: snippetValue.ToString();
		}

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS HTML editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippetName">The name (adx_name) of the Snippet (adx_contentsnippet) to be rendered.</param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <param name="allowCreate">
		/// Enable portal editing support for creating a snippet that does not exist, upon edit.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If a snippet with name <paramref name="snippetName"/> is not found, returns an empty string.
		/// </returns>
		public static IHtmlString HtmlSnippet(this HtmlHelper html, string snippetName, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null, bool allowCreate = AllowCreateDefault)
		{
			return Snippet(html, snippetName, "html", false, "div", cssClass, liquidEnabled, defaultValue, allowCreate);
		}

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS HTML editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippet">The <see cref="ISnippet"/> to be rendered.</param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="snippet"/> is null.</exception>
		public static IHtmlString HtmlSnippet(this HtmlHelper html, ISnippet snippet, string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null)
		{
			return Snippet(html, snippet, "html", false, "div", cssClass, liquidEnabled, defaultValue);
		}

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS text editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippetName">The name (adx_name) of the Snippet (adx_contentsnippet) to be rendered.</param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the snippet should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal snippet value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <param name="allowCreate">
		/// Enable portal editing support for creating a snippet that does not exist, upon edit.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
        /// permission. If a snippet with name <paramref name="snippetName"/> is not found, returns an empty string.
		/// </returns>
		public static IHtmlString TextSnippet(this HtmlHelper html, string snippetName, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null, bool allowCreate = AllowCreateDefault)
		{
			return Snippet(html, snippetName, "text", htmlEncode, tagName, cssClass, liquidEnabled, defaultValue, allowCreate);
		}

		/// <summary>
		/// Renders an HTML structure containing a Snippet (adx_contentsnippet) value as text, with support for inline CMS text editing
		/// for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="snippet">The <see cref="ISnippet"/> to be rendered.</param>
		/// <param name="htmlEncode">
		/// Whether or not the string value of the snippet should be rendered as HTML encoded. Set to false for fields that are
		/// intended to support HTML content, and to true for attributes that are not.
		/// </param>
		/// <param name="tagName">
		/// The HTML element name that will be used to enclose the literal snippet value. These enclosing elements are required
		/// by the CMS editing system. A DIV tag is used by default, and this is the element that should be used in most situations.
		/// </param>
		/// <param name="cssClass">
		/// A class attribute value that will be applied to the outermost container element rendered this helper.
		/// </param>
		/// <param name="defaultValue">
		/// Provide a default value to be rendered in the case that the snippet does not exist, or has no value.
		/// </param>
		/// <returns>
		/// An HTML structure containing a CRM entity attribute value as text, with support for inline CMS editing for users with
		/// permission.
		/// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="snippet"/> is null.</exception>
		public static IHtmlString TextSnippet(this HtmlHelper html, ISnippet snippet, bool htmlEncode = true, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, string defaultValue = null)
		{
			return Snippet(html, snippet, "text", htmlEncode, tagName, cssClass, liquidEnabled, defaultValue);
		}

		/// <summary>
		/// Renders an HTML structure that will allow for the creation of a new Snippet (adx_contentsnippet) record upon edit, for
		/// users with permission.
		/// </summary>
		internal static IHtmlString SnippetPlaceHolder(this HtmlHelper html, string snippetName, string editType = "html", bool htmlEncode = false, string tagName = "div", string cssClass = null, bool liquidEnabled = LiquidExtensions.LiquidEnabledDefault, Context liquidContext = null, string defaultValue = null, string displayName = null)
		{
			if (string.IsNullOrWhiteSpace(snippetName)) throw new ArgumentException("Value can't be null or whitespace.", "snippetName");

			var tag = new TagBuilder(tagName ?? "div");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			if (defaultValue == null)
			{
				tag.AddCssClass("no-value");
			}

			tag.AddCssClass("xrm-attribute");
			tag.AddCssClass("xrm-editable-{0}".FormatWith(editType));

			var valueContainer = new TagBuilder(tagName ?? "div");

			valueContainer.AddCssClass("xrm-attribute-value");

			var stringValue = defaultValue ?? string.Empty;

			stringValue = liquidEnabled
				? liquidContext == null
					? html.Liquid(stringValue)
					: html.Liquid(stringValue, liquidContext)
				: stringValue;

			if (htmlEncode)
			{
				valueContainer.AddCssClass("xrm-attribute-value-encoded");
				valueContainer.SetInnerText(stringValue);
			}
			else
			{
				valueContainer.InnerHtml = stringValue;
			}

			tag.InnerHtml += valueContainer.ToString();

			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var langContext = HttpContext.Current.GetContextLanguageInfo();
			
			if (portalViewContext.WebsiteAccessPermissionProvider.TryAssert(portalViewContext.CreateServiceContext(), WebsiteRight.ManageContentSnippets))
			{
				if (defaultValue != null)
				{
					tag.MergeAttribute("data-default", defaultValue);
				}

				JObject languageJson = null;

				tag.MergeAttribute("data-encoded", htmlEncode ? "true" : "false", true);
				tag.MergeAttribute("data-liquid", liquidEnabled ? "true" : "false", true);

				if (langContext.IsCrmMultiLanguageEnabled)
				{
					tag.MergeAttribute("data-languageContext", langContext.ContextLanguage.DisplayName);
					languageJson = new JObject
					{
						{ "Id", langContext.ContextLanguage.EntityReference.Id.ToString() },
						{ "LogicalName", langContext.ContextLanguage.EntityReference.LogicalName }
					};

					portalViewContext.RenderEditingMetadata("adx_contentsnippet", tag, snippetName, new JObject
					{
						{ "adx_name", snippetName },
						{ "adx_display_name", displayName },
						{ "adx_contentsnippetlanguageid", languageJson }
					});
				}

				else
				{
					portalViewContext.RenderEditingMetadata("adx_contentsnippet", tag, snippetName, new JObject
					{
						{ "adx_name", snippetName },
						{ "adx_display_name", displayName }
					});
				}
			}

			return new HtmlString(tag.ToString());
		}
	}
}
