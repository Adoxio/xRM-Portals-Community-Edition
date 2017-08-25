/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Pages
{
	using System;
	using System.Linq;
	using Microsoft.Xrm.Sdk;
	using Adxstudio.Xrm.Search;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Web.UI.WebControls;
	using Microsoft.Xrm.Portal.Configuration;

	public partial class Search : PortalPage
	{
		/// <summary>
		/// Web template which will be using for Faceted Search
		/// </summary>
		protected EntityReference FacetedSearchTemplate { get; set; }

		/// <summary>
		/// Tests that the application is enabled (ready) for Faceted Search.
		/// </summary>
		protected bool FacetedSearchEnabled
		{
			get
			{
				var website = this.Context.GetWebsite();
				return website.Settings.Get<bool>("Search/FacetedView");
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (FacetedSearchEnabled && FacetedSearchTemplate != null)
			{
				SearchResults.Controls.Clear();
			}
		}

		protected void Page_Init(object sender, EventArgs args)
		{
			if (!FacetedSearchEnabled)
			{
				return;
			}

			var portalOrgService = Context.GetOrganizationService();

			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webtemplate")
				{
					Filters = new[]
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("adx_name", ConditionOperator.Equal, "Faceted Search - Main Template"),
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_websiteid", ConditionOperator.Equal, Website.Id)
							}
						}
					}
				}
			};

			var facetedSearchWebTemplate = portalOrgService.RetrieveSingle(fetch);
			if (facetedSearchWebTemplate != null)
			{
				this.FacetedSearchTemplate = new EntityReference("adx_webtemplate", facetedSearchWebTemplate.Id);
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
	}
}
