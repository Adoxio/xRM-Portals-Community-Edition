/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Security
{
	internal class RequestCachingCrmEntitySecurityProvider : CachingCrmEntitySecurityProvider
	{
		public RequestCachingCrmEntitySecurityProvider(ICacheSupportingCrmEntitySecurityProvider underlyingProvider, ICrmEntitySecurityCacheInfoFactory cacheInfoFactory) : base(underlyingProvider, cacheInfoFactory) { }

		public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			var info = CacheInfoFactory.GetCacheInfo(context, entity, right);

			if (!info.IsCacheable)
			{
				return UnderlyingProvider.TryAssert(context, entity, right, dependencies);
			}

			// No locking is required here because the Items collection is already per-thread/per-request.
			var cachedValue = HttpContext.Current.Items[info.Key];

			if (cachedValue is bool)
			{
				return (bool)cachedValue;
			}

			var value = UnderlyingProvider.TryAssert(context, entity, right, dependencies);

			// We ignore whether the value is cacheable or not and always cache it at this level, as the result
			// will always be the same per-request.
			HttpContext.Current.Items[info.Key] = value;

			return value;
		}
	}
}
