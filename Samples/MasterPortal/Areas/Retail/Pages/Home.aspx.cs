/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Products;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Retail.Pages
{
	public partial class Home : PortalPage
	{
		private readonly Lazy<IPortalContext> _portal = new Lazy<IPortalContext>(() => PortalCrmConfigurationManager.CreatePortalContext(), LazyThreadSafetyMode.None);
		protected EntityReference Campaign;

		protected void Page_Load(object sender, EventArgs e)
		{
			var featuredProductsCampaignCode = ServiceContext.GetSiteSettingValueByName(Website, "Retail/Featured Products Campaign Code");

			if (string.IsNullOrWhiteSpace(featuredProductsCampaignCode))
			{
				return;
			}

			var campaign = ServiceContext.CreateQuery("campaign").FirstOrDefault(c => c.GetAttributeValue<string>("codename") == featuredProductsCampaignCode);

			if (campaign != null)
			{
				Campaign = campaign.ToEntityReference();
			}
			else
			{
				FeaturedProductsPanel.Visible = false;
			}
		}

		protected void CreateCampaignProductsDataAdapter(object sender, ObjectDataSourceEventArgs e)
		{
			e.ObjectInstance = new CampaignProductsDataAdapter(Campaign, new PortalContextDataAdapterDependencies(_portal.Value, null, Request.RequestContext));
		}
	}
}
