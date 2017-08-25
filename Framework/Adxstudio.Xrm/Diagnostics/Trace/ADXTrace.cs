/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core.Telemetry.EventSources;
	using Adxstudio.Xrm.Diagnostics.Trace;

	/// <summary>
	/// Trace implementation for ADX which will emit ETW events.
	/// </summary>
	[EventSource(Guid = "47257FB4-4830-456B-9024-8FFA41134A7C", Name = InternalName)]
	public class ADXTrace : EventSourceBase
	{
		/// <summary>
		/// The EventSource name.
		/// </summary>
		private const string InternalName = "ADXTrace";

		/// <summary>
		/// The internal TraceSource.
		/// </summary>
		private static readonly TraceSource InternalTrace = new TraceSource(InternalName);

		/// <summary>
		/// The variable for lazy initialization of <see cref="ADXTrace"/>.
		/// </summary>
		private static readonly Lazy<ADXTrace> _instance = new Lazy<ADXTrace>();

		public static ADXTrace Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private enum EventNames
		{
			//
			// Summary:
			//     This level adds standard errors that signify a problem.
			Error = 1,
			//
			// Summary:
			//     This level adds warning events (for example, events that are published because
			//     a disk is nearing full capacity).
			Warning = 2,
			//
			// Summary:
			//     This level adds informational events or messages that are not errors. These
			//     events can help trace the progress or state of an application.
			Informational = 3,
			//
			// Summary:
			//     This level adds detailed debugging events.
			Verbose = 4,
		}

		/// <summary>
		/// Trace Error emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public void TraceError(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			this.TraceError(
				category,
				message,
				memberName,
				sourceFilePath,
				sourceLineNumber,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		/// <summary>
		/// Trace Warning emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public void TraceWarning(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			this.TraceWarning(
				category,
				message,
				memberName,
				sourceFilePath,
				sourceLineNumber,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		/// <summary>
		/// Trace Info emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public void TraceInfo(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			this.TraceInfo(
				category,
				message,
				memberName,
				sourceFilePath,
				sourceLineNumber,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		/// <summary>
		/// Trace Verbose emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public void TraceVerbose(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			this.TraceVerbose(
				category,
				message,
				memberName,
				sourceFilePath,
				sourceLineNumber,
				this.PortalUrl,
				this.PortalVersion,
				this.ProductionOrTrial,
				this.SessionId,
				this.ElapsedTime());
		}

		/// <summary>
		/// Trace Error emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="sessionId">Current session identifier</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)EventNames.Error, Level = EventLevel.Error, Message = "Category:{0} Message:{1} MemberName:{2} SourceFilePath:{3} SourceLineNumber:{4} PortalURL:{5} PortalVersion:{6} PortalProductionOrTrial:{7} SessionId:{8} ElapsedTime:{9}", Version = 4)]
		private void TraceError(TraceCategory category, string message, string memberName, string sourceFilePath, int sourceLineNumber, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			TraceEvent(TraceEventType.Error, EventNames.Error, category, message, memberName, sourceFilePath, sourceLineNumber);

			this.WriteEvent((int)EventNames.Error, category, message, memberName, sourceFilePath, sourceLineNumber, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Trace Warning emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="sessionId">Current session identifier</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)EventNames.Warning, Level = EventLevel.Warning, Message = "Category:{0} Message:{1} MemberName:{2} SourceFilePath:{3} SourceLineNumber:{4} PortalURL:{5} PortalVersion:{6} PortalProductionOrTrial:{7} SessionId:{8} ElapsedTime:{9}", Version = 5)]
		private void TraceWarning(TraceCategory category, string message, string memberName, string sourceFilePath, int sourceLineNumber, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			TraceEvent(TraceEventType.Warning, EventNames.Warning, category, message, memberName, sourceFilePath, sourceLineNumber);

			this.WriteEvent((int)EventNames.Warning, category, message, memberName, sourceFilePath, sourceLineNumber, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Trace Info emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="sessionId">Current session identifier</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)EventNames.Informational, Level = EventLevel.Informational, Message = "Category:{0} Message:{1} MemberName:{2} SourceFilePath:{3} SourceLineNumber:{4} PortalURL:{5} PortalVersion:{6} PortalProductionOrTrial:{7} SessionId:{8} ElapsedTime:{9}", Version = 4)]
		private void TraceInfo(TraceCategory category, string message, string memberName, string sourceFilePath, int sourceLineNumber, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			TraceEvent(TraceEventType.Information, EventNames.Informational, category, message, memberName, sourceFilePath, sourceLineNumber);

			this.WriteEvent((int)EventNames.Informational, category, message, memberName, sourceFilePath, sourceLineNumber, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Trace Verbose emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		/// <param name="portalUrl">Url for the portal home page as listed in Azure</param>
		/// <param name="portalVersion">Version of the portal</param>
		/// <param name="portalProductionOrTrialType">Production or trial portal</param>
		/// <param name="sessionId">Current session identifier</param>
		/// <param name="elapsedTime">time elapsed within the current request</param>
		[Event((int)EventNames.Verbose, Level = EventLevel.Verbose, Message = "Category:{0} Message:{1} MemberName:{2} SourceFilePath:{3} SourceLineNumber:{4} PortalURL:{5} PortalVersion:{6} PortalProductionOrTrial:{7} SessionId:{8} ElapsedTime:{9}", Version = 5)]
		private void TraceVerbose(TraceCategory category, string message, string memberName, string sourceFilePath, int sourceLineNumber, string portalUrl, string portalVersion, string portalProductionOrTrialType, string sessionId, string elapsedTime)
		{
			TraceEvent(TraceEventType.Verbose, EventNames.Verbose, category, message, memberName, sourceFilePath, sourceLineNumber);

			this.WriteEvent((int)EventNames.Verbose, category, message, memberName, sourceFilePath, sourceLineNumber, portalUrl, portalVersion, portalProductionOrTrialType, sessionId, elapsedTime);
		}

		/// <summary>
		/// Trace an ETW event.
		/// </summary>
		/// <param name="type">The event type.</param>
		/// <param name="eventId">The event id.</param>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		private static void TraceEvent(TraceEventType type, EventNames eventId, TraceCategory category, string message, string memberName, string sourceFilePath, int sourceLineNumber)
		{
			InternalTrace.TraceEvent(
				PortalSettings.Instance, type, (int)eventId,
				"Event Level={0}, Category={1}, Message={2}, Member Name={3}, Source File Path={4}, Source Line Number={5}, PortalID={6}, PortalEvent={7}, TenantID={8}, OrgID={9}, Geo={10}, PortalApp={11}, PortalType={12}",
				eventId, category, message, memberName, sourceFilePath, sourceLineNumber, PortalDetail.Instance.PortalId, PortalEvents.TraceInfo, PortalDetail.Instance.TenantId, PortalDetail.Instance.OrgId, PortalDetail.Instance.Geo, PortalDetail.Instance.PortalApp, PortalDetail.Instance.PortalType);
		}
	}
}
