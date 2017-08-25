/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;
using Site.Pages;
using PortalContextDataAdapterDependencies = Adxstudio.Xrm.Cases.PortalContextDataAdapterDependencies;
using System.Web;

namespace Site.Areas.HelpDesk.Pages
{
	public partial class HelpDesk : PortalPage
	{
		public bool CaseDeflectionEnabled
		{
			get { return Html.BooleanSetting("HelpDesk/CaseDeflection/Enabled").GetValueOrDefault(true); }
		}

		public bool CaseEntitlementEnabled
		{
			get { return Html.BooleanSetting("HelpDesk/CaseEntitlementEnabled").GetValueOrDefault(false); }
		}

		public bool DirectCaseCreationEnabled
		{
			get { return Html.BooleanSetting("HelpDesk/DirectCaseCreation/Enabled").GetValueOrDefault(false); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			var dataAdapter = new UserCasesDataAdapter(new PortalContextDataAdapterDependencies(Portal, PortalName, Request.RequestContext));

			if (!IsPostBack)
			{
				CaseDeflection.Visible = CaseDeflectionEnabled;
				CreateCase.Visible = DirectCaseCreationEnabled;

				var permissionScopes = dataAdapter.SelectPermissionScopes();

				HideControlsBasedOnAccess(permissionScopes);
				PopulateCustomerFilter(permissionScopes);
			}

			var cases = GetCases(dataAdapter);

			var columnsGenerator = new SavedQueryColumnsGenerator(XrmContext, "incident", "Cases Web View");

			CaseList.DataKeyNames = new[] { "incidentid" };
			CaseList.DataSource = columnsGenerator.ToDataTable(cases.Select(c => c.Entity), "o", CultureInfo.InvariantCulture);
			CaseList.ColumnsGenerator = columnsGenerator;
			CaseList.DataBind();
		}

		protected void CaseList_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow || e.Row.Cells.Count < 1)
			{
				return;
			}

			var dataKey = CaseList.DataKeys[e.Row.RowIndex];

			if (dataKey != null)
			{
				var caseUrl = CaseUrl(dataKey.Value);

				if (!string.IsNullOrEmpty(caseUrl))
				{
					e.Row.Cells[0].Text = string.Format(@"<a href=""{0}"">{1}</a>", HttpUtility.HtmlEncode(caseUrl), HttpUtility.HtmlEncode(e.Row.Cells[0].Text));
				}
			}

			foreach (TableCell cell in e.Row.Cells)
			{
				DateTime cellAsDateTime;

				if (DateTime.TryParseExact(cell.Text, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out cellAsDateTime))
				{
					cell.Text = @"<abbr class=""timeago"">{0:r}</abbr>".FormatWith(cellAsDateTime);
				}
			}
		}

		protected string CaseUrl(object id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Case");

			if (page == null)
			{
				return null;
			}

			var pageUrl = ServiceContext.GetUrl(page);

			if (pageUrl == null)
			{
				return null;
			}

			var url = new UrlBuilder(pageUrl);

			url.QueryString.Set("caseid", id.ToString());

			return url.PathWithQueryString;
		}

		private IEnumerable<ICase> GetCases(ICaseAggregationDataAdapter dataAdapter)
		{
			var caseState = string.Equals(StatusDropDown.Text, "Active", StringComparison.InvariantCulture)
				? CaseState.Active
				: CaseState.Resolved;

			Guid account;

			return Guid.TryParse(CustomerFilter.SelectedValue, out account) && account != Guid.Empty
				? dataAdapter.SelectCases(account, caseState)
				: dataAdapter.SelectCases(caseState);
		}

		private void HideControlsBasedOnAccess(ICaseAccessPermissionScopes permissionScopes)
		{
			var canRead = permissionScopes.Self.Read || permissionScopes.Accounts.Any(permissions => permissions.Read);
			
			CaseList.Visible = CaseControls.Visible = canRead;
			NoCaseAccessWarning.Visible = !canRead;

			var canCreate = permissionScopes.Self.Create || permissionScopes.Accounts.Any(permissions => permissions.Create);

			CreateCase.Visible = CreateCase.Visible && canCreate;
		}

		private void PopulateCustomerFilter(ICaseAccessPermissionScopes permissionScopes)
		{
			if (!CustomerFilter.Visible)
			{
				return;
			}

			CustomerFilter.Items.Clear();

			if (permissionScopes.Self.Read)
			{
				CustomerFilter.Items.Add(new ListItem("My Cases", Guid.Empty.ToString()) { Selected = true });
			}

			foreach (var accountPermissions in permissionScopes.Accounts.Where(permissions => permissions.Read))
			{
				CustomerFilter.Items.Add(new ListItem(accountPermissions.Account.Name, accountPermissions.Account.Id.ToString()));
			}
		}

		protected void CreateCase_Click(object sender, EventArgs args)
		{
			RedirectToLoginIfAnonymous();

			if (CaseEntitlementEnabled)
			{
				var supportRequest = new Entity("adx_supportrequest");

				supportRequest["adx_name"] = "Support Request for {0}".FormatWith(Portal.User.GetAttributeValue<string>("fullname"));
				supportRequest["adx_responsiblecontact"] = Portal.User.ToEntityReference();

				XrmContext.AddObject(supportRequest);
				XrmContext.SaveChanges();

				var redirectUrl = BuildOpenNewSupportRequestUrl(supportRequest.Id);

				Response.Redirect(redirectUrl);
			}
			else
			{
				var redirectUrl = CreateCaseUrl();

				Response.Redirect(redirectUrl);
			}
		}

		protected string CreateCaseUrl()
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Create Case");

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			return url.PathWithQueryString;
		}

		protected string OpenNewSupportRequestSiteMarker = "Open New Support Request";

		protected string BuildOpenNewSupportRequestUrl(Guid id)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			var page = portalContext.ServiceContext.GetPageBySiteMarkerName(portalContext.Website, OpenNewSupportRequestSiteMarker);

			if (page == null)
			{
				throw new Exception("Please contact your system administrator. The required site marker {0} is missing.".FormatWith(OpenNewSupportRequestSiteMarker));
			}

			var pageUrl = portalContext.ServiceContext.GetUrl(page);

			if (pageUrl == null)
			{
				throw new Exception("Please contact your System Administrator. Unable to build URL for Site Marker {0}.".FormatWith(OpenNewSupportRequestSiteMarker));
			}

			var url = new UrlBuilder(pageUrl);

			url.QueryString.Set("id", id.ToString());

			return WebsitePathUtility.ToAbsolute(portalContext.Website, url.PathWithQueryString);
		}
	}
}
