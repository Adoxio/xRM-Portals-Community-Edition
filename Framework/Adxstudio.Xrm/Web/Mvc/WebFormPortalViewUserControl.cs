/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;


namespace Adxstudio.Xrm.Web.Mvc
{
	public abstract class WebFormPortalViewUserControl : UI.WebForms.WebFormUserControl
	{
		private Lazy<HtmlHelper> _html;
		private Lazy<UrlHelper> _url;

		protected HtmlHelper Html
		{
			get { return _html.Value; }
		}

		protected new virtual string PortalName { get; set; }

		protected UrlHelper Url
		{
			get { return _url.Value; }
		}

		protected override void OnInit(EventArgs args)
		{
			base.OnInit(args);

			_html = PortalViewPage.GetLazyHtmlHelper(PortalName, Request.RequestContext, Response);
			_url = PortalViewPage.GetLazyUrlHelper(Request.RequestContext);
		}

		protected void RedirectToHttpsIfNecessary()
		{
			if (Request.IsSecureConnection)
				return;

			var redirectUrl = Uri.UriSchemeHttps + Uri.SchemeDelimiter + Request.Url.Authority + Request.Url.PathAndQuery;

			Response.Redirect(redirectUrl);
		}

		protected void RedirectToHttpIfNecessary()
		{
			if (!Request.IsSecureConnection)
				return;

			var redirectUrl = Uri.UriSchemeHttp + Uri.SchemeDelimiter + Request.Url.Authority + Request.Url.PathAndQuery;

			Response.Redirect(redirectUrl);
		}
	}
}
