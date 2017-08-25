/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Crm.Sdk.Messages;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;

	public enum WorkflowState
	{
		Draft = 0,
		Published = 1,
	}

	public static class OrganizationServiceContextExtensions
	{
		/// <summary>
		/// Retrieves an active workflow entity by name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="workflowName"></param>
		/// <returns></returns>
		public static Entity GetWorkflowByName(this OrganizationServiceContext context, string workflowName)
		{
			var workflow = context.CreateQuery("workflow").SingleOrDefault(
				wf => wf.GetAttributeValue<OptionSetValue>("statecode") != null && wf.GetAttributeValue<OptionSetValue>("statecode").Value == (int)WorkflowState.Published
					&& wf.GetAttributeValue<OptionSetValue>("type") != null && wf.GetAttributeValue<OptionSetValue>("type").Value == 1
					&& wf.GetAttributeValue<string>("name") == workflowName);

			return workflow;
		}

		/// <summary>
		/// Executes an active workflow by name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="workflowName"></param>
		/// <param name="entityId"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static ExecuteWorkflowResponse ExecuteWorkflowByName(this OrganizationServiceContext context, string workflowName, Guid entityId)
		{
			var workflow = context.GetWorkflowByName(workflowName);

			if (workflow == null)
			{
				throw new InvalidOperationException("Unable to find the '{0}' workflow.".FormatWith(workflowName));
			}

			var request = new ExecuteWorkflowRequest
			{
				EntityId = entityId,
				WorkflowId = workflow.Id,
			};

			return (ExecuteWorkflowResponse)context.Execute(request);
		}

		#region Retrive

		/// <summary>Retrieves a single entity or throws an exception if the entity does not exist.</summary>
		/// <param name="context">The context.</param>
		/// <param name="target">The target entity.</param>
		/// <param name="columnSet">The attributes to select.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The expiration.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			EntityReference target,
			ColumnSet columnSet,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(target, columnSet, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="context">The context.</param>
		/// <param name="query">A query that returns a single entity.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			QueryBase query,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(query, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves a single entity or null.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="fetch">A query that returns a single entity.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			Fetch fetch,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(fetch, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves a single entity or null.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="condition">The fetch condition.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			string logicalName,
			IEnumerable<string> columns,
			Condition condition,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(logicalName, columns, condition, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves a single entity or null.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(logicalName, columns, conditions, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves a single entity or null.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="condition">The fetch condition.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			string logicalName,
			ICollection<FetchAttribute> attributes,
			Condition condition,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(logicalName, attributes, condition, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves a single entity or null.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			string logicalName,
			ICollection<FetchAttribute> attributes,
			ICollection<Condition> conditions,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(logicalName, attributes, conditions, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>Retrieves a single entity or null.</summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="primaryAttribute">The primary Attribute.</param>
		/// <param name="id">The id.</param>
		/// <param name="attributes">The columns.</param>
		/// <param name="enforceSingle">Enforce only a single match returns a result and multiple matches returns null.</param>
		/// <param name="enforceFirst">Enforce a non empty result.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static Entity RetrieveSingle(
			this OrganizationServiceContext context,
			string logicalName,
			string primaryAttribute,
			Guid id,
			ICollection<FetchAttribute> attributes,
			bool enforceSingle = false,
			bool enforceFirst = false,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveSingle(logicalName, primaryAttribute, id, attributes, enforceSingle, enforceFirst, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves entities.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static EntityCollection RetrieveMultiple(
			this OrganizationServiceContext context,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveMultiple(logicalName, columns, conditions, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves entities.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="fetch">The fetch query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="expiration">The timespan from execute time to expire from cache.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entities.</returns>
		public static EntityCollection RetrieveMultiple(
			this OrganizationServiceContext context,
			Fetch fetch,
			RequestFlag flag = RequestFlag.None,
			TimeSpan? expiration = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveMultiple(fetch, flag, expiration, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves all entities by paging results.
		/// </summary>
		/// <remarks>
		/// Breaking out of the <see cref="IEnumerable{Entity}"/> early terminates retrieval of further pages.
		/// </remarks>
		/// <param name="context">The context.</param>
		/// <param name="logicalName">The entity type.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="conditions">The fetch conditions.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entity.</returns>
		public static IEnumerable<Entity> RetrieveAll(
			this OrganizationServiceContext context,
			string logicalName,
			IEnumerable<string> columns,
			ICollection<Condition> conditions,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveAll(logicalName, columns, conditions, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves all entities by paging results.
		/// </summary>
		/// <remarks>
		/// Breaking out of the <see cref="IEnumerable{Entity}"/> early terminates retrieval of further pages.
		/// </remarks>
		/// <param name="context">The context.</param>
		/// <param name="fetch">The fetch query.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The entities.</returns>
		public static IEnumerable<Entity> RetrieveAll(
			this OrganizationServiceContext context,
			Fetch fetch,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveAll(fetch, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		#endregion

		#region RetrieveRelated

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this OrganizationServiceContext context,
			Entity entity,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveRelatedEntity(entity, relationshipSchemaName, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves an entity related to the target entity.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entity.</returns>
		public static Entity RetrieveRelatedEntity(
			this OrganizationServiceContext context,
			Entity entity,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveRelatedEntity(entity, relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationshipSchemaName">The relationship schema name.</param>
		/// <param name="columns">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this OrganizationServiceContext context,
			Entity entity,
			string relationshipSchemaName,
			IEnumerable<string> columns = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveRelatedEntities(entity, relationshipSchemaName, columns, filters, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		/// <summary>
		/// Retrieves entities related to the target entity.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="entity">The target entity.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="attributes">The attributes to select.</param>
		/// <param name="filters">The filters on the related entities.</param>
		/// <param name="flag">Request flag to specify request properties - like BypassCacheInvalidation/AllowStaleData.</param>
		/// <param name="memberName">The parameter is not used.</param>
		/// <param name="sourceFilePath">The parameter is not used.</param>
		/// <param name="sourceLineNumber">The parameter is not used.</param>
		/// <returns>The related entities.</returns>
		public static EntityCollection RetrieveRelatedEntities(
			this OrganizationServiceContext context,
			Entity entity,
			Relationship relationship,
			ICollection<FetchAttribute> attributes = null,
			ICollection<Filter> filters = null,
			RequestFlag flag = RequestFlag.None,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return (context as IOrganizationService).RetrieveRelatedEntities(entity, relationship, attributes, filters, flag, memberName, sourceFilePath, sourceLineNumber).AttachTo(context);
		}

		#endregion

		#region AttachTo Members

		public static IEnumerable<Entity> AttachTo(this IEnumerable<EntityNode> nodes, OrganizationServiceContext context, bool includeRelatedEntities = false)
		{
			return nodes.Select(node => AttachTo(node, context, includeRelatedEntities));
		}

		public static IEnumerable<Entity> AttachTo(this IEnumerable<CrmSiteMapNode> nodes, OrganizationServiceContext context, bool includeRelatedEntities = false)
		{
			return nodes.Select(node => AttachTo(node, context, includeRelatedEntities));
		}

		public static EntityCollection AttachTo(this EntityCollection entities, OrganizationServiceContext context)
		{
			foreach (var entity in entities.Entities)
			{
				AttachTo(entity, context);
			}

			return entities;
		}

		public static IEnumerable<Entity> AttachTo(this IEnumerable<Entity> entities, OrganizationServiceContext context)
		{
			return entities.Select(entity => AttachTo(entity, context));
		}

		public static Entity AttachTo(this EntityNode node, OrganizationServiceContext context, bool includeRelatedEntities = false)
		{
			if (node == null)
			{
				return null;
			}

			return MergeClone(context, node.ToEntity(), includeRelatedEntities);
		}

		public static Entity AttachTo(this CrmSiteMapNode node, OrganizationServiceContext context, bool includeRelatedEntities = false)
		{
			if (node == null)
			{
				return null;
			}

			return MergeClone(context, node.Entity, includeRelatedEntities);
		}

		public static Entity AttachTo(this Entity entity, OrganizationServiceContext context)
		{
			if (entity == null)
			{
				return null;
			}

			// aggregate queries return blank entities

			if (entity.Id == Guid.Empty)
			{
				return entity;
			}

			var attached = context.GetAttachedEntities().FirstOrDefault(e => e.Id == entity.Id);

			if (attached != null)
			{
				// use the existing entity instead
				return attached;
			}

			context.ReAttach(entity);

			return entity;
		}

		private static Entity MergeClone(OrganizationServiceContext context, Entity entity, bool includeRelatedEntities = false)
		{
			if (entity == null)
			{
				return null;
			}

			// aggregate queries return blank entities

			if (entity.Id == Guid.Empty)
			{
				return entity.Clone(includeRelatedEntities);
			}

			var attached = context.GetAttachedEntities().FirstOrDefault(e => e.Id == entity.Id);

			if (attached != null)
			{
				// use the existing entity instead
				return attached;
			}

			return context.AttachClone(entity, includeRelatedEntities);
		}

		#endregion
	}
}
