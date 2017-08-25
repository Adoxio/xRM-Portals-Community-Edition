/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Security
{
	internal abstract class CachingCrmEntitySecurityProvider : CacheSupportingCrmEntitySecurityProvider
	{
		protected CachingCrmEntitySecurityProvider(ICacheSupportingCrmEntitySecurityProvider underlyingProvider, ICrmEntitySecurityCacheInfoFactory cacheInfoFactory)
		{
			if (underlyingProvider == null)
			{
				throw new ArgumentNullException("underlyingProvider");
			}

			if (cacheInfoFactory == null)
			{
				throw new ArgumentNullException("cacheInfoFactory");
			}

			UnderlyingProvider = underlyingProvider;
			CacheInfoFactory = cacheInfoFactory;
		}

		protected ICrmEntitySecurityCacheInfoFactory CacheInfoFactory { get; private set; }

		protected ICacheSupportingCrmEntitySecurityProvider UnderlyingProvider { get; private set; }
	}
}
