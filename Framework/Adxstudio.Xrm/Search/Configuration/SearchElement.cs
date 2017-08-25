/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Adxstudio.Xrm.Search.Configuration
{
	public sealed class SearchElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _propDefaultProvider;
		private static readonly ConfigurationProperty _propEnabled;
		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propProviders;

		[ConfigurationProperty("defaultProvider", DefaultValue = null)]
		public string DefaultProvider
		{
			get { return (string)base[_propDefaultProvider]; }
			set { base[_propDefaultProvider] = value; }
		}

		[ConfigurationProperty("enabled", DefaultValue = true)]
		public bool Enabled
		{
			get { return (bool)base[_propEnabled]; }
			set { base[_propEnabled] = value; }
		}

		[ConfigurationProperty("providers")]
		public SearchProviderSettingsCollection Providers
		{
			get { return (SearchProviderSettingsCollection)base[_propProviders]; }
			set { base[_propProviders] = value; }
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		static SearchElement()
		{
			_propDefaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
			_propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
			_propProviders = new ConfigurationProperty("providers", typeof(SearchProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
			_properties = new ConfigurationPropertyCollection { _propDefaultProvider, _propEnabled, _propProviders };
		}
	}
}
