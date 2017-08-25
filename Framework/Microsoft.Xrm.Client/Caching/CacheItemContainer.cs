/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Wrapper class for the cache value and the cache item detail.
	/// </summary>
	public class CacheItemContainer
	{
		/// <summary>
		/// The CacheItemDetail.
		/// </summary>
		public CacheItemDetail Detail { get; set; }

		/// <summary>
		/// The CacheItemValue.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Intializes a new instance of <see cref="CacheItemContainer"/>>
		/// </summary>
		/// <param name="value">The cache value.</param>
		/// <param name="detail">The cache item detail.</param>
		public CacheItemContainer(object value, CacheItemDetail detail = null)
		{
			Value = value;
			Detail = detail;
		}
	}
}
