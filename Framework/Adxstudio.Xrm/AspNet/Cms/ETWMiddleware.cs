/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Adxstudio.Xrm.Diagnostics.Trace;

namespace Adxstudio.Xrm.AspNet.Cms
{
	/// <summary>
	/// Creating OWIN middleware to make the ETW activityId for all the ETW events emitted as part of a single request same.
	/// </summary>
	public class ETWMiddleware : OwinMiddleware
	{
		public ETWMiddleware(OwinMiddleware next) : base(next)
		{

		}

		/// <summary>
		/// The invoke sets the ActivityID.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task Invoke(IOwinContext context)
		{
			var guid = Guid.NewGuid();
			EventSource.SetCurrentThreadActivityId(guid);

			var headers = context.Response.Headers;

			headers.Append("x-ms-request-id", guid.ToString());

			var details = PortalDetail.Instance;

			if (!string.IsNullOrEmpty(details.PortalApp))
			{
				headers.Append("x-ms-portal-app", details.PortalApp);
			}

			if (Next != null)
			{
				await Next.Invoke(context);
			}
		}
	}
}
