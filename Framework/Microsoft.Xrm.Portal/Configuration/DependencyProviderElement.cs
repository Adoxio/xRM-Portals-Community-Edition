/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// The configuration settings for <see cref="IDependencyProvider"/> dependencies.
	/// </summary>
	/// <remarks>
	/// For an example of the configuration format refer to the <see cref="PortalCrmConfigurationManager"/>.
	/// </remarks>
	public sealed class DependencyProviderElement : InitializableConfigurationElement<IDependencyProvider>
	{
		private const string _defaultDependencyProviderTypeName = "Microsoft.Xrm.Portal.Configuration.DependencyProvider, Microsoft.Xrm.Portal";

		private static readonly ConfigurationPropertyCollection _properties;
		private static readonly ConfigurationProperty _propType;

		static DependencyProviderElement()
		{
			_propType = new ConfigurationProperty("type", typeof(string), _defaultDependencyProviderTypeName, ConfigurationPropertyOptions.None);
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
		[ConfigurationProperty("type", DefaultValue = _defaultDependencyProviderTypeName)]
		public override string Type
		{
			get { return (string)base[_propType]; }
			set { base[_propType] = value; }
		}

		private IDependencyProvider _dependencyProvider;

		/// <summary>
		/// Creates a <see cref="IDependencyProvider"/> object.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public IDependencyProvider GetDependencyProvider(string portalName)
		{
			if (_dependencyProvider == null)
			{
				_dependencyProvider = CreateDependencyAndInitialize(
					() => new DependencyProvider(portalName),
					portalName);
			}

			return _dependencyProvider;
		}

		protected override void Reset(ConfigurationElement parentElement)
		{
			base.Reset(parentElement);

			_dependencyProvider = null;
		}
	}
}
