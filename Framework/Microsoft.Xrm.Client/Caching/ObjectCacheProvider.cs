/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Threading;

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides <see cref="ObjectCache"/> operations.
	/// </summary>
	public class ObjectCacheProvider
	{
		/// <summary>
		/// Retrieves a configured <see cref="ObjectCache"/>.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public virtual ObjectCache GetInstance(string objectCacheName = null)
		{
			return CrmConfigurationManager.CreateObjectCache(objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element.
		/// </summary>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public virtual CacheItemPolicy GetCacheItemPolicy(string objectCacheName = null)
		{
			return CrmConfigurationManager.CreateCacheItemPolicy(objectCacheName, true);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element and adds a single <see cref="ChangeMonitor"/> to the policy.
		/// </summary>
		/// <param name="monitor"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public virtual CacheItemPolicy GetCacheItemPolicy(ChangeMonitor monitor, string objectCacheName = null)
		{
			var policy = GetCacheItemPolicy(objectCacheName);
			if (monitor != null) policy.ChangeMonitors.Add(monitor);
			return policy;
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> from a configuration element and adds a set of <see cref="CacheEntryChangeMonitor"/> objects.
		/// </summary>
		/// <param name="dependencies"></param>
		/// <param name="regionName"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public virtual CacheItemPolicy GetCacheItemPolicy(IEnumerable<string> dependencies, string regionName = null, string objectCacheName = null)
		{
			var cache = GetInstance(objectCacheName);
			var monitorKeys = dependencies.Select(d => d.ToLower());
			var monitor = GetChangeMonitor(cache, monitorKeys, regionName);
			return GetCacheItemPolicy(monitor, objectCacheName);
		}

		/// <summary>
		/// Builds a <see cref="CacheItemPolicy"/> and adds a set of <see cref="CacheEntryChangeMonitor"/> objects to the policy.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="dependencies"></param>
		/// <param name="regionName"></param>
		/// <param name="objectCacheName"></param>
		/// <returns></returns>
		public virtual CacheItemPolicy GetCacheItemPolicy(ObjectCache cache, IEnumerable<string> dependencies, string regionName = null, string objectCacheName = null)
		{
			var monitorKeys = dependencies.Select(d => d.ToLower());
			var monitor = GetChangeMonitor(cache, monitorKeys, regionName);
			return GetCacheItemPolicy(monitor, cache.Name);
		}

		/// <summary>
		/// Builds a <see cref="CacheEntryChangeMonitor"/> from a set of cache keys.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="monitorKeys"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		public virtual CacheEntryChangeMonitor GetChangeMonitor(ObjectCache cache, IEnumerable<string> monitorKeys, string regionName)
		{
			if (!monitorKeys.Any()) return null;

			// cache item dependencies need to be added to the cache prior to calling CreateCacheEntryChangeMonitor

			foreach (var key in monitorKeys)
			{
				cache.AddOrGetExisting(key, key, ObjectCache.InfiniteAbsoluteExpiration, regionName);
			}

			return cache.CreateCacheEntryChangeMonitor(monitorKeys);
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
		public virtual T Get<T>(
			ObjectCache cache,
			string cacheKey,
			Func<ObjectCache, T> load,
			Action<ObjectCache, T> insert,
			string regionName = null)
		{
			return LockManager.Get(
				cacheKey,
				// try to load from cache
				key => this.GetCacheItemValue(cache, key.ToLower(), regionName),
				key =>
				{
					// load object from the service

					var obj = load(cache);

					if (insert != null)
					{
						// insert object into cache

						insert(cache, obj);
					}

					return obj;
				});
		}

		/// <summary>
		/// Retrieves the cache item from cache, and returns value.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The cache key.</param>
		/// <param name="regionName">The regionName.</param>
		/// <returns></returns>
		private object GetCacheItemValue(ObjectCache cache, string key, string regionName)
		{
			var obj = cache.Get(key.ToLower(), regionName);

			var container = obj as CacheItemContainer;
			if (container != null)
			{
				return container.Value;
			}
			return obj;
		}

		/// <summary>
		/// Inserts an object into cache along with an associated <see cref="CacheItemDetail"/>.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="cacheKey"></param>
		/// <param name="value"></param>
		/// <param name="policy"></param>
		/// <param name="regionName"></param>
		public virtual void Insert(
			ObjectCache cache,
			string cacheKey,
			object value,
			CacheItemPolicy policy = null,
			string regionName = null)
		{
			var valueCacheKey = cacheKey.ToLower();

			// Create the cache value container - which contains the actual cache value and the cache item detail.
			var container = new CacheItemContainer(value, this.CreateCacheItemDetailObject(cache, cacheKey, policy, regionName));

			// Insert into cache.
			cache.Set(valueCacheKey, container, policy, regionName);
		}

		/// <summary>
		/// Create the <see cref="CacheItemDetail"/> associated with a cache item.
		/// </summary>
		/// <param name="cache">The cache</param>
		/// <param name="cacheKey">Cache item's key.</param>
		/// <param name="policy">Cache item's policy.</param>
		/// <param name="regionName">Region name.</param>
		/// <returns>Returns new instance of <see cref="CacheItemDetail"/></returns>
		public CacheItemDetail CreateCacheItemDetailObject(ObjectCache cache, string cacheKey, CacheItemPolicy policy, string regionName)
		{
			// Create CacheItemDetail
			var cacheItemDetail = new CacheItemDetail(cacheKey, policy);
			var cacheItemDetailPolicy = new CacheItemPolicy();

			// CacheItemDetail should be dependent on its related cache item
			cacheItemDetailPolicy.ChangeMonitors.Add(cache.CreateCacheEntryChangeMonitor(new[] { cacheKey }, regionName));

			return cacheItemDetail;
		}

		/// <summary>
		/// Retrieves the associated <see cref="CacheItemDetail"/> for a cache item.
		/// </summary>
		/// <param name="cache">The cache</param>
		/// <param name="cacheKey">Cache item's key.</param>
		/// <param name="regionName">Region name.</param>
		/// <returns></returns>
		public virtual CacheItemDetail GetCacheItemDetail(ObjectCache cache, string cacheKey, string regionName = null)
		{
			return (cache.Get(cacheKey.ToLower(), regionName) as CacheItemContainer)?.Detail;
		}

		/// <summary>
		/// Removes all items from the cache.
		/// </summary>
		/// <param name="cache"></param>
		public virtual void Clear(ObjectCache cache)
		{
			var items = cache.ToList();

			foreach (var item in items)
			{
				cache.Remove(item.Key);
			}
		}

		/// <summary>
		/// Removes all items from the cache by invoking the <see cref="IExtendedObjectCache"/> if it is available.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="regionName"></param>
		public virtual void RemoveAll(ObjectCache cache, string regionName = null)
		{
			var extended = cache as IExtendedObjectCache;

			if (extended != null)
			{
				extended.RemoveAll(regionName);
			}
			else
			{
				cache.Clear();
			}
		}
	}
}
