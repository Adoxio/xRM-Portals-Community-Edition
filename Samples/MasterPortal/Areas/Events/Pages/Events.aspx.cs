/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Web.Mvc.Html;
using Site.Pages;

namespace Site.Areas.Events.Pages
{
	public partial class Events : PortalPage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			var dataAdapter = new WebsiteEventDataAdapter(new PortalContextDataAdapterDependencies(Portal, PortalName));
			var now = DateTime.UtcNow;

			var past = Html.TimeSpanSetting("Events/DisplayTimeSpan/Past").GetValueOrDefault(TimeSpan.FromDays(90));
			var future = Html.TimeSpanSetting("Events/DisplayTimeSpan/Future").GetValueOrDefault(TimeSpan.FromDays(90));

			var occurrences = dataAdapter.SelectEventOccurrences(now.Subtract(past), now.Add(future)).ToArray();

			UpcomingEvents.DataSource = occurrences.Where(e => e.Start >= now).OrderBy(e => e.Start);
			UpcomingEvents.DataBind();

			PastEvents.DataSource = occurrences.Where(e => e.Start < now).OrderByDescending(e => e.Start);
			PastEvents.DataBind();
		}
	}
}
