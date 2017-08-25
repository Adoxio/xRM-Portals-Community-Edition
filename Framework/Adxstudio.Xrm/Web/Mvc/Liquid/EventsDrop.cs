/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	[Obsolete]
	public class EventsDrop : PortalDrop
	{
		private readonly IEventDataAdapter _adapter;

		private IDataAdapterDependencies _dependencies;

		public EventsDrop(IPortalLiquidContext portalLiquidContext, Adxstudio.Xrm.Cms.IDataAdapterDependencies dependencies)
			: base(portalLiquidContext)
		{
			if (dependencies == null) throw new ArgumentException("dependencies");

			_dependencies = dependencies;

			var eventDataAdapter = new EventDataAdapter(dependencies);

			_adapter = eventDataAdapter;
		}

		public override object BeforeMethod(string method)
		{
			if (method == null || !EventsEnabled)
			{
				return null;
			}

			Guid parsed;

			// If the method can be parsed as a Guid, look up the set by that.
			if (Guid.TryParse(method, out parsed))
			{
				var eventById = _adapter.Select(parsed);

				return eventById == null ? null : new EventDrop(this, _dependencies, eventById);
			}

			var eventByName = _adapter.Select(method);

			return eventByName == null ? null : new EventDrop(this, _dependencies, eventByName);
		}

		public EventOccurrencesDrop Occurrences
		{
			get { return EventsEnabled ? new EventOccurrencesDrop(this, _dependencies, false, false) : null; }
		}
	}
}
