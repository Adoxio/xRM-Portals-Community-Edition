/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Web;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// Represents a collection of site configuration nodes and settings for all sites in general.
	/// </summary>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	public sealed class PortalCrmSection : ConfigurationSection
	{
		private const string _defaultConfigurationProviderType = "Microsoft.Xrm.Portal.Configuration.PortalCrmConfigurationProvider, Microsoft.Xrm.Portal";

		/// <summary>
		/// The element name of the section.
		/// </summary>
		public const string SectionName = "microsoft.xrm.portal";

		private const bool _defaultRewriteVirtualPathEnabled = true;

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propConfigurationProviderType;
		private static readonly ConfigurationProperty _propPortals;
		private static readonly ConfigurationProperty _propCachePolicy;
		private static readonly ConfigurationProperty _propServiceBusObjectCacheName;
		private static readonly ConfigurationProperty _propRewriteVirtualPathEnabled;

		static PortalCrmSection()
		{
			_propConfigurationProviderType = new ConfigurationProperty("sectionProviderType", typeof(string), _defaultConfigurationProviderType, ConfigurationPropertyOptions.None);
			_propPortals = new ConfigurationProperty(PortalContextElementCollection.Name, typeof(PortalContextElementCollection), new PortalContextElementCollection(), ConfigurationPropertyOptions.None);
			_propCachePolicy = new ConfigurationProperty("cachePolicy", typeof(PortalCachePolicyElement), new PortalCachePolicyElement(), ConfigurationPropertyOptions.None);
			_propServiceBusObjectCacheName = new ConfigurationProperty("serviceBusObjectCacheName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propRewriteVirtualPathEnabled = new ConfigurationProperty("rewriteVirtualPathEnabled", typeof(bool), _defaultRewriteVirtualPathEnabled, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propConfigurationProviderType, _propPortals, _propCachePolicy, _propServiceBusObjectCacheName, _propRewriteVirtualPathEnabled };
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
		/// A collection of <see cref="PortalContextElement"/>.
		/// </summary>
		[ConfigurationProperty(PortalContextElementCollection.Name)]
		public PortalContextElementCollection Portals
		{
			get { return (PortalContextElementCollection)base[_propPortals]; }
			set { base[_propPortals] = value; }
		}

		/// <summary>
		/// The settings in which the <see cref="HttpCachePolicy"/> of the <see cref="HttpResponse"/> will cache rendered media.
		/// </summary>
		[ConfigurationProperty("cachePolicy")]
		public PortalCachePolicyElement CachePolicy
		{
			get { return (PortalCachePolicyElement)base[_propCachePolicy]; }
			set { base[_propCachePolicy] = value; }
		}

		/// <summary>
		/// The name of the <see cref="ObjectCacheElement"/> designated as the AppFabric Service Bus cache provider.
		/// </summary>
		[ConfigurationProperty("serviceBusObjectCacheName", DefaultValue = null)]
		public string ServiceBusObjectCacheName
		{
			get { return (string)base[_propServiceBusObjectCacheName]; }
			set { base[_propServiceBusObjectCacheName] = value; }
		}

		/// <summary>
		/// Enables detection of incoming virtual paths (those prefixed by "~/") from the client. If found, the context URL is rewritten to the virtual path.
		/// </summary>
		[ConfigurationProperty("rewriteVirtualPathEnabled", DefaultValue = _defaultRewriteVirtualPathEnabled)]
		public bool RewriteVirtualPathEnabled
		{
			get { return (bool)base[_propRewriteVirtualPathEnabled]; }
			set { base[_propRewriteVirtualPathEnabled] = value; }
		}

		protected override void Reset(ConfigurationElement parentElement)
		{
			base.Reset(parentElement);
			PortalCrmConfigurationManager.Reset();
		}
	}
}
