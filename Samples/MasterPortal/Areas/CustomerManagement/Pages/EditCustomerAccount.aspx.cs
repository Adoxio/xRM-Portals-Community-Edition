/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;
using Site.Pages;
using System.Web;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class EditCustomerAccount : PortalPage
	{
		private DataTable _contacts;

		private Entity _account;

		public Entity AccountToEdit
		{
			get
			{
				if (_account != null)
				{
					return _account;
				}

				Guid accountId;

				if (!Guid.TryParse(Request["AccountID"], out accountId))
				{
					return null;
				}

				_account = XrmContext.CreateQuery("account").FirstOrDefault(c => c.GetAttributeValue<Guid>("accountid") == accountId);

				return _account;
			}
		}

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

			if (AccountToEdit == null)
			{
				RecordNotFoundError.Visible = true;

				EditAccountForm.Visible = false;

				return;
			}

			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = FormView.ContextName };
			var managingPartnerAccount = ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == (AccountToEdit.GetAttributeValue<EntityReference>("msa_managingpartnerid") == null ? Guid.Empty : AccountToEdit.GetAttributeValue<EntityReference>("msa_managingpartnerid").Id));
			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelCreateAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));
			var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));
			var channelPermissionForParentAccountExists = managingPartnerAccount != null && channelPermission != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid") != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid").Equals(managingPartnerAccount.ToEntityReference());
			var validAcccountClassificationCode = managingPartnerAccount != null && managingPartnerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && managingPartnerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)Enums.AccountClassificationCode.Partner;
			
			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				EditAccountForm.Visible = false;

				ContactsList.Visible = false;

				return;
			}

			if (!channelWriteAccess)
			{
				ChannelPermissionsError.Visible = true;
			}
			else
			{
				if (managingPartnerAccount == null)
				{
					NoParentAccountError.Visible = true;
				}
				else if (!validAcccountClassificationCode)
				{
					ParentAccountClassificationCodeError.Visible = true;
				}
				else if (!channelPermissionForParentAccountExists)
				{
					NoChannelPermissionsForParentAccountError.Visible = true;
				}
			}

			if (!channelWriteAccess || managingPartnerAccount == null || !channelPermissionForParentAccountExists || !validAcccountClassificationCode)
			{
				EditAccountForm.Visible = false;

				ContactsList.Visible = false;

				return;
			}

			CreateContactButton.Visible = channelCreateAccess;

			CreateContactButton.QueryStringCollection = CreateCustomerContactQueryStringCollection();

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "account", "accountid", AccountToEdit.GetAttributeValue<Guid>("accountid"));

			formViewDataSource.FetchXml = fetchXml;

			AccountForm.Controls.Add(formViewDataSource);

			FormView.DataSourceID = "WebFormDataSource";

			if (!IsPostBack)
			{
				BindPrimaryContact();

				var contacts = ServiceContext.CreateQuery("contact").Where(c => c.GetAttributeValue<EntityReference>("parentcustomerid") != null && c.GetAttributeValue<EntityReference>("parentcustomerid").Equals(AccountToEdit.ToEntityReference()));

				_contacts = EnumerableExtensions.CopyToDataTable(contacts.Select(c => new
				{
					contactid = c.GetAttributeValue<Guid>("contactid"),
					ID = c.GetAttributeValue<string>("fullname"),
					CompanyName = c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")) == null ? String.Empty : c.GetRelatedEntity(ServiceContext, new Relationship("contact_customer_accounts")).GetAttributeValue<string>("name"),
					City = c.GetAttributeValue<string>("address1_city"),
					State = c.GetAttributeValue<string>("address1_stateorprovince"),
					Phone = c.GetAttributeValue<string>("address1_telephone1"),
					Email = c.GetAttributeValue<string>("emailaddress1"),
				}).ToList().OrderBy(c => c.CompanyName));

				_contacts.Columns["City"].ColumnName = "City";
				_contacts.Columns["State"].ColumnName = "State";
				_contacts.Columns["Phone"].ColumnName = "Phone";
				_contacts.Columns["Email"].ColumnName = "E-mail Address";

				AccountContactsList.DataKeyNames = new[] { "contactid" };
				AccountContactsList.DataSource = _contacts;
				AccountContactsList.DataBind();
			}
		}

		protected void SetPrimaryContactButton_Click(object sender, EventArgs e)
		{
			var selectedGuid = new Guid(PrimaryContactList.SelectedItem.Value);

			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == selectedGuid);

			if (contact == null)
			{
				throw new ApplicationException(string.Format("Couldn't find contact with contactid equal to {0}.", selectedGuid));
			}

			var account = XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == AccountToEdit.GetAttributeValue<Guid>("accountid"));

			if (account == null)
			{
				throw new ApplicationException(string.Format("An account with the account ID equal to {0} couldn’t be found.", AccountToEdit.GetAttributeValue<Guid>("accountid")));
			}

			account.SetAttributeValue("primarycontactid", contact.ToEntityReference());

			XrmContext.UpdateObject(contact);
			XrmContext.UpdateObject(account);
			XrmContext.SaveChanges();

			SuccessMessage.Visible = true;
		}

		private void BindPrimaryContact()
		{
			var empli = new ListItem
			{
				Text = " ",
			};

			PrimaryContactList.Items.Add(empli);

			var contacts = ServiceContext.CreateQuery("contact").Where(c => c.GetAttributeValue<EntityReference>("parentcustomerid") != null && c.GetAttributeValue<EntityReference>("parentcustomerid").Equals(AccountToEdit.ToEntityReference())).ToList();

			foreach (var contact in contacts)
			{
				var li = new ListItem
				{
					Text = contact.GetAttributeValue<string>("fullname"),
					Value = contact.GetAttributeValue<Guid>("contactid").ToString()
				};

				if ((AccountToEdit.GetAttributeValue<EntityReference>("primarycontactid") != null) && (li.Value == AccountToEdit.GetAttributeValue<EntityReference>("primarycontactid").Id.ToString()))
				{
					li.Selected = true;
				}

				PrimaryContactList.Items.Add(li);
			}

			if (contacts.Count < 1)
			{
				PrimaryContactList.Enabled = false;

				NoContactsExistWarningMessage.Visible = true;
			}
		}

		protected void UpdateAccountButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			FormView.UpdateItem();
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			if (PrimaryContactList.SelectedItem == null || PrimaryContactList.SelectedIndex == 0)
			{
				e.Values["primarycontactid"] = null;

				return;
			}

			Guid contactId;

			if (!Guid.TryParse(PrimaryContactList.SelectedItem.Value, out contactId))
			{
				return;
			}
			
			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == contactId);

			if (contact == null)
			{
				throw new ApplicationException(string.Format("Couldn't find contact with contactid equal to {0}.", contactId));
			}

			e.Values["primarycontactid"] = contact.ToEntityReference();
		}

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			SuccessMessage.Visible = true;
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
			var url = GetUrlForRequiredSiteMarker("Edit Customer Contact");

			url.QueryString.Set("ContactID", id.ToString());

			return url.PathWithQueryString;
		}

		protected QueryStringCollection CreateCustomerContactQueryStringCollection()
		{
			var id = AccountToEdit.GetAttributeValue<Guid>("accountid");

			var queryStringCollection = new QueryStringCollection("");

			queryStringCollection.Set("AccountID", id.ToString());

			queryStringCollection.Set("ReturnToAccount", "true");

			return queryStringCollection;
		}
	}
}
