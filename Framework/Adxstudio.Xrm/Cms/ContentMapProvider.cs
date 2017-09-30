/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.ServiceModel;
	using System.Threading.Tasks;
	using Adxstudio.Xrm.AspNet;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.EventHubBasedInvalidation;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.Web.UI;

	public class ContentMapProvider : IContentMapProvider, IDisposable
	{
		private readonly Func<CrmDbContext> CreateContext;

		private Lazy<ContentMap> _map;

		protected virtual ISolutionDefinitionProvider SolutionDefinitionProvider { get; private set; }

		public virtual TimeSpan? LockTimeout { get; set; }

		public Version BaseSolutionCrmVersion { get; set; }

		protected EventHubJobSettings EventHubJobSettings { get; private set; }

		protected PortalSolutions PortalSolutions { get; private set; }

		public ContentMapProvider(
			Func<CrmDbContext> createContext,
			ISolutionDefinitionProvider solutionDefinitionProvider,
			EventHubJobSettings eventHubJobSettings,
			PortalSolutions portalSolutions)
		{
			this.CreateContext = createContext;
			this.SolutionDefinitionProvider = solutionDefinitionProvider;
			this.EventHubJobSettings = eventHubJobSettings;
			this.PortalSolutions = portalSolutions;

			_map = new Lazy<ContentMap>(this.GetContentMap);
		}

		public virtual T Using<T>(Func<ContentMap, T> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return Using(ContentMapLockType.Read, action, memberName, sourceFilePath, sourceLineNumber);
		}

		public virtual void Using(Action<ContentMap> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Using(ContentMapLockType.Read, action, memberName, sourceFilePath, sourceLineNumber);
		}

		protected virtual T Using<T>(ContentMapLockType lockType, Func<ContentMap, T> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var map = _map.Value;
			return map.Using(lockType, () => action(map), memberName, sourceFilePath, sourceLineNumber);
		}

		protected virtual void Using(ContentMapLockType lockType, Action<ContentMap> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var map = _map.Value;
			map.Using(lockType, () => action(map), memberName, sourceFilePath, sourceLineNumber);
		}

		protected virtual ContentMap GetContentMap()
		{
			using (var context = this.CreateContext())
			{
				return this.GetContentMap(context);
			}
		}

		private ContentMap GetContentMap(CrmDbContext context)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LockTimeout={0}", this.LockTimeout));

			var sw = Stopwatch.StartNew();

			var solution = this.SolutionDefinitionProvider.GetSolution();
			var parameters = this.SolutionDefinitionProvider.GetQueryParameters();
			var map = new ContentMap(solution) { LockTimeout = this.LockTimeout.GetValueOrDefault(TimeSpan.FromMinutes(1)) };
			var entities = this.GetEntities(context, map.Solution, parameters).ToList();

			map.Using(ContentMapLockType.Write, () => this.BuildContentMap(map, entities));

			sw.Stop();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Duration: {0} ms", sw.ElapsedMilliseconds));

			return map;
		}

		protected virtual void BuildContentMap(ContentMap map, IEnumerable<Entity> entities)
		{
			var sw = Stopwatch.StartNew();

			map.AddRange(entities);

			ApplyBaseSolutionVersionToWebsite(map);

			sw.Stop();

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Duration: {0} ms", sw.ElapsedMilliseconds));
		}

		protected virtual IEnumerable<Entity> GetEntities(CrmDbContext context, SolutionDefinition solution, IDictionary<string, object> parameters)
		{
			ConcurrentBag<Entity> result = new ConcurrentBag<Entity>();
			var queries = solution.GetQueries(parameters).Where(query => query != null);

			Parallel.ForEach(queries, query =>
			{
				// Add content map entities explicitly to the Portalused entities list. 
				// Since we are skipping cache for content map entities and hence these entities won't be added in this list via Dependency calucaltion.
				ADXTrace.Instance.TraceVerbose(TraceCategory.Application, $"Entity {query.Entity.Name} is added to Enabled Entity List for Portal Cache ");
				WebAppConfigurationProvider.PortalUsedEntities.TryAdd(query.Entity.Name, true);
				Fetch(context.Service, query, result);
			});

			return result.AsEnumerable();
		}

		/// <summary>
		/// Applies base solution version to website. This should only be called AFTER the ContentMap is completely built.
		/// </summary>
		/// <param name="map">ContentMap to apply base solution version.</param>
		private void ApplyBaseSolutionVersionToWebsite(ContentMap map)
		{
			// Apply base solution version to Website
			const string websiteKey = "adx_website";

			if (map.ContainsKey(websiteKey) && map[websiteKey] != null)
			{
				var retrieveBaseSolutionCrmVersion = this.PortalSolutions.BaseSolutionCrmVersion;

				foreach (var node in map[websiteKey])
				{
					var websiteNode = node.Value as WebsiteNode;

					if (websiteNode != null)
					{
						websiteNode.CurrentBaseSolutionCrmVersion = retrieveBaseSolutionCrmVersion;
					}
				}
			}
		}

		private static void Fetch(IOrganizationService service, Fetch query, ConcurrentBag<Entity> result)
		{
			try
			{
				foreach (var entity in service.RetrieveAll(query))
				{
					result.Add(entity);
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception {0} occured in one of the queries in content map Load", e.Message));
				throw;
			}
		}

		public virtual void Clear()
		{
			_map = new Lazy<ContentMap>(GetContentMap);
		}

		public virtual EntityNode Refresh(ContentMap map, EntityReference reference)
		{
			using (var context = this.CreateContext())
			{
				return Refresh(context, map, reference);
			}
		}

		private static EntityNode Refresh(CrmDbContext context, ContentMap map, EntityReference reference)
		{
			reference.ThrowOnNull("reference");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(reference.LogicalName), reference.Id));

			EntityDefinition ed;

			if (map.Solution.Entities.TryGetValue(reference.LogicalName, out ed))
			{
				// retrieve a fresh entity which also acts as a backend validation

				var fetch = ed.CreateFetch();

				Entity entity = null;

				try
				{
					string primaryIdAttribute = EventHubBasedInvalidation.CrmChangeTrackingManager.Instance.TryGetPrimaryKey(reference.LogicalName);
					
					// The condition for the filter on primary key
					var primaryAttributeCondition = new Condition
					{
						Attribute = primaryIdAttribute,
						Operator = ConditionOperator.Equal,
						Value = reference.Id
					};

					var attributes = fetch.Entity.Attributes;
					var fQuery = new Fetch
					{
						Distinct = true,
						SkipCache = true,
						Entity = new FetchEntity
						{
							Name = reference.LogicalName,
							Attributes = attributes,
							Filters = new List<Filter>()
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>()
									{
										primaryAttributeCondition
									}
								}
							}
						}
					};

					entity = context.Service.RetrieveSingle(fQuery, true, true);
				}
				catch (FaultException<OrganizationServiceFault> fe)
				{
					// an exception occurs when trying to retrieve a non-existing entity

					if (!fe.Message.EndsWith("Does Not Exist"))
					{
						throw;
					}
				}

				// Check if the entity matches on the defined relationships.
				if (!ed.ShouldIncludeInContentMap(entity))
				{
					return null;
				}

				// check if the entity is inactive according to the definition
				var option = entity != null ? entity.GetAttributeValue<OptionSetValue>("statecode") : null;
				var isActive = ed.ActiveStateCode == null || (option != null && ed.ActiveStateCode.Value == option.Value);

				var node = map.Using(ContentMapLockType.Write, () => entity != null
					? (isActive
						? map.Replace(entity)
						: map.Deactivate(reference))
					: map.Remove(reference));

				return node;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Unknown: logicalName={0}", EntityNamePrivacy.GetEntityName(reference.LogicalName)));

			return null;
		}

		public virtual void Refresh(ContentMap map, List<EntityReference> references)
		{
			using (var context = this.CreateContext())
			{
				Refresh(context, map, references);
			}
		}
		
		private static void Refresh(CrmDbContext context, ContentMap map, List<EntityReference> references)
		{
			if (references.Count > 0)
			{
				references[0].ThrowOnNull("reference");
				EntityDefinition ed;
				Dictionary<Guid, Entity> mapEntities = new Dictionary<Guid, Entity>();
				bool getEntityDefinition = map.Solution.Entities.TryGetValue(references[0].LogicalName, out ed);

				if (getEntityDefinition)
				{
					List<Guid> guids = new List<Guid>();
					foreach (var reference in references)
					{
						reference.ThrowOnNull("reference");
						guids.Add(reference.Id);
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(reference.LogicalName), reference.Id));
					}
					try
					{
						string primaryEntityAttribute = EventHubBasedInvalidation.CrmChangeTrackingManager.Instance.TryGetPrimaryKey(references[0].LogicalName);

						var entities = RetrieveCRMRecords(context, primaryEntityAttribute, references[0], ed, guids);
						foreach (var entity in entities)
						{
							mapEntities.Add(entity.Id, entity);
						}

						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Retrieve Multiple Response for Entity {0} has Record Count {1} , Refrence Count {2} ", references[0].LogicalName, entities.Count, references.Count));

						// check if the entity is inactive according to the definition
						foreach (var reference in references)
						{
							var entity = mapEntities.ContainsKey(reference.Id) ? (Entity)mapEntities[reference.Id] : null;

							// Check if the entity matches on the defined relationships.
							if (!ed.ShouldIncludeInContentMap(entity))
							{
								continue;
							}

							var option = entity != null ? entity.GetAttributeValue<OptionSetValue>("statecode") : null;
							var isActive = ed.ActiveStateCode == null || (option != null && ed.ActiveStateCode.Value == option.Value);
							var node = map.Using(ContentMapLockType.Write, () => entity != null
								? isActive
									? map.Replace(entity)
									: map.Deactivate(reference)
								: map.Remove(reference));
						}

					}
					catch (FaultException<OrganizationServiceFault>)
					{
						// an exception occurs when trying to retrieve a non-existing entity
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("An exception occurs when trying to retrieve a non-existing entity"));
					}
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Unknown: logicalName={0}", EntityNamePrivacy.GetEntityName(references[0].LogicalName)));
				}
			}
		}

		private static DataCollection<Entity> RetrieveCRMRecords(CrmDbContext context, string primaryEntityAttribute, EntityReference reference, EntityDefinition ed, List<Guid> guids)
		{
			var fetch = ed.CreateFetch();

			//Make Retrive Multiple Query
			object[] guidArray = guids.Cast<object>().ToArray();

			var condition = new Condition(primaryEntityAttribute, ConditionOperator.In, guidArray);

			if (fetch.Entity.Filters == null || !fetch.Entity.Filters.Any())
			{
				fetch.AddFilter(new Filter
				{
					Conditions = new List<Condition>
					{
						condition
					}
				});
			}
			else
			{
				var firstFilter = fetch.Entity.Filters.FirstOrDefault();
				if (firstFilter.Conditions == null)
				{
					firstFilter.Conditions = new List<Condition>();
				}
				firstFilter.Conditions.Add(condition);
			}

			// retrieve a fresh entity which also acts as a backend validation
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Calling Retrieve Multiple Request for Entity {0} ", EntityNamePrivacy.GetEntityName(reference.LogicalName)));
			RetrieveMultipleResponse responses = (RetrieveMultipleResponse)context.Service.Execute(fetch.ToRetrieveMultipleRequest());
			var entities = responses.EntityCollection.Entities;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Retrieve Multiple Response for Entity {0} has Record Count {1}  ", EntityNamePrivacy.GetEntityName(reference.LogicalName), responses.EntityCollection.Entities.Count));
			return entities;
		}

		public virtual void Associate(ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			using (var context = this.CreateContext())
			{
				Associate(context, map, target, relationship, relatedEntities);
			}
		}

		private static void Associate(CrmDbContext context, ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			target.ThrowOnNull("target");
			relationship.ThrowOnNull("relationship");
			relatedEntities.ThrowOnNull("relatedEntities");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Target: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(target.LogicalName), target.Id));
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Relationship: SchemaName={0}, PrimaryEntityRole={1}", relationship.SchemaName, relationship.PrimaryEntityRole));

			foreach (var entity in relatedEntities)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Related: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id));
			}

			// validate that the relationships do in fact exist by querying for the intersects

			var entities = map.Using(ContentMapLockType.Read, () => RetrieveIntersectEntities(context, map, target, relationship, relatedEntities));

			if (entities != null)
			{
				// add intersect entities to the content map

				map.Using(ContentMapLockType.Write, () => map.AddRange(entities));

				foreach (var added in entities)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Added: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(added.LogicalName), added.Id));
				}
			}
		}

		public virtual void Disassociate(ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			using (var context = this.CreateContext())
			{
				this.Disassociate(context, map, target, relationship, relatedEntities);
			}
		}

		private void Disassociate(CrmDbContext context, ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			target.ThrowOnNull("target");
			relationship.ThrowOnNull("relationship");
			relatedEntities.ThrowOnNull("relatedEntities");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Target: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(target.LogicalName), target.Id));
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Relationship: SchemaName={0}, PrimaryEntityRole={1}", relationship.SchemaName, relationship.PrimaryEntityRole));

			foreach (var entity in relatedEntities)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Related: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id));
			}

			var entities = new List<EntityReference>();

			if (this.EventHubJobSettings.IsEnabled)
			{
				//logic to ignore to get Intersect entity which we already have in eventhub model.
				EntityReference intersectEntity = new EntityReference();
				intersectEntity.LogicalName = target.LogicalName;
				intersectEntity.Id = target.Id;
				entities.Add(intersectEntity);
			}
			else
			{
				// validate that the relationships do in fact not exist by querying for the intersects
				entities = map.Using(ContentMapLockType.Read, () => RetrieveDisassociatedIntersectEntities(context, map, target, relationship, relatedEntities).ToList());
			}

			if (entities != null)
			{
				// add intersect entities to the content map

				map.Using(ContentMapLockType.Write, () => map.RemoveRange(entities));
			}
		}

		private static IEnumerable<Entity> RetrieveIntersectEntities(CrmDbContext context, ContentMap map, EntityReference target, Relationship relationship, IEnumerable<EntityReference> relatedEntities)
		{
			var solution = map.Solution;

			// retrieve the intersect entity definition

			ManyRelationshipDefinition mrd;
			EntityDefinition ed;

			if (solution.ManyRelationships != null
				&& solution.ManyRelationships.TryGetValue(relationship.SchemaName, out mrd)
				&& solution.Entities.TryGetValue(mrd.IntersectEntityname, out ed))
			{
				// build the N:N query to retrieve the intersect entities

				var fetch = ed.CreateFetch();

				// reflexive N:N relationships are not supported

				var targetRelationship = ed.Relationships.Single(r => r.ForeignEntityLogicalname == target.LogicalName);

				var filters = fetch.Entity.Filters.FirstOrDefault() ?? new Filter { Conditions = new List<Condition>() };
				filters.Conditions.Add(new Condition(targetRelationship.ForeignIdAttributeName, ConditionOperator.Equal, target.Id));

				var relatedIds = relatedEntities.Select(related => related.Id).Cast<object>().ToArray();
				var firstRelated = relatedEntities.FirstOrDefault();

				if (firstRelated != null)
				{
					var relatedRelationship = ed.Relationships.Single(r => r.ForeignEntityLogicalname == firstRelated.LogicalName);

					filters.Conditions.Add(new Condition(relatedRelationship.ForeignIdAttributeName, ConditionOperator.In, relatedIds));

					var result = context.Service.RetrieveMultiple(fetch);

					return result.Entities;
				}
			}

			return null;
		}

		private static IEnumerable<EntityReference> RetrieveDisassociatedIntersectEntities(CrmDbContext context, ContentMap map, EntityReference target, Relationship relationship, IEnumerable<EntityReference> relatedEntities)
		{
			var solution = map.Solution;

			// retrieve the intersect entity definition

			ManyRelationshipDefinition mrd;
			EntityDefinition ed;

			if (solution.ManyRelationships != null
				&& solution.ManyRelationships.TryGetValue(relationship.SchemaName, out mrd)
				&& solution.Entities.TryGetValue(mrd.IntersectEntityname, out ed))
			{
				// retrieve the target node

				EntityNode targetNode;

				if (map.TryGetValue(target, out targetNode))
				{
					// retrieve the set of existing relationships for validation purposes

					var entities = RetrieveIntersectEntities(context, map, target, relationship, relatedEntities).Select(e => e.ToEntityReference()).ToList();

					// reflexive N:N relationships are not supported

					var targetRelationship = ed.Relationships.Single(r => r.ForeignEntityLogicalname == target.LogicalName && r.ToMany != null);

					// retrieve the intersect nodes that point to the target

					var intersects = targetRelationship.ToMany(targetNode).ToList();

					foreach (var related in relatedEntities)
					{
						EntityNode relatedNode;

						if (map.TryGetValue(related, out relatedNode))
						{
							var relatedRelationship = ed.Relationships.Single(r => r.ForeignEntityLogicalname == related.LogicalName && r.ToOne != null);

							// filter the intersect nodes that point to the related node (as well as the target)
							// ensure that the intersect does not exist in the collection of retrieved intersects

							var intersectsToRemove = intersects
								.Where(i => Equals(relatedRelationship.ToOne(i), relatedNode))
								.Select(i => i.ToEntityReference())
								.Where(i => entities.All(e => !Equals(e, i)));

							foreach (var intersect in intersectsToRemove)
							{
								yield return intersect;
							}
						}
					}
				}
			}
		}

		void IDisposable.Dispose() { }
	}
}
