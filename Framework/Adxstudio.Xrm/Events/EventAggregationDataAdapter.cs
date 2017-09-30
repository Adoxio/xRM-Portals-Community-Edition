/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Abstract base class that supports querying of <see cref="EventOccurrence">occurrences</see> of
	/// a given set of events.
	/// </summary>
	public class EventAggregationDataAdapter : IEventAggregationDataAdapter
	{
		protected EventAggregationDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		/// <summary>
		/// Gets all <see cref="EventOccurrence">event occurrences</see> within a given date/time range.
		/// </summary>
		public IEnumerable<IEventOccurrence> SelectEventOccurrences(DateTime min, DateTime max)
		{
			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			return SelectEvents()
				.SelectMany(e => e.GetRelatedEntities(serviceContext, "adx_event_eventschedule")
					.Where(es => security.TryAssert(serviceContext, es, CrmEntityRight.Read))
					.Select(es => new { Event = e, EventSchedule = es }))
				.SelectMany(e => serviceContext.GetDates(e.EventSchedule, min, max)
					.Select(d => new { e.Event, e.EventSchedule, Start = d }))
				.Select(e => new EventOccurrence(
					e.Event,
					e.EventSchedule,
					e.Start,
					GetEventOccurrenceUrl(serviceContext, e.Event, e.EventSchedule, e.Start),
					GetEventOccurrenceLocation(e.Event)))
				.OrderBy(e => e.Start)
				.ToArray();
		}

		/// <summary>
		/// Tries to match a specific <see cref="EventOccurrence">event occurrence</see> from a set of them, using
		/// querystring parameters on an <see cref="HttpRequest"/>. The parameters are "schedule", which is the
		/// <see cref="Guid"/> ID of an adx_eventschedule, and "start", which is the start <see cref="DateTime"/>
		/// of the occurrence.
		/// </summary>
		public bool TryMatchRequestEventOccurrence(HttpRequest request, IEnumerable<IEventOccurrence> occurrences, out IEventOccurrence requestOccurrence)
		{
			if (request == null) throw new ArgumentNullException("request");
			if (occurrences == null) throw new ArgumentNullException("occurrences");

			requestOccurrence = null;

			Guid schedule;
			
			if (!Guid.TryParse(request["schedule"], out schedule))
			{
				return false;
			}

			DateTime start;

			if (!DateTime.TryParseExact(request["start"], "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out start))
			{
				return false;
			}

			requestOccurrence = occurrences.FirstOrDefault(e => e.EventSchedule.Id == schedule && e.Start == start);

			return requestOccurrence != null;
		}

		public virtual IEnumerable<Entity> SelectEvents()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			return serviceContext.CreateQuery("adx_event")
				.ToArray()
				.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
				.ToArray();
		}

		private string GetEventOccurrenceUrl(OrganizationServiceContext serviceContext, Entity @event, Entity eventSchedule, DateTime start)
		{
			var urlProvider = Dependencies.GetUrlProvider();

			var path = urlProvider.GetUrl(serviceContext, @event);

			if (path == null)
			{
				return null;
			}

			var url = new UrlBuilder(path);

			if (eventSchedule != null)
			{
				url.QueryString["schedule"] = eventSchedule.Id.ToString();
				url.QueryString["start"] = start.ToString("o", CultureInfo.InvariantCulture);
			}

			return url.PathWithQueryString;
		}

		private static string GetEventOccurrenceLocation(Entity @event)
		{
			return @event.GetAttributeValue<string>("adx_locationname");
		}
	}
}
