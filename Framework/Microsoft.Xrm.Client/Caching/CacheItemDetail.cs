/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.Caching;
using System.Runtime.Serialization;

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides details of an item in cache.
	/// </summary>
	[Serializable]
	[DataContract(Namespace = V5.Contracts)]
	public sealed class CacheItemDetail
	{
		private static readonly object _cacheItemStatusLock = new object();

		/// <summary>
		/// The key of the related cache item.
		/// </summary>
		[DataMember]
		public string CacheKey { get; private set; }

		/// <summary>
		/// The creation date of the cache item.
		/// </summary>
		[DataMember]
		public DateTimeOffset CreatedOn { get; private set; }

		/// <summary>
		/// The last updated date of the cache item.
		/// </summary>
		[DataMember]
		public DateTimeOffset UpdatedOn { get; set; }

		/// <summary>
		/// The policy of the cache item.
		/// </summary>
		[DataMember]
		public CacheItemPolicyDetail Policy { get; private set; }

		/// <summary>
		/// The status of the cache item.
		/// </summary>
		[DataMember]
		public CacheItemStatus CacheItemStatus { get; private set; }

		/// <summary>
		/// Flag to determine whether or not to keep stale data in cache.
		/// </summary>
		[DataMember]
		public bool IsStaleDataAllowed { get; set; }

		/// <summary>
		/// Session if of the request which marked the cache item dirty.
		/// </summary>
		[DataMember]
		public string SessionId { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheItemDetail"/> class associated with a cache item.
		/// </summary>
		/// <param name="cacheKey">The associated cache item.</param>
		/// <param name="policy">The policy of the cache item.</param>
		public CacheItemDetail(string cacheKey, CacheItemPolicy policy)
		{
			CreatedOn = DateTimeOffset.UtcNow;
			UpdatedOn = CreatedOn;
			CacheKey = cacheKey;
			Policy = new CacheItemPolicyDetail(policy);
		}

		/// <summary>
		/// Updates the cache item status.
		/// </summary>
		/// <param name="status"></param>
		/// <returns>Returns true if the CacheItemStatus was changed.</returns>
		public bool TrySetCacheItemStatus(CacheItemStatus status)
		{
			lock (_cacheItemStatusLock)
			{
				if (this.CacheItemStatus != status)
				{
					this.CacheItemStatus = status;
					this.UpdatedOn = DateTimeOffset.UtcNow;
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Status of the Cached Item
	/// Dirty - This item has stale data.
	/// BeingProcessed - Data is being fetched from CRM.
	/// Current - The data is uptodate with CRM
	/// </summary>
	public enum CacheItemStatus
	{
		Current,
		BeingProcessed,
		Dirty
		
	}
}
