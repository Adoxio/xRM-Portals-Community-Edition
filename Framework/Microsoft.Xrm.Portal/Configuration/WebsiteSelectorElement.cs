/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Cms.WebsiteSelectors;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// The configuration settings for <see cref="IWebsiteSelector"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="PortalCrmConfigurationManager"/>.
	/// </remarks>
	public sealed class WebsiteSelectorElement : InitializableConfigurationElement<IWebsiteSelector>
	{
		private const string DefaultWebsiteSelectorTypeName = "Microsoft.Xrm.Portal.Cms.WebsiteSelectors.NameWebsiteSelector, Microsoft.Xrm.Portal";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propType;

		static WebsiteSelectorElement()
		{
			_propType = new ConfigurationProperty("type", typeof(string), DefaultWebsiteSelectorTypeName, ConfigurationPropertyOptions.None);
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
		[ConfigurationProperty("type", DefaultValue = DefaultWebsiteSelectorTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		/// <summary>
		/// Creates a <see cref="IWebsiteSelector"/> object.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public IWebsiteSelector CreateWebsiteSelector(string portalName = null)
		{
			return CreateDependencyAndInitialize(() => new NameWebsiteSelector(portalName), portalName);
		}
	}
}
