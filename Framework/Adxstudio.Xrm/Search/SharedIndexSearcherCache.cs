/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading;
using Microsoft.Xrm.Client.Diagnostics;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Search
{
	/// <summary>
	/// Provides a thread-safe cache/dictionary for sharing single, named <see cref="SharedIndexSearcher"/> instances
	/// within an application.
	/// </summary>
	public class SharedIndexSearcherCache : IIndexSearcherPool
	{
		private static readonly TimeSpan DefaultCacheLockTimeout = TimeSpan.FromSeconds(30);

		private readonly IDictionary<string, SharedIndexSearcher> _cache = new Dictionary<string, SharedIndexSearcher>();
		private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
		private readonly TimeSpan _cacheLockTimeout;

		public SharedIndexSearcherCache()
		{
			double timeoutSecondsSetting;

			_cacheLockTimeout = double.TryParse(ConfigurationManager.AppSettings[SharedIndexSearcher.LockTimeoutAppSettingName], out timeoutSecondsSetting)
				? TimeSpan.FromSeconds(timeoutSecondsSetting)
				: DefaultCacheLockTimeout;
		}

		public ICrmEntityIndexSearcher Get(string name, Func<ICrmEntityIndexSearcher> searcherFactory)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (searcherFactory == null) throw new ArgumentNullException("searcherFactory");

			// If we don't get a read lock quickly, just fail fast to constructing a new index searcher.
			if (!_cacheLock.TryEnterReadLock(TimeSpan.FromSeconds(1)))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Failed to acquire read lock on shared index searcher. Returning a new single-use index searcher.");

				return searcherFactory();
			}

			try
			{
				SharedIndexSearcher searcher;

				if (_cache.TryGetValue(name, out searcher))
				{
					return searcher;
				}
			}
			finally
			{
				_cacheLock.ExitReadLock();
			}

			// If we failed to get a cached searcher, construct a new one and insert it into the cache.

			// If we don't get a write lock quickly, just fail fast to constructing a new index searcher.
			if (!_cacheLock.TryEnterWriteLock(TimeSpan.FromSeconds(1)))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Failed to acquire write lock on shared index searcher cache. Returning a new single-use index searcher.");

				return searcherFactory();
			}

			try
			{
				SharedIndexSearcher searcher;

				if (_cache.TryGetValue(name, out searcher))
				{
					return searcher;
				}

				searcher = new SharedIndexSearcher(searcherFactory);

				_cache.Add(name, searcher);

				return searcher;
			}
			finally
			{
				_cacheLock.ExitWriteLock();
			}
		}

		public void Refresh(string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			if (!_cacheLock.TryEnterReadLock(_cacheLockTimeout))
			{
				throw new TimeoutException("A read lock couldn't be acquired on the shared index searcher cache.");
			}

			try
			{
				SharedIndexSearcher searcher;

				if (_cache.TryGetValue(name, out searcher))
				{
					searcher.Refresh();
				}
			}
			finally
			{
				_cacheLock.ExitReadLock();
			}
		}
	}
}
