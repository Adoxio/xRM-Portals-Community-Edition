/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.KnowledgeBase.Pages
{
	public partial class Subject : PortalPage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			if (Entity.LogicalName != "adx_webpage")
			{
				return;
			}

			var webPage = XrmContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == Entity.Id);

			if (webPage == null)
			{
				return;
			}

			var subject = webPage.GetAttributeValue<EntityReference>("adx_subjectid");

			if (subject == null)
			{
				MostPopularArticlesSearchData.Query =
					Html.Setting("knowledgebase/search/query", "adx_averagerating_int:5^5 adx_averagerating_int:4^4 adx_averagerating_int:3 +msa_publishtoweb:1 createdon.year:@thisyear^2 createdon.year:@1yearago createdon.year:@2yearsago^0.5");
			}
			else
			{
				MostPopularArticlesSearchData.SelectParameters.Add("subjectid", subject.Id.ToString());

				MostPopularArticlesSearchData.Query =
					Html.Setting("knowledgebase/search/querywithsubject", "adx_averagerating_int:5^5 adx_averagerating_int:4^4 adx_averagerating_int:3 +subjectid:@subjectid~0.99 +msa_publishtoweb:1 createdon.year:@thisyear^2 createdon.year:@1yearago createdon.year:@2yearsago^0.5");
			}

			var now = DateTime.UtcNow;

			MostPopularArticlesSearchData.SelectParameters.Add("thisyear", now.ToString("yyyy"));
			MostPopularArticlesSearchData.SelectParameters.Add("1yearago", now.AddYears(-1).ToString("yyyy"));
			MostPopularArticlesSearchData.SelectParameters.Add("2yearsago", now.AddYears(-2).ToString("yyyy"));
			MostPopularArticlesSearchData.SelectParameters.Add("3yearsago", now.AddYears(-3).ToString("yyyy"));
			MostPopularArticlesSearchData.SelectParameters.Add("4yearsago", now.AddYears(-4).ToString("yyyy"));
			MostPopularArticlesSearchData.SelectParameters.Add("5yearsago", now.AddYears(-5).ToString("yyyy"));

			if (subject == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(SearchData.Query))
			{
				SearchData.Query = "+subjectid:@subjectid~0.99 +msa_publishtoweb:1 createdon.year:@thisyear^2 createdon.year:@1yearago createdon.year:@2yearsago^0.5";
			}

			SearchData.SelectParameters.Add("subjectid", subject.Id.ToString());
			SearchData.SelectParameters.Add("thisyear", now.ToString("yyyy"));
			SearchData.SelectParameters.Add("1yearago", now.AddYears(-1).ToString("yyyy"));
			SearchData.SelectParameters.Add("2yearsago", now.AddYears(-2).ToString("yyyy"));
			SearchData.SelectParameters.Add("3yearsago", now.AddYears(-3).ToString("yyyy"));
			SearchData.SelectParameters.Add("4yearsago", now.AddYears(-4).ToString("yyyy"));
			SearchData.SelectParameters.Add("5yearsago", now.AddYears(-5).ToString("yyyy"));

			SubjectSearch.Visible = true;
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
