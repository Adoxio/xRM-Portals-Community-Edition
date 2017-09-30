/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Search
{
	public class SearchManager
	{
		public static event EventHandler Initializing;

		private static readonly object _initializeLock = new object();

		private static bool? _enabled;
		private static bool _initialized;
		private static SearchProvider _provider;
		private static ProviderCollection<SearchProvider> _providers;

		public static bool Enabled
		{
			get
			{
				if (!_initialized && !_enabled.HasValue)
				{
					_enabled = AdxstudioCrmConfigurationManager.GetCrmSection().Search.Enabled;
				}

				return _enabled.GetValueOrDefault(false);
			}
		}

		public static SearchProvider Provider
		{
			get
			{
				EnsureEnabled();

				return _provider;
			}
		}

		public static ProviderCollection<SearchProvider> Providers
		{
			get
			{
				EnsureEnabled();

				return _providers;
			}
		}

		public static SearchProvider GetProvider(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return Provider;
			}

			var provider = Providers[name];

			if (provider != null)
			{
				return provider;
			}

			throw new ProviderException("Unable to get search provider with name {0}.".FormatWith(name));
		}

		private static void EnsureEnabled()
		{
			Initialize();

			if (!Enabled)
			{
				throw new SearchDisabledProviderException("The Search feature has not been enabled. Please check your configuration.");
			}
		}

		private static void Initialize()
		{
			if (_initialized)
			{
				return;
			}

			lock (_initializeLock)
			{
				if (_initialized)
				{
					return;
				}

				OnInitializing();

				var searchElement = AdxstudioCrmConfigurationManager.GetCrmSection().Search;

				_enabled = searchElement.Enabled;

				if (_enabled.GetValueOrDefault(false))
				{
					_providers = new ProviderCollection<SearchProvider>();

					foreach (ProviderSettings providerSettings in searchElement.Providers)
					{
						_providers.Add(InstantiateProvider<SearchProvider>(providerSettings));
					}

					_providers.SetReadOnly();

					if (searchElement.DefaultProvider == null)
					{
						throw new ProviderException("Specify a default search provider.");
					}

					try
					{
						_provider = _providers[searchElement.DefaultProvider];
					}
					catch { }

					if (_provider == null)
					{
						var defaultProviderPropertyInformation = searchElement.ElementInformation.Properties["defaultProvider"];

						const string message = "Default Search Provider could not be found.";

						throw defaultProviderPropertyInformation == null
							? (Exception)new ProviderException(message)
							: new ConfigurationErrorsException(message, defaultProviderPropertyInformation.Source, defaultProviderPropertyInformation.LineNumber);
					}
				}

				_initialized = true;
			}
		}

		private static void OnInitializing()
		{
			var handler = Initializing;

			if (handler != null)
			{
				handler(null, new EventArgs());
			}
		}

		private static TProvider InstantiateProvider<TProvider>(ProviderSettings settings) where TProvider : ProviderBase
		{
			try
			{
				var typeSetting = settings.Type == null ? null : settings.Type.Trim();

				if (string.IsNullOrEmpty(typeSetting))
				{
					throw new ArgumentException("Type_Name_Required_For_Provider_Exception (Key present in resx file with same string)");
				}

				var providerType = Type.GetType(settings.Type, true, true);

				if (!typeof(TProvider).IsAssignableFrom(providerType))
				{
					throw new ArgumentException("Provider must implement the class {0}.".FormatWith(typeof(TProvider)));
				}

				var provider = (TProvider)Activator.CreateInstance(providerType);

				var parameters = settings.Parameters;
				var config = new NameValueCollection(parameters.Count, StringComparer.Ordinal);

				foreach (string key in parameters)
				{
					config[key] = parameters[key];
				}

				provider.Initialize(settings.Name, config);

				return provider;
			}
			catch (Exception e)
			{
				if (e is ConfigurationException)
				{
					throw;
				}

				var typePropertyInformation = settings.ElementInformation.Properties["type"];

				if (typePropertyInformation == null)
				{
					throw;
				}

				throw new ConfigurationErrorsException(e.Message, typePropertyInformation.Source, typePropertyInformation.LineNumber);
			}
		}
	}
}
