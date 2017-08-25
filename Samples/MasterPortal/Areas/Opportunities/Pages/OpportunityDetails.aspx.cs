/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Globalization;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Site.Pages;

namespace Site.Areas.Opportunities.Pages
{
	public partial class OpportunityDetails : PortalPage
	{
		private Entity _opportunity;

		public Entity OpenOpportunity
		{
			get
			{
				if (_opportunity != null)
				{
					return _opportunity;
				}

				Guid opportunityId;

				if (!Guid.TryParse(Request["OpportunityID"], out opportunityId))
				{
					return null;
				}

				_opportunity = XrmContext.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == opportunityId);

				return _opportunity;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			var primaryContact = GetPrimaryContactAndSetCompanyName();

			if (primaryContact == null || OpenOpportunity.GetAttributeValue<OptionSetValue>("statuscode") != null && (OpenOpportunity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Delivered || OpenOpportunity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Declined))
			{
				//Push a content-snippet error message saying that the opportunity is corrupt.
				ErrorMessage.Visible = true;
				CrmEntityFormViewsPanel.Visible = false;
				OpportunityStatusPanel.Visible = false;
				return;
			}

            AddFetchXmlToDataSource(ContactWebFormDataSource, "contact", "contactid", primaryContact.Id);

            ContactWebFormDataSource.CrmDataContextName = ContactFormView.ContextName;

			ContactFormView.DataSourceID = ContactWebFormDataSource.ID;

            AddFetchXmlToDataSource(OpportunityDataSource, "opportunity", "opportunityid", OpenOpportunity.Id);

            OpportunityDataSource.CrmDataContextName = OpportunityFormView.ContextName;

			OpportunityFormView.DataSourceID = OpportunityDataSource.ID;

			//GetContactList();

			//GetLeadHistory();

			PipelinePhaseText.Text = OpenOpportunity.GetAttributeValue<string>("stepname");

			if (!IsPostBack)
			{
				GetContactList();

				GetLeadHistory();

				BindPipelinePhaseDetails();
			}

			BindProductsLeadNotesContactsAndAssignedTo();

			if (!OpenOpportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false))
			{
				CancelOpportunity.Visible = false;
				CancelDetails.Visible = false;
				//CancelButton.Visible = false;
				//AddContactCheckBox.Visible = false;
				//AddContactList.Visible = false;
			}
			else
			{
				ReturnToNetwork.Visible = false;
				ReasonForReturn.Visible = false;
				//ReasonForReturnSource.Visible = false;
			}

			AddContactButton.QueryStringCollection = CreateCustomerContactQueryString();

		}

		protected void CancelButton_Click(object sender, EventArgs e)
		{
			Response.Redirect(Request.RawUrl);
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			if (!Page.IsValid)
			{
				return;
			}
			var accessPermissions = XrmContext.GetOpportunityAccessByContact(Contact);

			var canSave = false;

			foreach (var adxOpportunitypermissionse in accessPermissions)
			{
				if (adxOpportunitypermissionse.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false))
				{
					canSave = true;
				}
			}

			if (!canSave)
			{
				return;
			}

				ContactFormView.UpdateItem();

				OpportunityFormView.UpdateItem();
			}

		protected void ContactUpdating(object senders, CrmEntityFormViewUpdatingEventArgs e)
		{

		}

		protected void OpportunityUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			if (UpdatePipelinePhase.Checked)
			{
				e.Values["stepname"] = PipelinePhase.SelectedItem.Text;
				e.Values["salesstagecode"] = int.Parse(PipelinePhase.SelectedValue);

				//var processCode = PipelinePhase.SelectedValue;
			}
			else if (ReturnToNetwork.Checked)
			{
				e.Values["closeprobability"] = 0;
				e.Values["adx_reasonforreturn"] = ReasonForReturn.SelectedIndex + 100000000;
			}

			e.Values["description"] = OpportunityNotes.Text;

			Guid id;

			if ((AssignToList != null && !String.IsNullOrEmpty(AssignToList.SelectedValue)) && Guid.TryParse(AssignToList.SelectedItem.Value, out id))
			{
				e.Values["msa_partneroppid"] = id;
			}

		}

		
		protected void OpportunityUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext();

			var opportunity = context.CreateQuery("opportunity").First(o => o.GetAttributeValue<Guid>("opportunityid") == e.Entity.Id);

			var partnerReference = opportunity.GetAttributeValue<EntityReference>("msa_partnerid");

			if (partnerReference == null)
			{
				return;
			}

			var partner = context.CreateQuery("account").First(p => p.GetAttributeValue<Guid>("accountid") == partnerReference.Id);

			if (partner.GetAttributeValue<int?>("adx_numberofopportunitiesaccepted").GetValueOrDefault(0) == 0)
			{
				partner.SetAttributeValue("adx_numberofopportunitiesaccepted", 1);
			}

			var oppnote = new Entity("adx_opportunitynote");
			var oppnote2 = new Entity("adx_opportunitynote");
			var feedbackrate = (double)(partner.GetAttributeValue<int?>("adx_numberofopportunitieswithfeedback").GetValueOrDefault(0)) / (partner.GetAttributeValue<int?>("adx_numberofopportunitiesaccepted").GetValueOrDefault(1));

			if (UpdatePipelinePhase.Checked)
			{
				context.SetOpportunityStatusAndSave(opportunity, "Open", 0);
				opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.InProgress));
				if (!opportunity.GetAttributeValue<bool?>("adx_feedbackyet").GetValueOrDefault(false))
				{
					if (!(opportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false)))
					{
						partner.SetAttributeValue("adx_numberofopportunitieswithfeedback", partner.GetAttributeValue<int?>("adx_numberofopportunitieswithfeedback").GetValueOrDefault(0) + 1);
						partner.SetAttributeValue("adx_feedbackrate", feedbackrate);
						opportunity.SetAttributeValue("adx_feedbackyet", true);
					}
				}

				oppnote.SetAttributeValue("adx_name", PipelinePhase.SelectedItem.Text);
				oppnote.SetAttributeValue("adx_date", DateTime.UtcNow);
				oppnote.SetAttributeValue("adx_description", PipelineUpdateDetails.Text);
			}
			else if (WinOpportunity.Checked)
			{
				context.SetOpportunityStatusAndSave(opportunity, "Won", 0);
				opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Purchased));
				if (!opportunity.GetAttributeValue<bool?>("adx_feedbackyet").GetValueOrDefault(false))
				{
					if (!(opportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false)))
					{
						partner.SetAttributeValue("adx_numberofopportunitieswithfeedback", partner.GetAttributeValue<int?>("adx_numberofopportunitieswithfeedback").GetValueOrDefault(0) + 1);
						partner.SetAttributeValue("adx_feedbackrate", feedbackrate);
						opportunity.SetAttributeValue("adx_feedbackyet", true);
					}
				}

				opportunity.SetAttributeValue("adx_wondate", DateTime.UtcNow);
				var wonSetting = XrmContext.CreateQuery("adx_sitesetting").FirstOrDefault(ss => ss.GetAttributeValue<string>("adx_name") == "Won Opportunity Note");
				var wonNote = "Won";
				wonNote = (wonSetting != null) ? wonSetting.GetAttributeValue<string>("adx_value") : wonNote;

				oppnote.SetAttributeValue("adx_name", wonNote);
				oppnote.SetAttributeValue("adx_date", DateTime.UtcNow);
				oppnote.SetAttributeValue("adx_description", WonDetails.Text);
			}
			else if (CancelOpportunity.Checked)
			{
				context.SetOpportunityStatusAndSave(opportunity, "Lost", 0);
				opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Canceled));
				if (!opportunity.GetAttributeValue<bool?>("adx_feedbackyet").GetValueOrDefault(false))
				{
					if (!(opportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false)))
					{
						partner.SetAttributeValue("adx_numberofopportunitieswithfeedback", partner.GetAttributeValue<int?>("adx_numberofopportunitieswithfeedback").GetValueOrDefault(0) + 1);
						partner.SetAttributeValue("adx_feedbackrate", feedbackrate);
						opportunity.SetAttributeValue("adx_feedbackyet", true);
					}
				}

				var cancelSetting = XrmContext.CreateQuery("adx_sitesetting").FirstOrDefault(ss => ss.GetAttributeValue<string>("adx_name") == "Cancel Opportunity Note");
				var cancelNote = "Canceled";
				cancelNote = (cancelSetting != null) ? cancelSetting.GetAttributeValue<string>("adx_value") : cancelNote;

				oppnote.SetAttributeValue("adx_name", cancelNote);
				oppnote.SetAttributeValue("adx_date", DateTime.UtcNow);
				oppnote.SetAttributeValue("adx_description", CancelDetails.Text);
			}
			else if (AddContactCheckBox.Checked)
			{
				var selectedGuid = new Guid(AddContactList.SelectedItem.Value);

				var contact = context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == selectedGuid);

				var contactCrossover = opportunity.GetRelatedEntities(context, new Relationship("adx_opportunity_contact")).FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == contact.GetAttributeValue<Guid>("contactid"));

				if (contactCrossover == null)
				{
					context.AddLink(opportunity, new Relationship("adx_opportunity_contact"), contact);

					oppnote2.SetAttributeValue("adx_name", "Contact Added: " + contact.GetAttributeValue<string>("fullname"));
					oppnote2.SetAttributeValue("adx_date", DateTime.UtcNow);
					oppnote2.SetAttributeValue("adx_description", "Contact Added: " + contact.GetAttributeValue<string>("fullname"));

					context.UpdateObject(contact);
				}

				//var opportunity = OpenOpportunity;
			}
			else if (ReturnToNetwork.Checked)
			{
				context.SetOpportunityStatusAndSave(opportunity, "Lost", 0);
				opportunity.SetAttributeValue("statuscode", new OptionSetValue((int)Adxstudio.Xrm.Partner.Enums.OpportunityStatusReason.Returned));
				
				if (!(opportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false)))
				{
					partner.SetAttributeValue("adx_numberofreturnedopportunities", partner.GetAttributeValue<int?>("adx_numberofreturnedopportunities").GetValueOrDefault(0) + 1);
					partner.SetAttributeValue("adx_returnrate", (double)partner.GetAttributeValue<int?>("adx_numberofreturnedopportunities").GetValueOrDefault(0) / (partner.GetAttributeValue<int?>("adx_numberofopportunitiesaccepted").GetValueOrDefault(1)));
				}

				var returnSetting = XrmContext.CreateQuery("adx_sitesetting").FirstOrDefault(ss => ss.GetAttributeValue<string>("adx_name") == "Return Opportunity Note");
				var returnNote = "Returned to Network";
				returnNote = (returnSetting != null) ? returnSetting.GetAttributeValue<string>("adx_value") : returnNote;

				oppnote.SetAttributeValue("adx_name", returnNote);
				oppnote.SetAttributeValue("adx_date", DateTime.UtcNow);
				oppnote.SetAttributeValue("adx_description", ReasonForReturn.SelectedItem.Text);

				//add the OpportunityNote entity
			}

			var calculatePartnerDetailsAction = new Entity("adx_calculatepartnercapacityworkflowaction");
			calculatePartnerDetailsAction.SetAttributeValue("adx_accountid", partner.ToEntityReference());
			var assignedto = opportunity.GetRelatedEntity(context, new Relationship("msa_contact_opportunity"));

			if (!String.IsNullOrEmpty(oppnote.GetAttributeValue<string>("adx_name")))
			{
				oppnote.SetAttributeValue("adx_opportunityid", opportunity.ToEntityReference());
				oppnote.SetAttributeValue("adx_assignedto", assignedto != null ? assignedto.GetAttributeValue<string>("fullname") : string.Empty);
				context.AddObject(oppnote);
			}
			if (!String.IsNullOrEmpty(oppnote2.GetAttributeValue<string>("adx_name")))
			{
				oppnote2.SetAttributeValue("adx_opportunityid", opportunity.ToEntityReference());
				oppnote2.SetAttributeValue("adx_assignedto", assignedto != null ? assignedto.GetAttributeValue<string>("fullname") : string.Empty);
				context.AddObject(oppnote2);
			}
			var oppID = opportunity.Id;

			context.UpdateObject(partner);
			context.UpdateObject(opportunity);
			context.SaveChanges();

			if (!(opportunity.GetAttributeValue<bool?>("adx_partnercreated").GetValueOrDefault(false)))
			{
				context.AddObject(calculatePartnerDetailsAction);
			}

			context.SaveChanges();

			var opp = context.CreateQuery("opportunity").FirstOrDefault(o => o.GetAttributeValue<Guid>("opportunityid") == oppID);

			if (opp != null)
			{
				CurrentlyAssignedToLabel.Text = assignedto != null ? assignedto.GetAttributeValue<string>("fullname") : string.Empty;
				PipelinePhaseText.Text = HttpUtility.HtmlEncode(opp.GetAttributeValue<string>("stepname"));
			}

			DisableControlsBasedOnPipelinePhaseAndAccessPermissions();

			BindPipelinePhaseDetails();

			GetLeadHistory();

			GetContactList();

			ConfirmationMessage.Visible = true;
		}

		private void AddHistoryDiv(OpportunityHistory history)
		{
			var div = new HtmlGenericControl("div")
			{
				InnerHtml = string.Format(@"<span class=""stage-date"">{0}</span><span class=""stage-name"">{1}</span>{2}{3}",
					history.NoteCreatedOn.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"),
					history.Name.Substring(history.Name.IndexOf('-') + 1),
					!string.IsNullOrEmpty(history.PartnerAssignedTo)
						? string.Format(@"<span class=""stage-assigned-to"">{0}{1}</span>", "Assigned To:  ", Server.HtmlEncode(history.PartnerAssignedTo))
						: string.Empty,
					!string.IsNullOrEmpty(history.Details)
						? string.Format(@"<div class=""stage-details"">{0}</div>", Server.HtmlEncode(history.Details))
						: string.Empty)
			};

			// add div at the beginning for reverse chronological order
			OpportunityHistoryPlaceHolder.Controls.AddAt(0, div);
		}

		private void AddContactDiv(Entity contact)
		{
			var account = (contact.GetAttributeValue<EntityReference>("parentcustomerid") != null) ?
				ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == (contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? Guid.Empty : contact.GetAttributeValue<EntityReference>("parentcustomerid").Id)) : null;

			var companyName = account != null ? account.GetAttributeValue<string>("name") : contact.GetAttributeValue<string>("adx_organizationname");

			HtmlGenericControl div;

			var channelPermission = ServiceContext.GetChannelAccessByContact(Contact);

			var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));

			var channelReadAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));

			var parentAccount = (account != null && account.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null) ?
				ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == account.GetAttributeValue<EntityReference>("msa_managingpartnerid").Id) : null;

			string contactFormattedString = "";

			if ((parentAccount != null && channelPermission != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid") != null && (channelPermission.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference()))) ||
					(contact.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null && channelPermission != null && contact.GetAttributeValue<EntityReference>("msa_managingpartnerid").Equals(channelPermission.GetAttributeValue<EntityReference>("adx_accountid"))))
			{
				if (channelWriteAccess)
				{
					contactFormattedString = string.Format(@"<i class=""fa fa-edit""></i><a href=""{0}"" class=""Edit"">{1}</a>",
						EditContactUrl(contact.GetAttributeValue<Guid>("contactid")),
						contact.GetAttributeValue<string>("fullname"));
				}
				else if (channelReadAccess)
				{
					contactFormattedString = string.Format(@"<a href=""{0}"">{1}</a>",
						ReadOnlyContactUrl(contact.GetAttributeValue<Guid>("contactid")),
						contact.GetAttributeValue<string>("fullname"));
				}
			}
			else
			{
				contactFormattedString = contact.GetAttributeValue<string>("fullname");
			}

			div = new HtmlGenericControl("div")
			{

				InnerHtml = string.Format(@"<span class=""contact-name"">{0}</span>{1}",
					contactFormattedString,
					!string.IsNullOrEmpty(companyName)
					? string.Format(@"<span class=""contact-company-name"">{0}</span>", Server.HtmlEncode(companyName)) : string.Empty)
			};

			// add div at the beginning for reverse chronological order
			OpportunityContactsPlaceHolder.Controls.AddAt(0, div);
		}

		private void GetLeadHistory()
		{
			foreach (var history in XrmContext.GetOpportunityHistories(OpenOpportunity))
			{
				AddHistoryDiv(history);
			}
		}

		private void GetContactList()
		{
			var contacts = OpenOpportunity.GetRelatedEntities(XrmContext, new Relationship("adx_opportunity_contact"));

			foreach (var contact in contacts)
			{
				AddContactDiv(contact);
			}
		}

		private void BindProductsLeadNotesContactsAndAssignedTo()
		{
			var opportunityContact = OpenOpportunity.GetRelatedEntity(XrmContext, new Relationship("msa_contact_opportunity"));
			CurrentlyAssignedToLabel.Text = (opportunityContact != null) ? opportunityContact.GetAttributeValue<string>("fullname") : string.Empty;

			if (IsPostBack)
			{
				return;
			}

			Products.Text = string.Join(", ", OpenOpportunity.GetRelatedEntities(XrmContext, new Relationship("adx_opportunity_product")).Select(product => product.GetAttributeValue<string>("name")));

			OpportunityNotes.Text = GetFormattedDescription(OpenOpportunity.GetAttributeValue<string>("description"));

			//LeadAssignedTo.Text = OpenOpportunity.adx_PartnerAssignedTo;

			var empli = new ListItem();

			AssignToList.Items.Add(empli);

			//var contacts = XrmContext.GetContactsForContact(Contact).Cast<Contact>();

			AssertContactHasParentAccount();

			var contacts = XrmContext.CreateQuery("contact").Where(c => c.GetAttributeValue<EntityReference>("parentcustomerid") == Contact.GetAttributeValue<EntityReference>("parentcustomerid"));

			foreach (var contact in contacts)
			{
				if (contact.GetAttributeValue<OptionSetValue>("statecode") != null && contact.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
				{
					var li = new ListItem()
					{
						Text = contact.GetAttributeValue<string>("fullname"),
						Value = contact.GetAttributeValue<Guid>("contactid").ToString()
					};

					if (OpenOpportunity.GetAttributeValue<EntityReference>("msa_partneroppid") != null && li.Value == OpenOpportunity.GetAttributeValue<EntityReference>("msa_partneroppid").Id.ToString())
					{
						li.Selected = true;
					}

					AssignToList.Items.Add(li);
				}
			}

			var partnerAccount = ServiceContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == (Contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? Guid.Empty : Contact.GetAttributeValue<EntityReference>("parentcustomerid").Id));

			if (partnerAccount != null)
			{
				
				var fetchXmlString = string.Format(@"
					<fetch mapping=""logical"" distinct=""true"">
						<entity name=""contact"">
							<attribute name=""fullname"" />
							<attribute name=""telephone1"" />
							<attribute name=""contactid"" />
							<order attribute=""fullname"" descending=""false"" />
							<link-entity name=""account"" from=""accountid"" to=""parentcustomerid"" alias=""accountmanagingpartnerlink"" link-type=""outer"">
							</link-entity>
							<link-entity name=""adx_opportunity_contact"" from=""contactid"" to=""contactid"" link-type=""outer"">
								<link-entity name=""opportunity"" from=""opportunityid"" to=""opportunityid"" alias=""opportunitylink"" link-type=""outer""></link-entity>
							</link-entity>
							<filter type=""and"">
								<filter type=""or"">
									<condition attribute=""msa_managingpartnerid"" operator=""eq"" value=""{0}"" />
									<condition entityname=""accountmanagingpartnerlink"" attribute=""msa_managingpartnerid"" operator=""eq"" value=""{0}"" />
								</filter>
								<condition entityname=""opportunitylink"" attribute=""opportunityid"" operator=""ne"" value=""{1}"" />
							</filter>
						</entity>
					</fetch>", partnerAccount.Id, OpenOpportunity.Id);

				var fetchXml = XDocument.Parse(fetchXmlString);

				var response = (RetrieveMultipleResponse)ServiceContext.Execute(new RetrieveMultipleRequest
				{
					Query = new FetchExpression(fetchXml.ToString())
				});

				var customerContacts = response.EntityCollection.Entities.ToList();

				foreach (var li in customerContacts.Select(customerContact => new ListItem()
					{
						Text = customerContact.GetAttributeValue<string>("fullname"),
						Value = customerContact.GetAttributeValue<Guid>("contactid").ToString()
					}))
				{ AddContactList.Items.Add(li); }

			}

			if (AddContactList.Items.Count >= 1) return;

			AddContactList.Visible = false;
			AddContactCheckBox.Visible = false;
		}

		private void BindPipelinePhaseDetails()
		{

			PipelinePhase.Items.Clear();

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

			var phase = 0;

			foreach (var option in picklist.OptionSet.Options)
			{
				var text = option.Label.GetLocalizedLabelString();
				var value = option.Value.Value.ToString();

				if (text == OpenOpportunity.GetAttributeValue<string>("stepname"))
				{
					phase = option.Value.Value;
				}
			}

			foreach (var option in picklist.OptionSet.Options)
			{
				var li = new ListItem()
				{
					Text = option.Label.GetLocalizedLabelString(),
					Value = option.Value.Value.ToString()
				};

				if (option.Value.Value >= phase)
				{
					bool GoodToGo = true;

					foreach (ListItem item in PipelinePhase.Items)
					{
						if (item.Text == li.Text)
						{
							GoodToGo = false;
						}
					}

					if (GoodToGo)
					{
						PipelinePhase.Items.Add(li);
					}
				}

				if (li.Text == OpenOpportunity.GetAttributeValue<string>("stepname"))
				{
					li.Selected = true;
				}

			}
			DisableControlsBasedOnPipelinePhaseAndAccessPermissions();
		}

        private CrmDataSource CreateDataSource(string dataSourceId, string entityName, string entityIdAttribute, Guid? entityId)
        {
            var formViewDataSource = new CrmDataSource
            {
                ID = dataSourceId,
                FetchXml = string.Format(@"<fetch mapping='logical'> <entity name='{0}'> <all-attributes /> <filter type='and'> <condition attribute = '{1}' operator='eq' value='{{{2}}}'/> </filter> </entity> </fetch>",
                    entityName,
                    entityIdAttribute,
                    entityId)
            };

            CrmEntityFormViewsPanel.Controls.Add(formViewDataSource);

            return formViewDataSource;
        }

        private void AddFetchXmlToDataSource(CrmDataSource dataSource, string entityName, string entityIdAttribute, Guid? entityId)
        {
            dataSource.FetchXml =
                string.Format(
                    @"<fetch mapping='logical'> <entity name='{0}'> <all-attributes /> <filter type='and'> <condition attribute = '{1}' operator='eq' value='{{{2}}}'/> </filter> </entity> </fetch>",
                    entityName,
                    entityIdAttribute,
                    entityId);
        }

        private void DisableControlsBasedOnPipelinePhaseAndAccessPermissions()
		{

			var accessPermissions = XrmContext.GetOpportunityAccessByContact(Contact);

			//CreateCaseLink.Visible = false;
			AssignToList.Visible = false;
			//AssignToContact.Visible = false;
			OpportunityStatusPanel.Visible = false;
			SubmitButton.Visible = false;
			CancelOpportunity.Visible = false;

			foreach (var access in accessPermissions)
			{
				if (access.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false))
				{
					//CreateCaseLink.Visible = true;
					SubmitButton.Visible = true;
					OpportunityStatusPanel.Visible = true;
				}

				if (access.GetAttributeValue<bool?>("adx_assign").GetValueOrDefault(false))
				{
					AssignToList.Visible = true;
					//AssignToContact.Visible = true;
				}

				if (access.GetAttributeValue<bool?>("adx_delete").GetValueOrDefault(false))
				{
					CancelOpportunity.Visible = true;
				}

			}

			CurrentlyAssignedToLabel.Visible = !AssignToList.Visible;

			if (OpenOpportunity.GetAttributeValue<OptionSetValue>("statecode") != null && (OpenOpportunity.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Lost || OpenOpportunity.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Adxstudio.Xrm.Partner.Enums.OpportunityState.Won))
			{
				CrmEntityFormViewsPanel.Enabled = false;
				OpportunityStatusPanel.Enabled = false;
			}

		}

		private static string GetFormattedDescription(string description)
		{
			if (string.IsNullOrWhiteSpace(description))
			{
				return string.Empty;
			}

			var numbering = new Regex(@"(?= \d?\d\) )");

			return numbering.Replace(description, "\n\n");
		}

		private Entity GetPrimaryContactAndSetCompanyName()
		{
			if (OpenOpportunity == null)
			{
				return null;
			}

			Entity primaryContact = null;

			var customer = OpenOpportunity.GetAttributeValue<EntityReference>("customerid");

			if (customer.LogicalName == "account")
			{
				var account = XrmContext.CreateQuery("account").First(a => a.GetAttributeValue<Guid>("accountid") == customer.Id);

				CompanyName.Text = account.GetAttributeValue<string>("name");

				primaryContact = account.GetRelatedEntity(XrmContext, new Relationship("account_primary_contact"));

				var channelPermission = XrmContext.GetChannelAccessByContact(Contact);

				var channelWriteAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_write").GetValueOrDefault(false));

				var channelReadAccess = (channelPermission != null && channelPermission.GetAttributeValue<bool?>("adx_read").GetValueOrDefault(false));

				var parentAccount = (account.GetAttributeValue<EntityReference>("msa_managingpartnerid") != null) ? XrmContext.CreateQuery("account").FirstOrDefault(a => a.GetAttributeValue<Guid>("accountid") == account.GetAttributeValue<EntityReference>("msa_managingpartnerid").Id) : null;

				if (parentAccount != null && ((channelPermission != null && channelPermission.GetAttributeValue<EntityReference>("adx_accountid") != null) && channelPermission.GetAttributeValue<EntityReference>("adx_accountid").Equals(parentAccount.ToEntityReference())) && (parentAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode") != null && parentAccount.GetAttributeValue<OptionSetValue>("accountclassificationcode").Value == 100000000))
				{
					if (channelWriteAccess)
					{
						CompanyName.Text = string.Format(@"<a href=""{0}"" class=""Edit"">{1}</a>",
						HttpUtility.HtmlEncode(EditAccountUrl(account.GetAttributeValue<Guid>("accountid"))),
						HttpUtility.HtmlEncode(CompanyName.Text));
					}
					else if (channelReadAccess)
					{
						CompanyName.Text = string.Format(@"<a href=""{0}"" class=""Edit"">{1}</a>",
						HttpUtility.HtmlEncode(ReadOnlyAccountUrl(account.GetAttributeValue<Guid>("accountid"))),
						HttpUtility.HtmlEncode(CompanyName.Text));
					}
				}


				//CompanyName.Attributes.Add("style", "white-space: nowrap;");

			}
			else if (customer.LogicalName == "contact")
			{
				primaryContact = XrmContext.CreateQuery("contact").First(c => c.GetAttributeValue<Guid>("contactid") == customer.Id);

				var account = primaryContact.GetRelatedEntity(XrmContext, new Relationship("account_primary_contact"));

				CompanyName.Text = account != null ? account.GetAttributeValue<string>("name") : primaryContact.GetAttributeValue<string>("adx_organizationname");
			}

			return primaryContact;
		}

		protected string EditAccountUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Edit Customer Account");

			if (page == null) { return " "; }

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("AccountID", id.ToString());

			return url.PathWithQueryString;
		}

		protected string ReadOnlyAccountUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Read Only Account View");

			if (page == null) { return " "; }

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("AccountID", id.ToString());

			return url.PathWithQueryString;
		}

		protected QueryStringCollection CreateCustomerContactQueryString()
		{
			var queryStringCollection = new QueryStringCollection("");

			var oppId = OpenOpportunity.GetAttributeValue<Guid>("opportunityid");

			var account = OpenOpportunity.GetAttributeValue<EntityReference>("customerid");

			queryStringCollection.Set("OpportunityId", oppId.ToString());

			if (account != null)
			{
				queryStringCollection.Set("AccountId", account.Id.ToString());
			}

			return queryStringCollection;
		}

		protected string EditContactUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Edit Customer Contact");

			if (page == null) { return " "; }

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("ContactID", id.ToString());

			return url.PathWithQueryString;
		}

		protected string ReadOnlyContactUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Read Only Contact View");

			if (page == null) { return " "; }

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			url.QueryString.Set("ContactID", id.ToString());

			return url.PathWithQueryString;
		}
	}
}
