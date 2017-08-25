/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Site.Pages;

namespace Site.Areas.Service311.Pages
{
	public partial class ServiceRequestStatus : PortalPage
	{
		private string ServiceRequestNumberQueryStringKey
		{
			get
			{
				var setting = ServiceContext.GetSiteSettingValueByName(Website, "311/ServiceRequests/StatusCheckQuerystringKey");

				return string.IsNullOrWhiteSpace(setting) ? "refnum" : setting;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			var servicerequestnumber = Request.QueryString[ServiceRequestNumberQueryStringKey];
			
			if (string.IsNullOrWhiteSpace(servicerequestnumber))
			{
				ErrorPanel.Visible = true;

				return;
			}

			var serviceRequest = ServiceContext.CreateQuery("adx_servicerequest").FirstOrDefault(s => s.GetAttributeValue<string>("adx_servicerequestnumber") == servicerequestnumber);

			if (serviceRequest == null)
			{
				ErrorPanel.Visible = true;

				return;
			}

			ErrorPanel.Visible = false;

			var url = ServiceRequestDetailsUrl(serviceRequest.Id);

			if (url == null)
			{
				return;
			}

			Response.Redirect(url);
		}

		protected string ServiceRequestDetailsUrl(Guid id)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Service Request Details");

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

			url.QueryString.Set("id", id.ToString());

			return url.PathWithQueryString;
		}
	}
}
