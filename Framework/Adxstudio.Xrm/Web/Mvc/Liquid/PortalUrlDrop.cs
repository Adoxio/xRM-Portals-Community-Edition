/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public abstract class PortalUrlDrop : PortalDrop
	{
		private Lazy<bool> _isSitemapAncestor;
		private Lazy<bool> _isSitemapCurrent;

		protected PortalUrlDrop(IPortalLiquidContext portalLiquidContext) : base(portalLiquidContext)
		{
			_isSitemapAncestor = new Lazy<bool>(() => PortalViewContext.IsAncestorSiteMapNode(Url), LazyThreadSafetyMode.None);
			_isSitemapCurrent = new Lazy<bool>(() => PortalViewContext.IsCurrentSiteMapNode(Url), LazyThreadSafetyMode.None);
		}

		public bool IsSitemapAncestor
		{
			get { return _isSitemapAncestor.Value; }
		}

		public bool IsSitemapCurrent
		{
			get { return _isSitemapCurrent.Value; }
		}

		public abstract string Url { get; }
	}
}
