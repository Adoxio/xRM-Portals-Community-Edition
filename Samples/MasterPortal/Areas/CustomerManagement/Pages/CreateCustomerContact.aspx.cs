/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.CustomerManagement.Pages
{
	public partial class CreateCustomerContact : PortalPage
	{
		private Entity _account;

		private Entity _opportunity;

		public Entity ParentCustomerAccount
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

				_account = XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == accountId);

				return _account;
			}
		}

		public Entity OriginatingOpportunity
		{
			get
			{
				if (_opportunity != null)
				{
					return _opportunity;
				}

				Guid oppId;

				if (!Guid.TryParse(Request["OpportunityID"], out oppId))
				{
					return null;
				}

				_opportunity = XrmContext.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == oppId);

				return _opportunity;
			}
		}

		public bool ReturnToAccount
		{
			get
			{
				bool b = bool.TryParse(Request["ReturnToAccount"], out b);
				return b;
			}
		}

		public bool SetAsPrimary
		{
			get
			{
				bool b = bool.TryParse(Request["SetAsPrimary"], out b);
				return b;
			}
		}

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

				CreateContactForm.Visible = false;

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
				CreateContactForm.Visible = false;

				return;
			}

			PopulateDropDownList(parentCustomerAccount.ToEntityReference());
		}

		protected void CreateContactButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}

			ContactFormView.InsertItem();
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e) { }

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var contact = XrmContext.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == e.EntityId);

			if (contact == null)
			{
				throw new Exception(string.Format("A contact couldn't be found with a contact ID equal to {0}.", e.EntityId));
			}

			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);

			if (parentCustomerAccount == null)
			{
				throw new Exception("Parent Customer Account could not be found associated to the current user's contact.");
			}

			Guid accountId = (Guid.TryParse(CompanyNameList.SelectedValue, out accountId)) ? accountId : Guid.Empty;
			
			var account = XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == accountId);

			var opportunity = OriginatingOpportunity == null ? null : XrmContext.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == OriginatingOpportunity.Id);

			if (opportunity != null)
			{
				var contactCrossover = opportunity.GetRelatedEntities(XrmContext, new Relationship("adx_opportunity_contact")).FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid"));

				var oppnote = new Entity("adx_opportunitynote");

				if (contactCrossover == null)
				{
					XrmContext.AddLink(opportunity, new Relationship("adx_opportunity_contact"), contact);

					oppnote.SetAttributeValue("adx_name", "Contact Added: " + contact.GetAttributeValue<string>("fullname"));
					oppnote.SetAttributeValue("adx_date", DateTime.UtcNow);
					oppnote.SetAttributeValue("adx_description", "Contact Added: " + contact.GetAttributeValue<string>("fullname"));
					oppnote.SetAttributeValue("adx_opportunityid", opportunity.ToEntityReference());
					var assignedToContact = opportunity.GetRelatedEntity(XrmContext, new Relationship("msa_contact_opportunity"));
					oppnote.SetAttributeValue("adx_assignedto", assignedToContact != null ? assignedToContact.GetAttributeValue<string>("fullname") : string.Empty);
					XrmContext.AddObject(oppnote);
					XrmContext.UpdateObject(opportunity);
				}
			}

			contact.SetAttributeValue("msa_managingpartnerid", parentCustomerAccount.ToEntityReference());

			if (account != null)
			{
				contact.SetAttributeValue("parentcustomerid", account.ToEntityReference());

				XrmContext.UpdateObject(account);
			}

			XrmContext.UpdateObject(contact);
			
			if (SetAsPrimary)
			{
				if (account == null)
				{
					throw new Exception(string.Format("An account with the account ID equal to {0} couldn’t be found.", accountId));
				}

				account.SetAttributeValue("primarycontactid", contact.ToEntityReference());

				XrmContext.UpdateObject(account);
			}

			XrmContext.SaveChanges();

			if (opportunity != null)
			{
				var url = GetUrlForRequiredSiteMarker("Opportunity Details");

				url.QueryString.Set("OpportunityID", opportunity.GetAttributeValue<Guid>("opportunityid").ToString());

				Response.Redirect(url.PathWithQueryString);
			}
			else if (ReturnToAccount)
			{
				if (account == null)
				{
					throw new Exception(string.Format("An account with the account ID equal to {0} couldn’t be found.", accountId));
				}

				var url = GetUrlForRequiredSiteMarker("Edit Customer Account");

				url.QueryString.Set("AccountID", account.Id.ToString());

				Response.Redirect(url.PathWithQueryString);
			}
			else if (FromCreateOpportunity)
			{
				if (account == null)
				{
					throw new Exception(string.Format("An account with the account ID equal to {0} couldn’t be found.", accountId));
				}

				var url = GetUrlForRequiredSiteMarker("Create Opportunity");

				url.QueryString.Set("AccountId", account.Id.ToString());

				Response.Redirect(url.PathWithQueryString);
			}
			else
			{
				var url = GetUrlForRequiredSiteMarker("Manage Customer Contacts");

				Response.Redirect(url.PathWithQueryString);
			}
		}

		private void PopulateDropDownList(EntityReference managingPartner)
		{
			if (managingPartner == null)
			{
				return;
			}

			CompanyNameList.Items.Add(new ListItem("None"));

			var accounts =
				ServiceContext.CreateQuery("account")
					.Where(
						a =>
							a.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null &&
							a.GetAttributeValue<EntityReference>("msa_managingpartnerid").Equals(managingPartner))
					.OrderBy(a => a.GetAttributeValue<string>("name"));

			foreach (var account in accounts)
			{
				var li = new ListItem
				{
					Text = account.GetAttributeValue<string>("name"),
					Value = account.GetAttributeValue<Guid>("accountid").ToString()
				};

				if ((ParentCustomerAccount != null) && (account.GetAttributeValue<Guid>("accountid") == ParentCustomerAccount.GetAttributeValue<Guid>("accountid")))
				{
					li.Selected = true;
				}

				CompanyNameList.Items.Add(li);
			}
		}
	}
}
