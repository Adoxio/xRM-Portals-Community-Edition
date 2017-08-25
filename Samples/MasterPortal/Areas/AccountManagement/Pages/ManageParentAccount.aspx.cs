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
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using System.Web;

namespace Site.Areas.AccountManagement.Pages
{
	public partial class ManageParentAccount : PortalPage
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

			var parentAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var accountAccessPermissions = parentAccount == null ? new List<Entity>() : ServiceContext.GetAccountAccessByContact(Contact).ToList();
			var accountAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : accountAccessPermissions.Where(a => a.GetAttributeValue<EntityReference>("adx_accountid") != null && a.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference())).ToList();
			var contactAccessPermissions = parentAccount == null ? new List<Entity>() : ServiceContext.GetContactAccessByContact(Contact).ToList();
			var contactAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : contactAccessPermissions.Where(c => c.GetAttributeValue<EntityReference>("adx_accountid") != null && c.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference()) && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Enums.ContactAccessScope.Account).ToList();
			var canReadAccount = false;
			var canEditAccount = false;
			var canCreateContacts = false;
			var canReadContacts = false;

			if (parentAccount == null)
			{
				AccountInformation.Visible = false;

				ContactsList.Visible = false;

				NoParentAccountError.Visible = true;

				return;
			}
			
			if (!accountAccessPermissions.Any())
			{
				AccountInformation.Visible = false;

				ContactsList.Visible = false;

				NoAccountAccessPermissionsRecordError.Visible = true;

				return;
			}

			if (!accountAccessPermissionsForParentAccount.Any())
			{
				AccountInformation.Visible = false;

				ContactsList.Visible = false;

				NoAccountAccessPermissionsForParentAccountError.Visible = true;

				return;
			}

			foreach (var access in accountAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false))
				{
					canEditAccount = true;
					canReadAccount = true;
				}
				if (access.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false))
				{
					canReadAccount = true;
				}
			}

			foreach (var access in contactAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false))
				{
					canCreateContacts = true;
					canReadContacts = true;
				}
				if (access.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false))
				{
					canReadContacts = true;
				}
			}

			if (!contactAccessPermissions.Any())
			{
				NoContactAccessPermissionsRecordMessage.Visible = true;
			}
			else
			{
				if (!contactAccessPermissionsForParentAccount.Any())
				{
					NoContactAccessPermissionsForParentAccountMessage.Visible = true;
				}
			}

			if (!canReadAccount)
			{
				AccountInformation.Visible = false;

				ContactsList.Visible = false;

				return;
			}

			if (!canEditAccount)
			{
				AccountEditForm.Visible = false;

				AccountReadOnlyForm.Visible = true;

				AccountAccessWritePermissionDeniedMessage.Visible = true;
			}

			if (!canReadContacts)
			{
				AccountContactsList.Visible = false;

				if (contactAccessPermissions.Any() && contactAccessPermissionsForParentAccount.Any())
				{
					ContactAccessPermissionsMessage.Visible = true;
				}
			}

			CreateContactButton.Visible = canCreateContacts;

			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = AccountEditFormView.ContextName };
			
			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "account", "accountid", parentAccount.GetAttributeValue<Guid>("accountid"));

			formViewDataSource.FetchXml = fetchXml;

			AccountInformation.Controls.Add(formViewDataSource);

			AccountEditFormView.DataSourceID = "WebFormDataSource";

			AccountReadOnlyFormView.DataSourceID = "WebFormDataSource";

			var contacts = ServiceContext.GetContactsForContact(Contact)
					.Where(c => c.GetAttributeValue<EntityReference>("parentcustomerid") != null && c.GetAttributeValue<EntityReference>("parentcustomerid").Equals(parentAccount.ToEntityReference()));

			_contacts = EnumerableExtensions.CopyToDataTable(contacts.Select(c => new
			{
				contactid = c.GetAttributeValue<Guid>("contactid"),
				ID = c.GetAttributeValue<string>("fullname"),
				CompanyName = c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")) == null ? string.Empty : c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")).GetAttributeValue<string>("name"),
				City = c.GetAttributeValue<string>("address1_city"),
				State = c.GetAttributeValue<string>("address1_stateorprovince"),
				Phone = c.GetAttributeValue<string>("address1_telephone1"),
				Email = c.GetAttributeValue<string>("emailaddress1"),
			}).OrderBy(opp => opp.CompanyName));

			_contacts.Columns["City"].ColumnName = "City";
			_contacts.Columns["State"].ColumnName = "State";
			_contacts.Columns["Phone"].ColumnName = "Phone";
			_contacts.Columns["Email"].ColumnName = "E-mail Address";

			AccountContactsList.DataKeyNames = new[] { "contactid" };
			AccountContactsList.DataSource = _contacts;
			AccountContactsList.DataBind();
		}

		protected void UpdateAccountButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			AccountEditFormView.UpdateItem();
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e) { }

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected void AccountContactsList_Sorting(object sender, GridViewSortEventArgs e)
		{
			SortDirection = e.SortExpression == SortExpression
				? (SortDirection == "ASC" ? "DESC" : "ASC")
				: "ASC";

			SortExpression = e.SortExpression;

			_contacts.DefaultView.Sort = string.Format("{0} {1}", SortExpression, SortDirection);

			AccountContactsList.DataSource = _contacts;
			AccountContactsList.DataBind();
		}

		protected void AccountContactsList_OnRowDataBound(object sender, GridViewRowEventArgs e)
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

			var dataKey = AccountContactsList.DataKeys[e.Row.RowIndex].Value;

			e.Row.Cells[1].Text = string.Format(@"<a href=""{0}"">{1}</a>",
					HttpUtility.HtmlEncode(ContactDetailsUrl(dataKey)),
					HttpUtility.HtmlEncode(e.Row.Cells[1].Text));

			e.Row.Cells[1].Attributes.Add("style", "white-space: nowrap;");
		}

		protected string ContactDetailsUrl(object id)
		{
			var url = GetUrlForRequiredSiteMarker("Edit Portal User");

			url.QueryString.Set("ContactID", id.ToString());

			return url.PathWithQueryString;
		}

		protected void CreateContactButton_Click(object sender, EventArgs args)
		{
			var url = GetUrlForRequiredSiteMarker("Create Portal Contact");

			Response.Redirect(url.PathWithQueryString);
		}
	}
}
