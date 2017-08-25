/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Core.Telemetry
{
	using System;
	using System.Diagnostics;
	using Microsoft.AspNet.Identity.Owin;
	using Microsoft.Owin;

	/// <summary>
	/// Context around the start and elapsed time for the current request
	/// </summary>
	public class RequestElapsedTimeContext : IDisposable
	{
		/// <summary>
		/// the stopwatch
		/// </summary>
		private readonly Stopwatch stopwatch;

		/// <summary>
		/// Start time for the request
		/// </summary>
		public DateTime RequestStartTime { get; set; }

		/// <summary>
		/// Create method for the OwinContext
		/// </summary>
		/// <param name="options">the IdentityFactoryOptions</param>
		/// <param name="context">the context</param>
		/// <returns>an instance of the TimerContext</returns>
		public static RequestElapsedTimeContext Create(IdentityFactoryOptions<RequestElapsedTimeContext> options, IOwinContext context)
		{
			return new RequestElapsedTimeContext();
		}

		/// <summary>
		/// Elapsed time for the current request
		/// </summary>
		/// <returns>elapsed time in miliseconds</returns>
		public long ElapsedTime()
		{
			return this.stopwatch.ElapsedMilliseconds;
		}

		/// <summary>
		/// Required for the Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions
		/// </summary>
		public void Dispose()
		{
			this.stopwatch.Stop();
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="RequestElapsedTimeContext" /> class from being created.
		/// </summary>
		private RequestElapsedTimeContext()
		{
			this.stopwatch = new Stopwatch();
			this.stopwatch.Start();
		}
	}
}
