/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search
{
	using System.Collections.Generic;
	using Adxstudio.Xrm.Search.Facets;

	/// <summary>
	/// Interface for CRM entity result page
	/// </summary>
	/// <seealso cref="System.Collections.Generic.IEnumerable{Adxstudio.Xrm.Search.ICrmEntitySearchResult}" />
	public interface ICrmEntitySearchResultPage : IEnumerable<ICrmEntitySearchResult>
	{
		int ApproximateTotalHits { get; }

		int PageNumber { get; }

		int PageSize { get; }

		IEnumerable<FacetView> FacetViews { get; }

		IEnumerable<string> SortingOptions { get; }
	}
}
