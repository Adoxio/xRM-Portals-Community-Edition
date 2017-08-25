/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Client.Caching
{
	/// <summary>
	/// Provides extended operations to cache management.
	/// </summary>
	public interface IExtendedObjectCache
	{
		/// <summary>
		/// Removes all cache items.
		/// </summary>
		/// <param name="regionName"></param>
		void RemoveAll(string regionName);

		/// <summary>
		/// Removes a cache item that is local to the application.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="regionName"></param>
		/// <returns></returns>
		object RemoveLocal(string key, string regionName);

		/// <summary>
		/// Removes all cache items that are local to the application.
		/// </summary>
		/// <param name="regionName"></param>
		void RemoveAllLocal(string regionName);
	}
}
