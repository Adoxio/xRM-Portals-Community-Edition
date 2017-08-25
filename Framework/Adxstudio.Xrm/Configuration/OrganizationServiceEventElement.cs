/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// The modes in which the <see cref="AdxstudioCrmConfigurationManager"/> instantiates <see cref="IOrganizationServiceEventProvider"/> objects.
	/// </summary>
	public enum ServiceEventInstanceMode
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
	/// The configuration element for <see cref="IOrganizationService"/> events.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="AdxstudioCrmConfigurationManager"/>.
	/// </remarks>
	public class OrganizationServiceEventElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propProviders;
		private static readonly ConfigurationProperty _propInstanceMode;
		private static readonly ConfigurationPropertyCollection _properties;

		static OrganizationServiceEventElement()
		{
			_propName = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			_propProviders = new ConfigurationProperty(OrganizationServiceEventProviderElementCollection.Name, typeof(OrganizationServiceEventProviderElementCollection), new OrganizationServiceEventProviderElementCollection(), ConfigurationPropertyOptions.IsRequired);
			_propInstanceMode = new ConfigurationProperty("instanceMode", typeof(ServiceEventInstanceMode), ServiceEventInstanceMode.PerInstance, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propName, _propProviders, _propInstanceMode };
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
		/// Gets or sets the element name.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = null, IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)base[_propName]; }
			set { base[_propName] = value; }
		}

		/// <summary>
		/// Gets the collection of <see cref="Services.IOrganizationServiceEventProvider"/> to be initialized on the service.
		/// </summary>
		[ConfigurationProperty(OrganizationServiceEventProviderElementCollection.Name, IsDefaultCollection = false, IsRequired = true)]
		public OrganizationServiceEventProviderElementCollection Providers
		{
			get { return (OrganizationServiceEventProviderElementCollection)base[_propProviders]; }
			set { base[_propProviders] = value; }
		}

		/// <summary>
		/// The instance mode.
		/// </summary>
		[ConfigurationProperty("instanceMode", DefaultValue = ServiceEventInstanceMode.PerInstance)]
		public ServiceEventInstanceMode InstanceMode
		{
			get { return (ServiceEventInstanceMode)base[_propInstanceMode]; }
			set { base[_propInstanceMode] = value; }
		}

		/// <summary>
		/// Creates a collection of <see cref="IOrganizationServiceEventProvider"/> object.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOrganizationServiceEventProvider> CreateEventProviders()
		{
			return Providers.Cast<OrganizationServiceEventProviderElement>().Select(p => p.CreateEventProvider()).ToList();
		}
	}
}
