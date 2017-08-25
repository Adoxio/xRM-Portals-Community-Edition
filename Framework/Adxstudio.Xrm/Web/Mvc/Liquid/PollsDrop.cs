/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PollsDrop : PortalDrop
	{
		private readonly IPollDataAdapter _dataAdapter;

		public PollsDrop(IPortalLiquidContext portalLiquidContext, IPollDataAdapter dataAdapter)
			: base(portalLiquidContext)
		{
			if (dataAdapter == null) throw new ArgumentNullException("dataAdapter");

			_dataAdapter = dataAdapter;

			Placements = new PollPlacementsDrop(portalLiquidContext, dataAdapter);
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
				var pollById = _dataAdapter.SelectPoll(parsed);

				return pollById == null ? null : new PollDrop(this, pollById);
			}

			var pollByName = _dataAdapter.SelectPoll(method);

			return pollByName == null ? null : new PollDrop(this, pollByName);
		}

		public PollPlacementsDrop Placements { get; private set; }
	}
}
