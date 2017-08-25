/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Web.UI.WebControls;
using Site.Pages;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class ReadOnlyAccountView : PortalPage
	{
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

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (AccountToEdit == null)
			{
				RecordNotFoundError.Visible = true;

				AccountForm.Visible = false;

				return;
			}

			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = FormView.ContextName };
			var managingPartnerAccount = ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == (AccountToEdit.GetAttributeValue<EntityReference>("msa_managingpartnerid") == null ? Guid.Empty : AccountToEdit.GetAttributeValue<EntityReference>("msa_managingpartnerid").Id));
			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelReadAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));
			var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));
			var channelPermissionForParentAccountExists = managingPartnerAccount != null && channelPermission != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid") != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid").Equals(managingPartnerAccount.ToEntityReference());
			var validAcccountClassificationCode = managingPartnerAccount != null && managingPartnerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && managingPartnerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)Enums.AccountClassificationCode.Partner;

			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				AccountForm.Visible = false;

				return;
			}

			if (!channelReadAccess || !channelWriteAccess)
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
				AccountForm.Visible = false;

				return;
			}

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "account", "accountid", AccountToEdit.GetAttributeValue<Guid>("accountid"));

			formViewDataSource.FetchXml = fetchXml;

			AccountForm.Controls.Add(formViewDataSource);

			FormView.DataSourceID = "WebFormDataSource";
		}
	}
}
