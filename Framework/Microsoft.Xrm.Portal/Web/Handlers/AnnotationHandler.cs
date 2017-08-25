/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.Handlers
{
	/// <summary>
	/// A handler for serving annotation files.
	/// </summary>
	/// <remarks>
	/// The <see cref="HttpCachePolicy"/> cache policy can be adjusted by the following configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.portal" type="Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal"/>
	///  </configSections>
	/// 
	///  <microsoft.xrm.portal>
	///   <cachePolicy>
	///    <annotation
	///     cacheExtension=""
	///     cacheability="" [NoCache | Private | Public | Server | ServerAndNoCache | ServerAndPrivate]
	///     expires=""
	///     maxAge="01:00:00" [HH:MM:SS]
	///     revalidation="" [AllCaches | ProxyCaches | None]
	///     slidingExpiration="" [false | true]
	///     validUntilExpires="" [false | true]
	///     varyByCustom=""
	///     varyByContentEncodings="" [gzip;deflate]
	///     varyByContentHeaders=""
	///     varyByParams="*"
	///     />
	///   </cachePolicy>
	///  </microsoft.xrm.portal>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="HttpCachePolicyElement"/>
	public sealed class AnnotationHandler : IHttpHandler
	{
		private readonly Entity _annotation;

		public AnnotationHandler(Entity annotation)
		{
			_annotation = annotation;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			if (_annotation == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var body = _annotation.GetAttributeValue<string>("documentbody");

			if (string.IsNullOrWhiteSpace(body))
			{
				context.Response.StatusCode = (int)HttpStatusCode.NoContent;
				return;
			}

			var data = Convert.FromBase64String(body);
			var eTag = Utility.ComputeETag(data);
			var modifiedOn = _annotation.GetAttributeValue<DateTime?>("modifiedon");

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

			SetResponseParameters(context.Response, defaultCacheability, _annotation, data);

			Utility.Write(context.Response, data);
		}

		/// <summary>
		///  Sets the http response parameters for status code, caching, and headers.
		/// </summary>
		private static void SetResponseParameters(HttpResponse response, HttpCacheability defaultCacheability, Entity annotation, byte[] data)
		{
			response.StatusCode = (int)HttpStatusCode.OK;
			response.ContentType = annotation.GetAttributeValue<string>("mimetype");

			var contentDisposition = new StringBuilder("inline");

			AppendFilenameToContentDisposition(annotation, contentDisposition);

			response.AppendHeader("Content-Disposition", contentDisposition.ToString());
			response.AppendHeader("Content-Length", data.Length.ToString());

			var section = PortalCrmConfigurationManager.GetPortalCrmSection();
			var policy = section.CachePolicy.Annotation;

			Utility.SetResponseCachePolicy(policy, response, defaultCacheability);
		}

		private static void AppendFilenameToContentDisposition(Entity annotation, StringBuilder contentDisposition)
		{
			var filename = annotation.GetAttributeValue<string>("filename");

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
