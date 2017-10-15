/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[ToolboxData("<{0}:SiteMarkerLinkButton runat=\"server\">SiteMarkerLinkButton</{0}:SiteMarkerLinkButton>")]
	public class SiteMarkerLinkButton : LinkButton
	{
		
		private IPortalContext Portal
		{
			get { return PortalCrmConfigurationManager.CreatePortalContext(PortalName); }
		}

		public string SiteMarkerName { get; set; }

		public string PortalName { get; set; }

		/// <summary>
		/// If set to true (default), the link will be hidden if the site marker is missing.
		/// </summary>
		public bool? AutoHiddenIfAbsent { get; set; }

		public QueryStringCollection QueryStringCollection { get; set; }

		protected Entity RedirectPage
		{
			get
			{
				if (!string.IsNullOrEmpty(SiteMarkerName))
				{
					
					var context = Portal.ServiceContext;
					var website = Portal.Website;

					var page = context.GetPageBySiteMarkerName(website, SiteMarkerName);

					return page;
				}
				return null;
			}
		}

		protected override void OnPreRender(EventArgs args)
		{
			if (RedirectPage == null && (AutoHiddenIfAbsent ?? true))
			{
				Visible = false;
			}
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (RedirectPage != null)
			{
				RedirectPage.AssertEntityName("adx_webpage");

				var navigateUrl = new UrlBuilder(Portal.ServiceContext.GetUrl(RedirectPage)) { QueryString = QueryStringCollection };

				HttpContext.Current.Response.Redirect(navigateUrl.PathWithQueryString);
			}
		}
	}
}
