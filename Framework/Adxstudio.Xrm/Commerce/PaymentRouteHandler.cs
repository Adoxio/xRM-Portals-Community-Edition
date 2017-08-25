/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Commerce
{
	public class PaymentRouteHandler : IRouteHandler
	{
		public string PortalName { get; set; }

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new PaymentProcessingRequestHandlerFactory(requestContext);
		}
	}
}
