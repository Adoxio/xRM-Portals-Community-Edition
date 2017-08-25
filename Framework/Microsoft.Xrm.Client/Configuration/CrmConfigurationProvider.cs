/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using Microsoft.Xrm.Client.Collections.Generic;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// Provides configuration settings based on the <see cref="ConfigurationManager"/>.
	/// </summary>
	public class CrmConfigurationProvider
	{
		private static CrmSection CreateConfiguration()
		{
			var configuration = ConfigurationManager.GetSection(CrmSection.SectionName) as CrmSection ?? new CrmSection();
			var args = new CrmSectionCreatedEventArgs { Configuration = configuration };

			var handler = ConfigurationCreated;

			if (handler != null)
			{
				handler(null, args);
			}

			return args.Configuration;
		}

		/// <summary>
		/// Occurs after the <see cref="CrmSection"/> configuration is created.
		/// </summary>
		public static event EventHandler<CrmSectionCreatedEventArgs> ConfigurationCreated;

		private CrmSection _crmSection;

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public virtual CrmSection GetCrmSection()
		{
			return _crmSection ?? (_crmSection = CreateConfiguration());
		}

		/// <summary>
		/// Retrieves the configured connection string settings.
		/// </summary>
		/// <param name="connectionStringName"></param>
		/// <returns></returns>
		public virtual ConnectionStringSettings CreateConnectionStringSettings(string connectionStringName)
		{
			var section = GetCrmSection();

			return section.ConnectionStrings[connectionStringName]
				?? ConfigurationManager.ConnectionStrings[connectionStringName];
		}

		/// <summary>
		/// Retrieves the connection string name from the context name.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual string GetConnectionStringNameFromContext(string contextName, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var contextElement = section.Contexts.GetElementOrDefault(contextName, allowDefaultFallback);

			var connectionStringName = !string.IsNullOrWhiteSpace(contextElement.ConnectionStringName)
				? contextElement.ConnectionStringName
				: contextElement.Name;

			return connectionStringName;
		}

		/// <summary>
		/// Retrieves the configured <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual OrganizationServiceContext CreateContext(string contextName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var contextElement = section.Contexts.GetElementOrDefault(contextName, allowDefaultFallback);

			if (contextElement.Name == null)
			{
				throw new ConfigurationErrorsException("A custom context must be specified.");
			}

			var service = CreateService(contextName, true);
			var context = contextElement.CreateOrganizationServiceContext(service);

			return context;
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IOrganizationService CreateService(string contextName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var contextElement = section.Contexts.GetElementOrDefault(contextName, allowDefaultFallback);

			var serviceName = !string.IsNullOrWhiteSpace(contextElement.ServiceName)
				? contextElement.ServiceName
				: contextElement.Name;

			var connectionStringName = !string.IsNullOrWhiteSpace(contextElement.ConnectionStringName)
				? contextElement.ConnectionStringName
				: contextElement.Name;

			var service = CreateService(new CrmConnection(connectionStringName), serviceName, true);

			return service;
		}

		private IOrganizationService _service;
		private readonly ConcurrentDictionary<string, IOrganizationService> _serviceLookup = new ConcurrentDictionary<string, IOrganizationService>();

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="serviceName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IOrganizationService CreateService(CrmConnection connection, string serviceName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var serviceElement = section.Services.GetElementOrDefault(serviceName, allowDefaultFallback);
			var mode = serviceElement.InstanceMode;
			var name = serviceElement.Name;

			if (mode == OrganizationServiceInstanceMode.Static)
			{
				// return a single instance

				return _service ?? (_service = CreateService(serviceElement, connection));
			}

			if (mode == OrganizationServiceInstanceMode.PerName)
			{
				var key = name ?? GetDefaultContextName();

				if (!_serviceLookup.ContainsKey(key))
				{
					_serviceLookup[key] = CreateService(serviceElement, connection);
				}

				return _serviceLookup[key];
			}

			if (mode == OrganizationServiceInstanceMode.PerRequest && HttpSingleton<IOrganizationService>.Enabled)
			{
				var key = name ?? GetDefaultContextName();

				return HttpSingleton<IOrganizationService>.GetInstance(key, () => CreateService(serviceElement, connection));
			}

			var service = CreateService(serviceElement, connection);
			return service;
		}

		private IOrganizationService CreateService(OrganizationServiceElement serviceElement, CrmConnection connection)
		{
			var serviceCacheName = !string.IsNullOrWhiteSpace(serviceElement.ServiceCacheName)
				? serviceElement.ServiceCacheName
				: serviceElement.Name;
			var serviceCache = CreateServiceCache(serviceCacheName, connection.GetConnectionId(), true);

			var service = serviceElement.CreateOrganizationService(connection, serviceCache);

			return service;
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationServiceCache"/>.
		/// </summary>
		/// <param name="serviceCacheName"></param>
		/// <param name="connectionId"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IOrganizationServiceCache CreateServiceCache(string serviceCacheName = null, string connectionId = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var serviceCacheElement = section.ServiceCache.GetElementOrDefault(serviceCacheName, allowDefaultFallback);

			var objectCacheName = !string.IsNullOrWhiteSpace(serviceCacheElement.ObjectCacheName)
				? serviceCacheElement.ObjectCacheName
				: serviceCacheElement.Name;
			var objectCacheElement = section.ObjectCache.GetElementOrDefault(objectCacheName, allowDefaultFallback);

			var settings = new OrganizationServiceCacheSettings(connectionId)
			{
				ConnectionId = connectionId,
				CacheRegionName = serviceCacheElement.CacheRegionName,
				QueryHashingEnabled = serviceCacheElement.QueryHashingEnabled,
				PolicyFactory = objectCacheElement,
			};

			var objectCache = CreateObjectCache(objectCacheName, true);
			var serviceCache = serviceCacheElement.CreateOrganizationServiceCache(objectCache, settings);

			return serviceCache;
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationServiceCache"/>.
		/// </summary>
		/// <param name="serviceCacheName"></param>
		/// <param name="connection"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IOrganizationServiceCache CreateServiceCache(string serviceCacheName, CrmConnection connection, bool allowDefaultFallback = false)
		{
			return CreateServiceCache(serviceCacheName, connection.GetConnectionId(), allowDefaultFallback);
		}

		private ObjectCache _objectCache;
		private readonly ConcurrentDictionary<string, ObjectCache> _objectCacheLookup = new ConcurrentDictionary<string, ObjectCache>();

		/// <summary>
		/// Retrieves the configured <see cref="ObjectCache"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual ObjectCache CreateObjectCache(string objectCacheName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var objectCacheElement = section.ObjectCache.GetElementOrDefault(objectCacheName, allowDefaultFallback);
			var mode = objectCacheElement.InstanceMode;
			var name = !string.IsNullOrWhiteSpace(objectCacheElement.Name) ? objectCacheElement.Name : GetDefaultContextName();

			if (mode == ObjectCacheInstanceMode.Static)
			{
				// return a single instance

				return _objectCache ?? (_objectCache = objectCacheElement.CreateObjectCache(name));
			}

			if (mode == ObjectCacheInstanceMode.PerName)
			{
				var key = name ?? GetDefaultContextName() ?? ObjectCacheElement.DefaultObjectCacheName;

				if (!_objectCacheLookup.ContainsKey(key))
				{
					_objectCacheLookup[key] = objectCacheElement.CreateObjectCache(name);
				}

				return _objectCacheLookup[key];
			}

			// return a new instance for each call

			var objectCache = objectCacheElement.CreateObjectCache(name);
			return objectCache;
		}

		/// <summary>
		/// Retrieves the configured <see cref="CacheItemPolicy"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual CacheItemPolicy CreateCacheItemPolicy(string objectCacheName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var objectCacheElement = section.ObjectCache.GetElementOrDefault(objectCacheName, allowDefaultFallback);
			var policy = objectCacheElement.CreateCacheItemPolicy();

			return policy;
		}

		/// <summary>
		/// Retrieves the cached <see cref="ObjectCache"/> objects.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual IEnumerable<ObjectCache> GetObjectCaches(string name = null)
		{
			if (name != null)
			{
				ObjectCache cache;

				if (_objectCacheLookup.TryGetValue(name, out cache))
				{
					yield return cache;
				}

				yield break;
			}

			if (_objectCache != null) yield return _objectCache;
			foreach (var cache in _objectCacheLookup.Values) yield return cache;
		}

		private readonly ConcurrentDictionary<string, IDictionary<string, string>> _connectionLookup = new ConcurrentDictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// Creates and caches connection strings by name.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public virtual IDictionary<string, string> CreateConnectionDictionary(ConnectionStringSettings connectionString)
		{
			connectionString.ThrowOnNull("connectionString");

			var name = connectionString.Name;

			if (!_connectionLookup.ContainsKey(name))
			{
				// cache ths mapping for performance

				_connectionLookup[name] = connectionString.ConnectionString.ToDictionary();
			}

			return _connectionLookup[name];
		}

		private string GetDefaultContextName()
		{
			var section = GetCrmSection();
			var element = section.Contexts.GetElementOrDefault(null);
			return element.Name;
		}
	}
}
