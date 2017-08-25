/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using Microsoft.Xrm.Client.Threading;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// Represents a collection of site configuration nodes and settings for all sites in general.
	/// </summary>
	public sealed class CrmSection : ConfigurationSection
	{
		private const string _defaultConfigurationProviderType = "Microsoft.Xrm.Client.Configuration.CrmConfigurationProvider, Microsoft.Xrm.Client";
		private const string _defaultLockProviderType = "Microsoft.Xrm.Client.Threading.MutexLockProvider, Microsoft.Xrm.Client";
		private const string _defaultObjectCacheProviderType = "Microsoft.Xrm.Client.Caching.ObjectCacheProvider, Microsoft.Xrm.Client";

		/// <summary>
		/// The element name of the section.
		/// </summary>
		public const string SectionName = "microsoft.xrm.client";

		private static readonly ConfigurationProperty _propConfigurationProviderType;
		private static readonly ConfigurationProperty _propLockProviderType;
		private static readonly ConfigurationProperty _propObjectCacheProviderType;
		private static readonly ConfigurationProperty _propMutexTimeout;
		private static readonly ConfigurationProperty _propConnectionStrings;
		private static readonly ConfigurationProperty _propContexts;
		private static readonly ConfigurationProperty _propServices;
		private static readonly ConfigurationProperty _propServiceCache;
		private static readonly ConfigurationProperty _propObjectCache;
		private static readonly ConfigurationPropertyCollection _properties;

		static CrmSection()
		{
			_propConfigurationProviderType = new ConfigurationProperty("sectionProviderType", typeof(string), _defaultConfigurationProviderType, ConfigurationPropertyOptions.None);
			_propLockProviderType = new ConfigurationProperty("mutexProviderType", typeof(string), _defaultLockProviderType, ConfigurationPropertyOptions.None);
			_propObjectCacheProviderType = new ConfigurationProperty("objectCacheProviderType", typeof(string), _defaultObjectCacheProviderType, ConfigurationPropertyOptions.None);
			_propMutexTimeout = new ConfigurationProperty("mutexTimeout", typeof(TimeSpan?), null, ConfigurationPropertyOptions.None);
			_propConnectionStrings = new ConfigurationProperty(CrmConnectionStringSettingsCollection.Name, typeof(CrmConnectionStringSettingsCollection), new CrmConnectionStringSettingsCollection(), ConfigurationPropertyOptions.None);
			_propContexts = new ConfigurationProperty(OrganizationServiceContextElementCollection.Name, typeof(OrganizationServiceContextElementCollection), new OrganizationServiceContextElementCollection(), ConfigurationPropertyOptions.IsRequired);
			_propServices = new ConfigurationProperty(OrganizationServiceElementCollection.Name, typeof(OrganizationServiceElementCollection), new OrganizationServiceElementCollection(), ConfigurationPropertyOptions.None);
			_propServiceCache = new ConfigurationProperty(OrganizationServiceCacheElementCollection.Name, typeof(OrganizationServiceCacheElementCollection), new OrganizationServiceCacheElementCollection(), ConfigurationPropertyOptions.None);
			_propObjectCache = new ConfigurationProperty(ObjectCacheElementCollection.Name, typeof(ObjectCacheElementCollection), new ObjectCacheElementCollection(), ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propConfigurationProviderType, _propMutexTimeout, _propConnectionStrings, _propContexts, _propServices, _propServiceCache, _propObjectCache, _propLockProviderType, _propObjectCacheProviderType };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		/// <summary>
		/// The configuration provider type name.
		/// </summary>
		[ConfigurationProperty("sectionProviderType", DefaultValue = _defaultConfigurationProviderType)]
		public string ConfigurationProviderType
		{
			get { return (string)base[_propConfigurationProviderType]; }
			set { base[_propConfigurationProviderType] = value; }
		}

		/// <summary>
		/// The lock provider type name.
		/// </summary>
		[ConfigurationProperty("mutexProviderType", DefaultValue = _defaultLockProviderType)]
		public string LockProviderType
		{
			get { return (string)base[_propLockProviderType]; }
			set { base[_propLockProviderType] = value; }
		}

		/// <summary>
		/// The object cache provider type name.
		/// </summary>
		[ConfigurationProperty("objectCacheProviderType", DefaultValue = _defaultObjectCacheProviderType)]
		public string ObjectCacheProviderType
		{
			get { return (string)base[_propObjectCacheProviderType]; }
			set { base[_propObjectCacheProviderType] = value; }
		}

		/// <summary>
		/// The default timeout used by the <see cref="MutexExtensions"/>.
		/// </summary>
		[ConfigurationProperty("mutexTimeout")]
		public TimeSpan? MutexTimeout
		{
			get { return (TimeSpan?)base[_propMutexTimeout]; }
			set { base[_propMutexTimeout] = value; }
		}

		/// <summary>
		/// A collection of <see cref="ConnectionStringSettings"/>.
		/// </summary>
		[ConfigurationProperty(CrmConnectionStringSettingsCollection.Name)]
		public CrmConnectionStringSettingsCollection ConnectionStrings
		{
			get { return (CrmConnectionStringSettingsCollection)base[_propConnectionStrings]; }
			set { base[_propConnectionStrings] = value; }
		}

		/// <summary>
		/// A collection of <see cref="OrganizationServiceContextElement"/>.
		/// </summary>
		[ConfigurationProperty(OrganizationServiceContextElementCollection.Name, IsDefaultCollection = false, IsRequired = true)]
		public OrganizationServiceContextElementCollection Contexts
		{
			get { return (OrganizationServiceContextElementCollection)base[_propContexts]; }
			set { base[_propContexts] = value; }
		}

		/// <summary>
		/// A collection of <see cref="OrganizationServiceElement"/>.
		/// </summary>
		[ConfigurationProperty(OrganizationServiceElementCollection.Name)]
		public OrganizationServiceElementCollection Services
		{
			get { return (OrganizationServiceElementCollection)base[_propServices]; }
			set { base[_propServices] = value; }
		}

		/// <summary>
		/// A collection of <see cref="OrganizationServiceCacheElement"/>.
		/// </summary>
		[ConfigurationProperty(OrganizationServiceCacheElementCollection.Name)]
		public OrganizationServiceCacheElementCollection ServiceCache
		{
			get { return (OrganizationServiceCacheElementCollection)base[_propServiceCache]; }
			set { base[_propServiceCache] = value; }
		}

		/// <summary>
		/// A collection of <see cref="ObjectCacheElement"/>.
		/// </summary>
		[ConfigurationProperty(ObjectCacheElementCollection.Name)]
		public ObjectCacheElementCollection ObjectCache
		{
			get { return (ObjectCacheElementCollection)base[_propObjectCache]; }
			set { base[_propObjectCache] = value; }
		}

		protected override void Reset(ConfigurationElement parentElement)
		{
			base.Reset(parentElement);
			CrmConfigurationManager.Reset();
		}
	}
}
