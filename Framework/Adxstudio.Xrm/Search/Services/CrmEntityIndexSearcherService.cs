/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.AspNet.Cms;
using Microsoft.Xrm.Portal.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Search.Services
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class CrmEntityIndexSearcherService : ICrmEntityIndexSearcherService
	{
		public List<CrmEntityIndexInfo> GetIndexedEntityInfo(int languageCode, string searchProvider)
		{
			var provider = SearchManager.GetProvider(searchProvider);

			return (languageCode == 0 ? provider.GetIndexedEntityInfo() : provider.GetIndexedEntityInfo(languageCode)).ToList();
		}

		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public CrmEntitySearchResultPage Search(string query, int page, int pageSize, string logicalNames, string searchProvider)
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : pageSize;

			var provider = SearchManager.GetProvider(searchProvider);
			var contextLanguage = HttpContext.Current.GetContextLanguageInfo();

			using (var searcher = provider.GetIndexSearcher())
			{
				var entityQuery = string.IsNullOrEmpty(logicalNames)
					? new CrmEntityQuery(query, page, pageSize, contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled)
					: new CrmEntityQuery(query, page, pageSize, logicalNames.Split(','), contextLanguage.ContextLanguage, contextLanguage.IsCrmMultiLanguageEnabled);

				var rawResults = searcher.Search(entityQuery);

				var results = rawResults.Select(result => new CrmEntitySearchResult(result.EntityLogicalName, result.EntityID, result.Title, result.Url, result.Fragment, result.ResultNumber, result.Score, result.ExtendedAttributes));

				return new CrmEntitySearchResultPage(results, rawResults.ApproximateTotalHits, rawResults.PageNumber, rawResults.PageSize);
			}
		}
	}
}
