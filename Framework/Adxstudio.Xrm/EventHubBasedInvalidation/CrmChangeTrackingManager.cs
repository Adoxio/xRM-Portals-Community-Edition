/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Adxstudio.Xrm.AspNet;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Services;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
    /// <summary>
    /// Handles the interaction with CRM
    /// </summary>
    internal sealed class CrmChangeTrackingManager
    {
        private Guid organizationId;
        private OrganizationServiceContext organizationServiceContext;
        private static CrmChangeTrackingManager crmChangeTrackingManager;
        private readonly ConcurrentDictionary<string, EntityTrackingInfo> entityInfoList;
        private List<string> searchEnabledEntities = new List<string>();

        /// <summary>
		/// Special cased entities and their corresponding name attributes
		/// </summary>
		private static readonly IDictionary<string, string> targetNames = new Dictionary<string, string>
        {
            { "adx_sitesetting", "adx_name" },
            { "adx_setting", "adx_name" },
        };

		/// <summary>
		/// Keep a local refrence of the Processing Entity table
		/// </summary>
		private Dictionary<string, EntityRecordMessage> processingEntities;

        /// <summary>
        /// Gets the instance of this class
        /// </summary>
        public static CrmChangeTrackingManager Instance
        {
            get
            {
                return CrmChangeTrackingManager.crmChangeTrackingManager ??
                       (CrmChangeTrackingManager.crmChangeTrackingManager = new CrmChangeTrackingManager());
            }
        }

        /// <summary>
        /// private constructor
        /// </summary>
        private CrmChangeTrackingManager()
        {
            this.entityInfoList = new ConcurrentDictionary<string, EntityTrackingInfo>();
        }


        /// <summary>
        /// Update entities with initial timestamp, this will ensure that we are only getting the changes that we need
        /// </summary>
        /// <param name="updatedEntites">Entites that have Added/Modified records</param>
        private void UpdateTimeStamp(Dictionary<string, string> updatedEntites)
        {
            OrderExpression order = new OrderExpression();
            order.AttributeName = "versionnumber";
            order.OrderType = OrderType.Descending;

            var executeMulti = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };

            List<RetrieveMultipleRequest> mRequests = new List<RetrieveMultipleRequest>();

            foreach (KeyValuePair<string, string> entity in updatedEntites)
            {
                if (entity.Value == string.Empty || entity.Value == null)
                {
                    QueryExpression query = new QueryExpression()
                    {
                        EntityName = entity.Key,
                        ColumnSet = new ColumnSet("versionnumber"),
                        PageInfo = new PagingInfo() { Count = 10, PageNumber = 1 }
                    };
                    query.Orders.Add(order);
                    var request = new RetrieveMultipleRequest() { Query = query };
                    mRequests.Add(request);
                }
            }

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Number of timestamp query requests {0}", mRequests.Count.ToString()));

            if (mRequests.Count > 0)
            {
                executeMulti.Requests.AddRange(mRequests);

                var response = (ExecuteMultipleResponse)this.OrganizationServiceContext.Execute(executeMulti);

                foreach (ExecuteMultipleResponseItem item in response.Responses)
                {
                    if (item.Response != null && item.Response.Results.Count > 0)
                    {
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Result count for response {0}", item.Response.Results.Count));

                        EntityCollection collection = item.Response.Results.FirstOrDefault().Value as EntityCollection;

                        if (collection != null && collection.Entities.Count > 0)
                        {
                            int index = collection.Entities.Count - 1;
                            if (collection[index].Attributes.Contains("versionnumber"))
                            {
                                var timestamp = collection[index].Attributes["versionnumber"].ToString();
                                int currentVersionNumber;
                                if (int.TryParse(timestamp, out currentVersionNumber))
                                {
                                    var prevVersionNumber = currentVersionNumber - 1;
                                    timestamp = prevVersionNumber.ToString();
                                }

                                var timestampToken = timestamp + "!" + WebAppConfigurationProvider.AppStartTime;
                                updatedEntites[collection[index].LogicalName] = timestampToken;

                                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Last timestamp token set for Entity {0} {1}", collection[index].LogicalName, timestampToken));
                            }
                        }
                    }
                }
            }

        }


		/// <summary>
		/// Requests the record changed info since the given timeStamp from CRM
		/// </summary>
		/// <param name="entitiesWithLastTimestamp">entities with the timestamp to use to get the delta</param>
		/// <returns>The changed entity information</returns>
		internal TimeBasedChangedData RequestRecordDeltaFromCrm(Dictionary<string, string> entitiesWithLastTimestamp, CrmDbContext context, Guid websiteId)
		{
			try
			{
				//Keep a local refrence of the Processing table
				processingEntities = NotificationUpdateManager.Instance.ProcessingEntitiesTable();

				UpdateTimeStamp(entitiesWithLastTimestamp);
				var request = ExecuteMultipleRequest(entitiesWithLastTimestamp);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Running 'ExecuteMultipleRequest' with {0} requests.", request.Requests.Count().ToString()));

				var entities = string.Join(",", entitiesWithLastTimestamp.Keys);
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Retrieving changes for entities = {0}.", entities));

				var response = (ExecuteMultipleResponse)context.Service.Execute(request);

				if (response == null || response.Responses == null)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Got null response while processing the requests"));
					return null;
				}

				if (response.IsFaulted)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Got faulted response from '{0}' while processing atleast one of the message requests", response.ResponseName));
				}

				var responseCollection = new Dictionary<string, RetrieveEntityChangesResponse>();

				for (var i = 0; i < request.Requests.Count && i < response.Responses.Count; i++)
				{
					var entityChangeRequest = request.Requests[i] as RetrieveEntityChangesRequest;
					var resp = response.Responses[i];

					if (resp.Fault != null)
					{
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("RetrieveEntityChangesRequest faulted for entity '{0}'. Message: '{1}' ", entityChangeRequest.EntityName, resp.Fault.Message));
						continue;
					}
					responseCollection.Add(entityChangeRequest.EntityName, response.Responses[i].Response as RetrieveEntityChangesResponse);
				}

				return ParseBusinessEntityChangesResponse(responseCollection, context, websiteId);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
			}
			return null;
		}

        /// <summary>
        /// Create request for getting RetrieveEntityChanges for entities
        /// </summary>
        /// <param name="entitiesWithLastTimestamp">Entities with last update timestamp, for which we shoud get updated records</param>        
        /// <returns>ExecuteMultipleRequest for executing</returns>
        private ExecuteMultipleRequest ExecuteMultipleRequest(Dictionary<string, string> entitiesWithLastTimestamp)
        {

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Creating request for getting Retrieve entity changes for entities."));

            var requests = entitiesWithLastTimestamp.Select(updatedEntity => new RetrieveEntityChangesRequest()
            {
                EntityName = updatedEntity.Key,
                DataVersion = updatedEntity.Value,
                Columns = GetColumnSet(updatedEntity.Key),
                PageInfo = new PagingInfo()
            }).ToList();

            var requestWithResults = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };
            requestWithResults.Requests.AddRange(requests);

            return requestWithResults;
        }

        /// <summary>
        /// Get Column Set
        /// </summary>
        /// <param name="entityName">entity Name</param>        
        /// <returns>Return column set having only ID column or set it to pull all columns</returns>
        private ColumnSet GetColumnSet(string entityName)
        {
            string primaryKeyAttribute = this.TryGetPrimaryKey(entityName);
            ColumnSet columns = new ColumnSet();
            if (string.IsNullOrEmpty(primaryKeyAttribute))
            {
                columns.AllColumns = true;
            }
            else
            {
                columns.AddColumn(primaryKeyAttribute);
                SetAdditionalColumns(entityName, columns);
            }
            return columns;
        }

        /// <summary>
        /// Set the additional columns. Associate & disassociate message will have related entities. The primary key attribute related to 
        /// related entities also needs to be added into the columns set
        /// </summary>
        /// <param name="entityName">Primary Entity</param>
        /// <param name="columns">Column set</param>        
        private void SetAdditionalColumns(string entityName, ColumnSet columns)
        {
            //check the type of message for the respective entityName
            var message = this.processingEntities[entityName] as AssociateDisassociateMessage;

            //if the message is of AssociateDisassociateMessage type add related entities key attributes also.
            if (message != null)
            {
                //add related entity 1
                AddColumn(columns, message.RelatedEntity1Name);
                //add related entity 2
                AddColumn(columns, message.RelatedEntity2Name);
            }

            //add primary name attribute
            string primaryNameAttribute = this.TryGetPrimaryNameAttribute(entityName);
            if (!string.IsNullOrEmpty(primaryNameAttribute))
            {
                columns.AddColumn(primaryNameAttribute);
            }

			// add website lookup attribute
			EntityTrackingInfo info;
			if (entityInfoList.TryGetValue(entityName, out info) && info.WebsiteLookupAttribute != null)
			{
				columns.AddColumn(info.WebsiteLookupAttribute);
			}
        }

        /// <summary>
        /// Add primary key attribute to column set
        /// </summary>
        /// <param name="columns">column set</param>
        /// <param name="relatedEntityName">related Entity Name</param>
        private void AddColumn(ColumnSet columns, string relatedEntityName)
        {
            string primaryKeyAttribute = this.TryGetPrimaryKey(relatedEntityName);
            if (!string.IsNullOrEmpty(primaryKeyAttribute))
            {
                columns.AddColumn(primaryKeyAttribute);
            }
        }


		/// <summary>
		/// Parse CRM response to get list of updated entities with timestamps and list of updated records
		/// </summary>
		/// <param name="responseCollection">Dictionary for entityName and its RetrieveEntityChange response to parse</param>
		/// <returns>List of updated records</returns>
		private TimeBasedChangedData ParseBusinessEntityChangesResponse(Dictionary<string, RetrieveEntityChangesResponse> responseCollection, CrmDbContext context, Guid websiteId)
		{
			if (responseCollection == null || responseCollection.Count == 0)
			{
				return null;
			}
			var changedData = new TimeBasedChangedData
			{
				UpdatedEntitiesWithLastTimestamp = new Dictionary<string, string>(),
				UpdatedEntityRecords = new List<IChangedItem>()
			};

			foreach (var kvp in responseCollection)
			{
				var entityName = kvp.Key;
				var dataToken = kvp.Value.EntityChanges.DataToken;

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("RetrieveEntityChangesResponse received for entity: {0} with new data token: {1}", entityName, dataToken));
				KeyValuePair<string, string>? entityNameWithTimestamp = new KeyValuePair<string, string>(entityName, dataToken);

				if (changedData.UpdatedEntitiesWithLastTimestamp.ContainsKey(entityNameWithTimestamp.Value.Key))
				{
					continue;
				}
				changedData.UpdatedEntitiesWithLastTimestamp.Add(entityNameWithTimestamp.Value.Key, entityNameWithTimestamp.Value.Value);
			}

			changedData.UpdatedEntityRecords.AddRange(this.GetChangesRelatedToWebsite(responseCollection, context, websiteId));

			return changedData;
		}

		/// <summary>
		/// Get changes that belongs to current website.
		/// </summary>
		/// <param name="responseCollection">Entity changes response collection.</param>
		/// <param name="context">Crm DB context.</param>
		/// <param name="websiteId">Current website id.</param>
		/// <returns>Changes that belongs to website.</returns>
		private IEnumerable<IChangedItem> GetChangesRelatedToWebsite(Dictionary<string, RetrieveEntityChangesResponse> responseCollection, CrmDbContext context, Guid websiteId)
		{
			var changedItemList = new List<IChangedItem>();
			var groupedChanges = responseCollection
				.SelectMany(kvp => kvp.Value.EntityChanges.Changes)
				.GroupBy(change => this.GetEntityIdFromChangeItem(change));

			foreach (var itemGroup in groupedChanges)
			{
				try
				{
					if (this.ChangesBelongsToWebsite(itemGroup, context, websiteId))
					{
						changedItemList.AddRange(itemGroup);
					}
					else
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, 
							$"Changes regarding entity (id: {itemGroup.Key.ToString()}) don't belong to website {websiteId.ToString()}");
					}
				}
				catch (Exception ex)
				{
					WebEventSource.Log.GenericErrorException(ex);
				}
			}

			return changedItemList;
		}

		/// <summary>
		/// Gets entity id from changed item.
		/// </summary>
		/// <param name="item">Changed item.</param>
		/// <returns></returns>
		private Guid GetEntityIdFromChangeItem(IChangedItem item)
		{
			return item.Type == ChangeType.NewOrUpdated
				? (item as NewOrUpdatedItem).NewOrUpdatedEntity.Id
				: (item as RemovedOrDeletedItem).RemovedItem.Id;
		}

		/// <summary>
		/// Gets entity name from changed item.
		/// </summary>
		/// <param name="item">Changed item.</param>
		/// <returns>Casted entity.</returns>
		private string GetEntityNameFromChangedItem(IChangedItem item)
		{
			return item.Type == ChangeType.NewOrUpdated
				? (item as NewOrUpdatedItem).NewOrUpdatedEntity.LogicalName
				: (item as RemovedOrDeletedItem).RemovedItem.LogicalName;
		}

		/// <summary>
		/// Check that changes grouped by entity id belongs to website.
		/// </summary>
		/// <param name="groupedChanges">Changes grouped by entity id.</param>
		/// <param name="context">Crm DB context.</param>
		/// <param name="websiteId">Website id.</param>
		/// <returns></returns>
		private bool ChangesBelongsToWebsite(IGrouping<Guid, IChangedItem> groupedChanges, CrmDbContext context, Guid websiteId)
		{
			var entityId = groupedChanges.Key;
			var entityName = this.GetEntityNameFromChangedItem(groupedChanges.First());

			if (string.Equals("adx_website", entityName, StringComparison.OrdinalIgnoreCase))
			{
				return websiteId == entityId;
			}

			// if entity hasn't relationship with website or entity was deleted -> mark as `belongs to website`
			EntityTrackingInfo info;
			if (groupedChanges.Any(gc => gc.Type == ChangeType.RemoveOrDeleted)
				|| !entityInfoList.TryGetValue(entityName, out info) 
				|| info.WebsiteLookupAttribute == null)
			{
				return true;
			}
			
			// trying to get website's id from changed items 
			var itemWithWebsiteIdValue = groupedChanges
				.OfType<NewOrUpdatedItem>()
				.FirstOrDefault(item => item.NewOrUpdatedEntity.Contains(info.WebsiteLookupAttribute));

			// if all changes doesn't contain website lookup attribute but we know that entity should have it then try to get value from service context
			var updatedEntity = itemWithWebsiteIdValue != null
				? itemWithWebsiteIdValue.NewOrUpdatedEntity
				: context.Service.RetrieveSingle(new EntityReference(entityName, entityId), new ColumnSet(info.WebsiteLookupAttribute));

			return updatedEntity?.GetAttributeValue<EntityReference>(info.WebsiteLookupAttribute)?.Id == websiteId;
		}

        /// <summary>
        /// Returns the primary key for the given entity
        /// </summary>
        /// <param name="entityName">entity to get the primary key for</param>
        /// <returns>primary key attribute name if found, otherwise null</returns>
        internal string TryGetPrimaryKey(string entityName)
        {
            EntityTrackingInfo trackingInfo = null;

            // if entity name is null return;
            if (string.IsNullOrEmpty(entityName))
            {
                ADXTrace.Instance.TraceError(TraceCategory.Application, "TryGetPrimaryKey: the entity name is null");
				return null;
            }
            else if (!entityInfoList.TryGetValue(entityName, out trackingInfo))
            {
				var entityTrackingInfo = this.GetWebsiteLookupEntityTrackingInfo(entityName);
				if (entityTrackingInfo != null)
				{
					entityInfoList.AddOrUpdate(entityName, entityTrackingInfo, (name, info) => entityTrackingInfo);
					return entityTrackingInfo.EntityKeyAttribute;
				}
				return null;
            }

            return trackingInfo.EntityKeyAttribute;
        }

		/// <summary>
		/// Get <see cref="EntityTrackingInfo"/> for entity.
		/// </summary>
		/// <param name="entityName">Name of entity.</param>
		/// <returns>Info about entity or null.</returns>
		private EntityTrackingInfo GetWebsiteLookupEntityTrackingInfo(string entityName)
		{
			// ignore relationships for adx_website entity
			var isWebsiteEntity = string.Equals(entityName, "adx_website", StringComparison.OrdinalIgnoreCase);
			var metadata = isWebsiteEntity
				? this.OrganizationServiceContext.GetEntityMetadata(entityName)
				: this.OrganizationServiceContext.GetEntityMetadata(entityName, EntityFilters.Attributes | EntityFilters.Relationships);

			if (metadata == null)
			{
				return null;
			}

			// try get relationship to a website
			var websiteRelationship = isWebsiteEntity
				? null
				: metadata.ManyToOneRelationships.FirstOrDefault(r => string.Equals(r.ReferencedEntity, "adx_website", StringComparison.OrdinalIgnoreCase));
			
			return new EntityTrackingInfo
			{
				EntityKeyAttribute = metadata.PrimaryIdAttribute,
				WebsiteLookupAttribute = websiteRelationship?.ReferencingAttribute.Equals("adx_websiteid", StringComparison.CurrentCultureIgnoreCase) == true
					? "adx_websiteid"
					: null
			};
		}

        /// <summary>
		/// Get the Name attribute of the entity.
		/// </summary>
		/// <param name="entityName">Entity to get the name </param>
		/// <returns>Entity's Name attribute</returns>
		private string TryGetPrimaryNameAttribute(string entityName)
        {
            // for particular entities, return the value of the primary name attribute
            string name;
            return targetNames.TryGetValue(entityName, out name) ? name : null;
        }

        /// <summary>
        /// Returns an instance of the OrganizationServiceContext
        /// </summary>
        public OrganizationServiceContext OrganizationServiceContext
        {
            get { return this.organizationServiceContext ?? (this.organizationServiceContext = PortalCrmConfigurationManager.CreateServiceContext()); }
        }

        /// <summary>
        /// Gets the Organization Id
        /// </summary>
        /// <returns>Organization Id</returns>
        public Guid OrganizationId
        {
            get
            {
                return this.organizationId = ((WhoAmIResponse)this.OrganizationServiceContext.Execute(new WhoAmIRequest())).OrganizationId;
            }
        }
    }
}
