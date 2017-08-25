/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// The caching behavior mode.
	/// </summary>
	public enum OrganizationServiceCacheMode
	{
		/// <summary>
		/// Return the item found in cache and insert new items.
		/// </summary>
		LookupAndInsert,

		/// <summary>
		/// Always load and insert a new item into the cache.
		/// </summary>
		InsertOnly,

		/// <summary>
		/// Always load a new item without modifying the cache.
		/// </summary>
		Disabled
	}

	/// <summary>
	/// The cache retrieval mode.
	/// </summary>
	public enum OrganizationServiceCacheReturnMode
	{
		/// <summary>
		/// Return the same reference retrieved from the cache.
		/// </summary>
		Shared,

		/// <summary>
		/// Return a clone of the item retrieved from the cache.
		/// </summary>
		Cloned
	}

	/// <summary>
	/// Provides caching services for an <see cref="IOrganizationService"/>.
	/// </summary>
	public interface IOrganizationServiceCache
	{
		/// <summary>
		/// The caching behavior mode.
		/// </summary>
		OrganizationServiceCacheMode Mode { get; set; }

		/// <summary>
		/// The cache retrieval mode.
		/// </summary>
		OrganizationServiceCacheReturnMode ReturnMode { get; set; }

		/// <summary>
		/// Executes a request against the <see cref="IOrganizationService"/> or retrieves the response from the cache if found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <param name="execute"></param>
		/// <param name="selector"></param>
		/// <param name="selectorCacheKey"></param>
		/// <returns></returns>
		T Execute<T>(OrganizationRequest request, Func<OrganizationRequest, OrganizationResponse> execute, Func<OrganizationResponse, T> selector, string selectorCacheKey);

		/// <summary>
		/// Inserts a new item into the cache.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="query"></param>
		/// <param name="result"></param>
		void Insert(string key, object query, object result);

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entity"></param>
		void Remove(Entity entity);

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entity"></param>
		void Remove(EntityReference entity);

		/// <summary>
		/// Removes an entity from the cache.
		/// </summary>
		/// <param name="entityLogicalName"></param>
		/// <param name="id"></param>
		void Remove(string entityLogicalName, Guid? id);

		/// <summary>
		/// Removes a request from the cache.
		/// </summary>
		/// <param name="request"></param>
		void Remove(OrganizationRequest request);

		/// <summary>
		/// Removes a specific cache item.
		/// </summary>
		/// <param name="cacheKey"></param>
		void Remove(string cacheKey);

		/// <summary>
		/// Removes cache items based on a message description.
		/// </summary>
		void Remove(OrganizationServiceCachePluginMessage message);
	}
}
