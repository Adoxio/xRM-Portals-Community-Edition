/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Entity replication
	/// </summary>
	public abstract class CrmEntityReplication : IReplication
	{
		protected CrmEntityReplication(Entity source, OrganizationServiceContext context, string sourceEntityNameGuard)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (source.LogicalName != sourceEntityNameGuard)
			{
				throw new ArgumentException("Must have entity name {0}.".FormatWith(sourceEntityNameGuard), "source");
			}

			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			Source = source;
			Context = context;
		}

		protected OrganizationServiceContext Context { get; private set; }

		protected Entity Source { get; private set; }

		public virtual void Created() { }

		public virtual void Deleted() { }

		public virtual void Updated() { }

		protected ExecuteWorkflowResult ExecuteWorkflow(string workflowName, string targetEntityName, Guid targetEntityID)
		{
			return ExecuteWorkflow(Context, workflowName, targetEntityName, targetEntityID);
		}

		protected void ExecuteWorkflowOnSourceAndSubscribers(string entityDisplayName, string eventName, string subscriberAssociationName, EntityRole entityRole, string entityPrimaryKeyName)
		{
			var entityName = Source.LogicalName;

			ExecuteWorkflow("Adxstudio.Xrm {0} {1} (Master)".FormatWith(entityDisplayName, eventName), entityName, Source.Id);

			var entity = Context.CreateQuery(entityName).FirstOrDefault(e => e.GetAttributeValue<Guid>(entityPrimaryKeyName) == Source.Id);

			if (entity == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Entity type ""{0}"" with primary key {1} not found.", EntityNamePrivacy.GetEntityName(entityName), Source.Id));

				return;
			}

			var subscribers = entity.GetRelatedEntities(Context, subscriberAssociationName, entityRole);

			foreach (var subscriber in subscribers)
			{
				var subscriberEntityName = subscriber.LogicalName;

				ExecuteWorkflow("Adxstudio.Xrm {0} {1} (Subscriber)".FormatWith(entityDisplayName, eventName), subscriberEntityName, subscriber.Id);
			}
		}

		protected EntityReference GetDefaultPublishingState(EntityReference website)
		{
			if (website == null)
			{
				return null;
			}

			var defaultStateSetting = Context.CreateQuery("adx_sitesetting")
				.FirstOrDefault(s => s.GetAttributeValue<string>("adx_name") == "replication/default_publishing_state/name" && s.GetAttributeValue<EntityReference>("adx_websiteid") == website);

			if (defaultStateSetting == null)
			{
				return null;
			}

			var defaultStateName = defaultStateSetting.GetAttributeValue<string>("adx_value");

			var defaultState = Context.CreateQuery("adx_publishingstate")
				.FirstOrDefault(s => s.GetAttributeValue<string>("adx_name") == defaultStateName && s.GetAttributeValue<EntityReference>("adx_websiteid") == website);

			return defaultState == null ? null : defaultState.ToEntityReference();
		}

		protected ExecuteWorkflowResult ExecuteWorkflow(OrganizationServiceContext context, string workflowName, string targetEntityName, Guid targetEntityID)
		{
			var workflow = context.CreateQuery("workflow")
				.FirstOrDefault(w =>
					w.GetAttributeValue<string>("name") == workflowName &&
					w.GetAttributeValue<string>("primaryentity") == targetEntityName &&
					w.GetAttributeValue<bool?>("ondemand").GetValueOrDefault(false));

			if (workflow == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(@"Workflow named ""{0}"" with primary entity {1} not found.", workflowName, EntityNamePrivacy.GetEntityName(targetEntityName)));

				return new WorkflowNotFoundResult(ResourceManager.GetString("Could_Not_Find_Workflow_With_Name_And_Target_EntityName_Message").FormatWith(workflowName, targetEntityName));
			}

			var request = new OrganizationRequest("ExecuteWorkflow");
			request.Parameters["WorkflowId"] = workflow.GetAttributeValue<Guid>("workflowid");
			request.Parameters["EntityId"] = targetEntityID;

			var response = context.Execute(request);

			return new ExecuteWorkflowResult(ResourceManager.GetString("Workflow_Executed_Successfully_Message").FormatWith(workflowName), response);
		}

		protected class ExecuteWorkflowResult
		{
			public ExecuteWorkflowResult(string description, OrganizationResponse response) : this(description)
			{
				if (response == null)
				{
					throw new ArgumentNullException("response");
				}

				Response = response;
			}

			protected ExecuteWorkflowResult(string description)
			{
				Description = description;
			}

			public string Description { get; private set; }

			public OrganizationResponse Response { get; private set; }

			public virtual bool Success
			{
				get { return true; }
			}
		}

		protected class WorkflowNotFoundResult : ExecuteWorkflowResult
		{
			public WorkflowNotFoundResult(string description) : base(description) { }

			public override bool Success
			{
				get { return false; }
			}
		}
	}
}
