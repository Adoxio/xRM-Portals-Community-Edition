/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class WebLinkDrop : EntityDrop
	{
		private readonly Lazy<WebLinkDrop[]> _webLinks;

		public WebLinkDrop(IPortalLiquidContext portalLiquidContext, IWebLink webLink) : base(portalLiquidContext, webLink.Entity)
		{
			if (webLink == null) throw new ArgumentNullException("webLink");

			WebLink = webLink;

			Image = WebLink.HasImage ? new WebLinkImageDrop(WebLink) : null;

			_webLinks = new Lazy<WebLinkDrop[]>(() => WebLink.WebLinks.Select(e => new WebLinkDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public string Description
		{
			get { return WebLink.Description == null ? null : WebLink.Description.ToString(); }
		}

		public bool DisplayImageOnly
		{
			get { return WebLink.DisplayImageOnly; }
		}

		public bool DisplayPageChildLinks
		{
			get { return WebLink.DisplayPageChildLinks; }
		}

		public WebLinkImageDrop Image { get; private set; }

		public bool IsExternal
		{
			get { return WebLink.IsExternal; }
		}

		public string Name
		{
			get { return WebLink.Name == null ? null : WebLink.Name.ToString(); }
		}

		public bool Nofollow
		{
			get { return WebLink.NoFollow; }
		}

		public bool OpenInNewWindow
		{
			get { return WebLink.OpenInNewWindow; }
		}

		public string Tooltip
		{
			get { return WebLink.ToolTip; }
		}

		public override string Url
		{
			get { return WebLink.Url; }
		}

		public IEnumerable<WebLinkDrop> Weblinks
		{
			get { return _webLinks.Value.AsEnumerable(); }
		}

		protected IWebLink WebLink { get; private set; }
	}
}
