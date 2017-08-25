/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EventDrop : EntityDrop
	{
		private readonly IDataAdapterDependencies _dependencies;

		private readonly Lazy<SpeakerDrop[]> _speakers;

		private readonly Lazy<SponsorDrop[]> _sponsors;

		private readonly Lazy<EventScheduleDrop[]> _schedules;

		private IEventDataAdapter _eventDataAdapter;

		public EventDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IEvent oEvent)
			: base(portalLiquidContext, oEvent.Entity)
		{
			Event = oEvent;
			_dependencies = dependencies;

			_eventDataAdapter = new EventDataAdapter(Event.Entity, dependencies);

			_speakers = new Lazy<SpeakerDrop[]>(() => _eventDataAdapter.SelectSpeakers().Select(e => new SpeakerDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);

			_sponsors = new Lazy<SponsorDrop[]>(() => _eventDataAdapter.SelectSponsors().Select(e => new SponsorDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);

			_schedules = new Lazy<EventScheduleDrop[]>(() => _eventDataAdapter.SelectSchedules().Select(e => new EventScheduleDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		[Obsolete]
		protected IEvent Event { get; private set; }

		public string Name
		{
			get { return Event.Name; }
		}

		public EventOccurrencesDrop Occurrences
		{
			get { return EventsEnabled ? new EventOccurrencesDrop(this, _dependencies, Event, false, false) : null; }
		}

		public IEnumerable<SpeakerDrop> Speakers
		{
			get { return _speakers.Value.AsEnumerable(); }
		}

		public IEnumerable<SponsorDrop> Sponsors
		{
			get { return _sponsors.Value.AsEnumerable(); }
		}

		public IEnumerable<EventScheduleDrop> Schedules
		{
			get { return _schedules.Value.AsEnumerable(); }
		}
	}
}
