/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client.Messages
{
	public static partial class OrganizationServiceContextExtensions
	{
		#region Helpers

		private static void Execute(
			this OrganizationServiceContext context,
			string requestName,
			IEnumerable<KeyValuePair<string, object>> parameters)
		{
			var request = new OrganizationRequest(requestName);

			foreach (var parameter in parameters)
			{
				request.Parameters[parameter.Key] = parameter.Value;
			}

			context.Execute(request);
		}

		private static TResult Execute<TResponse, TResult>(
			this OrganizationServiceContext service,
			OrganizationRequest request,
			Func<TResponse, TResult> selector)
			where TResponse : OrganizationResponse
		{
			var response = service.Execute<TResponse>(request);
			return response != null ? selector(response) : default(TResult);
		}

		private static T Execute<T>(
			this OrganizationServiceContext context,
			string requestName,
			IEnumerable<KeyValuePair<string, object>> parameters)
			where T : OrganizationResponse
		{
			var request = new OrganizationRequest(requestName);

			foreach (var parameter in parameters)
			{
				request.Parameters[parameter.Key] = parameter.Value;
			}

			return context.Execute<T>(request);
		}

		private static T Execute<T>(
			this OrganizationServiceContext context,
			string requestName,
			IEnumerable<KeyValuePair<string, object>> parameters,
			string key)
		{
			var request = new OrganizationRequest(requestName);

			foreach (var parameter in parameters)
			{
				request.Parameters[parameter.Key] = parameter.Value;
			}

			return context.Return<T>(request, key);
		}

		private static T Return<T>(this OrganizationServiceContext context, OrganizationRequest request, string key)
		{
			var response = context.Execute(request);
			return response.Return<T>(key);
		}

		private static T Return<T>(this OrganizationResponse response, string key)
		{
			return response.Results.Contains(key) ? (T)response[key] : default(T);
		}

		#endregion

		public static RetrieveMultipleResponse RetrieveMultiple(this OrganizationServiceContext service, RetrieveMultipleRequest request)
		{
			return service.Execute<RetrieveMultipleResponse>(request);
		}

		public static EntityCollection RetrieveMultiple(this OrganizationServiceContext service, QueryBase query)
		{
			return Execute<RetrieveMultipleResponse, EntityCollection>(service,
				new RetrieveMultipleRequest
				{
					Query = query
				},
				response => response.EntityCollection);
		}

		public static EntityMetadata RetrieveEntity(
			this OrganizationServiceContext service,
			string logicalName,
			EntityFilters entityFilters = EntityFilters.Default,
			bool retrieveAsIfPublished = false)
		{
			const string requestName = "RetrieveEntity";
			var parameters = new Dictionary<string, object>
			{
				{ "LogicalName", logicalName },
				{ "EntityFilters", entityFilters },
				{ "RetrieveAsIfPublished", retrieveAsIfPublished },
				{ "MetadataId", Guid.Empty },
			};

			return service.Execute<EntityMetadata>(requestName, parameters, "EntityMetadata");
		}

		public static AttributeMetadata RetrieveAttribute(
			this OrganizationServiceContext service,
			string entityLogicalName,
			string logicalName,
			bool retrieveAsIfPublished = false)
		{
			const string requestName = "RetrieveAttribute";
			var parameters = new Dictionary<string, object>
			{
				{ "EntityLogicalName", entityLogicalName },
				{ "LogicalName", logicalName },
				{ "RetrieveAsIfPublished", retrieveAsIfPublished },
				{ "MetadataId", Guid.Empty },
			};

			return service.Execute<AttributeMetadata>(requestName, parameters, "AttributeMetadata");
		}

		public static EntityMetadata[] RetrieveAllEntities(
			this OrganizationServiceContext service,
			EntityFilters entityFilters = EntityFilters.Default,
			bool retrieveAsIfPublished = false)
		{
			const string requestName = "RetrieveAllEntities";
			var parameters = new Dictionary<string, object>
			{
				{ "EntityFilters", entityFilters },
				{ "RetrieveAsIfPublished", retrieveAsIfPublished },
				{ "MetadataId", Guid.Empty },
			};

			return service.Execute<EntityMetadata[]>(requestName, parameters, "EntityMetadata");
		}

		public static void SetState(this OrganizationServiceContext context, int state, int status, EntityReference entityMoniker)
		{
			SetState(context, entityMoniker, new OptionSetValue(state), new OptionSetValue(status));
		}

		public static void SetState(this OrganizationServiceContext service, int state, int status, Entity entity)
		{
			SetState(service, state, status, entity.ToEntityReference());
		}

		public static void WinOpportunity(this OrganizationServiceContext context, Entity opportunityClose, int status)
		{
			WinOpportunity(context, opportunityClose, new OptionSetValue(status));
		}

		public static void LoseOpportunity(this OrganizationServiceContext context, Entity opportunityClose, int status)
		{
			LoseOpportunity(context, opportunityClose, new OptionSetValue(status));
		}

		public static void CloseIncident(this OrganizationServiceContext context, Entity incidentResolution, int status)
		{
			CloseIncident(context, incidentResolution, new OptionSetValue(status));
		}
	}
}
