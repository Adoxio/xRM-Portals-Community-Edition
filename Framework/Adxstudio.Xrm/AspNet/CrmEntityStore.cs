/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.AspNet
{
	public interface IEntityStore<TModel, TKey> : IDisposable
	{
		Task<TKey> CreateAsync(TModel model);
		Task UpdateAsync(TModel model);
		Task DeleteAsync(TModel model);
		Task<TModel> FindByIdAsync(TKey id);
		Task<TModel> FindByNameAsync(string name);
	}

	public class CrmEntityStoreSettings
	{
		public virtual bool DeleteByStatusCode { get; set; }
		public virtual PortalSolutions PortalSolutions { get; set; }
	}

	public abstract class CrmEntityStore<TModel, TKey> : BaseStore
		where TModel : CrmModel<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected virtual CrmEntityStoreSettings Settings { get; private set; }
		protected virtual string LogicalName { get; private set; }
		protected virtual string PrimaryIdAttribute { get; private set; }
		protected virtual string PrimaryNameAttribute { get; private set; }
		protected virtual Version BaseSolutionCrmVersion { get; private set; }

		protected CrmEntityStore(string logicalName, string primaryIdAttribute, string primaryNameAttribute, CrmDbContext context, CrmEntityStoreSettings settings)
			: base(context)
		{
			if (string.IsNullOrWhiteSpace(logicalName)) throw new ArgumentNullException("logicalName");
			if (string.IsNullOrWhiteSpace(primaryIdAttribute)) throw new ArgumentNullException("primaryIdAttribute");
			if (string.IsNullOrWhiteSpace(primaryNameAttribute)) throw new ArgumentNullException("primaryNameAttribute");
			if (context == null) throw new ArgumentNullException("context");

			LogicalName = logicalName;
			PrimaryIdAttribute = primaryIdAttribute;
			PrimaryNameAttribute = primaryNameAttribute;
			Settings = settings;

			BaseSolutionCrmVersion = settings.PortalSolutions != null
				? settings.PortalSolutions.BaseSolutionCrmVersion
				: null;
		}

		#region IEntityStore

		public virtual Task<TKey> CreateAsync(TModel model)
		{
			ThrowIfDisposed();

			if (model == null) throw new ArgumentNullException("model");

			var entity = ToEntity(model);

			var id = Context.Service.Create(entity);

			return Task.FromResult(ToKey(id));
		}

		public virtual async Task UpdateAsync(TModel model)
		{
			ThrowIfDisposed();

			if (model == null) throw new ArgumentNullException("model");

			var entity = ToEntity(model);
			var snapshot = await FetchByIdAsync(entity.ToEntityReference()).WithCurrentCulture();

			Execute(ToUpdateRequests(entity, snapshot));
		}

		public virtual Task DeleteAsync(TModel model)
		{
			ThrowIfDisposed();

			if (model == null) throw new ArgumentNullException("model");

			Execute(ToDeleteRequests(model));

			return Task.FromResult(model);
		}

		public virtual Task<TModel> FindByIdAsync(TKey id)
		{
			ThrowIfDisposed();

			if (ToGuid(id) == Guid.Empty) throw new ArgumentException("Invalid ID.");

			return FindByConditionAsync(new Condition(PrimaryIdAttribute, ConditionOperator.Equal, id));
		}

		public virtual Task<TModel> FindByNameAsync(string name)
		{
			ThrowIfDisposed();

			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid name.");

			return FindByConditionAsync(new Condition(PrimaryNameAttribute, ConditionOperator.Equal, name));
		}

		protected abstract RetrieveRequest ToRetrieveRequest(EntityReference id);

		protected virtual Entity ToEntity(TModel model)
		{
			return model.Entity;
		}

		protected virtual TModel ToModel(Entity entity)
		{
			if (entity == null) return null;

			var model = new TModel();
			model.SetEntity(entity);

			return model;
		}

		protected virtual IEnumerable<OrganizationRequest> ToUpdateRequests(Entity entity, Entity snapshot)
		{
			var changedEntity = entity.ToChangedEntity(snapshot);
			var removedEntities = entity.ToRemovedEntities(snapshot);

			// update the local identity entity graph

			if (changedEntity != null)
			{
				yield return new UpdateRequest { Target = changedEntity };
			}

			// delete removed child entities

			foreach (var removedEntity in removedEntities)
			{
				yield return new DeleteRequest { Target = removedEntity.ToEntityReference() };
			}
		}

		protected virtual IEnumerable<OrganizationRequest> ToDeleteRequests(TModel model)
		{
			if (Settings.DeleteByStatusCode)
			{
				yield return GetDeactivateRequest(new EntityReference(LogicalName, ToGuid(model.Id)));
			}
			else
			{
				yield return new DeleteRequest { Target = new EntityReference(LogicalName, ToGuid(model.Id)) };
			}
		}

		protected virtual async Task<TModel> FindByConditionAsync(Condition condition)
		{
			var entity = await FetchByConditionAsync(condition).WithCurrentCulture();
			return ToModel(entity);
		}

		protected virtual Task<Entity> FetchByConditionAsync(params Condition[] conditions)
		{
			// fetch the local identity by a condition

			var fetch = new Fetch
			{
				Entity = new FetchEntity(LogicalName)
				{
					Attributes = FetchAttribute.None,
					Filters = new[] { new Filter {
						Conditions = GetActiveEntityConditions().Concat(conditions).ToArray()
					} }
				}
			};

			return FetchAsync(fetch);
		}

		protected virtual async Task<TModel> FindByIdAsync(EntityReference id)
		{
			var entity = await FetchByIdAsync(id).WithCurrentCulture();
			return ToModel(entity);
		}

		protected virtual Task<Entity> FetchByIdAsync(EntityReference id)
		{
			var request = this.ToRetrieveRequest(id);
			var response = this.Context.Service.Execute(request) as RetrieveResponse;
			return Task.FromResult(response.Entity);
		}

		protected virtual Guid ToGuid(TKey key)
		{
			return ToGuid<TKey>(key);
		}

		protected virtual TKey ToKey(Guid guid)
		{
			return ToKey<TKey>(guid);
		}

		protected virtual async Task<Entity> FetchAsync(Fetch fetch)
		{
			// fetch a lightweight entity

			var entity = await FetchSingleOrDefaultAsync(fetch).WithCurrentCulture();

			// expand to the full entity by ID

			return entity != null
				? await FetchByIdAsync(entity.ToEntityReference()).WithCurrentCulture()
				: null;
		}

		protected virtual OrganizationRequest GetDeactivateRequest(EntityReference entity)
		{
			return new OrganizationRequest("SetState")
			{
				Parameters = new ParameterCollection
				{
					{ "EntityMoniker", entity },
					{ "State", new OptionSetValue(1) },
					{ "Status", new OptionSetValue(2) },
				}
			};
		}

		protected virtual IEnumerable<Condition> GetActiveEntityConditions()
		{
			return GetActiveStateConditions();
		}

		protected virtual IEnumerable<Condition> GetActiveStateConditions()
		{
			yield return EntityExtensions.ActiveStateCondition;
		}

		protected void MergeRelatedEntities(ICollection<Entity> parents, Relationship relationship, string relatedLogicalName, ColumnSet relatedColumns)
		{
			if (parents == null || parents.Count == 0)
			{
				return;
			}

			var parentIds = parents.Select(parent => parent.Id).Cast<object>().ToList();
			var parentIdsCondition = new[] { new Condition { Attribute = PrimaryIdAttribute, Operator = ConditionOperator.In, Values = parentIds } };

			var fetchRelatedEntities = new Fetch
			{
				Entity = new FetchEntity(relatedLogicalName, relatedColumns.Columns)
				{
					Filters = new[] { new Filter {
						Conditions = GetActiveStateConditions().Concat(parentIdsCondition).ToList()
					} }
				}
			};

			var relatedEntities = Context.Service.RetrieveMultiple(fetchRelatedEntities);

			foreach (var parent in parents)
			{
				var parentId = parent.ToEntityReference();
				var relatedSubset = relatedEntities.Entities.Where(binding => Equals(binding.GetAttributeValue<EntityReference>(PrimaryIdAttribute), parentId));
				parent.RelatedEntities[relationship] = new EntityCollection(relatedSubset.ToList());
			}
		}

		private void Execute(IEnumerable<OrganizationRequest> requests)
		{
			// the current OrganizationServiceCache implementation does not support ExecuteMultiple

			//Context.Service.ExecuteMultiple(requests);

			foreach (var request in requests)
			{
				Context.Service.Execute(request);
			}
		}

		#endregion
	}
}
