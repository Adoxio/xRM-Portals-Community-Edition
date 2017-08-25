/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Site.Controls;
using System.Web;

namespace Site.Areas.HelpDesk.Controls
{
	public partial class CaseDeflection : PortalUserControl
	{
		public bool CaseEntitlementEnabled
		{
			get
			{
				var siteSetting = ServiceContext.GetSiteSettingValueByName(Website, "HelpDesk/CaseEntitlementEnabled") ?? "false";

				return siteSetting.ToLower() == "true";
			}
		}

		protected void Page_Load(object sender, EventArgs args)
		{
			if (IsPostBack)
			{
				return;
			}

			var dataAdapter = new UserCasesDataAdapter(new Adxstudio.Xrm.Cases.PortalContextDataAdapterDependencies(Portal, PortalName, Request.RequestContext));

			var permissionScopes = dataAdapter.SelectPermissionScopes();

			HideControlsBasedOnAccess(permissionScopes);
			
			var supportedProducts = XrmContext.CreateQuery("product")
				.Where(e => e.GetAttributeValue<OptionSetValue>("producttypecode") != null && e.GetAttributeValue<OptionSetValue>("producttypecode").Value == (int)ProductTypeCode.Supported)
				.OrderBy(e => e.GetAttributeValue<string>("name"))
				.ToArray();

			Product.DataSource = supportedProducts.Select(e => new ListItem(e.GetAttributeValue<string>("name"), e.GetAttributeValue<Guid>("productid").ToString()));
			Product.DataTextField = "Text";
			Product.DataValueField = "Value";
			Product.DataBind();

			if (supportedProducts.Length < 2)
			{
				Product.Visible = false;
			}

			var subject = HttpUtility.HtmlEncode(Request.QueryString["subject"]);

			if (!string.IsNullOrEmpty(subject))
			{
				Subject.Text =  HttpUtility.HtmlDecode(subject);
				ClearSearchButton.Visible = true;
				Deflection.Visible = true;
			}

			Guid productId;

			if (Guid.TryParse(Request.QueryString["product"], out productId))
			{
				Product.SelectedValue = productId.ToString();

				SearchData.Query = "(+_logicalname:incident~0.9^2 +statecode:@resolvedincidentstatecode +productid:@product~0.9 +(@subject)) OR (-_logicalname:incident~0.9 +(@subject))";
			}
			else
			{
				SetProductSelectedValue();
			}

			Subject.Attributes["onkeydown"] = OnEnterKeyDownThenClick(SearchButton);
		}

		private void HideControlsBasedOnAccess(ICaseAccessPermissionScopes permissionScopes)
		{
			var canCreate = permissionScopes.Self.Create || permissionScopes.Accounts.Any(permissions => permissions.Create);

			NoCaseAccessWarning.Visible = !canCreate;

			OpenNewSupportRequest.Visible = canCreate;
		}

		protected void SetProductSelectedValue()
		{
			var name = ServiceContext.GetSiteSettingValueByName(Website, "HelpDesk/Deflection/DefaultSelectedProductName");
			if (string.IsNullOrEmpty(name))
			{
				return;
			}
			SetProductSelectedValueByText(name);
		}

		protected void SetProductSelectedValueByText(string text)
		{
			var listItemToFind = Product.Items.FindByText(text);
			if (listItemToFind == null)
			{
				return;
			}
			if (Product.Items.Contains(listItemToFind))
			{
				Product.SelectedValue = listItemToFind.Value;
			}
		}

		protected void SearchData_OnSelected(object sender, SearchDataSourceStatusEventArgs args)
		{
			// If the SearchDataSource reports that the index was not found, try to build the index.
			if (args.Exception is IndexNotFoundException)
			{
				using (var builder = args.Provider.GetIndexBuilder())
				{
					builder.BuildIndex();
				}

				args.ExceptionHandled = true;

				// Redirect/refresh to let the user continue their search.
				Response.Redirect(Request.Url.PathAndQuery);
			}
		}

		protected string GetDisplayUrl(object urlData)
		{
			if (urlData == null)
			{
				return string.Empty;
			}

			try
			{
				return new UrlBuilder(urlData.ToString()).ToString();
			}
			catch (FormatException)
			{
				return string.Empty;
			}
		}

		protected void Submit_Click(object sender, EventArgs args)
		{
			if (string.IsNullOrEmpty(Subject.Text))
			{
				return;
			}

			var url = new UrlBuilder(Request.Url);

			url.QueryString.Set("subject", Subject.Text);

			var product = Product.SelectedItem;
			Guid productId;

			if (product != null && Guid.TryParse(product.Value, out productId))
			{
				url.QueryString.Set("product", productId.ToString());
			}

			Response.Redirect(url.PathWithQueryString);
		}

		protected string OpenNewSupportRequestSiteMarker = "Open New Support Request";

		protected void OpenNewSupportRequest_OnClick(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (CaseEntitlementEnabled)
			{
				var supportRequest = new Entity("adx_supportrequest");
				var metadataCache = new Dictionary<string, EntityMetadata>();

				supportRequest.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_name", Subject.Text, metadataCache);
				supportRequest.SetAttributeStringTruncatedToMaxLength(XrmContext, "adx_title", Subject.Text, metadataCache);
				supportRequest["adx_responsiblecontact"] = Portal.User.ToEntityReference();

				var product = Product.SelectedItem;
				Guid productId;

				if (product != null && Guid.TryParse(product.Value, out productId))
				{
					supportRequest["adx_product"] = new EntityReference("product", productId);
				}

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

		protected string BuildOpenNewSupportRequestUrl(Guid id)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			var page = portalContext.ServiceContext.GetPageBySiteMarkerName(portalContext.Website, OpenNewSupportRequestSiteMarker);

			if (page == null)
			{
				throw new Exception("Please contact your System Administrator. Required Site Marker '{0}' is missing.".FormatWith(OpenNewSupportRequestSiteMarker));
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

		private static string OnEnterKeyDownThenClick(Control button)
		{
			if (button == null) return string.Empty;

			return string.Format(@"
				if(!event.ctrlKey && !event.shiftKey && event.keyCode == 13) {{
					document.getElementById('{0}').click();
					return false;
				}}
				return true; ",
				button.ClientID);
		}
	}
}
