/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Providers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;
using Filter = Adxstudio.Xrm.Services.Query.Filter;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// HTML helpers related to portal styles (CSS).
	/// </summary>
	public static class StyleExtensions
	{
		/// <summary>
		/// Renders HTML link elements, referencing stylesheets managed within portal website data.
		/// </summary>
		/// <param name="html">
		/// Extension method target, provides support for HTML rendering and access to view context/data.
		/// </param>
		/// <param name="except">
		/// If specified, all paths except those contained within this collection.
		/// </param>
		/// <remarks>
		/// The stylesheets rendered by this method are any Web Files (adx_webfile) with a Partial URL
		/// (adx_partialurl) ending in ".css", and that are children of the current site map node, or
		/// children of any ancestor pages of the current node.
		/// </remarks>
		public static IHtmlString ContentStyles(this HtmlHelper html, IEnumerable<string> except = null)
		{
			return ContentStyles(html, except, null);
		}

		/// <summary>
		/// Renders HTML link elements, referencing culture-specific stylesheets managed within portal website data.
		/// </summary>
		/// <param name="html">
		/// Extension method target, provides support for HTML rendering and access to view context/data.
		/// </param>
		/// <param name="lcid">
		/// Specified culture to render culture-specific stylesheet.
		/// </param>
		public static IHtmlString LocalizedContentStyle(this HtmlHelper html, int lcid)
		{
			IDictionary<string, string> only = new Dictionary<string, string>();

			switch (lcid)
			{
				case LocaleIds.ChineseTraditional:
					only.Add("zh-TW.css", "~/css/lang/zh-TW.css");
					break;
				case LocaleIds.Japanese:
					only.Add("ja-JP.css", "~/css/lang/ja-JP.css");
					break;
				case LocaleIds.Korean:
					only.Add("ko-KR.css", "~/css/lang/ko-KR.css");
					break;
				case LocaleIds.ChineseSimplified:
					only.Add("zh-CN.css", "~/css/lang/zh-CN.css");
					break;
				case LocaleIds.ChineseHongKong:
					only.Add("zh-TW.css", "~/css/lang/zh-TW.css");
					break;
				case LocaleIds.Thai:
					only.Add("th-TH.css", "~/css/lang/th-TH.css");
					break;
				case LocaleIds.Hindi:
					only.Add("hi-IN.css", "~/css/lang/hi-IN.css");
					break;
				default:
					break;
			}
			return ContentStyles(html, null, only);
		}

		/// <summary>
		/// Renders HTML link elements, referencing stylesheets managed within portal website data.
		/// </summary>
		/// <param name="html">
		/// Extension method target, provides support for HTML rendering and access to view context/data.
		/// </param>
		/// <param name="only">
		/// If specified, only the paths contained within this dictionary will be linked to. The key is
		/// the path that must correspond to the adx_partialurl of the linked adx_webfile. The value
		/// is the path that will be linked to if that adx_webfile does not exist.
		/// </param>
		/// <remarks>
		/// The stylesheets rendered by this method are any Web Files (adx_webfile) with a Partial URL
		/// (adx_partialurl) ending in ".css", and that are children of the current site map node, or
		/// children of any ancestor pages of the current node.
		/// </remarks>
		public static IHtmlString ContentStyles(this HtmlHelper html, IDictionary<string, string> only)
		{
			return ContentStyles(html, null, only);
		}

		private static IHtmlString ContentStyles(HtmlHelper html, IEnumerable<string> except, IEnumerable<KeyValuePair<string, string>> only)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			var portalViewContext = PortalExtensions.GetPortalViewContext(html);

			var siteMapProvider = portalViewContext.SiteMapProvider as ContentMapCrmSiteMapProvider;

			var node = siteMapProvider == null
				? null
				: siteMapProvider.CurrentNode ?? siteMapProvider.RootNode;

			if (siteMapProvider != null && (node == null || (((CrmSiteMapNode)node).StatusCode == System.Net.HttpStatusCode.Forbidden) && node.Equals(node.RootNode)))
			{
				// If home root node has been secured then we need to retrieve the root without security validation in order to get the content stylesheets to be referenced.
				node = siteMapProvider.FindSiteMapNodeWithoutSecurityValidation("/");
			}

			var hrefs = ContentStyles(portalViewContext, node, html.ViewContext.TempData, except, only)
				.Distinct()
				.ToArray();

			if (!hrefs.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End: No content styles found.");

				return null;
			}

			var output = new StringBuilder();

			foreach (var href in hrefs)
			{
				var link = new TagBuilder("link");

				link.Attributes["rel"] = "stylesheet";
				link.Attributes["href"] = href;

				output.AppendLine(link.ToString(TagRenderMode.SelfClosing));
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return new HtmlString(output.ToString());
		}

		private static IEnumerable<string> ContentStyles(IPortalViewContext portalViewContext, SiteMapNode node, IDictionary<string, object> cache, IEnumerable<string> except, IEnumerable<KeyValuePair<string, string>> only)
		{
			var styles = ContentStyles(portalViewContext, node, cache).ToArray();

			string[] allDisplayModes;
			string[] availableDisplayModes;

			if (TryGetDisplayModes(out allDisplayModes, out availableDisplayModes))
			{
				var partialUrlTrie = new FileNameTrie(styles);
				var displayModeComparer = new DisplayModeComparer(availableDisplayModes);

				var groups = GetDisplayModeFileGroups(partialUrlTrie, allDisplayModes);

				if (only != null)
				{
					return only.Select(o =>
					{
						var extensionless = Path.GetFileNameWithoutExtension(o.Key);

						var matchGroup = groups.FirstOrDefault(s => string.Equals(s.Prefix, extensionless, StringComparison.OrdinalIgnoreCase));

						if (matchGroup == null)
						{
							return o.Value;
						}

						var file = matchGroup.Where(f => availableDisplayModes.Contains(f.DisplayModeId))
							.OrderBy(f => f, displayModeComparer)
							.FirstOrDefault();

						return file == null ? o.Value : file.Name.Item1;
					});
				}

				if (except != null)
				{
					return groups
						.Where(group => !except.Any(e => string.Equals(Path.GetFileNameWithoutExtension(e), group.Prefix, StringComparison.OrdinalIgnoreCase)))
						.Select(group => group.Where(f => availableDisplayModes.Contains(f.DisplayModeId))
							.OrderBy(f => f, displayModeComparer)
							.FirstOrDefault())
						.Where(f => f != null)
						.Select(f => f.Name.Item1);
				}

				return groups
					.Select(group => group.Where(f => availableDisplayModes.Contains(f.DisplayModeId))
						.OrderBy(f => f, displayModeComparer)
						.FirstOrDefault())
					.Where(f => f != null)
					.Select(f => f.Name.Item1);
			}

			if (only != null)
			{
				return only.Select(o =>
				{
					var match = styles.FirstOrDefault(s => string.Equals(s.Item2, o.Key, StringComparison.InvariantCultureIgnoreCase));

					return match == null ? o.Value : match.Item1;
				});
			}

			if (except != null)
			{
				return styles
					.Where(s => !except.Any(e => string.Equals(e, s.Item2, StringComparison.InvariantCultureIgnoreCase)))
					.Select(s => s.Item1);
			}

			return styles.Select(s => s.Item1);
		}

		private static bool TryGetDisplayModes(out string[] allDisplayModes, out string[] availableDisplayModes)
		{
			allDisplayModes = null;
			availableDisplayModes = null;

			var displayModeProvider = DisplayModeProvider.Instance;

			if (displayModeProvider == null)
			{
				return false;
			}

			allDisplayModes = displayModeProvider.Modes.Select(mode => mode.DisplayModeId).ToArray();

			var httpContext = HttpContext.Current;

			if (httpContext == null || httpContext.Request.RequestContext == null || httpContext.Request.RequestContext.HttpContext == null)
			{
				return false;
			}

			availableDisplayModes = displayModeProvider
				.GetAvailableDisplayModesForContext(httpContext.Request.RequestContext.HttpContext, null)
				.Select(mode => mode.DisplayModeId)
				.ToArray();

			return true;
		}

		private static IEnumerable<Tuple<string, string, int>> ContentStyles(IPortalViewContext portalViewContext, SiteMapNode node, IDictionary<string, object> cache)
		{
			if (node == null)
			{
				return Enumerable.Empty<Tuple<string, string, int>>();
			}

			var cacheKey = "{0}:ContentStyles:{1}".FormatWith(typeof(StyleExtensions).FullName, node.Url);

			object cached;

			if (cache.TryGetValue(cacheKey, out cached) && cached is IEnumerable<Tuple<string, string, int>>)
			{
				return cached as IEnumerable<Tuple<string, string, int>>;
			}

			// Get all adx_webpages in the site map path, starting from and including the current entity. These are
			// potential containers for content-managed stylesheets.
			var path = new List<EntityReference>();
			var current = node;

			while (current != null)
			{
				var entityNode = current as CrmSiteMapNode;

				if (entityNode != null && entityNode.HasCrmEntityName("adx_webpage"))
				{
					path.Add(entityNode.Entity.ToEntityReference());
				}

				current = current.ParentNode;
			}

			if (!path.Any())
			{
				return Enumerable.Empty<Tuple<string, string, int>>();
			}

			var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(portalViewContext.PortalName);

			var styles = contentMapProvider == null
				? ContentStyles(portalViewContext, path)
				: ContentStyles(portalViewContext, path, contentMapProvider);

			cache[cacheKey] = styles;

			return styles;
		}

		private static IEnumerable<Tuple<string, string, int>> ContentStyles(IPortalViewContext portalViewContext, List<EntityReference> path)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Getting content styles using query.");

			var serviceContext = portalViewContext.CreateServiceContext();

			var parentPageConditions = path.Select(reference => new Condition("adx_parentpageid", ConditionOperator.Equal, reference.Id)).ToList();

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webfile")
				{
					Filters = new List<Filter>
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new List<Condition>
							{
								new Condition("adx_websiteid", ConditionOperator.Equal, portalViewContext.Website.EntityReference.Id),
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_partialurl", ConditionOperator.EndsWith, ".css")
							},
							Filters = new List<Filter> { new Filter { Type = LogicalOperator.Or, Conditions = parentPageConditions } }
						}
					}
				}
			};

			return serviceContext.RetrieveMultiple(fetch).Entities
				.Select(e => new
				{
					PathOffset = path.FindIndex(pathItem => pathItem.Equals(e.GetAttributeValue<EntityReference>("adx_parentpageid"))),
					DisplayOrder = e.GetAttributeValue<int?>("adx_displayorder").GetValueOrDefault(0),
					Url = serviceContext.GetUrl(e),
					PartialUrl = e.GetAttributeValue<string>("adx_partialurl")
				})
				.Where(e => !string.IsNullOrEmpty(e.Url))
				.OrderByDescending(e => e.PathOffset)
				.ThenBy(e => e.DisplayOrder)
				.Select(e => new Tuple<string, string, int>(e.Url, e.PartialUrl, e.PathOffset))
				.ToArray();
		}

		private static IEnumerable<Tuple<string, string, int>> ContentStyles(IPortalViewContext portalViewContext, IEnumerable<EntityReference> path, IContentMapProvider contentMapProvider)
		{
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalViewContext.PortalName).GetDependency<IContentMapEntityUrlProvider>();

			if (urlProvider == null)
			{
				return Enumerable.Empty<Tuple<string, string, int>>();
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Getting content styles using Content Map.");

			var cssWebFileNodes = contentMapProvider.Using(map => path.SelectMany((pathItem, pathOffset) =>
			{
				WebPageNode webPage;

				// Guard against null reference exception on webPage.WebFiles. This might happen if WebPage is a translated content, but for whatever reason has no root web page.
				if (!map.TryGetValue(pathItem, out webPage) || webPage.WebFiles == null)
				{
					return Enumerable.Empty<Tuple<int, int, string, string>>();
				}

				return webPage.WebFiles
					.Where(e => e.PartialUrl != null && e.PartialUrl.EndsWith(".css"))
					.Select(e => new Tuple<int, int, string, string>(
						pathOffset,
						e.DisplayOrder.GetValueOrDefault(0),
						urlProvider.GetUrl(map, e),
						e.PartialUrl));
			}));

			return cssWebFileNodes
				.Where(e => !string.IsNullOrEmpty(e.Item3))
				.OrderByDescending(e => e.Item1)
				.ThenBy(e => e.Item2)
				.Select(e => new Tuple<string, string, int>(e.Item3, e.Item4, e.Item1))
				.ToArray();
		}

		private static IEnumerable<DisplayModeFileGroup> GetDisplayModeFileGroups(IEnumerable<FileNameTrie.Node> nodes, string[] displayModes)
		{
			var groups = new List<DisplayModeFileGroup>();

			GetDisplayModeFileGroups(string.Empty, nodes, groups, displayModes);

			return groups;
		}

		private static void GetDisplayModeFileGroups(string prefix, IEnumerable<FileNameTrie.Node> nodes, ICollection<DisplayModeFileGroup> groups, string[] displayModeIds)
		{
			var group = new DisplayModeFileGroup(prefix);

			foreach (var node in nodes)
			{
				if (node.Key == FileNameTrie.TerminalNodeKey)
				{
					group.Add(new DisplayModeFile(node.Value));

					continue;
				}

				FileNameTrie.Node terminalNode;

				if (node.TryGetTerminalNode(out terminalNode))
				{
					if (prefix == string.Empty)
					{
						groups.Add(new DisplayModeFileGroup(node.Key)
						{
							new DisplayModeFile(terminalNode.Value)
						});

						continue;
					}

					if (displayModeIds.Contains(node.Key, StringComparer.OrdinalIgnoreCase))
					{
						group.Add(new DisplayModeFile(terminalNode.Value, node.Key));

						continue;
					}
				}

				var nodePrefix = prefix == string.Empty
					? node.Key
					: "{0}.{1}".FormatWith(prefix, node.Key);

				GetDisplayModeFileGroups(nodePrefix, node.Children, groups, displayModeIds);
			}

			if (group.Any())
			{
				groups.Add(group);
			}
		}

		private class FileNameTrie : IEnumerable<FileNameTrie.Node>
		{
			public const string TerminalNodeKey = "";

			private readonly ICollection<Node> _rootNodes = new List<Node>();

			public FileNameTrie() { }

			public FileNameTrie(IEnumerable<Tuple<string, string, int>> fileNames)
			{
				foreach (var fileName in fileNames)
				{
					Add(fileName);
				}
			}

			public void Add(Tuple<string, string, int> fileName)
			{
				var extensionless = Path.GetFileNameWithoutExtension(fileName.Item2);

				if (string.IsNullOrEmpty(extensionless))
				{
					throw new ArgumentException("Unable to get file name without extension.", "fileName");
				}

				Add(fileName, extensionless.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries), _rootNodes);
			}

			public IEnumerator<Node> GetEnumerator()
			{
				return _rootNodes.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private void Add(Tuple<string, string, int> fileName, string[] fileNameParts, ICollection<Node> nodes)
			{
				if (!fileNameParts.Any())
				{
					nodes.Add(new Node(TerminalNodeKey, fileName));

					return;
				}

				var currentPart = fileNameParts.First();
				var matchingNode = nodes.SingleOrDefault(node => string.Equals(node.Key, currentPart, StringComparison.OrdinalIgnoreCase));

				if (matchingNode == null)
				{
					var newNode = new Node(currentPart);

					nodes.Add(newNode);

					Add(fileName, fileNameParts.Skip(1).ToArray(), newNode.Children);
				}
				else
				{
					Add(fileName, fileNameParts.Skip(1).ToArray(), matchingNode.Children);
				}
			}

			public class Node
			{
				public Node(string key, Tuple<string, string, int> value = null)
				{
					if (key == null) throw new ArgumentNullException("key");

					Key = key;
					Value = value;
					Children = new List<Node>();
				}

				public string Key { get; private set; }

				public Tuple<string, string, int> Value { get; private set; }

				public ICollection<Node> Children { get; private set; }

				public bool TryGetTerminalNode(out Node terminalNode)
				{
					terminalNode = null;

					if (Children.Count != 1)
					{
						return false;
					}

					terminalNode = Children.First();

					return terminalNode.Key == TerminalNodeKey;
				}
			}
		}

		public class DisplayModeFileGroup : IGrouping<string, DisplayModeFile>
		{
			private readonly ICollection<DisplayModeFile> _files = new List<DisplayModeFile>();

			public DisplayModeFileGroup(string prefix)
			{
				Prefix = prefix;
			}

			public string Key
			{
				get { return Prefix; }
			}

			public string Prefix { get; private set; }

			public void Add(DisplayModeFile file)
			{
				if (file == null) throw new ArgumentNullException("file");

				_files.Add(file);
			}

			public IEnumerator<DisplayModeFile> GetEnumerator()
			{
				return _files.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class DisplayModeFile
		{
			public DisplayModeFile(Tuple<string, string, int> name, string displayModeId = "")
			{
				Name = name;
				DisplayModeId = displayModeId;
			}

			public string DisplayModeId { get; private set; }

			public Tuple<string, string, int> Name { get; private set; }
		}

		private class DisplayModeComparer : IComparer<DisplayModeFile>
		{
			private readonly List<string> _displayModes;

			public DisplayModeComparer(IEnumerable<string> displayModes)
			{
				_displayModes = displayModes.ToList();
			}

			public int Compare(DisplayModeFile x, DisplayModeFile y)
			{
				if (x == null) throw new ArgumentNullException("x");
				if (y == null) throw new ArgumentNullException("y");

				if (x.Name.Item3 > y.Name.Item3)
				{
					return 1;
				}

				if (x.Name.Item3 < y.Name.Item3)
				{
					return -1;
				}

				var xIndex = _displayModes.FindIndex(displayMode => string.Equals(displayMode, x.DisplayModeId, StringComparison.OrdinalIgnoreCase));
				var yIndex = _displayModes.FindIndex(displayMode => string.Equals(displayMode, y.DisplayModeId, StringComparison.OrdinalIgnoreCase));

				return xIndex.CompareTo(yIndex);
			}
		}
	}
}
