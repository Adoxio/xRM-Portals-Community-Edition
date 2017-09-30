/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using Adxstudio.Xrm.Cms;

namespace Site.Areas.Cms
{
	public class CmsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Cms"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(AdDataAdapter.AdRoute, "_services/ads/{__portalScopeId__}/{id}", new { controller = "Ad", action = "Ad" });
			context.MapRoute(AdDataAdapter.PlacementRoute, "_services/ads/{__portalScopeId__}/placements/{id}", new { controller = "Ad", action = "AdPlacement" });
			context.MapRoute(AdDataAdapter.RandomAdRoute, "_services/ads/{__portalScopeId__}/placements/{id}/random", new { controller = "Ad", action = "RandomAd" });
			context.MapRoute(PollDataAdapter.SubmitPollRoute, "_services/polls/{__portalScopeId__}/SubmitPoll", new { controller = "Poll", action = "SubmitPoll" });
			context.MapRoute(PollDataAdapter.PollRoute, "_services/polls/{__portalScopeId__}/{id}", new { controller = "Poll", action = "Poll" });
			context.MapRoute(PollDataAdapter.PlacementRoute, "_services/polls/{__portalScopeId__}/placements/{id}", new { controller = "Poll", action = "PollPlacement" });
			context.MapRoute(PollDataAdapter.RandomPollRoute, "_services/polls/{__portalScopeId__}/placements/{id}/random", new { controller = "Poll", action = "RandomPoll" });
		}
	}
}
