/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Client.Threading
{
	/// <summary>
	/// Base implementation of a lock provider.
	/// </summary>
	public abstract class LockProvider : ILockProvider
	{
		public virtual T Lock<T>(string key, Func<T> action)
		{
			return Lock(key, GetDefaultTimeout(), action);
		}

		public virtual T Lock<T>(string key, int millisecondsTimeout, Func<T> action)
		{
			var result = default(T);
			Lock(key, millisecondsTimeout, () => { result = action(); });
			return result;
		}

		public virtual void Lock(string key, Action action)
		{
			Lock(key, GetDefaultTimeout(), action);
		}

		public abstract void Lock(string key, int millisecondsTimeout, Action action);

		public virtual T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			return Get(key, GetDefaultTimeout(), loadFromCache, loadFromService);
		}

		public virtual T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, GetDefaultTimeout(), loadFromCache, loadFromService, addToCache);
		}

		public virtual T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			var obj = loadFromCache(key);

			// first check
			if (obj == null)
			{
				Lock(
					key,
					millisecondsTimeout,
					() =>
					{
						// second check
						var value = loadFromCache(key) ?? loadFromService(key);
						Thread.MemoryBarrier();
						obj = value;
					});
			}

			return (T)obj;
		}

		public virtual T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, millisecondsTimeout, loadFromCache,
				k =>
				{
					var obj = loadFromService(k);
					addToCache(k, obj);
					return obj;
				});
		}

		protected virtual int GetDefaultTimeout()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			if (section != null && section.MutexTimeout != null)
			{
				var timeout = (int)section.MutexTimeout.Value.TotalMilliseconds;

				return timeout;
			}

			return Timeout.Infinite;
		}
	}
}
