/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// The configuration settings for content map features.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="AdxstudioCrmConfigurationManager"/>.
	/// </remarks>
	public class ContentMapElement : ConfigurationElement
	{
		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propEnabled;

		static ContentMapElement()
		{
			_propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);

			_properties = new ConfigurationPropertyCollection
			{
				_propEnabled,
			};
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
		/// Enables or disables the content map.
		/// </summary>
		[ConfigurationProperty("enabled", DefaultValue = true)]
		public bool Enabled
		{
			get { return (bool)base[_propEnabled]; }
			set { base[_propEnabled] = value; }
		}
	}
}
