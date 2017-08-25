/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// Methods for retrieving portal dependencies from the configuration.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Examples of dependency creation.
	/// <code>
	/// var name = "Xrm";
	/// 
	/// // retrieve by configuration
	/// var portal = PortalCrmConfigurationManager.CreatePortalContext(name);
	/// 
	/// // retrieve by instantiation
	/// var myConnection = CrmConnection.Parse("ServiceUri=...; Domain=...; UserName=...; Password=...;");
	/// var myObjectCache = MemoryCache.Default;
	/// var myServiceCache = new OrganizationServiceCache(myObjectCache, myConnection);
	/// var myService = new CachedOrganizationService(myConnection, myServiceCache);
	/// var myContext = new OrganizationServiceContext(myService);
	/// var myPortal = new PortalContext(myContext);
	/// </code>
	/// 
	/// Format of configuration (where a custom <see cref="OrganizationServiceContext"/> class called XrmServiceContext exists in the Xrm.dll assembly).
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///   <section name="microsoft.xrm.portal" type="Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal"/>
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
	///  <microsoft.xrm.portal
	///   sectionProviderType="Microsoft.Xrm.Portal.Configuration.PortalCrmConfigurationProvider, Microsoft.Xrm.Portal"
	///   serviceBusObjectCacheName="microsoft.xrm.portal"
	///   rewriteVirtualPathEnabled="true" [false | true]
	///   >
	///   
	///   <portals default="Xrm">
	///    <add
	///     name="Xrm"
	///     websiteName="My Portal"
	///     contextName="Xrm"
	///     cmsServiceBaseUri="~/Services/Cms.svc"
	///     instanceMode="PerRequest" [PerRequest | PerInstance]
	///     mergeOption="AppendOnly" [AppendOnly | OverwriteChanges | PreserveChanges | NoTracking]
	///     >
	///      <websiteSelector type="Microsoft.Xrm.Portal.Cms.WebsiteSelectors.NameWebsiteSelector, Microsoft.Xrm.Portal"/>
	///      <dependencyProvider type="Microsoft.Xrm.Portal.Configuration.DependencyProvider, Microsoft.Xrm.Portal"/>
	///      <crmEntitySecurityProvider type="Microsoft.Xrm.Portal.Cms.Security.CmsCrmEntitySecurityProvider, Microsoft.Xrm.Portal"/>
	///     </add>
	///   </portals>
	///  
	///   <cachePolicy>
	///    <embeddedResource
	///     cacheExtension=""
	///     cacheability="ServerAndPrivate" [NoCache | Private | Public | Server | ServerAndNoCache | ServerAndPrivate]
	///     expires=""
	///     maxAge="01:00:00" [HH:MM:SS]
	///     revalidation="" [AllCaches | ProxyCaches | None]
	///     slidingExpiration="" [false | true]
	///     validUntilExpires="" [false | true]
	///     varyByCustom=""
	///     varyByContentEncodings="gzip;x-gzip;deflate"
	///     varyByContentHeaders=""
	///     varyByParams=""
	///     />
	///    <annotation
	///     cacheExtension=""
	///     cacheability="ServerAndPrivate" [NoCache | Private | Public | Server | ServerAndNoCache | ServerAndPrivate]
	///     expires=""
	///     maxAge="01:00:00" [HH:MM:SS]
	///     revalidation="" [AllCaches | ProxyCaches | None]
	///     slidingExpiration="" [false | true]
	///     validUntilExpires="" [false | true]
	///     varyByCustom=""
	///     varyByContentEncodings="gzip;x-gzip;deflate"
	///     varyByContentHeaders=""
	///     varyByParams=""
	///     />
	///   </cachePolicy>
	/// 
	///  </microsoft.xrm.portal>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// 
	/// Minimum required configuration to configure an <see cref="IPortalContext"/>.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///   <section name="microsoft.xrm.portal" type="Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal"/>
	///  </configSections>
	/// 
	///  <connectionStrings>
	///   <add name="Xrm" connectionString="ServiceUri=...; Domain=...; Username=...; Password=..."/>
	///  </connectionStrings>
	///  
	///  <microsoft.xrm.client>
	///   <contexts>
	///    <add name="Xrm" type="Xrm.XrmServiceContext, Xrm"/>
	///   </contexts>
	///  </microsoft.xrm.client>
	///  
	///  <microsoft.xrm.portal>
	///   <portals>
	///    <add name="My Portal"/>
	///   </portals>
	///  </microsoft.xrm.portal>
	///  
	///  <location path="Services/Cms.svc">
	///   <system.web>
	///    <authorization>
	///     <allow roles="My Portal Administrators"/>
	///     <deny users="*"/>
	///    </authorization>
	///   </system.web>
	///  </location>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// 
	/// If the website is hosting a custom <see cref="OrganizationServiceContext"/> OData endpoint (at the path /Services/Cms.svc in these examples) then the
	/// endpoint should be protected with the following (in addition, the membership and role providers will need to be configured).
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <location path="Services/Cms.svc">
	///   <system.web>
	///    <authorization>
	///     <allow roles="My Portal Administrators"/>
	///     <deny users="*"/>
	///    </authorization>
	///   </system.web>
	///  </location>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// For further configuration details see <see cref="CrmConfigurationManager"/>
	/// </remarks>
	/// <seealso cref="CrmConfigurationManager"/>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalContext"/>
	public class PortalCrmConfigurationManager
	{
		private static Lazy<PortalCrmConfigurationProvider> _provider = new Lazy<PortalCrmConfigurationProvider>(CreateProvider);

		private static PortalCrmConfigurationProvider CreateProvider()
		{
			var section = ConfigurationManager.GetSection(PortalCrmSection.SectionName) as PortalCrmSection ?? new PortalCrmSection();

			if (!string.IsNullOrWhiteSpace(section.ConfigurationProviderType))
			{
				var typeName = section.ConfigurationProviderType;
				var type = TypeExtensions.GetType(typeName);

				if (type == null || !type.IsA<PortalCrmConfigurationProvider>())
				{
					throw new ConfigurationErrorsException("The value '{0}' is not recognized as a valid type or is not of the type '{1}'.".FormatWith(typeName, typeof(PortalCrmConfigurationProvider)));
				}

				return Activator.CreateInstance(type) as PortalCrmConfigurationProvider;
			}

			return new PortalCrmConfigurationProvider();
		}

		/// <summary>
		/// Resets the cached dependencies.
		/// </summary>
		public static void Reset()
		{
			_provider = new Lazy<PortalCrmConfigurationProvider>(CreateProvider);
		}

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public static PortalCrmSection GetPortalCrmSection()
		{
			return _provider.Value.GetPortalCrmSection();
		}

		/// <summary>
		/// Retrieves the configuration element for the portal.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static PortalContextElement GetPortalContextElement(string portalName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.GetPortalContextElement(portalName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="ICrmEntitySecurityProvider"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public static ICrmEntitySecurityProvider CreateCrmEntitySecurityProvider(string portalName = null)
		{
			return _provider.Value.CreateCrmEntitySecurityProvider(portalName);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IDependencyProvider"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public static IDependencyProvider CreateDependencyProvider(string portalName = null)
		{
			return _provider.Value.CreateDependencyProvider(portalName);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IOrganizationService CreateOrganizationService(string portalName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateOrganizationService(portalName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IPortalContext"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="request"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IPortalContext CreatePortalContext(string portalName = null, RequestContext request = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreatePortalContext(portalName, request, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static OrganizationServiceContext CreateServiceContext(string portalName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateServiceContext(portalName, allowDefaultFallback);
		}

		internal static string GetCmsServiceBaseUri(string portalName = null)
		{
			return _provider.Value.GetCmsServiceBaseUri(portalName);
		}

		internal static OrganizationServiceContext GetServiceContext(string portalName = null, RequestContext request = null)
		{
			return _provider.Value.GetServiceContext(portalName, request);
		}
	}
}
