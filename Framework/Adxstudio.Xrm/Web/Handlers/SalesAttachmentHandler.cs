/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	/// <summary>
	/// A handler for serving sales attachment (salesliteratureitem) files.
	/// </summary>
	public sealed class SalesAttachmentHandler : IHttpHandler
	{
		private readonly Entity _salesLiteratureItem;
		
		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="salesLiteratureItem">Sales Literature Item</param>
		public SalesAttachmentHandler(Entity salesLiteratureItem)
		{
			_salesLiteratureItem = salesLiteratureItem;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			if (_salesLiteratureItem == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var body = _salesLiteratureItem.GetAttributeValue<string>("documentbody");

			if (string.IsNullOrWhiteSpace(body))
			{
				context.Response.StatusCode = (int)HttpStatusCode.NoContent;
				return;
			}

			var data = Convert.FromBase64String(body);
			var eTag = Utility.ComputeETag(data);
			var modifiedOn = _salesLiteratureItem.GetAttributeValue<DateTime?>("modifiedon");

			var notModified = IsNotModified(context, eTag, modifiedOn);

			if (notModified)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotModified;
				return;
			}

			if (modifiedOn != null)
			{
				context.Response.Cache.SetLastModified(modifiedOn.Value);
			}

			if (!string.IsNullOrWhiteSpace(eTag))
			{
				context.Response.Cache.SetETag(eTag);
			}

			var defaultCacheability = context.User.Identity.IsAuthenticated ? HttpCacheability.Private : HttpCacheability.Public;

			SetResponseParameters(context.Response, defaultCacheability, _salesLiteratureItem, data);

			Utility.Write(new HttpResponseWrapper(context.Response), data);
		}

		/// <summary>
		///  Sets the http response parameters for status code, caching, and headers.
		/// </summary>
		private static void SetResponseParameters(HttpResponse response, HttpCacheability defaultCacheability, Entity salesLiteratureItem, byte[] data)
		{
			response.StatusCode = (int)HttpStatusCode.OK;
			response.ContentType = salesLiteratureItem.GetAttributeValue<string>("mimetype");

			const string contentDispositionText = "inline";
			
			var contentDisposition = new StringBuilder(contentDispositionText);

			AppendFilenameToContentDisposition(salesLiteratureItem, contentDisposition);

			response.AppendHeader("Content-Disposition", contentDisposition.ToString());
			response.AppendHeader("Content-Length", data.Length.ToString(CultureInfo.InvariantCulture));

			Utility.SetResponseCachePolicy(new HttpCachePolicyElement(), new HttpResponseWrapper(response), defaultCacheability);
		}

		private static void AppendFilenameToContentDisposition(Entity salesLiteratureItem, StringBuilder contentDisposition)
		{
			var filename = salesLiteratureItem.GetAttributeValue<string>("filename");

			if (string.IsNullOrEmpty(filename))
			{
				return;
			}

			// Escape any quotes in the filename. (There should rarely if ever be any, but still.)
			var escaped = filename.Replace(@"""", @"\""");

			// Quote the filename parameter value.
			contentDisposition.AppendFormat(@";filename=""{0}""", escaped);
		}

		private static bool IsNotModified(HttpContext context, string eTag, DateTime? modifiedOn)
		{
			var ifNoneMatch = context.Request.Headers["If-None-Match"];
			DateTime ifModifiedSince;

			// check the etag and last modified

			if (ifNoneMatch != null && ifNoneMatch == eTag)
			{
				return true;
			}

			if (modifiedOn != null
				&& DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out ifModifiedSince)
				&& ifModifiedSince.ToUniversalTime() >= modifiedOn.Value.ToUniversalTime())
			{
				return true;
			}

			return false;
		}
	}
}
