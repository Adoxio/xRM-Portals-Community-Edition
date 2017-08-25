/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Client.Threading
{
	/// <summary>
	/// Methods for processing locked operations.
	/// </summary>
	public interface ILockProvider
	{
		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		T Lock<T>(string key, Func<T> action);

				/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		T Lock<T>(string key, int millisecondsTimeout, Func<T> action);

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="action"></param>
		void Lock(string key, Action action);

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		void Lock(string key, int millisecondsTimeout, Action action);

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <returns></returns>
		T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService);

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <param name="addToCache"></param>
		/// <returns></returns>
		T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache);

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <returns></returns>
		T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService);

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <param name="addToCache"></param>
		/// <returns></returns>
		T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache);
	}
}
