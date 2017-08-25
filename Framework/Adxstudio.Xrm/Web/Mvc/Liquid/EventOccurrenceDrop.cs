/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EventOccurrenceDrop : PortalDrop  //PortalUrlDrop
	{
		private IDataAdapterDependencies _dependencies;

		public EventOccurrenceDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IEventOccurrence occurrence)
			: base(portalLiquidContext)
		{
			Occurrence = occurrence;
			_dependencies = dependencies;
		}

		protected IEventOccurrence Occurrence { get; private set; }

		public DateTime End
		{
			get { return Occurrence.End; }
		}

		[Obsolete]
		public EventDrop Event
		{
			get
			{
				return new EventDrop(this, _dependencies, new Event(Occurrence.Event));
			}
		}

		//Entity EventSchedule { get; }

		public bool IsAllDayEvent { get { return Occurrence.IsAllDayEvent; } }

		public string Location { get { return Occurrence.Location; } }

		public DateTime Start
		{
			get { return Occurrence.Start; }
		}

		public string Url
		{
			get { return Occurrence.Url; }
		}

		public DateTime StartTime
		{
			get { return Occurrence.Start; }
		}

		public DateTime EndTime
		{
			get { return Occurrence.End; }
		}


	}
}
