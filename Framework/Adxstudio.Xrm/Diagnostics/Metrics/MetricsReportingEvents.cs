/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Metrics
{
	using System;
	using System.Diagnostics.Tracing;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;

	/// <summary>
	/// Used to report metric initialize and send failures.
	/// </summary>
	[EventSource(Guid = "D707F148-68FA-4BE8-9607-0949EF3F97E0", Name = InternalName)]
	internal sealed class MetricsReportingEvents : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "MetricsReportingEvents";

		private MetricsReportingEvents() { }

		static readonly Lazy<MetricsReportingEvents> LazyInstance = new Lazy<MetricsReportingEvents>(() => new MetricsReportingEvents());

		public static MetricsReportingEvents Instance
		{
			get
			{
				return LazyInstance.Value;
			}
		}

		public enum EventNames
		{
			MetricInitializationFailed = 1,
			MetricReportingFailed = 2
		}

		[NonEvent]
		public void MetricInitializationFailed(string exception)
		{
			this.MetricInitializationFailed(
				exception,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		[NonEvent]
		public void MetricReportingFailed(string metricName, string exception)
		{
			this.MetricReportingFailed(
				metricName,
				exception,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		[Event((int)EventNames.MetricInitializationFailed, Message = "Exception : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrialType : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Critical, Version = 2)]
		private void MetricInitializationFailed(string exception, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			this.WriteEvent((int)EventNames.MetricInitializationFailed, exception, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		[Event((int)EventNames.MetricReportingFailed, Message = "Metric Name : {0} Exception : {1} PortalUrl : {2} PortalVersion : {3} PortalProductionOrTrialType : {4} SessionId:{5} ElapsedTime:{6}", Level = EventLevel.Critical, Version = 2)]
		private void MetricReportingFailed(string metricName, string exception, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			this.WriteEvent((int)EventNames.MetricReportingFailed, metricName, exception, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}
	}
}
