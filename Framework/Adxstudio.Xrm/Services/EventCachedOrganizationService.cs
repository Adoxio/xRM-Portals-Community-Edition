/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Services
{
	public class EventCachedOrganizationService : CachedOrganizationService
	{
		public OrganizationServiceEvents Events { get; set; }

		public EventCachedOrganizationService(string connectionStringName)
			: base(connectionStringName)
		{
		}

		public EventCachedOrganizationService(CrmConnection connection)
			: base(connection)
		{
		}

		public EventCachedOrganizationService(IOrganizationService service, string connectionId = null)
			: base(service, connectionId)
		{
		}

		public EventCachedOrganizationService(string connectionStringName, IOrganizationServiceCache cache)
			: base(connectionStringName, cache)
		{
		}

		public EventCachedOrganizationService(CrmConnection connection, IOrganizationServiceCache cache)
			: base(connection, cache)
		{
		}

		public EventCachedOrganizationService(IOrganizationService service, IOrganizationServiceCache cache)
			: base(service, cache)
		{
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			var eventName = config["eventName"] ?? name;

			Events = new OrganizationServiceEvents(eventName);

			base.Initialize(name, config);
		}

		public override Guid Create(Entity entity)
		{
			var id = base.Create(entity);

			if (Events != null) OnCreated(new OrganizationServiceCreatedEventArgs(this, entity, id));

			return id;
		}

		public override void Delete(string entityName, Guid id)
		{
			base.Delete(entityName, id);

			if (Events != null) OnDeleted(new OrganizationServiceDeletedEventArgs(this, entityName, id));
		}

		public override void Update(Entity entity)
		{
			base.Update(entity);

			if (Events != null) OnUpdated(new OrganizationServiceUpdatedEventArgs(this, entity));
		}

		public override OrganizationResponse Execute(OrganizationRequest request)
		{
			var response = base.Execute(request);

			if (Events != null) OnExecuted(new OrganizationServiceExecutedEventArgs(this, request, response));

			return response;
		}

		#region Event Invocation

		protected virtual void OnCreated(OrganizationServiceCreatedEventArgs e)
		{
			Events.OnCreated(e);
		}

		protected virtual void OnDeleted(OrganizationServiceDeletedEventArgs e)
		{
			Events.OnDeleted(e);
		}

		protected virtual void OnExecuted(OrganizationServiceExecutedEventArgs e)
		{
			Events.OnExecuted(e);
		}

		protected virtual void OnUpdated(OrganizationServiceUpdatedEventArgs e)
		{
			Events.OnUpdated(e);
		}

		#endregion
	}
}
