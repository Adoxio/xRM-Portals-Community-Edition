/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc.Html;
using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class WebLinkSetDrop : PortalViewEntityDrop
	{
		private readonly Lazy<WebLinkDrop[]> _webLinks;

		public WebLinkSetDrop(IPortalLiquidContext portalLiquidContext, IWebLinkSet webLinkSet) : base(portalLiquidContext, webLinkSet)
		{
			if (webLinkSet == null) throw new ArgumentNullException("webLinkSet");

			WebLinkSet = webLinkSet;

			_webLinks = new Lazy<WebLinkDrop[]>(() => webLinkSet.WebLinks.Select(e => new WebLinkDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public string Copy
		{
			get { return WebLinkSet.Copy == null ? null : WebLinkSet.Copy.ToString(); }
		}

		public string Name
		{
			get { return WebLinkSet.Name; }
		}

		public string Title
		{
			get { return WebLinkSet.Title == null ? null : WebLinkSet.Title.ToString(); }
		}

		public IEnumerable<WebLinkDrop> Weblinks
		{
			get { return _webLinks.Value.AsEnumerable(); }
		}

		protected IWebLinkSet WebLinkSet { get; private set; }

		public override string GetEditable(Context context, EditableOptions options)
		{
			var html = Html.WebLinkSetEditingMetadata(WebLinkSet);

			return html == null ? null : html.ToString();
		}
	}
}
