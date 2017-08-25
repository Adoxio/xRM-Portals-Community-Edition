/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class CreateCustomerAccount : PortalPage
	{
		public bool FromCreateOpportunity
		{
			get
			{
				bool b = bool.TryParse(Request["FromCreateOpportunity"], out b);
				return b;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelCreateAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));
			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var channelPermissionForParentAccountExists = parentCustomerAccount != null && channelPermission != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid") != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentCustomerAccount.ToEntityReference());
			var validAcccountClassificationCode = parentCustomerAccount != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)Enums.AccountClassificationCode.Partner;

			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				CreateAccountForm.Visible = false;

				return;
			}

			if (!channelCreateAccess)
			{
				ChannelPermissionsError.Visible = true;
			}
			else
			{
				if (parentCustomerAccount == null)
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

			if ((!channelCreateAccess) || parentCustomerAccount == null || !channelPermissionForParentAccountExists || !validAcccountClassificationCode)
			{
				CreateAccountForm.Visible = false;
			}
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e) { }

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var account = XrmContext.CreateQuery("account").FirstOrDefault(c => c.GetAttributeValue<Guid>("accountid") == e.EntityId);

			if (account == null)
			{
				throw new Exception(string.Format("An account with the account ID equal to {0} couldn’t be found.", e.EntityId));
			}

			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);

			if (parentCustomerAccount == null)
			{
				throw new Exception("Parent Customer Account could not be found associated to the current user's contact.");
			}

			account.SetAttributeValue("msa_managingpartnerid", parentCustomerAccount.ToEntityReference());

			var accountId = account.GetAttributeValue<Guid>("accountid");

			XrmContext.UpdateObject(account);

			XrmContext.SaveChanges();

			var url = GetUrlForRequiredSiteMarker("Create Customer Contact");

			url.QueryString.Set("AccountId", accountId.ToString());

			url.QueryString.Set("SetAsPrimary", "true");

			url.QueryString.Set(FromCreateOpportunity ? "FromCreateOpportunity" : "ReturnToAccount", "true");

			Response.Redirect(url.PathWithQueryString);
		}

		protected void CreateAccountButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			AccountFormView.InsertItem();
		}
	}
}
