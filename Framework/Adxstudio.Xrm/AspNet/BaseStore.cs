/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.AspNet
{
	public abstract class BaseStore : IDisposable
	{
		public CrmDbContext Context { get; private set; }
		public bool DisposeContext { get; set; }

		protected BaseStore(CrmDbContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			Context = context;
		}

		#region IDisposable

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (DisposeContext && disposing)
			{
				Context.Dispose();
			}

			Context = null;
			_disposed = true;
		}

		protected void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion

		protected virtual Guid ToGuid<TKey>(TKey key)
		{
			return key.ToGuid();
		}

		protected virtual TKey ToKey<TKey>(Guid guid)
		{
			return guid.ToKey<TKey>();
		}

		protected virtual Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
		{
			var response = Context.Service.Execute(request);
			return Task.FromResult(response);
		}

		protected virtual async Task<Entity> FetchSingleOrDefaultAsync(Fetch fetch)
		{
			var request = new RetrieveSingleRequest(fetch) { EnforceSingle = true };
			var response = await ExecuteAsync(request).WithCurrentCulture() as RetrieveSingleResponse;
			return response.Entity;
		}

		protected virtual async Task<Entity> FetchFirstOrDefaultAsync(Fetch fetch)
		{
			var request = new RetrieveSingleRequest(fetch) { EnforceSingle = false };
			var response = await ExecuteAsync(request).WithCurrentCulture() as RetrieveSingleResponse;
			return response.Entity;
		}
	}
}
