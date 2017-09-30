/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Search.Index;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.EventHubBasedInvalidation;
using Adxstudio.Xrm.Search.Facets;
using Adxstudio.Xrm.Services;
using Lucene.Net.Store;
using Microsoft.Xrm.Client.Services.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Adxstudio.Xrm.Services.Query;
using Filter = Adxstudio.Xrm.Services.Query.Filter;

namespace Adxstudio.Xrm.Search
{
	public class SearchIndexBuildRequest
	{
		private static readonly IEnumerable<string> BuildMessages = new[] { "Build", "Publish", "PublishAll" };
		private static readonly IEnumerable<string> DeleteMessages = new[] { "Delete" };
		private static readonly IEnumerable<string> UpdateMessages = new[] { "Create", "Update" };
        private static readonly IEnumerable<string> AssociateDisassociateMessages = new[] { "Associate", "Disassociate" };
        private static readonly IEqualityComparer<string> MessageComparer = StringComparer.InvariantCultureIgnoreCase;
		private const int KnowledgeArticleObjectTypeCode = 9953;

		/// <summary>
		/// Process the message from the HTTP POST request and rebuild the search index.
		/// </summary>
		/// <param name="message"></param>
		public static void ProcessMessage(OrganizationServiceCachePluginMessage message, SearchIndexInvalidationData searchIndexInvalidationData = null, OrganizationServiceContext serviceContext = null)
		{
			if (!SearchManager.Enabled)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Search isn't enabled for the current application.");

				return;
			}

			if (message == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Search Index build failure. Plugin Message is null.");

				return;
			}

			SearchProvider provider;
			var forceBuild = message.Target != null && message.Target.LogicalName == "adx_website";

			if (!TryGetSearchProvider(out provider))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Search Index build failure. Search Provider could not be found.");

				return;
			}
			
			if (forceBuild || BuildMessages.Contains(message.MessageName, MessageComparer))
			{
				PerformBuild(provider);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Search Index was successfully built.");

				return;
			}

            IContentMapProvider contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider();

            if (UpdateMessages.Contains(message.MessageName, MessageComparer))
			{
				if (message.Target == null || string.IsNullOrEmpty(message.Target.LogicalName))
				{
					throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an EntityName (entity logical name) parameter.", message.MessageName));
				}

				if (message.Target == null || message.Target.Id == Guid.Empty)
				{
					throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an ID (entity ID) parameter.", message.MessageName));
				}

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
				{
					if (message.Target.LogicalName == "savedquery")
					{
						var cachedSavedQuery = SearchMetadataCache.Instance.SearchSavedQueries.FirstOrDefault(x => x.SavedQueryId == message.Target.Id);
						if (cachedSavedQuery != null)
						{
							// SavedQueryUniqueId is timestamp to verify which change was published and need to invalidate Search Index 
							var actualSavedQueryUniqueId = SearchMetadataCache.Instance.GetSavedQueryUniqueId(message.Target.Id);
							if (cachedSavedQuery.SavedQueryIdUnique != actualSavedQueryUniqueId)
							{
								PerformUpdateAsync(provider, updater => updater.UpdateEntitySet(cachedSavedQuery.EntityName));
								SearchMetadataCache.Instance.CompleteMetadataUpdateForSearchSavedQuery(cachedSavedQuery, actualSavedQueryUniqueId);
							}
						}
					}

					if (message.Target.LogicalName == "adx_webpage" && searchIndexInvalidationData != null)
					{
						// bypass cache and go straight to CRM in case cache hasn't been updated yet
						var response = serviceContext.Execute(new RetrieveRequest {
							Target =  new EntityReference(message.Target.LogicalName, message.Target.Id),
							ColumnSet = new ColumnSet(new string[] {
								"adx_parentpageid",
								"adx_websiteid",
								"adx_publishingstateid",
								"adx_partialurl"
							})
						}) as RetrieveResponse;

						if (response != null && response.Entity != null)
						{ 
							var updatedWebPage = response.Entity;

							// If the parent page or website change, we need to invalidate that whole section of the web page hierarchy since the web roles
							// may change, including both root and content pages if MLP is enabled.
							if (!EntityReferenceEquals(searchIndexInvalidationData.ParentPage, updatedWebPage.GetAttributeValue<EntityReference>("adx_parentpageid")) ||
									!EntityReferenceEquals(searchIndexInvalidationData.Website, updatedWebPage.GetAttributeValue<EntityReference>("adx_websiteid")))
							{
								PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_webpage", message.Target.Id));
							}
							// If the publishing state or partial URL change, this will effect all the content pages and localized entities underneath them
							// if MLP is enabled. If MLP is disabled, LCID will equal null, and then just all pages/entities underneath this will reindex.
							else if (!EntityReferenceEquals(searchIndexInvalidationData.PublishingState, updatedWebPage.GetAttributeValue<EntityReference>("adx_publishingstateid")) ||
							  searchIndexInvalidationData.PartialUrl != updatedWebPage.GetAttributeValue<string>("adx_partialurl"))
							{
								PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_webpage", message.Target.Id, searchIndexInvalidationData.Lcid));
							}
						}
					}

					if (message.Target.LogicalName == "adx_webpageaccesscontrolrule_webrole")
					{
						contentMapProvider.Using(contentMap =>
						{
							WebPageAccessControlRuleToWebRoleNode webAccessControlToWebRoleNode;

							if (contentMap.TryGetValue(new EntityReference(message.Target.LogicalName, message.Target.Id), out webAccessControlToWebRoleNode))
							{
								PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_webpage", webAccessControlToWebRoleNode.WebPageAccessControlRule.WebPage.Id));
							}
						});
					}

					if (message.Target.LogicalName == "adx_webpageaccesscontrolrule")
                    {
                        contentMapProvider.Using(contentMap =>
                        {
                            WebPageAccessControlRuleNode webAccessControlNode;

                            if (contentMap.TryGetValue(new EntityReference(message.Target.LogicalName, message.Target.Id), out webAccessControlNode))
                            {
                                PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_webpage", webAccessControlNode.WebPage.Id));
                            }
                        });
                    }

                    if (message.Target.LogicalName == "adx_communityforumaccesspermission")
                    {
                        contentMapProvider.Using(contentMap =>
                        {
                            ForumAccessPermissionNode forumAccessNode;

                            if (contentMap.TryGetValue(new EntityReference(message.Target.LogicalName, message.Target.Id), out forumAccessNode))
                            {
                                PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_communityforum", forumAccessNode.Forum.Id));
                            }
                        });
                    }

					if (message.Target.LogicalName == "connection")
					{
						var fetch = new Fetch
						{
							Entity = new FetchEntity("connection")
							{
								Filters = new[] { new Filter {
									Conditions = new List<Condition> { new Condition("connectionid", ConditionOperator.Equal, message.Target.Id) }
								} }
							}
						};

						var connectionEntity = ((RetrieveSingleResponse)serviceContext.Execute(fetch.ToRetrieveSingleRequest())).Entity;

						var record1Id = connectionEntity.GetAttributeValue<EntityReference>("record1id");
						var record2Id = connectionEntity.GetAttributeValue<EntityReference>("record2id");

						// new product association to knowledge article could mean new product filtering rules
						if (record1Id != null && record1Id.LogicalName == "knowledgearticle" 
							&& record2Id != null && record2Id.LogicalName == "product")
						{
							PerformUpdate(provider, updater => updater.UpdateEntity("knowledgearticle", record1Id.Id));
						}
					}
					if (message.Target.LogicalName == "adx_contentaccesslevel")
					{
						var fetch = GetEntityFetch("adx_knowledgearticlecontentaccesslevel", "knowledgearticleid",
							"adx_contentaccesslevelid", message.Target.Id.ToString());

						var entities = FetchEntities(serviceContext, fetch);
						var guids = entities.Select(e => e.GetAttributeValue<Guid>("knowledgearticleid")).ToList();

						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("knowledgearticle", "knowledgearticleid", guids));
					}
					if (message.Target.LogicalName == "product")
					{
						var fetch = GetEntityFetch("connection", "record2id", "record1id", message.Target.Id.ToString());

						var entities = FetchEntities(serviceContext, fetch);
						var guids = entities.Select(e => e.GetAttributeValue<EntityReference>("record2id")).Select(g => g.Id).Distinct().ToList();

						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("knowledgearticle", "knowledgearticleid", guids));
					}
					if (message.Target.LogicalName == "annotation")
					{
						
						var annotationFetch = new Fetch
						{
							Entity = new FetchEntity("annotation", new[] { "objectid" })
							{
								Filters = new[] 
								{
									new Filter
									{
										Conditions = new List<Condition>
										{
											new Condition("annotationid", ConditionOperator.Equal, message.Target.Id),
										}
									}
								}
							}
						};


						var response =
							(RetrieveSingleResponse)serviceContext.Execute(annotationFetch.ToRetrieveSingleRequest());

						if (response.Entity == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, $"Retrieve of annotation entity failed for annotationId : {message.Target.Id}");
							throw new TransientNullReferenceException($"Retrieve of annotation entity failed for annotationId : {message.Target.Id}");
						}

						var knowledgeArticle = response.Entity?.GetAttributeValue<EntityReference>("objectid");
						if (knowledgeArticle == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, $"Could not find objectId in the retrieved annotation with annotationId : {message.Target.Id}");
							throw new TransientNullReferenceException($"Could not find objectId in the retrieved annotation with annotationId : {message.Target.Id}");
						}

						if (knowledgeArticle.Id != Guid.Empty && knowledgeArticle.LogicalName == "knowledgearticle")
						{
							//Updating Knowledge Article related to this Annotation
							PerformUpdate(provider, updater => updater.UpdateEntity("knowledgearticle", knowledgeArticle.Id));
						}
					}
					//Re-indexing annotations and related knowledge articles if NotesFilter gets changes
					if (message.Target.LogicalName == "adx_sitesetting" && message.Target.Name == "KnowledgeManagement/NotesFilter")
					{
						var notes = GetAllNotes(serviceContext);
						var knowledgeArticles = notes.Select(n => n.GetAttributeValue<EntityReference>("objectid"))
								.Distinct().Select(ka => ka.Id).Distinct()
								.ToList();

						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("annotation", "annotationid", notes.Select(n => n.Id).Distinct().ToList()));
						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("knowledgearticle", "knowledgearticleid", knowledgeArticles));

						return;
					}
					if (message.Target.LogicalName == "adx_sitesetting" && message.Target.Name == "KnowledgeManagement/DisplayNotes")
					{
						PerformUpdateAsync(provider, updater => updater.DeleteEntitySet("annotation"));

						var notes = GetAllNotes(serviceContext);
						var knowledgeArticles = notes.Select(n => n.GetAttributeValue<EntityReference>("objectid"))
								.Distinct().Select(ka => ka.Id).Distinct()
								.ToList();

						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("annotation", "annotationid", notes.Select(n => n.Id).Distinct().ToList()));
						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("knowledgearticle", "knowledgearticleid", knowledgeArticles));
						return;
					}
				}

                PerformUpdate(provider, updater => updater.UpdateEntity(message.Target.LogicalName, message.Target.Id));

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search Index was successfully updated. ({0}, {1}:{2})", message.MessageName, EntityNamePrivacy.GetEntityName(message.Target.LogicalName), message.Target.Id));

				return;
			}

            if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
            {
                if (AssociateDisassociateMessages.Contains(message.MessageName, MessageComparer))
                {
                    if (message.Target == null || string.IsNullOrEmpty(message.Target.LogicalName))
                    {
                        throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an EntityName (entity logical name) parameter.", message.MessageName));
                    }

                    if (message.Target == null || message.Target.Id == Guid.Empty)
                    {
                        throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an ID (entity ID) parameter.", message.MessageName));
                    }

                    if (message.RelatedEntities == null)
                    {
                        throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an EntityName (entity logical name) parameter.", message.MessageName));
                    }

                    if ((message.Target.LogicalName == "adx_webpage" && HasRelatedEntityType(message, "adx_webpageaccesscontrolrule")) ||
                        (message.Target.LogicalName == "adx_communityforum" && HasRelatedEntityType(message, "adx_communityforumaccesspermission")) ||
                        (message.Target.LogicalName == "adx_ideaforum" && HasRelatedEntityType(message, "adx_webrole")))
                    {
                        PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree(message.Target.LogicalName, message.Target.Id));
                    }

                    if (message.Target.LogicalName == "adx_webpageaccesscontrolrule" && 
						(HasRelatedEntityType(message, "adx_webrole") || HasRelatedEntityType(message, "adx_publishingstate")))
                    {
                        contentMapProvider.Using(contentMap =>
                        {
                            WebPageAccessControlRuleNode webAccessControlNode;

                            if (contentMap.TryGetValue(new EntityReference(message.Target.LogicalName, message.Target.Id), out webAccessControlNode))
                            {
                                PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_webpage", webAccessControlNode.WebPage.Id));
                            }
                        });
                    }

                    if (message.Target.LogicalName == "adx_communityforumaccesspermission" && HasRelatedEntityType(message, "adx_webrole"))
                    {
                        contentMapProvider.Using(contentMap =>
                        {
                            ForumAccessPermissionNode forumAccessNode;

                            if (contentMap.TryGetValue(new EntityReference(message.Target.LogicalName, message.Target.Id), out forumAccessNode))
                            {
                                PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree("adx_communityforum", forumAccessNode.Forum.Id));
                            }
                        });
                    }

                    if (message.Target.LogicalName == "adx_contentaccesslevel" && HasRelatedEntityType(message, "knowledgearticle"))
                    {
						foreach (var entityReference in message.RelatedEntities)
						{
							if (entityReference.LogicalName == "knowledgearticle")
							{
								PerformUpdate(provider, updater => updater.UpdateEntity(entityReference.LogicalName, entityReference.Id));
							}
						}
                        
                    }

					//Perform update for disassociate messages from WebNotification Plugin
					if (message.Target.LogicalName == "knowledgearticle" && (HasRelatedEntityType(message, "adx_contentaccesslevel")))
					{
						PerformUpdate(provider, updater => updater.UpdateEntity(message.Target.LogicalName, message.Target.Id));
					}

					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search Index was successfully updated. ({0}, {1}:{2})", message.MessageName, EntityNamePrivacy.GetEntityName(message.Target.LogicalName), message.Target.Id));

                    return;
                }
            }

            if (DeleteMessages.Contains(message.MessageName, MessageComparer))
			{
				if (message.Target == null || string.IsNullOrEmpty(message.Target.LogicalName))
				{
					throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an EntityName (entity logical name) parameter.", message.MessageName));
				}

				if (message.Target == null || message.Target.Id == Guid.Empty)
				{
					throw new HttpException((int)HttpStatusCode.BadRequest, string.Format("Message {0} requires an ID (entity ID) parameter.", message.MessageName));
				}

                if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.CmsEnabledSearching))
				{
					if (message.Target.LogicalName == "adx_webpageaccesscontrolrule" && searchIndexInvalidationData.WebPage != null)
					{
						PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree(searchIndexInvalidationData.WebPage.LogicalName, searchIndexInvalidationData.WebPage.Id));
					}

					if (message.Target.LogicalName == "adx_webpageaccesscontrolrule_webrole" && searchIndexInvalidationData.WebPage != null)
					{
						PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree(searchIndexInvalidationData.WebPage.LogicalName, searchIndexInvalidationData.WebPage.Id));
					}

					if (message.Target.LogicalName == "adx_communityforumaccesspermission" && searchIndexInvalidationData.Forum != null)
					{
						PerformUpdateAsync(provider, updater => updater.UpdateCmsEntityTree(searchIndexInvalidationData.Forum.LogicalName, searchIndexInvalidationData.Forum.Id));
					}

					if (message.Target.LogicalName == "connection")
					{
						// To update Knowledge Article that was related to this connection(Product) we need to retrieve KAid from index
						var relatedEntityList = GetRelatedEntities("connectionid", message.Target.Id, 1);

						if (!relatedEntityList.Any()) { return; }

						// Taking first here since there can only be one Knowledge Article related to connection
						var entity = relatedEntityList.First();
						if (entity.LogicalName != null && entity.LogicalName.Equals("knowledgearticle"))
						{
							PerformUpdate(provider, updater => updater.UpdateEntity("knowledgearticle", entity.Id));
						}
						return;
					}
					if (message.Target.LogicalName == "product" || message.Target.LogicalName == "adx_contentaccesslevel")
					{
						IEnumerable<EntityReference> relatedKnowledgeArticles = new List<EntityReference>();
						var indexedFieldName = "adx_contentaccesslevel";

						if (message.Target.LogicalName == "product")
						{
							indexedFieldName = FixedFacetsConfiguration.ProductFieldFacetName;
						}

						relatedKnowledgeArticles = GetRelatedEntities(indexedFieldName, message.Target.Id, 10000);
						if (!relatedKnowledgeArticles.Any()) { return; }

						var knowledgeArticlesIds =
							relatedKnowledgeArticles.Where(r => r.LogicalName.Equals("knowledgearticle")).Select(i => i.Id).ToList();

						PerformUpdateAsync(provider, updater => updater.UpdateEntitySet("knowledgearticle", "knowledgearticleid", knowledgeArticlesIds));
					}
					if (message.Target.LogicalName == "annotation")
					{
						var relatedKnowledgeArticles = GetRelatedEntities("annotationid", message.Target.Id, 10);
						var knowledgeArticleId = relatedKnowledgeArticles.Where(a => a.LogicalName == "knowledgearticle").Select(ka => ka.Id).FirstOrDefault();

						//Updating Knowledge Article related to this Annotation
						PerformUpdate(provider, updater => updater.UpdateEntity("knowledgearticle", knowledgeArticleId));
					}
				}

				PerformUpdate(provider, updater => updater.DeleteEntity(message.Target.LogicalName, message.Target.Id));

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Search Index was successfully updated. ({0}, {1}:{2})", message.MessageName, EntityNamePrivacy.GetEntityName(message.Target.LogicalName), message.Target.Id));

				return;
			}

			var supportedMessages = DeleteMessages.Union(BuildMessages.Union(UpdateMessages, MessageComparer), MessageComparer);

			ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format(@"Search Index Build Failed. Message ""{0}"" is not supported. Valid messages are {1}.", message.MessageName, string.Join(", ", supportedMessages.ToArray())));
		}

        private static bool HasRelatedEntityType(OrganizationServiceCachePluginMessage message, string relatedEntityLogicalName) 
        {
            return message.RelatedEntities.Exists(relatedEntity => relatedEntity != null && relatedEntity.LogicalName == relatedEntityLogicalName);
        }

		private static bool EntityReferenceEquals(EntityReference er1, EntityReference er2)
		{
			if (er1 == null && er2 == null) return true;
			if (er1 == null || er2 == null) return false;

			return er1.LogicalName == er2.LogicalName && er1.Id == er2.Id;
		}

		/// <summary>
		/// Build the search index
		/// </summary>
		/// <param name="request"></param>
		public static void BuildIndex(HttpRequest request)
		{
			SearchProvider provider;

			if (TryGetSearchProvider(out provider))
			{
				PerformBuild(provider);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Search Index was successfully built.");
			}
			else
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Search Index build failure. Search Provider could not be found.");
			}
		}

		private static bool TryGetSearchProvider(out SearchProvider provider)
		{
			try
			{
				provider = SearchManager.GetProvider(null);
			}
			catch (SearchDisabledProviderException)
			{
				provider = null;

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Search Index build terminated. Search is disabled.");

				return false;
			}
			
			return true;
		}

		private static void PerformBuild(SearchProvider provider)
		{
			using (var builder = provider.GetIndexBuilder())
			{
				builder.BuildIndex();
			}
		}

		private static void PerformUpdate(SearchProvider provider, Action<ICrmEntityIndexUpdater> action)
		{
			try
			{
				using (var updater = provider.GetIndexUpdater())
				{
					action(updater);
				}
			}
			catch (NoSuchDirectoryException)
			{
				PerformBuild(provider);
			}
			catch (FileNotFoundException)
			{
				PerformBuild(provider);
			}
		}

        private static void PerformUpdateAsync(SearchProvider provider, Action<ICrmEntityIndexUpdater> action)
        {
            Task.Factory.StartNew(() => PerformUpdate(provider, action));
        }

		private static IEnumerable<EntityReference> GetRelatedEntities(string fieldName, Guid fieldId, int maxSearchResults)
		{
			try
			{
				var indexSearcher = SearchManager.Provider.GetRawLuceneIndexSearcher();
				return indexSearcher.Search(new TermQuery(new Term(fieldName, fieldId.ToString())), maxSearchResults);
			}
			catch (IndexNotFoundException)
			{
				using (var builder = SearchManager.Provider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}
				// If Index was just rebuilt we don't need to return Updated Entities.
				return Enumerable.Empty<EntityReference>();
			}
		}

		private static IEnumerable<Entity> GetAllNotes(OrganizationServiceContext serviceContext)
		{
			var notesFetch = new Fetch
			{
				Entity = new FetchEntity("annotation", new[] { "annotationid", "objectid" })
				{
					Filters = new[] { new Filter
								{
									Conditions = new List<Condition> { new Condition("objecttypecode", ConditionOperator.Equal, KnowledgeArticleObjectTypeCode) }
								} }
				}
			};

			var response = (serviceContext as IOrganizationService).RetrieveMultiple(notesFetch, RequestFlag.AllowStaleData);
			return response.Entities;
		} 

		private static Fetch GetEntityFetch(string entityName, string attributeToSelect, string filterAttributeName, string filterAttributeValue)
		{
			return new Fetch
			{
				Entity = new FetchEntity(entityName, new List<string> { attributeToSelect })
				{
					Filters = new[] { new Filter {
						Conditions = new List<Condition> { new Condition(filterAttributeName, ConditionOperator.Equal, filterAttributeValue) }
					} }
				}
			};
		}

		private static IEnumerable<Entity> FetchEntities(OrganizationServiceContext serviceContext, Fetch fetch)
		{
			if (fetch == null)
			{
				return Enumerable.Empty<Entity>();
			}

			var entityResult = new List<Entity>();
			fetch.PageNumber = 1;

			while (true)
			{
				var response = (RetrieveMultipleResponse)serviceContext.Execute(fetch.ToRetrieveMultipleRequest());
				entityResult.AddRange(response.EntityCollection.Entities);

				if (!response.EntityCollection.MoreRecords || string.IsNullOrEmpty(response.EntityCollection.PagingCookie))
				{
					break;
				}

				fetch.PageNumber++;
				fetch.PagingCookie = response.EntityCollection.PagingCookie;
			}
			return entityResult;
		}

		public class SearchIndexInvalidationData
        {
            public string PartialUrl { get; set; }
            public EntityReference ParentPage { get; set; }
            public EntityReference Website { get; set; }
			public EntityReference PublishingState { get; set; }
			public EntityReference WebPage { get; set; }
			public EntityReference Forum { get; set; }
			public int? Lcid { get; set; }

        }
	}
}
