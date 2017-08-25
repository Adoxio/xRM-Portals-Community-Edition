/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Collections.Specialized;
	using System.Runtime.Caching;
	using System.Web;
	using System.Web.Hosting;
	using Adxstudio.Xrm.AspNet;
	using Adxstudio.Xrm.AspNet.PortalBus;
	using Microsoft.Owin;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Services;
	using Microsoft.Xrm.Client.Services.Messages;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// An <see cref="IOrganizationService"/> that distributes invalidation messages through the portal bus.
	/// </summary>
	public class PortalBusOrganizationServiceCache : CompositeOrganizationServiceCache
	{
		private string _name;
		private string _connectionStringName;
		private bool _syncRemoveEnabled;

		public PortalBusOrganizationServiceCache()
		{
		}

		public PortalBusOrganizationServiceCache(ObjectCache cache)
			: base(cache)
		{
		}

		public PortalBusOrganizationServiceCache(ObjectCache cache, CrmConnection connection)
			: base(cache, connection)
		{
		}

		public PortalBusOrganizationServiceCache(ObjectCache cache, string connectionId)
			: base(cache, connectionId)
		{
		}

		public PortalBusOrganizationServiceCache(ObjectCache cache, OrganizationServiceCacheSettings settings)
			: base(cache, settings)
		{
		}

		public PortalBusOrganizationServiceCache(IOrganizationServiceCache inner)
			: base(inner)
		{
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			_name = name;

			if (config != null)
			{
				_connectionStringName = GetConnectionStringName(name, config);

				bool syncRemoveEnabled;

				if (bool.TryParse(config["syncRemoveEnabled"], out syncRemoveEnabled))
				{
					this._syncRemoveEnabled = syncRemoveEnabled;
				}
			}

			base.Initialize(name, config);
		}

		public override T Execute<T>(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> execute, Func<OrganizationResponse, T> selector, string selectorCacheKey)
		{
			request.ThrowOnNull("request");
			execute.ThrowOnNull("execute");

			var response = base.Execute(request, execute, selector, selectorCacheKey);

			// create a cache invalidation message from the organization service request/response

			var message = GetMessage(request, response as OrganizationResponse);

			if (message != null)
			{
				Remove(message);
			}

			return response;
		}

		public override void Remove(string entityLogicalName, Guid? id)
		{
			entityLogicalName.ThrowOnNullOrWhitespace("entityLogicalName");

			if (id != null)
			{
				var message = GetMessage(entityLogicalName, id.Value);

				Remove(message);
			}
			else
			{
				// currently, entity type invalidations are not published remotely

				base.Remove(entityLogicalName, id);
			}
		}

		public override void Remove(EntityReference entity)
		{
			entity.ThrowOnNull("entity");

			var message = GetMessage(entity.LogicalName, entity.Id);

			Remove(message);
		}

		public override void Remove(Entity entity)
		{
			entity.ThrowOnNull("entity");

			Remove(entity.ToEntityReference());
		}

		public override void Remove(OrganizationServiceCachePluginMessage message)
		{
			message.ThrowOnNull("message");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			IOwinContext context = null;

			if (HttpContext.Current != null)
			{
				try
				{
					context = HttpContext.Current.GetOwinContext();
				}
				catch (Exception)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No owin.Environment item was found in the context");
					return;
				}
			}

			var portalBusMessage = new CacheInvalidationPortalBusMessage { Message = message };

			if (this._syncRemoveEnabled)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Sending messages syncronously");

				var task = PortalBusManager<CacheInvalidationPortalBusMessage>.SendAsync(context, portalBusMessage).WithCurrentCulture();
				task.GetAwaiter().GetResult();
			}
			else
			{
				this.RemoveLocal(message);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Sending messages asyncronously");

				HostingEnvironment.QueueBackgroundWorkItem(ct => PortalBusManager<CacheInvalidationPortalBusMessage>.SendAsync(context, portalBusMessage));
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");
		}

		public override void RemoveLocal(OrganizationServiceCachePluginMessage message)
		{
			message.ThrowOnNull("message");

			base.Remove(message);
		}

		private OrganizationServiceCachePluginMessage GetMessage(string entityLogicalName, Guid id)
		{
			var message = new OrganizationServiceCachePluginMessage
			{
				ConnectionStringName = _connectionStringName,
				ServiceCacheName = _name,
				Target = new PluginMessageEntityReference { LogicalName = entityLogicalName, Id = id }
			};

			return message;
		}
	}
}
