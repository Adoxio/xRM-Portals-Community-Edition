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
	public class PollPlacementDrop : EntityDrop
	{
		private readonly Lazy<PollDrop[]> _polls;

		private readonly Lazy<string> _placementUrl;
		private readonly Lazy<string> _randomUrl;
		private readonly Lazy<string> _submitUrl;

		public PollPlacementDrop(IPortalLiquidContext portalLiquidContext, IPollPlacement pollPlacement)
			: base(portalLiquidContext, pollPlacement.Entity)
		{
			if (pollPlacement == null) throw new ArgumentNullException("pollPlacement");

			PollPlacement = pollPlacement;

			_polls = new Lazy<PollDrop[]>(() => pollPlacement.Polls.Select(e => new PollDrop(this, e)).ToArray(),
				LazyThreadSafetyMode.None);

			_placementUrl = new Lazy<string>(GetPlacementUrl, LazyThreadSafetyMode.None);
			_randomUrl = new Lazy<string>(GetRandomUrl, LazyThreadSafetyMode.None);
			_submitUrl = new Lazy<string>(GetSubmitUrl, LazyThreadSafetyMode.None);
		}

		public string Name
		{
			get { return PollPlacement.Name; }
		}

		public IEnumerable<PollDrop> Polls
		{
			get { return _polls.Value.AsEnumerable(); }
		}

		protected IPollPlacement PollPlacement { get; private set; }

		public string PlacementUrl
		{
			get { return _placementUrl.Value; }
		}

		public string RandomUrl
		{
			get { return _randomUrl.Value; }
		}

		public string SubmitUrl
		{
			get { return _submitUrl.Value; }
		}

		private string GetPlacementUrl()
		{
			return UrlHelper.RouteUrl(PollDataAdapter.PlacementRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}

		private string GetRandomUrl()
		{
			return UrlHelper.RouteUrl(PollDataAdapter.RandomPollRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}

		private string GetSubmitUrl()
		{
			return UrlHelper.RouteUrl(PollDataAdapter.SubmitPollRoute, new
			{
				id = Id,
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id
			});
		}
	}
}
