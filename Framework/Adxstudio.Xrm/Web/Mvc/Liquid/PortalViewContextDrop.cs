/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PortalViewContextDrop : PortalDrop
	{
		public PortalViewContextDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext)
		{
			var viewContext = portalLiquidContext.PortalViewContext;

			Entity = viewContext.Entity == null ? null : new PortalViewEntityDrop(portalLiquidContext, viewContext.Entity);
			User = viewContext.User == null ? null : new UserDrop(portalLiquidContext, viewContext.User);
			Website = viewContext.Website == null ? null : new WebsiteDrop(portalLiquidContext, viewContext.Website);
		}

		public PortalViewContextDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies) : base(portalLiquidContext)
		{
			var viewContext = portalLiquidContext.PortalViewContext;
			
			Entity = viewContext.Entity == null ? null : new PortalViewEntityDrop(portalLiquidContext, viewContext.Entity);
			User = viewContext.User == null ? null : new UserDrop(portalLiquidContext, viewContext.User);
			Website = viewContext.Website == null ? null : new WebsiteDrop(portalLiquidContext, viewContext.Website, dependencies);
		}

		public PortalViewEntityDrop Entity { get; private set; }

		public UserDrop User { get; private set; }

		public WebsiteDrop Website { get; private set; }
	}
}
