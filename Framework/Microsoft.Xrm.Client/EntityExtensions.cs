/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Client.Reflection;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Client.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Helper methods on the <see cref="Entity"/> class.
	/// </summary>
	public static class EntityExtensions
	{
		/// <summary>
		/// Verifies that the LogicalName of an entity is the expected name.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="expectedEntityName"></param>
		public static void AssertEntityName(this Entity entity, params string[] expectedEntityName)
		{
			// accept null values

			if (entity == null) return;

			var entityName = entity.LogicalName;

			if (!expectedEntityName.Contains(entityName))
			{
				throw new ArgumentException(
					"The extension method expected an entity object of the type {0} but was passed an entity object of the type {1} instead.".FormatWith(
						string.Join(" or ", expectedEntityName),
						entityName));
			}
		}

		/// <summary>
		/// Deep clones an <see cref="Entity"/>.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="includeRelatedEntities"></param>
		/// <returns></returns>
		public static Entity Clone(this Entity entity, bool includeRelatedEntities = true)
		{
			var clone = Activator.CreateInstance(entity.GetType()) as Entity;

			return Clone(entity, clone, includeRelatedEntities, new List<Tuple<Entity, Entity>> { new Tuple<Entity, Entity>(entity, clone) });
		}

		private static Entity Clone(Entity entity, Entity clone, bool includeRelatedEntities, IEnumerable<Tuple<Entity, Entity>> path)
		{
			clone.LogicalName = entity.LogicalName;
			clone.Id = entity.Id;
			clone.EntityState = entity.EntityState;

			foreach (var value in entity.FormattedValues)
			{
				clone.FormattedValues[value.Key] = value.Value;
			}

			foreach (var attribute in entity.Attributes)
			{
				var attribClone = CloneAttribute(attribute.Value, includeRelatedEntities);
				clone.Attributes[attribute.Key] = attribClone;
			}

			if (includeRelatedEntities)
			{
				foreach (var related in entity.RelatedEntities)
				{
					var relatedClone = CloneEntityCollection(related.Value, true, path);
					clone.RelatedEntities[related.Key] = relatedClone;
				}
			}

			return clone;
		}

		private static object CloneAttribute(object attribute, bool includeRelatedEntities)
		{
			var reference = attribute as EntityReference;
			if (reference != null) return new EntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };

			var option = attribute as OptionSetValue;
			if (option != null) return new OptionSetValue(option.Value);

			var money = attribute as Money;
			if (money != null) return new Money(money.Value);

			var ecollection = attribute as EntityCollection;
			if (ecollection != null) return CloneEntityCollection(ecollection, includeRelatedEntities, new List<Tuple<Entity, Entity>>());

			return attribute;
		}

		/// <summary>
		/// Deep clones an <see cref="EntityCollection"/>.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="includeRelatedEntities"></param>
		/// <returns></returns>
		public static EntityCollection Clone(this EntityCollection entities, bool includeRelatedEntities = true)
		{
			return CloneEntityCollection(entities, includeRelatedEntities, new List<Tuple<Entity, Entity>>());
		}

		private static EntityCollection CloneEntityCollection(EntityCollection entities, bool includeRelatedEntities, IEnumerable<Tuple<Entity, Entity>> path)
		{
			var clones = new EntityCollection
			{
				EntityName = entities.EntityName,
				MinActiveRowVersion = entities.MinActiveRowVersion,
				MoreRecords = entities.MoreRecords,
				PagingCookie = entities.PagingCookie,
				TotalRecordCount = entities.TotalRecordCount,
				TotalRecordCountLimitExceeded = entities.TotalRecordCountLimitExceeded,
			};

			foreach (var entity in entities.Entities)
			{
				// cycle detection

				// resolve access to modified closure
				var e = entity;
				var duplicate = path.FirstOrDefault(p => p.Item1 == e);

				if (duplicate != null)
				{
					// found a cycle, link the parent to the matched entity

					clones.Entities.Add(duplicate.Item2);
				}
				else
				{
					// not a cycle, continue the recursion

					var clone = Activator.CreateInstance(entity.GetType()) as Entity;

					var entityClone = Clone(entity, clone, includeRelatedEntities, path.Concat(new List<Tuple<Entity, Entity>> { new Tuple<Entity, Entity>(entity, clone) }));
					clones.Entities.Add(entityClone);
				}
			}

			return clones;
		}

		#region Accessors

		/// <summary>
		/// Retrieves the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public static T GetAttributeValue<T>(this Entity entity, string attributeLogicalName)
		{
			var raw = entity.GetAttributeValue(attributeLogicalName);
			var value = GetPrimitiveValue<T>(raw);
			return value != null ? (T)value : default(T);
		}

		/// <summary>
		/// Retrieves the value of a sequence attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetAttributeCollectionValue<T>(this Entity entity, string attributeLogicalName)
			where T : Entity
		{
			var collection = entity.GetAttributeValue(attributeLogicalName) as EntityCollection;

			if (collection != null && collection.Entities != null)
			{
				return collection.Entities.Cast<T>();
			}

			return null;
		}

		/// <summary>
		/// Retrieves the value of an attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public static object GetAttributeValue(this Entity entity, string attributeLogicalName)
		{
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			return entity.Contains(attributeLogicalName) ? entity[attributeLogicalName] : null;
		}

		private static object GetPrimitiveValue<T>(object value)
		{
			if (value is T) return value;
			if (value == null) return default(T);

			if (value is OptionSetValue && typeof(T).GetUnderlyingType() == typeof(int))
			{
				return (value as OptionSetValue).Value;
			}

			if (value is EntityReference && typeof(T).GetUnderlyingType() == typeof(Guid))
			{
				return (value as EntityReference).Id;
			}

			if (value is Money && typeof(T).GetUnderlyingType() == typeof(decimal))
			{
				return (value as Money).Value;
			}

			if (value is CrmEntityReference && typeof(T).GetUnderlyingType() == typeof(EntityReference))
			{
				var reference = value as CrmEntityReference;
				return new EntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };
			}

			return value;
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeValue(this Entity entity, string attributeLogicalName, object value)
		{
			entity.SetAttributeValue<object>(attributeLogicalName, value);
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeValue<T>(this Entity entity, string attributeLogicalName, object value)
		{
			entity.SetAttributeValue<T>(attributeLogicalName, null, value);
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeValue(this Entity entity, string attributeLogicalName, string entityLogicalName, object value)
		{
			entity.SetAttributeValue<object>(attributeLogicalName, value);
		}

		/// <summary>
		/// Modifies the value of an attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeValue<T>(this Entity entity, string attributeLogicalName, string entityLogicalName, object value)
		{
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			var raw = GetComplexValue<T>(value, entityLogicalName);
			entity[attributeLogicalName] = raw;
		}

		/// <summary>
		/// Modifies the value of a sequence attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <param name="value"></param>
		public static void SetAttributeCollectionValue<T>(this Entity entity, string attributeLogicalName, IEnumerable<T> value)
			where T : Entity
		{
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			var collection = value != null ? new EntityCollection(new List<Entity>(value)) : null;
			entity[attributeLogicalName] = collection;
		}

		private static object GetComplexValue<T>(object value, string entityLogicalName)
		{
			if (value is T) return value;
			if (value == null) return default(T);

			if (typeof(T) == typeof(OptionSetValue) && value is int)
			{
				return new OptionSetValue((int)value);
			}

			if (typeof(T) == typeof(EntityReference) && value is Guid)
			{
				return new EntityReference(entityLogicalName, (Guid)value);
			}

			if (typeof(T) == typeof(Money) && value is decimal)
			{
				return new Money((decimal)value);
			}

			if (typeof(T) == typeof(EntityReference) && value is CrmEntityReference)
			{
				var reference = value as CrmEntityReference;
				return new EntityReference(reference.LogicalName, reference.Id) { Name = reference.Name };
			}

			return value;
		}

		/// <summary>
		/// Retrieves the label value of an attribute.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public static string GetFormattedAttributeValue(this Entity entity, string attributeLogicalName)
		{
			attributeLogicalName.ThrowOnNullOrWhitespace("attributeLogicalName");

			return entity.FormattedValues.Contains(attributeLogicalName)
				? entity.FormattedValues[attributeLogicalName]
				: null;
		}

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSelector"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static TResult GetRelatedEntity<TEntity, TResult>(
			this TEntity entity,
			Expression<Func<TEntity, TResult>> relationshipSelector)
			where TEntity : Entity
			where TResult : Entity
		{
			return ForEntityRelationship(entity, relationshipSelector, relationship => GetRelatedEntity<TResult>(entity, relationship));
		}

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static TEntity GetRelatedEntity<TEntity>(this Entity entity, string relationshipSchemaName, EntityRole? primaryEntityRole = null) where TEntity : Entity
		{
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };

			return GetRelatedEntity<TEntity>(entity, relationship);
		}

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationship"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static TEntity GetRelatedEntity<TEntity>(this Entity entity, Relationship relationship) where TEntity : Entity
		{
			relationship.ThrowOnNull("relationship");

			return entity.RelatedEntities.Contains(relationship)
				? (TEntity)entity.RelatedEntities[relationship].Entities.FirstOrDefault()
				: null;
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSelector"></param>
		/// <param name="value"></param>
		public static void SetRelatedEntity<TEntity, TResult>(
			this TEntity entity,
			Expression<Func<TEntity, TResult>> relationshipSelector,
			TResult value)
			where TEntity : Entity
			where TResult : Entity
		{
			ForEntityRelationship(entity, relationshipSelector, relationship => SetRelatedEntity(entity, relationship, value));
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="value"></param>
		public static void SetRelatedEntity<TEntity>(this Entity entity, string relationshipSchemaName, TEntity value) where TEntity : Entity
		{
			entity.SetRelatedEntity(relationshipSchemaName, null, value);
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <param name="value"></param>
		public static void SetRelatedEntity<TEntity>(this Entity entity, string relationshipSchemaName, EntityRole? primaryEntityRole, TEntity value) where TEntity : Entity
		{
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };

			SetRelatedEntity(entity, relationship, value);
		}

		/// <summary>
		/// Modifies a related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationship"></param>
		/// <param name="value"></param>
		public static void SetRelatedEntity<TEntity>(this Entity entity, Relationship relationship, TEntity value) where TEntity : Entity
		{
			relationship.ThrowOnNull("relationship");
			if (value != null && string.IsNullOrWhiteSpace(value.LogicalName)) throw new ArgumentException("The entity is missing a value for the 'LogicalName' property.", "value");

			var collection = value != null
				? new EntityCollection(new[] { value }) { EntityName = value.LogicalName }
				: null;

			if (collection != null)
			{
				entity.RelatedEntities[relationship] = collection;
			}
			else
			{
				entity.RelatedEntities.Remove(relationship);
			}
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSelector"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static IEnumerable<TResult> GetRelatedEntities<TEntity, TResult>(
			this Entity entity,
			Expression<Func<TEntity, IEnumerable<TResult>>> relationshipSelector)
			where TEntity : Entity
			where TResult : Entity
		{
			return ForEntityRelationship(entity, relationshipSelector, relationship => GetRelatedEntities<TResult>(entity, relationship));
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static IEnumerable<TEntity> GetRelatedEntities<TEntity>(this Entity entity, string relationshipSchemaName, EntityRole? primaryEntityRole = null) where TEntity : Entity
		{
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };

			return GetRelatedEntities<TEntity>(entity, relationship);
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationship"></param>
		/// <returns></returns>
		/// <remarks>
		/// The entity's <see cref="P:Microsoft.Xrm.Sdk.Entity.RelatedEntities"/> collection should be loaded by first calling the <see cref="M:Microsoft.Xrm.Sdk.Client.OrganizationServiceContext.LoadProperty(Microsoft.Xrm.Sdk.Entity,Microsoft.Xrm.Sdk.Relationship)"/> method.
		/// </remarks>
		public static IEnumerable<TEntity> GetRelatedEntities<TEntity>(this Entity entity, Relationship relationship) where TEntity : Entity
		{
			relationship.ThrowOnNull("relationship");

			var entities = entity.RelatedEntities.Contains(relationship)
				? entity.RelatedEntities[relationship].Entities.Cast<TEntity>()
				: null;

			if (entities != null)
			{
				foreach (var related in entities) yield return related;
			}
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSelector"></param>
		/// <param name="entities"></param>
		public static void SetRelatedEntities<TEntity, TResult>(
			this TEntity entity,
			Expression<Func<TEntity, IEnumerable<TResult>>> relationshipSelector,
			IEnumerable<TResult> entities)
			where TEntity : Entity
			where TResult : Entity
		{
			ForEntityRelationship(entity, relationshipSelector, relationship => SetRelatedEntities(entity, relationship, entities));
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="entities"></param>
		public static void SetRelatedEntities<TEntity>(this Entity entity, string relationshipSchemaName, IEnumerable<TEntity> entities) where TEntity : Entity
		{
			entity.SetRelatedEntities(relationshipSchemaName, null, entities);
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <param name="entities"></param>
		public static void SetRelatedEntities<TEntity>(this Entity entity, string relationshipSchemaName, EntityRole? primaryEntityRole, IEnumerable<TEntity> entities) where TEntity : Entity
		{
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			if (entities != null && entities.Any(e => string.IsNullOrWhiteSpace(e.LogicalName)))
			{
				throw new ArgumentException("An entity is missing a value for the 'LogicalName' property.", "entities");
			}

			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };

			SetRelatedEntities(entity, relationship, entities);
		}

		/// <summary>
		/// Modifies the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="entity"></param>
		/// <param name="relationship"></param>
		/// <param name="entities"></param>
		public static void SetRelatedEntities<TEntity>(this Entity entity, Relationship relationship, IEnumerable<TEntity> entities) where TEntity : Entity
		{
			var collection = entities != null
				? new EntityCollection(new List<Entity>(entities)) { EntityName = entities.First().LogicalName }
				: null;

			if (collection != null)
			{
				entity.RelatedEntities[relationship] = collection;
			}
			else
			{
				entity.RelatedEntities.Remove(relationship);
			}
		}

		/// <summary>
		/// Retrieves attribute values for <see cref="EntityReference"/> attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="attributeLogicalName"></param>
		/// <returns></returns>
		public static T GetEntityReferenceValue<T>(this Entity entity, string attributeLogicalName)
		{
			return GetAttributeValue<T>(entity, attributeLogicalName);
		}

		#endregion

		#region Context Dependent Members

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <returns></returns>
		public static Entity GetRelatedEntity(this Entity entity, OrganizationServiceContext context, string relationshipSchemaName, EntityRole? primaryEntityRole = null)
		{
			context.ThrowOnNull("context");
			entity.ThrowOnNull("entity");
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			return GetRelated(context, entity, relationshipSchemaName, primaryEntityRole, GetRelatedEntity<Entity>);
		}

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationship"></param>
		/// <returns></returns>
		public static Entity GetRelatedEntity(this Entity entity, OrganizationServiceContext context, Relationship relationship)
		{
			context.ThrowOnNull("context");
			entity.ThrowOnNull("entity");
			relationship.ThrowOnNull("relationship");

			return GetRelated(entity, relationship, () => GetRelatedEntity<Entity>(context, entity, relationship));
		}

		/// <summary>
		/// Retrieves the related entity for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationshipSelector"></param>
		/// <returns></returns>
		public static TResult GetRelatedEntity<TEntity, TResult>(
			this TEntity entity,
			OrganizationServiceContext context,
			Expression<Func<TEntity, TResult>> relationshipSelector)
			where TEntity : Entity
			where TResult : Entity
		{
			context.ThrowOnNull("context");

			return ForEntityRelationship(entity, relationshipSelector, relationship => GetRelatedEntity<TResult>(context, entity, relationship));
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetRelatedEntities(this Entity entity, OrganizationServiceContext context, string relationshipSchemaName, EntityRole? primaryEntityRole = null)
		{
			context.ThrowOnNull("context");
			entity.ThrowOnNull("entity");
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			return GetRelated(context, entity, relationshipSchemaName, primaryEntityRole, GetRelatedEntities<Entity>);
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationship"></param>
		/// <returns></returns>
		public static IEnumerable<Entity> GetRelatedEntities(this Entity entity, OrganizationServiceContext context, Relationship relationship)
		{
			context.ThrowOnNull("context");
			entity.ThrowOnNull("entity");
			relationship.ThrowOnNull("relationship");

			return GetRelated(entity, relationship, () => GetRelatedEntities<Entity>(context, entity, relationship));
		}

		/// <summary>
		/// Retrieves the collection of related entities for a specific relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="entity"></param>
		/// <param name="context"></param>
		/// <param name="relationshipSelector"></param>
		/// <returns></returns>
		public static IEnumerable<TResult> GetRelatedEntities<TEntity, TResult>(
			this TEntity entity,
			OrganizationServiceContext context,
			Expression<Func<TEntity, IEnumerable<TResult>>> relationshipSelector)
			where TEntity : Entity
			where TResult : Entity
		{
			context.ThrowOnNull("context");

			return ForEntityRelationship(entity, relationshipSelector, relationship => GetRelatedEntities<TResult>(context, entity, relationship));
		}

		private static T GetRelated<T>(
			OrganizationServiceContext context,
			Entity entity,
			string relationshipSchemaName,
			EntityRole? primaryEntityRole,
			Func<OrganizationServiceContext, Entity, Relationship, T> action)
		{
			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };
			return GetRelated(entity, relationship, () => action(context, entity, relationship));
		}

		private static T GetRelatedEntity<T>(OrganizationServiceContext context, Entity entity, Relationship relationship) where T : Entity
		{
			if (!context.IsAttached(entity))
			{
				// clear out the existing related entities
				if (entity.RelatedEntities.Contains(relationship))
				{
					entity.RelatedEntities.Remove(relationship);
				}

				// handle entity already being attached before calling entity.GetRelatedEntity
				if (context.GetAttachedEntities().FirstOrDefault(e => e.Id == entity.Id) is Entity attached)
				{
					entity = attached;
				}
			}

			context.LoadProperty(entity, relationship);
			var relatedEntity = entity.GetRelatedEntity<T>(relationship.SchemaName, relationship.PrimaryEntityRole);
			return relatedEntity;
		}

		private static IEnumerable<T> GetRelatedEntities<T>(OrganizationServiceContext context, Entity entity, Relationship relationship) where T : Entity
		{
			if (!context.IsAttached(entity) && entity.RelatedEntities.Contains(relationship))
			{
				// clear out the existing related entities

				entity.RelatedEntities.Remove(relationship);
			}

			context.LoadProperty(entity, relationship);
			var relatedEntities = entity.GetRelatedEntities<T>(relationship.SchemaName, relationship.PrimaryEntityRole);
			return relatedEntities ?? new T[] { };
		}

		private static T GetRelated<T>(Entity entity, Relationship relationship, Func<T> action)
		{
			var key = "id={0},relationship={1}".FormatWith(entity.Id, relationship);

			// lock to prevent a race condition on the related entities collection

			return LockManager.Lock(key, action);
		}

		private static TResult ForEntityRelationship<TResult, TEntity>(
			Entity entity,
			Expression<Func<TEntity, TResult>> relationshipSelector,
			Func<Relationship, TResult> action)
			where TEntity : Entity
			where TResult : Entity
		{
			entity.ThrowOnNull("entity");
			relationshipSelector.ThrowOnNull("relationshipSelector");

			var me = relationshipSelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = new Relationship(relnAttribute.SchemaName) { PrimaryEntityRole = relnAttribute.PrimaryEntityRole };
					return GetRelated(entity, relationship, () => action(relationship));
				}
			}

			throw new InvalidOperationException(Strings.InvalidRelationshipSelector.FormatWith(relationshipSelector));
		}

		private static void ForEntityRelationship<TResult, TEntity>(
			Entity entity,
			Expression<Func<TEntity, TResult>> relationshipSelector,
			Action<Relationship> action)
			where TEntity : Entity
			where TResult : Entity
		{
			ForEntityRelationship(entity, relationshipSelector,
				relationship =>
				{
					action(relationship);
					return null;
				});
		}

		private static IEnumerable<TResult> ForEntityRelationship<TResult, TEntity>(
			Entity entity,
			Expression<Func<TEntity, IEnumerable<TResult>>> relationshipSelector,
			Func<Relationship, IEnumerable<TResult>> action)
			where TEntity : Entity
			where TResult : Entity
		{
			entity.ThrowOnNull("entity");
			relationshipSelector.ThrowOnNull("relationshipSelector");

			var me = relationshipSelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = new Relationship(relnAttribute.SchemaName) { PrimaryEntityRole = relnAttribute.PrimaryEntityRole };
					return GetRelated(entity, relationship, () => action(relationship));
				}
			}

			throw new InvalidOperationException(Strings.InvalidRelationshipSelector.FormatWith(relationshipSelector));
		}

		private static void ForEntityRelationship<TResult, TEntity>(
			Entity entity,
			Expression<Func<TEntity, IEnumerable<TResult>>> relationshipSelector,
			Action<Relationship> action)
			where TEntity : Entity
			where TResult : Entity
		{
			ForEntityRelationship(entity, relationshipSelector,
				relationship =>
				{
					action(relationship);
					return null;
				});
		}

		#endregion

		private static class Strings
		{
			public const string InvalidRelationshipSelector = "The selector expression '{0}' does not invoke a valid relationship property.";
		}
	}
}
