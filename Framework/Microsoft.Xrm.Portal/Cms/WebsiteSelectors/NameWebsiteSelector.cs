/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Cms.WebsiteSelectors
{
	/// <summary>
	/// Selects website based on a website's name.
	/// </summary>
	/// <remarks>
	/// The website name is retrieved from the <see cref="PortalContextElement"/> by specifying the "websiteName" attribute.
	/// See <see cref="PortalCrmConfigurationManager"/> for an example.
	/// </remarks>
	public class NameWebsiteSelector : IWebsiteSelector, IInitializable // MSBug #119979: Won't seal, extension point.
	{
		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> where the website name is specified.
		/// </summary>
		public string PortalName { get; private set; }

		/// <summary>
		/// Gets the value of the configured website name.
		/// </summary>
		/// <exception cref="ConfigurationErrorsException">thrown if the app setting value is not found</exception>
		public string ConfiguredWebsiteName { get; protected set; }

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
			ConfiguredWebsiteName = config["websiteName"] ?? ConfigurationManager.AppSettings["crm-site"] ?? name;

			if (string.IsNullOrEmpty(ConfiguredWebsiteName))
			{
				// try get the name from the portal configuration element
				var portal = PortalCrmConfigurationManager.GetPortalContextElement(PortalName);
				ConfiguredWebsiteName = portal == null ? null : portal.Parameters["websiteName"] ?? portal.Name;
			}

			if (string.IsNullOrEmpty(ConfiguredWebsiteName))
			{
				throw new ConfigurationErrorsException(@"Unable to get configured website name. An appSetting key ""crm-site"" must be specified with a valid website name.");
			}
		}

		public NameWebsiteSelector(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// Selects website based on a website's name.
		/// </summary>
		/// <remarks>If more than one website matches, the first match is returned.</remarks>
		/// <exception cref="ApplicationException">thrown if a website match cannot be found</exception> 
		public virtual Entity GetWebsite(OrganizationServiceContext context, RequestContext request)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var website = GetWebsitesByName(context, ConfiguredWebsiteName).FirstOrDefault();

			if (website == null)
			{
				throw new ApplicationException(@"Unable to find a Website with the Name == ""{0}""".FormatWith(ConfiguredWebsiteName));
			}

			return website;
		}

		private static IEnumerable<Entity> GetWebsitesByName(OrganizationServiceContext context, string name)
		{
			return context.CreateQuery("adx_website")
				.Where(website => website.GetAttributeValue<string>("adx_name") == name)
				.AsEnumerable();
		}
	}
}
