/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Search
{
	public class ExtendedAttributeIndexSearcher : ICrmEntityIndexSearcher
	{
		private readonly string _dataContextName;
		private readonly ICrmEntityIndexSearcher _searcher;

		public ExtendedAttributeIndexSearcher(ICrmEntityIndexSearcher searcher, string dataContextName)
		{
			if (searcher == null)
			{
				throw new ArgumentNullException("searcher");
			}

			_searcher = searcher;
			_dataContextName = dataContextName;
		}

		public void Dispose()
		{
			_searcher.Dispose();
		}

		public ICrmEntitySearchResultPage Search(ICrmEntityQuery query)
		{
			var rawResults = _searcher.Search(query);

			var infoCache = new Dictionary<string, ExtendedAttributeSearchResultInfo>();
			var metadataCache = new Dictionary<string, EntityMetadata>();

			var results = rawResults.Select(result =>
			{
				var info = GetExtendedAttributeInfo(_dataContextName, result.Entity.LogicalName, infoCache, metadataCache);

				var extendedAttributes = info.GetAttributes(result.Entity, metadataCache);

				return new CrmEntitySearchResult(result.Entity, result.Score, result.ResultNumber, result.Title, result.Url, extendedAttributes)
				{
					Fragment = result.Fragment
				} as ICrmEntitySearchResult;
			});

			return new CrmEntitySearchResultPage(results, rawResults.ApproximateTotalHits, rawResults.PageNumber, rawResults.PageSize);
		}

		private static ExtendedAttributeSearchResultInfo GetExtendedAttributeInfo(string dataContextName, string logicalName, IDictionary<string, ExtendedAttributeSearchResultInfo> cache, IDictionary<string, EntityMetadata> metadataCache)
		{
			ExtendedAttributeSearchResultInfo info;

			if (cache.TryGetValue(logicalName, out info))
			{
				return info;
			}

			var context = string.IsNullOrEmpty(dataContextName) ? CrmConfigurationManager.CreateContext() : CrmConfigurationManager.CreateContext(dataContextName);

			context.MergeOption = MergeOption.NoTracking;

			info = new ExtendedAttributeSearchResultInfo(context, logicalName, metadataCache);

			cache[logicalName] = info;

			return info;
		}
	}
}
