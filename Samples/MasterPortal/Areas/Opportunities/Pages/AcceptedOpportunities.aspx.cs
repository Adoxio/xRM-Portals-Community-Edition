/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Pages;
using Site.Helpers;
using System.Web;

namespace Site.Areas.Opportunities.Pages
{
	public partial class AcceptedOpportunities : PortalPage
	{
		private DataTable _opportunities;

		protected string SortDirection
		{
			get { return ViewState["SortDirection"] as string ?? "ASC"; }
			set { ViewState["SortDirection"] = value; }
		}
		protected string SortExpression
		{
			get { return ViewState["SortExpression"] as string ?? "Accepted"; }
			set { ViewState["SortExpression"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			AssertContactHasParentAccount();

			var opportunities = Enumerable.Empty<Entity>();

			if (string.Equals(CustomerDropDown.Text, "My", StringComparison.InvariantCulture))
			{
				opportunities = ServiceContext.GetOpportunitiesSpecificToContact(Contact)
					.Where(o => o.GetAttributeValue<OptionSetValue>("statuscode") != null && o.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered
								&& o.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Declined);
			}
			else //if (string.Equals(CustomerFilter.Text, "All", StringComparison.InvariantCulture))
			{
				opportunities = ServiceContext.GetOpportunitiesForContact(Contact)
					.Where(o => o.GetAttributeValue<OptionSetValue>("statuscode") != null && o.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered
								&& o.GetAttributeValue<OptionSetValue>("statuscode").Value != (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Declined && o.GetAttributeValue<EntityReference>("msa_partnerid") != null &&
					o.GetAttributeValue<EntityReference>("msa_partnerid").Equals(Contact.GetAttributeValue<EntityReference>("parentcustomerid")));
			}
			//var searchQuery = Request["query"];

			if (!IsPostBack)
			{
				PopulateCustomerFilter(ServiceContext, Contact);
			}

			HideControlsBasedOnAccess(ServiceContext, Contact);

			if (string.Equals(StatusDropDown.Text, "Open", StringComparison.InvariantCulture))
			{
				opportunities = opportunities.Where(o => o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Open);
			}
			else if (string.Equals(StatusDropDown.Text, "Won", StringComparison.InvariantCulture))
			{
				opportunities = opportunities.Where(o => o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Won);
			}
			else if (string.Equals(StatusDropDown.Text, "Lost", StringComparison.InvariantCulture))
			{
				opportunities = opportunities.Where(o => o.GetAttributeValue<OptionSetValue>("statecode") != null && o.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Lost);
			}

			if (!string.IsNullOrEmpty(SearchText.Text))
			{
				opportunities =
					from o in opportunities
					join a in ServiceContext.CreateQuery("account") on o.GetAttributeValue<EntityReference>("customerid").Id equals
						a.GetAttributeValue<Guid>("accountid")
					where o.GetAttributeValue<EntityReference>("customerid") != null
					where
						a.GetAttributeValue<string>("name").ToLower().Contains(SearchText.Text.ToLower()) ||
							(!string.IsNullOrEmpty(o.GetAttributeValue<string>("adx_referencecode")) &&
								o.GetAttributeValue<string>("adx_referencecode").IndexOf(SearchText.Text.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0)
					select o;
			}

			_opportunities = EnumerableExtensions.CopyToDataTable(opportunities.Select(opp => new
			{
				opportunityid = opp.GetAttributeValue<Guid>("opportunityid"),
				ID = opp.GetAttributeValue<string>("adx_referencecode"),
				Accepted = opp.GetAttributeValue<DateTime?>("adx_accepteddate").HasValue ? opp.GetAttributeValue<DateTime?>("adx_accepteddate").GetValueOrDefault().ToString("yyyy/MM/dd") : null,
				CompanyName = opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")).GetAttributeValue<string>("name") : " ",
				City = opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")).GetAttributeValue<string>("address1_city") : " ",
				Territory = opp.GetRelatedEntity(ServiceContext, new Relationship("adx_territory_opportunity")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("adx_territory_opportunity")).GetAttributeValue<string>("name") : " ",
				Products = string.Join(", ", opp.GetRelatedEntities(ServiceContext, new Relationship("adx_opportunity_product")).Select(product => product.GetAttributeValue<string>("name"))),
				EstRevenue = opp.GetAttributeValue<Money>("estimatedvalue") != null ? opp.GetAttributeValue<Money>("estimatedvalue").Value.ToString("C") : null,
				EstClose = opp.GetAttributeValue<DateTime?>("estimatedclosedate").HasValue ? opp.GetAttributeValue<DateTime?>("estimatedclosedate").GetValueOrDefault().ToString("yyyy/MM/dd") : null,
				AssignedTo = (opp.GetRelatedEntity(ServiceContext, new Relationship("msa_contact_opportunity")) != null) ? opp.GetRelatedEntity(ServiceContext, new Relationship("msa_contact_opportunity")).GetAttributeValue<string>("fullname") : " ",
				Status = Enum.GetName(typeof(Adxstudio.Xrm.Partner.Enums.OpportunityState), opp.GetAttributeValue<OptionSetValue>("statecode").Value),
			}).OrderBy(opp => opp.CompanyName));

			_opportunities.Columns["ID"].ColumnName = "Topic";
			_opportunities.Columns["CompanyName"].ColumnName = "Company Name";
			_opportunities.Columns["EstRevenue"].ColumnName = "Est. Revenue";
			_opportunities.Columns["EstClose"].ColumnName = "Est. Purchase";
			_opportunities.Columns["AssignedTo"].ColumnName = "Assigned To";

			AcceptedOpportunitiesList.DataKeyNames = new[] { "opportunityid" };
			AcceptedOpportunitiesList.DataSource = _opportunities;
			AcceptedOpportunitiesList.DataBind();
		}

		protected void LeadsList_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.Header ||
				e.Row.RowType == DataControlRowType.DataRow)
			{
				e.Row.Cells[0].Visible = false;
			}

			if (e.Row.RowType != DataControlRowType.DataRow)
			{
				return;
			}

			var dataKey = AcceptedOpportunitiesList.DataKeys[e.Row.RowIndex].Value;

			e.Row.Cells[1].Text = string.Format(@"<i class=""{0}""></i> <a href=""{1}"" >{2}</a>",
					HttpUtility.HtmlEncode(GetAlertType(dataKey)),
					HttpUtility.HtmlEncode(OpportunityDetailsUrl(dataKey)),
					HttpUtility.HtmlEncode(e.Row.Cells[1].Text));

			//e.Row.Cells[1].Attributes.Add("style", "white-space: nowrap;");

			foreach (var cell in e.Row.Cells.Cast<DataControlFieldCell>().Where(cell => cell.ContainingField.HeaderText == "Est. Revenue"))
			{
				decimal parsedDecimal;

				if (decimal.TryParse(cell.Text, out parsedDecimal))
				{
					cell.Text = parsedDecimal.ToString("C");
				}
			}
		}

		protected string OpportunityDetailsUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Opportunity Details");

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("OpportunityID", id.ToString());

			return url.PathWithQueryString;
		}

		protected void ExportButton_Click(object sender, EventArgs args)
		{
			_opportunities.ExportToCsv("AcceptedOpportunities.csv", Context, new[] { "opportunityid" });
		}

		protected void AcceptedOpportunitiesList_Sorting(object sender, GridViewSortEventArgs e)
		{
			SortDirection = e.SortExpression == SortExpression
				? (SortDirection == "ASC" ? "DESC" : "ASC")
				: "ASC";

			SortExpression = e.SortExpression;

			_opportunities.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			AcceptedOpportunitiesList.DataSource = _opportunities;
			AcceptedOpportunitiesList.DataBind();
		}

		private string GetAlertType(object dataKey)
		{
			Guid id;

			Guid.TryParse(dataKey.ToString(), out id);

			var alertType = ServiceContext.GetAlertType(id, Website);

			if (alertType.ToString() == Enums.AlertType.Overdue.ToString()) return "fa fa-exclamation-circle";

			if (alertType.ToString() == Enums.AlertType.PotentiallyStalled.ToString()) return "fa fa-exclamation-triangle";

			return "";
		}

		private void PopulateCustomerFilter(OrganizationServiceContext context, Entity contact)
		{
			CustomerDropDown.Items.Clear();

			//var accounts = ServiceContext.GetAccountsRelatedToOpportunityAccessForContact(Contact).Cast<Account>().OrderBy(a => a.Name);
			//var accessPermissions = context.GetOpportunityAccessByContact(contact).Cast<adx_opportunitypermissions>();
			
			CustomerDropDown.Items.Add(new ListItem("All"));
			CustomerDropDown.Items.Add(new ListItem("My"));

			//foreach (var account in accounts)
			//{
			//    CustomerDropDown.Items.Add(new ListItem(account.Name, account.GetAttributeValue<Guid>("accountid").ToString()));
			//}
		}

		private void HideControlsBasedOnAccess(OrganizationServiceContext context, Entity contact)
		{
			var accessPermissions = context.GetOpportunityAccessByContact(contact);

			//CreateCaseLink.Visible = false;
			CustomerFilter.Visible = false;
			StatusFilter.Visible = false;
			NoOpportunityAccessLabel.Visible = true;
			CreateButton.Visible = false;

			foreach (var access in accessPermissions)
			{
				if (access.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false))
				{
					//CreateCaseLink.Visible = true;
					CustomerFilter.Visible = true;
					StatusFilter.Visible = true;
					NoOpportunityAccessLabel.Visible = false;

					if (access.GetAttributeValue<OptionSetValue>("adx_scope") != null && access.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityAccessScope.Account)
					{
						CustomerFilter.Visible = true;
					}

					CreateButton.Visible = true;
				}

				if (access.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false))
				{
					CustomerFilter.Visible = true;
					StatusDropDown.Visible = true;
					NoOpportunityAccessLabel.Visible = false;

					if (access.GetAttributeValue<OptionSetValue>("adx_scope") != null && access.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityAccessScope.Account)
					{
						CustomerFilter.Visible = true;
					}
				}
			}

			AcceptedOpportunitiesList.Visible = !NoOpportunityAccessLabel.Visible;
			SearchText.Visible = !NoOpportunityAccessLabel.Visible;
			SearchButton.Visible = !NoOpportunityAccessLabel.Visible;
			ExportBtn.Visible = !NoOpportunityAccessLabel.Visible;
		}

	}
}
