/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Adxstudio.Xrm.Performance.AggregateEvent
{
	public class PerformanceAggregateLogger : IPerformanceLogger
	{
		/// <summary>
		/// Key used to store the aggregator in HttpContext.Current
		/// </summary>
		public static readonly object AggregatorKey = "PerfAg";


		private IPerformanceLogger _logger;
		/// <summary>
		/// Initializes PerformanceAggregateLogger with a inner logger.
		/// </summary>
		/// <param name="fineGrainLogger">This inner logger "Log" will be called when calling this.Log </param>
		public PerformanceAggregateLogger(IPerformanceLogger fineGrainLogger)
		{
			_logger = fineGrainLogger;
		}
		public void Log(IPerformanceMarker marker)
		{
			_logger.Log(marker);

			IPerformanceAggregate ag = GetPerformanceAggregate();
			if (ag != null)
				ag.AppendMarker(marker);
		}
		
		static public IPerformanceAggregate GetPerformanceAggregate()
		{
			IPerformanceAggregate ag = null;
			var context = HttpContext.Current;

			if (context == null)
				return null;

			if (context.Items.Contains(AggregatorKey))
			{
				ag = context.Items[AggregatorKey] as IPerformanceAggregate;
			}
			else
			{
				ag = new PerformanceAggregate();
				context.Items.Add(AggregatorKey, ag);
			}

			return ag;
		}

	}
}
