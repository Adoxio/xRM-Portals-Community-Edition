/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Xrm.Client.Services;

namespace Adxstudio.Xrm.Caching
{
	/// <summary>
	/// Custom service bus behavior for the <see cref="OrganizationServiceCacheServiceBusServiceHost"/>.
	/// </summary>
	internal sealed class OrganizationServiceCacheServiceBehavior : IServiceBehavior
	{
		private readonly IOrganizationServiceCache _cache;

		public OrganizationServiceCacheServiceBehavior(IOrganizationServiceCache cache)
		{
			_cache = cache;
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			serviceDescription.ThrowOnNull("serviceDescription");
			serviceHostBase.ThrowOnNull("serviceHostBase");

			var sb = serviceDescription.Behaviors.Find<ServiceBehaviorAttribute>();

			foreach (var ed in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>().SelectMany(cd => cd.Endpoints))
			{
				if (sb != null && sb.InstanceContextMode == InstanceContextMode.Single)
				{
					ed.DispatchRuntime.SingletonInstanceContext = new InstanceContext(serviceHostBase);
				}

				ed.DispatchRuntime.InstanceProvider = new OrganizationServiceCacheServiceInstanceProvider(_cache);
			}
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
			// Nothing to do.
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			// Nothing to do.
		}
	}
}
