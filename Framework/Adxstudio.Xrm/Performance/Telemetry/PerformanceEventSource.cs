/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Performance
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Diagnostics.Trace;

	[EventSource(Guid = "F2E43462-6850-4CD3-9074-19DA26E24DE6", Name = InternalName)]
	public sealed class PerformanceEventSource : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalPerformance";

		/// <summary>
		/// The internal TraceSource.
		/// </summary>
		private static readonly TraceSource InternalTrace = new TraceSource(InternalName);

		/// <summary>
		/// The variable for lazy initialization of <see cref="PerformanceEventSource"/>.
		/// </summary>
		private static readonly Lazy<PerformanceEventSource> _instance = new Lazy<PerformanceEventSource>();

		public static PerformanceEventSource Log
		{
			get { return _instance.Value; }
		}

		private enum EventName
		{
			PerformanceMarker = 1,
			PerformanceAggregate = 2
		}

		[NonEvent]
		public void PerformanceMarker(IPerformanceMarker marker)
		{
			if (marker == null)
			{
				return;
			}

			PerformanceMarker(
				marker.Id,
				marker.Name ?? string.Empty,
				marker.Type,
				marker.Source,
				marker.Timestamp,
				marker.RequestId ?? string.Empty,
				marker.SessionId ?? string.Empty,
				PerfMarkerAreaHelper.AreaEnumToString(marker.Area),
				marker.Tag ?? string.Empty,
				marker.Elapsed.GetValueOrDefault().TotalMilliseconds,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial);
		}

		[Event((int)EventName.PerformanceMarker, Message = "Id : {0} Name : {1} Type : {2} Source : {3} Timestamp : {4} Request Id : {5} Session Id : {6} Area : {7} Tag : {8} Elapsed Time In Milliseconds : {9} PortalURL : {10} PortalVersion : {11} PortalProductionOrTrial : {12}", Level = EventLevel.Informational, Version = 2)]
		private void PerformanceMarker(string id, string name, PerformanceMarkerType type, PerformanceMarkerSource source, DateTime timestamp, string requestId, string sessionId, string area, string tag, double elapsedTotalMilliseconds, string portalUrl, string portalVersion, string portalProductionOrTrialType)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, TraceEventType.Information, (int)EventName.PerformanceMarker,
				"Id : {0} Name : {1} Type : {2} Source : {3} Timestamp : {4} Request Id : {5} Session Id : {6} Area : {7} Tag : {8} Elapsed Time In Milliseconds : {9}",
				id, name, type, source, timestamp, requestId, sessionId, area, tag, elapsedTotalMilliseconds);

			if (!IsEnabled())
			{
				return;
			}

			WriteEvent((int)EventName.PerformanceMarker, id, name, type, source, timestamp, requestId, sessionId, area, tag, elapsedTotalMilliseconds, portalUrl, portalVersion, portalProductionOrTrialType);
		}

		/// <summary>
		/// We are forced to extract aggregates one by one as the signature of PerformanceAggregate needs to match WriteEvent
		/// <seealso cref="PerformanceMarkerArea"/>
		/// </summary>
		/// <param name="aggregate"></param>
		[NonEvent]
		public void PerformanceAggregate(IPerformanceAggregate aggregate, TimeSpan totalElapsedTime)
		{
			if (aggregate == null)
			{
				return;
			}

			PerformanceAggregate(
				aggregate.FirstTimestamp,
				totalElapsedTime.TotalMilliseconds,
				aggregate.RequestId ?? string.Empty,
				aggregate.SessionId ?? string.Empty,
				aggregate.AggregatesInMilliseconds[(int)PerformanceMarkerArea.Unknown],
				aggregate.AggregatesInMilliseconds[(int)PerformanceMarkerArea.Crm],
				aggregate.AggregatesInMilliseconds[(int)PerformanceMarkerArea.Cms],
				aggregate.AggregatesInMilliseconds[(int)PerformanceMarkerArea.Liquid],
				aggregate.AggregatesInMilliseconds[(int)PerformanceMarkerArea.Security],
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial);
		}

		[Event((int)EventName.PerformanceAggregate, Message = "Timestamp : {0} Total Request Time : {1} Request Id : {2} Session Id : {3} Unknown Milliseconds : {4} Crm Milliseconds : {5} Cms Milliseconds : {6} Liquid Milliseconds : {7} Security Milliseconds : {8} PortalURL : {9} PortalVersion : {10} PortalProductionOrTrial : {11}", Level = EventLevel.Informational, Version = 2)]
		private void PerformanceAggregate(DateTime timestamp, double totalRequestTime, string requestId, string sessionId, double unknown_ms, double crm_ms, double cms_ms, double liquid_ms, double security_ms, string portalUrl, string portalVersion, string portalProductionOrTrialType)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, TraceEventType.Information, (int)EventName.PerformanceAggregate,
				"Timestamp : {0} Total Request Time : {1} Request Id : {2} Session Id : {3} Unknown Milliseconds : {4} Crm Milliseconds : {5} Cms Milliseconds : {6} Liquid Milliseconds : {7} Security Milliseconds : {8}",
				timestamp, totalRequestTime, requestId, sessionId, unknown_ms, crm_ms, cms_ms, liquid_ms, security_ms);

			if (!IsEnabled())
				return;

			WriteEvent((int)EventName.PerformanceAggregate, timestamp, totalRequestTime, requestId, sessionId, unknown_ms, crm_ms, cms_ms, liquid_ms, security_ms, portalUrl, portalVersion, portalProductionOrTrialType);
		}
	}
}
