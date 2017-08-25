/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	class InvalidationHandler
	{
		public void InvalidateCache(OrganizationServiceCachePluginMessage message)
		{
			foreach (var serviceCache in GetServiceCaches())
			{
				serviceCache.Remove(message);

				//logging
				if (message != null)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Invalidate message {0} from cache ", message.ToString()));
				}
			}
		}

		protected  IEnumerable<IOrganizationServiceCache> GetServiceCaches()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			var elements = section.ServiceCache.Cast<OrganizationServiceCacheElement>().ToList();

			if (!elements.Any())
			{
				yield return CrmConfigurationManager.CreateServiceCache(null, (string)null, true);
			}
			else
			{
				// ignore service cache objects that are nested in a composite service cache

				var ignored = (
					from element in elements
					let inner = element.Parameters["innerServiceCacheName"]
					where !string.IsNullOrWhiteSpace(inner)
					select inner).ToList();

				foreach (var element in elements.Where(e => !ignored.Contains(e.Name)))
				{
					var connectionId = GetConnectionId(element.Parameters);

					yield return CrmConfigurationManager.CreateServiceCache(element.Name, connectionId, true);
				}
			}
		}

		private static string GetConnectionId(NameValueCollection config)
		{
			var contextName = GetContextName(config);
			var connectionStringName = CrmConfigurationManager.GetConnectionStringNameFromContext(contextName, true);
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
