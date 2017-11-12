/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.AspNet.Cms;
using Microsoft.Xrm.Portal.Configuration;
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Search.WindowsAzure
{
	/// <summary>
	/// Implementation of <see cref="ISearchService"/> for search index service exposed by <see cref="CloudDriveServiceSearchProvider"/>.
	/// </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
	public class SearchService : ISearchService
	{
		public bool BuildIndex()
		{
			using (var builder = SearchManager.Provider.GetIndexBuilder())
			{
				builder.BuildIndex();
			}

			return true;
		}

		public EntityIndexInfo GetIndexedEntityInfo()
		{
			try
			{
				var logicalNames = SearchManager.Provider.GetIndexedEntityInfo().Select(i => i.LogicalName);

				return new EntityIndexInfo(logicalNames);
			}
			catch (IndexNotFoundException)
			{
				return new EntityIndexInfo { IndexNotFound = true };
			}
		}

		public EntitySearchResultPage Search(string query, int page, int pageSize, string logicalNames, string scope, string filter)
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : pageSize;

			// TODO check that this still works
			var contextLanguage = HttpContext.Current.GetContextLanguageInfo();
			var languageCode = contextLanguage.IsCrmMultiLanguageEnabled ? contextLanguage.ContextLanguage.Code : null;

			try
			{
				using (var searcher = SearchManager.Provider.GetIndexSearcher())
				{
					var entityQuery = string.IsNullOrEmpty(logicalNames)
						? new ScopedEntityQuery(new[] { scope }, query, page, pageSize, contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled, filter)
						: new ScopedEntityQuery(new[] { scope }, query, page, pageSize, logicalNames.Split(','), contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled, filter);

					var rawResults = searcher.Search(entityQuery);

					var results = rawResults.Select(result => new EntitySearchResult(result.EntityLogicalName, result.EntityID, result.Title, result.Fragment, result.Score));

					return new EntitySearchResultPage(results, rawResults.ApproximateTotalHits, rawResults.PageNumber, rawResults.PageSize);
				}
			}
			catch (IndexNotFoundException)
			{
				return new EntitySearchResultPage(new EntitySearchResult[] { }, 0, page, pageSize)
				{
					IndexNotFound = true
				};
			}
		}

		public void DeleteEntity(string entityLogicalName, Guid id)
		{
			using (var updater = SearchManager.Provider.GetIndexUpdater())
			{
				updater.DeleteEntity(entityLogicalName, id);
			}
		}

		public void DeleteEntitySet(string entityLogicalName)
		{
			using (var updater = SearchManager.Provider.GetIndexUpdater())
			{
				updater.DeleteEntitySet(entityLogicalName);
			}
		}

		public void UpdateEntity(string entityLogicalName, Guid id)
		{
			using (var updater = SearchManager.Provider.GetIndexUpdater())
			{
				updater.UpdateEntity(entityLogicalName, id);
			}
		}

		public void UpdateEntitySet(string entityLogicalName)
		{
			using (var updater = SearchManager.Provider.GetIndexUpdater())
			{
				updater.UpdateEntitySet(entityLogicalName);
			}
		}
	}
}
