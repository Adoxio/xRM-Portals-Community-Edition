/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adxstudio.Xrm.Events;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SponsorDrop : EntityDrop
	{
		public SponsorDrop(IPortalLiquidContext portalLiquidContext, IEventSponsor eventSponsor)
			: base(portalLiquidContext, eventSponsor.Entity)
		{

		}
	}
}
