/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using Adxstudio.Xrm.Search;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public class SearchDataSourceInfoView : SearchDataSourceView
	{
		public SearchDataSourceInfoView(IDataSource owner, string viewName) : base(owner, viewName) { }

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments args)
		{
			var page = (ICrmEntitySearchResultPage)base.ExecuteSelect(args);

			var parameters = Owner.SelectParameters.GetValues(HttpContext.Current, Owner);

			var queryText = GetQueryText(args, parameters);

			return new[] { new SearchData(page, queryText, parameters) };
		}
	}

	public class SearchData
	{
		private readonly IOrderedDictionary _parameters;

		public SearchData(ICrmEntitySearchResultPage resultPage, string queryText, IOrderedDictionary parameters)
		{
			if (resultPage == null)
			{
				throw new ArgumentNullException("resultPage");
			}

			_parameters = parameters ?? new OrderedDictionary();

			ApproximateTotalHits = resultPage.ApproximateTotalHits;
			PageNumber = resultPage.PageNumber;
			PageSize = resultPage.PageSize;
			Count = resultPage.Count();
			QueryText = queryText;

			FirstResultNumber = ((PageNumber - 1) * PageSize) + 1;
			LastResultNumber = (FirstResultNumber + Count) - 1;
		}

		public object this[string key]
		{
			get { return _parameters[key]; }
		}

		public int ApproximateTotalHits { get; private set; }

		public int Count { get; private set; }

		public int FirstResultNumber { get; private set; }

		public int LastResultNumber { get; private set; }

		public int PageNumber { get; private set; }

		public int PageSize { get; private set; }

		public string QueryText { get; private set; }
	}
}
