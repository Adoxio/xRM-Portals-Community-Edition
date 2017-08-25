/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Site.Controls;
using Property = Adxstudio.Xrm.Web.UI.WebControls.Property;

namespace Site.Areas.Events.Controls
{
	public partial class SessionScheduleDetails : PortalUserControl
	{
		public Entity Attendee;
		public object DataSource;
		public bool ShowSessionTitle;

		protected void Page_PreRender(object sender, EventArgs e)
		{
			ScheduleListView.DataSource = DataSource;

			ScheduleListView.DataBind();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Attendee = Contact;
		}

		protected void ScheduleListView_ItemCommand(object sender, ListViewCommandEventArgs e)
		{

			if (e == null || e.CommandArgument == null)
			{
				return;
			}

			var id = new Guid(e.CommandArgument.ToString());

			switch (e.CommandName)
			{
				case "AddToSchedule":
					{
						// add this to the user's schedule
						var sessionSchedule = XrmContext.CreateQuery("adx_eventschedule").First(es => es.GetAttributeValue<Guid>("adx_eventscheduleid") == id);
						var registration = new Entity("adx_eventregistration");
						registration.SetAttributeValue("adx_attendeeid", Attendee.ToEntityReference());
						registration.SetAttributeValue("adx_eventid", sessionSchedule.GetAttributeValue<EntityReference>("adx_eventid"));
						registration.SetAttributeValue("adx_eventscheduleid", sessionSchedule.ToEntityReference());
						registration.SetAttributeValue("adx_registrationdate", DateTime.UtcNow);
						XrmContext.AddObject(registration);
						XrmContext.SaveChanges();
					}
					break;
				case "RemoveFromSchedule":
					{
						// remove this from the user's schedule
						var sessionSchedule = XrmContext.CreateQuery("adx_eventschedule").First(es => es.GetAttributeValue<Guid>("adx_eventscheduleid") == id);
						var eventSchedules = sessionSchedule.GetRelatedEntities(XrmContext, new Relationship("adx_eventschedule_eventregistration")).ToList();
						var registration = eventSchedules.FirstOrDefault(er => er.GetAttributeValue<EntityReference>("adx_attendeeid") == Attendee.ToEntityReference());
						XrmContext.DeleteObject(registration);
						XrmContext.SaveChanges();
					}
					break;
				case "Feedback":
					{
						var feedbackPage = ServiceContext.GetPageBySiteMarkerName(Website, "Event Feedback");

						if (feedbackPage != null)
						{
							var url = new UrlBuilder(ServiceContext.GetUrl(feedbackPage));

							url.QueryString["id"] = id.ToString();

							Response.Redirect(url.PathWithQueryString);
						}
					}
					break;
			}

			// redirect to current page (to avoid re-post issues)
			if (SiteMap.CurrentNode == null) return;
			Response.Redirect(SiteMap.CurrentNode.Url);
		}

		protected Boolean CheckIfScheduledForCurrentUser(Entity schedule, Guid contactId)
		{
			if (schedule == null)
			{
				return false;
			}

			var eventSchedule = ServiceContext.CreateQuery("adx_eventschedule").FirstOrDefault(et => et.GetAttributeValue<Guid>("adx_eventscheduleid") == schedule.GetAttributeValue<Guid>("adx_eventscheduleid"));

			return eventSchedule != null && ServiceContext.CheckIfScheduledForCurrentUser(eventSchedule, contactId);
		}

		protected void ScheduleListView_ItemDataBound(object sender, ListViewItemEventArgs e)
		{
			var dataItem = e.Item as ListViewDataItem;

			if (dataItem == null || dataItem.DataItem == null)
			{
				return;
			}

			var scheduleItem = dataItem.DataItem as Entity;

			if (scheduleItem == null)
			{
				return;
			}

			var schedule = ServiceContext.CreateQuery("adx_eventschedule").FirstOrDefault(es => es.GetAttributeValue<Guid>("adx_eventscheduleid") == scheduleItem.GetAttributeValue<Guid>("adx_eventscheduleid"));

			var scheduleEvent = schedule.GetRelatedEntity(ServiceContext, new Relationship("adx_event_eventschedule"));

			var eventLocation = schedule.GetRelatedEntity(ServiceContext, new Relationship("adx_eventlocation_eventschedule"));

			var sessionEventControl = (CrmEntityDataSource)e.Item.FindControl("SessionEvent");

			var eventNameControl = (Property)e.Item.FindControl("EventName");

			var sessionEventLocationControl = (CrmEntityDataSource)e.Item.FindControl("SessionEventLocation");

			var eventLocationNameControl = (Property)e.Item.FindControl("EventLocationName");

			if (sessionEventControl != null && eventNameControl != null)
			{
				sessionEventControl.DataItem = scheduleEvent;

				eventNameControl.DataBind();
			}

			if (sessionEventLocationControl != null && eventLocationNameControl != null)
			{
				sessionEventLocationControl.DataItem = eventLocation;

				eventLocationNameControl.DataBind();
			}

		}
	}
}
