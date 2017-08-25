/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SiteMapDrop : PortalDrop
	{
		private readonly Lazy<SiteMapNodeDrop> _current;
		private readonly Lazy<SiteMapNodeDrop> _root;

		public SiteMapDrop(IPortalLiquidContext portalLiquidContext, SiteMapProvider siteMap) : base(portalLiquidContext)
		{
			if (siteMap == null) throw new ArgumentNullException("siteMap");

			SiteMap = siteMap;

			_current = new Lazy<SiteMapNodeDrop>(() => GetDrop(SiteMap.CurrentNode), LazyThreadSafetyMode.None);
			_root = new Lazy<SiteMapNodeDrop>(() => GetDrop(SiteMap.RootNode), LazyThreadSafetyMode.None);
		}

		public SiteMapNodeDrop Current
		{
			get { return _current.Value; }
		}

		public SiteMapNodeDrop Root
		{
			get { return _root.Value; }
		}

		protected SiteMapProvider SiteMap { get; private set; }

		public override object BeforeMethod(string method)
		{
			return method == null ? null : GetDrop(SiteMap.FindSiteMapNode(method));
		}

		private SiteMapNodeDrop GetDrop(SiteMapNode node)
		{
			return node == null ? null : new SiteMapNodeDrop(this, node);
		}
	}
}
