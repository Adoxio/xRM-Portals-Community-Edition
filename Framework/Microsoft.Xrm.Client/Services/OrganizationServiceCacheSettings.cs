/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Caching;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// Settings for defining caching behavior.
	/// </summary>
	public sealed class OrganizationServiceCacheSettings
	{
		/// <summary>
		/// A key value for uniquely distinguishing the connection.
		/// </summary>
		public string ConnectionId { get; set; }

		/// <summary>
		/// The cache region name.
		/// </summary>
		public string CacheRegionName { get; set; }

		/// <summary>
		/// Indicates that the query used to construct the cache key is to be hashed or left as a readable string.
		/// </summary>
		public bool QueryHashingEnabled { get; set; }

		/// <summary>
		/// The prefix string used for constructing the <see cref="CacheEntryChangeMonitor"/> objects assigned to the cache items.
		/// </summary>
		public string CacheEntryChangeMonitorPrefix { get; set; }

		/// <summary>
		/// A factory for creating <see cref="CacheItemPolicy"/> objects.
		/// </summary>
		public ICacheItemPolicyFactory PolicyFactory { get; set; }

		/// <summary>
		/// Initializes with default settings.
		/// </summary>
		public OrganizationServiceCacheSettings()
			: this((string)null)
		{
		}

		/// <summary>
		/// Initializes with default settings.
		/// </summary>
		/// <param name="connection"></param>
		public OrganizationServiceCacheSettings(CrmConnection connection)
			: this(connection.GetConnectionId())
		{
		}

		/// <summary>
		/// Initializes with default settings.
		/// </summary>
		/// <param name="connectionId"></param>
		public OrganizationServiceCacheSettings(string connectionId)
		{
			ConnectionId = connectionId;
			QueryHashingEnabled = true;
			CacheEntryChangeMonitorPrefix = "xrm:dependency";
		}
	}
}
