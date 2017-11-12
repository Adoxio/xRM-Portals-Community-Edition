/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Client.Metadata;
using Microsoft.Xrm.Client.Reflection;
using Microsoft.Xrm.Client.Runtime;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Helper methods on the <see cref="OrganizationServiceContext"/> class.
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		/// <summary>
		/// Executes an <see cref="OrganizationRequest"/> and returns a specifically typed <see cref="OrganizationResponse"/>.
		/// </summary>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="context"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public static TResponse Execute<TResponse>(this OrganizationServiceContext context, OrganizationRequest request)
			where TResponse : OrganizationResponse
		{
			return context.Execute(request) as TResponse;
		}

		/// <summary>
		/// Clones an arbitrary source entity and attaches it to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="includeRelatedEntities"></param>
		public static T AttachClone<T>(this OrganizationServiceContext context, T entity, bool includeRelatedEntities = false)
			where T : Entity
		{
			entity.ThrowOnNull("entity");

			var clone = entity.Clone(includeRelatedEntities);
			context.Attach(clone);
			return clone as T;
		}

		/// <summary>
		/// Clones an arbitrary source entity and attaches it to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="includeRelatedEntities"></param>
		public static T MergeClone<T>(this OrganizationServiceContext context, T entity, bool includeRelatedEntities = false)
			where T : Entity
		{
			entity.ThrowOnNull("entity");

			var entities = context.GetAttachedEntities();

			var attachedEntity = entities.FirstOrDefault(e => e.Id == entity.Id);

			if (attachedEntity != null)
			{
				return attachedEntity as T;
			}

			return AttachClone(context, entity, includeRelatedEntities);
		}

		/// <summary>
		/// Resets the EntityState of an entity before attaching it to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		public static void ReAttach(this OrganizationServiceContext context, Entity entity)
		{
			entity.ThrowOnNull("entity");

			if (context.IsAttached(entity)) return;

			if (entity.EntityState != null && entity.EntityState != EntityState.Unchanged)
			{
				entity.EntityState = null;
			}

			context.Attach(entity);
		}

		/// <summary>
		/// Detaches a sequence of entities.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entities"></param>
		/// <returns></returns>
		public static bool Detach(this OrganizationServiceContext context, IEnumerable<Entity> entities)
		{
			entities.ThrowOnNull("entities");

			var results = entities.Select(context.Detach);
			return results.All(result => result);
		}

		/// <summary>
		/// Loads the related entity collection for the specified relationship.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="propertySelector"></param>
		public static void LoadProperty<TEntity>(
			this OrganizationServiceContext context,
			TEntity entity,
			Expression<Func<TEntity, object>> propertySelector)
			where TEntity : Entity
		{
			var me = propertySelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = relnAttribute.SchemaName.ToRelationship(relnAttribute.PrimaryEntityRole);
					context.LoadProperty(entity, relationship);
				}

				context.LoadProperty(entity, me.Member.Name);
			}
			else
			{
				throw new InvalidOperationException(Strings.InvalidPropertySelector.FormatWith(propertySelector));
			}
		}

		/// <summary>
		/// Loads the related entity collection for the specified relationship.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="relationshipSchemaName"></param>
		/// <param name="primaryEntityRole"></param>
		public static void LoadProperty(this OrganizationServiceContext context, Entity entity, string relationshipSchemaName, EntityRole? primaryEntityRole = null)
		{
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			var relationship = new Relationship(relationshipSchemaName) { PrimaryEntityRole = primaryEntityRole };

			context.LoadProperty(entity, relationship);
		}

		public static void AttachLink<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			ForEntityRelationship(source, propertySelector, target, context.AttachLink);
		}

		public static void AttachLink(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship(source, relationshipSchemaName, primaryEntityRole, target, context.AttachLink);
		}

		public static bool DetachLink<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship<TSource, TTarget, bool>(source, propertySelector, target, context.DetachLink);
		}

		public static bool DetachLink(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			return ForEntityRelationship<bool>(source, relationshipSchemaName, primaryEntityRole, target, context.DetachLink);
		}

		public static bool IsAttached<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship<TSource, TTarget, bool>(source, propertySelector, target, context.IsAttached);
		}

		public static bool IsAttached<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, IEnumerable<TTarget>>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship<TSource, TTarget, bool>(source, propertySelector, target, context.IsAttached);
		}

		public static void IsAttached(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship<bool>(source, relationshipSchemaName, primaryEntityRole, target, context.IsAttached);
		}

		public static bool IsDeleted<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship<TSource, TTarget, bool>(source, propertySelector, target, context.IsDeleted);
		}

		public static void IsDeleted(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship<bool>(source, relationshipSchemaName, primaryEntityRole, target, context.IsDeleted);
		}

		public static void AddLink<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			ForEntityRelationship(source, propertySelector, target, context.AddLink);
		}

		public static void AddLink(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship(source, relationshipSchemaName, primaryEntityRole, target, context.AddLink);
		}

		public static void DeleteLink<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			ForEntityRelationship(source, propertySelector, target, context.DeleteLink);
		}

		public static void DeleteLink(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship(source, relationshipSchemaName, primaryEntityRole, target, context.DeleteLink);
		}

		public static void AddRelatedObject<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			ForEntityRelationship(source, propertySelector, target, context.AddRelatedObject);
		}

		public static void AddRelatedObject<TSource, TTarget>(
			this OrganizationServiceContext context,
			TSource source,
			Expression<Func<TSource, IEnumerable<TTarget>>> propertySelector,
			TTarget target)
			where TSource : Entity
			where TTarget : Entity
		{
			ForEntityRelationship(source, propertySelector, target, context.AddRelatedObject);
		}

		public static void AddRelatedObject(this OrganizationServiceContext context, Entity source, string relationshipSchemaName, Entity target, EntityRole? primaryEntityRole = null)
		{
			ForEntityRelationship(source, relationshipSchemaName, primaryEntityRole, target, context.AddRelatedObject);
		}

		private static void ForEntityRelationship<TSource, TTarget>(
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target,
			Action<Entity, Relationship, Entity> action)
			where TSource : Entity
			where TTarget : Entity
		{
			var me = propertySelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = relnAttribute.SchemaName.ToRelationship(relnAttribute.PrimaryEntityRole);
					action(source, relationship, target);
					return;
				}

				ForEntityRelationship(source, me.Member.Name, null, target, action);
			}
			else
			{
				throw new InvalidOperationException(Strings.InvalidRelationshipSelector.FormatWith(propertySelector));
			}
		}

		private static void ForEntityRelationship<TSource, TTarget>(
			TSource source,
			Expression<Func<TSource, IEnumerable<TTarget>>> propertySelector,
			TTarget target,
			Action<Entity, Relationship, Entity> action)
			where TSource : Entity
			where TTarget : Entity
		{
			var me = propertySelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = relnAttribute.SchemaName.ToRelationship(relnAttribute.PrimaryEntityRole);
					action(source, relationship, target);
					return;
				}

				ForEntityRelationship(source, me.Member.Name, null, target, action);
			}
			else
			{
				throw new InvalidOperationException(Strings.InvalidRelationshipSelector.FormatWith(propertySelector));
			}
		}

		private static TResult ForEntityRelationship<TSource, TTarget, TResult>(
			TSource source,
			Expression<Func<TSource, TTarget>> propertySelector,
			TTarget target,
			Func<Entity, Relationship, Entity, TResult> action)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship(source, propertySelector as LambdaExpression, target, action);
		}

		private static TResult ForEntityRelationship<TSource, TTarget, TResult>(
			TSource source,
			Expression<Func<TSource, IEnumerable<TTarget>>> propertySelector,
			TTarget target,
			Func<Entity, Relationship, Entity, TResult> action)
			where TSource : Entity
			where TTarget : Entity
		{
			return ForEntityRelationship(source, propertySelector as LambdaExpression, target, action);
		}

		private static TResult ForEntityRelationship<TResult>(
			Entity source,
			LambdaExpression propertySelector,
			Entity target,
			Func<Entity, Relationship, Entity, TResult> action)
		{
			var me = propertySelector.Body as MemberExpression;

			if (me != null)
			{
				var relnAttribute = me.Member.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = relnAttribute.SchemaName.ToRelationship(relnAttribute.PrimaryEntityRole);
					return action(source, relationship, target);
				}

				return ForEntityRelationship(source, me.Member.Name, null, target, action);
			}

			throw new InvalidOperationException(Strings.InvalidRelationshipSelector.FormatWith(propertySelector));
		}

		private static void ForEntityRelationship(Entity source, string relationshipSchemaName, EntityRole? primaryEntityRole, Entity target, Action<Entity, Relationship, Entity> action)
		{
			ForEntityRelationship<object>(source, relationshipSchemaName, primaryEntityRole,
				relationship =>
					{
						action(source, relationship, target);
						return null;
					});
		}

		private static T ForEntityRelationship<T>(Entity source, string relationshipSchemaName, EntityRole? primaryEntityRole, Entity target, Func<Entity, Relationship, Entity, T> action)
		{
			return ForEntityRelationship(source, relationshipSchemaName, primaryEntityRole, relationship => action(source, relationship, target));
		}

		private static T ForEntityRelationship<T>(Entity source, string relationshipSchemaName,  EntityRole? primaryEntityRole, Func<Relationship, T> action)
		{
			source.ThrowOnNull("source");
			relationshipSchemaName.ThrowOnNullOrWhitespace("relationshipSchemaName");

			return action(relationshipSchemaName.ToRelationship(primaryEntityRole));
		}

		/// <summary>
		/// Removes an item from cache if it is applicable to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="cacheKey"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryRemoveFromCache(this OrganizationServiceContext context, string cacheKey)
		{
			return TryAccessCache(context, cache => cache.Remove(cacheKey));
		}

		/// <summary>
		/// Removes a request/response from cache if it is applicable to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryRemoveFromCache(this OrganizationServiceContext context, OrganizationRequest request)
		{
			return TryAccessCache(context, cache => cache.Remove(request));
		}

		/// <summary>
		/// Removes an entity from cache if it is applicable to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entityLogicalName"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryRemoveFromCache(this OrganizationServiceContext context, string entityLogicalName, Guid? id)
		{
			return TryAccessCache(context, cache => cache.Remove(entityLogicalName, id));
		}

		/// <summary>
		/// Removes an entity from cache if it is applicable to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryRemoveFromCache(this OrganizationServiceContext context, EntityReference entity)
		{
			return TryAccessCache(context, cache => cache.Remove(entity));
		}

		/// <summary>
		/// Removes an entity from cache if it is applicable to the context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryRemoveFromCache(this OrganizationServiceContext context, Entity entity)
		{
			return TryAccessCache(context, cache => cache.Remove(entity));
		}

		/// <summary>
		/// Tries to perform an action on the underlying <see cref="IOrganizationServiceCache"/> if applicable.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		/// <remarks>
		/// In order for the operation to succeed, the <see cref="OrganizationServiceContext"/> must implement <see cref="IOrganizationServiceContainer"/> and
		/// the containing <see cref="IOrganizationService"/> must inherit <see cref="CachedOrganizationService"/>.
		/// </remarks>
		public static bool TryAccessCache(this OrganizationServiceContext context, Action<IOrganizationServiceCache> action)
		{
			var container = context as IOrganizationServiceContainer;

			if (container == null) return false;

			var service = container.Service as IOrganizationServiceCacheContainer;

			if (service == null || service.Cache == null) return false;

			action(service.Cache);

			return true;
		}

		private static class Strings
		{
			public const string InvalidRelationshipSelector = "The selector expression '{0}' does not invoke a valid relationship property.";
			public const string InvalidPropertySelector = "The selector expression '{0}' does not invoke a valid entity property.";
		}
	}

	/// <summary>
	/// For internal use only.
	/// </summary>
	public static class OrganizationServiceContextUtility
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="sourceEntityName"></param>
		/// <param name="websiteID"></param>
		/// <returns></returns>
		public static bool TryGetWebsiteIDFromParentLinkForEntityInPendingChanges(OrganizationServiceContext context, Entity entity, string sourceEntityName, out EntityReference websiteID)
		{
			var portalContext = context as CrmOrganizationServiceContext;

			if (portalContext == null)
			{
				throw new ArgumentException("The 'context' object must be of the type '{0}'.".FormatWith(typeof(CrmOrganizationServiceContext)));
			}

			// Get AddLink changes that reference our new entity.

			var relevantChanges = portalContext.Log.Where(l =>
				l.Operation == CrmOrganizationServiceContext.UpdatableOperation.AddReferenceToCollection
				&& l.Resource == entity && l.Target.LogicalName == sourceEntityName);

			foreach (var change in relevantChanges)
			{
				EntitySetInfo entitySetInfo;
				RelationshipInfo crmAssociationInfo;

				if (!OrganizationServiceContextInfo.TryGet(context.GetType(), change.Target.LogicalName, out entitySetInfo)
					|| !entitySetInfo.Entity.RelationshipsByPropertyName.TryGetValue(change.PropertyName, out crmAssociationInfo))
				{
					continue;
				}

				// If it's a 1-to-many relationship, we've found a parent link.
				if (crmAssociationInfo.IsCollection)
				{
					websiteID = change.Target.GetAttributeValue<EntityReference>("adx_websiteid");

					return true;
				}
			}

			websiteID = null;

			return false;
		}
	}
}
