/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides details of an item policy change monitor in cache.
	/// </summary>
	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public sealed class ChangeMonitorDetail
	{
		[DataMember]
		public ICollection<string> CacheKeys { get; private set; }

		[DataMember]
		public DateTimeOffset LastModified { get; private set; }

		[DataMember]
		public string RegionName { get; private set; }

		public ChangeMonitorDetail(CacheEntryChangeMonitor changeMonitor)
		{
			RegionName = changeMonitor.RegionName;
			LastModified = changeMonitor.LastModified;
			CacheKeys = changeMonitor.CacheKeys;
		}
	}
}
