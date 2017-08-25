/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Adxstudio.Xrm.Search.Facets;

namespace Adxstudio.Xrm.Search.Services
{
	[DataContract]
	public class CrmEntitySearchResultPage : ICrmEntitySearchResultPage
	{
		public CrmEntitySearchResultPage(IEnumerable<CrmEntitySearchResult> results, int approximateTotalHits, int pageNumber, int pageSize)
		{
			if (results == null)
			{
				throw new ArgumentNullException("results");
			}

			Results = results.ToList();

			ApproximateTotalHits = approximateTotalHits;
			PageNumber = pageNumber;
			PageSize = pageSize;
		}

		[DataMember]
		public int ApproximateTotalHits { get; private set; }

		[DataMember]
		public int PageNumber { get; private set; }

		[DataMember]
		public int PageSize { get; private set; }

		[DataMember]
		public IEnumerable<CrmEntitySearchResult> Results { get; private set; }
        
        [DataMember]
        public IEnumerable<FacetView> FacetViews { get; private set; }

		public IEnumerable<string> SortingOptions { get; private set; }

		public IEnumerator<ICrmEntitySearchResult> GetEnumerator()
		{
			return Results.Cast<ICrmEntitySearchResult>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
