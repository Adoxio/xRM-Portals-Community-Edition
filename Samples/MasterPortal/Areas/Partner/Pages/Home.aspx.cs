/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using System.Web;

namespace Site.Areas.Partner.Pages
{
	public partial class Home : PortalPage
	{
		protected const string HomeAlertsSavedQueryName = "Web - Partner Pipeline - Home Alerts";

		protected List<Entity> Opportunities
		{
			get
			{
				return XrmContext.GetOpportunitiesForContact(Contact).Where(o => o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Open).ToList();
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			//RedirectToLoginIfNecessary();

			var contact = Contact;

			if (contact != null)
			{
				var homeAlertsSavedQuery = XrmContext.CreateQuery("savedquery").FirstOrDefault(query => query.GetAttributeValue<OptionSetValue>("statecode") != null && query.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && query.GetAttributeValue<string>("name") == HomeAlertsSavedQueryName);

				var alerts = Opportunities.Where(opp => (opp.GetAttributeValue<OptionSetValue>("statuscode") != null && opp.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered)
					|| XrmContext.GetOpportunityLatestStatusModifiedOn(opp) <= DateTime.Now.AddDays(-ServiceContext.GetInactiveDaysUntilOverdue(Website)))
					.OrderByDescending(opp => GetAlertType(opp.Id)).ThenBy(opp => opp.GetRelatedEntity(XrmContext, new Relationship("opportunity_customer_accounts")).GetAttributeValue<string>("name"));
				
				var columnsGenerator = new SavedQueryColumnsGenerator(XrmContext, homeAlertsSavedQuery);

				Alerts.DataKeyNames = new[] { "opportunityid" };
				Alerts.DataSource = columnsGenerator.ToDataTable(alerts);
				Alerts.ColumnsGenerator = columnsGenerator;
				Alerts.DataBind();

				var newOpportunities = Opportunities.Where(opp => opp.GetAttributeValue<OptionSetValue>("statuscode") != null && opp.GetAttributeValue<OptionSetValue>("statuscode").Value == 
					(int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered);

				NewOpportunityCount.Text = newOpportunities.Count().ToString();
				NewOpportunityValue.Text = newOpportunities.Where(o => o.GetAttributeValue<Money>("estimatedvalue") != null).Sum(opp => opp.GetAttributeValue<Money>("estimatedvalue").Value).ToString("C");

				var acceptedOpportunities = Opportunities.Where(opp => opp.GetAttributeValue<OptionSetValue>("statuscode") != null && opp.GetAttributeValue<OptionSetValue>("statuscode").Value !=
					(int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered);

				AcceptedOpportunityCount.Text = acceptedOpportunities.Count().ToString();
				AcceptedOpportunityValue.Text = acceptedOpportunities.Where(o => o.GetAttributeValue<Money>("estimatedvalue") != null).Sum(opp => opp.GetAttributeValue<Money>("estimatedvalue").Value).ToString("C");
			}
			else
			{
				PartnerHomePanel.Visible = false;
			}
		}

		protected void Alerts_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow || e.Row.Cells.Count < 1)
			{
				return;
			}

			var dataKey = HttpUtility.HtmlEncode(Alerts.DataKeys[e.Row.RowIndex].Value);

			e.Row.Cells[0].Text = string.Format(@"<i class=""{0}""></i> <a href=""{1}"" >{2}</a>",
				HttpUtility.HtmlEncode(GetAlertType(dataKey)),
				HttpUtility.HtmlEncode(GetAlertType(dataKey)) == "fa fa-check-circle-o" ? NewOpportunitiesUrl() : OpportunityDetailsUrl(dataKey),
				HttpUtility.HtmlEncode(e.Row.Cells[0].Text));
		}

		protected string NewOpportunitiesUrl()
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "New Opportunities");

			return new UrlBuilder(ServiceContext.GetUrl(page)).PathWithQueryString;
		}

		protected string OpportunityDetailsUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Opportunity Details");

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("OpportunityID", id.ToString());

			return url.PathWithQueryString;
		}

		private string GetAlertType(object dataKey)
		{
			Guid id;

			Guid.TryParse(dataKey.ToString(), out id);

			var alertType = ServiceContext.GetAlertType(id, Website);

			if (alertType.ToString() == Enums.AlertType.Overdue.ToString()) return "fa fa-exclamation-circle";

			if (alertType.ToString() == Enums.AlertType.PotentiallyStalled.ToString()) return "fa fa-exclamation-triangle";

			if (alertType.ToString() == Enums.AlertType.New.ToString()) return "fa fa-check-circle-o";

			return "";
		}

	}
}
