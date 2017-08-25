/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public class CrmHyperLink : HyperLink
	{
		public string SiteMarkerName { get; set; }

		public string QueryString { get; set; }

		public string PortalName { get; set; }

		protected override void OnPreRender(EventArgs args)
		{
			if (!string.IsNullOrEmpty(SiteMarkerName))
			{
				var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var context = portal.ServiceContext;
				var website = portal.Website;

				var page = context.GetPageBySiteMarkerName(website, SiteMarkerName);

				if (page != null)
				{
					page.AssertEntityName("adx_webpage");

					NavigateUrl = portal.ServiceContext.GetUrl(page);
					
					if (string.IsNullOrEmpty(Text) && Controls.Count == 0)
					{
						var contentFormatter = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityContentFormatter>(GetType().FullName) ?? new PassthroughCrmEntityContentFormatter();

						Text = contentFormatter.Format(page.GetAttributeValue<string>("adx_name"), page, this);
					}
				}
			}

			if (!string.IsNullOrEmpty(QueryString))
			{
				// we need to append these querystring parameters
				if (!NavigateUrl.Contains("?"))
				{
					NavigateUrl += "?";
				}

				if (!NavigateUrl.EndsWith("?"))
				{
					NavigateUrl += "&";
				}

				NavigateUrl += QueryString;
			}
		}
	}
}
