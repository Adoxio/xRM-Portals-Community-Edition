/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Compilation
{
	using System;
	using System.Web;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Globalization;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web.Compilation;
	using Microsoft.Xrm.Sdk;
	using Adxstudio.Xrm.Cms;

	/// <summary>
	/// Expression builder for retrieving site setting content for the current portal.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Configuration:
	/// <code>
	/// <![CDATA[
	///  <configuration>
	///   <system.web>
	///    <compilation>
	///     <expressionBuilders>
	///      <add expressionPrefix="SiteSetting" type="Adxstudio.Xrm.Web.Compilation.SiteSettingExpressionBuilder, Adxstudio.Xrm"/>
	///     </expressionBuilders>
	///    </compilation>
	///   </system.web>
	///  </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page:
	/// <code>
	/// <![CDATA[
	///  <asp:Label runat="server" Text='<%$ SiteSetting: Name={setting name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}] %>'/>
	///  <asp:Label runat="server" Text='<%$ SiteSetting: Name=Browser Title Prefix, Format=-{0}- %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public class SiteSettingExpressionBuilder : CrmExpressionBuilder<SiteSettingExpressionBuilder.Provider>
	{
		/// <summary>
		/// <see cref="IExpressionBuilderProvider" />
		/// </summary>
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(arguments.GetValueByIndexOrName(0, "Name"))) ThrowArgumentException(propertyName, expressionPrefix, "Name={setting name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}]");

				var settingName = arguments.GetValueByIndexOrName(0, "Name");
				var defaultString = arguments.GetValueByIndexOrName(1, "Default") ?? string.Empty;
				var format = arguments.GetValueByIndexOrName(2, "Format");
				var returnType = GetReturnType(controlType, propertyName);

				var settings = new SettingDataAdapter(new PortalConfigurationDataAdapterDependencies(), HttpContext.Current.GetWebsite());
				var selected = settings.Select(settingName);
				var setting = selected == null ? null : selected.Entity;

				if (returnType.IsA(typeof(Entity))) return setting;

				if (returnType.IsA(typeof(EntityReference))) return setting == null ? null : setting.ToEntityReference();

				var value = setting == null ? null : setting.GetAttributeValue<string>("adx_value");

				object returnValue;

				if (returnType == typeof(string) && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(format))
				{
					returnValue = string.Format(CultureInfo.InvariantCulture, format, value);

					return returnValue;
				}

				if (string.IsNullOrWhiteSpace(value)) value = defaultString;

				if (returnType == typeof(string))
				{
					returnValue = value;
				}
				else
				{
					if (string.IsNullOrWhiteSpace(value))
					{
						return null;
					}

					var typeConverter = TypeDescriptor.GetConverter(returnType);
					returnValue = typeConverter.ConvertFromString(value);
				}

				return returnValue;
			}
		}
	}
}
