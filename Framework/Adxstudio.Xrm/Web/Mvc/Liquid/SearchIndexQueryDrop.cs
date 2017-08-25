/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Search;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SearchIndexQueryDrop : PortalDrop
	{
		private readonly Lazy<ICrmEntitySearchResultPage> _resultPage;
		private readonly Lazy<SearchIndexQueryResultDrop[]> _results;
		
		public SearchIndexQueryDrop(IPortalLiquidContext portalLiquidContext, SearchProvider searchProvider, ICrmEntityQuery query) : base(portalLiquidContext)
		{
			if (searchProvider == null) throw new ArgumentNullException("searchProvider");
			if (query == null) throw new ArgumentNullException("query");

			SearchProvider = searchProvider;
			Query = query;

			_resultPage = new Lazy<ICrmEntitySearchResultPage>(GetResultPage, LazyThreadSafetyMode.None);
			_results = new Lazy<SearchIndexQueryResultDrop[]>(GetResults, LazyThreadSafetyMode.None);
		}

		public int ApproximateTotalHits
		{
			get { return ResultPage.ApproximateTotalHits; }
		}

		public int Page
		{
			get { return ResultPage.PageNumber; }
		}

		public int PageSize
		{
			get { return ResultPage.PageSize; }
		}

		public IEnumerable<SearchIndexQueryResultDrop> Results
		{
			get { return _results.Value; }
		}

		protected ICrmEntityQuery Query { get; private set; }

		protected ICrmEntitySearchResultPage ResultPage
		{
			get { return _resultPage.Value; }
		}

		protected SearchProvider SearchProvider { get; private set; }

		private ICrmEntitySearchResultPage GetResultPage()
		{
			try
			{
				using (var searcher = SearchProvider.GetIndexSearcher())
				{
					return searcher.Search(Query);
				}
			}
			// If the index does not exist yet, build it and then resubmit the query.
			catch (IndexNotFoundException)
			{
				using (var builder = SearchProvider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}

				using (var searcher = SearchProvider.GetIndexSearcher())
				{
					return searcher.Search(Query);
				}
			}
		}

		private SearchIndexQueryResultDrop[] GetResults()
		{
			return ResultPage.Select(e => new SearchIndexQueryResultDrop(this, e)).ToArray();
		}
	}
}
