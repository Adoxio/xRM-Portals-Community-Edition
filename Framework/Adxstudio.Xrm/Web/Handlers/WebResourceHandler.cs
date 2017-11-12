/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// Used to retrieve a single media item from a Web Resource in Microsoft's Dynamics CRM.
	/// </summary>
	internal class WebResourceHandler : IHttpHandler
	{
		private readonly Entity _webResource;
		
		/// <summary>
		/// There does not appear to be anyway to get this Option Set from the CRM so this is the hard-coded list.
		/// </summary>
		private Dictionary<int, string> _webResourceTypes = new Dictionary<int, string>
		{
			{ 1, "text/html" }, //Web Page (HTML)
			{ 2, "text/css" }, //Style Sheet (CSS)
			{ 3, "application/x-javascript" }, //Script (JScript)
			{ 4, "text/xml" }, //Data (XML)
			{ 5, "image/png" }, //PNG format
			{ 6, "image/jpeg" }, //JPG format
			{ 7, "image/gif" }, //GIF format
			{ 8, "application/x-silverlight-app" }, //Silverlight (XAP)
			{ 9, "text/xml" }, //Stylesheet (XSL)
			{ 10, "image/x-icon" }, //ICO format
		};

		public WebResourceHandler(Entity webResource)
		{
			_webResource = webResource;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			if (_webResource == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(_webResource.GetAttributeValue<string>("content")))
				{
					context.Response.StatusCode = (int)HttpStatusCode.NoContent;

					return;
				}

				SetResponseParameters(context, _webResource);

				var data = Convert.FromBase64String(_webResource.GetAttributeValue<string>("content"));

				Utility.Write(new HttpResponseWrapper(context.Response), data);
			}
		}

		/// <summary>
		///  Sets the http response parameters for status code, caching, and headers.
		/// </summary>
		protected virtual void SetResponseParameters(HttpContext context, Entity webResource)
		{
			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.ContentType = GetMimeType(webResource);
			context.Response.Cache.SetExpires(DateTime.Now.AddDays(30));
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetLastModified(DateTime.Now);
		}

		private string GetMimeType(Entity webResource)
		{
			var key = webResource.GetAttributeValue<OptionSetValue>("webresourcetype").Value;

			return _webResourceTypes.ContainsKey(key)
				? _webResourceTypes[key]
				: "text/plain";
		}
	}
}
