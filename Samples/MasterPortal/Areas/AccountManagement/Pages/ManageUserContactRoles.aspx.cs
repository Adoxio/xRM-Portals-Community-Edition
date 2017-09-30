/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.AccountManagement.Pages
{
	public partial class ManageUserContactRoles : PortalPage
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

				ManagePermissions.Visible = false;

				return;
			}

			var parentAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var accountAccessPermissions = parentAccount == null ? new List<Entity>() : ServiceContext.GetAccountAccessByContact(Contact).ToList();
			var accountAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : accountAccessPermissions.Where(a => a.GetAttributeValue<EntityReference>("adx_accountid") != null && a.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference())).ToList();
			var contactAccessPermissions = parentAccount == null ? new List<Entity>() : XrmContext.GetContactAccessByContact(Contact).ToList();
			var contactAccessPermissionsForParentAccount = parentAccount == null ? new List<Entity>() : contactAccessPermissions.Where(c => c.GetAttributeValue<EntityReference>("adx_accountid") != null && c.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference()) && c.GetAttributeValue<OptionSetValue>("adx_scope").Value == (int)Enums.ContactAccessScope.Account).ToList();
			var channelPermissions = XrmContext.GetChannelAccessByContact(Contact);
			var opportunityPermissions = XrmContext.GetOpportunityAccessByContact(Contact).FirstOrDefault();
			var contactToEditOpportunityPermissions = XrmContext.CreateQuery("adx_opportunitypermissions").FirstOrDefault(c => c.GetAttributeValue<EntityReference>("adx_contactid") != null && c.GetAttributeValue<EntityReference>("adx_contactid").Equals(ContactToEdit.ToEntityReference()));
			var contactToEditChannelPermissions = XrmContext.CreateQuery("adx_channelpermissions").FirstOrDefault(c => c.GetAttributeValue<EntityReference>("adx_contactid") != null && c.GetAttributeValue<EntityReference>("adx_contactid").Equals(ContactToEdit.ToEntityReference()));
			var canEditContacts = false;
			var canManagePermissions = false;
			var canEditChannelPermissions = channelPermissions != null;
			var canEditOpportunityPermissions = opportunityPermissions != null;
			
			if (parentAccount == null)
			{
				ManagePermissions.Visible = false;

				NoParentAccountError.Visible = true;

				return;
			}

			if (!accountAccessPermissions.Any())
			{
				ManagePermissions.Visible = false;

				NoAccountAccessPermissionsRecordError.Visible = true;

				return;
			}

			if (!accountAccessPermissionsForParentAccount.Any())
			{
				ManagePermissions.Visible = false;

				NoAccountAccessPermissionsForParentAccountError.Visible = true;

				return;
			}

			if (!contactAccessPermissions.Any())
			{
				ManagePermissions.Visible = false;

				NoContactAccessPermissionsRecordError.Visible = true;

				return;
			}

			if (!contactAccessPermissionsForParentAccount.Any())
			{
				ManagePermissions.Visible = false;

				NoContactAccessPermissionsForParentAccountError.Visible = true;

				return;
			}

			foreach (var access in accountAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_managepermissions").GetValueOrDefault(false))
				{
					canManagePermissions = true;
				}
			}

			if (!canManagePermissions)
			{
				ManagePermissions.Visible = false;

				AccountAccessManagePermissionsDeniedError.Visible = true;

				return;
			}

			foreach (var access in contactAccessPermissionsForParentAccount)
			{
				if (access.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false))
				{
					canEditContacts = true;
				}
			}

			if (!canEditContacts)
			{
				ManagePermissions.Visible = false;

				ContactAccessPermissionsError.Visible = true;

				return;
			}

			if (channelPermissions == null && opportunityPermissions == null)
			{
				ManagePermissions.Visible = false;

				NoPermissionsError.Visible = true;

				return;
			}

			if (contactToEditOpportunityPermissions == null & contactToEditChannelPermissions == null)
			{
				ManagePermissions.Visible = false;

				PermissionsRecordsForContactEditNotFoundError.Visible = true;

				return;
			}

			if (!canEditOpportunityPermissions)
			{
				OpportunityPermissions.Visible = false;

				NoOpportunityPermissionsRecordWarning.Visible = true;
			}
			else
			{
				if (contactToEditOpportunityPermissions == null)
				{
					OpportunityPermissions.Visible = false;

					OpportunityPermissionsRecordForContactEditNotFoundError.Visible = true;
				}
				else
				{
					var opportunityPermissionsFormViewDataSource = new CrmDataSource { ID = "OpportunityPermissionsFormViewDataSource", CrmDataContextName = OpportunityPermissionsFormView.ContextName };

					var opportunityPermissionsFetchXml =
						string.Format(
							"<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>",
							"adx_opportunitypermissions", "adx_opportunitypermissionsid", contactToEditOpportunityPermissions.Id);

					opportunityPermissionsFormViewDataSource.FetchXml = opportunityPermissionsFetchXml;

					OpportunityPermissions.Controls.Add(opportunityPermissionsFormViewDataSource);

					OpportunityPermissionsFormView.DataSourceID = "OpportunityPermissionsFormViewDataSource";
				}
			}

			if (!canEditChannelPermissions)
			{
				ChannelPermissions.Visible = false;

				NoChannelPermissionsRecordWarning.Visible = true;
			}
			else
			{
				if (contactToEditChannelPermissions == null)
				{
					ChannelPermissions.Visible = false;

					ChannelPermissionsRecordForContactEditNotFoundError.Visible = true;
				}
				else
				{
					var channelPermissionsFormViewDataSource = new CrmDataSource { ID = "ChannelPermissionsWebViewDataSource", CrmDataContextName = ChannelPermissionsFormView.ContextName };

					var channelPermissionsFetchXml =
						string.Format(
							"<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>",
							"adx_channelpermissions", "adx_channelpermissionsid", contactToEditChannelPermissions.Id);

					channelPermissionsFormViewDataSource.FetchXml = channelPermissionsFetchXml;

					ChannelPermissions.Controls.Add(channelPermissionsFormViewDataSource);

					ChannelPermissionsFormView.DataSourceID = "ChannelPermissionsWebViewDataSource";
				}
			}
			
			if (!IsPostBack)
			{
				SetControlValues(contactToEditOpportunityPermissions, contactToEditChannelPermissions);
			}

			EnableControlsBasedOnPartnerAccess(opportunityPermissions, channelPermissions);
		}

		public void SetControlValues(Entity opportunitypermissions, Entity channelpermissions)
		{
			if (opportunitypermissions != null)
			{
				var oppCreate = opportunitypermissions.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false);
				var oppDelete = opportunitypermissions.GetAttributeValue<bool?>("adx_delete").GetValueOrDefault(false);
				var oppAcceptDecline = opportunitypermissions.GetAttributeValue<bool?>("adx_acceptdecline").GetValueOrDefault(false);
				var oppAssign = opportunitypermissions.GetAttributeValue<bool?>("adx_assign").GetValueOrDefault(false);

				OppCreateCheckBox.Checked = oppCreate;
				OppDeleteCheckBox.Checked = oppDelete;
				OppAcceptDeclineCheckBox.Checked = oppAcceptDecline;
				OppAssignCheckBox.Checked = oppAssign;
			}
			
			if (channelpermissions != null)
			{
				var channelWrite = channelpermissions.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false);
				var channelCreate = channelpermissions.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false);

				ChannelWriteCheckBox.Checked = channelWrite;
				ChannelCreateCheckBox.Checked = channelCreate;
			}
		}

		public void EnableControlsBasedOnPartnerAccess(Entity partnerOppPerms, Entity partnerChannelPerms)
		{
			if (partnerOppPerms != null)
			{
				var partnerOppCreate = partnerOppPerms.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false);
				var partnerOppDelete = partnerOppPerms.GetAttributeValue<bool?>("adx_delete").GetValueOrDefault(false);
				var partnerOppAcceptDecline = partnerOppPerms.GetAttributeValue<bool?>("adx_acceptdecline").GetValueOrDefault(false);
				var partnerOppAssign = partnerOppPerms.GetAttributeValue<bool?>("adx_assign").GetValueOrDefault(false);

				OppCreateCheckBox.Enabled = partnerOppCreate;
				OppDeleteCheckBox.Enabled = partnerOppDelete;
				OppAcceptDeclineCheckBox.Enabled = partnerOppAcceptDecline;
				OppAssignCheckBox.Enabled = partnerOppAssign;
			}

			if (partnerChannelPerms != null)
			{
				var partnerChannelWrite = partnerChannelPerms.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false);
				var partnerChannelCreate = partnerChannelPerms.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false);

				ChannelWriteCheckBox.Enabled = partnerChannelWrite;
				ChannelCreateCheckBox.Enabled = partnerChannelCreate;
			}
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			if (ChannelPermissionsFormView.Visible)
			{
				ChannelPermissionsFormView.UpdateItem();
			}

			if (OpportunityPermissionsFormView.Visible)
			{
				OpportunityPermissionsFormView.UpdateItem();
			}
		}

		protected void OppPermissionsUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			e.Values["adx_create"] = OppCreateCheckBox.Checked;
			e.Values["adx_delete"] = OppDeleteCheckBox.Checked;
			e.Values["adx_acceptdecline"] = OppAcceptDeclineCheckBox.Checked;
			e.Values["adx_assign"] = OppAssignCheckBox.Checked;
		}

		protected void OppPermissionsUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected void ChannelPermissionsUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			e.Values["adx_write"] = ChannelWriteCheckBox.Checked;
			e.Values["adx_create"] = ChannelCreateCheckBox.Checked;
		}

		protected void ChannelPermissionsUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}
	}
}
