/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Client.Diagnostics;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Provides event handling behaviour for replication events for <see cref="EventCachedOrganizationService"/>.
	/// </summary>
	public class CmsReplicationOrganizationServiceEventProvider : IOrganizationServiceEventProvider
	{
		/// <summary>
		/// Entity created event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Created(object sender, OrganizationServiceCreatedEventArgs args)
		{
			var service = args.Service;

			if (service == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Cannot acquire IOrganizationService; done.");

				return;
			}

			GetReplication(service, args.EntityRequestedToCreate).Created();
		}

		/// <summary>
		/// Entity deleted event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Deleted(object sender, OrganizationServiceDeletedEventArgs args) { }

		/// <summary>
		/// Executed event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Executed(object sender, OrganizationServiceExecutedEventArgs args)
		{
			if (args != null && args.Request is CreateRequest)
			{
				var createRequest = (CreateRequest)args.Request;
				var entity = createRequest.Target;
				var service = args.Service;

				if (service == null)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Cannot acquire IOrganizationService; done.");

					return;
				}

				GetReplication(service, entity).Created();
			}
		}

		/// <summary>
		/// Entity updated event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Updated(object sender, OrganizationServiceUpdatedEventArgs args)
		{
			var service = args.Service;

			if (service == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Cannot acquire IOrganizationService; done.");

				return;
			}

			GetReplication(service, args.EntityRequestedToUpdate).Updated();
		}

		protected OrganizationServiceContext GetDataContext(IOrganizationService service)
		{
			return new OrganizationServiceContext(service);
		}

		protected IReplication GetReplication(IOrganizationService service, Entity entity)
		{
			var context = GetDataContext(service);

			var replicationFactory = new CrmEntityReplicationFactory(context);

			return replicationFactory.GetReplication(entity);
		}
	}
}
