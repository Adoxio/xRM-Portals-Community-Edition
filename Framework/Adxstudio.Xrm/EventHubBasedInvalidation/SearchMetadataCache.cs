/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using System.Collections.Generic;

	using Adxstudio.Xrm.Cms;

	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// The search metadata cache.
	/// </summary>
	public class SearchMetadataCache
	{
		/// <summary>
		/// The search metadata cache instance.
		/// </summary>
		private static readonly Lazy<SearchMetadataCache> SearchMetadataCacheInstance = new Lazy<SearchMetadataCache>(() => new SearchMetadataCache());

		/// <summary>
		/// The organization service context.
		/// </summary>
		private OrganizationServiceContext organizationServiceContext;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		public static SearchMetadataCache Instance
		{
			get
			{
				return SearchMetadataCacheInstance.Value;
			}
		}

		/// <summary>
		/// Gets the search enabled entities.
		/// </summary>
		public HashSet<string> SearchEnabledEntities { get; private set; }

		/// <summary>
		/// Gets the search saved queries.
		/// </summary>
		public HashSet<SearchSavedQuery> SearchSavedQueries { get; private set; }

		/// <summary>
		/// Gets the organization service context.
		/// </summary>
		public OrganizationServiceContext OrganizationServiceContext
		{
			get { return this.organizationServiceContext ?? (this.organizationServiceContext = PortalCrmConfigurationManager.CreateServiceContext()); }
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="SearchMetadataCache"/> class from being created.
		/// </summary>
		private SearchMetadataCache()
		{
			this.SearchEnabledEntities = new HashSet<string>();
			this.SearchSavedQueries = new HashSet<SearchSavedQuery>();
		}

		/// <summary>
		/// Query CRM to get Portal Search Enabled entities
		/// </summary>
		/// <returns>
		/// The search enabled entities.
		/// </returns>
		public HashSet<string> GetPortalSearchEnabledEntities()
		{
			this.SearchEnabledEntities = new HashSet<string>();
			this.SearchSavedQueries = new HashSet<SearchSavedQuery>();

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.AddCondition("name", ConditionOperator.Equal, this.GetSearchSavedQueryViewName());
			filterExpression.AddCondition("componentstate", ConditionOperator.Equal, 0);
			QueryExpression query = new QueryExpression()
			{
				EntityName = "savedquery",
				ColumnSet = new ColumnSet("returnedtypecode", "savedqueryidunique"),
				Criteria = filterExpression
			};
			var request = new RetrieveMultipleRequest() { Query = query };
			var response = (RetrieveMultipleResponse)this.OrganizationServiceContext.Execute(request);
			var entities = response.EntityCollection.Entities;

			foreach (Entity entity in entities)
			{
				var savedQueryItem = new SearchSavedQuery(entity);
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Entity {0} has Portal Search View Present in CRM ", savedQueryItem.EntityName));
				this.SearchEnabledEntities.Add(savedQueryItem.EntityName);
				this.SearchSavedQueries.Add(savedQueryItem);
			}

			// These entities are not searchable but changes to them can invalidate certain searchable entities
			List<string> searchAffectingEntities = new List<string>()
			{
				"adx_webpageaccesscontrolrule",
				"adx_communityforumaccesspermission",
				"connection",
				"adx_contentaccesslevel",
				"adx_knowledgearticlecontentaccesslevel",
				"adx_webpageaccesscontrolrule_webrole",
				"adx_website",
				"savedquery"
			};
			searchAffectingEntities.ForEach(x => this.SearchEnabledEntities.Add(x));
			return this.SearchEnabledEntities;
		}

		/// <summary>
		/// Get Search SavedQuery View Name from site setting "Search/IndexQueryName" or use default name "Portal Search"
		/// </summary>
		/// <returns>Saved Query view</returns>
		private string GetSearchSavedQueryViewName()
		{
			var searchSavedQueryNameFromSettings = this.OrganizationServiceContext.GetSettingValueByName("Search/IndexQueryName");
			var searchSavedQueryView = string.IsNullOrEmpty(searchSavedQueryNameFromSettings)
											? "Portal Search"
											: searchSavedQueryNameFromSettings;
			return searchSavedQueryView;
		}

		/// <summary>
		/// Get SavedQueryUniqueId attribute of SavedQuery by id
		/// </summary>
		/// <param name="savedqueryId">
		/// The saved query id.
		/// </param>
		/// <returns>
		/// The <see cref="Guid"/>.
		/// </returns>
		public Guid GetSavedQueryUniqueId(Guid savedqueryId)
		{
			var response =
				this.OrganizationServiceContext.Execute(
					new RetrieveRequest
						{
							Target = new EntityReference("savedquery", savedqueryId),
							ColumnSet = new ColumnSet("savedqueryidunique")
						}) as
				RetrieveResponse;

			if (response != null && response.Entity != null)
			{
				return response.Entity.GetAttributeValue<Guid>("savedqueryidunique");
			}

			return Guid.Empty;
		}

		/// <summary>
		/// The complete metadata update for search saved query.
		/// </summary>
		/// <param name="savedQuery">
		/// The saved query.
		/// </param>
		/// <param name="newUniqueId">
		/// The new unique id.
		/// </param>
		public void CompleteMetadataUpdateForSearchSavedQuery(SearchSavedQuery savedQuery, Guid newUniqueId)
		{
			savedQuery.SavedQueryIdUnique = newUniqueId;
		}
	}
}
