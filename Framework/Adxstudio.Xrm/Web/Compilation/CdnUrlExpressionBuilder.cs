/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Compilation;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Compilation
{
	/// <summary>
	/// Expression builder for retrieving CDN based URLs. Add the desired CDN host name as a site setting named "/url/cdn".
	/// </summary>
	/// <remarks>
	/// The "Url" value must begin with "~/cdn/".
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="CdnUrl" type="Adxstudio.Xrm.Web.Compilation.CdnUrlExpressionBuilder, Adxstudio.Xrm"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ CdnUrl: Url=~/cdn/{source URL} [, Name={setting name}] [, Portal={portal name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ CdnUrl: Url=~/cdn/css/site.css %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class CdnUrlExpressionBuilder : CrmExpressionBuilder<CdnUrlExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				var url = arguments.GetValueByIndexOrName(0, "Url");

				if (string.IsNullOrEmpty(url) || !url.Contains("~/cdn/"))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "Url=~/cdn/{source URL} [, Name={setting name}] [, Portal={portal name}]");
				}

				var settingName = arguments.GetValueByIndexOrName(1, "Name") ?? "/url/cdn";
				var portalName = arguments.GetValueByIndexOrName(2, "Portal");

				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
				var context = portal.ServiceContext;
				var website = portal.Website;

				var setting = context.GetSiteSettingByName(website, settingName);

				var returnType = GetReturnType(controlType, propertyName);
				if (returnType.IsA(typeof(Entity))) return setting;
				if (setting == null) return url;

				var format = url.Replace("~/cdn/", "{0}/");

				return GetEvalData(setting, "adx_value", null, format, returnType);
			}
		}
	}

	internal static class NameValueCollectionExtensions
	{
		public static string GetValueByIndexOrName(this NameValueCollection collection, int index, string name)
		{
			if (collection.AllKeys.Contains(name)) return collection[name];
			if (collection.Count > index && collection.AllKeys[index] == "_{0}".FormatWith(index)) return collection[index];
			return null;
		}

		public static string[] GetValuesByIndexOrName(this NameValueCollection collection, int index, string name)
		{
			if (collection.AllKeys.Contains(name)) return collection.GetValues(name);
			if (collection.Count > index && collection.AllKeys[index] == "_{0}".FormatWith(index)) return collection.GetValues(index);
			return null;
		}
	}
}
