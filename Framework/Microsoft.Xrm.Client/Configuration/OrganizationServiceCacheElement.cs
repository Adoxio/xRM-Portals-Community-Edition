/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Runtime.Caching;
using Microsoft.Xrm.Client.Services;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// The configuration settings for <see cref="IOrganizationServiceCache"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="CrmConfigurationManager"/>.
	/// </remarks>
	/// <seealso cref="CrmConfigurationManager"/>
	public sealed class OrganizationServiceCacheElement : InitializableConfigurationElement<IOrganizationServiceCache>
	{
		private const string _defaultOrganizationServiceCacheTypeName = "Microsoft.Xrm.Client.Services.OrganizationServiceCache, Microsoft.Xrm.Client";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationProperty _propObjectCacheName;
		private static readonly ConfigurationProperty _propCacheRegionName;
		private static readonly ConfigurationProperty _propCacheCacheMode;
		private static readonly ConfigurationProperty _propCacheReturnMode;
		private static readonly ConfigurationProperty _propQueryHashingEnabled;

		static OrganizationServiceCacheElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propType = new ConfigurationProperty("type", typeof(string), _defaultOrganizationServiceCacheTypeName, ConfigurationPropertyOptions.None);
			_propObjectCacheName = new ConfigurationProperty("objectCacheName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propCacheRegionName = new ConfigurationProperty("cacheRegionName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propCacheCacheMode = new ConfigurationProperty("cacheMode", typeof(OrganizationServiceCacheMode?), null, ConfigurationPropertyOptions.None);
			_propCacheReturnMode = new ConfigurationProperty("returnMode", typeof(OrganizationServiceCacheReturnMode?), null, ConfigurationPropertyOptions.None);
			_propQueryHashingEnabled = new ConfigurationProperty("queryHashingEnabled", typeof(bool), true, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propName, _propType, _propObjectCacheName, _propCacheRegionName, _propCacheCacheMode, _propCacheReturnMode, _propQueryHashingEnabled };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets or sets the element name.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = null, IsKey = true, IsRequired = true)]
		public override string Name
		{
			get { return (string)base[_propName]; }
			set { base[_propName] = value; }
		}

		/// <summary>
		/// The dependency type name.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue = _defaultOrganizationServiceCacheTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// The name of the nested <see cref="ObjectCacheElement"/> configuration.
		/// </summary>
		[ConfigurationProperty("objectCacheName", DefaultValue = null)]
		public string ObjectCacheName
		{
			get { return (string)base[_propObjectCacheName]; }
			set { base[_propObjectCacheName] = value; }
		}

		/// <summary>
		/// The cache region name.
		/// </summary>
		[ConfigurationProperty("cacheRegionName", DefaultValue = null)]
		public string CacheRegionName
		{
			get { return (string)base[_propCacheRegionName]; }
			set { base[_propCacheRegionName] = value; }
		}

		/// <summary>
		/// The caching behavior mode.
		/// </summary>
		[ConfigurationProperty("cacheMode", DefaultValue = null)]
		public OrganizationServiceCacheMode? CacheMode
		{
			get { return (OrganizationServiceCacheMode?)base[_propCacheCacheMode]; }
			set { base[_propCacheCacheMode] = value; }
		}

		/// <summary>
		/// The cache retrieval mode.
		/// </summary>
		[ConfigurationProperty("returnMode", DefaultValue = null)]
		public OrganizationServiceCacheReturnMode? ReturnMode
		{
			get { return (OrganizationServiceCacheReturnMode?)base[_propCacheReturnMode]; }
			set { base[_propCacheReturnMode] = value; }
		}

		/// <summary>
		/// Indicates that the query used to construct the cache key is to be hashed or left as a readable string.
		/// </summary>
		[ConfigurationProperty("queryHashingEnabled", DefaultValue = true)]
		public bool QueryHashingEnabled
		{
			get { return (bool)base[_propQueryHashingEnabled]; }
			set { base[_propQueryHashingEnabled] = value; }
		}

		/// <summary>
		/// Creates a <see cref="IOrganizationServiceCache"/> object.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="cacheItemPolicyFactory"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public IOrganizationServiceCache CreateOrganizationServiceCache(ObjectCache cache = null, OrganizationServiceCacheSettings settings = null)
		{
			var obj = CreateDependency(
				() => new OrganizationServiceCache(cache, settings),
				cache, settings);

			if (obj is OrganizationServiceCache) PreInitialize(obj as OrganizationServiceCache);

			return Initialize(obj);
		}

		private void PreInitialize(OrganizationServiceCache serviceCache)
		{
			if (CacheMode != null) serviceCache.Mode = CacheMode.Value;
			if (ReturnMode != null) serviceCache.ReturnMode = ReturnMode.Value;
		}
	}
}
