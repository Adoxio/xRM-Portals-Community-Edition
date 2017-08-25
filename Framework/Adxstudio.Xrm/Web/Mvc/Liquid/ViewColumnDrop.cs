/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.UI.JsonConfiguration;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ViewColumnDrop : PortalDrop
	{
		public ViewColumnDrop(IPortalLiquidContext portalLiquidContext, IViewColumn column) : base(portalLiquidContext)
		{
			AttributeLogicalName = column.AttributeLogicalName;

			DisplayName = column.DisplayName;

			Width = column.Width;
		}

		public string AttributeLogicalName { get; set; }
		
		public string DisplayName { get; set; }
		
		public int Width { get; set; }
	}
}
