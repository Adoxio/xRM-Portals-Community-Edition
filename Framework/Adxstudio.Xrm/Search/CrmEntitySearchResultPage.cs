/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Search.Facets;

namespace Adxstudio.Xrm.Search
{
	public class CrmEntitySearchResultPage : ICrmEntitySearchResultPage
	{
		private readonly IEnumerable<ICrmEntitySearchResult> _results;

		public CrmEntitySearchResultPage(IEnumerable<ICrmEntitySearchResult> results, int approximateTotalHits, int pageNumber, int pageSize, IEnumerable<FacetView> facetViews = null, IEnumerable<string> sortingOptions = null)
		{
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}

			_results = results.ToList();

			ApproximateTotalHits = approximateTotalHits;
			PageNumber = pageNumber;
			PageSize = pageSize;
            FacetViews = facetViews;
			SortingOptions = sortingOptions;
		}

		public int ApproximateTotalHits { get; private set; }

		public int PageNumber { get; private set; }

		public int PageSize { get; private set; }

		public IEnumerable<FacetView> FacetViews { get; private set; }

		public IEnumerable<string> SortingOptions { get; private set; }

		public IEnumerator<ICrmEntitySearchResult> GetEnumerator()
		{
			return _results.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
