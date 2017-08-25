/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Provides an iCalendar 2.0 (http://tools.ietf.org/html/rfc5545) representation of a given adx_eventschedule.
	/// </summary>
	public class CalendarHandler : IHttpHandler
	{
		private readonly Guid _eventScheduleId;

		public CalendarHandler(Guid eventScheduleId, string portalName = null)
		{
			_eventScheduleId = eventScheduleId;
			PortalName = portalName;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public string PortalName { get; private set; }

		public void ProcessRequest(HttpContext context)
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			var eventSchedule = serviceContext.CreateQuery("adx_eventschedule")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_eventscheduleid") == _eventScheduleId);

			if (eventSchedule == null || !security.TryAssert(serviceContext, eventSchedule, CrmEntityRight.Read))
			{
				NotFound(context.Response, ResourceManager.GetString("Event_Not_Found"));

				return;
			}

			var @event = eventSchedule.GetRelatedEntity(serviceContext, new Relationship("adx_event_eventschedule"));

			if (@event == null)
			{
				NotFound(context.Response, ResourceManager.GetString("Event_Not_Found"));

				return;
			}

			var vevent = new VEvent
			{
				Uid = "{0}@{1}".FormatWith(eventSchedule.Id, context.Request.Url.Host),
				Start = eventSchedule.GetAttributeValue<DateTime?>("adx_starttime"),
				End = eventSchedule.GetAttributeValue<DateTime?>("adx_endtime"),
				Timestamp = DateTime.UtcNow,
				Summary = @event.GetAttributeValue<string>("adx_name"),
				Description = VCalendar.StripHtml(@event.GetAttributeValue<string>("adx_content")),
				DescriptionHtml = @event.GetAttributeValue<string>("adx_content"),
				Location = @event.GetAttributeValue<string>("adx_locationname"),
				RecurrenceRule = GetRecurrenceRule(eventSchedule),
			};

			var vcalendar = new VCalendar(new[] { vevent });

			context.Response.ContentType = "text/calendar";
			context.Response.Write(vcalendar.ToString());
		}

		// http://tools.ietf.org/html/rfc5545.html#section-3.3.10
		private string GetRecurrenceRule(Entity schedule)
		{
			if (schedule == null) throw new ArgumentNullException("schedule");

			var rrule = string.Empty;

			var interval = schedule.GetAttributeValue<int?>("adx_interval");
			var recurrence = schedule.GetAttributeValue<OptionSetValue>("adx_recurrence") == null ? 1 : schedule.GetAttributeValue<OptionSetValue>("adx_recurrence").Value; // 1=Nonrecurring; 2=Daily; 3=Weekly; 4=Monthly;
			var maxRecurrences = schedule.GetAttributeValue<int?>("adx_maxrecurrences");
			var week = schedule.GetAttributeValue<OptionSetValue>("adx_week") == null ? null : (int?)schedule.GetAttributeValue<OptionSetValue>("adx_week").Value; // 1=First;2=Second;3=Third;4=Fourth;5=Last;
			var recurrenceEndDate = schedule.GetAttributeValue<DateTime?>("adx_recurrenceenddate");
			var sunday = schedule.GetAttributeValue<bool?>("adx_sunday");
			var monday = schedule.GetAttributeValue<bool?>("adx_monday");
			var tuesday = schedule.GetAttributeValue<bool?>("adx_tuesday");
			var wednesday = schedule.GetAttributeValue<bool?>("adx_wednesday");
			var thursday = schedule.GetAttributeValue<bool?>("adx_thursday");
			var friday = schedule.GetAttributeValue<bool?>("adx_friday");
			var saturday = schedule.GetAttributeValue<bool?>("adx_saturday");

			switch (recurrence)
			{
				case 1: //Nonrecurring
					return null;
				case 2: //Daily
					rrule += "FREQ=DAILY;";
					break;
				case 3: //Weekly
					rrule += "FREQ=WEEKLY;";
					break;
				case 4: //Monthly
					rrule += "FREQ=MONTHLY;";
					break;
				case 5: //Yearly
					rrule += "FREQ=YEARLY;";
					break;
			}

			if (interval != null)
			{
				rrule += string.Format("INTERVAL={0};", interval);
			}

			if (maxRecurrences != null)
			{
				rrule += string.Format("COUNT={0};", maxRecurrences);
			}

			var byday = "BYDAY=";

			if (sunday != null && sunday.GetValueOrDefault())
			{
				byday += "SU,";
			}

			if (monday != null && monday.GetValueOrDefault())
			{
				byday += "MO,";
			}

			if (tuesday != null && tuesday.GetValueOrDefault())
			{
				byday += "TU,";
			}

			if (wednesday != null && wednesday.GetValueOrDefault())
			{
				byday += "WE,";
			}

			if (thursday != null && thursday.GetValueOrDefault())
			{
				byday += "TH,";
			}

			if (friday != null && friday.GetValueOrDefault())
			{
				byday += "FR,";
			}

			if (saturday != null && saturday.GetValueOrDefault())
			{
				byday += "SA,";
			}

			if (byday.Substring(byday.Length - 1) == ",")
			{
				byday = byday.TrimEnd(',');
			}

			if (byday != "BYDAY=")
			{
				rrule += string.Format("{0};", byday);
			}

			switch (week)
			{
				case null:
					break;
				case 1:
					rrule += string.Format("BYSETPOS={0};", week);
					break;
				case 2:
					rrule += string.Format("BYSETPOS={0};", week);
					break;
				case 3:
					rrule += string.Format("BYSETPOS={0};", week);
					break;
				case 4:
					rrule += string.Format("BYSETPOS={0};", week);
					break;
				case 5: //last
					rrule += string.Format("BYSETPOS={0};", -1);
					break;
			}

			if (maxRecurrences == null && recurrenceEndDate.HasValue)
			{
				// The UNTIL or COUNT rule parts are OPTIONAL but they MUST NOT occur in the same 'recur'.
				rrule += string.Format(CultureInfo.InvariantCulture, "UNTIL={0:yyyyMMddTHHmmssZ}", recurrenceEndDate);
			}

			return rrule;
		}

		private static void NotFound(HttpResponse response, string message)
		{
			response.StatusCode = 404;
			response.ContentType = "text/plain";
			response.Write(message);
			response.End();
		}
	}
}
