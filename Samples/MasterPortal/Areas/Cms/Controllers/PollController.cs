/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Configuration;

namespace Site.Areas.Cms.Controllers
{
	public class PollController : Controller
	{
		private const string poll_alias = "poll";
		private const string placement_alias = "placement";

		private PollDataAdapter _dataAdapter;

		private PollDataAdapter DataAdapter
		{
			get
			{
				if (_dataAdapter == null)
				{
					var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
					var dependencies = new PortalContextDataAdapterDependencies(portalContext);
					_dataAdapter = new PollDataAdapter(dependencies);
				}
				return _dataAdapter;
			}
		}

		[HttpGet]
		public ActionResult Poll(string id, string alias = poll_alias)
		{
			Guid guid;
			var poll = Guid.TryParse(id, out guid) ? DataAdapter.SelectPoll(guid) : DataAdapter.SelectPoll(id);

			return PollView(poll, alias);
		}

		[ChildActionOnly]
		public ActionResult PollPlacement(string id, bool random = true, string alias = placement_alias)
		{
			Guid guid;
			var placement = Guid.TryParse(id, out guid)
				? DataAdapter.SelectPollPlacement(guid)
				: DataAdapter.SelectPollPlacement(id);

			return PlacementView(placement, random, alias);
		}

		[HttpGet]
		public ActionResult RandomPoll(string id, string alias = poll_alias)
		{
			Guid guid;
			var poll = Guid.TryParse(id, out guid)
				? DataAdapter.SelectRandomPoll(guid)
				: DataAdapter.SelectRandomPoll(id);

			return PollView(poll, alias);
		}

		private PortalViewContext PortalViewContext()
		{
			// Hack - it feels like separation of controller/view is quite difficult here
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);
			var portalViewContext = new PortalViewContext(dataAdapterDependencies, requestContext: Request.RequestContext);
			return portalViewContext;
		}

		private ActionResult PollView(IPoll poll, string alias)
		{
			ViewData["alias"] = alias;

			ViewData[PortalExtensions.PortalViewContextKey] = PortalViewContext();

			return View("Poll", poll);
		}

		private ActionResult PlacementView(IPollPlacement placement, bool random, string alias)
		{
			ViewData["alias"] = alias;

			ViewData["random"] = random;

			ViewData[PortalExtensions.PortalViewContextKey] = PortalViewContext();

			return View("PollPlacement", placement);
		}

		[HttpPost]
		[AjaxValidateAntiForgeryToken]
		public ActionResult SubmitPoll(Guid pollId, Guid optionId, string alias = poll_alias)
		{
			var poll = DataAdapter.SelectPoll(pollId);

			if (poll != null)
			{
				var pollOption = poll.Options.FirstOrDefault(o => o.Id == optionId);

				if (pollOption != null)
				{
					DataAdapter.SubmitPoll(poll, pollOption);

					poll = DataAdapter.SelectPoll(poll.Id);

					return PollView(poll, alias);
				}
			}

			return PollView(poll, alias);
		}
	}
}
