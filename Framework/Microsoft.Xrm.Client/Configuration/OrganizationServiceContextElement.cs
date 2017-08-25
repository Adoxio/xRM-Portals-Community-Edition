/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client.Configuration
{
	/// <summary>
	/// The configuration settings for <see cref="OrganizationServiceContext"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="CrmConfigurationManager"/>.
	/// </remarks>
	/// <seealso cref="CrmConfigurationManager"/>
	public sealed class OrganizationServiceContextElement : InitializableConfigurationElement<OrganizationServiceContext>
	{
		private const string _defaultOrganizationServiceContextTypeName = "Microsoft.Xrm.Client.CrmOrganizationServiceContext, Microsoft.Xrm.Client";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationProperty _propServiceName;
		private static readonly ConfigurationProperty _propConnectionStringName;

		static OrganizationServiceContextElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propType = new ConfigurationProperty("type", typeof(string), _defaultOrganizationServiceContextTypeName, ConfigurationPropertyOptions.None);
			_propServiceName = new ConfigurationProperty("serviceName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propConnectionStringName = new ConfigurationProperty("connectionStringName", typeof(string), null, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propName, _propType, _propServiceName, _propConnectionStringName };
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
		[ConfigurationProperty("type", DefaultValue = _defaultOrganizationServiceContextTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// The name of the nested <see cref="OrganizationServiceElement"/> configuration.
		/// </summary>
		[ConfigurationProperty("serviceName", DefaultValue = null)]
		public string ServiceName
		{
			get { return (string)base[_propServiceName]; }
			set { base[_propServiceName] = value; }
		}

		/// <summary>
		/// The name of the service connection string.
		/// </summary>
		[ConfigurationProperty("connectionStringName", DefaultValue = null)]
		public string ConnectionStringName
		{
			get { return (string)base[_propConnectionStringName]; }
			set { base[_propConnectionStringName] = value; }
		}

		/// <summary>
		/// Creates a <see cref="OrganizationServiceContext"/> object.
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public OrganizationServiceContext CreateOrganizationServiceContext(IOrganizationService service)
		{
			return CreateDependencyAndInitialize(
				() => new CrmOrganizationServiceContext(service),
				service);
		}
	}
}
