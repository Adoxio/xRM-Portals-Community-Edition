/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EventScheduleDrop : EntityDrop
	{
		private IDataAdapterDependencies _dependencies;

		public EventScheduleDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IEventSchedule eventSchedule)
			: base(portalLiquidContext, eventSchedule.Entity)
		{
			Schedule = eventSchedule;

			_dependencies = dependencies;
		}

		protected IEventSchedule Schedule { get; private set; }

		public string Name
		{
			get { return Schedule.Name; }
		}

		[Obsolete]
		public EventOccurrencesDrop Occurrences
		{
			get { return EventsEnabled ? new EventOccurrencesDrop(this, _dependencies, new Event(Schedule.Event), false, false) : null; }
		}

		public DateTime End
		{
			get { return Schedule.EndTime; }
		}

		[Obsolete]
		public EventDrop Event
		{
			get
			{
				return new EventDrop(this, _dependencies, new Event(Schedule.Event));
			}
		}

		public bool IsAllDayEvent { get { return Schedule.IsAllDayEvent; } }

		public DateTime Start
		{
			get { return Schedule.StartTime; }
		}

		public DateTime StartTime
		{
			get { return Schedule.StartTime; }
		}

		public DateTime EndTime
		{
			get { return Schedule.EndTime; }
		}
	}
}
