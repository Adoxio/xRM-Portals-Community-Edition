/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Diagnostics;
using System.Web;

namespace Microsoft.Xrm.Client.Diagnostics
{
	/// <summary>
	/// Code tracing methods.
	/// </summary>
	/// <remarks>
	/// The framework uses several built-in <see cref="TraceSource"/> objects for tracing. These sources can be identified by name and reconfigured.
	/// <example>
	/// Controlling the trace output from the configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	///  <system.diagnostics>
	///   <sharedListeners>
	///    <add name="Console" type="System.Diagnostics.ConsoleTraceListener"/>
	///   </sharedListeners>
	///   <switches>
	///    <add name="Framework"
	///     value="All" [Off | Critical | Error | Warning | Information | Verbose | All]
	///    />
	///    <add name="Workflow"
	///     value="All" [Off | Critical | Error | Warning | Information | Verbose | All]
	///    />
	///    <add name="AppDomain"
	///     value="All" [Off | Critical | Error | Warning | Information | Verbose | All]
	///    />
	///   </switches>
	///   <sources>
	///    <source name="Framework">
	///     <listeners>
	///      <add name="Console"/>
	///     </listeners>
	///    </source>
	///    <source name="Workflow">
	///     <listeners>
	///      <add name="Console"/>
	///     </listeners>
	///    </source>
	///    <source name="AppDomain">
	///     <listeners>
	///      <add name="Console"/>
	///     </listeners>
	///    </source>
	///   </sources>
	///   <trace autoflush="true"/>
	///  </system.diagnostics>
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public static class Tracing
	{
		static Tracing()
		{
			Framework = new TraceSource("Framework", SourceLevels.All);
			Workflow = new TraceSource("Workflow", SourceLevels.All);
			AppDomain = new TraceSource("AppDomain", SourceLevels.All);
		}

		public static TraceSource Framework { get; private set; }
		public static TraceSource Workflow { get; private set; }
		public static TraceSource AppDomain { get; private set; }

		public static void FrameworkInformation(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Framework, TraceEventType.Information, className, memberName, format, args);
		}

		public static void WorkflowInformation(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Workflow, TraceEventType.Information, className, memberName, format, args);
		}

		public static void AppDomainInformation(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(AppDomain, TraceEventType.Information, className, memberName, format, args);
		}

		public static void FrameworkError(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Framework, TraceEventType.Error, className, memberName, format, args);
		}

		public static void WorkflowError(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Workflow, TraceEventType.Error, className, memberName, format, args);
		}

		public static void AppDomainError(string className, string memberName, string format, params object[] args)
		{
			TraceEvent(AppDomain, TraceEventType.Error, className, memberName, format, args);
		}

		public static void FrameworkEvent(TraceEventType eventType, string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Framework, eventType, className, memberName, format, args);
		}

		public static void WorkflowEvent(TraceEventType eventType, string className, string memberName, string format, params object[] args)
		{
			TraceEvent(Workflow, eventType, className, memberName, format, args);
		}

		public static void AppDomainEvent(TraceEventType eventType, string className, string memberName, string format, params object[] args)
		{
			TraceEvent(AppDomain, eventType, className, memberName, format, args);
		}

		private static void TraceEvent(
			TraceSource source,
			TraceEventType eventType,
			string className,
			string memberName,
			string format,
			params object[] args)
		{
			if (format == null)
			{
				format = "<null>";
			}

			try
			{
				// escape the curly brackets if no arguments are included

				if (args == null || args.Length == 0)
				{
					format = format.Replace("{", "{{").Replace("}", "}}");
				}

				if (HttpContext.Current != null)
				{
					// respect the switch value of the trace source

					if (!source.Switch.ShouldTrace(eventType)) return;

					if (eventType == TraceEventType.Critical
						|| eventType == TraceEventType.Error
							|| eventType == TraceEventType.Warning)
					{
						HttpContext.Current.Trace.Warn(
							className + ": " + memberName,
							format.FormatWith(args));
					}
					else
					{
						HttpContext.Current.Trace.Write(
							className + ": " + memberName,
							format.FormatWith(args));
					}
				}
				else
				{
					source.TraceEvent(
						eventType,
						0,
						"{0}: {1}: {2}".FormatWith(
							className,
							memberName,
							format.FormatWith(args)));
				}
			}
			catch
			{
				// tracing errors should not be fatal, handle error locally
			}
		}
	}
}
