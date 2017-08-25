/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Account;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.AccountManagement.Pages
{
	public partial class CreatePortalContact : PortalPage
	{
		protected bool Invite
		{
			get { return ViewState["Invite"] as bool? ?? false; }
			set { ViewState["Invite"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var parentAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var contactAccessPermissions = parentAccount == null ? new List<Entity>() : XrmContext.GetContactAccessByContact(Contact).ToList();
			var contactAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : contactAccessPermissions.Where(c => c.GetAttributeValue<EntityReference>("adx_accountid") != null && c.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference()) && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Enums.ContactAccessScope.Account).ToList();
			var canCreateContacts = false;
			
			if (parentAccount == null)
			{
				ContactWebForm.Visible = false;

				NoParentAccountError.Visible = true;

				return;
			}

			if (!contactAccessPermissions.Any())
			{
				ContactWebForm.Visible = false;

				NoContactAccessPermissionsRecordError.Visible = true;

				return;
			}

			if (!contactAccessPermissionsForParentAccount.Any())
			{
				ContactWebForm.Visible = false;

				NoContactAccessPermissionsForParentAccountError.Visible = true;

				return;
			}

			foreach (var access in contactAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false))
				{
					canCreateContacts = true;
				}
			}

			if (!canCreateContacts)
			{
				ContactWebForm.Visible = false;

				ContactAccessPermissionsError.Visible = true;
			}
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var account = XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == (Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? Guid.Empty : Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id));

			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == e.EntityId);

			if (account == null || contact == null)
			{
				throw new Exception("Unable to retrieve account or contact for the logged in user.");
			}

			contact.SetAttributeValue("parentcustomerid", account.ToEntityReference());

			//XrmContext.UpdateObject(account);

			if (Invite)
			{
				InviteContact(contact);
			}

			XrmContext.UpdateObject(contact);

			XrmContext.SaveChanges();

			var url = GetUrlForRequiredSiteMarker("Manage Partner Account");
			
			Response.Redirect(url.PathWithQueryString);
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			Invite = false;

			ContactFormView.InsertItem();
		}

		protected void InviteAndSaveButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			Invite = true;

			ContactFormView.InsertItem();
		}

		protected void InviteContact(Entity contact)
		{
			var invitation = new Entity("adx_invitation");
			invitation.SetAttributeValue("adx_name", "Auto-generated email confirmation");
			invitation.SetAttributeValue("adx_type", new OptionSetValue(756150000)); // Single
			invitation.SetAttributeValue("adx_invitecontact", contact.ToEntityReference());
			invitation.SetAttributeValue("adx_invitationcode", XrmContext.CreateInvitationCode());

			var oppPermissions = new Entity("adx_opportunitypermissions");
			oppPermissions.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_name", "opportunitity permissions for " + contact.GetAttributeValue<string>("fullname"));
			oppPermissions.SetAttributeValue("adx_contactid", contact.ToEntityReference());
			oppPermissions.SetAttributeValue("adx_accountid", contact.GetAttributeValue<EntityReference>("parentcustomerid"));
			oppPermissions.SetAttributeValue("adx_scope", new OptionSetValue((int)Enums.OpportunityAccessScope.Self));
			oppPermissions.SetAttributeValue("adx_read", true);

			var channelPermissions = new Entity("adx_channelpermissions");
			channelPermissions.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_name", "channel permissions for " + contact.GetAttributeValue<string>("fullname"));
			channelPermissions.SetAttributeValue("adx_contactid", contact.ToEntityReference());
			channelPermissions.SetAttributeValue("adx_accountid", contact.GetAttributeValue<EntityReference>("parentcustomerid"));

			XrmContext.AddObject(invitation);
			XrmContext.AddObject(channelPermissions);
			XrmContext.AddObject(oppPermissions);
			XrmContext.UpdateObject(contact);
			XrmContext.SaveChanges();

			// Execute workflow to send invitation code in confirmation email
			XrmContext.ExecuteWorkflowByName(ServiceContext.GetSiteSettingValueByName(Website, "Account/EmailConfirmation/WorkflowName") ?? "ADX Sign Up Email", invitation.Id);
		}
	}
}
