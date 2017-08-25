/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[ToolboxData("<{0}:CrmHyperLink runat=server></{0}:CrmHyperLink>")]
	public class CrmHyperLink : Microsoft.Xrm.Portal.Web.UI.WebControls.CrmHyperLink
	{
		/// <summary>
		/// If set to true (default), the link will be hidden if the site marker is missing.
		/// </summary>
		public bool? AutoHiddenIfAbsent { get; set; }

		protected override void OnPreRender(EventArgs args)
		{
			if (!string.IsNullOrEmpty(SiteMarkerName))
			{
				var portalViewContext = new PortalViewContext(
					new PortalContextDataAdapterDependencies(
						PortalCrmConfigurationManager.CreatePortalContext(PortalName),
						PortalName,
						Context.Request.RequestContext));

				var target = portalViewContext.SiteMarkers.Select(SiteMarkerName);

				if (target != null)
				{
					NavigateUrl = new UrlBuilder(target.Url);

					if (string.IsNullOrEmpty(Text) && Controls.Count == 0)
					{
						var contentFormatter = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<ICrmEntityContentFormatter>(GetType().FullName) ?? new PassthroughCrmEntityContentFormatter();

						Text = contentFormatter.Format(target.Entity.GetAttributeValue<string>("adx_name"), target.Entity, this);
					}
				}
				else if (AutoHiddenIfAbsent ?? true)
				{
					Visible = false;
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
