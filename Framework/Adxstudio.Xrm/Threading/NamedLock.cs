/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Threading
{
	/// <summary>
	/// Provides locking against a string name.
	/// </summary>
	internal class NamedLock
	{
		private readonly Dictionary<string, ReferenceCount> _locks = new Dictionary<string, ReferenceCount>();

		/// <summary>
		/// Acquires an exclusive lock on the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IDisposable Lock(string name)
		{
			return Lock(name, Timeout.Infinite);
		}

		/// <summary>
		/// Acquires an exclusive lock on the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <returns></returns>
		public IDisposable Lock(string name, int millisecondsTimeout)
		{
			Monitor.Enter(_locks);

			ReferenceCount obj;

			_locks.TryGetValue(name, out obj);

			if (obj == null)
			{
				obj = new ReferenceCount();

				Monitor.Enter(obj);

				_locks.Add(name, obj);

				Monitor.Exit(_locks);
			}
			else
			{
				obj.AddRef();

				Monitor.Exit(_locks);

				if (!Monitor.TryEnter(obj, millisecondsTimeout))
				{
					throw new TimeoutException(string.Format("A timeout occurred while waiting on the named lock {0}.", name));
				}
			}

			return new NamedLockHandle(this, name);
		}

		/// <summary>
		/// Releases an exclusive lock on the specified name.
		/// </summary>
		/// <param name="name"></param>
		public void Unlock(string name)
		{
			lock (_locks)
			{
				ReferenceCount obj;

				_locks.TryGetValue(name, out obj);

				if (obj != null)
				{
					Monitor.Exit(obj);

					if (0 == obj.Release())
					{
						_locks.Remove(name);
					}
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
		public T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			return Get(key, Timeout.Infinite, loadFromCache, loadFromService);
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
		public T Get<T>(string key, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, Timeout.Infinite, loadFromCache, loadFromService, addToCache);
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
		public T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService)
		{
			var obj = loadFromCache(key);

			// first check
			if (obj == null)
			{
				using (Lock(key, millisecondsTimeout))
				{
					// second check
					obj = loadFromCache(key) ?? loadFromService(key);
				}
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
		public T Get<T>(string key, int millisecondsTimeout, Func<string, object> loadFromCache, Func<string, T> loadFromService, Action<string, T> addToCache)
		{
			return Get(key, millisecondsTimeout, loadFromCache,
				k =>
				{
					var obj = loadFromService(k);
					addToCache(k, obj);
					return obj;
				});
		}

		private class NamedLockHandle : IDisposable
		{
			private readonly NamedLock _parent;
			private readonly string _name;

			public NamedLockHandle(NamedLock parent, string name)
			{
				_parent = parent;
				_name = name;
			}

			#region IDisposable Members

			public void Dispose()
			{
				_parent.Unlock(_name);
			}

			#endregion
		}

		private class ReferenceCount
		{
			private int _count;

			public ReferenceCount()
			{
				AddRef();
			}

			public int AddRef()
			{
				return Interlocked.Increment(ref _count);
			}

			public int Release()
			{
				return Interlocked.Decrement(ref _count);
			}
		}
	}
}
