/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Areas.Setup
{
	public abstract class SetupManager
	{
		public abstract bool Exists();
		public abstract ConnectionStringSettings GetConnectionString();
		public abstract string GetWebsiteName();
		public abstract void Save(Uri organizationServiceUrl, AuthenticationProviderType authenticationType, string domain, string username, string password, Guid websiteId);
		public abstract bool Enabled();

		protected virtual void SaveWebsiteBinding(Guid websiteId)
		{
			using (var service = new OrganizationService(new CrmConnection(GetConnectionString())))
			{
				var website = new EntityReference("adx_website", websiteId);
				var siteName = GetSiteName();
				var virtualPath = HostingEnvironment.ApplicationVirtualPath ?? "/";

				var query = new QueryExpression("adx_websitebinding") { ColumnSet = new ColumnSet("adx_websiteid") };
				query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
				query.Criteria.AddCondition("adx_sitename", ConditionOperator.Equal, siteName);

				var pathFilter = query.Criteria.AddFilter(LogicalOperator.Or);
				if (!virtualPath.StartsWith("/")) virtualPath = "/" + virtualPath;
				pathFilter.AddCondition("adx_virtualpath", ConditionOperator.Equal, virtualPath);
				pathFilter.AddCondition("adx_virtualpath", ConditionOperator.Equal, virtualPath.Substring(1));
				if (virtualPath.Substring(1) == string.Empty)
				{
					pathFilter.AddCondition("adx_virtualpath", ConditionOperator.Null);
				}

				var bindings = service.RetrieveMultiple(query);

				if (bindings.Entities.Count == 0)
				{
					var websiteBinding = CreateWebsiteBinding(website, siteName, virtualPath);
					service.Create(websiteBinding);
				}
			}
		}

		private static Entity CreateWebsiteBinding(EntityReference websiteId, string siteName, string virtualPath)
		{
			var websiteBinding = new Entity("adx_websitebinding");
			websiteBinding.SetAttributeValue<string>("adx_name", StringExtensions.FormatWith("Binding: {0}{1}", siteName, virtualPath));
			websiteBinding.SetAttributeValue<EntityReference>("adx_websiteid", websiteId);
			websiteBinding.SetAttributeValue<string>("adx_sitename", siteName);
			websiteBinding.SetAttributeValue<string>("adx_virtualpath", virtualPath);

			return websiteBinding;
		}

		public static string GetSiteName()
		{
			string siteName = HostingEnvironment.SiteName;

			var match = SiteNameRegex.Match(siteName);
			return match.Success && match.Groups["site"].Success
				? match.Groups["site"].Value
				: siteName;
		}

		/// <summary>
		/// Regular expression to grep site name
		/// </summary>
		private static readonly Regex SiteNameRegex = new Regex(@"^.+_IN_\d+_(?<site>.+)$|^(?<site>[^\(\)]+)\(\d+\)$", RegexOptions.IgnoreCase);
	}

	internal class DefaultSetupManager : SetupManager
	{
		private DefaultSetupManager() { }

		public static SetupManager Create()
		{
			return new DefaultSetupManager();
		}

		public override void Save(Uri organizationServiceUrl, AuthenticationProviderType authenticationType, string domain, string username, string password, Guid websiteId)
		{
			var xml = new XElement("settings");
			xml.SetElementValue("organizationServiceUrl", organizationServiceUrl.OriginalString);
			xml.SetElementValue("authenticationType", authenticationType.ToString());
			xml.SetElementValue("domain", domain);
			xml.SetElementValue("username", username);
			xml.SetElementValue("password", Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(password), _machineKeyPurposes)));
			xml.Save(_settingsPath.Value);

			try
			{
				SaveWebsiteBinding(websiteId);
			}
			catch (Exception)
			{
				File.Delete(_settingsPath.Value);
			}
		}

		public override bool Exists()
		{
			return File.Exists(_settingsPath.Value);
		}

		public override ConnectionStringSettings GetConnectionString()
		{
			return Exists() ? _connectionString.Value : null;
		}

		public override string GetWebsiteName()
		{
			return Exists() ? _websiteName.Value : null;
		}

		public override bool Enabled()
		{
			return _isEnabled.Value;
		}


		private static readonly Lazy<string> _settingsPath = new Lazy<string>(GetSettingsPath);
		private static readonly Lazy<ConnectionStringSettings> _connectionString = new Lazy<ConnectionStringSettings>(InitConnectionString);
		private static readonly Lazy<string> _websiteName = new Lazy<string>(InitWebsiteName);
		private static readonly string[] _machineKeyPurposes = { "adxstudio", "setup" };
		private static readonly Lazy<bool> _isEnabled = new Lazy<bool>(CheckPortalConfigType);

		private static string GetSettingsPath()
		{
			var virtualPath = ConfigurationManager.AppSettings[typeof(SetupManager).FullName + ".SettingsPath"] ?? "~/App_Data/settings.xml";
			var settingsPath = HostingEnvironment.MapPath(virtualPath);
			var dataDirectory = Path.GetDirectoryName(settingsPath);

			if (!Directory.Exists(dataDirectory))
			{
				Directory.CreateDirectory(dataDirectory);
			}

			return settingsPath;
		}

		private static ConnectionStringSettings InitConnectionString()
		{
			var xml = XElement.Load(_settingsPath.Value);
			var organizationServiceUrl = GetUrl(xml, "organizationServiceUrl");
			var authenticationType = ToAuthType(GetEnum<AuthenticationProviderType>(xml, "authenticationType"));
			var domain = GetText(xml, "domain");
			var username = GetText(xml, "username");
			var password = Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(GetText(xml, "password")), _machineKeyPurposes));

			var connectionString = authenticationType == AuthenticationType.AD
				? StringExtensions.FormatWith("AuthType={0}; Url={1}; Username={2}; Password={3}; Domain={4};", authenticationType, organizationServiceUrl, username, password, domain)
				: StringExtensions.FormatWith("AuthType={0}; Url={1}; Username={2}; Password={3};", authenticationType, organizationServiceUrl, username, password);

			return new ConnectionStringSettings("Xrm", connectionString);
		}

		private static AuthenticationType ToAuthType(AuthenticationProviderType? apt)
		{
			if (apt == AuthenticationProviderType.ActiveDirectory) return AuthenticationType.AD;
			if (apt == AuthenticationProviderType.Federation) return AuthenticationType.IFD;
			if (apt == AuthenticationProviderType.OnlineFederation) return AuthenticationType.Office365;
			if (apt == AuthenticationProviderType.LiveId) return AuthenticationType.Live;
			return AuthenticationType.InvalidConnection;
		}

		private static string InitWebsiteName()
		{
			var xml = XElement.Load(_settingsPath.Value);
			return GetText(xml, "websiteName");
		}

		private static string GetText(XContainer xml, string tagName)
		{
			var element = xml.Element(tagName);
			return element != null ? element.Value : null;
		}

		private static Uri GetUrl(XContainer xml, string tagName)
		{
			var text = GetText(xml, tagName);
			return !string.IsNullOrWhiteSpace(text) ? new Uri(text) : null;
		}

		private static T? GetEnum<T>(XContainer xml, string tagName) where T : struct
		{
			var text = GetText(xml, tagName);
			return !string.IsNullOrWhiteSpace(text) ? StringExtensions.ToEnum<T>(text) as T? : null;
		}

		private enum PortalConfigType
		{
			OnLine,
			OnPrem,
			Azure
		}
		
		private static bool CheckPortalConfigType()
		{
			var portalConfigType = ConfigurationManager.AppSettings["PortalConfigType"];
			PortalConfigType result;
			if (!Enum.TryParse(portalConfigType, true, out result))
			{
				result = PortalConfigType.OnLine;
			}
			return  (result != PortalConfigType.OnLine);
		}

		/// <summary>
		///  Decision switch for the sort of Auth to login to CRM with 
		/// </summary>
		public enum AuthenticationType
		{
			/// <summary>
			/// Active Directory Auth
			/// </summary>
			AD,
			/// <summary>
			/// Live Auth
			/// </summary>
			Live,
			/// <summary>
			/// SPLA Auth
			/// </summary>
			IFD,
			/// <summary>
			/// CLAIMS based Auth
			/// </summary>
			Claims,
			/// <summary>
			/// Office365 base login process
			/// </summary>
			Office365,
			/// <summary>
			/// OAuth based Auth
			/// </summary>
			OAuth,
			/// <summary>
			/// Invalid connection
			/// </summary>
			InvalidConnection = -1
		}
	}
}
