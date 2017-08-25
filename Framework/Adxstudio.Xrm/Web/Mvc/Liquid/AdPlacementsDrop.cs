/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AdPlacementsDrop : PortalDrop
	{
		private readonly IAdDataAdapter _adPlacements;

		public AdPlacementsDrop(IPortalLiquidContext portalLiquidContext, IAdDataAdapter adPlacements)
			: base(portalLiquidContext)
		{
			if (adPlacements == null) throw new ArgumentNullException("adPlacements");

			_adPlacements = adPlacements;
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
				var adPlacementById = _adPlacements.SelectAdPlacement(parsed);

				return adPlacementById == null ? null : new AdPlacementDrop(this, adPlacementById);
			}

			var adPlacementByName = _adPlacements.SelectAdPlacement(method);

			return adPlacementByName == null ? null : new AdPlacementDrop(this, adPlacementByName);
		}
	}
}
