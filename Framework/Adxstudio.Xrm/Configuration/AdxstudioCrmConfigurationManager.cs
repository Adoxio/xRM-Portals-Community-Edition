/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Search.Configuration;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// Methods for retrieving dependencies from the configuration.
	/// </summary>
	/// <remarks>
	/// Format of configuration (where a custom <see cref="OrganizationServiceContext"/> class called XrmServiceContext exists in the Xrm.dll assembly).
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///   <section name="microsoft.xrm.portal" type="Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal"/>
	///   <section name="adxstudio.xrm" type="Adxstudio.Xrm.Configuration.CrmSection, Adxstudio.Xrm"/>
	///   <section name="adxstudio.licenses" type="Adxstudio.Xrm.Configuration.LicensesSectionHandler, Adxstudio.Xrm"/>
	///  </configSections>
	/// 
	///  <connectionStrings>
	///   <add name="Xrm" connectionString="ServiceUri=...; Domain=...; Username=...; Password=..."/>
	///  </connectionStrings>
	///  
	///  <microsoft.xrm.client>
	///   <contexts default="Xrm">
	///    <add name="Xrm" type="Xrm.XrmServiceContext, Xrm" connectionStringName="Xrm"/>
	///   </contexts>
	///  </microsoft.xrm.client>
	///  
	///  <microsoft.xrm.portal>
	///   <portals default="Xrm">
	///    <add name="Xrm" websiteName="My Portal"/>
	///   </portals>
	///  </microsoft.xrm.portal>
	/// 
	///  <adxstudio.xrm>
	///  
	///   <cacheFeed enabled="false" localOnly="true" objectCacheName="" showValues="true" traced="true"/>
	///   
	///   <events>
	///    <add
	///     name="Xrm"
	///     instanceMode="PerInstance" [Static | PerName | PerRequest | PerInstance]
	///    >
	///     <providers>
	///      <add name="Xrm" type="Adxstudio.Xrm.Services.IOrganizationServiceEventProvider, Adxstudio.Xrm"/>
	///     </providers>
	///    </add>
	///   </events>
	///   
	///   <search enabled="true" defaultProvider="Portal">
	///    <providers>
	///     <add
	///      name="Portal"
	///      type="Adxstudio.Xrm.Search.PortalSearchProvider, Adxstudio.Xrm"
	///      portalName="Xrm"
	///      dataContextName="Xrm"
	///      indexPath=""
	///      stemmer="English"/>
	///    </providers>
	///   </search>
	///  </adxstudio.xrm>
	///  
	///  <adxstudio.licenses>
	///   <license for="XRM Extensions" ...>...</license>
	///   <license for="XRM Extensions" ...>...</license>
	///  </adxstudio.licenses>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	/// <seealso cref="CacheFeedElement"/>
	/// <seealso cref="OrganizationServiceEventElement"/>
	/// <seealso cref="SearchElement"/>
	/// <seealso cref="LicensesSectionHandler"/>
	public class AdxstudioCrmConfigurationManager
	{
		private static Lazy<AdxstudioCrmConfigurationProvider> _provider = new Lazy<AdxstudioCrmConfigurationProvider>(CreateProvider);

		private static AdxstudioCrmConfigurationProvider CreateProvider()
		{
			var section = ConfigurationManager.GetSection(CrmSection.SectionName) as CrmSection ?? new CrmSection();

			if (!string.IsNullOrWhiteSpace(section.ConfigurationProviderType))
			{
				var typeName = section.ConfigurationProviderType;
				var type = TypeExtensions.GetType(typeName);

				if (type == null || !type.IsA<AdxstudioCrmConfigurationProvider>())
				{
					throw new ConfigurationErrorsException("The value {0} is not recognized as a valid type or is not of the type {1}.".FormatWith(typeName, typeof(AdxstudioCrmConfigurationProvider)));
				}

				return Activator.CreateInstance(type) as AdxstudioCrmConfigurationProvider;
			}

			return new AdxstudioCrmConfigurationProvider();
		}

		/// <summary>
		/// Resets the cached dependencies.
		/// </summary>
		public static void Reset()
		{
			_provider = new Lazy<AdxstudioCrmConfigurationProvider>(CreateProvider);
		}

		public static CrmSection GetCrmSection()
		{
			return _provider.Value.GetCrmSection();
		}

		public static IEnumerable<IOrganizationServiceEventProvider> CreateEventProviders(string eventName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateEventProviders(eventName, allowDefaultFallback);
		}

		public static IContentMapProvider CreateContentMapProvider(CrmConnection connection, string contentMapProviderName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateContentMapProvider(connection, contentMapProviderName, allowDefaultFallback);
		}

		public static IContentMapProvider CreateContentMapProvider(string portalName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateContentMapProvider(portalName, allowDefaultFallback);
		}

		public static ISolutionDefinitionProvider CreateSolutionDefinitionProvider(string solutionDefinitionProviderName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateSolutionDefinitionProvider(solutionDefinitionProviderName, allowDefaultFallback);
		}

		/// <summary>
		/// Returns a boolean indicating if the unique solution name is installed in the CRM organization.
		/// </summary>
		/// <param name="solutionName"></param>
		/// <returns></returns>
		public static bool TryAssertSolutionName(string solutionName)
		{
			var solutionDefinitionProvider = CreateSolutionDefinitionProvider(null, true);
			var solution = solutionDefinitionProvider.GetSolution();
			return solution.Solutions.Contains(solutionName);
		}

		protected static OrganizationService CreateOrganizationService(string portalName = null, bool allowDefaultFallback = false, string serviceName = null)
		{
			var portalContextElement = PortalCrmConfigurationManager.GetPortalContextElement(portalName, allowDefaultFallback);

			var contextName = !string.IsNullOrWhiteSpace(portalContextElement.ContextName)
				? portalContextElement.ContextName
				: portalContextElement.Name;

			var connection = new CrmConnection(CrmConfigurationManager.GetConnectionStringNameFromContext(contextName));

			return CrmConfigurationManager.CreateService(connection, serviceName) as OrganizationService;
		}
	}
}
