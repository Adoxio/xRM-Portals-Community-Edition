/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Compilation
{
	using System;
	using System.Web;
	using System.Collections.Specialized;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web.Compilation;
	using Microsoft.Xrm.Sdk;
	using Adxstudio.Xrm.Cms;

	/// <summary>
	/// Expression builder for retrieving CDN based site setting URL content.
	/// </summary>
	/// <remarks>
	/// The value of the site setting must begin with "~/cdn/" in order for the resulting URL to be resolved to the CDN URL.
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="CdnSiteSetting" type="Adxstudio.Xrm.Web.Compilation.CdnSiteSettingExpressionBuilder, Adxstudio.Xrm"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ CdnSiteSetting: Name={setting name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}] [, CdnSettingName={setting name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ CdnSiteSetting: Name=/css/header_background, CdnSettingName=/url/cdn %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class CdnSiteSettingExpressionBuilder : CrmExpressionBuilder<CdnSiteSettingExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(arguments.GetValueByIndexOrName(0, "Name")))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "Name={setting name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}] [, CdnSettingName={setting name}]");
				}

				// retrieve the site setting value

				var settingName = arguments.GetValueByIndexOrName(0, "Name");
				var defaultValue = arguments.GetValueByIndexOrName(1, "Default") ?? string.Empty;
				var format = arguments.GetValueByIndexOrName(2, "Format");

				var settings = new SettingDataAdapter(new PortalConfigurationDataAdapterDependencies(), HttpContext.Current.GetWebsite());
				var selected = settings.Select(settingName);
				var setting = selected == null ? null : selected.Entity;

				var returnType = GetReturnType(controlType, propertyName);
				if (returnType.IsA(typeof(Entity))) return setting;

				var url = setting == null
					? defaultValue
					: GetEvalData(setting, "adx_value", null, format, returnType) as string;

				if (string.IsNullOrEmpty(url) || !url.Contains("~/cdn/")) return url;

				// retrieve the CDN hostname value

				var cdnSettingName = arguments.GetValueByIndexOrName(4, "CdnSettingName") ?? "/url/cdn";
				selected = settings.Select(cdnSettingName);
				var cdnSetting = selected == null ? null : selected.Entity;

				if (cdnSetting == null) return url;

				// combine the two values

				var cdnFormat = url.Replace("~/cdn/", "{0}/");

				return GetEvalData(cdnSetting, "adx_value", null, cdnFormat, returnType);
			}
		}
	}
}
