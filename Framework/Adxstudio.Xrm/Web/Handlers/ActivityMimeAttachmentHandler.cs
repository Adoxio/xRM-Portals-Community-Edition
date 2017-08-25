/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Adxstudio.Xrm.Activity;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	public sealed class ActivityMimeAttachmentHandler : IHttpHandler
	{
		private readonly Entity _attachment;
		private readonly Entity _webfile;

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="annotation">Note</param>
		/// <param name="webfile">Web File</param>
		public ActivityMimeAttachmentHandler(Entity attachment, Entity webfile)
		{
			_attachment = attachment;
			_webfile = webfile;
		}

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="annotation">Note</param>
		public ActivityMimeAttachmentHandler(Entity annotation)
		{
			_attachment = annotation;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			if (_attachment == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}


			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: context.Request.RequestContext);
			var dataAdapter = new ActivityMimeAttachmentDataAdapter(dataAdapterDependencies);

			dataAdapter.DownloadAttachment(new HttpContextWrapper(context), _attachment, _webfile);
		}
	}
}
