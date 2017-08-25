/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Routing
{
	public class OrganizationServiceCachePluginMessageHandler : IHttpHandler
	{
		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public OrganizationServiceCachePluginMessage Message { get; private set; }

		public OrganizationServiceCachePluginMessageHandler(string portalName, OrganizationServiceCachePluginMessage message)
		{
			PortalName = portalName;
			Message = message;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.ThrowOnNull("context");

			context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			var cache = GetOrganizationServiceCache();

			cache.ExtendedRemoveLocal(Message);
		}

		internal static IOrganizationServiceCache GetOrganizationServiceCache()
		{
			string connectionId;
			var serviceCacheName = GetServiceCacheName(out connectionId);
			var cache = CrmConfigurationManager.CreateServiceCache(serviceCacheName, connectionId);
			return cache;
		}

		private static string GetServiceCacheName(out string connectionId)
		{
			// try the default provider

			var section = CrmConfigurationManager.GetCrmSection();
			var defaultElement = section.ServiceCache.GetElementOrDefault(null);

			if (IsExtendedOrganizationServiceCache(defaultElement.DependencyType))
			{
				connectionId = GetConnectionId(defaultElement.Parameters);

				return defaultElement.Name;
			}

			// return the first element that has a ServiceBusObjectCache dependency

			var element = section.ServiceCache.Cast<OrganizationServiceCacheElement>().FirstOrDefault(cache => IsExtendedOrganizationServiceCache(cache.DependencyType));

			if (element != null)
			{
				connectionId = GetConnectionId(element.Parameters);
				return element.Name;
			}

			connectionId = null;
			return null;
		}

		private static bool IsExtendedOrganizationServiceCache(Type type)
		{
			return typeof(IExtendedOrganizationServiceCache).IsAssignableFrom(type);
		}

		private static string GetConnectionId(NameValueCollection config)
		{
			var contextName = GetContextName(config);
			var connectionStringName = CrmConfigurationManager.GetConnectionStringNameFromContext(contextName);
			var connection = new CrmConnection(connectionStringName);

			return connection.GetConnectionId();
		}

		private static string GetContextName(NameValueCollection config)
		{
			var contextName = config["contextName"];

			if (!string.IsNullOrWhiteSpace(contextName)) return contextName;

			var portalName = config["portalName"];

			var portalContextElement = PortalCrmConfigurationManager.GetPortalContextElement(portalName, true);

			var configName = !string.IsNullOrWhiteSpace(portalContextElement.ContextName)
				? portalContextElement.ContextName
				: portalContextElement.Name;

			return configName;
		}
	}
}
