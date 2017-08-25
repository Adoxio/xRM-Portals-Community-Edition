/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	/// <summary>
	/// The configuration settings for the <see cref="CacheFeedHandler"/>.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="AdxstudioCrmConfigurationManager"/>.
	/// </remarks>
	public class CacheFeedElement : ConfigurationElement
	{
		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propEnabled;
		private static readonly ConfigurationProperty _propLocalOnly;
		private static readonly ConfigurationProperty _propTraced;
		private static readonly ConfigurationProperty _propShowValues;
		private static readonly ConfigurationProperty _propObjectCacheName;
		private static readonly ConfigurationProperty _propStylesheet;
		private static readonly ConfigurationProperty _propContentType;

		static CacheFeedElement()
		{
			_propEnabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
			_propLocalOnly = new ConfigurationProperty("localOnly", typeof(bool), true, ConfigurationPropertyOptions.None);
			_propTraced = new ConfigurationProperty("traced", typeof(bool), true, ConfigurationPropertyOptions.None);
			_propShowValues = new ConfigurationProperty("showValues", typeof(bool), true, ConfigurationPropertyOptions.None);
			_propObjectCacheName = new ConfigurationProperty("objectCacheName", typeof(string), null, ConfigurationPropertyOptions.None);
			_propStylesheet = new ConfigurationProperty("stylesheet", typeof(string), null, ConfigurationPropertyOptions.None);
			_propContentType = new ConfigurationProperty("contentType", typeof(string), null, ConfigurationPropertyOptions.None);
			_properties = new ConfigurationPropertyCollection
			{
				_propEnabled,
				_propLocalOnly,
				_propTraced,
				_propShowValues,
				_propObjectCacheName,
				_propStylesheet,
				_propContentType,
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
		/// Enables or disables the <see cref="CacheFeedHandler"/>.
		/// </summary>
		[ConfigurationProperty("enabled", DefaultValue = false)]
		public bool Enabled
		{
			get { return (bool)base[_propEnabled]; }
			set { base[_propEnabled] = value; }
		}

		/// <summary>
		/// Requires that the <see cref="CacheFeedHandler"/> be accessed locally.
		/// </summary>
		[ConfigurationProperty("localOnly", DefaultValue = true)]
		public bool LocalOnly
		{
			get { return (bool)base[_propLocalOnly]; }
			set { base[_propLocalOnly] = value; }
		}

		/// <summary>
		/// Enables or disables tracing of <see cref="CacheFeedHandler"/> requests.
		/// </summary>
		[ConfigurationProperty("traced", DefaultValue = true)]
		public bool Traced
		{
			get { return (bool)base[_propTraced]; }
			set { base[_propTraced] = value; }
		}

		/// <summary>
		/// Enables or disables details of the cache item values.
		/// </summary>
		[ConfigurationProperty("showValues", DefaultValue = true)]
		public bool ShowValues
		{
			get { return (bool)base[_propShowValues]; }
			set { base[_propShowValues] = value; }
		}

		/// <summary>
		/// Specifies a specific <see cref="ObjectCacheElement"/> to be displayed. Leave uninitialized to display all of the configured caches.
		/// </summary>
		[ConfigurationProperty("objectCacheName", DefaultValue = null)]
		public string ObjectCacheName
		{
			get { return (string)base[_propObjectCacheName]; }
			set { base[_propObjectCacheName] = value; }
		}

		/// <summary>
		/// The URL to the XSL stylesheet to transform the response content.
		/// </summary>
		[ConfigurationProperty("stylesheet", DefaultValue = null)]
		public string Stylesheet
		{
			get { return (string)base[_propStylesheet]; }
			set { base[_propStylesheet] = value; }
		}

		/// <summary>
		/// The desired content type of the cache feed output.
		/// </summary>
		[ConfigurationProperty("contentType", DefaultValue = "text/json")]
		public string ContentType
		{
			get { return (string)base[_propContentType]; }
			set { base[_propContentType] = value; }
		}
	}
}
