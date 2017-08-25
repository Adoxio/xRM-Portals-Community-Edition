/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Services
{
	/// <summary>
	/// An <see cref="IOrganizationService"/> that traces the service request parameters.
	/// </summary>
	public class DebugOrganizationService : CrmOnlineOrganizationService
	{
		/// <summary>
		/// Trace the service response if applicable.
		/// </summary>
		public bool IncludeResponse { get; set; }

		/// <summary>
		/// The JSON serialization formatting.
		/// </summary>
		public Formatting Formatting { get; set; }

		public DebugOrganizationService(string connectionStringName) : base(connectionStringName)
		{
		}

		public DebugOrganizationService(CrmConnection connection) : base(connection)
		{
		}

		public DebugOrganizationService(IOrganizationService service) : base(service)
		{
		}

		public DebugOrganizationService(IOrganizationService service, string connectionId) : base(service, connectionId)
		{
		}

		public DebugOrganizationService(string connectionStringName, IOrganizationServiceCache cache) : base(connectionStringName, cache)
		{
		}

		public DebugOrganizationService(CrmConnection connection, IOrganizationServiceCache cache) : base(connection, cache)
		{
		}

		public DebugOrganizationService(IOrganizationService service, IOrganizationServiceCache cache) : base(service, cache)
		{
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			Formatting formatting;

			if (Enum.TryParse(config["formatting"], out formatting))
			{
				Formatting = formatting;
			}

			bool includeResponse;

			if (bool.TryParse(config["includeResponse"], out includeResponse))
			{
				IncludeResponse = includeResponse;
			}

			base.Initialize(name, config);
		}

		public override Guid Create(Entity entity)
		{
			Trace("Create", entity);

			var id = base.Create(entity);

			if (IncludeResponse)
			{
				Trace("Create", new { Id = id });
			}

			return id;
		}

		public override void Delete(string entityName, Guid id)
		{
			Trace("Delete", new { EntityName = entityName, Id = id });

			base.Delete(entityName, id);
		}

		public override void Update(Entity entity)
		{
			Trace("Update", entity);

			base.Update(entity);
		}

		public override Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
		{
			Trace("Retrieve", new { EntityName = entityName, Id = id, ColumnSet = columnSet });

			var entity = base.Retrieve(entityName, id, columnSet);

			if (IncludeResponse)
			{
				Trace("Retrieve", entity);
			}

			return entity;
		}

		public override EntityCollection RetrieveMultiple(QueryBase query)
		{
			Trace("RetrieveMultiple", query);
			Trace("RetrieveMultiple", query as FetchExpression);

			var entities = base.RetrieveMultiple(query);

			if (IncludeResponse)
			{
				Trace("RetrieveMultiple", entities);
			}

			return entities;
		}

		public override OrganizationResponse Execute(OrganizationRequest request)
		{
			Trace("Execute", request);
			Trace("Execute", request as RetrieveMultipleRequest);

			var response = base.Execute(request);

			if (IncludeResponse)
			{
				Trace("Execute", response);
			}

			return response;
		}

		private static void Trace(string member, RetrieveMultipleRequest request)
		{
			if (request == null) return;

			Trace(member, request.Query as FetchExpression);
		}

		private static void Trace(string member, FetchExpression query)
		{
			if (query == null) return;

			var fetch = XElement.Parse(query.Query);
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("{0}{1}", member, fetch.ToString()));
		}

		private void Trace(string member, object obj)
		{
			var json = JsonConvert.SerializeObject(obj, Formatting);
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("{0}{1}", member, json));
		}
	}
}
