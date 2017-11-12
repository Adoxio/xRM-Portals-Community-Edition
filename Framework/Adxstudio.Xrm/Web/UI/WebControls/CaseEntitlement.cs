/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.HelpDesk;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// CaseEntitlement Control
	/// </summary>
	[ToolboxData(@"<{0}:CaseEntitlement runat=""server""></{0}:CaseEntitlement>")]
	[DefaultProperty("")]
	public class CaseEntitlement : CompositeControl
	{
		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		[Description("The portal configuration that the control binds to.")]
		[DefaultValue("")]
		public string PortalName
		{
			get { return ((string)ViewState["PortalName"]) ?? string.Empty; }
			set { ViewState["PortalName"] = value; }
		}

		/// <summary>
		/// Gets or sets Validation Group to be assigned to controls.
		/// </summary>
		[Description("The Validation Group to be assigned to controls")]
		[DefaultValue("")]
		public string ValidationGroup
		{
			get { return ((string)ViewState["ValidationGroup"]) ?? string.Empty; }
			set { ViewState["ValidationGroup"] = value; }
		}

		/// <summary>
		/// Gets or sets number of prepaid incidents required.
		/// </summary>
		[Description("Gets or sets number of prepaid incidents required.")]
		[DefaultValue(1)]
		public int PrepaidIncidentsRequired
		{
			get { return (int)(ViewState["PrepaidIncidentsRequired"] ?? 1); }
			set { ViewState["PrepaidIncidentsRequired"] = value; }
		}

		/// <summary>
		/// Gets or sets number of prepaid incidents available.
		/// </summary>
		[Description("Gets or sets number of prepaid incidents available.")]
		[DefaultValue(0)]
		public int PrepaidIncidentsAvailable
		{
			get { return (int)(ViewState["PrepaidIncidentsAvailable"] ?? 0); }
			private set { ViewState["PrepaidIncidentsAvailable"] = value; }
		}

		/// <summary>
		/// Determines if product is required.
		/// </summary>
		[Description("Determines if product is required.")]
		[DefaultValue(false)]
		public bool ProductIsRequired
		{
			get { return (bool)(ViewState["ProductIsRequired"] ?? false); }
			set { ViewState["ProductIsRequired"] = value; }
		}

		/// <summary>
		/// ID of the support request
		/// </summary>
		public Guid SupportRequestID
		{
			get
			{
				return (Guid)(ViewState["SupportRequestID"] ?? Guid.Empty);
			}
			set { ViewState["SupportRequestID"] = value; }
		}

		/// <summary>
		/// ID of the Product to associate the support request with and to retrieve the support plan for
		/// </summary>
		public Guid ProductID
		{
			get
			{
				return (Guid)(ViewState["ProductID"] ?? Guid.Empty);
			}
			set { ViewState["ProductID"] = value; }
		}

		/// <summary>
		/// ID of the current user's contact
		/// </summary>
		public Guid ContactID
		{
			get
			{
				return (Guid)(ViewState["ContactID"] ?? Guid.Empty);
			}
			set { ViewState["ContactID"] = value; }
		}

		/// <summary>
		/// Customer to associate the support request with
		/// </summary>
		public EntityReference Customer
		{
			get
			{
				return ViewState["Customer"] as EntityReference;
			}
			set { ViewState["Customer"] = value; }
		}

		/// <summary>
		/// ID of the Support Plan to associate the support request with
		/// </summary>
		public Guid SupportPlanID
		{
			get
			{
				return (Guid)(ViewState["SupportPlanID"] ?? Guid.Empty);
			}
			set { ViewState["SupportPlanID"] = value; }
		}

		protected bool EnableReselectCustomerButton
		{
			get { return (bool)(ViewState["EnableReselectCustomerButton"] ?? false); }
			set { ViewState["EnableReselectCustomerButton"] = value; }
		}

		protected readonly string DefaultCustomerDropdownSelfText = ResourceManager.GetString("Myself_Text");
		protected const string SupportRequestIdQueryStringParameterName = "id";
		protected readonly string DefaultSupportPlanName = ResourceManager.GetString("Support_Plan_Name_Text");
		protected readonly string DefaultReselectCustomerButtonText = ResourceManager.GetString("Select_Different_Customer_Button_Text");
		protected readonly string DefaultReselectCustomerRequiredErrorText = ResourceManager.GetString("Select_Customer_Error_Message");
		protected readonly string DefaultReselectPlanButtonText = ResourceManager.GetString("Select_Different_Support_Plan_Butoon_Text");
		protected readonly string DefaultReselectPlanRequiredErrorText = ResourceManager.GetString("Select_Support_Plan_Error_Message");
		protected readonly string HelpDeskEntitlementValidSnippetName = string.Format(ResourceManager.GetString("Help_Desk_Entitlement_Message"), "Valid");
		protected readonly string HelpDeskEntitlementSelectPlanSnippetName = string.Format(ResourceManager.GetString("Help_Desk_Entitlement_Message"), "Select Plan");
		protected readonly string HelpDeskEntitlementNoPlanSnippetName = string.Format(ResourceManager.GetString("Help_Desk_Entitlement_Message"), "No Plan");
		protected readonly string HelpDeskEntitlementSelectCustomerSnippetName = string.Format(ResourceManager.GetString("Help_Desk_Entitlement_Message"), "Select Customer");
		protected readonly string HelpDeskEntitlementErrorSnippetName = string.Format(ResourceManager.GetString("Help_Desk_Entitlement_Message"), "Error");
		protected static readonly string HeaderSnippetHTML = string.Format("<h3>{0}</h3>", ResourceManager.GetString("Support_Plan_Name_Text"));
		protected readonly string DefaultSnippetHelpDeskEntitlementValid = string.Format(@"
			{0}
			<p>{1}</p>
			<p>{2}</p>
			<p>{3}</p>",
			HeaderSnippetHTML,
			string.Format(ResourceManager.GetString("Plan_Name_Text"), "<strong>{0}</strong>"),
			string.Format(ResourceManager.GetString("Prepaid_Incidents_Available_With_Parameter_Text"), "<strong>{1}</strong>"),
			string.Format(ResourceManager.GetString("Upon_Completion_Of_Opening_Support_Request"), string.Format("<strong> {0} </strong>", ResourceManager.GetString("Prepaid_Incidents_Available_Text")), "<strong>{2}</strong>"));
		protected readonly string DefaultSnippetHelpDeskEntitlementSelectPlan = string.Format(@"
			{0}
			<p>{1}</p>",
			HeaderSnippetHTML,
			ResourceManager.GetString("Please_Select_Support_Plans_Text"));
		protected readonly string DefaultSnippetHelpDeskEntitlementNoPlan = string.Format(@"
			{0}
			<p class='text-error'>{1}</p>
			<p>{2}</p>
			<p>{3}</p>",
			HeaderSnippetHTML,
			ResourceManager.GetString("Your_Account_Does_Not_Have_Support_Plan_Text"),
			ResourceManager.GetString("To_Open_New_Support_Request_You_Must_Purchase_New_Support_Plan_Text"),
			ResourceManager.GetString("Click_Next_To_Purchase_Support_Text"));
		protected readonly string DefaultSnippetHelpDeskEntitlementSelectCustomer = string.Format(@"
			{0}
			<p>{1}</p>",
			HeaderSnippetHTML,
			ResourceManager.GetString("Select_Customer_To_Assign_Support_Request_Text"));
		protected readonly string DefaultSnippetHelpDeskEntitlementError = string.Format(@"
			{0}
			<p class='text-error'>{1}</p>
			<p>{2}</p>
			<p>{3}</p>",
			HeaderSnippetHTML,
			ResourceManager.GetString("Problem_With_Your_Account_Text"),
			ResourceManager.GetString("Please_Call_Text"),
			ResourceManager.GetString("Working_Hours_Text"));
		protected readonly string DefaultSnippetHelpDeskEntitlementPermissionDenied = string.Format(@"
			<div class='alert alert-block alert-error alert-danger'>{0}</div>
			<p>{1}</p>
			<p>{2}</p>
			<p>{3}</p>",
			ResourceManager.GetString("You_Do_Dot_Have_Permission_To_Open_Support_Request_Text"),
			ResourceManager.GetString("Please_Contact_Support_Text"),
			ResourceManager.GetString("Phone_Number"),
			ResourceManager.GetString("Working_Hours_Text"));

		protected int NumberOfAccounts
		{
			get { return (int)(ViewState["NumberOfAccounts"] ?? 0); }
			set { ViewState["NumberOfAccounts"] = value; }
		}

		protected int NumberOfPlans
		{
			get { return (int)(ViewState["NumberOfPlans"] ?? 0); }
			set { ViewState["NumberOfPlans"] = value; }
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();

			if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request[SupportRequestIdQueryStringParameterName]))
			{
				Guid supportRequestId;

				if (Guid.TryParse(HttpContext.Current.Request[SupportRequestIdQueryStringParameterName], out supportRequestId))
				{
					SupportRequestID = supportRequestId;
				}
			}

			if (SupportRequestID == Guid.Empty)
			{
				RenderSupportRequestIdError();

				return;
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var supportRequest = context.CreateQuery("adx_supportrequest").FirstOrDefault(o => o.GetAttributeValue<Guid>("adx_supportrequestid") == SupportRequestID);

			if (supportRequest == null)
			{
				RenderSupportRequestIdError();

				return;
			}

			var product = supportRequest.GetAttributeValue<EntityReference>("adx_product");

			ProductID = product == null ? Guid.Empty : product.Id;

			if (ProductIsRequired && ProductID == Guid.Empty)
			{
				RenderProductIdError();

				return;
			}

			if (Customer == null)
			{
				ProcessCustomer();
			}
			else if (Customer.Id != Guid.Empty)
			{
				if (SupportPlanID == Guid.Empty)
				{
					switch (Customer.LogicalName)
					{
						case "account":
							ProcessSupportPlansForAccount();
							break;
						case "contact":
							ProcessSupportPlansForContact();
							break;
						default:
							RenderGeneralError();
							break;
					}
				}
				else
				{
					ProcessSingleSupportPlan(SupportPlanID);
				}
			}
		}

		protected virtual void ProcessCustomer()
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var dataAdapter = new UserCasesDataAdapter(new PortalContextDataAdapterDependencies(portalContext, PortalName, HttpContext.Current.Request.RequestContext));
			var permissionScopes = dataAdapter.SelectPermissionScopes();
			var canCreate = permissionScopes.Self.Create || permissionScopes.Accounts.Any(permissions => permissions.Create);
			if (!canCreate)
			{
				RenderPermissionDenied();
				return;
			}
			var accounts = permissionScopes.Accounts.Where(permissions => permissions.Create).ToList();
			NumberOfAccounts = accounts.Count();
			if (NumberOfAccounts > 1 || (permissionScopes.Self.Create && NumberOfAccounts == 1))
			{
				EnableReselectCustomerButton = true;
				var snippetHelpDeskEntitlementSelectCustomer = new Literal { ID = "SelectCustomer", Text = DefaultSnippetHelpDeskEntitlementSelectCustomer };
				var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Select Customer");
				if (snippet != null)
				{
					snippetHelpDeskEntitlementSelectCustomer.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementSelectCustomer;
				}
				var panelHelpDeskEntitlementSelectCustomer = new Panel { ID = "EntitlementSelectCustomerPanel" };
				panelHelpDeskEntitlementSelectCustomer.Controls.Add(snippetHelpDeskEntitlementSelectCustomer);
				var customerDropdown = new DropDownList
				{
					ID = "CustomerDropDown",
					CssClass = "form-control",
					AutoPostBack = true,
					ValidationGroup = ValidationGroup
				};
				customerDropdown.Items.Add(new ListItem(string.Empty));
				if (permissionScopes.Self.Create && portalContext.User != null)
				{
					ContactID = portalContext.User.Id;
					customerDropdown.Items.Add(new ListItem(portalContext.User.Attributes.ContainsKey("fullname") ? portalContext.User.Attributes["fullname"].ToString() : DefaultCustomerDropdownSelfText, portalContext.User.Id.ToString()));
				}
				foreach (var accountPermissions in permissionScopes.Accounts.Where(permissions => permissions.Create).OrderBy(o => o.Account.Name))
				{
					customerDropdown.Items.Add(new ListItem(accountPermissions.Account.Name, accountPermissions.Account.Id.ToString()));
				}
				customerDropdown.SelectedIndexChanged += CustomerDropDownList_SelectedIndexChanged;
				var requiredValidator = new RequiredFieldValidator
				{
					ID = "CustomerDropDownRequiredFieldValidator",
					ControlToValidate = customerDropdown.ID,
					Display = ValidatorDisplay.Static,
					ErrorMessage = ResourceManager.GetString("Select_Customer_Error_Message"),
					ValidationGroup = ValidationGroup,
					InitialValue = string.Empty,
					CssClass = "help-block error",
				};
				panelHelpDeskEntitlementSelectCustomer.Controls.Add(customerDropdown);
				panelHelpDeskEntitlementSelectCustomer.Controls.Add(requiredValidator);
				Controls.Add(panelHelpDeskEntitlementSelectCustomer);
			}
			else
			{
				if (NumberOfAccounts == 1)
				{
					var account = accounts.Single();
					Customer = new EntityReference("account", account.Account.Id);
					ProcessSupportPlansForAccount();
				}
				else if (permissionScopes.Self.Create && portalContext.User != null)
				{
					ContactID = portalContext.User.Id;
					Customer = new EntityReference("contact", portalContext.User.Id);
					ProcessSupportPlansForContact();
				}
				else
				{
					RenderGeneralError();
				}
			}
		}

		protected virtual void ProcessSupportPlansForAccount()
		{
			if (ProductIsRequired)
			{
				ProcessSupportPlansForAccount(Customer.Id, ProductID);
			}
			else
			{
				ProcessSupportPlansForAccount(Customer.Id);
			}
		}

		protected virtual void ProcessSupportPlansForAccount(Guid accountid)
		{
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var plans = context.GetActiveSupportPlansForAccount(accountid).ToList();
			ProcessSupportPlans(plans);
		}

		protected virtual void ProcessSupportPlansForAccount(Guid accountid, Guid productid)
		{
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var plans = context.GetActiveSupportPlansForAccount(accountid, productid).ToList();
			ProcessSupportPlans(plans);
		}

		protected virtual void ProcessSupportPlansForContact()
		{
			if (ProductIsRequired)
			{
				ProcessSupportPlansForContact(Customer.Id, ProductID);
			}
			else
			{
				ProcessSupportPlansForContact(Customer.Id);
			}
		}

		protected virtual void ProcessSupportPlansForContact(Guid contactid)
		{
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var plans = context.GetActiveSupportPlansForContact(contactid).ToList();
			ProcessSupportPlans(plans);
		}

		protected virtual void ProcessSupportPlansForContact(Guid contactid, Guid productid)
		{
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var plans = context.GetActiveSupportPlansForContact(contactid, productid).ToList();
			ProcessSupportPlans(plans);
		}

		protected virtual void ProcessSupportPlans(List<Entity> plans)
		{
			Controls.Clear();
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			NumberOfPlans = plans.Count();
			if (NumberOfPlans > 1)
			{
				SupportPlanID = Guid.Empty;
				// Multiple plans found so prompt for single plan selection
				var snippetHelpDeskEntitlementSelectPlan = new Literal { ID = "SelectPlan", Text = DefaultSnippetHelpDeskEntitlementSelectPlan };
				var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Select Plan");
				if (snippet != null)
				{
					snippetHelpDeskEntitlementSelectPlan.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementSelectPlan;
				}
				var panelHelpDeskEntitlementSelectPlan = new Panel { ID = "EntitlementSelectPlanPanel" };
				panelHelpDeskEntitlementSelectPlan.Controls.Add(snippetHelpDeskEntitlementSelectPlan);
				var p = new HtmlGenericControl("p");
				var supportPlans = plans.OrderBy(o => o.GetAttributeValue<string>("adx_name")).Select(o => new { DataValueField = o.GetAttributeValue<Guid>("adx_supportplanid").ToString(), DataTextField = string.Format("{0}: Prepaid Incidents Remaining {1}", o.GetAttributeValue<string>("adx_name"), o.GetAttributeValue<int>("adx_allotmentsremaining")) });
				var planList = new RadioButtonList
				{
					ID = "PlanDropDown",
					DataSource = supportPlans,
					DataTextField = "DataTextField",
					DataValueField = "DataValueField",
					AppendDataBoundItems = true,
					AutoPostBack = true,
					ValidationGroup = ValidationGroup,
					CssClass = "cell checkbox-cell",
					RepeatLayout = RepeatLayout.Flow
				};
				planList.DataBind();
				planList.SelectedIndexChanged += PlanRadioButtonList_SelectedIndexChanged;
				p.Controls.Add(planList);
				var requiredValidator = new RequiredFieldValidator
				{
					ID = "PlanDropDownRequiredFieldValidator",
					ControlToValidate = planList.ID,
					Display = ValidatorDisplay.Static,
					ErrorMessage = ResourceManager.GetString("Select_Support_Plan_Error_Message"),
					ValidationGroup = ValidationGroup,
					InitialValue = string.Empty,
					CssClass = "help-block error",
				};
				p.Controls.Add(requiredValidator);
				panelHelpDeskEntitlementSelectPlan.Controls.Add(p);
				AddContextualButtons(panelHelpDeskEntitlementSelectPlan);
				Controls.Add(panelHelpDeskEntitlementSelectPlan);
			}
			else if (NumberOfPlans == 1)
			{
				var plan = plans.Single();
				ProcessSingleSupportPlan(plan);
			}
			else
			{
				// No plan found so display no plan message
				SupportPlanID = Guid.Empty;
				var snippetHelpDeskEntitlementNoPlan = new Literal { ID = "NoPlan", Text = DefaultSnippetHelpDeskEntitlementNoPlan };
				var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement No Plan");
				if (snippet != null)
				{
					snippetHelpDeskEntitlementNoPlan.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementNoPlan;
				}
				var panelHelpDeskEntitlementNoPlan = new Panel { ID = "EntitlementNoPanel" };
				panelHelpDeskEntitlementNoPlan.Controls.Add(snippetHelpDeskEntitlementNoPlan);
				AddContextualButtons(panelHelpDeskEntitlementNoPlan);
				Controls.Add(panelHelpDeskEntitlementNoPlan);
			}
		}

		protected virtual void ProcessSingleSupportPlan(Guid planid)
		{
			if (planid == Guid.Empty)
			{
				return;
			}
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var plan = context.CreateQuery("adx_supportplan").FirstOrDefault(o => o.GetAttributeValue<Guid>("adx_supportplanid") == planid);
			if (plan == null)
			{
				return;
			}
			ProcessSingleSupportPlan(plan);
		}

		protected virtual void ProcessSingleSupportPlan(Entity plan)
		{
			Controls.Clear();
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			SupportPlanID = plan.Id;
			PrepaidIncidentsAvailable = plan.GetAttributeValue<int?>("adx_allotmentsremaining") ?? 0;
			var supportPlanName = plan.GetAttributeValue<string>("adx_name") ?? DefaultSupportPlanName;
			var snippetHelpDeskEntitlementValid = new Literal { ID = "Valid", Text = string.Format(DefaultSnippetHelpDeskEntitlementValid, supportPlanName, PrepaidIncidentsAvailable, PrepaidIncidentsRequired) };
			var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Valid");
			if (snippet != null)
			{
				snippetHelpDeskEntitlementValid.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value"))
														? string.Format(snippet.GetAttributeValue<string>("adx_value"), supportPlanName,
																		PrepaidIncidentsAvailable, PrepaidIncidentsRequired)
														: string.Format(DefaultSnippetHelpDeskEntitlementValid, supportPlanName,
																		PrepaidIncidentsAvailable, PrepaidIncidentsRequired);
			}
			var panelHelpDeskEntitlementValid = new Panel { ID = "EntitlementValidPanel" };
			panelHelpDeskEntitlementValid.Controls.Add(snippetHelpDeskEntitlementValid);
			AddContextualButtons(panelHelpDeskEntitlementValid);
			Controls.Add(panelHelpDeskEntitlementValid);
		}

		protected void RenderSupportRequestIdError()
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var snippetHelpDeskEntitlementSupportRequestIdError = new Literal { ID = "SupportRequestIdError", Text = DefaultSnippetHelpDeskEntitlementError };
			var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Support Request ID Error");
			if (snippet != null)
			{
				snippetHelpDeskEntitlementSupportRequestIdError.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementError;
			}
			var panelHelpDeskEntitlementSupportRequestIdError = new Panel { ID = "EntitlementSupportRequestIdErrorPanel" };
			panelHelpDeskEntitlementSupportRequestIdError.Controls.Add(snippetHelpDeskEntitlementSupportRequestIdError);
			Controls.Add(panelHelpDeskEntitlementSupportRequestIdError);
			EnableDisableWebFormNextButton(false);
		}

		protected void RenderProductIdError()
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var snippetHelpDeskEntitlementProductIdError = new Literal { ID = "ProductIdError", Text = DefaultSnippetHelpDeskEntitlementError };
			var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Product ID Error");
			if (snippet != null)
			{
				snippetHelpDeskEntitlementProductIdError.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementError;
			}
			var panelHelpDeskEntitlementProductIdError = new Panel { ID = "EntitlementProductIdErrorPanel" };
			panelHelpDeskEntitlementProductIdError.Controls.Add(snippetHelpDeskEntitlementProductIdError);
			Controls.Add(panelHelpDeskEntitlementProductIdError);
			EnableDisableWebFormNextButton(false);
		}

		protected void RenderPermissionDenied()
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var snippetHelpDeskEntitlementPermissionDenied = new Literal { ID = "EntitlementPermissionDenied", Text = DefaultSnippetHelpDeskEntitlementPermissionDenied };
			var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Permission Denied");
			if (snippet != null)
			{
				snippetHelpDeskEntitlementPermissionDenied.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementPermissionDenied;
			}
			var panelHelpDeskEntitlementPermissionDenied = new Panel { ID = "EntitlementPermissionDeniedPanel" };
			panelHelpDeskEntitlementPermissionDenied.Controls.Add(snippetHelpDeskEntitlementPermissionDenied);
			Controls.Add(panelHelpDeskEntitlementPermissionDenied);
			EnableDisableWebFormNextButton(false);
		}

		protected void RenderGeneralError()
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var snippetHelpDeskEntitlementError = new Literal { ID = "Error", Text = DefaultSnippetHelpDeskEntitlementError };
			var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Error");
			if (snippet != null)
			{
				snippetHelpDeskEntitlementError.Text = !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")) ? snippet.GetAttributeValue<string>("adx_value") : DefaultSnippetHelpDeskEntitlementError;
			}
			var panelHelpDeskEntitlementError = new Panel { ID = "EntitlementErrorPanel" };
			panelHelpDeskEntitlementError.Controls.Add(snippetHelpDeskEntitlementError);
			Controls.Add(panelHelpDeskEntitlementError);
			EnableDisableWebFormNextButton(false);
		}

		protected void PlanRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
		{
			var control = (RadioButtonList)sender;
			var stringID = control.SelectedValue;
			Guid id;
			if (Guid.TryParse(stringID, out id))
			{
				SupportPlanID = id;
			}
			ProcessSingleSupportPlan(id);
		}

		protected void CustomerDropDownList_SelectedIndexChanged(object sender, EventArgs e)
		{
			var control = (DropDownList)sender;
			var stringID = control.SelectedValue;
			Guid customerid;
			if (!Guid.TryParse(stringID, out customerid))
			{
				Customer = null;
				return;
			}
			if (customerid == ContactID)
			{
				Customer = new EntityReference("contact", customerid);
				ProcessSupportPlansForContact();
			}
			else
			{
				Customer = new EntityReference("account", customerid);
				ProcessSupportPlansForAccount();
			}
		}

		protected void ReselectCustomerButton_Click(object sender, EventArgs e)
		{
			Customer = null;
			CreateChildControls();
		}

		protected void ReselectPlanButton_Click(object sender, EventArgs e)
		{
			SupportPlanID = Guid.Empty;
			CreateChildControls();
		}

		protected void AddContextualButtons(Control container)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var p = new HtmlGenericControl("p");
			if (EnableReselectCustomerButton)
			{
				var text = DefaultReselectCustomerButtonText;
				var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Reselect Customer Button Text");
				if (snippet != null && !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")))
				{
					text = snippet.GetAttributeValue<string>("adx_value");
				}
				var reselectCustomerButton = new Button { Text = text, CausesValidation = false, CssClass = "btn btn-default" };
				reselectCustomerButton.Click += ReselectCustomerButton_Click;
				p.Controls.Add(reselectCustomerButton);
			}
			if (NumberOfPlans > 1 && SupportPlanID != Guid.Empty)
			{
				var text = DefaultReselectPlanButtonText;
				var snippet = context.CreateQuery("adx_contentsnippet").FirstOrDefault(c => c.GetAttributeValue<string>("adx_name") == "Help Desk Entitlement Reselect Plan Button Text");
				if (snippet != null && !string.IsNullOrWhiteSpace(snippet.GetAttributeValue<string>("adx_value")))
				{
					text = snippet.GetAttributeValue<string>("adx_value");
				}
				var reselectPlanButton = new Button { Text = text, CausesValidation = false, CssClass = "btn btn-default" };
				reselectPlanButton.Click += ReselectPlanButton_Click;
				p.Controls.Add(reselectPlanButton);
			}

			if (EnableReselectCustomerButton || (NumberOfPlans > 1 && SupportPlanID != Guid.Empty))
			{
				container.Controls.Add(p);
			}
		}

		protected void EnableDisableWebFormNextButton(bool enable)
		{
			var parent = Parent as WebFormUserControl;
			if (parent != null && parent.WebForm != null)
			{
				parent.WebForm.EnableDisableNextButton(enable);
			}
		}

		/// <summary>
		/// Submit event handler for the WebFormUserControl that renders this control.
		/// </summary>
		/// <param name="control">Calling user control</param>
		/// <param name="e">Event arguments</param>
		/// <returns></returns>
		public virtual WebFormSubmitEventArgs Submit(WebFormUserControl control, WebFormSubmitEventArgs e)
		{
			if (control == null || e == null)
			{
				return e;
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			if (control.CurrentStepEntityID != Guid.Empty)
			{
				// Update existing entity
				var sourceEntity = new Entity(control.CurrentStepEntityLogicalName) { Id = control.CurrentStepEntityID };
				sourceEntity["adx_prepaidincidentscurrentlyavailable"] = PrepaidIncidentsAvailable < 0 ? 0 : PrepaidIncidentsAvailable;
				sourceEntity["adx_prepaidincidentsrequired"] = PrepaidIncidentsRequired < 0 ? 0 : PrepaidIncidentsRequired;
				if (SupportPlanID != Guid.Empty)
				{
					sourceEntity["adx_supportplan"] = new EntityReference("adx_supportplan", SupportPlanID);
				}
				else
				{
					sourceEntity["adx_paymentrequired"] = true;
				}
				if (Customer != null && Customer.LogicalName == "account")
				{
					sourceEntity["adx_customer"] = new EntityReference("account", Customer.Id);
				}
				context.Attach(sourceEntity);
				context.UpdateObject(sourceEntity);
				context.SaveChanges();
			}
			else
			{
				var sourceEntity = new Entity(control.CurrentStepEntityLogicalName);
				sourceEntity["adx_prepaidincidentscurrentlyavailable"] = PrepaidIncidentsAvailable < 0 ? 0 : PrepaidIncidentsAvailable;
				sourceEntity["adx_prepaidincidentsrequired"] = PrepaidIncidentsRequired < 0 ? 0 : PrepaidIncidentsRequired;
				if (SupportPlanID != Guid.Empty)
				{
					sourceEntity["adx_supportplan"] = new EntityReference("adx_supportplan", SupportPlanID);
				}
				else
				{
					sourceEntity["adx_paymentrequired"] = true;
				}
				if (Customer != null && Customer.LogicalName == "account")
				{
					sourceEntity["adx_customer"] = new EntityReference("account", Customer.Id);
				}
				context.AddObject(sourceEntity);
				context.SaveChanges();
				e.EntityID = sourceEntity.Id;
			}

			return e;
		}
	}
}
