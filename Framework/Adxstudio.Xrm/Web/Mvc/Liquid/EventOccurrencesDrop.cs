/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EventOccurrencesDrop : PortalDrop
	{
		private bool _isMin;
		private bool _isMax;
		private DateTime? _min;
		private DateTime? _max;

		private Lazy<EventOccurrenceDrop[]> _occurences;

		private readonly IEventAggregationDataAdapter _adapter;

		//private IPortalLiquidContext _context;

		private IDataAdapterDependencies _dependencies;

		public EventOccurrencesDrop(IPortalLiquidContext portalLiquidContext, 
									IDataAdapterDependencies dependencies, 
									bool isMin, 
									bool isMax, 
									DateTime? min = null, 
									DateTime? max = null)
									: base(portalLiquidContext)
		{
			var eventAggregationDataAdapter = new WebsiteEventDataAdapter(dependencies);

			if (eventAggregationDataAdapter == null) throw new ArgumentNullException("dependencies");

			_adapter = eventAggregationDataAdapter;

			SetParams(dependencies, isMin, isMax, min, max);

		}

		public EventOccurrencesDrop(IPortalLiquidContext portalLiquidContext,
									IDataAdapterDependencies dependencies,
									IEvent oEvent,
									bool isMin,
									bool isMax,
									DateTime? min = null,
									DateTime? max = null)
			: base(portalLiquidContext)
		{
			Event = oEvent;

			var eventAggregationDataAdapter = new EventDataAdapter(dependencies);

			if (eventAggregationDataAdapter == null) throw new ArgumentNullException("dependencies");

			_adapter = eventAggregationDataAdapter;

			SetParams(dependencies, isMin, isMax, min, max);
		}

		private void SetParams(IDataAdapterDependencies dependencies, bool isMin, bool isMax, DateTime? min, DateTime? max)
		{
			_isMin = isMin;
			_isMax = isMax;
			_min = min;
			_max = max;

			//_context = portalLiquidContext;
			_dependencies = dependencies;

			_occurences =
				new Lazy<EventOccurrenceDrop[]>(
					() => _adapter.SelectEventOccurrences(DateTime.Today.AddYears(-2), DateTime.Today.AddYears(2))
						.Select(e => new EventOccurrenceDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		[Obsolete]
		protected IEvent Event { get; private set; }

		public override object BeforeMethod(string method)
		{
			if (method == null || !EventsEnabled)
			{
				return null;
			}

			DateTime dateTime;

			if (DateTime.TryParse(method, out dateTime))
			{

				if (_isMin)
				{
					_min = dateTime;

					if (_max != null) //there is a minimum value
					{
						if (_max < _min)
						{
							throw new ArgumentOutOfRangeException("_max", ResourceManager.GetString("MaxDate_LessThan_MinDate_Exception"));
						}

						_occurences = new Lazy<EventOccurrenceDrop[]>(() => _adapter.SelectEventOccurrences(_min.Value, _max.Value)
							.Select(e => new EventOccurrenceDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

						return All;
					}

					_occurences = new Lazy<EventOccurrenceDrop[]>(() => _adapter.SelectEventOccurrences(_min.Value, DateTime.Today.AddYears(2))
						.Select(e => new EventOccurrenceDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

					return new EventOccurrencesDrop(this, _dependencies, false, false, _min, _max);
				}

				if (_isMax)
				{
					_max = dateTime;

					if (_min != null) //there is a minimum value
					{
						if (_max < _min)
						{
							throw new ArgumentOutOfRangeException("_max", ResourceManager.GetString("MaxDate_LessThan_MinDate_Exception"));
						}

						_occurences = new Lazy<EventOccurrenceDrop[]>(() => _adapter.SelectEventOccurrences(_min.Value, _max.Value)
							.Select(e => new EventOccurrenceDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

						return All;
					}

					//there is no minimum value

					_occurences = new Lazy<EventOccurrenceDrop[]>(() => _adapter.SelectEventOccurrences(DateTime.Today.AddYears(-2), _max.Value)
							.Select(e => new EventOccurrenceDrop(this, _dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

					return new EventOccurrencesDrop(this, _dependencies, false, false, _min, _max);
				}

			}

			return null;
		}

		public IEnumerable<EventOccurrenceDrop> All
		{
			get { return _occurences.Value.AsEnumerable(); }
		}

		public EventOccurrencesDrop Min
		{
			get { return new EventOccurrencesDrop(this, _dependencies, true, false, _min, _max); }
		}

		public EventOccurrencesDrop From
		{
			get { return Min; }
		}

		public EventOccurrencesDrop Max
		{
			get { return new EventOccurrencesDrop(this, _dependencies, false, true, _min, _max); }
		}

		public EventOccurrencesDrop Until
		{
			get { return Max; }
		}

		public EventOccurrencesDrop To
		{
			get { return Max; }
		}
	}
}
