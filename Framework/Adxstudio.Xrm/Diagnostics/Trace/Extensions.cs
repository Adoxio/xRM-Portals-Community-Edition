/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Trace
{
	using System.Diagnostics;
	using Adxstudio.Xrm.Configuration;

	/// <summary>
	/// Helpers related to logging.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Dispatches trace logs to either the custom <see cref="TraceSource"/> or the common <see cref="Trace"/> class.
		/// </summary>
		/// <param name="traceSource">The TraceSource.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="eventType">The event type.</param>
		/// <param name="id">The event Id.</param>
		/// <param name="format">The message format.</param>
		/// <param name="args">The message arguments.</param>
		public static void TraceEvent(this TraceSource traceSource, PortalSettings settings, TraceEventType eventType, int id, string format, params object[] args)
		{
			// write the to the current TraceSource
			traceSource.TraceEvent(eventType, id, format, args);

			if (settings.WriteToDiagnosticsTrace)
			{
				// redirect to the common System.Diagnostics.Trace
				if (traceSource.Switch.ShouldTrace(eventType))
				{
					if (eventType == TraceEventType.Critical || eventType == TraceEventType.Error)
					{
						Trace.TraceError(format, args);
					}
					else if (eventType == TraceEventType.Warning)
					{
						Trace.TraceWarning(format, args);
					}
					else if (eventType == TraceEventType.Information || eventType == TraceEventType.Verbose)
					{
						Trace.TraceInformation(format, args);
					}
				}
			}
		}
	}
}
