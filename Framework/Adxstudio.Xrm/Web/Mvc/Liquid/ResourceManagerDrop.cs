/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ResourceManagerDrop : PortalDrop
	{
		public ResourceManagerDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext) { }

		public override object BeforeMethod(string method)
		{
			return ResourceManager.GetString(method);
		}
	}
}
