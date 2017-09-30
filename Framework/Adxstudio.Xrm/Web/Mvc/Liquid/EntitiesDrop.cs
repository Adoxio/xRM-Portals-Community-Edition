/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntitiesDrop : PortalDrop
	{
		public EntitiesDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext) { }

		public override object BeforeMethod(string method)
		{
			return method == null ? null : new EntitySetDrop(this, method);
		}
	}
}
