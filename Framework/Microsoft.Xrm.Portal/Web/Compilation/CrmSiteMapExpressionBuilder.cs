/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Web;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	/// <summary>
	/// Expression builder for getting various values from the <see cref="CrmSiteMapProvider"/>, given
	/// one of several supported node selectors.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="CrmSiteMap" type="Microsoft.Xrm.Portal.Web.Compilation.CrmSiteMapExpressionBuilder, Microsoft.Xrm.Portal"/>
	///    </expressionBuilders>
	///   </compilation>
	///  </system.web>
	/// 
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ CrmSiteMap: (Current|SiteMarker={siteMarker}) [, Return={Node|Entity|Url}] [, Eval={expression}] [, Format={format string}] [, Portal={portal name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ CrmSiteMap: Current, Eval=Entity %>'/>
	/// <asp:Label runat="server" Text='<%$ CrmSiteMap: SiteMarker=Profile, Eval=Url, Format=-{0}- %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class CrmSiteMapExpressionBuilder : CrmExpressionBuilder<CrmSiteMapExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				var siteMarker = arguments.GetValueByIndexOrName(0, "SiteMarker");

				if (string.IsNullOrEmpty(siteMarker))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "(Current|SiteMarker={siteMarker}) [, Return={Node|Entity|Url}] [, Eval={expression}] [, Format={format string}] [, Portal={portal name}]");
				}

				var returnType = arguments.GetValueByIndexOrName(1, "Return");
				var portalName = arguments.GetValueByIndexOrName(4, "Portal");

				var node = GetSiteMapNode(siteMarker, portalName);

				if (node == null)
				{
					return null;
				}

				if (returnType != null)
				{
					if (string.Equals(returnType, "Entity", StringComparison.InvariantCulture))
					{
						return node.Entity;
					}

					if (string.Equals(returnType, "Url", StringComparison.InvariantCulture))
					{
						return node.Url;
					}

					return node;
				}

				var eval = arguments.GetValueByIndexOrName(2, "Eval");
				var format = arguments.GetValueByIndexOrName(3, "Format");

				return GetEvalData(node, null, eval, format, GetReturnType(controlType, propertyName));
			}

			private static CrmSiteMapNode GetSiteMapNode(string siteMarker, string portalName)
			{
				if (!SiteMap.Enabled)
				{
					return null;
				}

				if (string.Equals(siteMarker, "Current", StringComparison.InvariantCulture))
				{
					return SiteMap.CurrentNode as CrmSiteMapNode;
				}

				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
				var context = portal.ServiceContext;
				var website = portal.Website;

				var page = context.GetPageBySiteMarkerName(website, siteMarker);

				if (page == null)
				{
					return null;
				}

				var pageUrl = context.GetUrl(page);

				if (pageUrl == null)
				{
					return null;
				}

				return SiteMap.Provider.FindSiteMapNode(pageUrl) as CrmSiteMapNode;
			}
		}
	}
}
