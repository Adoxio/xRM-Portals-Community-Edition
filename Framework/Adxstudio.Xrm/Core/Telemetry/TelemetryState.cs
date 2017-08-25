/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Core.Telemetry
{
	using System;
	using System.Configuration;
	using System.Linq;
	using System.Web;

	/// <summary>
	/// An Enum describing details around an application ending
	/// </summary>
	[Flags]
	public enum ApplicationEndFlags
	{
		Unknown = 0,
		Configuration = 1,
		MetadataDriven = 2,
		TouchWebConfig = 4,
		UnloadAppDomain = 8,

	}

	/// <summary>
	/// Encapsulates Telemetry-centric Application State
	/// </summary>
	public static class TelemetryState
	{
		/// <summary>
		/// Flags describing details into why an Application is ending
		/// </summary>
		public static ApplicationEndFlags ApplicationEndInfo { get; set; } = ApplicationEndFlags.Unknown;

		/// <summary>
		/// This is to ignore Request Execution Time and Performance Events for Traffic Manager Pings and Azure pings
		/// GTMProbe for Traffic Manager pings
		/// AlwaysOn for Azure pings
		/// AppInsights for Application Insights' Availablity Tests
		/// </summary>
		/// <returns>true if valid user agent; otherwise false</returns>
		public static bool IsTelemetryEnabledUserAgent()
		{
			// invalid validation if the useragent is null
			string userAgent;
			if (!TelemetryState.HasRequestContext || (userAgent = HttpContext.Current.Request.UserAgent) == null)
			{
				return false;
			}

			var ignoredAgentsString = ConfigurationManager.AppSettings["IgnoreUserAgent"];
			if (string.IsNullOrWhiteSpace(ignoredAgentsString))
			{

				// don't ignore any
				return true;
			}

			var ignoredList = ignoredAgentsString.Split(',');
			return !ignoredList.Any(userAgent.Contains);
		}

		/// <summary>
		/// This is to ignore non-page requests
		/// e.g., css,js, ...
		/// </summary>
		/// <returns>true if valid path; otherwise false</returns>
		public static bool IsTelemetryEnabledRequestExtension()
		{
			string requestExtension;
			if (!TelemetryState.HasRequestContext
				|| (requestExtension = HttpContext.Current.Request.CurrentExecutionFilePathExtension) == null)
			{
				return false;
			}

			var allowedString = ConfigurationManager.AppSettings["TelemetryFileTypes"];
			if (allowedString == null)
			{

				// don't ignore any
				return true;
			}

			var allowed = allowedString.Split(',');
			return allowed.Any(requestExtension.Equals);
		}

		/// <summary>
		/// This is to ignore non-user based requests
		/// e.g., /_services/ /setup/ /_resources/ /xrm/js/
		/// </summary>
		/// <returns>true if valid path; otherwise false</returns>
		public static bool IsTelemetryEnabledRequestPath()
		{
			if (!TelemetryState.HasRequestContext)
			{
				return false;
			}

			var path = HttpContext.Current.Request.Path;
			return !TelemetryState.IsPathMatch(path);
		}

		/// <summary>
		/// Does the path have a match the ignored path list
		/// </summary>
		/// <param name="path">path to validate</param>
		/// <returns>true if the path matches with an item in the list; otherwise false</returns>
		private static bool IsPathMatch(string path)
		{
			var ignoredString = ConfigurationManager.AppSettings["IgnorePath"];
			if (ignoredString == null)
			{
				return false;
			}

			var ignoredList = ignoredString.Split(',');
			return ignoredList.Any(path.StartsWith);
		}

		/// <summary>
		/// This is to ignore non-page requests
		/// Wil check the path and the referrer's path to determine if the page request came from a valid path
		/// </summary>
		/// <returns>true if valid path; otherwise false</returns>
		public static bool IsTelemetryEnabledRequestPage()
		{
			if (!TelemetryState.HasRequestContext)
			{
				return false;
			}

			Uri referrer = null;
			if ((referrer = HttpContext.Current.Request.UrlReferrer) == null)
			{
				return TelemetryState.IsTelemetryEnabledRequestPath();
			}

			return !TelemetryState.IsPathMatch(referrer.AbsolutePath);
		}

		/// <summary>
		/// Returns true if the Context and the Request are not null; otherwise false 
		/// </summary>
		private static bool HasRequestContext
		{
			get
			{
				return HttpContext.Current != null
					&& HttpContext.Current.Request != null;
			}
		}
	}
}
