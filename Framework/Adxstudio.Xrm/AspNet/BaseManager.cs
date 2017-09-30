/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.AspNet
{
	public abstract class BaseManager<TStore> : IDisposable
		where TStore : class, IDisposable
	{
		protected virtual TStore Store { get; private set; }

		protected BaseManager(TStore store)
		{
			if (store == null) throw new ArgumentNullException("store");

			Store = store;
		}

		#region IDisposable

		private bool _disposed;

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				Store.Dispose();
			}

			Store = null;
			_disposed = true;
		}

		protected virtual void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion
	}
}
