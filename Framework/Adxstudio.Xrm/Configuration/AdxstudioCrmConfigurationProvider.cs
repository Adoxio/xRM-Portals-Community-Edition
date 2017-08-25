/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Threading;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Configuration
{
	public class AdxstudioCrmConfigurationProvider
	{
		private static CrmSection CreateConfiguration()
		{
			var configuration = ConfigurationManager.GetSection(CrmSection.SectionName) as CrmSection ?? new CrmSection();
			var args = new CrmSectionCreatedEventArgs { Configuration = configuration };

			var handler = ConfigurationCreated;

			if (handler != null)
			{
				handler(null, args);
			}

			return args.Configuration;
		}

		/// <summary>
		/// Occurs after the <see cref="CrmSection"/> configuration is created.
		/// </summary>
		public static event EventHandler<CrmSectionCreatedEventArgs> ConfigurationCreated;

		private CrmSection _crmSection;

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public virtual CrmSection GetCrmSection()
		{
			return _crmSection ?? (_crmSection = CreateConfiguration());
		}

		private IEnumerable<IOrganizationServiceEventProvider> _eventProviders;
		private readonly ConcurrentDictionary<string, IEnumerable<IOrganizationServiceEventProvider>> _eventProvidersLookup = new ConcurrentDictionary<string, IEnumerable<IOrganizationServiceEventProvider>>();

		public virtual IEnumerable<IOrganizationServiceEventProvider> CreateEventProviders(string eventName = null, bool allowDefaultFallback = false)
		{
			var section = GetCrmSection();

			var eventElement = section.Events.GetElementOrDefault(eventName, allowDefaultFallback);
			var mode = eventElement.InstanceMode;
			var name = !string.IsNullOrWhiteSpace(eventName) ? eventName : eventElement.Name;

			if (mode == ServiceEventInstanceMode.Static)
			{
				// return a single instance

				return _eventProviders ?? (_eventProviders = eventElement.CreateEventProviders());
			}

			if (mode == ServiceEventInstanceMode.PerName)
			{
				var key = name ?? GetDefaultContextName();

				if (!_eventProvidersLookup.ContainsKey(key))
				{
					_eventProvidersLookup[key] = eventElement.CreateEventProviders();
				}

				return _eventProvidersLookup[key];
			}

			if (mode == ServiceEventInstanceMode.PerRequest && HttpSingleton<IEnumerable<IOrganizationServiceEventProvider>>.Enabled)
			{
				var key = name ?? GetDefaultContextName();

				return HttpSingleton<IEnumerable<IOrganizationServiceEventProvider>>.GetInstance(key, eventElement.CreateEventProviders);
			}

			var providers = eventElement.CreateEventProviders();
			return providers;
		}

		private static IContentMapProvider _contentMapProvider;

		public static void Set(IContentMapProvider contentMapProvider)
		{
			_contentMapProvider = contentMapProvider;
		}

		public virtual IContentMapProvider CreateContentMapProvider(CrmConnection connection, string contentMapProviderName = null, bool allowDefaultFallback = false)
		{
			return _contentMapProvider;
		}

		public IContentMapProvider CreateContentMapProvider(string portalName = null, bool allowDefaultFallback = false)
		{
			return _contentMapProvider;
		}

		private static ISolutionDefinitionProvider _solutionDefinitionProvider;

		public static void Set(ISolutionDefinitionProvider solutionDefinitionProvider)
		{
			_solutionDefinitionProvider = solutionDefinitionProvider;
		}

		public virtual ISolutionDefinitionProvider CreateSolutionDefinitionProvider(string solutionDefinitionProviderName = null, bool allowDefaultFallback = false)
		{
			return _solutionDefinitionProvider;
		}

		private static string GetDefaultContextName()
		{
			var section = CrmConfigurationManager.GetCrmSection();
			var element = section.Contexts.GetElementOrDefault(null);
			return element.Name;
		}
	}
}
