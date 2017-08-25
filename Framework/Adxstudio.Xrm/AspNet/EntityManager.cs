/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;

namespace Adxstudio.Xrm.AspNet
{
	public class EntityManager<TStore, TModel, TKey> : BaseManager<TStore>
		where TStore : class, IEntityStore<TModel, TKey>
		where TModel : IModel<TKey>
		where TKey : IEquatable<TKey>
	{
		public EntityManager(TStore store)
			: base(store)
		{
		}

		public virtual async Task CreateAsync(TModel model)
		{
			ThrowIfDisposed();

			model.Id = await Store.CreateAsync(model).WithCurrentCulture();
		}

		public virtual async Task UpdateAsync(TModel model)
		{
			ThrowIfDisposed();

			await Store.UpdateAsync(model).WithCurrentCulture();
		}

		public virtual async Task DeleteAsync(TModel model)
		{
			ThrowIfDisposed();

			await Store.DeleteAsync(model).WithCurrentCulture();
		}

		public virtual async Task<TModel> FindByIdAsync(TKey id)
		{
			ThrowIfDisposed();

			var model = await Store.FindByIdAsync(id).WithCurrentCulture();

			return model;
		}

		public virtual async Task<TModel> FindByNameAsync(string name)
		{
			ThrowIfDisposed();

			var model = await Store.FindByNameAsync(name).WithCurrentCulture();

			return model;
		}

		protected virtual Guid ToGuid(TKey key)
		{
			return key.ToGuid();
		}

		protected virtual TKey ToKey(Guid guid)
		{
			return guid.ToKey<TKey>();
		}
	}
}
