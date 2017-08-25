/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Search.Facets;

namespace Adxstudio.Xrm.Search
{
	public interface ICrmEntityQuery
	{
		string Filter { get; }

		IEnumerable<string> LogicalNames { get; }

		int PageNumber { get; }

		int PageSize { get; }

		string LanguageCode { get; }

		string QueryText { get; }

		IEnumerable<FacetConstraints> FacetConstraints { get; }

		string SortingOption { get; set; }
		
		IWebsiteLanguage ContextLanguage { get; }

		bool MultiLanguageEnabled { get; }

		string QueryTerm { get; }
	}
}
