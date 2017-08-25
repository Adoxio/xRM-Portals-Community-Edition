/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client.Security;

namespace Adxstudio.Xrm.Events
{
	public static class OrganizationServiceContextExtensions
	{
		public static IEnumerable<Entity> GetEventTags(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_eventtag").ToList();
		}

		public static Entity GetEvent(this OrganizationServiceContext context, Guid eventID)
		{
			var findEvent =
				from c in context.CreateQuery("adx_event")
				where c.GetAttributeValue<Guid>("adx_eventid") == eventID
				select c;

			return findEvent.FirstOrDefault();
		}

		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IEnumerable<Entity> GetPublishedEvents(this OrganizationServiceContext context)
		{
			return GetPublishedEvents(context, PortalContext.Current.Website);
		}

		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IEnumerable<Entity> GetPublishedEvents(this OrganizationServiceContext context, Entity site, string portalName = null)
		{
			site.AssertEntityName("adx_website");

			var findEvents =
				from e in context.CreateQuery("adx_event")
				where e.GetAttributeValue<EntityReference>("adx_websiteid") == site.ToEntityReference()
				select e;

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

			return findEvents.ToList().Where(e => securityProvider.TryAssert(context, e, CrmEntityRight.Read));
		}

		private static Entity GetEventTagByName(this OrganizationServiceContext context, string tagName)
		{
			return context.CreateQuery("adx_eventtag").ToList().Where(et => TagName.Equals(et.GetAttributeValue<string>("adx_name"), tagName)).FirstOrDefault();
		}

		/// <summary>
		/// Adds a event tag association by name to a event.
		/// </summary>
		/// <param name="taggableEventId">The ID of the event whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be associated with the event (will be created if necessary).
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void AddTagToEventAndSave(this OrganizationServiceContext context, Guid taggableEventId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (taggableEventId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "taggableEventId");
			}

			var taggableEvent = context.CreateQuery("adx_event").Single(e => e.GetAttributeValue<Guid>("adx_eventid") == taggableEventId);

			var tag = GetEventTagByName(context, tagName);

			// If the tag doesn't exist, create it
			if (tag == null)
			{
				tag = new Entity("adx_eventtag");
				tag["adx_name"] = tagName;

				context.AddObject(tag);
				context.SaveChanges();
				context.ReAttach(taggableEvent);
				context.ReAttach(tag);
			}

			if (!taggableEvent.GetRelatedEntities(context, "adx_eventtag_event").Any(t => t.GetAttributeValue<Guid>("adx_eventtagid") == tag.Id))
			{
				context.AddLink(taggableEvent, new Relationship("adx_eventtag_event"), tag);

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Removes a event tag association by name from a event.
		/// </summary>
		/// <param name="taggableEventId">The ID of the event whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated with from the event.
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void RemoveTagFromEventAndSave(this OrganizationServiceContext context, Guid taggableEventId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (taggableEventId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "taggableEventId");
			}

			var taggableEvent = context.CreateQuery("adx_event").Single(e => e.GetAttributeValue<Guid>("adx_eventid") == taggableEventId);

			var tag = GetEventTagByName(context, tagName);

			// If the tag doesn't exist, do nothing
			if (tag == null)
			{
				return;
			}

			context.DeleteLink(taggableEvent, new Relationship("adx_eventtag_event"), tag);
			context.SaveChanges();
		}

		public static IEnumerable<DateTime> GetScheduledDates(this OrganizationServiceContext context, Entity evnt, TimeSpan duration)
		{
			return GetScheduledDates(context, evnt, DateTime.UtcNow, DateTime.UtcNow.Add(duration));
		}

		public static IEnumerable<DateTime> GetScheduledDates(this OrganizationServiceContext context, Entity evnt, DateTime firstDate, DateTime lastDate)
		{
			evnt.AssertEntityName("adx_event");

			var schedules = new List<DateTime>();

			var eventSchedulesSet = evnt.GetRelatedEntities(context, "adx_event_eventschedule");

			var eventSchedules = (
				from es in eventSchedulesSet
				//where es.PublishingState.IsVisible != null && es.PublishingState.IsVisible.Value 
				select es).ToList();

			foreach (var schedule in eventSchedules)
			{
				var publishingState = schedule.GetRelatedEntity(context, "adx_publishingstate_eventschedule");

				if (publishingState != null && publishingState.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault())
				{
					var s = (
						from date in context.GetDates(schedule, firstDate, lastDate)
						orderby date
						select date).ToList();

					schedules.AddRange(s);
				}
			}

			return schedules;
		}

		public static int GetTaggedEventsCount(this OrganizationServiceContext context, Entity eventTag)
		{
			eventTag.AssertEntityName("adx_eventtag");

			var events = eventTag.GetRelatedEntities(context, "adx_eventtag_event");

			return events.Count();
		}

		/// <summary>
		/// Checks if the current user (contact) has scheduled this event
		/// </summary>
		/// <returns></returns>
		public static bool CheckIfScheduledForCurrentUser(this OrganizationServiceContext context, Entity schedule, Guid contactId)
		{
			schedule.AssertEntityName("adx_eventschedule");

			var registrations = schedule.GetRelatedEntities(context, "adx_eventschedule_eventregistration");

			var registration = (
				from er in registrations
				where er.GetAttributeValue<EntityReference>("adx_attendeeid") == new EntityReference("contact", contactId)
				select er).FirstOrDefault();

			return registration != null;
		}

		/// <summary>
		/// Retrieves a list of upcoming dates that schedule is applicable for.
		/// </summary>
		/// <param name="eventSchedule">schedule of the event.</param>
		/// <param name="timespan">Timespan for dates to include.</param>
		/// <returns>List of dates.</returns>
		public static IEnumerable<DateTime> GetDates(this OrganizationServiceContext context, Entity eventSchedule, TimeSpan timespan)
		{
			return GetDates(context, eventSchedule, DateTime.UtcNow, DateTime.UtcNow.Add(timespan));
		}

		/// <summary>
		/// Retrieves a list of upcoming dates that schedule is applicable for
		/// </summary>
		/// <param name="eventSchedule">schedule of the event.</param>
		/// <param name="date">Date to test.</param>
		/// <returns></returns>
		public static IEnumerable<DateTime> GetDates(this OrganizationServiceContext context, Entity eventSchedule, DateTime date)
		{
			return GetDates(context, eventSchedule, date, date.AddDays(1));
		}

		/// <summary>
		/// Retrieves a list of dates that schedule is applicable for between two dates.
		/// </summary>
		/// <param name="eventSchedule">schedule of the event.</param>
		/// <param name="firstDate">First date to test.</param>
		/// <param name="lastDate">Last date to test.</param>
		/// <returns>List of dates.</returns>
		public static IEnumerable<DateTime> GetDates(this OrganizationServiceContext context, Entity eventSchedule, DateTime firstDate, DateTime lastDate)
		{
			eventSchedule.AssertEntityName("adx_eventschedule");

			var interval = eventSchedule.GetAttributeValue<int?>("adx_interval");
			var recurrenceOption = eventSchedule.GetAttributeValue<OptionSetValue>("adx_recurrence");
			var recurrence = recurrenceOption == null ? null : (int?)recurrenceOption.Value;
			var startTime = eventSchedule.GetAttributeValue<DateTime?>("adx_starttime");
			var endTime = eventSchedule.GetAttributeValue<DateTime?>("adx_endtime");
			var maxRecurrences = eventSchedule.GetAttributeValue<int?>("adx_maxrecurrences");
			var weekOption = eventSchedule.GetAttributeValue<OptionSetValue>("adx_week");
			var week = weekOption == null ? null : (int?)weekOption.Value;
			var recurrenceEndDate = eventSchedule.GetAttributeValue<DateTime?>("adx_recurrenceenddate");

			var scheduledDates = new List<DateTime>();
			var intervalValue = interval.HasValue ? interval.Value : 1;

			if (recurrence == 1 /*"Nonrecurring"*/)
			{
				// this is a nonrecurring event.  We only need to add the current date
				if (endTime >= firstDate && startTime < lastDate)
				{
					scheduledDates.Add(startTime.Value);
				}
			}
			else if (recurrence == 2 /*"Daily"*/)
			{
				DateTime d = startTime.Value;
				var done = false;
				var counter = 0;

				while (!done)
				{
					// check if the time for this event is between now and the maximum timespan
					if (d >= firstDate && d < lastDate)
					{
						var sunday = eventSchedule.GetAttributeValue<bool?>("adx_sunday");
						var monday = eventSchedule.GetAttributeValue<bool?>("adx_monday");
						var tuesday = eventSchedule.GetAttributeValue<bool?>("adx_tuesday");
						var wednesday = eventSchedule.GetAttributeValue<bool?>("adx_wednesday");
						var thursday = eventSchedule.GetAttributeValue<bool?>("adx_thursday");
						var friday = eventSchedule.GetAttributeValue<bool?>("adx_friday");
						var saturday = eventSchedule.GetAttributeValue<bool?>("adx_saturday");

						// this is in our date window.  Check if there are restrictions on the days
						if (
							(d.DayOfWeek == DayOfWeek.Sunday && (sunday != null && sunday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Monday && (monday != null && monday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Tuesday && (tuesday != null && tuesday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Wednesday && (wednesday != null && wednesday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Thursday && (thursday != null && thursday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Friday && (friday != null && friday.Value)) ||
							(d.DayOfWeek == DayOfWeek.Saturday && (saturday != null && saturday.Value)))
						{
							scheduledDates.Add(d);
						}
					}

					// move to the next event
					d = d.AddDays(intervalValue);

					if ((maxRecurrences.HasValue ? counter++ >= maxRecurrences : false) || d > lastDate || d > recurrenceEndDate)
					{
						done = true;
					}
				}
			}
			else if (recurrence == 3 /*"Weekly"*/)
			{
				DateTime d = startTime.Value;
				var done = false;
				var counter = 0;

				while (!done)
				{
					// check if the time for this event is between now and the maximum timespan
					if (d >= firstDate && d < lastDate)
					{
						// this is in our date window. 
						scheduledDates.Add(d);
					}

					// move to the next event
					d = d.AddDays(intervalValue * 7);

					if ((maxRecurrences.HasValue ? counter++ >= maxRecurrences : false) || d > lastDate || d > recurrenceEndDate)
					{
						done = true;
					}
				}
			}
			else if (recurrence == 4 /*"Monthly"*/ && week == null)
			{
				DateTime d = startTime.Value;
				var done = false;
				var counter = 0;

				while (!done)
				{
					// check if the time for this event is between now and the maximum timespan
					if (d >= firstDate && d < lastDate)
					{
						// this is in our date window. 
						scheduledDates.Add(d);
					}

					// move to the next event
					d = d.AddMonths(intervalValue);

					if ((maxRecurrences.HasValue ? counter++ >= maxRecurrences : false) || d > lastDate || d > recurrenceEndDate)
					{
						done = true;
					}
				}
			}
			else if (recurrence == 4 /*"Monthly"*/ && week != null)
			{
				DateTime d = startTime.Value;
				var done = false;
				var counter = 0;

				while (!done)
				{
					var d2 = new DateTime(d.Year, d.Month, 1, d.Hour, d.Minute, d.Second);
					if (week == 2 /*"Second"*/)
					{
						d2 = d2.AddDays(7);
					}
					else if (week == 3 /*"Third"*/)
					{
						d2 = d2.AddDays(14);
					}
					else if (week == 4 /*"Fourth"*/)
					{
						d2 = d2.AddDays(21);
					}
					else if (week == 5 /*"Last"*/)
					{
						d2 = d2.AddMonths(1);
						d2 = d2.AddDays(-7);
					}

					// move forward to the first scheduled day that matches the original day
					while (d2.DayOfWeek != startTime.Value.DayOfWeek)
					{
						d2 = d2.AddDays(1);
					}

					// check if the time for this event is between now and the maximum timespan
					if (d2 >= firstDate && d2 < lastDate)
					{
						// this is in our date window. 
						scheduledDates.Add(d2);
					}

					// move to the next event
					d = d.AddMonths(intervalValue);

					// check if we are done, but be careful that we might have to check another iteration if the date is
					// a little past the specified date and the setting is set to use the 'last' week of the month. It is safe to 
					// check an extra week past our final date.
					if ((maxRecurrences.HasValue ? counter++ >= maxRecurrences : false) || d > lastDate.AddDays(7) || d > recurrenceEndDate)
					{
						done = true;
					}
				}
			}

			var eventScheduleExceptions = eventSchedule.GetRelatedEntities(context, "adx_eventschedule_eventscheduleexception");

			foreach (var scheduleException in eventScheduleExceptions)
			{
				var scheduledTime = scheduleException.GetAttributeValue<DateTime?>("adx_scheduledtime");

				// check if this scheduled date was in the list of dates we calculated, and remove it if it is
				if (scheduledDates.Contains(scheduledTime.Value))
				{
					scheduledDates.Remove(scheduledTime.Value);
				}

				var rebookingTime = scheduleException.GetAttributeValue<DateTime?>("adx_rebookingtime");

				// check if this rescheduled date fits within our time window
				if (rebookingTime.HasValue && rebookingTime.Value >= firstDate && rebookingTime.Value < lastDate)
				{
					scheduledDates.Add(rebookingTime.Value);
				}
			}

			return scheduledDates;
		}
	}
}
