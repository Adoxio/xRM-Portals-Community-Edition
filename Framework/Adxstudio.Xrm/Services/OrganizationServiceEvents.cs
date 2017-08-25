/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Services
{
	public class OrganizationServiceEvents
	{
		/// <summary>
		/// Raised after the Create method is invoked.
		/// </summary>
		public event EventHandler<OrganizationServiceCreatedEventArgs> Created;

		/// <summary>
		/// Rasised after the Delete method is invoked.
		/// </summary>
		public event EventHandler<OrganizationServiceDeletedEventArgs> Deleted;

		/// <summary>
		/// Raised after the Execute method is invoked.
		/// </summary>
		public event EventHandler<OrganizationServiceExecutedEventArgs> Executed;

		/// <summary>
		/// Raised after the Update method is invoked.
		/// </summary>
		public event EventHandler<OrganizationServiceUpdatedEventArgs> Updated;

		public OrganizationServiceEvents()
		{
		}

		public OrganizationServiceEvents(string eventName)
		{
			LoadEventProviders(eventName);
		}

		private void LoadEventProviders(string eventName)
		{
			var providers = AdxstudioCrmConfigurationManager.CreateEventProviders(eventName);

			foreach (var provider in providers)
			{
				Created += provider.Created;
				Deleted += provider.Deleted;
				Executed += provider.Executed;
				Updated += provider.Updated;
			}
		}

		#region Event Invocation

		internal void OnCreated(OrganizationServiceCreatedEventArgs e)
		{
			var handler = Created;

			if (handler == null) return;

			handler(this, e);
		}

		internal void OnDeleted(OrganizationServiceDeletedEventArgs e)
		{
			var handler = Deleted;

			if (handler == null) return;

			handler(this, e);
		}

		internal void OnExecuted(OrganizationServiceExecutedEventArgs e)
		{
			var handler = Executed;

			if (handler == null) return;

			handler(this, e);
		}

		internal void OnUpdated(OrganizationServiceUpdatedEventArgs e)
		{
			var handler = Updated;

			if (handler == null) return;

			handler(this, e);
		}

		#endregion
	}

	public abstract class OrganizationServiceEventArgs : EventArgs
	{
		protected OrganizationServiceEventArgs(IOrganizationService service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			Service = service;
		}

		public IOrganizationService Service { get; private set; }
	}

	public class OrganizationServiceCreatedEventArgs : OrganizationServiceEventArgs
	{
		public OrganizationServiceCreatedEventArgs(IOrganizationService service, Entity entityRequestedToCreate, Guid responseId) : base(service)
		{
			EntityRequestedToCreate = entityRequestedToCreate;
			ResponseId = responseId;
		}

		public Entity EntityRequestedToCreate { get; private set; }

		public Guid ResponseId { get; private set; }
	}

	public class OrganizationServiceDeletedEventArgs : OrganizationServiceEventArgs
	{
		public OrganizationServiceDeletedEventArgs(IOrganizationService service, string entityNameRequestedToDelete, Guid entityIdRequestedToDelete) : base(service)
		{
			EntityNameRequestedToDelete = entityNameRequestedToDelete;
			EntityIdRequestedToDelete = entityIdRequestedToDelete;
		}

		public string EntityNameRequestedToDelete { get; private set; }

		public Guid EntityIdRequestedToDelete { get; private set; }
	}

	public class OrganizationServiceExecutedEventArgs : OrganizationServiceEventArgs
	{
		public OrganizationServiceExecutedEventArgs(IOrganizationService service, OrganizationRequest request, OrganizationResponse response) : base(service)
		{
			Request = request;
			Response = response;
		}

		public OrganizationRequest Request { get; private set; }

		public OrganizationResponse Response { get; private set; }
	}

	public class OrganizationServiceUpdatedEventArgs : OrganizationServiceEventArgs
	{
		public OrganizationServiceUpdatedEventArgs(IOrganizationService service, Entity entityRequestedToUpdate) : base(service)
		{
			EntityRequestedToUpdate = entityRequestedToUpdate;
		}

		public Entity EntityRequestedToUpdate { get; private set; }
	}
}
