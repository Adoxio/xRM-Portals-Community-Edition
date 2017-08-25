/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// Indicates that a class contains an <see cref="IOrganizationServiceCache"/>.
	/// </summary>
	public interface IOrganizationServiceCacheContainer
	{
		/// <summary>
		/// Gets the service cache.
		/// </summary>
		IOrganizationServiceCache Cache { get; }
	}

	/// <summary>
	/// An <see cref="OrganizationService"/> that utilizes an <see cref="IOrganizationServiceCache"/> for caching service responses.
	/// </summary>
	public class CachedOrganizationService : OrganizationService, IOrganizationServiceCacheContainer
	{
		/// <summary>
		/// The caching provider.
		/// </summary>
		public IOrganizationServiceCache Cache { get; private set; }

		public CachedOrganizationService(string connectionStringName)
			: this(new CrmConnection(connectionStringName))
		{
		}

		public CachedOrganizationService(CrmConnection connection)
			: base(connection)
		{
			Initialze(connection.GetConnectionId());
		}

		public CachedOrganizationService(IOrganizationService service)
			: this(service, (string)null)
		{
		}

		public CachedOrganizationService(IOrganizationService service, string connectionId)
			: base(service)
		{
			Initialze(connectionId);
		}

		public CachedOrganizationService(string connectionStringName, IOrganizationServiceCache cache)
			: this(new CrmConnection(connectionStringName), cache)
		{
		}

		public CachedOrganizationService(CrmConnection connection, IOrganizationServiceCache cache)
			: base(connection)
		{
			Cache = cache;
		}

		public CachedOrganizationService(IOrganizationService service, IOrganizationServiceCache cache)
			: base(service)
		{
			Cache = cache;
		}

		private void Initialze(string connectionId)
		{
			Cache = new OrganizationServiceCache(null, connectionId);
		}

		public override Guid Create(Entity entity)
		{
			var timer = Stopwatch.StartNew();

			var id = base.Create(entity);

			if (Cache != null) Cache.Remove(entity);

			timer.Stop();

			Tracing.FrameworkInformation("CachedOrganizationService", "Create", "id={0}: {1} ms", id, timer.ElapsedMilliseconds);

			return id;
		}

		public override void Delete(string entityName, Guid id)
		{
			var timer = Stopwatch.StartNew();

			base.Delete(entityName, id);

			if (Cache != null) Cache.Remove(entityName, id);

			timer.Stop();

			Tracing.FrameworkInformation("CachedOrganizationService", "Delete", "id={0}: {1} ms", id, timer.ElapsedMilliseconds);
		}

		public override void Update(Entity entity)
		{
			var timer = Stopwatch.StartNew();

			base.Update(entity);

			if (Cache != null) Cache.Remove(entity);

			timer.Stop();

			Tracing.FrameworkInformation("CachedOrganizationService", "Update", "{0} ms", timer.ElapsedMilliseconds);
		}

		public override Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
		{
			var request = new RetrieveRequest
			{
				Target = new EntityReference { LogicalName = entityName, Id = id },
				ColumnSet = columnSet
			};

			var response = Execute<RetrieveResponse>(request);

			return response != null ? response.Entity : null;
		}

		public override EntityCollection RetrieveMultiple(QueryBase query)
		{
			var request = new RetrieveMultipleRequest { Query = query };
			var response = Execute<RetrieveMultipleResponse>(request);

			return response != null ? response.EntityCollection : null;
		}

		public override OrganizationResponse Execute(OrganizationRequest request)
		{
			return Execute<OrganizationResponse>(request);
		}

		public T Execute<T>(OrganizationRequest request, Func<OrganizationResponse, T> selector, string selectorCacheKey)
		{
			var execute = Cache != null ? Cache.Execute : null as Func<OrganizationRequest, Func<OrganizationRequest, OrganizationResponse>, Func<OrganizationResponse, T>, string, T>;

			return Execute(request, execute, selector, selectorCacheKey);
		}

		private T Execute<T>(OrganizationRequest request) where T : OrganizationResponse
		{
			return Execute(request, response => response as T, null);
		}

		private T Execute<T>(
			OrganizationRequest request,
			Func<OrganizationRequest, Func<OrganizationRequest, OrganizationResponse>, Func<OrganizationResponse, T>, string, T> execute, Func<OrganizationResponse, T> selector,
			string selectorCacheKey)
		{
			return execute != null
				? execute(request, InnerExecute, selector, selectorCacheKey)
				: selector(InnerExecute(request));
		}

		private OrganizationResponse InnerExecute(OrganizationRequest request)
		{
			// unwrap the KeyedRequest

			var innerRequest = (request is KeyedRequest) ? (request as KeyedRequest).Request : request;

			var timer = Stopwatch.StartNew();

			var response = base.Execute(innerRequest);

			timer.Stop();

			Tracing.FrameworkInformation("CachedOrganizationService", "InnerExecute", "{0}: {1} ms", innerRequest.GetType().Name, timer.ElapsedMilliseconds);

			var retrieveMultipleResponse = response as RetrieveMultipleResponse;

			if (retrieveMultipleResponse != null)
			{
				var count = retrieveMultipleResponse.EntityCollection != null
					&& retrieveMultipleResponse.EntityCollection.Entities != null
						? retrieveMultipleResponse.EntityCollection.Entities.Count
						: 0;

				Tracing.FrameworkInformation("CachedOrganizationService", "InnerExecute", "Count: {0}", count);
			}

			return response;
		}
	}
}
