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
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.AccountManagement.Pages
{
	public partial class EditPortalContact : PortalPage
	{
		private Entity _contact;

		public Entity ContactToEdit
		{
			get
			{
				if (_contact != null)
				{
					return _contact;
				}

				Guid contactId;

				if (!Guid.TryParse(Request["ContactID"], out contactId))
				{
					return null;
				}

				_contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == contactId);

				return _contact;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (ContactToEdit == null)
			{
				RecordNotFoundError.Visible = true;

				ContactInformation.Visible = false;

				return;
			}

			var parentAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var accountAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : XrmContext.GetAccountAccessByContact(Contact).Where(a => a.GetAttributeValue<EntityReference>("adx_accountid") != null && a.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference())).ToList();
			var contactAccessPermissions = parentAccount == null ? new List<Entity>() : XrmContext.GetContactAccessByContact(Contact).ToList();
			var contactAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : contactAccessPermissions.Where(c => c.GetAttributeValue<EntityReference>("adx_accountid") != null && c.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference()) && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Enums.ContactAccessScope.Account).ToList();
			var canEditContacts = false;
			var canReadContacts = false;
			var canManagePermissions = false;

			if (parentAccount == null)
			{
				ContactInformation.Visible = false;
				
				NoParentAccountError.Visible = true;

				return;
			}

			if (!contactAccessPermissions.Any())
			{
				ContactInformation.Visible = false;

				NoContactAccessPermissionsRecordError.Visible = true;

				return;
			}

			if (!contactAccessPermissionsForParentAccount.Any())
			{
				ContactInformation.Visible = false;

				NoContactAccessPermissionsForParentAccountError.Visible = true;

				return;
			}

			foreach (var access in contactAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false))
				{
					canEditContacts = true;
					canReadContacts = true;
				}
				if (access.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false))
				{
					canReadContacts = true;
				}
			}

			foreach (var access in accountAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_managepermissions").GetValueOrDefault(false))
				{
					canManagePermissions = true;
				}
			}
			
			if (!canReadContacts)
			{
				ContactInformation.Visible = false;

				return;
			}

			if (!canEditContacts)
			{
				ContactEditFormView.Visible = false;

				ContactReadOnlyFormView.Visible = true;

				ContactAccessWritePermissionDeniedMessage.Visible = true;
			}

			ManagePermissionsButton.Visible = canManagePermissions;
			
			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = ContactEditFormView.ContextName };

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "contact", "contactid", ContactToEdit.GetAttributeValue<Guid>("contactid"));

			formViewDataSource.FetchXml = fetchXml;

			ContactInformation.Controls.Add(formViewDataSource);

			ContactEditFormView.DataSourceID = "WebFormDataSource";

			ContactReadOnlyFormView.DataSourceID = "WebFormDataSource";
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e) { }

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected void UpdateButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			ContactEditFormView.UpdateItem();
		}

		protected void InviteButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			//create invitation Code

			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == ContactToEdit.GetAttributeValue<Guid>("contactid"));

			if (contact == null)
			{
				throw new ArgumentNullException(string.Format("Unable to find contact with id equal to {0}", ContactToEdit.GetAttributeValue<Guid>("contactid")));
			}

			var invitation = new Entity("adx_invitation");
			invitation.SetAttributeValue("adx_name", "Auto-generated email confirmation");
			invitation.SetAttributeValue("adx_type", new OptionSetValue(756150000)); // Single
			invitation.SetAttributeValue("adx_invitecontact", contact.ToEntityReference());
			invitation.SetAttributeValue("adx_invitationcode", XrmContext.CreateInvitationCode());

			XrmContext.AddObject(invitation);

			CreatePermissions();

			XrmContext.SaveChanges();

			// Execute workflow to send invitation code in confirmation email

			XrmContext.ExecuteWorkflowByName(ServiceContext.GetSiteSettingValueByName(Website, "Account/EmailConfirmation/WorkflowName") ?? "ADX Sign Up Email", invitation.Id);
			
			InvitationConfirmationMessage.Visible = true;
		}

		protected void ManagePermissionsButton_Click(object sender, EventArgs e)
		{
			var url = GetUrlForRequiredSiteMarker("Manage Permissions");

			var permissionsCreated = CreatePermissions();

			if (permissionsCreated)
			{
				XrmContext.SaveChanges();
			}

			var id = ContactToEdit.GetAttributeValue<Guid>("contactid");

			url.QueryString.Set("ContactID", id.ToString());

			Response.Redirect(url.PathWithQueryString);
		}

		protected bool CreatePermissions()
		{
			var changes = false;

			var oppPermissions = XrmContext.CreateQuery("adx_opportunitypermissions").FirstOrDefault(op => op.GetAttributeValue<EntityReference>("adx_contactid") != null && op.GetAttributeValue<EntityReference>("adx_contactid").Equals(ContactToEdit.ToEntityReference()));

			if (oppPermissions == null)
			{
				oppPermissions = new Entity("adx_opportunitypermissions");
				oppPermissions.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_name", "opportunitity permissions for " + ContactToEdit.GetAttributeValue<string>("fullname"));
				oppPermissions.SetAttributeValue("adx_contactid", ContactToEdit.ToEntityReference());
				oppPermissions.SetAttributeValue("adx_accountid", ContactToEdit.GetAttributeValue<EntityReference>("parentcustomerid"));
				oppPermissions.SetAttributeValue("adx_scope", new OptionSetValue((int)Enums.OpportunityAccessScope.Self));
				oppPermissions.SetAttributeValue("adx_read", true);

				XrmContext.AddObject(oppPermissions);

				changes = true;
			}

			var channelPermissions = ServiceContext.CreateQuery("adx_channelpermissions").FirstOrDefault(cp => cp.GetAttributeValue<EntityReference>("adx_contactid") != null && cp.GetAttributeValue<EntityReference>("adx_contactid").Equals(ContactToEdit.ToEntityReference()));

			if (channelPermissions == null)
			{
				channelPermissions = new Entity("adx_channelpermissions");
				channelPermissions.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_name", "channel permissions for " + ContactToEdit.GetAttributeValue<string>("fullname"));
				channelPermissions.SetAttributeValue("adx_contactid", ContactToEdit.ToEntityReference());
				channelPermissions.SetAttributeValue("adx_accountid", ContactToEdit.GetAttributeValue<EntityReference>("parentcustomerid"));

				XrmContext.AddObject(channelPermissions);

				changes = true;
			}

			return changes;
		}
	}
}
