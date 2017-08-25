/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.ServiceModel;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Client.Services.Messages;

namespace Adxstudio.Xrm.Caching
{
	/// <summary>
	/// An AppFabric Service Bus cache invalidation client channel.
	/// </summary>
	public interface IOrganizationServiceCacheChannel : IOrganizationServiceCacheService, IClientChannel { }

	/// <summary>
	/// An AppFabric Service Bus cache invalidation service.
	/// </summary>
	[ServiceContract(Namespace = Namespaces.Default)]
	public interface IOrganizationServiceCacheService
	{
		/// <summary>
		/// Removes cache items based on a message description.
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void Remove(OrganizationServiceCachePluginMessage message);
	}

	/// <summary>
	/// A service that performs local cache invalidations using a <see cref="IExtendedOrganizationServiceCache"/> provider.
	/// </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
	public class OrganizationServiceCacheService : IOrganizationServiceCacheService
	{
		private readonly IOrganizationServiceCache _cache;

		public OrganizationServiceCacheService(IOrganizationServiceCache cache)
		{
			_cache = cache;
		}

		public virtual void Remove(OrganizationServiceCachePluginMessage message)
		{
			try
			{
				message.ThrowOnNull("message");

                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("MessageName={0}, ServiceCacheName={1}, ConnectionStringName={2}", message.MessageName, message.ServiceCacheName, message.ConnectionStringName));

				if (message.Target != null && message.Relationship == null)
				{
					var entity = message.Target.ToEntityReference();

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Id={0}, LogicalName={1}", entity.Id, EntityNamePrivacy.GetEntityName(entity.LogicalName)));
				}

				if (message.Category != null)
				{
					var category = message.Category.Value;

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("category={0}", category));
				}

				if (message.Target != null && message.Relationship != null && message.RelatedEntities != null)
				{
					var target = message.Target.ToEntityReference();
					var relationship = message.Relationship.ToRelationship();
					var relatedEntities = message.RelatedEntities.ToEntityReferenceCollection();

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Target: Id={0}, LogicalName={1}", target.Id, EntityNamePrivacy.GetEntityName(target.LogicalName)));
                    foreach (var entity in relatedEntities)
					{
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Related: Id={0}, LogicalName={1}", entity.Id, EntityNamePrivacy.GetEntityName(entity.LogicalName)));
                    }
				}

				_cache.ExtendedRemoveLocal(message);
			}
			catch (Exception error)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, error.ToString());
			}
		}
	}
}
