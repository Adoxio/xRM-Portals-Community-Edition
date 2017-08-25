/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Site.Pages;
using Microsoft.Xrm.Sdk;
using System.Web;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class ManageCustomerContacts : PortalPage
	{
		private DataTable _contacts;

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

			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelCreateAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));
			var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));
			var channelReadAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));
			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var validAcccountClassificationCode = parentCustomerAccount != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)Enums.AccountClassificationCode.Partner;

			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				ContactsList.Visible = false;

				return;
			}

			if (!channelReadAccess && !channelWriteAccess)
			{
				ChannelPermissionsError.Visible = true;
			}
			else
			{
				if (parentCustomerAccount == null)
				{
					NoParentAccountError.Visible = true;
				}
				else
				{
					ParentAccountClassificationCodeError.Visible = !validAcccountClassificationCode;
				}
			}

			if ((!channelReadAccess && !channelWriteAccess) || parentCustomerAccount == null || !validAcccountClassificationCode)
			{
				ContactsList.Visible = false;

				return;
			}

			CreateButton.Visible = channelCreateAccess;

			if (!IsPostBack)
			{
				PopulateCustomerFilter();
			}
			
			var contacts = new List<Entity>();
			
			if (string.Equals(CustomerFilter.Text, "All", StringComparison.InvariantCulture))
			{
				var myContacts = ServiceContext.CreateQuery("contact").Where(c => c.GetAttributeValue<EntityReference>("msa_managingpartnerid") == parentCustomerAccount.ToEntityReference());

				contacts.AddRange(myContacts);

				var accounts = ServiceContext.CreateQuery("account").Where(a => a.GetAttributeValue<EntityReference>("msa_managingpartnerid") == parentCustomerAccount.ToEntityReference()).ToList();

				if (accounts.Any())
				{
					foreach (var account in accounts)
					{
						var currentContacts =
							ServiceContext.CreateQuery("contact")
								.Where(
									c =>
										(c.GetAttributeValue<EntityReference>("msa_managingpartnerid") == null ||
										 (c.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null &&
										  !c.GetAttributeValue<EntityReference>("msa_managingpartnerid").Equals(parentCustomerAccount.ToEntityReference()))) &&
										c.GetAttributeValue<EntityReference>("parentcustomerid") != null &&
										c.GetAttributeValue<EntityReference>("parentcustomerid").Equals(account.ToEntityReference()));

						contacts.AddRange(currentContacts);
					}
				}
			}
			else if (string.Equals(CustomerFilter.Text, "My", StringComparison.InvariantCulture))
			{
				var currentContacts =
					ServiceContext.CreateQuery("contact")
						.Where(
							c =>
								c.GetAttributeValue<EntityReference>("parentcustomerid") == null &&
								(c.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null &&
								 c.GetAttributeValue<EntityReference>("msa_managingpartnerid").Equals(parentCustomerAccount.ToEntityReference())));

				contacts.AddRange(currentContacts);
			}
			else
			{
				Guid accountid;

				if (Guid.TryParse(CustomerFilter.SelectedValue, out accountid))
				{
					var currentContacts =
						ServiceContext.CreateQuery("contact")
							.Where(
								c =>
									c.GetAttributeValue<EntityReference>("parentcustomerid") != null &&
									c.GetAttributeValue<EntityReference>("parentcustomerid").Equals(new EntityReference("account", accountid)));

					contacts.AddRange(currentContacts);
				}
			}

			_contacts = EnumerableExtensions.CopyToDataTable(contacts.Select(c => new
			{
				contactid = c.GetAttributeValue<Guid>("contactid"),
				ID = c.GetAttributeValue<string>("fullname"),
				CompanyName = c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")) == null ? string.Empty : c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")).GetAttributeValue<string>("name"),
				City = c.GetAttributeValue<string>("address1_city"),
				State = c.GetAttributeValue<string>("address1_stateorprovince"),
				Phone = c.GetAttributeValue<string>("address1_telephone1"),
				Email = c.GetAttributeValue<string>("emailaddress1"),
			}));

			_contacts.Columns["City"].ColumnName = "City";
			_contacts.Columns["State"].ColumnName = "State";
			_contacts.Columns["Phone"].ColumnName = "Phone";
			_contacts.Columns["Email"].ColumnName = "E-mail Address";

			CustomerContactsList.DataKeyNames = new[] { "contactid" };
			CustomerContactsList.DataSource = _contacts;
			CustomerContactsList.DataBind();

			Guid id;

			if (CustomerFilter.SelectedItem != null && Guid.TryParse(CustomerFilter.SelectedItem.Value, out id))
			{
				CreateButton.QueryStringCollection = new QueryStringCollection("");

				CreateButton.QueryStringCollection.Set("AccountID", id.ToString());
			}
		}

		protected void CustomerContactsList_Sorting(object sender, GridViewSortEventArgs e)
		{
			SortDirection = e.SortExpression == SortExpression ? (SortDirection == "ASC" ? "DESC" : "ASC") : "ASC";

			SortExpression = e.SortExpression;

			_contacts.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			CustomerContactsList.DataSource = _contacts;
			CustomerContactsList.DataBind();
		}

		protected void CustomerContactsList_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.Header || e.Row.RowType == DataControlRowType.DataRow)
			{
				e.Row.Cells[0].Visible = false;
			}

			if (e.Row.RowType != DataControlRowType.DataRow)
			{
				return;
			}

			var dataKey = CustomerContactsList.DataKeys[e.Row.RowIndex].Value;

			e.Row.Cells[1].Text = string.Format(@"<a href=""{0}"">{1}</a>", HttpUtility.HtmlEncode(EditContactUrl(dataKey)), HttpUtility.HtmlEncode(e.Row.Cells[1].Text));

			e.Row.Cells[1].Attributes.Add("style", "white-space: nowrap;");
		}

		protected string EditContactUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Edit Customer Contact");

			if (page == null)
			{
				throw new Exception("Please contact the system administrator. A required site marker titled Edit Customer Contact doesn't exist.");
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("ContactID", id.ToString());

			return url.PathWithQueryString;
		}

		private void PopulateCustomerFilter()
		{
			CustomerFilter.Items.Clear();

			var accounts =
				ServiceContext.CreateQuery("account")
					.Where(
						a =>
							a.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null &&
							a.GetAttributeValue<EntityReference>("msa_managingpartnerid")
								.Equals(Contact.GetAttributeValue<EntityReference>("parentcustomerid")))
					.OrderBy(a => a.GetAttributeValue<string>("name"));
			
			CustomerFilter.Items.Add(new ListItem("All"));

			CustomerFilter.Items.Add(new ListItem("My"));

			foreach (var account in accounts)
			{
				CustomerFilter.Items.Add(new ListItem(account.GetAttributeValue<string>("name"), account.GetAttributeValue<Guid>("accountid").ToString()));
			}
		}
	}
}
