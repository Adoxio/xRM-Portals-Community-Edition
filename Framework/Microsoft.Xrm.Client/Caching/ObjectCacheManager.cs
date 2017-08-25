/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides methods for managing <see cref="ObjectCache"/> items.
	/// </summary>
	/// <remarks>
	/// The <see cref="ObjectCache"/> can be retrieved from configuration by name prior to performing further cache management operations.
	/// <example>
	/// An example of the configuration element used by the class.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///  </configSections>
	///  <microsoft.xrm.client>
	///   <objectCache default="Xrm">
	///    <add name="Xrm" type=type="System.Runtime.Caching.MemoryCache, System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"/>
	///   </objectCache>
	///  </microsoft.xrm.client>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public static class ObjectCacheManager
	{
		private static Lazy<ObjectCacheProvider> _provider = new Lazy<ObjectCacheProvider>(CreateProvider);

		private static ObjectCacheProvider CreateProvider()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			if (!string.IsNullOrWhiteSpace(section.ObjectCacheProviderType))
			{
				var typeName = section.ObjectCacheProviderType;
				var type = TypeExtensions.GetType(typeName);

				if (type == null || !type.IsA<ObjectCacheProvider>())
				{
					throw new ConfigurationErrorsException("The value '{0}' is not recognized as a valid type or is not of the type '{1}'.".FormatWith(typeName, typeof(ObjectCacheProvider)));
				}

				return Activator.CreateInstance(type) as ObjectCacheProvider;
			}

			return new ObjectCacheProvider();
		}

		/// <summary>
		/// Resets the cached dependencies.
		/// </summary>
		public static void Reset()
		{
			_provider = new Lazy<ObjectCacheProvider>(CreateProvider);
		}

		/// <summary>
		/// Retrieves a configured <see cref="ObjectCache"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public static ObjectCache GetInstance(string objectCacheName = null)
		{
			return _provider.Value.GetInstance(objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public static CacheItemPolicy GetCacheItemPolicy(string objectCacheName = null)
		{
			return _provider.Value.GetCacheItemPolicy(objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element and adds a single <see cref="ChangeMonitor"/> to the policy.
		/// </summary>
		/// <param name="monitor"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public static CacheItemPolicy GetCacheItemPolicy(this ChangeMonitor monitor, string objectCacheName = null)
		{
			return _provider.Value.GetCacheItemPolicy(monitor, objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element and adds a set of <see cref="CacheEntryChangeMonitor"/> objects.
		/// </summary>
		/// <param name="dependencies"></param>
		/// <param name="regionName"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public static CacheItemPolicy GetCacheItemPolicy(IEnumerable<string> dependencies, string regionName = null, string objectCacheName = null)
		{
			return _provider.Value.GetCacheItemPolicy(dependencies, regionName, objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> and adds a set of <see cref="CacheEntryChangeMonitor"/> objects to the policy.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="dependencies"></param>
		/// <param name="regionName"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public static CacheItemPolicy GetCacheItemPolicy(this ObjectCache cache, IEnumerable<string> dependencies, string regionName = null, string objectCacheName = null)
		{
			return _provider.Value.GetCacheItemPolicy(cache, dependencies, regionName, objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheEntryChangeMonitor"/> from a set of cache keys.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="monitorKeys"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static CacheEntryChangeMonitor GetChangeMonitor(this ObjectCache cache, IEnumerable<string> monitorKeys, string regionName)
		{
			return _provider.Value.GetChangeMonitor(cache, monitorKeys, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="insert"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			string cacheKey,
			Func<ObjectCache, T> load,
			Action<ObjectCache, T> insert,
			string regionName = null)
		{
			return Get((string)null, cacheKey, load, insert, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectCacheName"></param>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="insert"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			string objectCacheName,
			string cacheKey,
			Func<ObjectCache, T> load,
			Action<ObjectCache, T> insert,
			string regionName = null)
		{
			var cache = GetInstance(objectCacheName);
			return cache.Get(cacheKey, load, insert, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="getPolicy"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			string cacheKey,
			Func<ObjectCache, T> load,
			Func<CacheItemPolicy> getPolicy = null,
			string regionName = null)
		{
			return Get((string)null, cacheKey, load, getPolicy, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectCacheName"></param>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="getPolicy"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			string objectCacheName,
			string cacheKey,
			Func<ObjectCache, T> load,
			Func<CacheItemPolicy> getPolicy = null,
			string regionName = null)
		{
			var cache = GetInstance(objectCacheName);
			var getPol = getPolicy ?? (() => GetCacheItemPolicy(objectCacheName));
			return cache.Get(cacheKey, load, getPol, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="insert"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			this ObjectCache cache,
			string cacheKey,
			Func<ObjectCache, T> load,
			Action<ObjectCache, T> insert,
			string regionName = null)
		{
			return _provider.Value.Get(cache, cacheKey, load, insert, regionName);
		}

		/// <summary>
		/// Retrieves a cached object or loads the object if it does not exist in cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="load"></param>
		/// <param name="getPolicy"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static T Get<T>(
			this ObjectCache cache,
			string cacheKey,
			Func<ObjectCache, T> load,
			Func<CacheItemPolicy> getPolicy = null,
			string regionName = null)
		{
			Action<ObjectCache, T> insert = (c, obj) =>
			{
				var policy = getPolicy != null ? getPolicy() : GetCacheItemPolicy(cache.Name);
				c.Insert(cacheKey, obj, policy, regionName);
			};

			return Get(cache, cacheKey, load, insert);
		}

		/// <summary>
		/// Inserts an object into cache along with an associated <see cref="CacheItemDetail"/>.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="value"></param>
		/// <param name="policy"></param>
		/// <param name="regionName"></param>
		public static void Insert(
			this ObjectCache cache,
			string cacheKey,
			object value,
			CacheItemPolicy policy = null,
			string regionName = null)
		{
			_provider.Value.Insert(cache, cacheKey, value, policy, regionName);
		}

		/// <summary>
		/// Inserts an object into cache along with an associated <see cref="CacheItemDetail"/>.
		/// </summary>
		/// <remarks>
		/// The corresponding <see cref="CacheItemPolicy"/> is updated to include a <see cref="CacheEntryChangeMonitor"/> for each dependency string provided.
		/// </remarks>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="value"></param>
		/// <param name="dependencies"></param>
		/// <param name="regionName"></param>
		public static void Insert(
			this ObjectCache cache,
			string cacheKey,
			object value,
			IEnumerable<string> dependencies,
			string regionName = null)
		{
			var policy = GetCacheItemPolicy(cache, dependencies, regionName);
			cache.Insert(cacheKey, value, policy, regionName);
		}

		/// <summary>
		/// Returns the <see cref="CacheItemDetail"/> object for the given cache item.
		/// </summary>
		/// <param name="cache">Target cache.</param>
		/// <param name="cacheKey">Cache item's key.</param>
		/// <param name="policy">Cache item's policy.</param>
		/// <param name="regionName">Region name.</param>
		public static CacheItemDetail CreateCacheItemDetailObject(this ObjectCache cache, string cacheKey, CacheItemPolicy policy, string regionName)
		{
			return _provider.Value.CreateCacheItemDetailObject(cache, cacheKey, policy, regionName);
		}

		/// <summary>
		/// Retrieves the associated <see cref="CacheItemDetail"/> for a cache item.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public static CacheItemDetail GetCacheItemDetail(this ObjectCache cache, string cacheKey, string regionName = null)
		{
			return _provider.Value.GetCacheItemDetail(cache, cacheKey, regionName);
		}

		/// <summary>
		/// Removes all items from the cache.
		/// </summary>
		/// <param name="cache"></param>
		public static void Clear(this ObjectCache cache)
		{
			_provider.Value.Clear(cache);
		}

		/// <summary>
		/// Removes all items from the cache by invoking the <see cref="IExtendedObjectCache"/> if it is available.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="regionName"></param>
		public static void RemoveAll(this ObjectCache cache, string regionName = null)
		{
			_provider.Value.RemoveAll(cache, regionName);
		}
	}
}
