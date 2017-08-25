/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SharePointDrop : PortalDrop
	{
		public SharePointDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext)
		{
			Documents = new SharePointDocumentsDrop(portalLiquidContext);
		}

		public SharePointDocumentsDrop Documents { get; private set; }
	}
}
