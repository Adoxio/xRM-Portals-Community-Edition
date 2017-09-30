/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using Microsoft.Xrm.Client.Diagnostics;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Search
{
	/// <summary>
	/// Provides a wrapper around a <see cref="ICrmEntityIndexSearcher"/> factory, maintaining a single instance
	/// at a time, with reader/writer locking to allow refreshing (re-initialization) of the searcher in a
	/// thread-safe manner.
	/// </summary>
	public class SharedIndexSearcher : ICrmEntityIndexSearcher
	{
		public const string LockTimeoutAppSettingName = "Adxstudio.Xrm.Search.SharedIndexSearcher.LockTimeout";

		private static readonly TimeSpan DefaultSearcherLockTimeout = TimeSpan.FromSeconds(30);

		private ICrmEntityIndexSearcher _searcher;
		private readonly Func<ICrmEntityIndexSearcher> _searcherFactory;
		private readonly ReaderWriterLockSlim _searcherLock = new ReaderWriterLockSlim();
		private readonly TimeSpan _searcherLockTimeout;

		public SharedIndexSearcher(Func<ICrmEntityIndexSearcher> searcherFactory)
		{
			if (searcherFactory == null) throw new ArgumentNullException("searcherFactory");

			_searcherFactory = searcherFactory;
			_searcher = _searcherFactory();

			double timeoutSecondsSetting;

			_searcherLockTimeout = double.TryParse(ConfigurationManager.AppSettings[LockTimeoutAppSettingName], out timeoutSecondsSetting)
				? TimeSpan.FromSeconds(timeoutSecondsSetting)
				: DefaultSearcherLockTimeout;
		}

		public void Dispose() { }

		public void Refresh()
		{
			// If there are already waiting writes/refreshes, just bail out, as there's no point in queuing another.
			if (_searcherLock.WaitingWriteCount > 0)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "There are existing waiting writes to the shared index searcher, exiting.");

				return;
			}

			if (!_searcherLock.TryEnterWriteLock(_searcherLockTimeout))
			{
				throw new TimeoutException("A write lock couldn't be acquired on the shared index searcher instance.");
			}

			var staleSearcher = _searcher;

			try
			{
				_searcher = _searcherFactory();
			}
			finally
			{
				_searcherLock.ExitWriteLock();
			}

			staleSearcher.Dispose();
		}

		public ICrmEntitySearchResultPage Search(ICrmEntityQuery query)
		{
			// If we don't get a read lock quickly, just fail fast to constructing and using a new index searcher.
			if (!_searcherLock.TryEnterReadLock(TimeSpan.FromSeconds(1)))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Failed to acquire read lock on shared index searcher. Creating new single-use index searcher.");

				using (var searcher = _searcherFactory())
				{
					return searcher.Search(query);
				}
			}

			try
			{
				return _searcher.Search(query);
			}
			finally
			{
				_searcherLock.ExitReadLock();
			}
		}
	}
}
