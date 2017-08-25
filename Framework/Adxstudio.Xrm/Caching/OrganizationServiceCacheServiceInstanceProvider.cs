/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Xrm.Client.Services;

namespace Adxstudio.Xrm.Caching
{
	/// <summary>
	/// Custom service bus behavior for the <see cref="OrganizationServiceCacheServiceBusServiceHost"/>.
	/// </summary>
	internal sealed class OrganizationServiceCacheServiceInstanceProvider : IInstanceProvider
	{
		private readonly IOrganizationServiceCache _cache;

		public OrganizationServiceCacheServiceInstanceProvider(IOrganizationServiceCache cache)
		{
			_cache = cache;
		}

		public object GetInstance(InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return new OrganizationServiceCacheService(_cache);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}
	}
}
