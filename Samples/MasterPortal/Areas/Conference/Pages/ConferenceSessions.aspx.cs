/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Conferences;

namespace Site.Areas.Conference.Pages
{
	public partial class ConferenceSessions : ConferencePage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			var dataAdapter = new ConferenceEventDataAdapter(new PortalContextDataAdapterDependencies(Portal, PortalName), PortalConference);

			var occurrences = dataAdapter.SelectEventOccurrences(
				PortalConference.GetAttributeValue<DateTime?>("adx_startingdate").GetValueOrDefault(DateTime.MinValue),
				PortalConference.GetAttributeValue<DateTime?>("adx_enddate").GetValueOrDefault(DateTime.MaxValue)).ToArray();

			UpcomingEvents.DataSource = occurrences
				.OrderBy(e => e.Start);

			UpcomingEvents.DataBind();
		}
	}
}
