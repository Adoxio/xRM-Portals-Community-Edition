/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SiteMarkersDrop : PortalDrop
	{
		private readonly ISiteMarkerDataAdapter _siteMarkers;

		public SiteMarkersDrop(IPortalLiquidContext portalLiquidContext, ISiteMarkerDataAdapter siteMarkers) : base(portalLiquidContext)
		{
			if (siteMarkers == null) throw new ArgumentNullException("siteMarkers");

			_siteMarkers = siteMarkers;
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			var target = _siteMarkers.Select(method);

			return target == null ? null : new SiteMarkerDrop(this, target);
		}
	}
}
