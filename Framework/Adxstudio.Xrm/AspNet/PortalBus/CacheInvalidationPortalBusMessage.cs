/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Threading.Tasks;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Owin;
using Microsoft.Xrm.Client.Services.Messages;
using Adxstudio.Xrm.Diagnostics.Trace;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	public class CacheInvalidationPortalBusMessage : PortalBusMessage
	{
		public OrganizationServiceCachePluginMessage Message { get; set; }

		public override Task InvokeAsync(IOwinContext context)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Id={0}", Id));

			Trace(Message);

			var cache = OrganizationServiceCachePluginMessageHandler.GetOrganizationServiceCache();
			cache.ExtendedRemoveLocal(Message);

			return Task.FromResult(0);
		}

		private static void Trace(OrganizationServiceCachePluginMessage message)
		{
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
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Related: Id={0}, LogicalName={1}, ", entity.Id, EntityNamePrivacy.GetEntityName(entity.LogicalName)));
				}
			}
		}
	}
}
