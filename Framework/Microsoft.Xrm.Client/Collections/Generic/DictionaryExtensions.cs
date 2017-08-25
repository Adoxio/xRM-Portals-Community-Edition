/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xrm.Client.Collections.Generic
{
	/// <summary>
	/// Extensions on the dictionary class.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns the first non-null value for a sequence of keys.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static TValue FirstNotNullOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
		{
			return keys.Where(dictionary.ContainsKey).Select(key => dictionary[key]).FirstOrDefault();
		}

		/// <summary>
		/// Returns the first non-null or empty string value for a sequence of keys.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static string FirstNotNullOrEmpty<TKey>(this IDictionary<TKey, string> dictionary, params TKey[] keys)
		{
			return keys.Where(key => dictionary.ContainsKey(key) && !string.IsNullOrEmpty(dictionary[key])).Select(key => dictionary[key]).FirstOrDefault();
		}
	}
}
