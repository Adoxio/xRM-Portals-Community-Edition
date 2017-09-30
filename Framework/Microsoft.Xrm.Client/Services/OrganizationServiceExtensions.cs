/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client.Services
{
	public static class OrganizationServiceExtensions
	{
		public static TResponse Execute<TResponse>(this IOrganizationService service, OrganizationRequest request)
			where TResponse : OrganizationResponse
		{
			return service.Execute(request) as TResponse;
		}

		private static TResult Execute<TResponse, TResult>(this IOrganizationService service, OrganizationRequest request, Func<TResponse, TResult> selector)
			where TResponse : OrganizationResponse
		{
			var response = service.Execute<TResponse>(request);
			return response != null ? selector(response) : default(TResult);
		}

		public static RetrieveEntityResponse RetrieveEntity(this IOrganizationService service, RetrieveEntityRequest request)
		{
			return service.Execute<RetrieveEntityResponse>(request);
		}

		public static EntityMetadata RetrieveEntity(this IOrganizationService service, string logicalName, EntityFilters entityFilters = default(EntityFilters), bool retrieveAsIfPublished = default(bool))
		{
			return Execute<RetrieveEntityResponse, EntityMetadata>(service,
				new RetrieveEntityRequest
				{
					LogicalName = logicalName,
					EntityFilters = entityFilters,
					RetrieveAsIfPublished = retrieveAsIfPublished,
				},
				response => response.EntityMetadata);
		}

		public static RetrieveMultipleResponse RetrieveMultiple(this IOrganizationService service, RetrieveMultipleRequest request)
		{
			return service.Execute<RetrieveMultipleResponse>(request);
		}

		public static EntityCollection RetrieveMultiple(this IOrganizationService service, QueryBase query)
		{
			return Execute<RetrieveMultipleResponse, EntityCollection>(service,
				new RetrieveMultipleRequest
				{
					Query = query
				},
				response => response.EntityCollection);
		}

		public static QueryExpression FetchXmlToQueryExpression(this IOrganizationService service, string fetchXml)
		{
			const string requestName = "FetchXmlToQueryExpression";
			var parameters = new Dictionary<string, object>
			{
				{ "FetchXml", fetchXml },
			};

			return service.Execute<QueryExpression>(requestName, parameters, "Query");
		}

		#region Helpers

		private static T Execute<T>(
			this IOrganizationService service,
			string requestName,
			IEnumerable<KeyValuePair<string, object>> parameters,
			string key)
		{
			var request = new OrganizationRequest(requestName);

			foreach (var parameter in parameters)
			{
				request.Parameters[parameter.Key] = parameter.Value;
			}

			return service.Return<T>(request, key);
		}

		private static T Return<T>(this IOrganizationService service, OrganizationRequest request, string key)
		{
			var response = service.Execute(request);
			return response.Return<T>(key);
		}

		private static T Return<T>(this OrganizationResponse response, string key)
		{
			return response.Results.Contains(key) ? (T)response[key] : default(T);
		}

		#endregion
	}
}
