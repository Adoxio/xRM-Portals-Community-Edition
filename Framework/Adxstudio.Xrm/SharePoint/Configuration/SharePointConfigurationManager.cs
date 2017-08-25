/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Adxstudio.SharePoint.Collections.Generic;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.SharePoint.Configuration
{
	internal static class SharePointConfigurationManager
	{
		public static void Reset()
		{
			_connectionLookup.Clear();
			_sharePointSection = null;
		}

		private static SharePointSection _sharePointSection;

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public static SharePointSection GetSharePointSection()
		{
			return ConfigurationManager.GetSection(SharePointSection.SectionName) as SharePointSection
				?? _sharePointSection ?? (_sharePointSection = new SharePointSection());
		}

		private static readonly ConcurrentDictionary<string, IDictionary<string, string>> _connectionLookup = new ConcurrentDictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// Creates and caches connection strings by name.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IDictionary<string, string> CreateConnectionDictionary(ConnectionStringSettings connectionString)
		{
			connectionString.ThrowOnNull("connectionString");

			var name = connectionString.Name;

			if (!_connectionLookup.ContainsKey(name))
			{
				var connection = connectionString.ConnectionString.ToDictionary();

				if (name == "Xrm")
				{
					// if using the CRM connection, replace the Url with the SharePoint one

					var context = PortalCrmConfigurationManager.CreateServiceContext();

					var sharePointUrl = context.GetSettingValueByName("SharePoint/URL");

					if (string.IsNullOrWhiteSpace(sharePointUrl))
					{
						var sharePointSites = context.CreateQuery("sharepointsite").Where(site => site.GetAttributeValue<int?>("statecode") == 0).ToArray(); // all active SP sites
						
						var defaultSharePointSite = sharePointSites.FirstOrDefault(site => site.GetAttributeValue<bool>("isdefault"));

						if (defaultSharePointSite == null) throw new Exception("A SharePoint/URL site setting couldn't be found, and no default SharePoint site exists. Specify a SharePoint/URL site setting or make a default SharePoint site.");

						sharePointUrl = defaultSharePointSite.GetAttributeValue<string>("absoluteurl") ?? string.Empty;

						var parentSiteReference = defaultSharePointSite.GetAttributeValue<EntityReference>("parentsite");

						if (parentSiteReference != null)
						{
							var parentSite = sharePointSites.FirstOrDefault(site => site.Id == parentSiteReference.Id);

							if (parentSite != null)
							{
								sharePointUrl = "{0}/{1}".FormatWith(parentSite.GetAttributeValue<string>("absoluteurl").TrimEnd('/'), defaultSharePointSite.GetAttributeValue<string>("relativeurl"));
							}
						}
					}

					connection["Url"] = sharePointUrl;
				}

				// cache ths mapping for performance
				_connectionLookup[name] = connection;
			}

			return _connectionLookup[name];
		}
	}
}
