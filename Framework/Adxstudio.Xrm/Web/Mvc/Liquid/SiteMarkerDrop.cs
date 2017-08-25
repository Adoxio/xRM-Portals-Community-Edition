/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SiteMarkerDrop : EntityDrop
	{
		public SiteMarkerDrop(IPortalLiquidContext portalLiquidContext, ISiteMarkerTarget target) : base(portalLiquidContext, target.Entity)
		{
			if (target == null) throw new ArgumentNullException("target");

			Target = target;
		}

		public override string Url
		{
			get { return Target.Url; }
		}

		protected ISiteMarkerTarget Target { get; private set; }
	}
}
