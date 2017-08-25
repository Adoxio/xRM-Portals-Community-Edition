/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Cms.Controllers
{
	public class AdController : Controller
	{
		private const string ad_alias = "ad";
		private const string placement_alias = "placement";

		private AdDataAdapter _dataAdapter;

		private AdDataAdapter DataAdapter
		{
			get
			{
				if (_dataAdapter == null)
				{
					var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
					var dependencies = new PortalContextDataAdapterDependencies(portalContext);
					_dataAdapter = new AdDataAdapter(dependencies);
				}
				return _dataAdapter;
			}
		}

		[AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
		public ActionResult Ad(string id, bool showcopy = true, string alias = ad_alias)
		{
			Guid guid;
			var ad = Guid.TryParse(id, out guid) ? DataAdapter.SelectAd(guid) : DataAdapter.SelectAd(id);

			return AdView(ad, showcopy, alias);
		}

		[AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
		public ActionResult AdPlacement(string id, bool showcopy = true, bool random = true, string alias = placement_alias)
		{
			Guid guid;
			var placement = Guid.TryParse(id, out guid)
				? DataAdapter.SelectAdPlacement(guid)
				: DataAdapter.SelectAdPlacement(id);

			return PlacementView(placement, showcopy, random, alias);
		}

		[AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
		public ActionResult RandomAd(string id, bool showcopy = true, string alias = ad_alias)
		{
			Guid guid;
			var ad = Guid.TryParse(id, out guid)
				? DataAdapter.SelectRandomAd(guid)
				: DataAdapter.SelectRandomAd(id);

			return AdView(ad, showcopy, alias);
		}

		private PortalViewContext PortalViewContext()
		{
			// Hack - it feels like separation of controller/view is quite difficult here
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var portalViewContext = new PortalViewContext(dataAdapterDependencies, requestContext: Request.RequestContext);
			return portalViewContext;
		}

		private ActionResult AdView(IAd ad, bool showcopy, string alias)
		{
			ViewData["showcopy"] = showcopy;

			ViewData["alias"] = alias;

			ViewData[PortalExtensions.PortalViewContextKey] = PortalViewContext();

			return View("Ad", ad);
		}

		private ActionResult PlacementView(IAdPlacement placement, bool showcopy, bool random, string alias)
		{
			ViewData["showcopy"] = showcopy;

			ViewData["random"] = random;

			ViewData["alias"] = alias;

			ViewData[PortalExtensions.PortalViewContextKey] = PortalViewContext();

			return View("AdPlacement", placement);
		}
	}
}
