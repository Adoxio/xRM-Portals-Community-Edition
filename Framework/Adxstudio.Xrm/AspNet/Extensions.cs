/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Owin;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	
	internal static class TaskExtensions
	{
		public static ConfiguredTaskAwaitable WithCurrentCulture(this Task task)
		{
			return task.ConfigureAwait(false);
		}

		public static ConfiguredTaskAwaitable<T> WithCurrentCulture<T>(this Task<T> task)
		{
			return task.ConfigureAwait(false);
		}
	}

	public static class Extensions
	{
		/// <summary>
		/// Filter columns based on crm solution version and returns the filtered columns as string array.
		/// </summary>
		/// <param name="columns">Entity Node columns</param>
		/// <param name="solutionVersion">solution version based on which filtering needs to be done.</param>
		/// <returns> Returns string array of column names.</returns>
		public static string[] ToFilteredColumns(this EntityNodeColumn[] columns, Version solutionVersion)
		{
			if (columns.Any())
			{
				return columns
					.Where(c => c.IntroducedVersion != null && c.IntroducedVersion.Major <= solutionVersion.Major && c.IntroducedVersion.Minor <= solutionVersion.Minor)
					.Select(c => c.Name)
					.ToArray();
			}

			return null;
		}

		public static string GetRequestBody(this IOwinContext context)
		{
			if (!string.Equals(context.Request.Method, "POST"))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Unable to read request body. Invalid request method.");

				return null;
			}

			if (context.Request.CallCancelled.IsCancellationRequested)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Unable to read request body. The client is disconnected.");

				return null;
			}

			var reader = new StreamReader(context.Request.Body);
			var originalPosition = context.Request.Body.Position;
			var body = reader.ReadToEnd();
			context.Request.Body.Position = originalPosition;

			return body;
		}
	}

	public static class OrganizationServiceExtensions
	{
		public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService service, IEnumerable<OrganizationRequest> requests, bool returnResponses = false, bool continueOnError = false)
		{
			var request = new ExecuteMultipleRequest
			{
				Settings = new ExecuteMultipleSettings { ContinueOnError = continueOnError, ReturnResponses = returnResponses },
				Requests = new OrganizationRequestCollection(),
			};

			request.Requests.AddRange(requests);

			return service.Execute(request) as ExecuteMultipleResponse;
		}
	}

	public static class EntityExtensions
	{
		internal static readonly Condition ActiveStateCondition = new Condition("statecode", ConditionOperator.Equal, 0);

		internal static Guid ToGuid<TKey>(this TKey key)
		{
			if (Equals(key, default(TKey)) || Equals(key, string.Empty)) return Guid.Empty;
			if (typeof(TKey) == typeof(string)) return new Guid((string)(object)key);
			return (Guid)(object)key;
		}

		internal static TKey ToKey<TKey>(this Guid guid)
		{
			if (typeof(TKey) == typeof(string)) return (TKey)(object)guid.ToString();
			return (TKey)(object)guid;
		}

		/// <summary>
		/// Returns an entity graph containing the attributes and relationships that have changed 
		/// </summary>
		public static Entity ToChangedEntity(this Entity entity, Entity snapshot)
		{
			if (entity == null) return null;

			var result = new Entity(entity.LogicalName)
			{
				Id = entity.Id,
				EntityState = entity.Id == Guid.Empty ? EntityState.Created : entity.EntityState,
			};

			result.Attributes.AddRange(ToChangedAttributes(entity.Attributes, snapshot));
			result.RelatedEntities.AddRange(ToChangedRelationships(entity.RelatedEntities, snapshot));

			return result.Attributes.Any() || result.RelatedEntities.Any()
				? result
				: null;
		}

		private static IEnumerable<KeyValuePair<string, object>> ToChangedAttributes(
			IEnumerable<KeyValuePair<string, object>> attributes, Entity snapshot)
		{
			foreach (var attribute in attributes)
			{
				object value;

				if (snapshot == null)
				{
					yield return attribute;
				}
				else if (!snapshot.Attributes.TryGetValue(attribute.Key, out value))
				{
					if (attribute.Value != null)
					{
						yield return attribute;
					}
				}
				else if (!Equals(attribute.Value, value))
				{
					yield return attribute;
				}
			}
		}

		private static IEnumerable<KeyValuePair<Relationship, EntityCollection>> ToChangedRelationships(
			IEnumerable<KeyValuePair<Relationship, EntityCollection>> relationships, Entity snapshot)
		{
			foreach (var relationship in relationships)
			{
				EntityCollection collection;

				if (snapshot == null || !snapshot.RelatedEntities.TryGetValue(relationship.Key, out collection))
				{
					collection = null;
				}

				var result = new EntityCollection(ToChangedRelationship(relationship.Value.Entities, collection).ToList());

				if (result.Entities.Any())
				{
					yield return new KeyValuePair<Relationship, EntityCollection>(relationship.Key, result);
				}
			}
		}

		private static IEnumerable<Entity> ToChangedRelationship(IEnumerable<Entity> entities, EntityCollection collection)
		{
			foreach (var entity in entities)
			{
				var snapshot = collection != null
					? collection.Entities.SingleOrDefault(e => Equals(e.ToEntityReference(), entity.ToEntityReference()))
					: null;

				var result = ToChangedEntity(entity, snapshot);

				if (result != null)
				{
					yield return result;
				}
			}
		}

		/// <summary>
		/// Returns a collection of entities that exist in the snapshot entity graph but are absent in this entity graph.
		/// </summary>
		public static IEnumerable<Entity> ToRemovedEntities(this Entity entity, Entity snapshot)
		{
			var removed = ToRemovedEntity(entity, snapshot);
			return GetRelatedEntitiesRecursive(removed).Except(new[] { removed }).ToList();
		}

		private static Entity ToRemovedEntity(Entity entity, Entity snapshot)
		{
			if (snapshot == null) return null;

			var result = new Entity(snapshot.LogicalName)
			{
				Id = snapshot.Id,
				EntityState = snapshot.Id == Guid.Empty ? EntityState.Created : snapshot.EntityState,
			};

			result.RelatedEntities.AddRange(ToRemovedRelationships(entity, snapshot.RelatedEntities));

			return entity == null || result.RelatedEntities.Any()
				? result
				: null;
		}

		private static IEnumerable<Entity> GetRelatedEntitiesRecursive(Entity entity)
		{
			if (entity == null) yield break;

			yield return entity;

			foreach (
				var related in
					entity.RelatedEntities.SelectMany(relationship => relationship.Value.Entities)
						.SelectMany(GetRelatedEntitiesRecursive))
			{
				yield return related;
			}
		}

		private static IEnumerable<KeyValuePair<Relationship, EntityCollection>> ToRemovedRelationships(Entity entity,
			IEnumerable<KeyValuePair<Relationship, EntityCollection>> relationships)
		{
			foreach (var relationship in relationships)
			{
				EntityCollection collection;

				if (entity == null || !entity.RelatedEntities.TryGetValue(relationship.Key, out collection))
				{
					collection = null;
				}

				var result = new EntityCollection(ToRemovedRelationship(collection, relationship.Value.Entities).ToList());

				if (result.Entities.Any())
				{
					yield return new KeyValuePair<Relationship, EntityCollection>(relationship.Key, result);
				}
			}
		}

		private static IEnumerable<Entity> ToRemovedRelationship(EntityCollection collection, IEnumerable<Entity> entities)
		{
			foreach (var snapshot in entities)
			{
				var entity = collection != null
					? collection.Entities.SingleOrDefault(e => Equals(e.ToEntityReference(), snapshot.ToEntityReference()))
					: null;

				var result = ToRemovedEntity(entity, snapshot);

				if (result != null)
				{
					yield return result;
				}
			}
		}
	}
}
