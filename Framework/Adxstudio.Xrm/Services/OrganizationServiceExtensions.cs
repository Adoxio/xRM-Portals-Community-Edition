/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk.Metadata;

	/// <summary>
	/// Helper functions for the <see cref="IOrganizationService"/> interface.
	/// </summary>
	public static class OrganizationServiceExtensions
	{
		#region Execute

		/// <summary>
		/// Executes a request.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="request">The request.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static OrganizationResponse ExecuteRequest(
			this IOrganizationService service,
			OrganizationRequest request,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return service.Execute(ToCachedOrganizationRequest(request, flag, null, memberName, sourceFilePath, sourceLineNumber));
		}

		#endregion

		#region RetrieveSingle

		/// <summary>Retrieves a single entity or throws an exception if the entity does not exist.</summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="columnSet">The attributes to select.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			EntityReference target,
			ColumnSet columnSet,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var request = new RetrieveRequest { Target = target, ColumnSet = columnSet };
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, expiration, memberName, sourceFilePath, sourceLineNumber)) as RetrieveResponse;

			return response.Entity;
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="query">A query that returns a single entity.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			QueryBase query,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var request = new RetrieveSingleRequest(query) { EnforceSingle = enforceSingle, EnforceFirst = enforceFirst };
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, expiration, memberName, sourceFilePath, sourceLineNumber)) as RetrieveSingleResponse;

			return response.Entity;
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="fetch">A query that returns a single entity.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			Fetch fetch,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var request = new RetrieveSingleRequest(fetch) { EnforceSingle = enforceSingle, EnforceFirst = enforceFirst };
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, expiration, memberName, sourceFilePath, sourceLineNumber)) as RetrieveSingleResponse;

			return response.Entity;
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="condition">The fetch condition.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			string logicalName,
			IEnumerable<string> columns,
			Condition condition,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return service.RetrieveSingle(logicalName, columns, new[] { condition }, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(logicalName, columns)
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = conditions
						}
					}
				}
			};

			return service.RetrieveSingle(fetch, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="condition">The fetch condition.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			string logicalName,
			ICollection<FetchAttribute> attributes,
			Condition condition,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return service.RetrieveSingle(logicalName, attributes, new[] { condition }, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			string logicalName,
			ICollection<FetchAttribute> attributes,
			ICollection<Condition> conditions,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(logicalName)
				{
					Attributes = attributes,
					Filters = new[]
					{
						new Filter
						{
							Conditions = conditions
						}
					}
				}
			};

			return service.RetrieveSingle(fetch, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="primaryAttribute">The primary Attribute.</param>
		/// <param name="id">The id.</param>
		/// <param name="attributes">The columns.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this IOrganizationService service,
			string logicalName,
			string primaryAttribute,
			Guid id,
			ICollection<FetchAttribute> attributes,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var condition = new Condition(primaryAttribute, ConditionOperator.Equal, id);

			return service.RetrieveSingle(logicalName, attributes, condition, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		#endregion

		#region RetrieveMultiple

		/// <summary>
		/// Retrieves entities.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static EntityCollection RetrieveMultiple(
			this IOrganizationService service,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(logicalName, columns)
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = conditions
						}
					}
				}
			};

			return service.RetrieveMultiple(fetch, flag, expiration, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="fetch">The fetch query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entities.</returns>
		public static EntityCollection RetrieveMultiple(
			this IOrganizationService service,
			Fetch fetch,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Func<Fetch, EntityCollection> fetchPage = query => FetchPage(service, query, flag, expiration, memberName, sourceFilePath, sourceLineNumber);

			var pages = FetchPages(fetch, fetchPage).ToList();
			var allPages = JoinPages(pages);

			return allPages;
		}

		/// <summary>
		/// Join multiple pages into a single <see cref="EntityCollection"/>.
		/// </summary>
		/// <param name="pages">The pages.</param>
		/// <returns>The joined page collection.</returns>
		private static EntityCollection JoinPages(ICollection<EntityCollection> pages)
		{
			if (pages.Count < 2)
			{
				return pages.FirstOrDefault();
			}

			var last = pages.Last();
			var joinedPages = pages.SelectMany(page => page.Entities).ToList();

			var entities = new EntityCollection(joinedPages)
			{
				EntityName = last.EntityName,
				ExtensionData = last.ExtensionData,
				MinActiveRowVersion = last.MinActiveRowVersion,
				MoreRecords = last.MoreRecords,
				PagingCookie = last.PagingCookie,
				TotalRecordCount = last.TotalRecordCount,
				TotalRecordCountLimitExceeded = last.TotalRecordCountLimitExceeded
			};

			return entities;
		}

		#endregion

		#region RetrieveAll

		/// <summary>
		/// Retrieves all entities by paging results.
		/// </summary>
		/// <remarks>
		/// Breaking out of the <see cref="IEnumerable{Entity}"/> early terminates retrieval of further pages.
		/// </remarks>
		/// <param name="service">The service.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static IEnumerable<Entity> RetrieveAll(
			this IOrganizationService service,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity(logicalName, columns)
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = conditions
						}
					}
				}
			};

			return service.RetrieveAll(fetch, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves all entities by paging results.
		/// </summary>
		/// <remarks>
		/// Breaking out of the <see cref="IEnumerable{Entity}"/> early terminates retrieval of further pages.
		/// </remarks>
		/// <param name="service">The service.</param>
		/// <param name="fetch">The fetch query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entities.</returns>
		public static IEnumerable<Entity> RetrieveAll(
			this IOrganizationService service,
			Fetch fetch,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Func<Fetch, EntityCollection> fetchPage = query => FetchPage(service, query, flag, null, memberName, sourceFilePath, sourceLineNumber);

			return FetchPages(fetch, fetchPage).SelectMany(page => page.Entities);
		}

		/// <summary>
		/// Retrieves all entities by paging results.
		/// </summary>
		/// <remarks>
		/// Breaking out of the <see cref="IEnumerable{EntityCollection}"/> early terminates retrieval of further pages.
		/// </remarks>
		/// <param name="query">The query.</param>
		/// <param name="fetchPage">The action to fetch a single page.</param>
		/// <returns>The entities.</returns>
		private static IEnumerable<EntityCollection> FetchPages(Fetch query, Func<Fetch, EntityCollection> fetchPage)
		{
			if (query.PageNumber == null && query.PageSize == null && query.PagingCookie == null)
			{
				// manage paging automatically
				bool moreRecords;
				var pagingCookie = string.Empty;
				var pageNumber = 1;
				const int PageCount = 5000;

				do
				{
					query.PageNumber = pageNumber;
					query.PageSize = PageCount;

					if (!string.IsNullOrWhiteSpace(pagingCookie))
					{
						query.PagingCookie = pagingCookie;
					}

					var page = fetchPage(query);

					moreRecords = page.MoreRecords;
					pagingCookie = page.PagingCookie;

					yield return page;

					++pageNumber;
				}
				while (moreRecords);
			}
			else
			{
				// explicit paging is specified
				var page = fetchPage(query);

				yield return page;
			}
		}

		/// <summary>
		/// Execute individual fetch.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="query">The query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName"> The member name.</param>
		/// <param name="sourceFilePath"> The source file path.</param>
		/// <param name="sourceLineNumber"> The source line number.</param>
		/// <returns>The entities.</returns>
		private static EntityCollection FetchPage(
			IOrganizationService service,
			Fetch query,
			RequestFlag flag,
			TimeSpan? expiration,
			string memberName,
			string sourceFilePath,
			int sourceLineNumber)
		{
			var request = new FetchMultipleRequest(query);
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, expiration, memberName, sourceFilePath, sourceLineNumber)) as RetrieveMultipleResponse;
			var result = response.EntityCollection;

			return result;
		}

		#endregion

		#region RetrieveRelatedEntity

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this IOrganizationService service,
			Entity entity,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return RetrieveRelatedEntity(service, entity.ToEntityReference(), relationshipSchemaName, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this IOrganizationService service,
			EntityReference target,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var entities = GetRelatedEntities(service, target, relationshipSchemaName, EntityRole.Referencing, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber);
			return entities.Entities.SingleOrDefault();
		}

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this IOrganizationService service,
			Entity entity,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return RetrieveRelatedEntity(service, entity.ToEntityReference(), relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this IOrganizationService service,
			EntityReference target,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var entities = GetRelatedEntities(service, target, relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber);
			return entities.Entities.SingleOrDefault();
		}

		#endregion

		#region RetrieveRelatedEntities

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this IOrganizationService service,
			Entity entity,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return RetrieveRelatedEntities(service, entity.ToEntityReference(), relationshipSchemaName, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this IOrganizationService service,
			EntityReference target,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return GetRelatedEntities(service, target, relationshipSchemaName, EntityRole.Referenced, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this IOrganizationService service,
			Entity entity,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return RetrieveRelatedEntities(service, entity.ToEntityReference(), relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this IOrganizationService service,
			EntityReference target,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return GetRelatedEntities(service, target, relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="defaultEntityRole">The default relationship role for reflexive relationships.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		private static EntityCollection GetRelatedEntities(
			IOrganizationService service,
			EntityReference target,
			string relationshipSchemaName,
			EntityRole defaultEntityRole,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var primaryEntityRole = IsReflexive(service, relationshipSchemaName) ? defaultEntityRole : (EntityRole?)null;
			var relationship = relationshipSchemaName.ToRelationship(primaryEntityRole);
			var attributes = columns != null ? columns.Select(column => new FetchAttribute(column)).ToArray() : null;

			return GetRelatedEntities(service, target, relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		private static EntityCollection GetRelatedEntities(
			IOrganizationService service,
			EntityReference target,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			service.ThrowOnNull("service");
			target.ThrowOnNull("target");
			relationship.ThrowOnNull("relationship");

			var entityName = GetRelatedEntityName(service, target, relationship);

			var fetch = new Fetch
			{
				Entity = new FetchEntity(entityName)
				{
					Attributes = attributes ?? FetchAttribute.All,
					Filters = filters
				}
			};

			Func<Fetch, EntityCollection> fetchPage = query => FetchPage(service, target, relationship, fetch, flag, memberName, sourceFilePath, sourceLineNumber);

			var pages = FetchPages(fetch, fetchPage).ToList();
			var allPages = JoinPages(pages);

			return allPages;
		}

		/// <summary>
		/// Execute individual fetch.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="fetch">The query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName"> The member name.</param>
		/// <param name="sourceFilePath"> The source file path.</param>
		/// <param name="sourceLineNumber"> The source line number.</param>
		/// <returns>The entities.</returns>
		private static EntityCollection FetchPage(
			IOrganizationService service,
			EntityReference target,
			Relationship relationship,
			Fetch fetch,
			RequestFlag flag,
			string memberName,
			string sourceFilePath,
			int sourceLineNumber)
		{
			// required to provide an entityName value
			var query = new RelationshipQueryCollection
			{
				{ relationship, fetch.ToFetchExpression() }
			};

			var request = new RetrieveRequest { Target = target, ColumnSet = new ColumnSet(), RelatedEntitiesQuery = query };
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, null, memberName, sourceFilePath, sourceLineNumber)) as RetrieveResponse;

			var related = response.Entity.RelatedEntities;
			var result = related.Contains(relationship) ? related[relationship] : null;

			return result;
		}

		/// <summary>
		/// Retrieves the related entity name.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <returns>The entity name.</returns>
		private static string GetRelatedEntityName(IOrganizationService service, EntityReference target, Relationship relationship)
		{
			// reflexive relationships have the same entity logical name

			if (relationship.PrimaryEntityRole != null)
			{
				return target.LogicalName;
			}

			// for non-reflexive relationships take the other entity logical name

			// Todo : check if we need to use stale data feature here.
			var response = service.ExecuteRequest(new RetrieveRelationshipRequest { Name = relationship.SchemaName }) as RetrieveRelationshipResponse;

			var oneToMany = response.RelationshipMetadata as OneToManyRelationshipMetadata;

			if (oneToMany != null)
			{
				return oneToMany.ReferencingEntity == target.LogicalName
					? oneToMany.ReferencedEntity
					: oneToMany.ReferencingEntity;
			}

			var manyToMany = response.RelationshipMetadata as ManyToManyRelationshipMetadata;

			if (manyToMany != null)
			{
				return manyToMany.Entity1LogicalName == target.LogicalName
					? manyToMany.Entity2LogicalName
					: manyToMany.Entity1LogicalName;
			}

			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load the '{0}' relationship.", relationship.SchemaName));
		}

		/// <summary>
		/// Uses metadata to determine if a relationship is reflexive.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <returns>'true' if the relationship is reflexive.</returns>
		private static bool IsReflexive(IOrganizationService service, string relationshipSchemaName)
		{
			// Todo: check if we need to use Stale data feature here.
			var response = service.ExecuteRequest(new RetrieveRelationshipRequest { Name = relationshipSchemaName }) as RetrieveRelationshipResponse;

			var oneToMany = response.RelationshipMetadata as OneToManyRelationshipMetadata;

			if (oneToMany != null)
			{
				return oneToMany.ReferencedEntity == oneToMany.ReferencingEntity;
			}

			var manyToMany = response.RelationshipMetadata as ManyToManyRelationshipMetadata;

			if (manyToMany != null)
			{
				return manyToMany.Entity2LogicalName == manyToMany.Entity1LogicalName;
			}

			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load the '{0}' relationship.", relationshipSchemaName));
		}

		#endregion

		#region Create

		/// <summary>
		/// Creates crm entity record.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The member name.</param>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="sourceLineNumber">The source line number</param>
		/// <returns>Guid of the entity that got created.</returns>
		public static Guid ExecuteCreate(
			this IOrganizationService service, 
			Entity entity,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var request = new CreateRequest { Target = entity };
			var response = service.Execute(ToCachedOrganizationRequest(request, flag, null, memberName, sourceFilePath, sourceLineNumber)) as CreateResponse;
			return response.id;
		}

		#endregion

		#region Update

		/// <summary>
		/// Updates crm entity record.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The member name.</param>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="sourceLineNumber">The source line number</param>
		public static void ExecuteUpdate(
			this IOrganizationService service,
			Entity entity,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var request = new UpdateRequest { Target = entity };
			service.Execute(ToCachedOrganizationRequest(request, flag, null, memberName, sourceFilePath, sourceLineNumber));
		}

		#endregion

		#region Helper Methods

		/// <summary>To the cached organization request object.</summary>
		/// <param name="request">The request.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The member name.</param>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="sourceLineNumber">The source line number.</param>
		/// <returns>The <see cref="CachedOrganizationRequest"/>.</returns>
		private static CachedOrganizationRequest ToCachedOrganizationRequest(
			OrganizationRequest request,
			RequestFlag flag,
			TimeSpan? expiration,
			string memberName,
			string sourceFilePath,
			int sourceLineNumber)
		{
			var caller = new Caller { MemberName = memberName, SourceFilePath = sourceFilePath, SourceLineNumber = sourceLineNumber };
			var req = new CachedOrganizationRequest(request, flag, expiration, caller);
			return req;
		}

		#endregion
	}
}
