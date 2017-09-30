/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Client.Threading
{
	/// <summary>
	/// Helper methods on the <see cref="Mutex"/> class.
	/// </summary>
	internal static class MutexExtensions
	{
		private static readonly string _appDomain = Thread.GetDomain().FriendlyName;

		private static string GetProcessSpecificKey(string key)
		{
			// the Mutex class does not accept keys that contain a backslash character

			const string format = "appDomain={0}:key={1}";
			var specificKey = format.FormatWith(_appDomain, key);
			var hash = Hash(specificKey);

			return hash;
		}

		private static string Hash(string text)
		{
			var bytes = Encoding.Unicode.GetBytes(text);

			using (var sha1 = new SHA1CryptoServiceProvider())
			{
				var hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
				return hash;
			}
		}

		private static int GetDefaultTimeout()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			if (section != null && section.MutexTimeout != null)
			{
				var timeout = (int)section.MutexTimeout.Value.TotalMilliseconds;

				return timeout;
			}

			return Timeout.Infinite;
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T Lock<T>(string key, Func<Mutex, T> action)
		{
			return Lock(key, GetDefaultTimeout(), action);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T Lock<T>(string key, int millisecondsTimeout, Func<Mutex, T> action)
		{
			var result = default(T);
			Lock(key, millisecondsTimeout, mutex => { result = action(mutex); });
			return result;
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="action"></param>
		public static void Lock(string key, Action<Mutex> action)
		{
			Lock(key, GetDefaultTimeout(), action);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		public static void Lock(string key, int millisecondsTimeout, Action<Mutex> action)
		{
			var processSpecificKey = GetProcessSpecificKey(key);

			using (var mutex = new Mutex(false, processSpecificKey))
			{
				try
				{
					if (!mutex.WaitOne(millisecondsTimeout))
					{
						throw new TimeoutException("A timeout occurred while waiting on the named mutex '{0}'.".FormatWith(key));
					}

					action(mutex);
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
		}

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <returns></returns>
		public static T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			return Get(key, GetDefaultTimeout(), loadFromCache, loadFromService);
		}

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <param name="addToCache"></param>
		/// <returns></returns>
		public static T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, GetDefaultTimeout(), loadFromCache, loadFromService, addToCache);
		}

		/// <summary>
		/// Retrieves a resource from a cache or a data service using an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="loadFromCache"></param>
		/// <param name="loadFromService"></param>
		/// <returns></returns>
		public static T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			var obj = loadFromCache(key);

			// first check
			if (obj == null)
			{
				Lock(
					key,
					millisecondsTimeout,
					_ =>
					{
						// second check
						var value = loadFromCache(key) ?? loadFromService(key);
						Thread.MemoryBarrier();
						obj = value;
					});
			}

			return (T)obj;
		}

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
		public static T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, millisecondsTimeout, loadFromCache,
				k =>
				{
					var obj = loadFromService(k);
					addToCache(k, obj);
					return obj;
				});
		}
	}
}
