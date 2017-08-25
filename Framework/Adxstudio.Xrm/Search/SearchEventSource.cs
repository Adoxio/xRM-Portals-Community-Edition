/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search
{
	using System;
	using System.Diagnostics.Tracing;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;

	[EventSource(Guid = "8C3F51E2-A61A-47AC-B82C-0F6E79FDC1BF", Name = InternalName)]
	internal sealed class SearchEventSource : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "PortalSearch";

		private static readonly Lazy<SearchEventSource> _instance = new Lazy<SearchEventSource>();

		public static SearchEventSource Log
		{
			get { return _instance.Value; }
		}

		private enum EventName
		{
			/// <summary>
			/// Error occurred during attempted search index query.
			/// </summary>
			QueryError = 1,

			/// <summary>
			/// Error occurred during attempted search index read.
			/// </summary>
			ReadError = 2,

			/// <summary>
			/// Error occurred during attempted search index write.
			/// </summary>
			WriteError = 3,
		}

		/// <summary>
		/// Log that error occurred during attempted search index query.
		/// </summary>
		[NonEvent]
		public void QueryError(Exception exception)
		{
			if (exception == null)
			{
				return;
			}

			QueryError(
				exception.ToString(),
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		[Event((int)EventName.QueryError, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 3)]
		private void QueryError(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.QueryError, message ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log that error occurred during attempted search index read.
		/// </summary>
		[NonEvent]
		public void ReadError(Exception exception)
		{
			if (exception == null)
			{
				return;
			}

			ReadError(
				exception.ToString(),
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		[Event((int)EventName.ReadError, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 3)]
		private void ReadError(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.ReadError, message ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Log that error occurred during attempted search index write.
		/// </summary>
		[NonEvent]
		public void WriteError(Exception exception)
		{
			if (exception == null)
			{
				return;
			}

			WriteError(
				exception.ToString(),
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		[Event((int)EventName.WriteError, Message = "Message : {0} PortalUrl : {1} PortalVersion : {2} PortalProductionOrTrial : {3} SessionId:{4} ElapsedTime:{5}", Level = EventLevel.Error, Version = 3)]
		private void WriteError(string message, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			WriteEvent(EventName.WriteError, message ?? string.Empty, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}
	}
}
