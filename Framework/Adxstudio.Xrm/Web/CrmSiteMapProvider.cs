/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// A <see cref="SiteMapProvider"/> for navigating portal <see cref="Entity"/> hierarchies.
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <siteMap enabled="true" defaultProvider="Xrm">
	///    <providers>
	///     <add
	///      name="Xrm"
	///      type="Adxstudio.Xrm.Web.CrmSiteMapProvider"
	///      securityTrimmingEnabled="true"
	///      portalName="Xrm" [Microsoft.Xrm.Portal.Configuration.PortalContextElement]
	///     />
	///    </providers>
	///   </siteMap>
	///  </system.web>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class CrmSiteMapProvider : Microsoft.Xrm.Portal.Web.CrmSiteMapProvider
	{
		public override SiteMapNode CurrentNode
		{
			get
			{
				if (IsPageless(HttpContext.Current))
				{
					// short circuit to the root node for flagged routes
					return this.RootNode;
				}

				return base.CurrentNode;
			}
		}

		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			var clientUrl = ExtractClientUrlFromRawUrl(rawUrl);

			TraceInfo("FindSiteMapNode({0})", clientUrl.PathWithQueryString);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			// Find any possible SiteMarkerRoutes (or other IPortalContextRoutes) that match this path.
			var contextPath = RouteTable.Routes.GetPortalContextPath(portal, clientUrl.Path) ?? clientUrl.Path;

			// If the URL matches a web page, try to look up that page and return a node.
			var pageMappingResult = UrlMapping.LookupPageByUrlPath(context, website, contextPath);

			if (pageMappingResult.Node != null && pageMappingResult.IsUnique)
			{
				return GetAccessibleNodeOrAccessDeniedNode(context, pageMappingResult.Node);
			}
			else if (!pageMappingResult.IsUnique)
			{
				return GetNotFoundNode();
			}

			// If the URL matches a web file, try to look up that file and return a node.
			var file = UrlMapping.LookupFileByUrlPath(context, website, clientUrl.Path);

			if (file != null)
			{
				return GetAccessibleNodeOrAccessDeniedNode(context, file);
			}

			// If there is a pageid Guid on the querystring, try to look up a web page by
			// that ID and return a node.
			Guid pageid;

			if (TryParseGuid(clientUrl.QueryString["pageid"], out pageid))
			{
				var foundPage = context.CreateQuery("adx_webpage")
					.FirstOrDefault(wp => wp.GetAttributeValue<Guid>("adx_webpageid") == pageid
						&& wp.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference());

				if (foundPage != null)
				{
					return GetAccessibleNodeOrAccessDeniedNode(context, foundPage);
				}
			}

			// If the above lookups failed, try find a node in any other site map providers.
			foreach (SiteMapProvider subProvider in SiteMap.Providers)
			{
				// Skip this provider if it is the same as this one.
				if (subProvider.Name == Name) continue;
				
				var node = subProvider.FindSiteMapNode(clientUrl.PathWithQueryString);

				if (node != null)
				{
					return node;
				}
			}

			return GetNotFoundNode();
		}

		public override SiteMapNode GetParentNode(SiteMapNode node)
		{
			TraceInfo("GetParentNode({0})", node.Key);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var pageMappingResult = UrlMapping.LookupPageByUrlPath(context, website, node.Url);

			if (pageMappingResult.Node == null)
			{
				return null;
			}

			var parentPage = pageMappingResult.Node.GetRelatedEntity(context, "adx_webpage_webpage", EntityRole.Referencing);

			if (parentPage == null)
			{
				return null;
			}

			return GetAccessibleNodeOrAccessDeniedNode(context, parentPage);
		}

		protected override void AddNode(SiteMapNode node)
		{
			AddNode(node, base.AddNode);
		}

		protected override void AddNode(SiteMapNode node, SiteMapNode parentNode)
		{
			AddNode(node, n => base.AddNode(n, parentNode));
		}

		protected void AddNode(SiteMapNode node, Action<SiteMapNode> add)
		{
			if (!(node is CrmSiteMapNode))
			{
				add(node);

				return;
			}

			Uri absoluteUri;
			var encoded = false;

			if (Uri.TryCreate(node.Url, UriKind.Absolute, out absoluteUri))
			{
				node.Url = HttpContext.Current.Server.UrlEncode(node.Url);
				encoded = true;
			}

			add(node);

			if (encoded)
			{
				node.Url = HttpContext.Current.Server.UrlDecode(node.Url);
			}
		}

		public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
		{
			TraceInfo("GetChildNodes({0})", node.Key);

			var children = new List<SiteMapNode>();

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			// Shorcuts do not have children, may have the same Url as a web page.
			if (IsShortcutNode(node))
			{
				return new SiteMapNodeCollection();
			}

			var pageMappingResult = UrlMapping.LookupPageByUrlPath(context, website, node.Url);

			// If the node URL is that of a web page...
			if (pageMappingResult.Node != null && pageMappingResult.IsUnique)
			{
				var childEntities = context.GetChildPages(pageMappingResult.Node).Union(context.GetChildFiles(pageMappingResult.Node)).Union(context.GetChildShortcuts(pageMappingResult.Node));

				foreach (var entity in childEntities)
				{
					try
					{
						if (entity.LogicalName == "adx_shortcut")
						{
							var targetNode = GetShortcutTargetNode(context, entity);
							var shortcutChildNode = GetShortcutCrmNode(context, entity, targetNode);

							if (shortcutChildNode != null && ChildNodeValidator.Validate(context, shortcutChildNode))
							{
								children.Add(shortcutChildNode);
							}
						}
						else
						{
							var childNode = GetNode(context, entity);

							if (childNode != null && ChildNodeValidator.Validate(context, childNode))
							{
								children.Add(childNode);
							}
						}
					}
					catch (Exception e)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Exception creating child node for node child entity [{0}:{1}]: {2}", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id, e.ToString()));

						continue;
					}
				}
			}

			// Append values from other site map providers.
			foreach (SiteMapProvider subProvider in SiteMap.Providers)
			{
				// Skip this provider if it is the same as this one.
				if (subProvider.Name == Name) continue;

				var subProviderChildNodes = subProvider.GetChildNodes(node);

				if (subProviderChildNodes == null) continue;

				foreach (SiteMapNode childNode in subProviderChildNodes)
				{
					children.Add(childNode);
				}
			}

			children.Sort(new SiteMapNodeDisplayOrderComparer());

			return new SiteMapNodeCollection(children.ToArray());
		}

		protected CrmSiteMapNode GetShortcutCrmNode(OrganizationServiceContext serviceContext, Entity shortcut, CrmSiteMapNode targetNode)
		{
			if (shortcut == null)
			{
				throw new ArgumentNullException("shortcut");
			}

			if (shortcut.LogicalName != "adx_shortcut")
			{
				throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(shortcut.Id, shortcut.GetType().FullName), "shortcut");
			}

			var url = !string.IsNullOrEmpty(shortcut.GetAttributeValue<string>("adx_externalurl"))
				? shortcut.GetAttributeValue<string>("adx_externalurl")
				: serviceContext.GetUrl(shortcut);

			// Node does not have a valid URL, and should be filtered out of the sitemap.
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}

			var shortcutDescription = shortcut.GetAttributeValue<string>("adx_description");

			var description = !string.IsNullOrEmpty(shortcutDescription)
				? shortcutDescription
				: targetNode != null
					? targetNode.Description
					: string.Empty;

			return new CrmSiteMapNode(
				this,
				url,
				url,
				shortcut.GetAttributeValue<string>("adx_title") ?? shortcut.GetAttributeValue<string>("adx_name"),
				description,
				targetNode != null ? targetNode.RewriteUrl : url,
				shortcut.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
				shortcut);
		}

		private CrmSiteMapNode GetShortcutTargetNode(OrganizationServiceContext context, Entity entity)
		{
			if (!string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_externalurl")))
			{
				return null;
			}

			var targetWebPage = entity.GetRelatedEntity(context, "adx_webpage_shortcut");

			if (targetWebPage != null)
			{
				return GetNode(context, targetWebPage);
			}

			var targetSurvey = entity.GetRelatedEntity(context, "adx_survey_shortcut");

			if (targetSurvey != null)
			{
				return GetNode(context, targetSurvey);
			}

			var targetWebFile = entity.GetRelatedEntity(context, "adx_webfile_shortcut");

			if (targetWebFile != null)
			{
				return GetNode(context, targetWebFile);
			}

			var targetEvent = entity.GetRelatedEntity(context, "adx_event_shortcut");

			if (targetEvent != null)
			{
				return GetNode(context, targetEvent);
			}

			var targetForum = entity.GetRelatedEntity(context, "adx_communityforum_shortcut");

			if (targetForum != null)
			{
				return GetNode(context, targetForum);
			}

			return null;
		}

		private static readonly Regex _notFoundServerRedirectPattern = new Regex(@"404;(?<ClientUrl>.*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		protected static UrlBuilder ExtractClientUrlFromRawUrl(string rawUrl)
		{
			var rawUrlBuilder = new UrlBuilder(rawUrl);

			var notFoundServerRedirectMatch = _notFoundServerRedirectPattern.Match(rawUrl);

			if (!notFoundServerRedirectMatch.Success)
			{
				return rawUrlBuilder;
			}

			var clientUrl = new UrlBuilder(notFoundServerRedirectMatch.Groups["ClientUrl"].Value);

			if (!string.Equals(rawUrlBuilder.Host, clientUrl.Host, StringComparison.OrdinalIgnoreCase))
			{
				throw new SecurityException("rawUrl host {0} and server internal redirect URL host {1} do not match.".FormatWith(rawUrlBuilder.Host, clientUrl.Host));
			}

			return clientUrl;
		}

		protected static bool IsShortcutNode(SiteMapNode node)
		{
			var crmNode = node as CrmSiteMapNode;

			return crmNode != null && crmNode.Entity != null && crmNode.Entity.LogicalName == "adx_shortcut";
		}

		/// <summary>
		/// Gets whether the HttpContext page is "pageless", i.e. it's a route that doesn't exist in the sitemap.
		/// ex: SignIn, Register pages. They are defined by MVC routes with property "pageless" = true.
		/// </summary>
		/// <param name="context">The HttpContext.</param>
		/// <returns>Whether the context page is "pageless".</returns>
		public static bool IsPageless(HttpContext context)
		{
			var wrappedContext = new HttpContextWrapper(context);
			var routeData = RouteTable.Routes.GetRouteData(wrappedContext);
			var pageless = routeData != null ? routeData.Values["pageless"] as bool? : null;
			return pageless.GetValueOrDefault(false);
		}

		/// <summary>
		/// Get the value of nonMVC flag from HttpContext/Route Data, if set
		/// </summary>
		/// <param name="context">Http Context</param>
		/// <returns>Return nonMVC</returns>
		public static bool IsNonMVC(HttpContext context)
		{
			var wrappedContext = new HttpContextWrapper(context);
			var routeData = RouteTable.Routes.GetRouteData(wrappedContext);
			var nonMVC = routeData != null ? routeData.Values["nonMVC"] as bool? : null;
			return nonMVC.GetValueOrDefault(false);
		}
	}
}
