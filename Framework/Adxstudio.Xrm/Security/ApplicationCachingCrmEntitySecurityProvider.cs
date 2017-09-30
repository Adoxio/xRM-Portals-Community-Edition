/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Security
{
	using System.Diagnostics;
	using Caching;
	using Microsoft.Xrm.Client.Caching;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk.Client;
	using Services;

	internal class ApplicationCachingCrmEntitySecurityProvider : CachingCrmEntitySecurityProvider
	{
		public ApplicationCachingCrmEntitySecurityProvider(
			ICacheSupportingCrmEntitySecurityProvider underlyingProvider,
			ICrmEntitySecurityCacheInfoFactory cacheInfoFactory)
			: base(underlyingProvider, cacheInfoFactory) { }

		public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			var info = CacheInfoFactory.GetCacheInfo(context, entity, right);

			if (!info.IsCacheable)
			{
				return UnderlyingProvider.TryAssert(context, entity, right, dependencies);
			}

			Stopwatch stopwatch = null;

			return ObjectCacheManager.Get(info.Key,
				cache =>
				{
					stopwatch = Stopwatch.StartNew();

					var value = UnderlyingProvider.TryAssert(context, entity, right, dependencies);

					stopwatch.Stop();

					return value;
				},
				(cache, value) =>
				{
					if (dependencies.IsCacheable)
					{
						cache.Insert(info.Key, value, dependencies);

						if (stopwatch != null)
						{
							cache.AddCacheItemTelemetry(info.Key, new CacheItemTelemetry { Duration = stopwatch.Elapsed });
						}
					}
				});
		}
	}
}
