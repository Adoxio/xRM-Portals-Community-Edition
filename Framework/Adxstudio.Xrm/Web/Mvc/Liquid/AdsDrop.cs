/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AdsDrop : PortalDrop
	{
		private readonly IAdDataAdapter _ads;

		public AdsDrop(IPortalLiquidContext portalLiquidContext, IAdDataAdapter ads)
			: base(portalLiquidContext)
		{
			if (ads == null) throw new ArgumentNullException("ads");

			_ads = ads;

			Placements = new AdPlacementsDrop(portalLiquidContext, ads);
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
				var adById = _ads.SelectAd(parsed);

				return adById == null ? null : new AdDrop(this, adById);
			}

			var adByName = _ads.SelectAd(method);

			return adByName == null ? null : new AdDrop(this, adByName);
		}

		public AdPlacementsDrop Placements { get; private set; }
	}
}
