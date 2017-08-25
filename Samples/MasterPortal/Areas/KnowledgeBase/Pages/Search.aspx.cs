/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web;
using Site.Pages;

namespace Site.Areas.KnowledgeBase.Pages
{
	public partial class Search : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			Guid subjectId;

			if (!string.IsNullOrEmpty(Request["subjectid"]) && Guid.TryParse(Request["subjectid"], out subjectId))
			{
				SearchData.SelectParameters.Add("subjectid", subjectId.ToString());

				SearchData.Query = Html.Setting("knowledgebase/search/querywithsubject", "+(@Query) +subjectid:@subjectid~0.99 +msa_publishtoweb:1 createdon.year:@thisyear^2 createdon.year:@1yearago createdon.year:@2yearsago^0.5");
			}
			else
			{
				SearchData.Query = Html.Setting("knowledgebase/search/query", "+(@Query) +msa_publishtoweb:1 createdon.year:@thisyear^2 createdon.year:@1yearago createdon.year:@2yearsago^0.5");
			}
		}

		protected string GetDisplayUrl(object urlData)
		{
			if (urlData == null)
			{
				return string.Empty;
			}

			try
			{
				return new UrlBuilder(urlData.ToString()).ToString();
			}
			catch (FormatException)
			{
				return string.Empty;
			}
		}

		protected void SearchData_OnSelected(object sender, SearchDataSourceStatusEventArgs args)
		{
			// If the SearchDataSource reports that the index was not found, try to build the index.
			if (args.Exception is IndexNotFoundException)
			{
				using (var builder = args.Provider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}

				args.ExceptionHandled = true;

				// Redirect/refresh to let the user continue their search.
				Response.Redirect(Request.Url.PathAndQuery);
			}
		}

		protected void SearchResults_DataBound(object sender, EventArgs args)
		{
			var pager = ((Control)sender).NamingContainer.FindControl("SearchResultPager") as DataPager;

			if (pager == null)
			{
				return;
			}

			pager.Visible = pager.PageSize < pager.TotalRowCount;
		}
	}
}
