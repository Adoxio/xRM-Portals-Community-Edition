/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Caching
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Runtime.Caching;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Threading;
	using Adxstudio.Xrm.Web.Modules;
	using Microsoft.Xrm.Client.Caching;
	using Microsoft.Xrm.Client.Configuration;

	/// <summary>
	/// A custom <see cref="ObjectCache"/> that propagates cache dependencies to the ASP.Net output cache.
	/// </summary>
	/// <remarks>
	/// The basic configuration uses a nested <see cref="MemoryCache"/> to provide caching services.
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
	///      <add name="Xrm" type="Adxstudio.Xrm.Caching.ObjectCacheOutputCacheProvider, Adxstudio.Xrm"/>
	///     </providers>
	///    </outputCache>
	///   </caching>
	///  </system.web>
	/// 
	///  <microsoft.xrm.client>
	///   <objectCache default="Xrm">
	///    <add name="Xrm" type="Adxstudio.Xrm.Caching.OutputObjectCache, Adxstudio.Xrm"/>
	///   </objectCache>
	///  </microsoft.xrm.client>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="ObjectCacheOutputCacheProvider"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class OutputObjectCache : CompositeObjectCache
	{
		private static string[] _excludeKeys =
		{
			typeof(CacheItemDetail).ToString().ToLower(),
			typeof(CacheItemTelemetry).ToString().ToLower(),
			"xrm:dependency:entity"	// We don't need output cache to be dependent on these entity tags, just on the queries themselves (which should get invalidated anyway when one of these tags is invalidated).
		};

		/// <summary>
		/// A name to distinguish the current <see cref="OutputObjectCache"/> in order for the <see cref="OutputCacheModule"/> to perform selective caching.
		/// </summary>
		public string OutputObjectCacheName { get; private set; }

		public OutputObjectCache()
		{
		}

		public OutputObjectCache(ObjectCache cache)
			: base(cache)
		{
		}

		public OutputObjectCache(string name, ObjectCache cache)
			: base(name, cache)
		{
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			OutputObjectCacheName = config["outputObjectCacheName"]
				?? AdxstudioCrmConfigurationManager.GetCrmSection().OutputObjectCacheName;

			base.Initialize(name, config);
		}

		public override object Get(string key, string regionName = null)
		{
			AddKey(key);

			return base.Get(key, regionName);
		}

		public override CacheItem GetCacheItem(string key, string regionName = null)
		{
			AddKey(key);

			return base.GetCacheItem(key, regionName);
		}

		public override IDictionary<string, object> GetValues(string regionName, params string[] keys)
		{
			foreach (var key in keys)
			{
				AddKey(key);
			}

			return base.GetValues(regionName, keys);
		}

		public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
		{
			foreach (var key in keys)
			{
				AddKey(key);
			}

			return base.GetValues(keys, regionName);
		}

		public override bool Add(CacheItem item, CacheItemPolicy policy)
		{
			AddKey(item.Key);

			return base.Add(item, policy);
		}

		public override bool Add(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			AddKey(key);

			return base.Add(key, value, absoluteExpiration, regionName);
		}

		public override bool Add(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			AddKey(key);

			return base.Add(key, value, policy, regionName);
		}

		public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			AddKey(key);

			return base.AddOrGetExisting(key, value, absoluteExpiration, regionName);
		}

		public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
		{
			AddKey(value.Key);

			return base.AddOrGetExisting(value, policy);
		}

		public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			AddKey(key);

			return base.AddOrGetExisting(key, value, policy, regionName);
		}

		public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
		{
			AddKey(key);

			base.Set(key, value, absoluteExpiration, regionName);
		}

		public override void Set(CacheItem item, CacheItemPolicy policy)
		{
			AddKey(item.Key);

			base.Set(item, policy);
		}

		public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			AddKey(key);

			base.Set(key, value, policy, regionName);
		}

		public override object this[string key]
		{
			get { return base[key]; }

			set
			{
				AddKey(key);

				base[key] = value;
			}
		}

		private bool AddKey(string key)
		{
			if (_excludeKeys.Any(key.StartsWith))
			{
				return false;
			}

			return HttpSingleton<OutputCacheKeyCollection>.Enabled
				? HttpSingleton<OutputCacheKeyCollection>.GetInstance(OutputObjectCacheName, () => new OutputCacheKeyCollection()).Add(key)
				: false;
		}
	}
}
