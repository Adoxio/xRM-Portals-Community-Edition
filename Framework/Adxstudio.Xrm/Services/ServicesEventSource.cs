/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using System.Linq;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;

	[EventSource(Guid = "76032881-E1AD-481D-80C9-3157C0E5B8BC", Name = InternalName)]
	internal sealed class ServicesEventSource : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalServices";

		/// <summary>
		/// The internal TraceSource.
		/// </summary>
		private static readonly TraceSource InternalTrace = new TraceSource(InternalName);

		/// <summary>
		/// The variable for lazy initialization of <see cref="ServicesEventSource"/>.
		/// </summary>
		private static readonly Lazy<ServicesEventSource> _instance = new Lazy<ServicesEventSource>();

		public static ServicesEventSource Log
		{
			get { return _instance.Value; }
		}

		private enum EventName
		{
			/// <summary>
			/// Application has failed to connect to CRM.
			/// </summary>
			UnableToConnect = 1,

			/// <summary>
			/// <see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/> to be executed.
			/// </summary>
			OrganizationRequest = 2,

			/// <summary>
			/// Request to create a record.
			/// </summary>
			Create = 3,

			/// <summary>
			/// Request to read a record.
			/// </summary>
			Retrieve = 4,

			/// <summary>
			/// Request to update a record.
			/// </summary>
			Update = 5,

			/// <summary>
			/// Request to delete a record.
			/// </summary>
			Delete = 6,

			/// <summary>
			/// Request to read one or more records.
			/// </summary>
			RetrieveMultiple = 7,

			/// <summary>
			/// Request to associate a record to another.
			/// </summary>
			Associate = 8,

			/// <summary>
			/// Request to disassociate a record from another.
			/// </summary>
			Disassociate = 9
		}

		/// <summary>
		/// Log execution of an associate of an <see cref="Microsoft.Xrm.Sdk.Entity"/> record to other record(s).
		/// </summary>
		/// <param name="entityLogicalName">Logical name of the entity record.</param>
		/// <param name="entityId">Uniqued ID of the record.</param>
		/// <param name="relationship"><see cref="Microsoft.Xrm.Sdk.Relationship"/></param>
		/// <param name="relatedEntities"><see cref="Microsoft.Xrm.Sdk.EntityReferenceCollection"/></param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Associate(string entityLogicalName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, long duration)
		{
			if (relationship == null || relatedEntities == null || relatedEntities.Count <= 0)
			{
				return GetActivityId();
			}

			var relatedEntitiesString = string.Join(";",
				relatedEntities.ToArray().Select(o => string.Format("{0}:{1}", o.LogicalName, o.Id)));

			WriteEventAssociate(
				entityLogicalName,
				entityId.ToString(),
				relationship.SchemaName,
				relatedEntitiesString,
				duration,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Associate, Message = "Entity Logical Name : {0} Entity ID : {1} Relationship Name : {2} Related Entities : {3} Duration : {4} PortalUrl : {5} PortalVersion : {6} PortalProductionOrTrialType : {7} SessionId : {8} ElapsedTime : {9}", Level = EventLevel.Informational, Version = 4)]
		private void WriteEventAssociate(string entityLogicalName, string entityId, string relationshipName, string relatedEntities, long duration, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.Associate,
				"Entity Logical Name : {0} Entity ID : {1} Relationship Name : {2} Related Entities : {3} Duration : {4}",
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				relationshipName ?? string.Empty,
				relatedEntities ?? string.Empty,
				duration);

			WriteEvent(
				EventName.Associate,
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				relationshipName ?? string.Empty,
				relatedEntities ?? string.Empty,
				duration,
				portalUrl,
				portalVersion,
				portalProductionOrTrialType,
				sessionId,
				elapsedTime);
		}

		/// <summary>
		/// Log execution of create of an <see cref="Microsoft.Xrm.Sdk.Entity"/> record.
		/// </summary>
		/// <param name="entity"><see cref="Microsoft.Xrm.Sdk.Entity"/> record to create.</param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Create(Entity entity, long duration)
		{
			if (entity == null)
			{
				return GetActivityId();
			}

			WriteEventCreate(
				entity.LogicalName,
				duration,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Create, Message = "Entity Logical Name : {0} Duration : {1} PortalUrl : {2} PortalVersion : {3} PortalProductionOrTrialType : {4} SessionId : {8} ElapsedTime : {9}", Level = EventLevel.Informational, Version = 4)]
		private void WriteEventCreate(string entityLogicalName, long duration, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.Create,
				"Entity Logical Name : {0} Duration : {1}",
				entityLogicalName ?? string.Empty,
				duration);

			WriteEvent(EventName.Create, entityLogicalName ?? string.Empty, duration, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log execution of a delete of an <see cref="Microsoft.Xrm.Sdk.Entity"/> record.
		/// </summary>
		/// <param name="entityLogicalName">Logical name of the entity record.</param>
		/// <param name="entityId">Uniqued ID of the record.</param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Delete(string entityLogicalName, Guid entityId, long duration)
		{
			WriteEventDelete(
				entityLogicalName,
				entityId.ToString(),
				duration,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Delete, Message = "Entity Logical Name : {0} Entity ID : {1} Duration : {2} PortalUrl : {3} PortalVersion : {4} PortalProductionOrTrialType : {5} SessionId : {6} ElapsedTime : {7}", Level = EventLevel.Informational, Version = 4)]
		private void WriteEventDelete(string entityLogicalName, string entityId, long duration, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.Delete,
				"Entity Logical Name : {0} Entity ID : {1} Duration : {2}",
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				duration);

			WriteEvent(EventName.Delete, entityLogicalName ?? string.Empty, entityId ?? string.Empty, duration, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log execution of disassociate of an <see cref="Microsoft.Xrm.Sdk.Entity"/> record from other record(s).
		/// </summary>
		/// <param name="entityLogicalName">Logical name of the entity record.</param>
		/// <param name="entityId">Uniqued ID of the record.</param>
		/// <param name="relationship"><see cref="Microsoft.Xrm.Sdk.Relationship"/></param>
		/// <param name="relatedEntities"><see cref="Microsoft.Xrm.Sdk.EntityReferenceCollection"/></param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Disassociate(string entityLogicalName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, long duration)
		{
			if (relationship == null || relatedEntities == null || relatedEntities.Count <= 0)
			{
				return GetActivityId();
			}

			var relatedEntitiesString = string.Join(";",
				relatedEntities.ToArray().Select(o => string.Format("{0}:{1}", o.LogicalName, o.Id)));

			WriteEventDisassociate(
				entityLogicalName,
				entityId.ToString(),
				relationship.SchemaName,
				relatedEntitiesString,
				duration,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Disassociate, Message = "Entity Logical Name : {0} Entity ID : {1} Relationship Name : {2} Related Entities : {3} Duration : {4} PortalUrl : {5} PortalVersion : {6} PortalProductionOrTrialType : {7} SessionId : {8} ElapsedTime : {9}", Level = EventLevel.Informational, Version = 4)]
		private void WriteEventDisassociate(string entityLogicalName, string entityId, string relationshipName, string relatedEntities, long duration, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.Disassociate,
				"Entity Logical Name : {0} Entity ID : {1} Relationship Name : {2} Related Entities : {3} Duration : {4}",
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				relationshipName ?? string.Empty,
				relatedEntities ?? string.Empty,
				duration);

			WriteEvent(EventName.Disassociate,
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				relationshipName ?? string.Empty,
				relatedEntities ?? string.Empty,
				duration,
				portalUrl,
				portalVersion,
				portalProductionOrTrialType,
				sessionId,
				elapsedTime);
		}

		/// <summary>
		/// Log execution of an <see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/>.
		/// </summary>
		/// <param name="request"><see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/> to be executed.</param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <param name="cached">Indicates a cached request.</param>
		/// <param name="telemetry">Additional telemetry.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string OrganizationRequest(OrganizationRequest request, long duration, bool cached, CacheItemTelemetry telemetry = null)
		{
			if (request == null)
			{
				return GetActivityId();
			}

			var cachedRequest = request as CachedOrganizationRequest;

			if (cachedRequest != null)
			{
				return this.OrganizationRequest(cachedRequest.Request, duration, cached, cachedRequest.Telemetry);
			}

			var retrieveMultiple = request as RetrieveMultipleRequest;

			if (retrieveMultiple != null)
			{
				return RetrieveMultiple(retrieveMultiple.Query, duration, cached, telemetry);
			}

			var fetchMultiple = request as FetchMultipleRequest;

			if (fetchMultiple != null)
			{
				return RetrieveMultiple(fetchMultiple.Query, duration, cached, telemetry);
			}

			var retrieveSingle = request as RetrieveSingleRequest;

			if (retrieveSingle != null)
			{
				return RetrieveMultiple(retrieveSingle.Query, duration, cached, telemetry);
			}

			var retrieve = request as RetrieveRequest;

			if (retrieve != null && retrieve.Target != null)
			{
				return Retrieve(retrieve.Target.LogicalName, retrieve.Target.Id, retrieve.ColumnSet, duration, cached, telemetry);
			}

			var create = request as CreateRequest;

			if (create != null && create.Target != null)
			{
				return Create(new Entity(create.Target.LogicalName) { Id = create.Target.Id }, duration);
			}

			var update = request as UpdateRequest;

			if (update != null && update.Target != null)
			{
				return Update(new Entity(update.Target.LogicalName) { Id = update.Target.Id }, duration);
			}

			var delete = request as DeleteRequest;

			if (delete != null && delete.Target != null)
			{
				return Delete(delete.Target.LogicalName, delete.Target.Id, duration);
			}

			var associate = request as AssociateRequest;

			if (associate != null && associate.Target != null)
			{
				return Associate(associate.Target.LogicalName, associate.Target.Id, associate.Relationship, associate.RelatedEntities, duration);
			}

			var disassociate = request as DisassociateRequest;

			if (disassociate != null && disassociate.Target != null)
			{
				return Disassociate(disassociate.Target.LogicalName, disassociate.Target.Id, disassociate.Relationship, disassociate.RelatedEntities, duration);
			}

			WriteEventOrganizationRequest(
				request.RequestName,
				request.RequestId.HasValue ? request.RequestId.Value.ToString() : null,
				duration,
				cached,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime(),
				telemetry?.Caller.MemberName,
				telemetry?.Caller.SourceFilePath,
				telemetry?.Caller.SourceLineNumber ?? 0);

			return GetActivityId();
		}

		[Event((int)EventName.OrganizationRequest, Message = "Request Name : {0} Request ID : {1} Duration : {2} Cached : {3} PortalUrl : {4} PortalVersion : {5} PortalProductionOrTrialType : {6} SessionId : {7} ElapsedTime : {8} MemberName : {9} SourceFilePath : {10} SourceLineNumber : {11}", Level = EventLevel.Informational, Version = 5)]
		private void WriteEventOrganizationRequest(string requestName, string requestId, long duration, bool cached, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.OrganizationRequest,
				"Request Name : {0} Request ID : {1} Duration : {2} Cached : {3} MemberName: {4} SourceFilePath : {5} SourceLineNumber : {6}",
				requestName ?? string.Empty,
				requestId ?? string.Empty,
				duration,
				cached,
				memberName,
				sourceFilePath,
				sourceLineNumber);

			WriteEvent(EventName.OrganizationRequest, requestName ?? string.Empty, requestId ?? string.Empty, duration, cached, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Log execution of a retrieve of an <see cref="Microsoft.Xrm.Sdk.Entity"/> record.
		/// </summary>
		/// <param name="entityLogicalName">Logical name of the entity record.</param>
		/// <param name="entityId">Uniqued ID of the record.</param>
		/// <param name="columnSet"><see cref="Microsoft.Xrm.Sdk.Query.ColumnSet"/></param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <param name="cached">Indicates a cached request.</param>
		/// <param name="telemetry">Additional telemetry.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Retrieve(string entityLogicalName, Guid entityId, ColumnSet columnSet, long duration, bool cached, CacheItemTelemetry telemetry)
		{
			var columns = string.Empty;

			if (columnSet != null && columnSet.Columns != null && columnSet.Columns.Count > 0)
			{
				columns = string.Join(";", columnSet.Columns.ToArray());
			}

			WriteEventRetrieve(
				entityLogicalName,
				entityId.ToString(),
				columns,
				duration,
				cached,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime(),
				telemetry?.IsAllColumns ?? false,
				telemetry?.Caller.MemberName,
				telemetry?.Caller.SourceFilePath,
				telemetry?.Caller.SourceLineNumber ?? 0);

			return GetActivityId();
		}

		[Event((int)EventName.Retrieve, Message = "Entity Logical Name : {0} Entity ID : {1} ColumnSet : {2} Duration : {3} Cached : {4} PortalUrl : {5} PortalVersion : {6} PortalProductionOrTrialType : {7} SessionId : {8} ElapsedTime : {9} AllColumns : {10} MemberName : {11} SourceFilePath : {12} SourceLineNumber : {13}", Level = EventLevel.Informational, Version = 6)]
		private void WriteEventRetrieve(string entityLogicalName, string entityId, string columnSet, long duration, bool cached, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime, bool allColumns, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.Retrieve,
				"Entity Logical Name : {0} Entity ID : {1} ColumnSet : {2} Duration : {3} Cached : {4} AllColumns : {5} MemberName: {6} SourceFilePath : {7} SourceLineNumber : {8}",
				entityLogicalName ?? string.Empty,
				entityId ?? string.Empty,
				columnSet ?? string.Empty,
				duration,
				cached,
				allColumns,
				memberName,
				sourceFilePath,
				sourceLineNumber);

			WriteEvent(EventName.Retrieve, entityLogicalName ?? string.Empty, entityId ?? string.Empty, columnSet ?? string.Empty, duration, cached, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime, allColumns, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Log execution of a retrieve multiple of <see cref="Microsoft.Xrm.Sdk.Entity"/> records.
		/// </summary>
		/// <param name="query"><see cref="Microsoft.Xrm.Sdk.Query.QueryBase"/> query to be executed.</param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <param name="cached">Indicates a cached request.</param>
		/// <param name="telemetry">Additional telemetry.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string RetrieveMultiple(QueryBase query, long duration, bool cached, CacheItemTelemetry telemetry)
		{
			if (query == null)
			{
				return GetActivityId();
			}

			var queryInfo = string.Empty;
			var fetchExpression = query as FetchExpression;
			var queryExpression = query as QueryExpression;
			var queryByAttribute = query as QueryByAttribute;

			if (fetchExpression != null && fetchExpression.Query != null)
			{
				queryInfo = fetchExpression.Query;
			}

			if (queryExpression != null)
			{
				queryInfo = queryExpression.EntityName;
			}

			if (queryByAttribute != null)
			{
				queryInfo = queryByAttribute.EntityName;
			}

			WriteEventRetrieveMultiple(
				queryInfo,
				duration,
				cached,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime(),
				telemetry?.IsAllColumns ?? false,
				telemetry?.Caller.MemberName,
				telemetry?.Caller.SourceFilePath,
				telemetry?.Caller.SourceLineNumber ?? 0);

			return GetActivityId();
		}

		[Event((int)EventName.RetrieveMultiple, Message = "Query Info : {0} Duration : {1} Cached : {2} PortalUrl : {3} PortalVersion : {4} PortalProductionOrTrialType : {5} SessionId : {6} ElapsedTime : {7} AllColumns : {8} MemberName : {9} SourceFilePath : {10} SourceLineNumber : {11}", Level = EventLevel.Informational, Version = 6)]
		private void WriteEventRetrieveMultiple(string queryInfo, long duration, bool cached, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime, bool allColumns, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information,
				(int)EventName.RetrieveMultiple,
				"Query Info : {0} Duration : {1} Cached : {2} Allcolumns : {3} MemberName: {4} SourceFilePath : {5} SourceLineNumber : {6}",
				queryInfo ?? string.Empty,
				duration,
				cached,
				allColumns,
				memberName,
				sourceFilePath,
				sourceLineNumber);

			WriteEvent(EventName.RetrieveMultiple, queryInfo ?? string.Empty, duration, cached, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime, allColumns, memberName, sourceFilePath, sourceLineNumber);
		}

		/// <summary>
		/// Log execution of an update of <see cref="Microsoft.Xrm.Sdk.Entity"/> record.
		/// </summary>
		/// <param name="entity"><see cref="Microsoft.Xrm.Sdk.Entity"/> record to update.</param>
		/// <param name="duration">The duration in milliseconds.</param>
		/// <returns>Unique ID of event</returns>
		[NonEvent]
		public string Update(Entity entity, long duration)
		{
			if (entity == null)
			{
				return GetActivityId();
			}

			WriteEventUpdate(
				entity.LogicalName,
				entity.Id,
				duration,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());

			return GetActivityId();
		}

		[Event((int)EventName.Update, Message = "Entity Logical Name : {0} Entity ID : {1} Duration : {2} PortalUrl : {3} PortalVersion : {4} PortalProductionOrTrialType : {5} SessionId : {6} ElapsedTime : {7}", Level = EventLevel.Informational, Version = 4)]
		private void WriteEventUpdate(string entityLogicalName, Guid entityId, long duration, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance,
				TraceEventType.Information, (int)EventName.Update,
				"Entity Logical Name : {0} Entity ID : {1} Duration : {2}",
				entityLogicalName ?? string.Empty, entityId, duration);

			WriteEvent(EventName.Update, entityLogicalName ?? string.Empty, entityId, duration, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}
	}
}
