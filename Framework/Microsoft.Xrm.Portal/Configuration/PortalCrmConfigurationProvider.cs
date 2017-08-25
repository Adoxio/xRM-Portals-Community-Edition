/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Web.Routing;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Configuration
{
	/// <summary>
	/// Provides portal configuration settings based on the <see cref="ConfigurationManager"/>.
	/// </summary>
	public class PortalCrmConfigurationProvider
	{
		private static PortalCrmSection CreateConfiguration()
		{
			var configuration = ConfigurationManager.GetSection(PortalCrmSection.SectionName) as PortalCrmSection ?? new PortalCrmSection();
			var args = new PortalCrmSectionCreatedEventArgs { Configuration = configuration };

			var handler = ConfigurationCreated;

			if (handler != null)
			{
				handler(null, args);
			}

			return args.Configuration;
		}

		/// <summary>
		/// Occurs after the <see cref="PortalCrmSection"/> configuration is created.
		/// </summary>
		public static event EventHandler<PortalCrmSectionCreatedEventArgs> ConfigurationCreated;

		private PortalCrmSection _portalCrmSection;

		/// <summary>
		/// Retrieves the configuration section.
		/// </summary>
		/// <returns></returns>
		public virtual PortalCrmSection GetPortalCrmSection()
		{
			return _portalCrmSection ?? (_portalCrmSection = CreateConfiguration());
		}

		/// <summary>
		/// Retrieves the configuration element for the portal.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual PortalContextElement GetPortalContextElement(string portalName = null, bool allowDefaultFallback = false)
		{
			var section = GetPortalCrmSection();
			var portalContextElement = section.Portals.GetElementOrDefault(portalName, allowDefaultFallback);
			return portalContextElement;
		}

		/// <summary>
		/// Retrieves the configured <see cref="ICrmEntitySecurityProvider"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public virtual ICrmEntitySecurityProvider CreateCrmEntitySecurityProvider(string portalName = null)
		{
			var element = GetPortalContextElement(portalName);
			return element.CrmEntitySecurityProvider.CreateCrmEntitySecurityProvider(portalName);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IDependencyProvider"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public virtual IDependencyProvider CreateDependencyProvider(string portalName = null)
		{
			var element = GetPortalContextElement(portalName);
			return element.DependencyProvider.GetDependencyProvider(portalName);
		}

		/// <summary>
		/// Retrieves the configured <see cref="IOrganizationService"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IOrganizationService CreateOrganizationService(string portalName = null, bool allowDefaultFallback = false)
		{
			var portalContextElement = GetPortalContextElement(portalName, allowDefaultFallback);

			var contextName = !string.IsNullOrWhiteSpace(portalContextElement.ContextName)
				? portalContextElement.ContextName
				: portalContextElement.Name;

			var service = CrmConfigurationManager.CreateService(contextName, true);
			return service;
		}

		/// <summary>
		/// Retrieves the configured <see cref="IPortalContext"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="request"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual IPortalContext CreatePortalContext(string portalName = null, RequestContext request = null, bool allowDefaultFallback = false)
		{
			var portalContextElement = GetPortalContextElement(portalName, allowDefaultFallback);
			var mode = portalContextElement.InstanceMode;

			if (mode == PortalContextInstanceMode.PerRequest && HttpSingleton<IPortalContext>.Enabled)
			{
				return HttpSingleton<IPortalContext>.GetInstance(portalName, () => portalContextElement.CreatePortalContext(request));
			}

			// PortalContextInstanceMode.PerInstance

			var portal = portalContextElement.CreatePortalContext(request);
			return portal;
		}

		/// <summary>
		/// Retrieves the configured <see cref="OrganizationServiceContext"/>.
		/// </summary>
		/// <param name="portalName"></param>
		/// <param name="allowDefaultFallback"></param>
		/// <returns></returns>
		public virtual OrganizationServiceContext CreateServiceContext(string portalName = null, bool allowDefaultFallback = false)
		{
			var portalContextElement = GetPortalContextElement(portalName, allowDefaultFallback);

			var contextName = !string.IsNullOrWhiteSpace(portalContextElement.ContextName)
				? portalContextElement.ContextName
				: portalContextElement.Name;

			var context = CrmConfigurationManager.CreateContext(contextName, true);
			return context;
		}

		internal string GetCmsServiceBaseUri(string portalName = null)
		{
			var element = GetPortalContextElement(portalName);
			return element.CmsServiceBaseUri;
		}

		internal OrganizationServiceContext GetServiceContext(string portalName = null, RequestContext request = null)
		{
			var portal = CreatePortalContext(portalName, request);
			return portal.ServiceContext;
		}
	}
}
