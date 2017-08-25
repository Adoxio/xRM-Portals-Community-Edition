/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using System.Web;

namespace Site.Areas.Opportunities.Pages
{
	public partial class NewOpportunities : PortalPage
	{

		private List<string> _acceptChecked = new List<string>();
		private List<string> _declineChecked = new List<string>();

		//private DataTable _opportunities;

		protected DataTable Opportunities
		{
			get { return ViewState["NewOpportunities"] as DataTable ?? GetOpportunities(); }
			set { ViewState["NewOpportunities"] = value; }
		}

		protected string SavedOpportunities
		{
			get { return ViewState["SavedOpportunities"] as string ?? string.Empty; }
			set { ViewState["SavedOpportunities"] = value; }
		}

		protected string SortDirection
		{
			get { return ViewState["SortDirection"] as string ?? "ASC"; }
			set { ViewState["SortDirection"] = value; }
		}
		protected string SortExpression
		{
			get { return ViewState["SortExpression"] as string ?? "Delivered"; }
			set { ViewState["SortExpression"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			AssertContactHasParentAccount();

			Opportunities.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			NewOpportunitiesList.DataKeyNames = new[] { "opportunityid" };
			NewOpportunitiesList.DataSource = Opportunities;
			NewOpportunitiesList.DataBind();
		}

		protected void OpportunitiesList_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.Header)
			{
				e.Row.Cells[0].Visible = false;

				e.Row.Cells.Add(new TableHeaderCell { Text = "Accept" });
				e.Row.Cells.Add(new TableHeaderCell { Text = "Decline" });

				return;
			}

			if (e.Row.RowType != DataControlRowType.DataRow)
			{
				return;
			}

			e.Row.Cells[0].Visible = false;

			var dataKey = HttpUtility.HtmlEncode(NewOpportunitiesList.DataKeys[e.Row.RowIndex].Value.ToString());

			//e.Row.Cells[1].Attributes.Add("style", "white-space: nowrap;");

			foreach (var cell in e.Row.Cells.Cast<DataControlFieldCell>().Where(cell => cell.ContainingField.HeaderText == "Est. Revenue"))
			{
				decimal parsedDecimal;

				if (decimal.TryParse(cell.Text, out parsedDecimal))
				{
					cell.Text = parsedDecimal.ToString("C");
				}
			}

			var oppPermissions = XrmContext.GetOpportunityAccessByContactForParentAccount(Contact).Where(oa =>
	oa.GetAttributeValue<bool?>("adx_acceptdecline") ?? false == true);

			var acceptCell = new TableCell { HorizontalAlign = HorizontalAlign.Center, Width = new Unit("1%") };
			var acceptCheckBox = new CheckBox { ID = dataKey + "_accept", CssClass = "accept" };
			acceptCheckBox.InputAttributes["class"] = dataKey;
			acceptCell.Controls.Add(acceptCheckBox);
			e.Row.Cells.Add(acceptCell);

			var declineCell = new TableCell { HorizontalAlign = HorizontalAlign.Center, Width = new Unit("1%") };
			var declineCheckBox = new CheckBox { ID = dataKey + "_decline", CssClass = "decline" };
			declineCheckBox.InputAttributes["class"] = dataKey;
			declineCell.Controls.Add(declineCheckBox);
			e.Row.Cells.Add(declineCell);

			if (!oppPermissions.ToList().Any())
			{
				acceptCheckBox.Enabled = false;
				declineCheckBox.Enabled = false;
			}
		}

		protected void NewOpportunitiesList_Sorting(object sender, GridViewSortEventArgs e)
		{
			StoreCheckedValues();

			SortDirection = e.SortExpression == SortExpression
				? (SortDirection == "ASC" ? "DESC" : "ASC")
				: "ASC";

			SortExpression = e.SortExpression;

			Opportunities.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			NewOpportunitiesList.DataSource = Opportunities;
			NewOpportunitiesList.DataBind();

			RestoreCheckedValues();
		}

		protected void SaveButton_Click(object sender, EventArgs args)
		{
			foreach (GridViewRow row in NewOpportunitiesList.Rows)
			{
				var dataKey = new Guid(NewOpportunitiesList.DataKeys[row.RowIndex].Value.ToString());

				var cellCount = row.Cells.Count;

				var acceptCell = row.Cells[cellCount - 2];

				var accept = acceptCell.FindControl(dataKey + "_accept") as CheckBox;

				var declineCell = row.Cells[cellCount - 1];

				var decline = declineCell.FindControl(dataKey + "_decline") as CheckBox;

				var opportunity = XrmContext.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == dataKey);

				if (SavedOpportunities.Contains(dataKey.ToString()) ||
					(!accept.Checked && !decline.Checked) || (accept.Checked && decline.Checked))
				{
					continue;
				}

				var partnerReference = opportunity.GetAttributeValue<EntityReference>("msa_partnerid");

				if (partnerReference == null)
				{
					continue;
				}

				var partner = XrmContext.CreateQuery("account").First(a => a.GetAttributeValue<Guid>("accountid") == partnerReference.Id);

				var oppPermissions = XrmContext.GetOpportunityAccessByContactForParentAccount(Contact).Where(oa =>
					oa.GetAttributeValue<bool?>("adx_acceptdecline").GetValueOrDefault(false));

				if (partner.GetAttributeValue<int?>("adx_numberofopportunitiesdelivered").GetValueOrDefault(0) == 0)
				{
					partner.SetAttributeValue("adx_numberofopportunitiesdelivered", 1);
				}

				var touchrate = (double)(partner.GetAttributeValue<int?>("adx_numberofopportunitiesaccepted").GetValueOrDefault(0) + (partner.GetAttributeValue<int?>("adx_numberofopportunitiesdeclined").GetValueOrDefault(0))) / (partner.GetAttributeValue<int?>("adx_numberofopportunitiesdelivered").GetValueOrDefault(1));
				partner.SetAttributeValue("adx_touchrate", touchrate);

				if (accept.Checked)
				{
					if (oppPermissions.ToList().Any())
					{
						//we mark this opportunity as accepted
						opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Accepted));
						opportunity.SetAttributeValue("stepname", OpportunityHistory.PipelinePhaseAccepted);
						opportunity.SetAttributeValue("adx_accepteddate", DateTime.UtcNow);

						partner.SetAttributeValue("adx_numberofopportunitiesaccepted", partner.GetAttributeValue<int?>("adx_numberofopportunitiesaccepted").GetValueOrDefault(0) + 1);
						}
					}
				else if (decline.Checked)
				{
					if (oppPermissions.ToList().Any())
					{
						//we mark this opportunity as declined
						opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Declined));
						opportunity.SetAttributeValue("stepname", OpportunityHistory.PipelinePhaseDeclined);

						partner.SetAttributeValue("adx_numberofopportunitiesdeclined", partner.GetAttributeValue<int?>("adx_numberofopportunitiesdeclined").GetValueOrDefault(0) + 1);
						partner.SetAttributeValue("adx_activeopportunitycount", partner.GetAttributeValue<int?>("adx_activeopportunitycount").GetValueOrDefault(0) - 1);

						XrmContext.AddLink(opportunity, new Relationship("adx_account_declinedopportunity"), partner);
					}
				}

				opportunity.SetAttributeValue("adx_expirydate", null);

				XrmContext.UpdateObject(opportunity);
				XrmContext.UpdateObject(partner);
				XrmContext.SaveChanges();

				SavedOpportunities += dataKey + ",";
			}

			RestoreCheckedValues();
			//Response.Redirect(Request.RawUrl);
		}

		private DataTable GetOpportunities()
		{
			var opportunities = ServiceContext.GetOpportunitiesForContact(Contact)
			.Where(o => o.GetAttributeValue<EntityReference>("msa_partnerid") != null && o.GetAttributeValue<EntityReference>("msa_partnerid").Equals(Contact.GetAttributeValue<EntityReference>("parentcustomerid")) &&
				o.GetAttributeValue<OptionSetValue>("statuscode") != null && o.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered);

			Opportunities = EnumerableExtensions.CopyToDataTable(opportunities.Select(opp => new
			{
				opportunityid = opp.GetAttributeValue<Guid>("opportunityid"),
				ID = opp.GetAttributeValue<string>("adx_referencecode"),
				Delivered = opp.GetAttributeValue<DateTime?>("adx_delivereddate").HasValue ? opp.GetAttributeValue<DateTime?>("adx_delivereddate").GetValueOrDefault().ToString("yyyy/MM/dd") : null,
				CompanyName = opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")).GetAttributeValue<string>("name") : " ",
				City = opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("opportunity_customer_accounts")).GetAttributeValue<string>("address1_city") : " ",
				Territory = opp.GetRelatedEntity(ServiceContext, new Relationship("adx_territory_opportunity")) != null ? opp.GetRelatedEntity(ServiceContext, new Relationship("adx_territory_opportunity")).GetAttributeValue<string>("name") : " ",
				Products = string.Join(", ", opp.GetRelatedEntities(ServiceContext, new Relationship("adx_opportunity_product")).Select(product => product.GetAttributeValue<string>("name"))),
				EstRevenue = opp.GetAttributeValue<Money>("estimatedvalue") != null ? opp.GetAttributeValue<Money>("estimatedvalue").Value.ToString("C") : null,
				Expires = opp.GetAttributeValue<DateTime?>("adx_expirydate").HasValue ? opp.GetAttributeValue<DateTime?>("adx_expirydate").GetValueOrDefault().ToString("yyyy/MM/dd") : null,
			}).OrderBy(opp => opp.Delivered).ThenBy(opp => opp.CompanyName));

			Opportunities.Columns["ID"].ColumnName = "Topic";
			Opportunities.Columns["CompanyName"].ColumnName = "Company Name";
			Opportunities.Columns["EstRevenue"].ColumnName = "Est. Revenue";
			//_opportunities.Columns["EstClose"].ColumnName = "Est. Purchase";

			return Opportunities;
		}

		private void RestoreCheckedValues()
		{
			foreach (GridViewRow row in NewOpportunitiesList.Rows)
			{
				var dataKey = NewOpportunitiesList.DataKeys[row.RowIndex].Value.ToString();

				var cellCount = row.Cells.Count;

				if (_acceptChecked.Contains(dataKey))
				{
					var acceptCell = row.Cells[cellCount - 2];

					var accept = acceptCell.FindControl(dataKey + "_accept") as CheckBox;

					accept.Checked = true;
				}

				if (_declineChecked.Contains(dataKey))
				{
					var declineCell = row.Cells[cellCount - 1];

					var decline = declineCell.FindControl(dataKey + "_decline") as CheckBox;

					decline.Checked = true;
				}

				if (SavedOpportunities.Contains(dataKey))
				{
					row.CssClass = "saved" + (row.RowIndex % 2 == 1 ? " alternate-row" : string.Empty);
				}
			}
		}

		private void StoreCheckedValues()
		{
			foreach (GridViewRow row in NewOpportunitiesList.Rows)
			{
				var dataKey = NewOpportunitiesList.DataKeys[row.RowIndex].Value.ToString();

				var cellCount = row.Cells.Count;

				var acceptCell = row.Cells[cellCount - 2];

				var accept = acceptCell.FindControl(dataKey + "_accept") as CheckBox;

				if (accept.Checked)
				{
					_acceptChecked.Add(dataKey);
				}

				var declineCell = row.Cells[cellCount - 1];

				var decline = declineCell.FindControl(dataKey + "_decline") as CheckBox;

				if (decline.Checked)
				{
					_declineChecked.Add(dataKey);
				}
			}
		}
	}
}
