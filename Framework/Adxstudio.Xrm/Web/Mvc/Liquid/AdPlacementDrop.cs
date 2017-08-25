/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AdPlacementDrop : EntityDrop
	{
		private readonly Lazy<AdDrop[]> _ads;

		private readonly Lazy<string> _placementUrl;
		private readonly Lazy<string> _randomUrl;

		public AdPlacementDrop(IPortalLiquidContext portalLiquidContext, IAdPlacement adPlacement)
			: base(portalLiquidContext, adPlacement.Entity)
		{
			if (adPlacement == null) throw new ArgumentNullException("adPlacement");

			AdPlacement = adPlacement;

			_ads = new Lazy<AdDrop[]>(() => adPlacement.Ads.Select(e => new AdDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);

			_placementUrl = new Lazy<string>(GetPlacementUrl, LazyThreadSafetyMode.None);
			_randomUrl = new Lazy<string>(GetRandomUrl, LazyThreadSafetyMode.None);
		}

		public string Name
		{
			get { return AdPlacement.Name; }
		}

		public IEnumerable<AdDrop> Ads
		{
			get { return _ads.Value.AsEnumerable(); }
		}

		protected IAdPlacement AdPlacement { get; private set; }

		public string PlacementUrl
		{
			get { return _placementUrl.Value; }
		}

		public string RandomUrl
		{
			get { return _randomUrl.Value; }
		}

		private string GetPlacementUrl()
		{
			return UrlHelper.RouteUrl(AdDataAdapter.PlacementRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}

		private string GetRandomUrl()
		{
			return UrlHelper.RouteUrl(AdDataAdapter.RandomAdRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}
	}
}
