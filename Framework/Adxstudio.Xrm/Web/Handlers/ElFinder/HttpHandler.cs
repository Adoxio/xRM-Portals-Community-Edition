/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public class HttpHandler : IHttpHandler, IRequiresSessionState
	{
		private readonly string _portalName;
		private readonly RequestContext _requestContext;

		public HttpHandler(string portalName, RequestContext requestContext)
		{
			_portalName = portalName;
			_requestContext = requestContext;
		}

		public void ProcessRequest(HttpContext context)
		{
			var commandRouter = new HttpCommandRouter(_portalName, _requestContext);

			var response = commandRouter.GetResponse(context);

			if (response.CloseConnection)
			{
				context.Response.Headers.Set("Connection", "close");

				return;
			}

			context.Response.ContentEncoding = Encoding.UTF8;

			// On POSTs, elFinder has problems dealing with this content type, even though the
			// response is indeed JSON.
			if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.InvariantCultureIgnoreCase))
			{
				context.Response.ContentType = "application/json";
			}

			context.Response.Write(response.ToJson());
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}
