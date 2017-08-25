/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Events.Pages
{
	public partial class MyEventSchedule : PortalPage
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (IsPostBack || Contact == null)
			{
				return;
			}

			var query = from eventSchedule in XrmContext.CreateQuery("adx_eventschedule")
				join @event in XrmContext.CreateQuery("adx_event") on eventSchedule.GetAttributeValue<EntityReference>("adx_eventid").Id equals @event.GetAttributeValue<Guid>("adx_eventid")
				join eventRegistration in XrmContext.CreateQuery("adx_eventregistration") on @event.GetAttributeValue<EntityReference>("adx_eventid").Id equals eventRegistration.GetAttributeValue<EntityReference>("adx_eventid").Id
				where @event.GetAttributeValue<EntityReference>("adx_websiteid") == Website.ToEntityReference()
				where eventSchedule.GetAttributeValue<EntityReference>("adx_eventid") != null
				where eventRegistration.GetAttributeValue<EntityReference>("adx_eventid") != null && eventRegistration.GetAttributeValue<EntityReference>("adx_attendeeid") == Contact.ToEntityReference()
				select eventSchedule;

			var eventSchedules = query.ToArray();

			SessionSchedule.DataSource = eventSchedules;
			EmptyPanel.Visible = !eventSchedules.Any();
		}
	}
}
