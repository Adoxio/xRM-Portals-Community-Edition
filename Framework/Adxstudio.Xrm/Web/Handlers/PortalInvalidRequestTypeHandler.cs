/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class PortalInvalidRequestTypeHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.StatusCode = 500;
			context.Response.End();
			throw new Exception("Invalid Request Type.");
		}
	}
}
