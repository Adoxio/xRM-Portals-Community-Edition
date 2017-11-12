/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	public enum SiteMapNodeType
	{
		Root,
		Parent,
		Current,
	}

	public static class SiteMapExtensions
	{
		public static bool IsRootSiteMapNode(this HtmlHelper html)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return false;
			}

			var current = siteMapProvider.CurrentNode;

			if (current == null)
			{
				return false;
			}

			var root = siteMapProvider.RootNode;

			if (root == null)
			{
				return false;
			}

			return current.Url == root.Url;
		}

		public static bool IsRootSiteMapNode(this HtmlHelper html, string url)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return false;
			}

			var current = siteMapProvider.CurrentNode;

			if (current == null)
			{
				return false;
			}

			var root = siteMapProvider.RootNode;

			if (root == null)
			{
				return false;
			}

			return root.Url == url;
		}

		public static bool IsCurrentSiteMapNode(this HtmlHelper html, string url)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return portalViewContext.IsCurrentSiteMapNode(url);
		}

		public static bool IsCurrentSiteMapNode(this HtmlHelper html, SiteMapNode siteMapNode)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return portalViewContext.IsCurrentSiteMapNode(siteMapNode);
		}

		public static bool IsAncestorSiteMapNode(this HtmlHelper html, string url, bool excludeRootNodes = false)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return portalViewContext.IsAncestorSiteMapNode(url, excludeRootNodes);
		}

		public static bool IsAncestorSiteMapNode(this HtmlHelper html, SiteMapNode siteMapNode, bool excludeRootNodes = false)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			return portalViewContext.IsAncestorSiteMapNode(siteMapNode, excludeRootNodes);
		}

		public static bool IsFirstGenerationParentSiteMapNode(this HtmlHelper html, string url)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return false;
			}

			var node = siteMapProvider.FindSiteMapNode(url);

			if (node == null)
			{
				return false;
			}

			if (node.ParentNode == null)
			{
				return false;
			}

			return node.RootNode.Key == node.ParentNode.Key;
		}

		public static IEnumerable<SiteMapNode> SiteMapChildNodes(this HtmlHelper html)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var current = siteMapProvider.CurrentNode;

			if (current == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}
			
			var entityCurrent = current as CrmSiteMapNode;

			if (entityCurrent != null && entityCurrent.StatusCode != HttpStatusCode.OK)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			return current.ChildNodes.Cast<SiteMapNode>();
		}

		public static IEnumerable<SiteMapNode> SiteMapChildNodes(this HtmlHelper html, string url)
		{
			if (url == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var target = siteMapProvider.FindSiteMapNode(url);

			if (target == null)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			var entityTarget = target as CrmSiteMapNode;

			if (entityTarget != null && entityTarget.StatusCode != HttpStatusCode.OK)
			{
				return Enumerable.Empty<SiteMapNode>();
			}

			return target.ChildNodes.Cast<SiteMapNode>();
		}

		public static IEnumerable<Tuple<SiteMapNode, SiteMapNodeType>> SiteMapPath(this HtmlHelper html, int? takeLast = null)
		{
			return SiteMapPath(html, provider => provider.GetCurrentNodeAndHintAncestorNodes(-1), takeLast);
		}

		public static IEnumerable<Tuple<SiteMapNode, SiteMapNodeType>> SiteMapPath(this HtmlHelper html, string url, int? takeLast = null)
		{
			return SiteMapPath(html, provider => url == null ? null : provider.FindSiteMapNode(url), takeLast);
		}

		private static IEnumerable<Tuple<SiteMapNode, SiteMapNodeType>> SiteMapPath(HtmlHelper html, Func<SiteMapProvider, SiteMapNode> getCurrentNode, int? takeLast)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);
			var siteMapProvider = portalViewContext.SiteMapProvider;

			if (siteMapProvider == null)
			{
				return Enumerable.Empty<Tuple<SiteMapNode, SiteMapNodeType>>();
			}

			var current = getCurrentNode(siteMapProvider);

			if (current == null)
			{
				return Enumerable.Empty<Tuple<SiteMapNode, SiteMapNodeType>>();
			}

			var path = new Stack<Tuple<SiteMapNode, SiteMapNodeType>>();

			path.Push(new Tuple<SiteMapNode, SiteMapNodeType>(current, SiteMapNodeType.Current));

			current = current.ParentNode;

			while (current != null)
			{
				var parent = current.ParentNode;

				path.Push(new Tuple<SiteMapNode, SiteMapNodeType>(
					current,
					parent == null ? SiteMapNodeType.Root : SiteMapNodeType.Parent));

				current = parent;
			}

			var nodes = takeLast != null ? path.Skip(Math.Max(0, path.Count() - takeLast.Value)) : path;

			return nodes.ToList();
		}

		public static string SiteMapState(this HtmlHelper html)
		{
			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			var current = portalViewContext.CurrentSiteMapNode;

			return current == null
				? null
				: string.Join(":", new[] { current.Url }.Concat(portalViewContext.CurrentSiteMapNodeAncestors.Select(e => e.Url)));
		}

		public static IHtmlString Breadcrumb(this HtmlHelper html, string url = null, int? takeLast = null)
		{
			return Breadcrumb(() => url == null ? html.SiteMapPath(takeLast) : html.SiteMapPath(url, takeLast));
		}

		private static IHtmlString Breadcrumb(Func<IEnumerable<Tuple<SiteMapNode, SiteMapNodeType>>> getSiteMapPath)
		{
			var path = getSiteMapPath().ToList();

			if (!path.Any())
			{
				return null;
			}

			var items = new StringBuilder();

			foreach (var node in path)
			{
				var li = new TagBuilder("li");

				if (node.Item2 == SiteMapNodeType.Current)
				{
					li.AddCssClass("active");
					li.SetInnerText(node.Item1.Title);
				}
				else
				{
					var breadCrumbUrl = node.Item1.Url;

					if (ContextLanguageInfo.IsCrmMultiLanguageEnabledInWebsite(PortalContext.Current.Website)
						&& ContextLanguageInfo.DisplayLanguageCodeInUrl)
					{
						breadCrumbUrl = ContextLanguageInfo
							.PrependLanguageCode(ApplicationPath.Parse(node.Item1.Url))
							.AbsolutePath;
					}

					var a = new TagBuilder("a");
					
					a.Attributes["href"] = breadCrumbUrl;
					a.SetInnerText(node.Item1.Title);

					li.InnerHtml += a.ToString();
				}

				items.AppendLine(li.ToString());
			}

			var ul = new TagBuilder("ul");

			ul.AddCssClass("breadcrumb");
			ul.InnerHtml += items.ToString();

			return new HtmlString(ul.ToString());
		}

		public static IEnumerable<SiteMapNode> CurrentWebLinkChildNodes(this HtmlHelper html, string webLinkSetName, IEnumerable<string> entityLogicalNamesToExclude = null)
		{
			var webLinkSet = PortalExtensions.GetPortalViewContext(html).WebLinks.Select(webLinkSetName);

			return webLinkSet == null
				? Enumerable.Empty<SiteMapNode>()
				: CurrentWebLinkChildNodes(html, webLinkSet, entityLogicalNamesToExclude);
		}

		public static IEnumerable<SiteMapNode> CurrentWebLinkChildNodes(this HtmlHelper html, IWebLinkSet webLinkSet, IEnumerable<string> entityLogicalNamesToExclude = null)
		{
			if (webLinkSet == null)
			{
				throw new ArgumentNullException("webLinkSet");
			}

			var currentWebLink = webLinkSet.WebLinks.FirstOrDefault(e =>
				e.Url != null
				&& !html.IsRootSiteMapNode(e.Url)
				&& ((html.IsCurrentSiteMapNode(e.Url) && html.IsFirstGenerationParentSiteMapNode(e.Url))
					|| html.IsAncestorSiteMapNode(e.Url, true)));

			return currentWebLink == null
				? Enumerable.Empty<SiteMapNode>()
				: entityLogicalNamesToExclude == null
					? html.SiteMapChildNodes(currentWebLink.Url)
					: html.SiteMapChildNodes(currentWebLink.Url).Where(e =>
					{
						var entityNode = e as CrmSiteMapNode;

						if (entityNode == null)
						{
							return true;
						}

						return !entityLogicalNamesToExclude.Any(entityNode.HasCrmEntityName);
					});
		}
	}
}
