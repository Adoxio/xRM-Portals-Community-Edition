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
	public partial class Home : PortalPage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			var setting = ServiceContext.CreateQuery("adx_sitesetting").FirstOrDefault(s => s.GetAttributeValue<string>("adx_name") == "twitter_feed_enabled");

			var twitterEnabled = false;

			if (setting != null)
			{
				bool.TryParse(setting.GetAttributeValue<string>("adx_value"), out twitterEnabled);
			}

			TwitterFeedPanel.Visible = twitterEnabled;

			ServiceRequestsListView.DataSource = GetServiceRequests();
			ServiceRequestsListView.DataBind();
		}

		private IEnumerable<SiteMapNode> GetServiceRequests()
		{
			var nodes = new List<SiteMapNode> { };

			var linkSet = ServiceContext.GetLinkSetByName(Website, "Service Requests List");

			if (linkSet != null)
			{

				var pages = from page in ServiceContext.CreateQuery("adx_webpage")
							join link in ServiceContext.CreateQuery("adx_weblink") 
								on page.GetAttributeValue<Guid>("adx_webpageid") equals link.GetAttributeValue<EntityReference>("adx_pageid").Id
							join set in ServiceContext.CreateQuery("adx_weblinkset") 
								on link.GetAttributeValue<EntityReference>("adx_weblinksetid").Id equals set.GetAttributeValue<Guid>("adx_weblinksetid")
							where set.GetAttributeValue<string>("adx_name") == "Service Requests List"
							where set.GetAttributeValue<EntityReference>("adx_websiteid").Id == Website.Id
							select page;

				foreach (var page in pages)
				{
					nodes.Add(System.Web.SiteMap.Provider.FindSiteMapNode(ServiceContext.GetUrl(page)));
				}

			}
			else
			{
				var page = ServiceContext.GetPageBySiteMarkerName(Website, "Service Requests List");

				if (page != null)
				{
					var url = new UrlBuilder(ServiceContext.GetUrl(page));

					var serviceRequestsNode = System.Web.SiteMap.Provider.FindSiteMapNodeFromKey(url.ToString());

					if (serviceRequestsNode != null)
					{
						nodes = serviceRequestsNode.ChildNodes.Cast<SiteMapNode>().ToList();
					}
				}
			}

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
