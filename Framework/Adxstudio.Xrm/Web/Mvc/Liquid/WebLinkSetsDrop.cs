/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class WebLinkSetsDrop : PortalDrop
	{
		private readonly IWebLinkSetDataAdapter _webLinkSets;

		public WebLinkSetsDrop(IPortalLiquidContext portalLiquidContext, IWebLinkSetDataAdapter webLinkSets) : base(portalLiquidContext)
		{
			if (webLinkSets == null) throw new ArgumentNullException("webLinkSets");

			_webLinkSets = webLinkSets;
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			Guid parsed;

			// If the method can be parsed as a Guid, look up the set by that.
			if (Guid.TryParse(method, out parsed))
			{
				var webLinkSetById = _webLinkSets.Select(parsed);

				return webLinkSetById == null ? null : new WebLinkSetDrop(this, webLinkSetById);
			}

			var webLinkSetByName = _webLinkSets.Select(method);

			return webLinkSetByName == null ? null : new WebLinkSetDrop(this, webLinkSetByName);
		}
	}
}
