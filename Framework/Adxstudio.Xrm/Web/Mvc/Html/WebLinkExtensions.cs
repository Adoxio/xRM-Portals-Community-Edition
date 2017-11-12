/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering Web Links (adx_weblinkset, adx_weblink) in Adxstudio Portals applications.
	/// </summary>
	public static class WebLinkExtensions
	{
		private const int DefaultMaximumWebLinkChildDepth = 2;

		/// <summary>
		/// Returns HTML to render a link (%lt;a&gt; tag) for a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> to render a link for.</param>
		/// <param name="showImage">
		/// Determines whether any image associated with <paramref name="webLink"/> will also be rendered. True by default. If an image
		/// is present, it will be rendered as an &lt;img&gt; tag, nested inside the &lt;a&gt;
		/// </param>
		/// <param name="cssClass">A class attribute value to be added to the link tag.</param>
		/// <returns>
		/// Returns HTML to render a link (%lt;a&gt; tag).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="ArgumentNullException"/> is null.</exception>
		public static IHtmlString WebLink(this HtmlHelper html, IWebLink webLink, bool showImage = true, string cssClass = null)
		{
			return WebLink(html, webLink, new { }, showImage, cssClass);
		}

		/// <summary>
		/// Returns HTML to render a link (%lt;a&gt; tag) for a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> to render a link for.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="showImage">
		/// Determines whether any image associated with <paramref name="webLink"/> will also be rendered. True by default. If an image
		/// is present, it will be rendered as an &lt;img&gt; tag, nested inside the &lt;a&gt;
		/// </param>
		/// <param name="cssClass">A class attribute value to be added to the link tag.</param>
		/// <returns>
		/// Returns HTML to render a link (%lt;a&gt; tag).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="ArgumentNullException"/> is null.</exception>
		public static IHtmlString WebLink(this HtmlHelper html, IWebLink webLink, object htmlAttributes, bool showImage = true, string cssClass = null)
		{
			return WebLink(html, webLink, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), showImage, cssClass);
		}

		/// <summary>
		/// Returns HTML to render a link (%lt;a&gt; tag) for a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> to render a link for.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the link tag.</param>
		/// <param name="showImage">
		/// Determines whether any image associated with <paramref name="webLink"/> will also be rendered. True by default. If an image
		/// is present, it will be rendered as an &lt;img&gt; tag, nested inside the &lt;a&gt;
		/// </param>
		/// <param name="cssClass">A class attribute value to be added to the link tag.</param>
		/// <returns>
		/// Returns HTML to render a link (%lt;a&gt; tag).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="ArgumentNullException"/> is null.</exception>
		public static IHtmlString WebLink(this HtmlHelper html, IWebLink webLink, IDictionary<string, object> htmlAttributes, bool showImage = true, string cssClass = null)
		{
			return new HtmlString(WebLinkTag(html, webLink, htmlAttributes, showImage, cssClass).ToString());
		}

		private static TagBuilder WebLinkTag(HtmlHelper html, IWebLink webLink, IDictionary<string, object> htmlAttributes, bool showImage = true, string cssClass = null)
		{
			if (webLink == null)
			{
				throw new ArgumentNullException("webLink");
			}

			var tag = new TagBuilder("a");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			if (webLink.Url != null)
			{
				tag.Attributes["href"] = webLink.Url;
			}

			if (!string.IsNullOrEmpty(webLink.ToolTip))
			{
				tag.Attributes["title"] = webLink.ToolTip;
			}
			
			if (webLink.NoFollow)
			{
				tag.Attributes["rel"] = "nofollow";
			}

			if (webLink.OpenInNewWindow)
			{
				tag.Attributes["target"] = "_blank";
			}

			var text = webLink.Name.Value == null ? string.Empty : webLink.Name.Value.ToString();

			if (showImage && webLink.HasImage)
			{
				tag.InnerHtml += WebLinkImage(html, webLink);

				if (!webLink.DisplayImageOnly)
				{
					var nameTag = new TagBuilder("span");

					nameTag.AddCssClass("name");
					nameTag.SetInnerText(text);

					tag.InnerHtml += " " + nameTag;
				}
			}
			else
			{
				tag.SetInnerText(text);
			}

			if (htmlAttributes != null)
			{
				tag.MergeAttributes(htmlAttributes, true);
			}

			return tag;
		}

		/// <summary>
		/// Returns HTML to render an image associated with a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> whose image will be rendered, if it has one.</param>
		/// <returns>
		/// Returns HTML to render a web link image. If <paramref name="webLink"/> has no associated image, retuens an empty string.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webLink"/> is null.</exception>
		public static IHtmlString WebLinkImage(this HtmlHelper html, IWebLink webLink)
		{
			return WebLinkImage(html, webLink, new { });
		}

		/// <summary>
		/// Returns HTML to render an image associated with a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> whose image will be rendered, if it has one.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the image tag.</param>
		/// <returns>
		/// Returns HTML to render a web link image. If <paramref name="webLink"/> has no associated image, retuens an empty string.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webLink"/> is null.</exception>
		public static IHtmlString WebLinkImage(this HtmlHelper html, IWebLink webLink, object htmlAttributes)
		{
			return WebLinkImage(html, webLink, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		/// <summary>
		/// Returns HTML to render an image associated with a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> whose image will be rendered, if it has one.</param>
		/// <param name="htmlAttributes">HTML attributes that will be added to the image tag.</param>
		/// <returns>
		/// Returns HTML to render a web link image. If <paramref name="webLink"/> has no associated image, retuens an empty string.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webLink"/> is null.</exception>
		public static IHtmlString WebLinkImage(this HtmlHelper html, IWebLink webLink, IDictionary<string, object> htmlAttributes)
		{
			if (webLink == null)
			{
				throw new ArgumentNullException("webLink");
			}

			if (!webLink.HasImage)
			{
				return new HtmlString(string.Empty);
			}

			// If the ImageUrl attribute starts with a '.' followed by a word character (alphanumeric or underscore), interpret
			// this as a CSS class for an icon element. Return an empty element with that class.
			if (webLink.ImageUrl.StartsWith(".") && Regex.IsMatch(webLink.ImageUrl, @"^\.\w"))
			{
				var classes = webLink.ImageUrl.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim());

				var icon = new TagBuilder("span");

				icon.AddCssClass(string.Join(" ", classes));
				icon.AddCssClass("weblink-image");
				icon.MergeAttribute("aria-hidden", "true", true);

				return new HtmlString(icon.ToString());
			}

			var style = string.Empty;
			var image = new TagBuilder("img");

			image.AddCssClass("weblink-image");

			image.Attributes["src"] = UrlHelper.GenerateContentUrl(webLink.ImageUrl, html.ViewContext.HttpContext);
			image.Attributes["alt"] = webLink.ImageAlternateText ?? (webLink.Name != null && webLink.Name.Value != null ? webLink.Name.Value.ToString() : null);

			if (webLink.ImageHeight.HasValue)
			{
				image.Attributes["height"] = webLink.ImageHeight.Value.ToString(CultureInfo.InvariantCulture);
				style = string.Format("height:{0}px;", webLink.ImageHeight.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (webLink.ImageWidth.HasValue)
			{
				image.Attributes["width"] = webLink.ImageWidth.Value.ToString(CultureInfo.InvariantCulture);
				style += string.Format("width:{0}px;", webLink.ImageWidth.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (!string.IsNullOrWhiteSpace(style))
			{
				image.Attributes["style"] = style;
			}

			if (htmlAttributes != null)
			{
				image.MergeAttributes(htmlAttributes, true);
			}

			return new HtmlString(image.ToString(TagRenderMode.SelfClosing));
		}

		/// <summary>
		/// Returns HTML to render a list item tag (&lt;li&gt;) for a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> to be rendered.</param>
		/// <param name="showDescription">Render the description attribute (adx_description) of the web link.</param>
		/// <param name="showImage">Render the optional image associated with the web link.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag, if the target URL for the web link matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag, if the target URL for the web link matches the URL of
		/// an ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="htmlAttributes">Optional HTML attributes that will be added to the image tag.</param>
		/// <param name="maximumWebLinkChildDepth">
		/// The maximum depth of child web links that this method will render.
		/// </param>
		/// <param name="currentWebLinkChildDepth">
		/// The current web liink child depth being rendered by this method.
		/// </param>
		/// <param name="webLinkVisitor">
		/// Action that is executed for each weblink to be rendered.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render a list item tag (&lt;li&gt;) for a given <see cref="IWebLink"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webLink"/> is null.</exception>
		public static IHtmlString WebLinkListItem(this HtmlHelper html, IWebLink webLink, bool showDescription = true, bool showImage = true, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, IDictionary<string, object> htmlAttributes = null, int maximumWebLinkChildDepth = DefaultMaximumWebLinkChildDepth, int currentWebLinkChildDepth = 2, Action<IWebLink, int, TagBuilder, TagBuilder, TagBuilder, IEnumerable<SiteMapNode>> webLinkVisitor = null, bool clientSiteMapState = false)
		{
			return new HtmlString(WebLinkListItemTag(html, webLink, PortalExtensions.GetPortalViewContext(html), showDescription, showImage, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, htmlAttributes, maximumWebLinkChildDepth, currentWebLinkChildDepth, webLinkVisitor, clientSiteMapState).ToString());
		}

		private static TagBuilder WebLinkListItemTag(HtmlHelper html, IWebLink webLink, IPortalViewContext portalViewContext, bool showDescription = true, bool showImage = true, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, IDictionary<string, object> htmlAttributes = null, int maximumWebLinkChildDepth = DefaultMaximumWebLinkChildDepth, int currentWebLinkChildDepth = 2, Action<IWebLink, int, TagBuilder, TagBuilder, TagBuilder, IEnumerable<SiteMapNode>> webLinkVisitor = null, bool clientSiteMapState = false)
		{
			if (webLink == null)
			{
				throw new ArgumentNullException("webLink");
			}

			if (portalViewContext == null)
			{
				throw new ArgumentNullException("portalViewContext");
			}

			var itemTag = new TagBuilder("li");

			if (!webLink.IsExternal && webLink.Url != null)
			{
				if (clientSiteMapState)
				{
					itemTag.Attributes["data-sitemap-node"] = webLink.Url;
				}
				else
				{
					if (!string.IsNullOrEmpty(currentSiteMapNodeCssClass)
						&& (webLink.Page == null
							? portalViewContext.IsCurrentSiteMapNode(webLink.Url)
							: portalViewContext.IsCurrentSiteMapNode(webLink.Page)))
					{
						itemTag.AddCssClass(currentSiteMapNodeCssClass);
					}

					if (!string.IsNullOrEmpty(ancestorSiteMapNodeCssClass)
						&& (webLink.Page == null
							? portalViewContext.IsAncestorSiteMapNode(webLink.Url, true)
							: portalViewContext.IsAncestorSiteMapNode(webLink.Page, true)))
					{
						itemTag.AddCssClass(ancestorSiteMapNodeCssClass);
					}
				}
			}

			var linkTag = WebLinkTag(html, webLink, null, showImage);

			var childListTag = new TagBuilder("ul");

			if (htmlAttributes != null)
			{
				itemTag.MergeAttributes(htmlAttributes, true);
			}

			var childSiteMapNodes = GetChildSiteMapNodes(webLink, portalViewContext).ToArray();

			if (webLinkVisitor != null)
			{
				webLinkVisitor(webLink, currentWebLinkChildDepth, itemTag, linkTag, childListTag, childSiteMapNodes);
			}

			itemTag.InnerHtml += linkTag;

			if (showDescription)
			{
				var description = new TagBuilder("div");

				description.AddCssClass("description");
				description.InnerHtml = webLink.Description.Value == null ? string.Empty : webLink.Description.Value.ToString();

				itemTag.InnerHtml += description.ToString();
			}

			var hasChildren = false;

			if (currentWebLinkChildDepth <= maximumWebLinkChildDepth)
			{
				if (webLink.DisplayPageChildLinks)
				{
					if (childSiteMapNodes.Any())
					{
						hasChildren = true;

						childListTag.AddCssClass("weblinks-depth-{0}".FormatWith(currentWebLinkChildDepth));
						childListTag.MergeAttribute("data-weblinks-depth", currentWebLinkChildDepth.ToString(CultureInfo.InvariantCulture), true);

						foreach (var childNode in childSiteMapNodes)
						{
							childListTag.InnerHtml += SiteMapNodeWebLinkListItemTag(childNode, portalViewContext, showDescription, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, clientSiteMapState: clientSiteMapState);
						}
					}
				}
				else if (webLink.WebLinks.Any())
				{
					hasChildren = true;

					childListTag.AddCssClass("weblinks-depth-{0}".FormatWith(currentWebLinkChildDepth));
					childListTag.MergeAttribute("data-weblinks-depth", currentWebLinkChildDepth.ToString(CultureInfo.InvariantCulture), true);

					foreach (var childWebLink in webLink.WebLinks)
					{
						childListTag.InnerHtml += WebLinkListItemTag(
							html,
							childWebLink,
							portalViewContext,
							showDescription,
							showImage,
							currentSiteMapNodeCssClass,
							ancestorSiteMapNodeCssClass,
							maximumWebLinkChildDepth: maximumWebLinkChildDepth,
							currentWebLinkChildDepth: currentWebLinkChildDepth + 1,
							webLinkVisitor: webLinkVisitor,
							clientSiteMapState: clientSiteMapState);
					}
				}
			}

			if (hasChildren)
			{
				itemTag.InnerHtml += childListTag;
			}

			return itemTag;
		}

		/// <summary>
		/// Returns Bootstrap List Group HTML to render a list item tag (&lt;li&gt;) for a given <see cref="IWebLink"/>.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLink">The <see cref="IWebLink"/> to be rendered.</param>
		/// <param name="showDescription">Render the description attribute (adx_description) of the web link.</param>
		/// <param name="showImage">Render the optional image associated with the web link.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag, if the target URL for the web link matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag, if the target URL for the web link matches the URL of
		/// an ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="htmlAttributes">Optional HTML attributes that will be added to the image tag.</param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render a list item tag (&lt;li&gt;) for a given <see cref="IWebLink"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="webLink"/> is null.</exception>
		public static IHtmlString WebLinkListGroupItem(this HtmlHelper html, IWebLink webLink, bool showDescription = true, bool showImage = true, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, IDictionary<string, object> htmlAttributes = null, bool clientSiteMapState = false)
		{
			return new HtmlString(WebLinkListGroupItemTag(html, webLink, PortalExtensions.GetPortalViewContext(html), showDescription, showImage, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, htmlAttributes, clientSiteMapState).ToString());
		}

		private static TagBuilder WebLinkListGroupItemTag(HtmlHelper html, IWebLink webLink, IPortalViewContext portalViewContext, bool showDescription = true, bool showImage = true, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, IDictionary<string, object> htmlAttributes = null, bool clientSiteMapState = false)
		{
			if (webLink == null)
			{
				throw new ArgumentNullException("webLink");
			}

			if (portalViewContext == null)
			{
				throw new ArgumentNullException("portalViewContext");
			}

			var tag = new TagBuilder("a");

			tag.AddCssClass("list-group-item");
			tag.AddCssClass("weblink");

			if (!webLink.IsExternal && webLink.Url != null)
			{
				if (clientSiteMapState)
				{
					tag.Attributes["data-sitemap-node"] = webLink.Url;
				}
				else
				{
					if (!string.IsNullOrEmpty(currentSiteMapNodeCssClass)
						&& (webLink.Page == null
							? portalViewContext.IsCurrentSiteMapNode(webLink.Url)
							: portalViewContext.IsCurrentSiteMapNode(webLink.Page)))
					{
						tag.AddCssClass(currentSiteMapNodeCssClass);
					}

					if (!string.IsNullOrEmpty(ancestorSiteMapNodeCssClass)
						&& (webLink.Page == null
							? portalViewContext.IsAncestorSiteMapNode(webLink.Url, true)
							: portalViewContext.IsAncestorSiteMapNode(webLink.Page, true)))
					{
						tag.AddCssClass(ancestorSiteMapNodeCssClass);
					}
				}
			}

			if (webLink.Url != null)
			{
				tag.Attributes["href"] = webLink.Url;
			}

			if (!string.IsNullOrEmpty(webLink.ToolTip))
			{
				tag.Attributes["title"] = webLink.ToolTip;
			}
			
			if (webLink.NoFollow)
			{
				tag.Attributes["rel"] = "nofollow";
			}

			if (webLink.OpenInNewWindow)
			{
				tag.Attributes["target"] = "_blank";
			}

			if (htmlAttributes != null)
			{
				tag.MergeAttributes(htmlAttributes, true);
			}

			if (showImage && webLink.HasImage)
			{
				tag.InnerHtml += WebLinkImage(html, webLink);

				if (webLink.DisplayImageOnly)
				{
					return tag;
				}
				
				tag.InnerHtml += " ";
			}

			var text = webLink.Name.Value == null ? string.Empty : webLink.Name.Value.ToString();

			if (showDescription)
			{
				var headingTag = new TagBuilder("h4");

				headingTag.AddCssClass("list-group-item-heading weblink-name");
				headingTag.SetInnerText(text);

				tag.InnerHtml += headingTag.ToString();

				var textTag = new TagBuilder("div");

				textTag.AddCssClass("list-group-item-text weblink-description");
				textTag.InnerHtml += webLink.Description.Value == null ? string.Empty : webLink.Description.Value.ToString();

				tag.InnerHtml += textTag.ToString();

				return tag;
			}

			var nameTag = new TagBuilder("span");

			nameTag.AddCssClass("weblink-name");
			nameTag.SetInnerText(text);

			tag.InnerHtml += nameTag.ToString();

			return tag;
		}

		private static IEnumerable<SiteMapNode> GetChildSiteMapNodes(IWebLink webLink, IPortalViewContext portalViewContext)
		{
			if (webLink == null)
			{
				throw new ArgumentNullException("webLink");
			}

			if (portalViewContext == null)
			{
				throw new ArgumentNullException("portalViewContext");
			}

			if (webLink.Url == null || !webLink.DisplayPageChildLinks)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var siteMapNode = siteMapProvider.FindSiteMapNode(webLink.Url);

			if (siteMapNode == null || !siteMapNode.HasChildNodes)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var entityNode = siteMapNode as CrmSiteMapNode;

			if (entityNode != null && entityNode.StatusCode != HttpStatusCode.OK)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			return siteMapNode.ChildNodes.Cast<SiteMapNode>();
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset). Provides
		/// inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSetName">The name of the Web Link Set (adx_weblinkset) to render.</param>
		/// <param name="showDescriptions">Render the description attributes (adx_description) of the web links.</param>
		/// <param name="showImages">Render the optional images associated with the web links.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">
		/// Class attribute value to be added to the inner list (UL) element rendered by this method.
		/// </param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="maximumWebLinkChildDepth">
		/// The maximum depth of child web links that this method will render.
		/// </param>
		/// <param name="webLinkVisitor">
		/// Action that is executed for each weblink to be rendered.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset). If a web link
		/// set with name <paramref name="webLinkSetName"/> is not found, returns an empty string.
		/// </returns>
		public static IHtmlString WebLinks(this HtmlHelper html, string webLinkSetName, bool showDescriptions = true, bool showImages = true, string cssClass = null, string listCssClass = null, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, int maximumWebLinkChildDepth = DefaultMaximumWebLinkChildDepth, Action<IWebLink, int, TagBuilder, TagBuilder, TagBuilder, IEnumerable<SiteMapNode>> webLinkVisitor = null, bool clientSiteMapState = false)
		{
			var webLinkSet = PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);

			return webLinkSet == null
				? new HtmlString(string.Empty)
				: WebLinks(html, webLinkSet, showDescriptions, showImages, cssClass, listCssClass, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, maximumWebLinkChildDepth, webLinkVisitor, clientSiteMapState);
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset). Provides
		/// inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSet">The <see cref="IWebLinkSet"/> to render.</param>
		/// <param name="showDescriptions">Render the description attributes (adx_description) of the web links.</param>
		/// <param name="showImages">Render the optional images associated with the web links.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">
		/// Class attribute value to be added to the inner list (UL) element rendered by this method.
		/// </param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="maximumWebLinkChildDepth">
		/// The maximum depth of child web links that this method will render.
		/// </param>
		/// <param name="webLinkVisitor">
		/// Action that is executed for each weblink to be rendered.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="webLinkSet"/> is null.
		/// </exception>
		public static IHtmlString WebLinks(this HtmlHelper html, IWebLinkSet webLinkSet, bool showDescriptions = true, bool showImages = true, string cssClass = null, string listCssClass = null, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, int maximumWebLinkChildDepth = DefaultMaximumWebLinkChildDepth, Action<IWebLink, int, TagBuilder, TagBuilder, TagBuilder, IEnumerable<SiteMapNode>> webLinkVisitor = null, bool clientSiteMapState = false)
		{
			if (webLinkSet == null)
			{
				throw new ArgumentNullException("webLinkSet");
			}

			var tag = new TagBuilder("div");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			var webLinks = webLinkSet.WebLinks.ToArray();

			if (webLinks.Any())
			{
				var list = new TagBuilder("ul");

				if (!string.IsNullOrEmpty(listCssClass))
				{
					list.AddCssClass(listCssClass);
				}

				if (clientSiteMapState)
				{
					list.Attributes["data-state"] = "sitemap";

					if (!string.IsNullOrEmpty(currentSiteMapNodeCssClass))
					{
						list.Attributes["data-sitemap-current"] = currentSiteMapNodeCssClass;
					}

					if (!string.IsNullOrEmpty(ancestorSiteMapNodeCssClass))
					{
						list.Attributes["data-sitemap-ancestor"] = ancestorSiteMapNodeCssClass;
					}
				}

				list.AddCssClass("weblinks-depth-1");
				list.Attributes["data-weblinks-depth"] = "1";

				var portalViewContext = PortalExtensions.GetPortalViewContext(html);

				foreach (var webLink in webLinkSet.WebLinks)
				{
					list.InnerHtml += WebLinkListItemTag(
						html,
						webLink,
						portalViewContext,
						showDescriptions,
						showImages,
						currentSiteMapNodeCssClass,
						ancestorSiteMapNodeCssClass,
						maximumWebLinkChildDepth: maximumWebLinkChildDepth,
						webLinkVisitor: webLinkVisitor,
						clientSiteMapState: clientSiteMapState);
				}

				tag.InnerHtml += list.ToString();
			}

			if (webLinkSet.Editable)
			{
				tag.AddCssClass("xrm-entity");
				tag.AddCssClass("xrm-editable-{0}".FormatWith(webLinkSet.EntityReference.LogicalName));
				tag.Attributes["data-weblinks-maxdepth"] = maximumWebLinkChildDepth.ToString(CultureInfo.InvariantCulture);

				var portalViewContext = PortalExtensions.GetPortalViewContext(html);

				portalViewContext.RenderEditingMetadata(webLinkSet, tag);
			}

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Returns Bootstrap List Group HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset).
		/// Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSetName">The name of the Web Link Set (adx_weblinkset) to render.</param>
		/// <param name="showDescriptions">Render the description attributes (adx_description) of the web links.</param>
		/// <param name="showImages">Render the optional images associated with the web links.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">
		/// Class attribute value to be added to the inner list (UL) element rendered by this method.
		/// </param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset). If a web link
		/// set with name <paramref name="webLinkSetName"/> is not found, returns an empty string.
		/// </returns>
		public static IHtmlString WebLinksListGroup(this HtmlHelper html, string webLinkSetName, bool showDescriptions = true, bool showImages = true, string cssClass = null, string listCssClass = null, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, bool clientSiteMapState = false)
		{
			var webLinkSet = PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);

			return webLinkSet == null
				? new HtmlString(string.Empty)
				: WebLinksListGroup(html, webLinkSet, showDescriptions, showImages, cssClass, listCssClass, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, clientSiteMapState);
		}

		/// <summary>
		/// Returns Bootstrap List Group HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset).
		/// Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSet">The <see cref="IWebLinkSet"/> to render.</param>
		/// <param name="showDescriptions">Render the description attributes (adx_description) of the web links.</param>
		/// <param name="showImages">Render the optional images associated with the web links.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">
		/// Class attribute value to be added to the inner list (UL) element rendered by this method.
		/// </param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="webLinkSet"/> is null.
		/// </exception>
		public static IHtmlString WebLinksListGroup(this HtmlHelper html, IWebLinkSet webLinkSet, bool showDescriptions = true, bool showImages = true, string cssClass = null, string listCssClass = null, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, bool clientSiteMapState = false)
		{
			if (webLinkSet == null)
			{
				throw new ArgumentNullException("webLinkSet");
			}

			var tag = new TagBuilder("div");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			var webLinks = webLinkSet.WebLinks.ToArray();

			if (webLinks.Any())
			{
				var list = new TagBuilder("div");

				list.AddCssClass("list-group");
				list.AddCssClass("weblinks");

				if (!string.IsNullOrEmpty(listCssClass))
				{
					list.AddCssClass(listCssClass);
				}

				if (clientSiteMapState)
				{
					list.Attributes["data-state"] = "sitemap";

					if (!string.IsNullOrEmpty(currentSiteMapNodeCssClass))
					{
						list.Attributes["data-sitemap-current"] = currentSiteMapNodeCssClass;
					}

					if (!string.IsNullOrEmpty(ancestorSiteMapNodeCssClass))
					{
						list.Attributes["data-sitemap-ancestor"] = ancestorSiteMapNodeCssClass;
					}
				}

				list.AddCssClass("weblinks-depth-1");
				list.Attributes["data-weblinks-depth"] = "1";

				var portalViewContext = PortalExtensions.GetPortalViewContext(html);

				foreach (var webLink in webLinkSet.WebLinks)
				{
					list.InnerHtml += WebLinkListGroupItemTag(
						html,
						webLink,
						portalViewContext,
						showDescriptions,
						showImages,
						currentSiteMapNodeCssClass,
						ancestorSiteMapNodeCssClass,
						clientSiteMapState: clientSiteMapState);
				}

				tag.InnerHtml += list.ToString();
			}

			if (webLinkSet.Editable)
			{
				tag.AddCssClass("xrm-entity");
				tag.AddCssClass("xrm-editable-{0}".FormatWith(webLinkSet.EntityReference.LogicalName));
				tag.Attributes["data-weblinks-maxdepth"] = "1";

				var portalViewContext = PortalExtensions.GetPortalViewContext(html);

				portalViewContext.RenderEditingMetadata(webLinkSet, tag);
			}

			return new HtmlString(tag.ToString());
		}

		/// <summary>
		/// Gets a <see cref="IWebLinkSet"/> (adx_weblinkset) associated with the current web page or any of its parent pages.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <returns>
		/// Returns a <see cref="IWebLinkSet"/>, or null if the current web page or any of its parent pages does not have a navigation web link set assigned.
		/// </returns>
		public static IWebLinkSet WebLinkSet(this HtmlHelper html)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			if (portalViewContext.Entity.EntityReference.LogicalName != "adx_webpage")
			{
				return null;
			}

			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return null;
			}

			var current = siteMapProvider.GetCurrentNodeAndHintAncestorNodes(-1);

			if (current == null)
			{
				return null;
			}

			EntityReference navigation = null;

			while (current != null)
			{
				var entityNode = current as CrmSiteMapNode;

				if (entityNode != null)
				{
					navigation = entityNode.Entity.GetAttributeValue<EntityReference>("adx_navigation");

					if (navigation != null)
					{
						break;
					}
				}

				var parent = current.ParentNode;

				current = parent;
			}

			if (navigation == null)
			{
				return null;
			}

			var webLinkSetName = navigation.Name;

			return PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);
		}

		/// <summary>
		/// Gets a <see cref="IWebLinkSet"/> (adx_weblinkset) by name.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSetName">The name of the web link set to be retrieved.</param>
		/// <returns>
		/// Returns a <see cref="IWebLinkSet"/>, or null if one with the name <paramref name="webLinkSetName"/> is not found.
		/// </returns>
		public static IWebLinkSet WebLinkSet(this HtmlHelper html, string webLinkSetName)
		{
			return PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);
		}

		/// <summary>
		/// Returns HTML to render editing metadata for a given <see cref="IWebLinkSet"/>, for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSet">The <see cref="IWebLinkSet"/> to render metadata for.</param>
		/// <returns>
		/// Returns HTML editing metadata for the given web link set. If <paramref name="webLinkSet"/> is null, or is not
		/// editable by the current user, returns an empty string.
		/// </returns>
		/// <remarks>
		/// In order for this metadata to enable editing of the given web link set, it must be nested within a container element,
		/// which has the classes "xrm-entity" and "xrm-editable-adx_weblinkset".
		/// </remarks>
		public static IHtmlString WebLinkSetEditingMetadata(this HtmlHelper html, IWebLinkSet webLinkSet)
		{
			if (webLinkSet == null || !webLinkSet.Editable)
			{
				return new HtmlString(string.Empty);
			}

			var tag = new TagBuilder("div");

			PortalExtensions.GetPortalViewContext(html).RenderEditingMetadata(webLinkSet, tag);

			return new HtmlString(tag.InnerHtml);
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar. Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSetName">The name of the web link set to render.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="dropdownMenuCssClass">
		/// An optional class attribute value to be added to the list (UL) tag of a dropdown menu.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar.
		/// </returns>
		/// <remarks>
		/// If a web link set with the name <paramref name="webLinkSetName"/> is not found, this returns an empty string.
		/// </remarks>
		public static IHtmlString WebLinksNavBar(
			this HtmlHelper html,
			string webLinkSetName,
			string cssClass = null,
			string currentSiteMapNodeCssClass = null,
			string ancestorSiteMapNodeCssClass = null,
			string dropdownMenuCssClass = null,
			bool clientSiteMapState = false)
		{
			var webLinkSet = PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);

			return webLinkSet == null
				? new HtmlString(string.Empty)
				: WebLinksNavBar(html, webLinkSet, cssClass, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, dropdownMenuCssClass, clientSiteMapState);
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar. Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSet">The <see cref="IWebLinkSet"/> to render.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="dropdownMenuCssClass">
		/// An optional class attribute value to be added to the list (UL) tag of a dropdown menu.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="webLinkSet"/> is null.
		/// </exception>
		public static IHtmlString WebLinksNavBar(
			this HtmlHelper html,
			IWebLinkSet webLinkSet,
			string cssClass = null,
			string currentSiteMapNodeCssClass = null,
			string ancestorSiteMapNodeCssClass = null,
			string dropdownMenuCssClass = null,
			bool clientSiteMapState = false)
		{
			return WebLinksDropdowns(html, webLinkSet, cssClass, "nav navbar-nav", currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, dropdownMenuCssClass, clientSiteMapState);
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a nav with a
		/// single level of Bootstrap dropdowns. Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSetName">The name of the web link set to render.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">Class attribute value to be added to the top-level list (UL) element rendered by this method.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="dropdownMenuCssClass">
		/// An optional class attribute value to be added to the list (UL) tag of a dropdown menu.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar.
		/// </returns>
		/// <remarks>
		/// If a web link set with the name <paramref name="webLinkSetName"/> is not found, this returns an empty string.
		/// </remarks>
		public static IHtmlString WebLinksDropdowns(
			this HtmlHelper html,
			string webLinkSetName,
			string cssClass = null,
			string listCssClass = null,
			string currentSiteMapNodeCssClass = null,
			string ancestorSiteMapNodeCssClass = null,
			string dropdownMenuCssClass = null,
			bool clientSiteMapState = false)
		{
			var webLinkSet = PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);

			return webLinkSet == null
				? new HtmlString(string.Empty)
				: WebLinksDropdowns(html, webLinkSet, cssClass, listCssClass, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, dropdownMenuCssClass, clientSiteMapState);
		}

		/// <summary>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a nav with a
		/// single level of Bootstrap dropdowns. Provides inline editing support for users with permission.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="webLinkSet">The <see cref="IWebLinkSet"/> to render.</param>
		/// <param name="cssClass">Class attribute value to be added to the container (DIV) element rendered by this method.</param>
		/// <param name="listCssClass">Class attribute value to be added to the top-level list (UL) element rendered by this method.</param>
		/// <param name="currentSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target URL matches the URL of
		/// the current portal Site Map node.
		/// </param>
		/// <param name="ancestorSiteMapNodeCssClass">
		/// An optional class attribute value to be added to the list item tag of any web link whose target matches the URL of an
		/// ancestor node of the current portal Site Map node.
		/// </param>
		/// <param name="dropdownMenuCssClass">
		/// An optional class attribute value to be added to the list (UL) tag of a dropdown menu.
		/// </param>
		/// <param name="clientSiteMapState">
		/// Will add HTML5 data- attributes to the rendered HTML that can be used for client-side highlighting of active and ancestor
		/// site map nodes, rather than processing this server-side.
		/// </param>
		/// <returns>
		/// Returns HTML to render the Web Links (adx_weblink) associated with a given Web Link Set (adx_weblinkset), as a Bootstrap
		/// Navbar.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="webLinkSet"/> is null.
		/// </exception>
		public static IHtmlString WebLinksDropdowns(
			this HtmlHelper html,
			IWebLinkSet webLinkSet,
			string cssClass = null,
			string listCssClass = null,
			string currentSiteMapNodeCssClass = null,
			string ancestorSiteMapNodeCssClass = null,
			string dropdownMenuCssClass = null,
			bool clientSiteMapState = false)
		{
			return WebLinks(html, webLinkSet, false, true, cssClass, listCssClass, currentSiteMapNodeCssClass, ancestorSiteMapNodeCssClass, 2, (webLink, depth, itemTag, linkTag, childListTag, childSiteMapNodes) =>
			{
				if (!((webLink.DisplayPageChildLinks && childSiteMapNodes.Any()) || webLink.WebLinks.Any()))
				{
					return;
				}

				if (depth == 2)
				{
					itemTag.AddCssClass("dropdown");

					linkTag.AddCssClass("dropdown-toggle");
					linkTag.MergeAttribute("data-toggle", "dropdown", true);
					linkTag.MergeAttribute("role", "button", true);
					linkTag.MergeAttribute("aria-expanded", "false", true);

					var caret = new TagBuilder("span");

					caret.AddCssClass("caret");
					caret.MergeAttribute("aria-hidden", "true", true);

					linkTag.InnerHtml += " " + caret.ToString(TagRenderMode.Normal);

					childListTag.AddCssClass("dropdown-menu");
					childListTag.MergeAttribute("role", "menu", true);

					if (!string.IsNullOrEmpty(dropdownMenuCssClass))
					{
						childListTag.AddCssClass(dropdownMenuCssClass);
					}

					if (string.IsNullOrEmpty(webLink.Url))
					{
						linkTag.MergeAttribute("href", "#", true);
					}
					else
					{
						childListTag.InnerHtml += WebLinkListItem(html, webLink, maximumWebLinkChildDepth: 0);

						var dividerTag = new TagBuilder("li");

						dividerTag.AddCssClass("divider");

						childListTag.InnerHtml += dividerTag;
					}
				}
			}, clientSiteMapState);
		}

		private static TagBuilder SiteMapNodeWebLinkListItemTag(SiteMapNode siteMapNode, IPortalViewContext portalViewContext, bool showDescription = true, string currentSiteMapNodeCssClass = null, string ancestorSiteMapNodeCssClass = null, IDictionary<string, object> htmlAttributes = null, bool clientSiteMapState = false)
		{
			if (siteMapNode == null)
			{
				throw new ArgumentNullException("siteMapNode");
			}

			if (portalViewContext == null)
			{
				throw new ArgumentNullException("portalViewContext");
			}

			var itemTag = new TagBuilder("li");

			if (clientSiteMapState)
			{
				itemTag.Attributes["data-sitemap-node"] = siteMapNode.Url;
			}
			else
			{
				if (!string.IsNullOrEmpty(currentSiteMapNodeCssClass) && portalViewContext.IsCurrentSiteMapNode(siteMapNode))
				{
					itemTag.AddCssClass(currentSiteMapNodeCssClass);
				}

				if (!string.IsNullOrEmpty(ancestorSiteMapNodeCssClass) && portalViewContext.IsAncestorSiteMapNode(siteMapNode))
				{
					itemTag.AddCssClass(ancestorSiteMapNodeCssClass);
				}
			}

			if (htmlAttributes != null)
			{
				itemTag.MergeAttributes(htmlAttributes, true);
			}

			var linkTag = new TagBuilder("a");

			linkTag.Attributes["href"] = siteMapNode.Url;
			linkTag.Attributes["title"] = siteMapNode.Title;

			linkTag.SetInnerText(siteMapNode.Title);

			itemTag.InnerHtml += linkTag;

			if (showDescription)
			{
				var description = new TagBuilder("div");

				description.AddCssClass("description");
				description.InnerHtml = siteMapNode.Description ?? string.Empty;

				itemTag.InnerHtml += description.ToString();
			}

			return itemTag;
		}
	}
}
