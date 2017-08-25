/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Services;
using System.Linq;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Reflection;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Indicates that a class contains an <see cref="IOrganizationService"/>.
	/// </summary>
	public interface IOrganizationServiceContainer
	{
		/// <summary>
		/// Gets the service.
		/// </summary>
		IOrganizationService Service { get; }
	}

	/// <summary>
	/// An <see cref="OrganizationServiceContext"/> that is compatible with WCF Data Services.
	/// </summary>
	public class CrmOrganizationServiceContext : OrganizationServiceContext, IInitializable, IOrganizationService, IUpdatable, IExpandProvider, IOrganizationServiceContainer
	{
		private readonly bool _shouldDisposeService;
		private readonly IOrganizationService _service;

		IOrganizationService IOrganizationServiceContainer.Service
		{
			get { return _service; }
		}

		public CrmOrganizationServiceContext()
			: this((string)null)
		{
		}

		public CrmOrganizationServiceContext(string contextName)
			: this(CrmConfigurationManager.CreateService(contextName))
		{
			_shouldDisposeService = true;
		}

		public CrmOrganizationServiceContext(CrmConnection connection)
			: this(CrmConfigurationManager.CreateService(connection))
		{
			_shouldDisposeService = true;
		}

		public CrmOrganizationServiceContext(IOrganizationService service)
			: base(service)
		{
			_service = service;
		}

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
		}

		protected override void OnBeginEntityTracking(Entity entity)
		{
			// associate the entity with this context for lazy loading relationships

			var crmEntity = entity as CrmEntity;

			if (crmEntity != null)
			{
				crmEntity.Attach(this);
			}

			base.OnBeginEntityTracking(entity);
		}

		protected override void OnEndEntityTracking(Entity entity)
		{
			var crmEntity = entity as CrmEntity;

			if (crmEntity != null)
			{
				crmEntity.Attach(null);
			}

			base.OnEndEntityTracking(entity);
		}

		protected override void OnSaveChanges(SaveChangesResultCollection results)
		{
			// reattach entities that are found in the save changes results

			if (!results.HasError)
			{
				var entities = results.SelectMany(result => GetEntitiesForObject(result.Request));

				foreach (var entity in entities)
				{
					// reset the state
					entity.EntityState = null;

					Attach(entity);
				}
			}
		}

		private IEnumerable<Entity> GetEntitiesForObject(object query, IEnumerable<object> path = null)
		{
			if (query is KeyedRequest) return GetEntities(query as KeyedRequest, path ?? new List<object> { query });
			if (query is OrganizationRequest) return GetEntities(query as OrganizationRequest, path ?? new List<object> { query });
			if (query is OrganizationResponse) return GetEntities(query as OrganizationResponse, path ?? new List<object> { query });

			if (query is IEnumerable<Entity>) return query as IEnumerable<Entity>;
			if (query is EntityCollection) return GetEntities(query as EntityCollection);
			if (query is Entity) return GetEntities(query as Entity);

			return GetEntitiesEmpty();
		}

		private static IEnumerable<Entity> GetEntitiesEmpty()
		{
			yield break;
		}

		private static IEnumerable<Entity> GetEntities(Entity entity)
		{
			yield return entity;
		}

		private IEnumerable<Entity> GetEntities(KeyedRequest request, IEnumerable<object> path)
		{
			return GetEntitiesForObject(request.Request, path);
		}

		private IEnumerable<Entity> GetEntities(OrganizationRequest request, IEnumerable<object> path)
		{
			foreach (var parameter in request.Parameters)
			{
				var value = parameter.Value;

				if (value != null && !path.Contains(value))
				{
					foreach (var child in GetEntitiesForObject(value, path.Concat(new[] { value })))
					{
						yield return child;
					}
				}
			}
		}

		private IEnumerable<Entity> GetEntities(OrganizationResponse response, IEnumerable<object> path)
		{
			foreach (var parameter in response.Results)
			{
				var value = parameter.Value;

				if (value != null && !path.Contains(value))
				{
					foreach (var child in GetEntitiesForObject(value, path.Concat(new[] { value })))
					{
						yield return child;
					}
				}
			}
		}

		private static IEnumerable<Entity> GetEntities(EntityCollection entities)
		{
			return entities.Entities;
		}

		#region IUpdatable Members

		private static readonly string[] _readOnlyProperties = new[] { "Id", "LogicalName", "EntityState", "RelatedEntities" };

		void IUpdatable.ClearChanges()
		{
			ClearChanges();
		}

		object IUpdatable.CreateResource(string containerName, string fullTypeName)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "CreateResource", "containerName={0}, fullTypeName={1}", containerName, fullTypeName);

			var entityType = TypeExtensions.GetType(fullTypeName);

			if (entityType.IsA(typeof(Entity)))
			{
				var entity = Activator.CreateInstance(entityType) as Entity;

				AddObject(entity);

				return entity;
			}

			return Activator.CreateInstance(entityType);
		}

		void IUpdatable.DeleteResource(object targetResource)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "DeleteResource", "targetResource={0}", targetResource);

			DeleteObject(targetResource as Entity);
		}

		object IUpdatable.GetResource(IQueryable query, string fullTypeName)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "GetResource", "fullTypeName={0}", fullTypeName);

			object resource = null;

			foreach (var obj in query)
			{
				if (resource != null)
				{
					throw new DataServiceException("Expected a single response.");
				}

				resource = obj;
			}

			if (fullTypeName != null && resource != null)
			{
				var resourceType = resource.GetType().FullName;

				if (!fullTypeName.Equals(resourceType))
				{
					throw new DataServiceException("Expected resource of type '{0}' but retrieved a resource of type '{1}' instead.".FormatWith(fullTypeName, resourceType));
				}
			}

			return resource;
		}

		object IUpdatable.ResetResource(object resource)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "ResetResource", "resource={0}", resource);

			// We do not clear out all attribute values except for the Id and LogicalName
			return resource;
		}

		object IUpdatable.ResolveResource(object resource)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "ResolveResource", "resource={0}", resource);

			// convert the resource from an EntityReference (key) to a full Entity

			var id = resource as EntityReference;

			if (id != null && id.Id != Guid.Empty && !string.IsNullOrWhiteSpace(id.LogicalName))
			{
				return ResolveObject(id);
			}

			// if the entity does not have a creation date, reload the entity

			var entity = resource as Entity;

			if (entity != null
				&& entity.Id != Guid.Empty
				&& !string.IsNullOrWhiteSpace(entity.LogicalName)
				&& !entity.Attributes.Contains("createdon"))
			{
				return ResolveObject(entity.ToEntityReference());
			}

			return resource;
		}

		private Entity ResolveObject(EntityReference id)
		{
			// The resolved entity currently does not get attached to the context
			var resolved = Retrieve(id.LogicalName, id.Id, new ColumnSet(true));
			return resolved;
		}

		void IUpdatable.SaveChanges()
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "SaveChanges", "Begin");

			var results = SaveChanges();

			foreach (var result in results)
			{
				if (result.Error != null)
				{
					Tracing.FrameworkError("CrmOrganizationServiceContext", "SaveChanges", result.Error.Message);
				}
			}

			if (results.HasError)
			{
				throw new DataServiceException("An error occured during save.");
			}

			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "SaveChanges", "End");
		}

		void IUpdatable.AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "AddReferenceToCollection", "targetResource={0}, propertyName={1}, resourceToBeAdded={2}", targetResource, propertyName, resourceToBeAdded);

			var relationship = GetRelationship(targetResource, propertyName);

			AddLink(targetResource as Entity, relationship, resourceToBeAdded as Entity);

			AddToLog(UpdatableOperation.AddReferenceToCollection, targetResource as Entity, resourceToBeAdded as Entity, propertyName, null);
		}

		void IUpdatable.RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "RemoveReferenceFromCollection", "targetResource={0}, propertyName={1}, resourceToBeRemoved={2}", targetResource, propertyName, resourceToBeRemoved);

			var relationship = GetRelationship(targetResource, propertyName);

			DeleteLink(targetResource as Entity, relationship, resourceToBeRemoved as Entity);
		}

		void IUpdatable.SetReference(object targetResource, string propertyName, object propertyValue)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "SetReference", "targetResource={0}, propertyName={1}, propertyValue={2}", targetResource, propertyName, propertyValue);

			var relationship = GetRelationship(targetResource, propertyName);

			AddLink(targetResource as Entity, relationship, propertyValue as Entity);
		}

		object IUpdatable.GetValue(object targetResource, string propertyName)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "GetValue", "targetResource={0}, propertyName={1}", targetResource, propertyName);

			var type = targetResource.GetType();
			var pi = type.GetProperty(propertyName);

			if (pi == null)
			{
				throw new DataServiceException("The target resource of type '{0}' does not contain a property named '{1}'.".FormatWith(type, propertyName));
			}

			return pi.GetValue(targetResource, null);
		}

		void IUpdatable.SetValue(object targetResource, string propertyName, object propertyValue)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "SetValue", "targetResource={0}, propertyName={1}, propertyValue={2}", targetResource, propertyName, propertyValue);

			var type = targetResource.GetType();
			var pi = type.GetProperty(propertyName);

			if (pi == null)
			{
				throw new DataServiceException("The target resource of type '{0}' does not contain a property named '{1}'.".FormatWith(type, propertyName));
			}

			if (pi.CanWrite && IsReadOnlyEntityProperty(targetResource, propertyName))
			{
				var value = ParseValue(propertyValue);
				pi.SetValue(targetResource, value, null);

				var target = targetResource as Entity;

				if (target != null)
				{
					UpdateObject(target);
				}
			}
		}

		private static Relationship GetRelationship(object targetResource, string propertyName)
		{
			var entityType = targetResource.GetType();

			// find the relationship schema name

			var propertyInfo = entityType.GetProperty(propertyName);

			if (propertyInfo != null)
			{
				var relnAttribute = propertyInfo.GetFirstOrDefaultCustomAttribute<RelationshipSchemaNameAttribute>();

				if (relnAttribute != null)
				{
					var relationship = relnAttribute.SchemaName.ToRelationship(relnAttribute.PrimaryEntityRole);
					return relationship;
				}
			}

			return propertyName.ToRelationship();
		}

		private static bool IsReadOnlyEntityProperty(object resource, string propertyName)
		{
			return !(resource is Entity) || !_readOnlyProperties.Contains(propertyName);
		}

		private static object ParseValue(object value)
		{
			// check if this is a null EntityReference

			var reference = value as EntityReference;
			if (reference != null && reference.LogicalName == null && reference.Id == Guid.Empty) return null;
			return value;
		}

		internal enum UpdatableOperation { AddReferenceToCollection }

		internal class UpdatableLog
		{
			public UpdatableOperation Operation { get; set; }
			public Entity Target { get; set; }
			public Entity Resource { get; set; }
			public string PropertyName { get; set; }
			public object PropertyValue { get; set; }

			public UpdatableLog(UpdatableOperation operation, Entity target, Entity resource, string propertyName, object propertyValue)
			{
				Operation = operation;
				Target = target;
				Resource = resource;
				PropertyName = propertyName;
				PropertyValue = propertyValue;
			}
		}

		private readonly Lazy<List<UpdatableLog>> _log = new Lazy<List<UpdatableLog>>(() => new List<UpdatableLog>());

		internal IList<UpdatableLog> Log
		{
			get { return _log.Value;  }
		}

		private void AddToLog(UpdatableOperation operation, Entity target, Entity resource, string propertyName, object propertyValue)
		{
			Log.Add(new UpdatableLog(operation, target, resource, propertyName, propertyValue));
		}

		#endregion

		#region IExpandProvider Members

		IEnumerable IExpandProvider.ApplyExpansions(IQueryable queryable, ICollection<ExpandSegmentCollection> expandPaths)
		{
			Tracing.FrameworkInformation("CrmOrganizationServiceContext", "ApplyExpansions", "expandPaths={0}", expandPaths);

			// $expand=property1/subproperty1,property2/subproperty2

			if (expandPaths.Count() > 0)
			{
				foreach (var entity in queryable)
				{
					if (!(entity is Entity)) break;

					foreach (var path in expandPaths)
					{
						foreach (var segment in path)
						{
							LoadProperty(entity as Entity, segment.Name);
						}
					}
				}
			}

			return queryable;
		}

		#endregion

		#region IOrganizationService Members

		public Guid Create(Entity entity)
		{
			return _service.Create(entity);
		}

		public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
		{
			return _service.Retrieve(entityName, id, columnSet);
		}

		public void Update(Entity entity)
		{
			_service.Update(entity);
		}

		public void Delete(string entityName, Guid id)
		{
			_service.Delete(entityName, id);
		}

		public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			_service.Associate(entityName, entityId, relationship, relatedEntities);
		}

		public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			_service.Disassociate(entityName, entityId, relationship, relatedEntities);
		}

		public EntityCollection RetrieveMultiple(QueryBase query)
		{
			return _service.RetrieveMultiple(query);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && _shouldDisposeService)
			{
				var service = _service as IDisposable;

				if (service != null)
				{
					service.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
