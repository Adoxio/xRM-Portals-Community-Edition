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
	public class PollDrop : EntityDrop
	{
		private readonly Lazy<PollOptionDrop[]> _options;

		private readonly Lazy<string> _pollUrl;
		private readonly Lazy<string> _submitUrl;

		public PollDrop(IPortalLiquidContext portalLiquidContext, IPoll poll)
			: base(portalLiquidContext, poll.Entity)
		{
			if (poll == null) throw new ArgumentNullException("poll");

			Poll = poll;

			_options = new Lazy<PollOptionDrop[]>(() => poll.Options.Select(e => new PollOptionDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);

			UserSelectedOption = Poll.UserSelectedOption == null ? null : new PollOptionDrop(this, Poll.UserSelectedOption);

			_pollUrl = new Lazy<string>(GetPollUrl, LazyThreadSafetyMode.None);
			_submitUrl = new Lazy<string>(GetSubmitUrl, LazyThreadSafetyMode.None);
		}

		protected IPoll Poll { get; private set; }

		public string Name
		{
			get { return Poll.Name; }
		}

		public string Question
		{
			get { return Poll.Question; }
		}

		public string SubmitButtonLabel
		{
			get { return Poll.SubmitButtonLabel; }
		}

		public bool HasUserVoted
		{
			get { return Poll.UserSelectedOption != null; }
		}

		public IEnumerable<PollOptionDrop> Options
		{
			get { return _options.Value.AsEnumerable(); }
		}

		public PollOptionDrop UserSelectedOption { get; private set; }

		public int Votes { get { return Poll.Votes; } }

		public string PollUrl
		{
			get { return _pollUrl.Value; }
		}

		public string SubmitUrl
		{
			get { return _submitUrl.Value; }
		}

		private string GetPollUrl()
		{
			return UrlHelper.RouteUrl(PollDataAdapter.PollRoute, new
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
