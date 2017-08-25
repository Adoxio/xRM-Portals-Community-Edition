/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Pages;
using PortalContextDataAdapterDependencies = Adxstudio.Xrm.Cases.PortalContextDataAdapterDependencies;

namespace Site.Areas.HelpDesk.Pages
{
	public partial class CreateCase : PortalPage
	{
		protected ICaseAccessPermissionScopesProvider CaseAccessPermissionScopesProvider { get; private set; }
		protected ICaseAccessPermissionScopes CaseAccessPermissionScopes { get; private set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (IsPostBack)
			{
				return;
			}

			CaseAccessPermissionScopesProvider = new ContactCaseAccessPermissionScopesProvider(Contact.ToEntityReference(), new PortalContextDataAdapterDependencies(Portal, PortalName, Request.RequestContext));

			CaseAccessPermissionScopes = CaseAccessPermissionScopesProvider.SelectPermissionScopes();

			if (!CaseAccessPermissionScopes.Self.Create && !CaseAccessPermissionScopes.Accounts.Any(o => o.Create))
			{
				CreateCaseForm.Visible = false;
				NoCaseAccessMessage.Visible = true;
				return;
			}

			var customerDropdown = (DropDownList)CreateCaseForm.FindControl("FormView").FindControl("CustomerDropdown");

			if (customerDropdown == null)
			{
				return;
			}

			customerDropdown.Items.Clear();

			if (CaseAccessPermissionScopes.Self.Create)
			{
				customerDropdown.Items.Add(new ListItem("Assigned to Me", Contact.Id.ToString()));
				customerDropdown.SelectedIndex = 0;
			}

			if (CaseAccessPermissionScopes.Accounts.Any(o => o.Create))
			{
				foreach (var accountAccessPermission in CaseAccessPermissionScopes.Accounts)
				{
					customerDropdown.Items.Add(new ListItem(accountAccessPermission.Account.Name, accountAccessPermission.Account.Id.ToString()));
					customerDropdown.SelectedIndex = 0;
				}
			}

			if ((CaseAccessPermissionScopes.Self.Create && !CaseAccessPermissionScopes.Accounts.Any(o => o.Create)) |
				(!CaseAccessPermissionScopes.Self.Create && CaseAccessPermissionScopes.Accounts.Count() <= 1))
			{
				customerDropdown.Enabled = false;
			}
			else
			{
				customerDropdown.Enabled = true;
			}
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			if (e.EntityId == null | e.EntityId == Guid.Empty)
			{
				throw new ApplicationException("The ID of the created record wasn't provided. This is usually a result of a plug-in failure. To troubleshoot, please check the ASP.NET trace or review the failed system jobs.");
			}

			var newId = e.EntityId ?? Guid.Empty;

			var attachment = (FileUpload)CreateCaseForm.FindControl("FormView").FindControl("Attachment");

			var caseDataAdapter = new CaseDataAdapter(new EntityReference("incident", newId), new PortalContextDataAdapterDependencies(Portal, PortalName, Request.RequestContext));

			if (attachment != null && attachment.HasFile)
			{
				caseDataAdapter.AddNote(string.Empty, attachment.PostedFile.FileName, attachment.PostedFile.ContentType, attachment.FileBytes);
			}

			// redirect to the case
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Case");
			if (page == null)
			{
				throw new ApplicationException("Required Site Marker named 'Case' does not exist.");
			}
			var url = new UrlBuilder(ServiceContext.GetUrl(page));
			url.QueryString.Set("caseid", newId.ToString());
			Response.Redirect(url.PathWithQueryString);
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var customerDropdown = (DropDownList)CreateCaseForm.FindControl("FormView").FindControl("CustomerDropdown");

			if (customerDropdown == null)
			{
				e.Values["customerid"] = Contact.ToEntityReference();
			}
			else
			{
				Guid customerId;

				if (Guid.TryParse(customerDropdown.SelectedValue, out customerId))
				{
					var customerLogicalName = customerId == Contact.Id ? "contact" : "account";

					var customer = new EntityReference(customerLogicalName, customerId);

					e.Values["customerid"] = customer;
				}
			}

			if (!e.Values.ContainsKey("customerid"))
			{
				e.Values["customerid"] = Contact.ToEntityReference();
			}

			if (!e.Values.ContainsKey("responsiblecontactid"))
			{
				e.Values["responsiblecontactid"] = Contact.ToEntityReference();
			}

			e.Values["adx_createdbyusername"] = Contact.GetAttributeValue<string>("fullname");

			// e.Values["adx_createdbyipaddress"] = Request.UserHostAddress;
		}
	}
}
