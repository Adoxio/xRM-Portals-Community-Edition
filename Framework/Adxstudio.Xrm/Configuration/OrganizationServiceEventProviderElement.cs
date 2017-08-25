/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Adxstudio.Xrm.Cms.Replication;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// The configuration element for declaring an <see cref="IOrganizationServiceEventProvider"/>.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="AdxstudioCrmConfigurationManager"/>.
	/// </remarks>
	public class OrganizationServiceEventProviderElement : InitializableConfigurationElement<IOrganizationServiceEventProvider>
	{
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propType;
		private static readonly ConfigurationPropertyCollection _properties;

		static OrganizationServiceEventProviderElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.None);
			_propType = new ConfigurationProperty("type", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

			_properties = new ConfigurationPropertyCollection { _propName, _propType };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets or sets the element name.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = null)]
		public override string Name
		{
			get { return (string)base[_propName]; }
			set { base[_propName] = value; }
		}

		/// <summary>
		/// Gets or sets the type of the <see cref="IOrganizationServiceEventProvider"/> to load.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue = null, IsRequired = true)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// Creates a <see cref="IOrganizationServiceEventProvider"/> object.
		/// </summary>
		/// <returns></returns>
		public IOrganizationServiceEventProvider CreateEventProvider()
		{
			return CreateDependencyAndInitialize(() => new CmsReplicationOrganizationServiceEventProvider());
		}
	}
}
