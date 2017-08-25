/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Runtime.Caching;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// Represents a class capable of building a cache policy from cache settings.
	/// </summary>
	public interface ICacheItemPolicyFactory
	{
		/// <summary>
		/// Creates a cache item policy.
		/// </summary>
		/// <returns></returns>
		CacheItemPolicy Create();
	}
}
