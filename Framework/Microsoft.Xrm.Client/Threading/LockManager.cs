/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using Microsoft.Xrm.Client.Configuration;

namespace Microsoft.Xrm.Client.Threading
{
	/// <summary>
	/// Provides exclusive lock helpers.
	/// </summary>
	public static class LockManager
	{
		private static Lazy<ILockProvider> _provider = new Lazy<ILockProvider>(CreateProvider);

		private static ILockProvider CreateProvider()
		{
			var section = CrmConfigurationManager.GetCrmSection();

			if (section != null && !string.IsNullOrWhiteSpace(section.LockProviderType))
			{
				var typeName = section.LockProviderType;
				var type = TypeExtensions.GetType(typeName);

				if (type == null || !type.IsA<ILockProvider>())
				{
					throw new ConfigurationErrorsException("The value '{0}' is not recognized as a valid type or is not of the type '{1}'.".FormatWith(typeName, typeof(ILockProvider)));
				}

				return Activator.CreateInstance(type) as ILockProvider;
			}

			return new MutexLockProvider();
		}

		/// <summary>
		/// Resets the cached dependencies.
		/// </summary>
		public static void Reset()
		{
			_provider = new Lazy<ILockProvider>(CreateProvider);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T Lock<T>(string key, Func<T> action)
		{
			return _provider.Value.Lock(key, action);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T Lock<T>(string key, int millisecondsTimeout, Func<T> action)
		{
			return _provider.Value.Lock(key, millisecondsTimeout, action);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="action"></param>
		public static void Lock(string key, Action action)
		{
			_provider.Value.Lock(key, action);
		}

		/// <summary>
		/// Performs an action under an exclusive lock.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="action"></param>
		public static void Lock(string key, int millisecondsTimeout, Action action)
		{
			_provider.Value.Lock(key, millisecondsTimeout, action);
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
			return _provider.Value.Get(key, loadFromCache, loadFromService);
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
			return _provider.Value.Get(key, loadFromCache, loadFromService, addToCache);
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
			return _provider.Value.Get(key, millisecondsTimeout, loadFromCache, loadFromService);
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
			return _provider.Value.Get(key, millisecondsTimeout, loadFromCache, loadFromService, addToCache);
		}
	}
}
