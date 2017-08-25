/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Runtime.Caching.Configuration;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// Methods for retrieving dependencies from the configuration.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Examples of dependency creation.
	/// <code>
	/// var name = "Xrm";
	/// 
	/// // retrieve by configuration
	/// var connection = new CrmConnection(name);
	/// var objectCache = CrmConfigurationManager.CreateObjectCache(name);
	/// var serviceCache = CrmConfigurationManager.CreateServiceCache(name, connection);
	/// var service = CrmConfigurationManager.CreateService(connection, name);
	/// var context = CrmConfigurationManager.CreateContext(name);
	/// 
	/// // retrieve by instantiation
	/// var myConnection = CrmConnection.Parse("ServiceUri=...; Domain=...; UserName=...; Password=...;");
	/// var myObjectCache = MemoryCache.Default;
	/// var myServiceCache = new OrganizationServiceCache(myObjectCache, myConnection);
	/// var myService = new CachedOrganizationService(myConnection, myServiceCache);
	/// var myContext = new OrganizationServiceContext(myService);
	/// </code>
	/// 
	/// Format of configuration (where a custom <see cref="OrganizationServiceContext"/> class called XrmServiceContext exists in the Xrm.dll assembly).
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///  </configSections>
	/// 
	///  <connectionStrings>
	///   <add name="Xrm" connectionString="ServiceUri=...; Domain=...; Username=...; Password=..."/>
	///  </connectionStrings>
	///  
	///  <microsoft.xrm.client
	///   sectionProviderType="Microsoft.Xrm.Client.Configuration.CrmConfigurationProvider, Microsoft.Xrm.Client">
	///   <contexts default="Xrm">
	///    <add name="Xrm" type="Xrm.XrmServiceContext, Xrm" connectionStringName="Xrm" serviceName="Xrm"/>
	///   </contexts>
	///   <services default="Xrm">
	///    <add
	///     name="Xrm"
	///     type="Microsoft.Xrm.Client.Services.CachedOrganizationService, Microsoft.Xrm.Client"
	///     serviceCacheName="Xrm"
	///     instanceMode="PerRequest" [Static | PerName | PerRequest | PerInstance]
	///    />
	///   </services>
	///   <serviceCache default="Xrm">
	///    <add
	///     name="Xrm"
	///     type="Microsoft.Xrm.Client.Services.OrganizationServiceCache, Microsoft.Xrm.Client"
	///     objectCacheName="Xrm"
	///     cacheMode="LookupAndInsert" [LookupAndInsert | InsertOnly | Disabled]
	///     returnMode="Cloned" [Shared | Cloned]
	///     queryHashingEnabled="false" [false | true]
	///    />
	///   </serviceCache>
	///   <objectCache default="Xrm">
	///    <add
	///     name="Xrm"
	///     type="System.Runtime.Caching.MemoryCache, System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
	///     instanceMode="PerName" [Static | PerName | PerInstance]
	///     absoluteExpiration=""
	///     slidingExpiration="00:00:00" [HH:MM:SS]
	///     duration="00:00:00" [HH:MM:SS]
	///     priority="Default" [Default | NotRemovable]
	///     outputCacheProfileName="Xrm"
	///    />
	///   </objectCache>
	///  </microsoft.xrm.client>
	///  
	///  <system.runtime.caching>
	///   <memoryCache>
	///    <namedCaches>
	///     <add name="Xrm"
	///      cacheMemoryLimitMegabytes="0"
	///      physicalMemoryLimitPercentage="0"
	///      pollingInterval="00:00:00" />
	///     </namedCaches>
	///    </memoryCache>
	///  </system.runtime.caching>
	/// 
	///  <system.web>
	///   <caching>
	///    <outputCacheSettings>
	///     <outputCacheProfiles>
	///      <add name="Xrm" enabled="true" duration="-1"/>
	///     </outputCacheProfiles>
	///    </outputCacheSettings>
	///   </caching>
	///  </system.web>
	/// 
	/// </configuration>
	/// ]]>
	/// </code>
	/// 
	/// Minimum required configuration to configure a custom <see cref="OrganizationServiceContext"/>.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
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
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// For further configuration details see <see cref="T:Microsoft.Xrm.Portal.Configuration.PortalCrmConfigurationManager"/>
	/// </remarks>
	/// <seealso cref="T:Microsoft.Xrm.Portal.Configuration.PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConnection"/>
	/// <seealso cref="OrganizationServiceContextElement"/>
	/// <seealso cref="OrganizationServiceElement"/>
	/// <seealso cref="OrganizationServiceCacheElement"/>
	/// <seealso cref="ObjectCacheElement"/>
	/// <seealso cref="MemoryCacheSection"/>
	/// <seealso cref="OrganizationServiceContext"/>
	/// <seealso cref="OrganizationService"/>
	/// <seealso cref="OrganizationServiceCache"/>
	/// <seealso cref="CachedOrganizationService"/>
	/// <seealso cref="MemoryCache"/>
	public static class CrmConfigurationManager
	{
		private static Lazy<CrmConfigurationProvider> _provider = new Lazy<CrmConfigurationProvider>(CreateProvider);

		private static CrmConfigurationProvider CreateProvider()
		{
			var section = ConfigurationManager.GetSection(CrmSection.SectionName) as CrmSection ?? new CrmSection();

			if (!string.IsNullOrWhiteSpace(section.ConfigurationProviderType))
			{
				var typeName = section.ConfigurationProviderType;
				var type = TypeExtensions.GetType(typeName);

				if (type == null || !type.IsA<CrmConfigurationProvider>())
				{
					throw new ConfigurationErrorsException("The value '{0}' is not recognized as a valid type or is not of the type '{1}'.".FormatWith(typeName, typeof(CrmConfigurationProvider)));
				}

				return Activator.CreateInstance(type) as CrmConfigurationProvider;
			}

			return new CrmConfigurationProvider();
		}

		/// <summary>
		/// Resets the cached dependencies.
		/// </summary>
		public static void Reset()
		{
			_provider = new Lazy<CrmConfigurationProvider>(CreateProvider);
			OrganizationService.Reset();
			ObjectCacheManager.Reset();
			LockManager.Reset();
		}

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public static CrmSection GetCrmSection()
		{
			return _provider.Value.GetCrmSection();
		}

		/// <summary>
		/// Retrieves the configured connection string settings.
		/// </summary>
		/// <param name="connectionStringName"></param>
		/// <returns></returns>
		public static ConnectionStringSettings CreateConnectionStringSettings(string connectionStringName)
		{
			return _provider.Value.CreateConnectionStringSettings(connectionStringName);
		}

		/// <summary>
		/// Retrieves the connection string name from the context name.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static string GetConnectionStringNameFromContext(string contextName, bool allowDefaultFallback = false)
		{
			return _provider.Value.GetConnectionStringNameFromContext(contextName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static OrganizationServiceContext CreateContext(string contextName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateContext(contextName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IOrganizationService CreateService(string contextName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateService(contextName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="serviceName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IOrganizationService CreateService(CrmConnection connection, string serviceName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateService(connection, serviceName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationServiceCache"/>.
		/// </summary>
		/// <param name="serviceCacheName"></param>
		/// <param name="connectionId"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IOrganizationServiceCache CreateServiceCache(string serviceCacheName = null, string connectionId = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateServiceCache(serviceCacheName, connectionId, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationServiceCache"/>.
		/// </summary>
		/// <param name="serviceCacheName"></param>
		/// <param name="connection"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static IOrganizationServiceCache CreateServiceCache(string serviceCacheName, CrmConnection connection, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateServiceCache(serviceCacheName, connection, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="ObjectCache"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static ObjectCache CreateObjectCache(string objectCacheName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateObjectCache(objectCacheName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the configured <see cref="CacheItemPolicy"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public static CacheItemPolicy CreateCacheItemPolicy(string objectCacheName = null, bool allowDefaultFallback = false)
		{
			return _provider.Value.CreateCacheItemPolicy(objectCacheName, allowDefaultFallback);
		}

		/// <summary>
		/// Retrieves the cached <see cref="ObjectCache"/> objects.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static IEnumerable<ObjectCache> GetObjectCaches(string name = null)
		{
			return _provider.Value.GetObjectCaches(name);
		}

		/// <summary>
		/// Creates and caches connection strings by name.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IDictionary<string, string> CreateConnectionDictionary(ConnectionStringSettings connectionString)
		{
			return _provider.Value.CreateConnectionDictionary(connectionString);
		}
	}
}
