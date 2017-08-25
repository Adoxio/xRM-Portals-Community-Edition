/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Partner;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Site.Pages;

namespace Site.Areas.Opportunities.Pages
{
	public partial class CreateNewOpportunity : PortalPage
	{

		public TextBox PotentialCustomer
		{
			get { return (TextBox)createOpp.FindControl("PotentialCustomer"); }
		}

		private Entity _account;

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

				_account = XrmContext.CreateQuery("account").FirstOrDefault(c => c.GetAttributeValue<Guid>("accountid") == accountId);

				return _account;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (Page.IsPostBack)
			{
				return;
			}
			
			var opportunityPermissions = XrmContext.GetOpportunityAccessByContact(Contact).ToList();

			if (!opportunityPermissions.Any())
			{
				NoOpportunityPermissionsRecordError.Visible = true;

				OpportunityDetailsPanel.Visible = false;

				return;
			}

			bool createAccess = false;

			foreach (var access in opportunityPermissions)
			{
				var opportunityCreateAccess = (access != null && access.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));

				if (opportunityCreateAccess)
				{
					createAccess = true;
				}
			}

			if (!createAccess)
			{
				OpportunityPermissionsError.Visible = true;

				OpportunityDetailsPanel.Visible = false;

				return;
			}

			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);
			var channelCreateAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_create").GetValueOrDefault(false));
			var parentCustomerAccount = Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? null : ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
			var validAcccountClassificationCode = parentCustomerAccount != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && parentCustomerAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == (int)CustomerManagement.Enums.AccountClassificationCode.Partner;

			if (channelPermission == null)
			{
				NoChannelPermissionsRecordError.Visible = true;

				CreateCustomerButton.Visible = false;
			}
			else
			{
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
					else
					{
						ParentAccountClassificationCodeError.Visible = !validAcccountClassificationCode;
					}
				}

				if ((!channelCreateAccess) || parentCustomerAccount == null || !validAcccountClassificationCode)
				{
					CreateCustomerButton.Visible = false;
				}
			}

			var partnerAccounts = ServiceContext.CreateQuery("account").Where(a => a.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null && a.GetAttributeValue<EntityReference>("msa_managingpartnerid").Equals(parentCustomerAccount.ToEntityReference())).ToList();

			var accounts = partnerAccounts
				.Where(a => a.GetAttributeValue<EntityReference>("primarycontactid") != null)
				.OrderBy(a => a.GetAttributeValue<string>("name"))
				.Select(a => new ListItem(a.GetAttributeValue<string>("name"), a.GetAttributeValue<Guid>("accountid").ToString())).ToList();

			if (!accounts.Any())
			{
				Account_dropdown.Enabled = false;
				
				if (!partnerAccounts.Any())
				{
					NoManagingPartnerCustomerAccountsMessage.Visible = true;
				}
				else
				{
					NoPrimaryContactOnManagingPartnerCustomerAccountsMessage.Visible = true;

					ManageCustomersButton.Visible = true;
				}
			}
			else
			{
				NoManagingPartnerCustomerAccountsMessage.Visible = false;

				NoPrimaryContactOnManagingPartnerCustomerAccountsMessage.Visible = false;

				Account_dropdown.DataSource = accounts;

				Account_dropdown.DataTextField = "Text";

				Account_dropdown.DataValueField = "Value";

				Account_dropdown.DataBind();

				if (ParentCustomerAccount != null)
				{
					Account_dropdown.ClearSelection();

					foreach (ListItem li in Account_dropdown.Items)
					{
						Guid id;

						if (Guid.TryParse(li.Value, out id) && id == ParentCustomerAccount.GetAttributeValue<Guid>("accountid"))
						{
							li.Selected = true;
						}
					}
				}
			}
		}

		protected void SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(Account_dropdown.SelectedValue))
			{
				createOpp.Visible = true;
				CreateCustomerButton.Visible = false;
			}
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var currentContact = XrmContext.MergeClone(Contact);

			if (currentContact == null)
			{
				return;
			}

			var contactParentCustomerAccount = currentContact.GetRelatedEntity(XrmContext, new Relationship("contact_customer_accounts"));

			if (contactParentCustomerAccount != null)
			{
				e.Values["msa_partnerid"] = contactParentCustomerAccount.ToEntityReference();
			}

			e.Values["msa_partneroppid"] = currentContact.ToEntityReference();

			e.Values["adx_createdbyusername"] = Contact.GetAttributeValue<string>("fullname");

			// e.Values["adx_createdbyipaddress"] = Request.UserHostAddress ?? "";

			e.Values["adx_partnercreated"] = true;

			// If no estimated revenue was submitted, leave as system-calculated.

			e.Values["isrevenuesystemcalculated"] = (!e.Values.ContainsKey("estimatedvalue")) || (e.Values["estimatedvalue"] == null);

			Guid accountId;

			if ((Account_dropdown.SelectedItem == null || Account_dropdown.SelectedIndex == 0) || !Guid.TryParse(Account_dropdown.SelectedItem.Value, out accountId))
			{
				return;
			}

			var account = XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == accountId);

			if (account != null)
			{
				e.Values["customerid"] = account.ToEntityReference();
			}
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var opportunity = XrmContext.CreateQuery("opportunity").First(o => o.GetAttributeValue<Guid>("opportunityid") == e.EntityId);

			var opportunityProductsFromLead = opportunity.GetAttributeValue<string>("adx_opportunityproductsfromlead");

			var productsList = new List<Entity>();

			if (!String.IsNullOrEmpty(opportunityProductsFromLead))
			{
				var products = XrmContext.CreateQuery("product");

				var words = opportunityProductsFromLead.Split(',');

				foreach (var word in words)
				{
					foreach (var product in products)
					{
						if (product.GetAttributeValue<string>("name").Trim().ToUpper() == word.Trim().ToUpper())
						{
							productsList.Add(product);
						}
					}
				}
			}

			foreach (var leadProduct in productsList)
			{
				if (!XrmContext.IsAttached(leadProduct))
				{
					XrmContext.Attach(leadProduct);
				}

				XrmContext.AddLink(opportunity, new Relationship("adx_opportunity_product"), leadProduct);
			}

			opportunity.SetAttributeValue("adx_referencecode", GetOpportunityReferenceCode());

			var salesStage = opportunity.GetAttributeValue<OptionSetValue>("salesstagecode") == null ? 0 : opportunity.GetAttributeValue<OptionSetValue>("salesstagecode").Value;

			var response = (RetrieveAttributeResponse)ServiceContext.Execute(new RetrieveAttributeRequest
			{
				EntityLogicalName = "opportunity",
				LogicalName = "salesstagecode"
			});

			var picklist = response.AttributeMetadata as PicklistAttributeMetadata;
			if (picklist == null)
			{
				return;
			}

			foreach (var option in picklist.OptionSet.Options)
			{
				if (option != null && option.Value != null && option.Value.Value == salesStage)
				{
					opportunity.SetAttributeValue("stepname", option.Label.GetLocalizedLabelString());
				}
			}

			var leadType = XrmContext.CreateQuery("adx_leadtype").FirstOrDefault(lt => lt.GetAttributeValue<string>("adx_name") == "Partner Created");

			if (leadType != null)
			{
				opportunity.SetAttributeValue("adx_leadtypeid", leadType.ToEntityReference());
			}
			
			XrmContext.UpdateObject(opportunity);

			XrmContext.SaveChanges();

			var url = GetUrlForRequiredSiteMarker("Accepted Opportunities");

			Response.Redirect(url);
		}

		protected void CreateCustomerButton_Click(object sender, EventArgs args)
		{
			var url = GetUrlForRequiredSiteMarker("Create Customer Account");

			url.QueryString.Set("FromCreateOpportunity", "true");

			Response.Redirect(url.PathWithQueryString);
		}

		protected void ManageCustomerButton_Click(object sender, EventArgs args)
		{
			var url = GetUrlForRequiredSiteMarker("Manage Customer Accounts");

			Response.Redirect(url.PathWithQueryString);
		}

		public string GetOpportunityReferenceCode()
		{
			var str = "OPP-";
			var random = new Random();
			for (var i = 0; i < 12; i++)
			{
				var r = random.Next(0x10);

				char s = ' ';

				switch (r)
				{
					case 0: s = '0'; break;
					case 1: s = '1'; break;
					case 2: s = '2'; break;
					case 3: s = '3'; break;
					case 4: s = '4'; break;
					case 5: s = '5'; break;
					case 6: s = '6'; break;
					case 7: s = '7'; break;
					case 8: s = '8'; break;
					case 9: s = '9'; break;
					case 10: s = 'A'; break;
					case 11: s = 'B'; break;
					case 12: s = 'C'; break;
					case 13: s = 'D'; break;
					case 14: s = 'E'; break;
					case 15: s = 'F'; break;
				}

				str = str + s;
			}

			var uuid = str;

			return uuid;
		}
	}
}
