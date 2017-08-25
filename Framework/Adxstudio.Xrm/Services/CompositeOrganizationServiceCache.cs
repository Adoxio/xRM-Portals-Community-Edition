/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Services
{
	/// <summary>
	/// An <see cref="IOrganizationService"/> wrapper class.
	/// </summary>
	public class CompositeOrganizationServiceCache : IExtendedOrganizationServiceCache, IInitializable
	{
		public IOrganizationServiceCache Inner { get; private set; }

		public CompositeOrganizationServiceCache()
		{
		}

		public CompositeOrganizationServiceCache(ObjectCache cache)
			: this(new OrganizationServiceCache(cache))
		{
		}

		public CompositeOrganizationServiceCache(ObjectCache cache, CrmConnection connection)
			: this(new OrganizationServiceCache(cache, connection))
		{
		}

		public CompositeOrganizationServiceCache(ObjectCache cache, string connectionId)
			: this(new OrganizationServiceCache(cache, connectionId))
		{
		}

		public CompositeOrganizationServiceCache(ObjectCache cache, OrganizationServiceCacheSettings settings)
			: this(new OrganizationServiceCache(cache, settings))
		{
		}

		public CompositeOrganizationServiceCache(IOrganizationServiceCache inner)
		{
			Inner = inner;
		}

		public virtual void Initialize(string name, NameValueCollection config)
		{
			if (Inner == null)
			{
				if (config != null)
				{
					var connectionId = GetConnectionId(name, config);
					var innerServiceCacheName = config["innerServiceCacheName"];

					if (!string.IsNullOrWhiteSpace(innerServiceCacheName))
					{
						// instantiate by config

						Inner = CrmConfigurationManager.CreateServiceCache(innerServiceCacheName, connectionId, true);
					}
					else
					{
						Inner = CreateDefaultServiceCache(name, connectionId, true);
					}
				}
			}
		}

		public virtual T Execute<T>(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> execute, Func<OrganizationResponse, T> selector, string selectorCacheKey)
		{
			return Inner.Execute(request, execute, selector, selectorCacheKey);
		}

		public virtual OrganizationServiceCacheMode Mode
		{
			get { return Inner.Mode; }
			set { Inner.Mode = value; }
		}

		public virtual void Insert(string key, object query, object result)
		{
			Inner.Insert(key, query, result);
		}

		public virtual void Remove(string cacheKey)
		{
			Inner.Remove(cacheKey);
		}

		public virtual void Remove(OrganizationRequest request)
		{
			Inner.Remove(request);
		}

		public virtual void Remove(string entityLogicalName, Guid? id)
		{
			Inner.Remove(entityLogicalName, id);
		}

		public virtual void Remove(EntityReference entity)
		{
			Inner.Remove(entity);
		}

		public virtual void Remove(Entity entity)
		{
			Inner.Remove(entity);
		}

		public virtual void Remove(OrganizationServiceCachePluginMessage message)
		{
			Inner.Remove(message);
		}

		public virtual void RemoveLocal(OrganizationServiceCachePluginMessage message)
		{
			Remove(message);
		}

		public virtual OrganizationServiceCacheReturnMode ReturnMode
		{
			get { return Inner.ReturnMode; }
			set { Inner.ReturnMode = value; }
		}

		protected virtual string GetConnectionStringName(string name, NameValueCollection config)
		{
			var contextName = GetContextName(name, config);
			var connectionStringName = CrmConfigurationManager.GetConnectionStringNameFromContext(contextName);

			return connectionStringName;
		}

		protected virtual string GetConnectionId(string name, NameValueCollection config)
		{
			var connectionStringName = GetConnectionStringName(name, config);
			var connection = new CrmConnection(connectionStringName);

			return connection.GetConnectionId();
		}

		protected virtual IOrganizationServiceCache CreateDefaultServiceCache(ObjectCache objectCache, OrganizationServiceCacheSettings settings)
		{
			return new OrganizationServiceCache(objectCache, settings);
		}

		private static string GetContextName(string name, NameValueCollection config)
		{
			var contextName = config["contextName"];

			if (!string.IsNullOrWhiteSpace(contextName)) return contextName;

			var portalName = config["portalName"];

			var portalContextElement = PortalCrmConfigurationManager.GetPortalContextElement(portalName, true);

			var configName = !string.IsNullOrWhiteSpace(portalContextElement.ContextName)
				? portalContextElement.ContextName
				: portalContextElement.Name;

			return configName ?? name;
		}

		private IOrganizationServiceCache CreateDefaultServiceCache(string serviceCacheName = null, string connectionId = null, bool allowDefaultFallback = false)
		{
			var section = CrmConfigurationManager.GetCrmSection();

			var serviceCacheElement = section.ServiceCache.GetElementOrDefault(serviceCacheName, allowDefaultFallback);

			var objectCacheName = !string.IsNullOrWhiteSpace(serviceCacheElement.ObjectCacheName)
				? serviceCacheElement.ObjectCacheName
				: serviceCacheElement.Name;
			var objectCacheElement = section.ObjectCache.GetElementOrDefault(objectCacheName, allowDefaultFallback);

			var settings = new OrganizationServiceCacheSettings(connectionId)
			{
				ConnectionId = connectionId,
				CacheRegionName = serviceCacheElement.CacheRegionName,
				QueryHashingEnabled = serviceCacheElement.QueryHashingEnabled,
				PolicyFactory = objectCacheElement,
			};

			var objectCache = CrmConfigurationManager.CreateObjectCache(objectCacheName, true);
			var serviceCache = CreateDefaultServiceCache(objectCache, settings);

			return serviceCache;
		}

		internal static OrganizationServiceCachePluginMessage GetMessage(OrganizationRequest request, OrganizationResponse response)
		{
			var reference = GetEntityReference(request, response);

			if (reference != null)
			{
				return new OrganizationServiceCachePluginMessage { MessageName = request.RequestName, Target = reference.ToPluginMessageEntityReference() };
			}

			if (request.RequestName == "Associate" || request.RequestName == "Disassociate")
			{
				var target = request["Target"] as EntityReference;
				var relationship = request["Relationship"] as Relationship;
				var relatedEntities = request["RelatedEntities"] as EntityReferenceCollection;

				return new OrganizationServiceCachePluginMessage
				{
					MessageName = request.RequestName,
					Target = target.ToPluginMessageEntityReference(),
					Relationship = relationship.ToPluginMessageRelationship(),
					RelatedEntities = relatedEntities.ToPluginMessageEntityReferenceCollection(),
				};
			}

			return null;
		}

		private static EntityReference GetEntityReference(OrganizationRequest request, OrganizationResponse response)
		{
			switch (request.RequestName)
			{
				case "Create":
					return new EntityReference((request["Target"] as Entity).LogicalName, (Guid)response["id"]);
				case "Update":
					return (request["Target"] as Entity).ToEntityReference();
				case "Delete":
					return request["Target"] as EntityReference;
				case "SetState":
					return request["EntityMoniker"] as EntityReference;
			}

			return null;
		}
	}
}
