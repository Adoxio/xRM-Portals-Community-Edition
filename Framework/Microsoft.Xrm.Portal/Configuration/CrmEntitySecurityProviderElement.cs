/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Cms.Security;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// The configuration settings for <see cref="ICrmEntitySecurityProvider"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="PortalCrmConfigurationManager"/>.
	/// </remarks>
	public sealed class CrmEntitySecurityProviderElement : InitializableConfigurationElement<ICrmEntitySecurityProvider>
	{
		private const string _defaultCrmEntitySecurityProviderTypeName = "Microsoft.Xrm.Portal.Cms.Security.CmsCrmEntitySecurityProvider, Microsoft.Xrm.Portal";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propType;

		static CrmEntitySecurityProviderElement()
		{
			_propType = new ConfigurationProperty("type", typeof(string), _defaultCrmEntitySecurityProviderTypeName, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection { _propType };
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets or sets the element name.
		/// </summary>
		public override string Name { get; set; }

		/// <summary>
		/// The dependency type name.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue = _defaultCrmEntitySecurityProviderTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// Returns a new instance of the configured <see cref="ICrmEntitySecurityProvider"/>.
		/// </summary>
		/// <returns></returns>
		public ICrmEntitySecurityProvider CreateCrmEntitySecurityProvider(string portalName)
		{
			return CreateDependencyAndInitialize(() => new CmsCrmEntitySecurityProvider(portalName), portalName);
		}
	}
}
