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

namespace Microsoft.Xrm.Portal.Web.Handlers
{
	/// <summary>
	/// Used to invalidate cache items based on the type of message (publish, create, update, or delete) and the entity type and id.
	/// </summary>
	/// <remarks>
	/// Only the default or configured <see cref="OrganizationServiceCache"/>s will be invalidated.
	/// </remarks>
	public class CacheInvalidationHandler : IHttpHandler, IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new CacheInvalidationHandler();
		}

		public bool IsReusable
		{
			get { return true; }
		}

		public virtual void ProcessRequest(HttpContext context)
		{
			try
			{
				if (context.Request.ContentType == "application/json")
				{
					ProcessJsonRequest(context);

					return;
				}

				var message = GetMessage(context);

				if (string.Equals("Publish", message, StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals("PublishAll", message, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (var serviceCache in GetServiceCaches())
					{
						serviceCache.Remove(GetPluginMessage(CacheItemCategory.Metadata));
					}

					// get the default object cache

					foreach (var cache in GetObjectCaches())
					{
						cache.Remove("xrm:dependency:metadata:*");
					}

					return;
				}

				if (string.Equals("InvalidateAll", message, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (var serviceCache in GetServiceCaches())
					{
						serviceCache.Remove(GetPluginMessage(CacheItemCategory.All));
					}

					// get the default object cache

					foreach (var cache in GetObjectCaches())
					{
						cache.RemoveAll();
					}

					return;
				}

				var entity = GetEntityReference(context);

				// get the default service cache

				if (string.Equals("Create", message, StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals("Update", message, StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals("Delete", message, StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (var serviceCache in GetServiceCaches())
					{
						serviceCache.Remove(entity);
					}

					return;
				}
			}
			catch (Exception e)
			{
				Tracing.FrameworkError(GetType().Name, "ProcessRequest", "Cache invalidation failed. {0}", e.Message);
			}
		}

		protected virtual void ProcessJsonRequest(HttpContext context)
		{
			// {
			//  "MessageName":"Update",
			//  "Target":{"LogicalName":"adx_webpageaccesscontrolrule","Id":"40b062c7-1aea-e011-b5b7-001d60c95b1e"}
			// }

			// {
			//  "MessageName":"Associate",
			//  "Target":{"LogicalName":"adx_webpageaccesscontrolrule","Id":"40b062c7-1aea-e011-b5b7-001d60c95b1e"},
			//  "Relationship":{"SchemaName":"adx_webpageaccesscontrolrule_webrole","PrimaryEntityRole":"0"},
			//  "RelatedEntities":[{"LogicalName":"adx_webrole","Id":"bf5420f9-de03-e111-a1a1-00155d03a708"}]
			// }

			var body = GetRequestBody(context);

			if (!string.IsNullOrWhiteSpace(body))
			{
				var message = body.DeserializeByJson(typeof(OrganizationServiceCachePluginMessage), null) as OrganizationServiceCachePluginMessage;

				ThrowOnNull(message, "The plug-in message is unspecified.");

				Tracing.FrameworkInformation(GetType().Name, "ProcessRequest", body);

				foreach (var serviceCache in GetServiceCaches())
				{
					serviceCache.Remove(message);
				}
			}
		}

		protected virtual IEnumerable<ObjectCache> GetObjectCaches()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			var elements = section.ObjectCache.Cast<ObjectCacheElement>().ToList();

			if (!elements.Any())
			{
				yield return CrmConfigurationManager.CreateObjectCache();
			}
			else
			{
				// ignore service cache objects that are nested in a composite service cache

				var ignored = (
					from element in elements
					let inner = element.Parameters["innerObjectCacheName"]
					where !string.IsNullOrWhiteSpace(inner)
					select inner).ToList();

				foreach (var element in elements.Where(e => !ignored.Contains(e.Name)))
				{
					yield return CrmConfigurationManager.CreateObjectCache(element.Name);
				}
			}
		}

		protected virtual IEnumerable<IOrganizationServiceCache> GetServiceCaches()
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

		protected static EntityReference GetEntityReference(HttpContext context)
		{
			var entityName = context.Request["EntityName"];
			var id = context.Request["ID"];

			ThrowOnNullOrWhiteSpace(entityName, "'EntityName' must be defined as a query string parameter.");
			ThrowOnNullOrWhiteSpace(id, "'ID' must be defined as a query string parameter.");

			return new EntityReference(entityName, new Guid(id));
		}

		protected static string GetMessage(HttpContext context)
		{
			var message = context.Request["Message"];

			ThrowOnNullOrWhiteSpace(message, "'Message' must be defined as a query string parameter.");

			return message;
		}

		private static void ThrowOnNull(object obj, string message)
		{
			if (obj == null) throw new ArgumentException(message);
		}

		private static void ThrowOnNullOrWhiteSpace(string text, string message)
		{
			if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException(message);
		}

		private static string GetRequestBody(HttpContext context)
		{
			using (var reader = new StreamReader(context.Request.InputStream))
			{
				return reader.ReadToEnd();
			}
		}

		private static OrganizationServiceCachePluginMessage GetPluginMessage(CacheItemCategory category)
		{
			return new OrganizationServiceCachePluginMessage { Category = category };
		}
	}
}
