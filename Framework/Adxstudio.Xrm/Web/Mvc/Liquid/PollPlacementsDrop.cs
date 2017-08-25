/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PollPlacementsDrop : PortalDrop
	{
		private readonly IPollDataAdapter _dataAdapter;

		public PollPlacementsDrop(IPortalLiquidContext portalLiquidContext, IPollDataAdapter dataAdapter)
			: base(portalLiquidContext)
		{
			if (dataAdapter == null) throw new ArgumentNullException("dataAdapter");

			_dataAdapter = dataAdapter;
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
				var pollPlacementById = _dataAdapter.SelectPollPlacement(parsed);

				return pollPlacementById == null ? null : new PollPlacementDrop(this, pollPlacementById);
			}

			var pollPlacementByName = _dataAdapter.SelectPollPlacement(method);

			return pollPlacementByName == null ? null : new PollPlacementDrop(this, pollPlacementByName);
		}
	}
}
