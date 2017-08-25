/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using System.Web;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// The modes in which the <see cref="CrmConfigurationManager"/> instantiates <see cref="IOrganizationService"/> objects.
	/// </summary>
	public enum OrganizationServiceInstanceMode
	{
		/// <summary>
		/// Create a static instance.
		/// </summary>
		Static,

		/// <summary>
		/// Create an instance for each element name.
		/// </summary>
		PerName,

		/// <summary>
		/// Create an instance for each web request.
		/// </summary>
		/// <remarks>
		/// In the absense of a <see cref="HttpContext"/>, this setting equals PerInstance.
		/// </remarks>
		PerRequest,

		/// <summary>
		/// Create an instance on every invocation.
		/// </summary>
		PerInstance,
	}

	/// <summary>
	/// The configuration settings for <see cref="IOrganizationService"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="CrmConfigurationManager"/>.
	/// </remarks>
	/// <seealso cref="CrmConfigurationManager"/>
	public sealed class OrganizationServiceElement : InitializableConfigurationElement<IOrganizationService>
	{
		private const string _defaultOrganizationServiceTypeName = "Microsoft.Xrm.Client.Services.CachedOrganizationService, Microsoft.Xrm.Client";
		private const OrganizationServiceInstanceMode _defaultInstanceMode = OrganizationServiceInstanceMode.PerRequest;

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationProperty _propServiceCacheName;
		private static readonly ConfigurationProperty _propObjectCacheName;
		private static readonly ConfigurationProperty _propInstanceMode;

		static OrganizationServiceElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propType = new ConfigurationProperty("type", typeof(string), _defaultOrganizationServiceTypeName, ConfigurationPropertyOptions.None);
			_propServiceCacheName = new ConfigurationProperty("serviceCacheName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propObjectCacheName = new ConfigurationProperty("objectCacheName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propInstanceMode = new ConfigurationProperty("instanceMode", typeof(OrganizationServiceInstanceMode), _defaultInstanceMode, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propName, _propType, _propServiceCacheName, _propObjectCacheName, _propInstanceMode };
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
		[ConfigurationProperty("type", DefaultValue = _defaultOrganizationServiceTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// The name of the nested <see cref="OrganizationServiceCacheElement"/> configuration.
		/// </summary>
		[ConfigurationProperty("serviceCacheName", DefaultValue = null)]
		public string ServiceCacheName
		{
			get { return (string)base[_propServiceCacheName]; }
			set { base[_propServiceCacheName] = value; }
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
		/// The instance mode.
		/// </summary>
		[ConfigurationProperty("instanceMode", DefaultValue = _defaultInstanceMode)]
		public OrganizationServiceInstanceMode InstanceMode
		{
			get { return (OrganizationServiceInstanceMode)base[_propInstanceMode]; }
			set { base[_propInstanceMode] = value; }
		}

		/// <summary>
		/// Creates a <see cref="IOrganizationService"/> object.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="serviceCache"></param>
		/// <returns></returns>
		public IOrganizationService CreateOrganizationService(CrmConnection connection, IOrganizationServiceCache serviceCache)
		{
			return CreateDependencyAndInitialize(
				() => new CachedOrganizationService(connection, serviceCache),
				new object[] { connection, serviceCache },
				() => new OrganizationService(connection),
				new object[] { connection });
		}
	}
}
