/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Events.Pages
{
	public partial class EventFeedback : PortalPage
	{
		protected Guid AttendeeContactID { get { return Contact.GetAttributeValue<Guid>("contactid"); } }

		private Guid? _sessionEventScheduleID;

		protected Guid? SessionEventScheduleID
		{
			get
			{
				if (_sessionEventScheduleID != null) return _sessionEventScheduleID;

				try
				{
					Guid id;

					if (!Guid.TryParse(Request.QueryString["id"], out id))
					{
						return null;
					}

					var sched = XrmContext.CreateQuery("adx_eventschedule").FirstOrDefault(es => es.GetAttributeValue<Guid>("adx_eventscheduleid") == id);

					if (sched != null)
					{
						_sessionEventScheduleID = sched.GetAttributeValue<Guid>("adx_eventscheduleid");
					}
					else
					{
						return null;
					}
				}
				catch
				{
					//SessionEventScheduleID is not a valid Guid and will cause the LINQDataSource Query to fail
					Response.Redirect(Request.RawUrl.Remove(Request.RawUrl.IndexOf("?", StringComparison.Ordinal)));

					return null;
				}
				return _sessionEventScheduleID;
			}
			set
			{
				if (value != null)
				{
					try
					{
						_sessionEventScheduleID = value;
					}
					catch
					{
						throw new InvalidCastException();
					}
				}
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Request.IsAuthenticated)
			{
				var returnUrl = string.Format("{0}?id={1}", ServiceContext.GetUrl(Entity), Request.QueryString["id"]);

				Response.Redirect(string.Format("~/login?ReturnUrl={0}", HttpUtility.UrlEncode(returnUrl)));

				return;
			}
			if (SessionEventScheduleID == null)
			{
				SessionScheduleNotFoundMessage.Visible = true;
				FeedbackFormPanel.Visible = false;
			}

			var schedule = XrmContext.CreateQuery("adx_eventschedule").FirstOrDefault(en => en.GetAttributeValue<Guid>("adx_eventscheduleid") == SessionEventScheduleID);

			EventName.Text = GetEventNameFromSchedule(XrmContext, schedule);

			if (schedule != null)
			{
				EventDate.Text = String.Format("{0:r}", schedule.GetAttributeValue<DateTime>("adx_starttime"));
			}
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var schedule = XrmContext.CreateQuery("adx_eventschedule").FirstOrDefault(sn => sn.GetAttributeValue<Guid>("adx_eventscheduleid") == SessionEventScheduleID);

			e.Values["adx_name"] = GetEventNameFromSchedule(XrmContext, schedule) + " session feedback from " + Contact.GetAttributeValue<string>("fullname");
			e.Values["adx_attendeeid"] = Contact.ToEntityReference();
			if (schedule != null)
			{
				e.Values["adx_eventscheduleid"] = schedule.ToEntityReference();
			}
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			ThankYouPanel.Visible = true;
			FeedbackFormPanel.Visible = false;
		}

		protected string GetEventNameFromSchedule(OrganizationServiceContext context, Entity schedule)
		{
			if (schedule == null)
			{
				return "";
			}

			var session = schedule.GetRelatedEntity(context, new Relationship("adx_event_eventschedule"));

			return session == null ? "" : session.GetAttributeValue<string>("adx_name");
		}
	}
}
