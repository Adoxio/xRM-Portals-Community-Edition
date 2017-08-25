/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Caching
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Web.Caching;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Caching;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Threading;

	/// <summary>
	/// A custom <see cref="OutputCacheProvider"/> for writing to a configured <see cref="ObjectCache"/>.
	/// </summary>
	/// <remarks>
	/// The following configuration points the <see cref="OutputCacheProvider"/> to a secondary local <see cref="MemoryCache"/>. If the
	/// 'cacheItemDependenciesEnabled' flag is 'true', then any cache insertions collected during the course of a request is added to the output cache item
	/// as cache dependencies.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.client" type="Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client"/>
	///  </configSections>
	/// 
	///  <system.web>
	///   <caching>
	///    <outputCache defaultProvider="Xrm">
	///     <providers>
	///      <add
	///       name="Xrm"
	///       type="Adxstudio.Xrm.Caching.ObjectCacheOutputCacheProvider, Adxstudio.Xrm"
	///       objectCacheName="LocalMemory"
	///       cacheItemDependenciesEnabled="true" [true|false]
	///       />
	///     </providers>
	///    </outputCache>
	///   </caching>
	///  </system.web>
	///  
	///  <microsoft.xrm.client>
	///   <objectCache default="Xrm">
	///    <add name="Xrm" type="Adxstudio.Xrm.Caching.OutputObjectCache, Adxstudio.Xrm" innerObjectCacheName="LocalMemory"/>
	///    <add name="LocalMemory"/> <!-- uses MemoryCache by default -->
	///   </objectCache>
	///  </microsoft.xrm.client>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// 
	/// Enable output caching on the ASP.Net page.
	/// <code>
	/// <![CDATA[
	/// <%@ Page ... %>
	/// <%@ OutputCache VaryByParam="*" Duration="60" %>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="OutputObjectCache"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class ObjectCacheOutputCacheProvider : OutputCacheProvider
	{
		private static readonly string _outputCacheDependency = "xrm:dependency:output:*";

		private static readonly string _outputCacheSecondaryDependencyPrefix = "xrm:secondary-dependency:output";

		/// <summary>
		/// The name of the configured <see cref="ObjectCacheElement"/> to be used for caching.
		/// </summary>
		public string ObjectCacheName { get; set; }

		/// <summary>
		/// The name of a specific cache region.
		/// </summary>
		public string CacheRegionName { get; set; }

		/// <summary>
		/// A format string for decorating the cache item keys.
		/// </summary>
		public string CacheKeyFormat { get; set; }

		/// <summary>
		/// The flag to apply cache dependencies to the output cache item. Enabled by default.
		/// </summary>
		public bool CacheItemDependenciesEnabled { get; set; }

		/// <summary>
		/// The flag to also add CacheItemDetails when adding OutputCache items to cache. This allows dependencies of OutputCache items to be 
		/// shown in the cache feed. Disabled by default.
		/// </summary>
		public bool IncludeCacheItemDetails { get; set; }

		public override void Initialize(string name, NameValueCollection config)
		{
			ObjectCacheName = config["objectCacheName"];
			CacheRegionName = config["cacheRegionName"];
			CacheKeyFormat = config["cacheKeyFormat"] ?? @"{0}:{{0}}".FormatWith(this);

			bool cacheItemDependenciesEnabled;
			if (!bool.TryParse(config["cacheItemDependenciesEnabled"], out cacheItemDependenciesEnabled))
			{
				cacheItemDependenciesEnabled = true;    // true by default
			}
			CacheItemDependenciesEnabled = cacheItemDependenciesEnabled;

			bool includeCacheItemDetails;
			IncludeCacheItemDetails = bool.TryParse(config["includeCacheItemDetails"], out includeCacheItemDetails) && includeCacheItemDetails;	// false by default

			base.Initialize(name, config);

			config.Remove("objectCacheName");
			config.Remove("cacheRegionName");
			config.Remove("cacheKeyFormat");
			config.Remove("cacheItemDependenciesEnabled");
			config.Remove("includeCacheItemDetails");

			if (config.Count > 0)
			{
				string unrecognizedAttribute = config.GetKey(0);

				if (!string.IsNullOrEmpty(unrecognizedAttribute))
				{
					throw new ConfigurationErrorsException("The {0} doesn't recognize or support the attribute {1}.".FormatWith(this, unrecognizedAttribute));
				}
			}
		}

		public override object Add(string key, object entry, DateTime utcExpiry)
		{
			var cache = GetCache();
			var cacheKey = GetCacheKey(key);

			var container = new CacheItemContainer(entry);

			if (CacheItemDependenciesEnabled)
			{
				var policy = GetPolicy(cache, utcExpiry, CacheRegionName);

				container.Detail = this.IncludeCacheItemDetails
					? cache.CreateCacheItemDetailObject(cacheKey, policy, CacheRegionName)
					: null;

				var addedItem = cache.AddOrGetExisting(cacheKey, container, policy, CacheRegionName);

				return (addedItem as CacheItemContainer)?.Value;
			}

			return (cache.AddOrGetExisting(cacheKey, container, ToDateTimeOffset(utcExpiry), CacheRegionName) as CacheItemContainer)?.Value;
		}
		
		public override object Get(string key)
		{
			var cache = GetCache();
			var cacheKey = GetCacheKey(key);
			return (cache.Get(cacheKey, CacheRegionName) as CacheItemContainer)?.Value;
		}

		public override void Remove(string key)
		{
			var cache = GetCache();
			var cacheKey = GetCacheKey(key);
			cache.Remove(cacheKey, CacheRegionName);
		}

		public override void Set(string key, object entry, DateTime utcExpiry)
		{
			var cache = GetCache();
			var cacheKey = GetCacheKey(key);

			var container = new CacheItemContainer(entry);

			if (CacheItemDependenciesEnabled)
			{
				var policy = GetPolicy(cache, utcExpiry, CacheRegionName);

				container.Detail = this.IncludeCacheItemDetails
					? cache.CreateCacheItemDetailObject(cacheKey, policy, CacheRegionName)
					: null;

				cache.Set(cacheKey, container, policy, CacheRegionName);

				return;
			}

			cache.Set(cacheKey, container, ToDateTimeOffset(utcExpiry), CacheRegionName);
		}

		public static string GetSecondaryDependencyKey(string key)
		{
			return string.Format("{0}:{1}", _outputCacheSecondaryDependencyPrefix, key.ToLower());
		}
		
		private CacheItemPolicy GetPolicy(ObjectCache cache, DateTime utcExpiry, string regionName)
		{
			// Add an output cache specific dependency item and key
			cache.AddOrGetExisting(_outputCacheDependency, _outputCacheDependency, ObjectCache.InfiniteAbsoluteExpiration, CacheRegionName);

			// Get the keys from HttpContext
			var name = AdxstudioCrmConfigurationManager.GetCrmSection().OutputObjectCacheName;
			var keys = HttpSingleton<OutputCacheKeyCollection>.GetInstance(name, () => new OutputCacheKeyCollection());
			
			// Create Monitor and Policy objects
			var monitorKeys = keys.Select(d => d.ToLower()).ToArray();
			var monitor = GetChangeMonitor(cache, monitorKeys, regionName);
			var policy = monitor.GetCacheItemPolicy(cache.Name);
			policy.AbsoluteExpiration = ToDateTimeOffset(utcExpiry);
			policy.RemovedCallback += CacheEventSource.Log.OnRemovedCallback;
			return policy;
		}
		
		private static CacheEntryChangeMonitor GetChangeMonitor(ObjectCache cache, string[] monitorKeys, string regionName)
		{
			if (!monitorKeys.Any()) return null;

			// Only take the dependencies that currently exist in the cache, some may have been invalidated in the interim.
			// Also filter out output cache secondary dependencies.
			var filteredKeys = monitorKeys.Where(key => cache.Contains(key, regionName) && !key.StartsWith(_outputCacheSecondaryDependencyPrefix)).Distinct();
			if (!filteredKeys.Any())
			{
				return null;
			}

			// For each output cache dependency, create a secondary dependency, this allows callers to invalidate just the output cache without also invalidating
			// other cache items which could be in dirty state.
			var includingSecondaryKeys = new List<string>();
			foreach (var filteredKey in filteredKeys)
			{
				// Each secondary dependency should be dependent on the primary dependency.
				var policy = new CacheItemPolicy();
				policy.ChangeMonitors.Add(cache.CreateCacheEntryChangeMonitor(new[] { filteredKey }, regionName));
				var secondaryDependencyKey = GetSecondaryDependencyKey(filteredKey);
				cache.AddOrGetExisting(secondaryDependencyKey, string.Empty, policy, regionName);
				includingSecondaryKeys.Add(secondaryDependencyKey);
			}
			includingSecondaryKeys.AddRange(filteredKeys);

			return cache.CreateCacheEntryChangeMonitor(includingSecondaryKeys, regionName);
		}

		private ObjectCache GetCache()
		{
			return ObjectCacheManager.GetInstance(ObjectCacheName);
		}

		private static DateTimeOffset ToDateTimeOffset(DateTime dt)
		{
			if (dt == DateTime.MinValue) return DateTimeOffset.MinValue;
			if (dt == DateTime.MaxValue) return DateTimeOffset.MaxValue;
			return dt;
		}

		private string GetCacheKey(string key)
		{
			return CacheKeyFormat.FormatWith(key).ToLower();
		}
	}
}
