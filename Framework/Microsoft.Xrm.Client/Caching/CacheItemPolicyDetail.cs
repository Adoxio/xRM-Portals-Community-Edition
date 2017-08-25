/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides details of an item policy in cache.
	/// </summary>
	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public sealed class CacheItemPolicyDetail
	{
		[DataMember]
		public DateTimeOffset AbsoluteExpiration { get; private set; }

		[DataMember]
		public TimeSpan SlidingExpiration { get; private set; }

		[DataMember]
		public CacheItemPriority Priority { get; private set; }

		[DataMember]
		public ICollection<ChangeMonitorDetail> ChangeMonitors { get; private set; }

		public CacheItemPolicyDetail(CacheItemPolicy policy)
		{
			AbsoluteExpiration = policy.AbsoluteExpiration;
			SlidingExpiration = policy.SlidingExpiration;
			Priority = policy.Priority;
			ChangeMonitors = policy.ChangeMonitors
				.Where(cm => cm is CacheEntryChangeMonitor)
				.Select(cm => new ChangeMonitorDetail(cm as CacheEntryChangeMonitor))
				.ToList();
		}
	}
}
