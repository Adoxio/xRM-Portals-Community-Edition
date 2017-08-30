/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.Owin;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Returns health of the portal.
	/// </summary>
	public class HealthMiddleware : OwinMiddleware
	{
		/// <summary>
		/// path string for health.
		/// </summary>
		private readonly PathString callbackPath = new PathString("/_services/about/health");

		/// <summary>
		/// Initializes a new instance of the <see cref="HealthMiddleware"/> class.
		/// </summary>
		/// <param name="next">OwinMiddleware Param</param>
		public HealthMiddleware(OwinMiddleware next)
			: base(next)
		{
		}

		/// <summary>
		/// returns the json of the health of the portal
		/// </summary>
		/// <param name="context">IOwinContext Param</param>
		/// <returns>Returns Task</returns>
		public override async Task Invoke(IOwinContext context)
		{
			if (object.Equals(context.Request.Path, this.callbackPath))
			{
				string info = this.GetInfo();
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(info);
			}
			else
			{
				await Next.Invoke(context);
			}
		}

		/// <summary>
		/// returns the string json of the health of the portal like if app is up/down, CRM Connectivity, ServiceBus health etc.,
		/// </summary>
		/// <returns>Returns json with contents of health</returns>
		private string GetInfo()
		{
			// Now returns webapphealthy : true.
			// To Do: Add CRM Connectivity, ServiceBus, and other health related flags
			var info = new JObject(new JProperty("webapphealthy", "true"));
			return info.ToString(Formatting.Indented);
		}
	}
}
