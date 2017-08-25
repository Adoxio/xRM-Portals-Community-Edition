/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Net.Mail;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Adxstudio.Xrm.Web.Modules
{
	/// <summary>
	/// Sends application error details (using a <see cref="SmtpClient"/>) to configured recipients.
	/// </summary>
	/// <remarks>
	/// Configuration is done through a set of application settings. The only required application setting is the "Adxstudio.Xrm.Web.Modules.AzureErrorNotifierModule.SmtpClient.To" setting.
	/// 
	/// Includes improvements for running in a Windows Azure environment.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	///  <appSettings>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.Host" value="localhost"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.From" value="webmaster@contoso.com"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.SmtpClient.To" value="recipient1@contoso.com,recipient2@contoso.com"/>
	///   <add key="Adxstudio.Xrm.Web.Modules.ErrorNotifierModule.StatusCodesExcluded" value="400,404"/>
	///  </appSettings>
	///  <system.net>
	///   <mailSettings>
	///    <smtp from="webmaster@contoso.com">
	///     <network host="localhost"/>
	///    </smtp>
	///   </mailSettings>
	///  </system.net>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	public class AzureErrorNotifierModule : ErrorNotifierModule
	{
		protected override string GetConfigurationSettingValue(string configName)
		{
			string value;

			try
			{
				value = GetAzureConfigurationSettingValue(configName);
			}
			catch (RoleEnvironmentException)
			{
				// the setting does not exist in the deployment configuration
				value = null;
			}

			return value ?? ConfigurationManager.AppSettings[configName];
		}

		private static string GetAzureConfigurationSettingValue(string configName)
		{
			return RoleEnvironment.IsAvailable ? RoleEnvironment.GetConfigurationSettingValue(configName) : null;
		}
	}
}
