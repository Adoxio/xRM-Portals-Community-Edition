/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Performance
{
	public interface IPerformanceAggregate
	{
		DateTime FirstTimestamp { get; }
		string RequestId { get; }
		string SessionId { get; }	

		/// <summary>
		/// Contains all the Aggregates times per Area. 
		/// </summary>
		double[] AggregatesInMilliseconds { get; }

		void AppendMarker(IPerformanceMarker pm);
	}
}
