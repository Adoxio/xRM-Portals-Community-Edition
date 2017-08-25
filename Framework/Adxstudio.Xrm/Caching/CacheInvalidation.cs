/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Caching
{
	internal static class CacheInvalidation
	{
		/// <summary>
		/// Process the message received from a CRM Plugin that sends HTTP POST request to invalidate the cache.
		/// </summary>
		/// <param name="message"></param>
		public static void ProcessMessage(OrganizationServiceCachePluginMessage message)
		{
			if (message == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Cache invalidation failure. Plugin Message is null.");

				return;
			}

			if (message.Target != null && message.Target.LogicalName == "adx_website")
			{
				InvalidateAllCache();
			}
			else
			{
				InvalidateCache(message);
			}
		}

		private static void InvalidateCache(OrganizationServiceCachePluginMessage message)
		{
			var serviceCaches = GetServiceCaches();

			foreach (var organizationServiceCache in serviceCaches)
				organizationServiceCache.Remove(message);
		}

		/// <summary>
		/// Invalidates the entire cache
		/// </summary>
		public static void InvalidateAllCache()
		{
			var serviceCaches = GetServiceCaches();

			foreach (var organizationServiceCache in serviceCaches)
				organizationServiceCache.Remove(new OrganizationServiceCachePluginMessage { Category = CacheItemCategory.All });

			var objectCaches = GetObjectCaches();

			foreach (var cache in objectCaches)
				ObjectCacheManager.RemoveAll(cache);
		}

		private static IEnumerable<IOrganizationServiceCache> GetServiceCaches()
		{
			var section = CrmConfigurationManager.GetCrmSection();
			var elements = section.ServiceCache.Cast<OrganizationServiceCacheElement>().ToList();

			if (!elements.Any())
			{
				yield return CrmConfigurationManager.CreateServiceCache(null, (string)null, true);
			}
			else
			{
				var ignored =
					elements.Select(element => new { element, inner = element.Parameters["innerServiceCacheName"] })
						.Where(param0 => !string.IsNullOrWhiteSpace(param0.inner))
						.Select(param0 => param0.inner)
						.ToList();

				foreach (var serviceCacheElement in elements.Where(e => !ignored.Contains(e.Name)))
				{
					var connectionId = GetConnectionId(serviceCacheElement.Parameters);
					yield return CrmConfigurationManager.CreateServiceCache(serviceCacheElement.Name, connectionId, true);
				}
			}
		}

		private static string GetConnectionId(NameValueCollection config)
		{
			return new CrmConnection(CrmConfigurationManager.GetConnectionStringNameFromContext(GetContextName(config), true)).GetConnectionId();
		}

		private static string GetContextName(NameValueCollection config)
		{
			var str = config["contextName"];

			if (!string.IsNullOrWhiteSpace(str)) return str;

			var portalContextElement = PortalCrmConfigurationManager.GetPortalContextElement(config["portalName"], true);

			return !string.IsNullOrWhiteSpace(portalContextElement.ContextName) ? portalContextElement.ContextName : portalContextElement.Name;
		}

		private static IEnumerable<ObjectCache> GetObjectCaches()
		{
			var section = CrmConfigurationManager.GetCrmSection();
			var elements = section.ObjectCache.Cast<ObjectCacheElement>().ToList();

			if (!elements.Any())
			{
				yield return CrmConfigurationManager.CreateObjectCache();
			}
			else
			{
				var ignored =
					elements.Select(element => new { element, inner = element.Parameters["innerObjectCacheName"] })
						.Where(param0 => !string.IsNullOrWhiteSpace(param0.inner))
						.Select(param0 => param0.inner)
						.ToList();

				foreach (var objectCacheElement in elements.Where(e => !ignored.Contains(e.Name)))
				{
					yield return CrmConfigurationManager.CreateObjectCache(objectCacheElement.Name);
				}
			}
		}
	}
}
