/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Configuration of links for actions on a form or view.
	/// </summary>
	public class RedirectActionLink : ViewActionLink
	{
		/// <summary>
		/// Indicates either to do a redirect to web page, or redirect to URL or refresh the current grid/form on complete of the action.
		/// </summary>
		public OnComplete? OnComplete { get; set; }

		/// <summary>
		/// URL to Redirect to upon completion
		/// </summary>
		public UrlBuilder RedirectUrl { get; set; }

		/// <summary>
		/// Entity Reference to the Web Page record to be redirect to instead of URL.
		/// </summary>
		public EntityReference WebPage { get; set; }

		public RedirectActionLink()
		{
		}

		public RedirectActionLink(LinkActionType type, bool enabled, UrlBuilder url, string label, string tooltip)
			: base(type, enabled, url, label, tooltip, null)
		{
			OnComplete = JsonConfiguration.OnComplete.Refresh;
		}

		public RedirectActionLink(EntityReference page, LinkActionType type, bool enabled, string label, string tooltip)
			: base(type, enabled, null, label, tooltip, null)
		{
			if (page == null || page.LogicalName != "adx_webpage") return;
			WebPage = page;
			OnComplete = JsonConfiguration.OnComplete.RedirectToWebPage;
		}

		public RedirectActionLink(IPortalContext portalContext, int languageCode, RedirectAction action, LinkActionType type, bool enabled = false, UrlBuilder url = null, string portalName = null, string label = null, string tooltip = null) : base(portalContext, languageCode, action, type, enabled, url, portalName, label, tooltip)
		{
			string redirectUrl = null;
			
			OnComplete = action.OnComplete.GetValueOrDefault(JsonConfiguration.OnComplete.Refresh);
			
			switch (OnComplete)
			{
				case JsonConfiguration.OnComplete.Refresh:
					return;
				case JsonConfiguration.OnComplete.RedirectToWebPage:
					if (action.RedirectWebpageId != null)
					{
						redirectUrl = action.GetRedirectWebPageUrl(portalContext, portalName);
					}
					break;
				case JsonConfiguration.OnComplete.RedirectToUrl:
					if (action.RedirectUrl != null)
					{
						redirectUrl = action.RedirectUrl;
					}
					break;
			}

			if (string.IsNullOrEmpty(redirectUrl)) return;

			var urlBuilder = new UrlBuilder(redirectUrl);

			RedirectUrl = urlBuilder;
		}
	}
}
