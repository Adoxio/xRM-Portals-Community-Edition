/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Service311.Pages
{
	public partial class Home_facebook : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ServiceRequestsListView.DataSource = GetServiceRequests();
			ServiceRequestsListView.DataBind();
		}

		private IEnumerable<SiteMapNode> GetServiceRequests()
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Service Requests List");

			if (page == null)
			{
				return new SiteMapNode[] { };
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(page));

			var serviceRequestsNode = System.Web.SiteMap.Provider.FindSiteMapNodeFromKey(url);

			if (serviceRequestsNode == null)
			{
				return new SiteMapNode[] { };
			}

			var nodes = serviceRequestsNode.ChildNodes.Cast<SiteMapNode>().ToList();

			return nodes;
		}

		protected string GetThumbnailUrl(object webpageObject)
		{
			var webpageEntity = webpageObject as Entity;

			if (webpageEntity == null)
			{
				return null;
			}

			var imageReference = webpageEntity.GetAttributeValue<EntityReference>("adx_image");

			if (imageReference == null)
			{
				return null;
			}

			var webfile = ServiceContext.CreateQuery("adx_webfile").FirstOrDefault(file => file.GetAttributeValue<Guid>("adx_webfileid") == imageReference.Id);

			if (webfile == null)
			{
				return null;
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(webfile));

			return url.PathWithQueryString;
		}
	}
}
