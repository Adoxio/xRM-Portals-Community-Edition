/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Runtime.Serialization;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Client.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// Provides caching services for an <see cref="IOrganizationService"/> through an underlying <see cref="ObjectCache"/>.
	/// </summary>
	/// <remarks>
	/// When the underlying service is executed, the <see cref="OrganizationRequest"/> along with the ConnectionId is converted to a cache key. The resulting
	/// <see cref="OrganizationResponse"/> is inserted into the cache using the cache key for subsequent lookups.
	/// 
	/// Cache item dependencies in the form of <see cref="CacheEntryChangeMonitor"/> objects are generated from the <see cref="OrganizationResponse"/> and
	/// assigned to the cache items during insertion. When an <see cref="Entity"/> is removed from the cache, the dependencies ensure that the associated cache
	/// items are removed.
	/// </remarks>
	public class OrganizationServiceCache : IOrganizationServiceCache, IInitializable
	{
		private const string _dependencyEntityObjectFormat = "{0}:entity:{1}:id={2}";
		private const string _dependencyEntityClassFormat = "{0}:entity:{1}";
		private const string _dependencyMetadataFormat = "{0}:metadata:*";
		private const string _dependencyContentFormat = "{0}:content:*";

		private readonly ICacheItemPolicyFactory _cacheItemPolicyFactory;

		/// <summary>
		/// The cache region used when interacting with the <see cref="ObjectCache"/>.
		/// </summary>
		public string CacheRegionName { get; private set; }

		/// <summary>
		/// The caching behavior mode.
		/// </summary>
		public OrganizationServiceCacheMode Mode { get; set; }

		/// <summary>
		/// The cache retrieval mode.
		/// </summary>
		public OrganizationServiceCacheReturnMode ReturnMode { get; set; }

		/// <summary>
		/// The underlying cache.
		/// </summary>
		public virtual ObjectCache Cache { get; private set; }

		/// <summary>
		/// The prefix string used for constructing the <see cref="CacheEntryChangeMonitor"/> objects assigned to the cache items.
		/// </summary>
		public virtual string CacheEntryChangeMonitorPrefix { get; private set; }

		/// <summary>
		/// A key value for uniquely distinguishing the connection.
		/// </summary>
		public string ConnectionId { get; private set; }

		/// <summary>
		/// Gets or sets the flag determining whether or not to hash the serialized query.
		/// </summary>
		public bool QueryHashingEnabled { get; private set; }

		static OrganizationServiceCache()
		{
			Initialize();
		}

		public OrganizationServiceCache()
			: this(null)
		{
		}

		public OrganizationServiceCache(ObjectCache cache)
			: this(cache, (OrganizationServiceCacheSettings)null)
		{
		}

		public OrganizationServiceCache(ObjectCache cache, CrmConnection connection)
			: this(cache, connection.GetConnectionId())
		{
		}

		public OrganizationServiceCache(ObjectCache cache, string connectionId)
			: this(cache, new OrganizationServiceCacheSettings(connectionId))
		{
		}

		public OrganizationServiceCache(ObjectCache cache, OrganizationServiceCacheSettings settings)
		{
			var cacheSettings = settings ?? new OrganizationServiceCacheSettings();

			Cache = cache ?? MemoryCache.Default;
			Mode = OrganizationServiceCacheMode.LookupAndInsert;
			ReturnMode = OrganizationServiceCacheReturnMode.Cloned;

			ConnectionId = cacheSettings.ConnectionId;
			CacheRegionName = cacheSettings.CacheRegionName;
			QueryHashingEnabled = cacheSettings.QueryHashingEnabled;
			CacheEntryChangeMonitorPrefix = cacheSettings.CacheEntryChangeMonitorPrefix;
			_cacheItemPolicyFactory = cacheSettings.PolicyFactory;
		}

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
		}

		/// <summary>
		/// Executes a request against the <see cref="IOrganizationService"/> or retrieves the response from the cache if found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <param name="execute"></param>
		/// <param name="selector"></param>
		/// <param name="selectorCacheKey"></param>
		/// <returns></returns>
		public T Execute<T>(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> execute, Func<OrganizationResponse, T> selector, string selectorCacheKey)
		{
			return InnerExecute(request, execute, selector, selectorCacheKey);
		}

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entity"></param>
		public void Remove(Entity entity)
		{
			InvalidateCacheDependencies(GetDependencies(entity));
		}

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entity"></param>
		public void Remove(EntityReference entity)
		{
			InvalidateCacheDependencies(GetDependencies(entity));
		}

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entityLogicalName"></param>
		/// <param name="id"></param>
		public void Remove(string entityLogicalName, Guid? id)
		{
			InvalidateCacheDependency(GetDependency(entityLogicalName));

			if (id != null && id.Value != Guid.Empty)
			{
				InvalidateCacheDependency(GetDependency(entityLogicalName, id.Value));
			}
		}

		/// <summary>
		/// Removes a request from the cache.
		/// </summary>
		/// <param name="request"></param>
		public void Remove(OrganizationRequest request)
		{
			var dependencies = GetDependenciesForObject(request).ToList();

			InvalidateCacheDependencies(dependencies);
		}

		/// <summary>
		/// Removes a specific cache item.
		/// </summary>
		/// <param name="cacheKey"></param>
		public void Remove(string cacheKey)
		{
			Cache.Remove(cacheKey, CacheRegionName);
		}

		public void Remove(OrganizationServiceCachePluginMessage message)
		{
			if (message.Category != null)
			{
				Tracing.FrameworkInformation("OrganizationServiceCache", "Remove", "Category={0}", message.Category.Value);

				Remove(message.Category.Value);
			}

			if (message.Target != null)
			{
				var entity = message.Target.ToEntityReference();

				Tracing.FrameworkInformation("OrganizationServiceCache", "Remove", "LogicalName={0}, Id={1}, Name={2}", entity.LogicalName, entity.Id, entity.Name);

				Remove(entity);
			}

			if (message.RelatedEntities != null)
			{
				var relatedEntities = message.RelatedEntities.ToEntityReferenceCollection();

				foreach (var entity in relatedEntities)
				{
					Tracing.FrameworkInformation("OrganizationServiceCache", "Remove", "LogicalName={0}, Id={1}, Name={2}", entity.LogicalName, entity.Id, entity.Name);

					Remove(entity);
				}
			}
		}

		private void Remove(CacheItemCategory category)
		{
			if (category == CacheItemCategory.All)
			{
				Cache.RemoveAll();
				return;
			}

			if (category.HasFlag(CacheItemCategory.Metadata))
			{
				Remove(_dependencyMetadataFormat.FormatWith(CacheEntryChangeMonitorPrefix));
			}

			if (category.HasFlag(CacheItemCategory.Content))
			{
				Remove(_dependencyContentFormat.FormatWith(CacheEntryChangeMonitorPrefix));
			}
		}

		private TResult InnerExecute<TRequest, TResponse, TResult>(TRequest request, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector, string selectorCacheKey)
		{
			// perform a cached execute or fallback to a regular execute

			var isCachedRequest = IsCachedRequest(request as OrganizationRequest);

			var response = isCachedRequest ? Get(request, execute, selector, selectorCacheKey) : InnerExecute(request, execute, selector);

			if (!isCachedRequest)
			{
				var dependencies = GetDependenciesForObject(request).ToList();

				InvalidateCacheDependencies(dependencies);
			}

			return response;
		}

		private TResult Get<TRequest, TResponse, TResult>(TRequest query, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector, string selectorCacheKey)
		{
			if (Mode == OrganizationServiceCacheMode.LookupAndInsert) return LookupAndInsert(query, execute, selector, selectorCacheKey);
			if (Mode == OrganizationServiceCacheMode.InsertOnly) return InsertOnly(query, execute, selector, selectorCacheKey);
			if (Mode == OrganizationServiceCacheMode.Disabled) return Disabled(query, execute, selector, selectorCacheKey);

			return InnerExecute(query, execute, selector);
		}

		private TResult LookupAndInsert<TRequest, TResponse, TResult>(TRequest query, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector, string selectorCacheKey)
		{
			var cacheKey = GetCacheKey(query, selectorCacheKey);

			var response = Cache.Get(cacheKey,
				cache =>
				{
					Tracing.FrameworkInformation("OrganizationServiceCache", OrganizationServiceCacheMode.LookupAndInsert.ToString(), cacheKey);
					return InnerExecute(query, execute, selector);
				},
				(cache, result) => Insert(cacheKey, query, result),
				CacheRegionName);

			return ReturnMode == OrganizationServiceCacheReturnMode.Cloned ? CloneResponse(response) : response;
		}

		private TResult InsertOnly<TRequest, TResponse, TResult>(TRequest query, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector, string selectorCacheKey)
		{
			var cacheKey = GetCacheKey(query, selectorCacheKey);
			var result = default(TResult);

			LockManager.Lock(
				cacheKey,
				() =>
				{
					Tracing.FrameworkInformation("OrganizationServiceCache", OrganizationServiceCacheMode.InsertOnly.ToString(), cacheKey);

					result = InnerExecute(query, execute, selector);

					Insert(cacheKey, query, result);
				});

			return result;
		}

		private TResult Disabled<TRequest, TResponse, TResult>(TRequest query, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector, string selectorCacheKey)
		{
			var cacheKey = GetCacheKey(query, selectorCacheKey);

			Tracing.FrameworkInformation("OrganizationServiceCache", OrganizationServiceCacheMode.Disabled.ToString(), cacheKey);

			return InnerExecute(query, execute, selector);
		}

		private static TResult InnerExecute<TRequest, TResponse, TResult>(TRequest query, Func<TRequest, TResponse> execute, Func<TResponse, TResult> selector)
		{
			return selector(execute(query));
		}

		public void Insert(string key, object query, object result)
		{
			var cachePolicy = InternalGetCachePolicy(query, result);
			Insert(key, result, cachePolicy);
		}

		private void Insert(string key, object result, CacheItemPolicy cachePolicy)
		{
			// select the cache entry monitors and add their keys to the cache

			Cache.Insert(key, result, cachePolicy, CacheRegionName);
		}

		/// <summary>
		/// An extensiblity method for retrieving a custom cache policy.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="result"></param>
		/// <param name="cacheItemPolicy"></param>
		/// <returns></returns>
		protected virtual bool TryGetCachePolicy(object query, object result, out CacheItemPolicy cacheItemPolicy)
		{
			cacheItemPolicy = null;
			return false;
		}

		private CacheItemPolicy InternalGetCachePolicy(object query, object result)
		{
			// extensibility point

			CacheItemPolicy cacheItemPolicy;

			return TryGetCachePolicy(query, result, out cacheItemPolicy)
				? cacheItemPolicy
				: GetCachePolicy(query, result);
		}

		protected CacheItemPolicy GetCachePolicy(object query, object result)
		{
			var cachePolicy = GetBaseCachePolicy();
			var dependencies = GetDependenciesForObject(query).Concat(GetDependenciesForObject(result)).Distinct().ToList();
			var monitor = Cache.GetChangeMonitor(dependencies, CacheRegionName);

			if (monitor != null)
			{
				cachePolicy.ChangeMonitors.Add(monitor);
			}

			return cachePolicy;
		}

		protected CacheItemPolicy GetBaseCachePolicy()
		{
			return _cacheItemPolicyFactory != null ? _cacheItemPolicyFactory.Create() : new CacheItemPolicy();
		}

		/// <summary>
		/// An extensiblity method for retrieving dependencies.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="dependencies"></param>
		/// <returns></returns>
		protected virtual bool TryGetDependencies(object query, out IEnumerable<string> dependencies)
		{
			dependencies = null;
			return false;
		}

		private IEnumerable<string> GetDependenciesForObject(object query, IEnumerable<object> path = null)
		{
			IEnumerable<string> dependencies;

			if (TryGetDependencies(query, out dependencies))
			{
				return dependencies;
			}

			if (query is KeyedRequest) return GetDependencies(query as KeyedRequest, path ?? new List<object> { query });
			if (query is OrganizationRequest) return GetDependencies(query as OrganizationRequest, path ?? new List<object> { query });
			if (query is OrganizationResponse) return GetDependencies(query as OrganizationResponse, path ?? new List<object> { query });

			if (query is QueryBase) return GetDependencies(query as QueryBase);
			if (query is IEnumerable<Entity>) return GetDependencies(query as IEnumerable<Entity>);
			if (query is IEnumerable<EntityReference>) return GetDependencies(query as IEnumerable<EntityReference>);
			if (query is EntityCollection) return GetDependencies(query as EntityCollection);
			if (query is Entity) return GetDependencies(query as Entity);
			if (query is EntityReference) return GetDependencies(query as EntityReference);
			if (query is RelationshipQueryCollection) return GetDependencies(query as RelationshipQueryCollection);

			return GetDependenciesEmpty();
		}

		private static IEnumerable<string> GetDependenciesEmpty()
		{
			yield break;
		}

		private IEnumerable<string> GetDependencies(KeyedRequest request, IEnumerable<object> path)
		{
			return GetDependenciesForObject(request.Request, path);
		}

		private IEnumerable<string> GetDependencies(OrganizationRequest request, IEnumerable<object> path)
		{
			if (IsContentRequest(request))
			{
				yield return _dependencyContentFormat.FormatWith(CacheEntryChangeMonitorPrefix);
			}
			else if (IsMetadataRequest(request))
			{
				yield return _dependencyMetadataFormat.FormatWith(CacheEntryChangeMonitorPrefix);
			}

			foreach (var parameter in request.Parameters)
			{
				var value = parameter.Value;

				if (value != null && !path.Contains(value))
				{
					foreach (var child in GetDependenciesForObject(value, path.Concat(new[] { value })))
					{
						yield return child;
					}
				}
			}
		}

		private IEnumerable<string> GetDependencies(OrganizationResponse response, IEnumerable<object> path)
		{
			foreach (var parameter in response.Results)
			{
				var value = parameter.Value;

				if (value != null && !path.Contains(value))
				{
					foreach (var child in GetDependenciesForObject(value, path.Concat(new[] { value })))
					{
						yield return child;
					}
				}
			}
		}

		private IEnumerable<string> GetDependencies(RelationshipQueryCollection collection)
		{
			foreach (var relatedEntitiesQuery in collection)
			{
				foreach (var dependency in GetDependencies(relatedEntitiesQuery.Value))
				{
					yield return dependency;
				}
			}
		}

		private IEnumerable<string> GetDependencies(QueryBase query)
		{
			if (query is QueryExpression)
			{
				yield return GetDependency((query as QueryExpression).EntityName);

				foreach (var linkEntity in GetLinkEntities(query as QueryExpression))
				{
					yield return GetDependency(linkEntity.LinkToEntityName);
					yield return GetDependency(linkEntity.LinkFromEntityName);
				}
			}
			else if (query is QueryByAttribute)
			{
				yield return GetDependency((query as QueryByAttribute).EntityName);
			}
		}

		private IEnumerable<string> GetDependencies(EntityCollection entities)
		{
			yield return GetDependency(entities.EntityName);
			
			foreach (var dependency in GetDependencies(entities.Entities))
			{
				yield return dependency;
			}
		}

		private IEnumerable<string> GetDependencies(IEnumerable<Entity> entities)
		{
			return entities.SelectMany(GetDependencies);
		}

		private IEnumerable<string> GetDependencies(IEnumerable<EntityReference> entities)
		{
			return entities.SelectMany(GetDependencies);
		}

		private IEnumerable<string> GetDependencies(RelatedEntityCollection relationships)
		{
			return relationships.SelectMany(r => GetDependencies(r.Value));
		}

		private IEnumerable<string> GetDependencies(Entity entity)
		{
			yield return GetDependency(entity.LogicalName);
			yield return GetDependency(entity);

			// walk the related entities

			foreach (var related in GetDependencies(entity.RelatedEntities))
			{
				yield return related;
			}
		}

		private IEnumerable<string> GetDependencies(EntityReference entity)
		{
			yield return GetDependency(entity.LogicalName);
			yield return GetDependency(entity);
		}

		private static IEnumerable<LinkEntity> GetLinkEntities(QueryExpression query)
		{
			return GetLinkEntities(query.LinkEntities);
		}

		private string GetDependency(Entity entity)
		{
			return GetDependency(entity.LogicalName, entity.Id);
		}

		private string GetDependency(EntityReference entity)
		{
			return GetDependency(entity.LogicalName, entity.Id);
		}

		private string GetDependency(string entityName)
		{
			return _dependencyEntityClassFormat.FormatWith(CacheEntryChangeMonitorPrefix, entityName);
		}

		private string GetDependency(string entityName, Guid? id)
		{
			return _dependencyEntityObjectFormat.FormatWith(CacheEntryChangeMonitorPrefix, entityName, id);
		}

		private static IEnumerable<LinkEntity> GetLinkEntities(IEnumerable<LinkEntity> linkEntities)
		{
			foreach (var linkEntity in linkEntities)
			{
				if (linkEntity != null)
				{
					yield return linkEntity;

					foreach (var child in GetLinkEntities(linkEntity.LinkEntities))
					{
						yield return child;
					}
				}
			}
		}

		/// <summary>
		/// An extensiblity method for retrieving a custom cache key.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cacheKey"></param>
		/// <returns></returns>
		protected virtual bool TryGetCacheKey(object request, out string cacheKey)
		{
			cacheKey = null;
			return false;
		}

		private string InternalGetCacheKey(object request)
		{
			// extensiblity point

			string cacheKey;

			return TryGetCacheKey(request, out cacheKey)
				? cacheKey
				: GetCacheKey(request);
		}

		private string GetCacheKey(object query, string selectorCacheKey)
		{
			var text = InternalGetCacheKey(query) ?? Serialize(query);
			var connection = !string.IsNullOrWhiteSpace(ConnectionId)
				? ":ConnectionId={0}".FormatWith(QueryHashingEnabled ? ConnectionId.GetHashCode().ToString(CultureInfo.InvariantCulture) : ConnectionId)
				: null;
			var code = QueryHashingEnabled ? text.GetHashCode().ToString(CultureInfo.InvariantCulture) : text;
			var selector = !string.IsNullOrEmpty(selectorCacheKey)
				? ":Selector={0}".FormatWith(selectorCacheKey)
				: null;

			return "{0}{1}:Query={2}{3}".FormatWith(
				GetType().FullName,
				connection,
				code,
				selector);
		}

		protected string GetCacheKey(object request)
		{
			if (request == null) return null;

			// use the explicit key from the KeyedRequest

			if (request is KeyedRequest) return (request as KeyedRequest).Key;

			// optimized serializations

			if (request is RetrieveRequest
				&& (request as RetrieveRequest).ColumnSet.AllColumns
				&& (request as RetrieveRequest).RelatedEntitiesQuery == null)
			{
				return _serializedRetrieveRequestFormat.FormatWith(
					(request as RetrieveRequest).Target.LogicalName,
					(request as RetrieveRequest).Target.Id);
			}

			if (request is RetrieveRequest
				&& !(request as RetrieveRequest).ColumnSet.AllColumns
				&& (request as RetrieveRequest).ColumnSet.Columns.Count == 0
				&& (request as RetrieveRequest).RelatedEntitiesQuery != null
				&& (request as RetrieveRequest).RelatedEntitiesQuery.Count == 1
				&& IsBasicQueryExpression((request as RetrieveRequest).RelatedEntitiesQuery.First().Value))
			{
				var relnQuery = (request as RetrieveRequest).RelatedEntitiesQuery.First();
				var query = relnQuery.Value as QueryExpression;

				return _serializedRetrieveRequestWithRelatedQueryFormat.FormatWith(
					(request as RetrieveRequest).Target.LogicalName,
					(request as RetrieveRequest).Target.Id,
					relnQuery.Key.SchemaName,
					relnQuery.Key.PrimaryEntityRole != null ? ((int)relnQuery.Key.PrimaryEntityRole.Value).ToString() : "null",
					query.EntityName);
			}

			if (request is RetrieveAllEntitiesRequest)
			{
				return _serializedRetrieveAllEntitiesRequestFormat.FormatWith(
					(int)(request as RetrieveAllEntitiesRequest).EntityFilters,
					(request as RetrieveAllEntitiesRequest).RetrieveAsIfPublished);
			}

			if (request is RetrieveEntityRequest)
			{
				return _serializedRetrieveEntityRequestFormat.FormatWith(
					(int)(request as RetrieveEntityRequest).EntityFilters,
					(request as RetrieveEntityRequest).LogicalName,
					(request as RetrieveEntityRequest).RetrieveAsIfPublished);
			}

			if (request is RetrieveMultipleRequest
				&& IsBasicQueryExpression((request as RetrieveMultipleRequest).Query))
			{
				var query = (request as RetrieveMultipleRequest).Query as QueryExpression;

				return _serializedRetrieveMultipleRequestFormat.FormatWith(
					query.EntityName,
					query.Distinct);
			}

			if (request is RetrieveRelationshipRequest)
			{
				return _serializedRetrieveRelationshipRequestFormat.FormatWith(
					(request as RetrieveRelationshipRequest).Name,
					(request as RetrieveRelationshipRequest).RetrieveAsIfPublished);
			}

			return null;
		}

		private static bool IsBasicQueryExpression(QueryBase query)
		{
			var qe = query as QueryExpression;
			if (qe == null) return false;

			return qe.ColumnSet.AllColumns
				&& qe.Criteria.Conditions.Count == 0
				&& qe.Criteria.Filters.Count == 0
				&& qe.LinkEntities.Count == 0
				&& qe.Orders.Count == 0
				&& qe.PageInfo.Count == 0
				&& qe.PageInfo.PageNumber == 0
				&& qe.PageInfo.PagingCookie == null
				&& qe.PageInfo.ReturnTotalRecordCount == false;
		}

		private static string _serializedRetrieveRequestFormat;
		private static string _serializedRetrieveRequestWithRelatedQueryFormat;
		private static string _serializedRetrieveAllEntitiesRequestFormat;
		private static string _serializedRetrieveEntityRequestFormat;
		private static string _serializedRetrieveMultipleRequestFormat;
		private static string _serializedRetrieveRelationshipRequestFormat;

		private static void Initialize()
		{
			const string schemaName = "__schemaName__";
			const string entityName = "__entityName__";
			const string logicalName = "__logicalName__";
			const EntityFilters filters = EntityFilters.All;
			var id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

			var retrieveRequest = new RetrieveRequest { ColumnSet = new ColumnSet(true), Target = new EntityReference(logicalName, id) };
			_serializedRetrieveRequestFormat = SerializeForFormat(retrieveRequest)
				.Replace(logicalName, @"{0}")
				.Replace(id.ToString(), @"{1}");

			var relationship = new Relationship(schemaName) { PrimaryEntityRole = EntityRole.Referencing };
			var query = new RelationshipQueryCollection { { relationship, new QueryExpression(entityName) { ColumnSet = new ColumnSet(true) } } };
			var retrieveRequestWithRelatedQuery = new RetrieveRequest { ColumnSet = new ColumnSet(), Target = new EntityReference(logicalName, id), RelatedEntitiesQuery = query };
			_serializedRetrieveRequestWithRelatedQueryFormat = SerializeForFormat(retrieveRequestWithRelatedQuery)
				.Replace(logicalName, @"{0}")
				.Replace(id.ToString(), @"{1}")
				.Replace(schemaName, @"{2}")
				.Replace(@"""PrimaryEntityRole"":0", @"""PrimaryEntityRole"":{3}")
				.Replace(entityName, @"{4}");

			var retrieveAllEntitiesRequest = new RetrieveAllEntitiesRequest { EntityFilters = filters, RetrieveAsIfPublished = true };
			_serializedRetrieveAllEntitiesRequestFormat = SerializeForFormat(retrieveAllEntitiesRequest)
				.Replace(((int)filters).ToString(), @"{0}")
				.Replace("true", @"{1}");

			var retrieveEntityRequest = new RetrieveEntityRequest { EntityFilters = filters, LogicalName = logicalName, RetrieveAsIfPublished = true };
			_serializedRetrieveEntityRequestFormat = SerializeForFormat(retrieveEntityRequest)
				.Replace(((int)filters).ToString(), @"{0}")
				.Replace(logicalName, @"{1}")
				.Replace("true", @"{2}");

			var retrieveMultipleRequest = new RetrieveMultipleRequest { Query = new QueryExpression(logicalName) { Distinct = true } };
			_serializedRetrieveMultipleRequestFormat = SerializeForFormat(retrieveMultipleRequest)
				.Replace(logicalName, @"{0}")
				.Replace("true", @"{1}");

			var retrieveRelationshipRequest = new RetrieveRelationshipRequest { Name = logicalName, RetrieveAsIfPublished = true };
			_serializedRetrieveRelationshipRequestFormat = SerializeForFormat(retrieveRelationshipRequest)
				.Replace(logicalName, @"{0}")
				.Replace("true", @"{1}");
		}

		private static string Serialize(object value)
		{
			return value.SerializeByJson(KnownTypesProvider.QueryExpressionKnownTypes);
		}

		private static string SerializeForFormat(object value)
		{
			var text = Serialize(value);

			// escape the {} brackets

			return text.Replace("{", "{{").Replace("}", "}}");
		}

		private static readonly IEnumerable<string> _cachedRequestsContent = new[]
		{
			"Retrieve", "RetrieveMultiple",
		};

		private static readonly IEnumerable<string> _cachedRequestsMetadata = new[]
		{
			"RetrieveAllEntities",
			"RetrieveAllOptionSets",
			"RetrieveAllManagedProperties",
			"RetrieveAttribute",
			"RetrieveEntity",
			"RetrieveRelationship",
			"RetrieveTimestamp",
			"RetrieveOptionSet",
			"RetrieveManagedProperty",
		};

		private static readonly IEnumerable<string> _cachedRequests = _cachedRequestsContent.Concat(_cachedRequestsMetadata);

		private static readonly string[] _cachedRequestsSorted = _cachedRequests.OrderBy(r => r).ToArray();

		private static bool IsCachedRequest(OrganizationRequest request)
		{
			return request != null && Array.BinarySearch(_cachedRequestsSorted, request.RequestName) >= 0;
		}

		private static bool IsContentRequest(OrganizationRequest request)
		{
			return request != null && _cachedRequestsContent.Contains(request.RequestName);
		}

		private static bool IsMetadataRequest(OrganizationRequest request)
		{
			return request != null && _cachedRequestsMetadata.Contains(request.RequestName);
		}

		private void InvalidateCacheDependencies(IEnumerable<string> dependencies)
		{
			foreach (var dependency in dependencies)
			{
				InvalidateCacheDependency(dependency);
			}
		}

		private void InvalidateCacheDependency(string dependency)
		{
			Tracing.FrameworkInformation("OrganizationServiceCache", "InvalidateCacheDependency", dependency);

			Cache.Remove(dependency);
		}

		protected virtual TResult CloneResponse<TResult>(TResult item)
		{
			// clone the responses with potentially mutable data, metadata responses are treated as immutable

			var retrieveResponse = item as RetrieveResponse;
			if (retrieveResponse != null) return (TResult)CloneResponse(retrieveResponse);

			var retrieveMultipleResponse = item as RetrieveMultipleResponse;
			if (retrieveMultipleResponse != null) return (TResult)CloneResponse(retrieveMultipleResponse);

			return item;
		}

		private static object CloneResponse(RetrieveMultipleResponse response)
		{
			var clone = new RetrieveMultipleResponse();
			clone["EntityCollection"] = response.EntityCollection.Clone();
			return clone;
		}

		private static object CloneResponse(RetrieveResponse response)
		{
			var clone = new RetrieveResponse();
			clone["Entity"] = response.Entity.Clone();
			return clone;
		}
	}
}
