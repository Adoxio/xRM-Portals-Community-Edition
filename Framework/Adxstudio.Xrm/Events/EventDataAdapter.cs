/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Aggregates all <see cref="EventOccurrence">occurrences</see> of a single adx_event.
	/// </summary>
	public class EventDataAdapter : EventAggregationDataAdapter, IEventDataAdapter
	{
		private readonly Entity _event;

		public EventDataAdapter(IDataAdapterDependencies dependencies)
			: base(dependencies)
		{
		
		}

		public EventDataAdapter(Entity @event, IDataAdapterDependencies dependencies) : base(dependencies)
		{
			if (@event == null) throw new ArgumentNullException("event");

			Microsoft.Xrm.Client.EntityExtensions.AssertEntityName(@event, "adx_event");

			_event = @event;
		}

		//implements the SelectEvents() method to rturn a list containing only the current event
		public override IEnumerable<Entity> SelectEvents()
		{
			return new[] { _event };
		}

		public IEvent Select(Guid eventId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", eventId));

			var myevent = Select(e => e.GetAttributeValue<Guid>("adx_eventid") == eventId);

			if (myevent == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", eventId));

			return myevent;
		}

		public IEvent Select(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return null;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Start");

			var @event = Select(e => e.GetAttributeValue<string>("adx_name") == eventName);

			if (@event == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Not Found");
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return @event;
		}

		protected virtual IEvent Select(Predicate<Entity> match)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			var publishingStateAccessProvider = new PublishingStateAccessProvider(Dependencies.GetRequestContext().HttpContext);

			// Bulk-load all ad entities into cache.
			var allEntities = serviceContext.CreateQuery("adx_event")
				.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == website)
				.ToArray();

			var entity = allEntities.FirstOrDefault(e =>
				match(e)
				&& IsActive(e)
				&& publishingStateAccessProvider.TryAssert(serviceContext, e));

			if (entity == null)
			{
				return null;
			}

			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Forum={0}: Not Found", entity.Id));

				return null;
			}

			var myevent = new Event(entity);

			return myevent;
		}

		public IEnumerable<IEventSpeaker> SelectSpeakers()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var speakers = from speaker in serviceContext.CreateQuery("adx_eventspeaker")
							join speakerEvent in serviceContext.CreateQuery("adx_eventspeaker_event") on
								speaker.GetAttributeValue<Guid>("adx_eventspeakerid") equals
								speakerEvent.GetAttributeValue<EntityReference>("adx_eventspeakerid").Id
							join e in serviceContext.CreateQuery("adx_event") on
								speakerEvent.GetAttributeValue<EntityReference>("adx_eventid").Id equals
								e.GetAttributeValue<Guid>("adx_eventid")
							where e.GetAttributeValue<Guid>("adx_eventid") == _event.Id
							select speaker;

			return speakers.ToArray().Select(e => new EventSpeaker(e));
		}

		public IEnumerable<IEventSponsor> SelectSponsors()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var sponsors = from sponsor in serviceContext.CreateQuery("adx_eventsponsor")
						   join sponsorEvent in serviceContext.CreateQuery("adx_eventsponsor_event") on
							   sponsor.GetAttributeValue<Guid>("adx_eventsponsorid") equals
							   sponsorEvent.GetAttributeValue<EntityReference>("adx_eventsponsorid").Id
						   join e in serviceContext.CreateQuery("adx_event") on
							   sponsorEvent.GetAttributeValue<EntityReference>("adx_eventid").Id equals
							   e.GetAttributeValue<Guid>("adx_eventid")
						   where e.GetAttributeValue<Guid>("adx_eventid") == _event.Id
						   select sponsor;

			return sponsors.ToArray().Select(e => new EventSponsor(e));
		}

		public IEnumerable<IEventSchedule> SelectSchedules()
		{
			var serviceContext = Dependencies.GetServiceContext();

			var schedules = serviceContext.CreateQuery("adx_eventschedule")
				.Where(es => es.GetAttributeValue<EntityReference>("adx_eventid").Id == _event.Id)
				.ToArray();

			var aSchedules = schedules.Select(es => new EventSchedule(_event, es))
							.OrderBy(es => es.StartTime)
							.ToArray();

			return aSchedules;
		}

		private static bool IsActive(Entity entity)
		{
			if (entity == null)
			{
				return false;
			}

			var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");

			return statecode != null && statecode.Value == 0;
		}
	}
}
