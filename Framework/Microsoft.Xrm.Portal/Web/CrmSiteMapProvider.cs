/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
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
	///      type="Microsoft.Xrm.Portal.Web.CrmSiteMapProvider"
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
	public class CrmSiteMapProvider : CrmSiteMapProviderBase
	{
		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			var clientUrl = ExtractClientUrlFromRawUrl(rawUrl);

			// For the case when we're relying on the redirect-404-to-dummy-Default.aspx trick for
			// URL routing, normalize this path to '/'.
			if (clientUrl.Path.Equals("/Default.aspx", StringComparison.OrdinalIgnoreCase))
			{
				clientUrl.Path = "/";
			}

			TraceInfo("FindSiteMapNode({0})", clientUrl.PathWithQueryString);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			// If the URL matches a web page, try to look up that page and return a node.
			var page = UrlMapping.LookupPageByUrlPath(context, website, clientUrl.Path);

			if (page != null)
			{
				return GetAccessibleNodeOrAccessDeniedNode(context, page);
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
				var webPagesInCurrentWebsite = website.GetRelatedEntities(context, "adx_website_webpage");

				page = webPagesInCurrentWebsite.Where(wp => wp.GetAttributeValue<Guid>("adx_webpageid") == pageid).FirstOrDefault();

				if (page != null)
				{
					return GetAccessibleNodeOrAccessDeniedNode(context, page);
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

		public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
		{
			TraceInfo("GetChildNodes({0})", node.Key);

			var children = new List<SiteMapNode>();

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var page = UrlMapping.LookupPageByUrlPath(context, website, node.Url);

			// If the node URL is that of a web page...
			if (page != null)
			{
				var childEntities = context.GetChildPages(page).Union(context.GetChildFiles(page));

				// Add the (valid) child pages and files of that page to the children we will return.
				foreach (var entity in childEntities)
				{
					var childNode = GetNode(context, entity);

					if (ChildNodeValidator.Validate(context, childNode))
					{
						children.Add(childNode);
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

		public override SiteMapNode GetParentNode(SiteMapNode node)
		{
			TraceInfo("GetParentNode({0})", node.Key);

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var page = UrlMapping.LookupPageByUrlPath(context, website, node.Url);

			if (page == null)
			{
				return null;
			}

			var parentPage = page.GetRelatedEntity(context, "adx_webpage_webpage", EntityRole.Referencing);

			if (parentPage == null)
			{
				return null;
			}

			return GetAccessibleNodeOrAccessDeniedNode(context, parentPage);
		}

		protected override SiteMapNode GetRootNodeCore()
		{
			TraceInfo("GetRootNodeCore()");

			var portal = PortalContext;
			var context = portal.ServiceContext;
			var website = portal.Website;

			var homePage = context.GetPageBySiteMarkerName(website, "Home");

			if (homePage == null)
			{
				throw new InvalidOperationException(@"Website ""{0}"" must have a web page with the site marker ""Home"".".FormatWith(website.GetAttributeValue<string>("adx_name")));
			}

			return GetAccessibleNodeOrAccessDeniedNode(context, homePage);
		}

		protected override CrmSiteMapNode GetNode(OrganizationServiceContext context, Entity entity)
		{
			return GetNode(context, entity, HttpStatusCode.OK);
		}

		protected CrmSiteMapNode GetNode(OrganizationServiceContext context, Entity entity, HttpStatusCode statusCode)
		{
			entity.ThrowOnNull("entity");

			var entityName = entity.LogicalName;

			if (entityName == "adx_webfile")
			{
				return GetWebFileNode(context, entity, statusCode);
			}

			if (entityName == "adx_webpage")
			{
				return GetWebPageNode(context, entity, statusCode);
			}

			throw new ArgumentException("Entity {0} ({1}) is not of a type supported by this provider.".FormatWith(entity.Id, entity.GetType().FullName), "entity");
		}

		private CrmSiteMapNode GetWebFileNode(OrganizationServiceContext context, Entity file, HttpStatusCode statusCode)
		{
			var contentFormatter = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityContentFormatter>(GetType().FullName) ?? new PassthroughCrmEntityContentFormatter();

			var url = context.GetUrl(file);
			var name = contentFormatter.Format(file.GetAttributeValue<string>("adx_name"), file, this);
			var summary = contentFormatter.Format(file.GetAttributeValue<string>("adx_summary"), file, this);

			var fileAttachmentProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityFileAttachmentProvider>();

			var attachmentInfo = fileAttachmentProvider.GetAttachmentInfo(context, file).FirstOrDefault();

			// apply a detached clone of the entity since the SiteMapNode is out of the scope of the current OrganizationServiceContext

			var fileClone = file.Clone(false);

			// If there's no file attached to the webfile, return a NotFound node with no rewrite path.
			if (attachmentInfo == null)
			{
				return new CrmSiteMapNode(
					this,
					url,
					url,
					name,
					summary,
					null,
					file.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow),
					fileClone,
					HttpStatusCode.NotFound);
			}

			return new CrmSiteMapNode(
				this,
				url,
				url,
				name,
				summary,
				attachmentInfo.Url,
				attachmentInfo.LastModified.GetValueOrDefault(DateTime.UtcNow),
				file,
				statusCode);
		}

		private static readonly Regex _notFoundServerRedirectPattern = new Regex(@"404;(?<ClientUrl>.*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		private static UrlBuilder ExtractClientUrlFromRawUrl(string rawUrl)
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
				throw new SecurityException(@"rawUrl host ""{0}"" and server internal redirect URL host ""{1}"" do not match.".FormatWith(rawUrlBuilder.Host, clientUrl.Host));
			}

			return clientUrl;
		}

		public class SiteMapNodeDisplayOrderComparer : IComparer<SiteMapNode>
		{
			public int Compare(SiteMapNode x, SiteMapNode y)
			{
				var crmX = x as CrmSiteMapNode;
				var crmY = y as CrmSiteMapNode;

				// If neither are CrmSiteMapNodes, they are ordered equally.
				if (crmX == null && crmY == null)
				{
					return 0;
				}

				// If x is not a CrmSiteMapNode, and y is, order x after y.
				if (crmX == null)
				{
					return 1;
				}

				// If x is a CrmSiteMapNode, and y is not, order x before y.
				if (crmY == null)
				{
					return -1;
				}

				int? xDisplayOrder;
				
				// Try get a display order value for x.
				try
				{
					xDisplayOrder = crmX.Entity.GetAttributeValue<int?>("adx_displayorder");
				}
				catch
				{
					xDisplayOrder = null;
				}

				int? yDisplayOrder;
				
				// Try get a display order value for y.
				try
				{
					yDisplayOrder = crmY.Entity.GetAttributeValue<int?>("adx_displayorder");
				}
				catch
				{
					yDisplayOrder = null;
				}

				// If neither has a display order, they are ordered equally.
				if (!(xDisplayOrder.HasValue || yDisplayOrder.HasValue))
				{
					return 0;
				}

				// If x has no display order, and y does, order x after y.
				if (!xDisplayOrder.HasValue)
				{
					return 1;
				}

				// If x has a display order, and y does not, order y after x.
				if (!yDisplayOrder.HasValue)
				{
					return -1;
				}

				// If both have display orders, order by the comparison of that value.
				return xDisplayOrder.Value.CompareTo(yDisplayOrder.Value);
			}
		}
	}
}
