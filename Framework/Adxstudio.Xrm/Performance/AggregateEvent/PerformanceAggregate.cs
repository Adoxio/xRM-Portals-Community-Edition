/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Performance
{
	internal class PerformanceAggregate : IPerformanceAggregate
	{
		
		public double[] AggregatesInMilliseconds { get; private set; }
		public DateTime FirstTimestamp { get; private set; }
		public string RequestId { get; private set; }
		public string SessionId { get; private set; }
	

		public PerformanceAggregate()
		{
			AggregatesInMilliseconds = new double[Enum.GetNames(typeof(PerformanceMarkerArea)).Length];
			FirstTimestamp = DateTime.MaxValue;
			RequestId = string.Empty;
			SessionId = string.Empty;
		}


		/// <summary>
		/// We keep track of stopwatches which have an Elapsed value and we sum them by area.
		/// </summary>
		/// <param name="pm"></param>
		public void AppendMarker(IPerformanceMarker pm)
		{			
			if (pm.Type == PerformanceMarkerType.Stopwatch && pm.Elapsed.HasValue)
			{
				PerformanceMarkerArea currentArea = pm.Area;
				double elapsed = pm.Elapsed.Value.TotalMilliseconds;
				AggregatesInMilliseconds[(int)currentArea] += elapsed;
				TryUpdateMetaData(pm.Timestamp, pm.RequestId, pm.SessionId);
			}
		}

		/// <summary>
		/// When marker starts, the RequestId or SessionId might not be initialized yet.
		/// </summary>
		/// <param name="timestamp">We pick the earliest Timestamp for this request as our reference TimeStamp</param>
		/// <param name="requestId">The requestId of the current HttpContext</param>
		/// <param name="sessionId">The sessionId of the current HttpContext</param>
		private void TryUpdateMetaData(DateTime timestamp, string requestId, string sessionId)
		{
			if (timestamp < FirstTimestamp)
				FirstTimestamp = timestamp;
			if (!string.IsNullOrEmpty(requestId))
				RequestId = requestId;
			if (!string.IsNullOrEmpty(sessionId))
				SessionId = sessionId;
		}
	}
}
