/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web.Handlers
{
	/// <summary>
	/// A handler for serving embedded resources.
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
	///    <embeddedResource
	///     cacheExtension=""
	///     cacheability="Public" [NoCache | Private | Public | Server | ServerAndNoCache | ServerAndPrivate]
	///     expires=""
	///     maxAge="01:00:00" [HH:MM:SS]
	///     revalidation="" [AllCaches | ProxyCaches | None]
	///     slidingExpiration="" [false | true]
	///     validUntilExpires="" [false | true]
	///     varyByCustom=""
	///     varyByContentEncodings="gzip;deflate"
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
	public sealed class EmbeddedResourceHttpHandler : IHttpHandler
	{
		private readonly IEnumerable<EmbeddedResourceAssemblyAttribute> _mappings;
		private readonly string[] _paths;

		public EmbeddedResourceHttpHandler()
			: this((string[])null)
		{
		}

		public EmbeddedResourceHttpHandler(params string[] paths)
			: this(Web.Utility.GetEmbeddedResourceMappingAttributes().ToList(), paths)
		{
		}

		public EmbeddedResourceHttpHandler(IEnumerable<EmbeddedResourceAssemblyAttribute> mappings, params string[] paths)
		{
			mappings.ThrowOnNull("mappings");
			paths.ThrowOnNull("paths");

			_mappings = mappings;
			_paths = paths;
		}

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var paths = GetPaths(context);

			if (paths.Count() == 1)
			{
				Render(context, paths.Single());
			}
			else
			{
				Render(context, paths);
			}
		}

		private IEnumerable<string> GetPaths(HttpContext context)
		{
			return _paths ?? context.Request["paths"].Split(',');
		}

		private void Render(HttpContext context, string virtualPath)
		{
			Assembly assembly;
			string resourceName;

			if (TryFindResource(virtualPath, out assembly, out resourceName))
			{
				// compute and check the etag

				var ifNoneMatch = context.Request.Headers["If-None-Match"];
				var eTag = Utility.ComputeETag(assembly, resourceName);

				if (ifNoneMatch == eTag)
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotModified;
					return;
				}

				if (!string.IsNullOrWhiteSpace(eTag))
				{
					context.Response.Cache.SetETag(eTag);
				}

				SetResponseParameters(context.Response);

				SetResponseParameters(context.Response, virtualPath, resourceName);

				RenderResource(assembly, resourceName, context.Response.OutputStream);
			}
		}

		private void Render(HttpContext context, IEnumerable<string> virtualPaths)
		{
			// gather the resources into memory

			var data = new MemoryStream();

			foreach (var path in virtualPaths)
			{
				Assembly assembly;
				string resourceName;

				if (TryFindResource(path, out assembly, out resourceName))
				{
					SetResponseParameters(context.Response, path, resourceName);

					RenderResource(assembly, resourceName, data);
				}
			}

			data.Position = 0;

			var ifNoneMatch = context.Request.Headers["If-None-Match"];
			var eTag = Utility.ComputeETag(data);

			if (ifNoneMatch == eTag)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotModified;
				return;
			}

			if (!string.IsNullOrWhiteSpace(eTag))
			{
				context.Response.Cache.SetETag(eTag);
			}

			SetResponseParameters(context.Response);

			data.Position = 0;

			RenderResource(data, context.Response.OutputStream);
		}

		private bool TryFindResource(string virtualPath, out Assembly assembly, out string resourceName)
		{
			var mapping = _mappings.Match(virtualPath);

			if (mapping != null)
			{
				// check if this file exists as an embedded resource

				var resources = mapping.FindResource(virtualPath);

				if (resources != null)
				{
					assembly = mapping.Assembly;
					resourceName = resources.ResourceName;

					return true;
				}
			}

			assembly = null;
			resourceName = null;

			return false;
		}

		private static void RenderResource(Assembly assembly, string resourceName, Stream output)
		{
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				RenderResource(stream, output);
			}
		}

		private static void RenderResource(Stream input, Stream output)
		{
			var buffer = new byte[4096];

			using (var reader = new BinaryReader(input))
			{
				int bytesRead;

				do
				{
					bytesRead = reader.Read(buffer, 0, buffer.Length);
					output.Write(buffer, 0, bytesRead);
				} while (bytesRead == buffer.Length);
			}
		}

		private static void SetResponseParameters(HttpResponse response)
		{
			var section = PortalCrmConfigurationManager.GetPortalCrmSection();
			var policy = section.CachePolicy.EmbeddedResource;

			Utility.SetResponseCachePolicy(policy, response, HttpCacheability.Public, defaultVaryByContentEncodings: "gzip;deflate");
		}

		private static void SetResponseParameters(HttpResponse response, string virtualPath, string resourceName)
		{
			var extensionWithDot = Path.GetExtension(virtualPath);
			var extension = extensionWithDot != null ? extensionWithDot.TrimStart('.') : string.Empty;

			string contentType;

			if (!string.IsNullOrWhiteSpace(extension) && _mimeMap.TryGetValue(extension, out contentType))
			{
				response.ContentType = contentType;
			}
		}

		#region IIS://localhost/MimeMap

		private static readonly Dictionary<string, string> _mimeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "323", "text/h323" },
			{ "aaf", "application/octet-stream" },
			{ "aca", "application/octet-stream" },
			{ "accdb", "application/msaccess" },
			{ "accde", "application/msaccess" },
			{ "accdt", "application/msaccess" },
			{ "acx", "application/internet-property-stream" },
			{ "afm", "application/octet-stream" },
			{ "ai", "application/postscript" },
			{ "aif", "audio/x-aiff" },
			{ "aifc", "audio/aiff" },
			{ "aiff", "audio/aiff" },
			{ "application", "application/x-ms-application" },
			{ "art", "image/x-jg" },
			{ "asd", "application/octet-stream" },
			{ "asf", "video/x-ms-asf" },
			{ "asi", "application/octet-stream" },
			{ "asm", "text/plain" },
			{ "asr", "video/x-ms-asf" },
			{ "asx", "video/x-ms-asf" },
			{ "atom", "application/atom+xml" },
			{ "au", "audio/basic" },
			{ "avi", "video/x-msvideo" },
			{ "axs", "application/olescript" },
			{ "bas", "text/plain" },
			{ "bcpio", "application/x-bcpio" },
			{ "bin", "application/octet-stream" },
			{ "bmp", "image/bmp" },
			{ "c", "text/plain" },
			{ "cab", "application/octet-stream" },
			{ "calx", "application/vnd.ms-office.calx" },
			{ "cat", "application/vnd.ms-pki.seccat" },
			{ "cdf", "application/x-cdf" },
			{ "chm", "application/octet-stream" },
			{ "class", "application/x-java-applet" },
			{ "clp", "application/x-msclip" },
			{ "cmx", "image/x-cmx" },
			{ "cnf", "text/plain" },
			{ "cod", "image/cis-cod" },
			{ "cpio", "application/x-cpio" },
			{ "cpp", "text/plain" },
			{ "crd", "application/x-mscardfile" },
			{ "crl", "application/pkix-crl" },
			{ "crt", "application/x-x509-ca-cert" },
			{ "csh", "application/x-csh" },
			{ "css", "text/css" },
			{ "csv", "application/octet-stream" },
			{ "cur", "application/octet-stream" },
			{ "dcr", "application/x-director" },
			{ "deploy", "application/octet-stream" },
			{ "der", "application/x-x509-ca-cert" },
			{ "dib", "image/bmp" },
			{ "dir", "application/x-director" },
			{ "disco", "text/xml" },
			{ "dll", "application/x-msdownload" },
			{ "dll.config", "text/xml" },
			{ "dlm", "text/dlm" },
			{ "doc", "application/msword" },
			{ "docm", "application/vnd.ms-word.document.macroEnabled.12" },
			{ "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
			{ "dot", "application/msword" },
			{ "dotm", "application/vnd.ms-word.template.macroEnabled.12" },
			{ "dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
			{ "dsp", "application/octet-stream" },
			{ "dtd", "text/xml" },
			{ "dvi", "application/x-dvi" },
			{ "dwf", "drawing/x-dwf" },
			{ "dwp", "application/octet-stream" },
			{ "dxr", "application/x-director" },
			{ "eml", "message/rfc822" },
			{ "emz", "application/octet-stream" },
			{ "eot", "application/octet-stream" },
			{ "eps", "application/postscript" },
			{ "etx", "text/x-setext" },
			{ "evy", "application/envoy" },
			{ "exe", "application/octet-stream" },
			{ "exe.config", "text/xml" },
			{ "fdf", "application/vnd.fdf" },
			{ "fif", "application/fractals" },
			{ "fla", "application/octet-stream" },
			{ "flr", "x-world/x-vrml" },
			{ "flv", "video/x-flv" },
			{ "gif", "image/gif" },
			{ "gtar", "application/x-gtar" },
			{ "gz", "application/x-gzip" },
			{ "h", "text/plain" },
			{ "hdf", "application/x-hdf" },
			{ "hdml", "text/x-hdml" },
			{ "hhc", "application/x-oleobject" },
			{ "hhk", "application/octet-stream" },
			{ "hhp", "application/octet-stream" },
			{ "hlp", "application/winhlp" },
			{ "hqx", "application/mac-binhex40" },
			{ "hta", "application/hta" },
			{ "htc", "text/x-component" },
			{ "htm", "text/html" },
			{ "html", "text/html" },
			{ "htt", "text/webviewhtml" },
			{ "hxt", "text/html" },
			{ "ico", "image/x-icon" },
			{ "ics", "application/octet-stream" },
			{ "ief", "image/ief" },
			{ "iii", "application/x-iphone" },
			{ "inf", "application/octet-stream" },
			{ "ins", "application/x-internet-signup" },
			{ "isp", "application/x-internet-signup" },
			{ "IVF", "video/x-ivf" },
			{ "jar", "application/java-archive" },
			{ "java", "application/octet-stream" },
			{ "jck", "application/liquidmotion" },
			{ "jcz", "application/liquidmotion" },
			{ "jfif", "image/pjpeg" },
			{ "jpb", "application/octet-stream" },
			{ "jpe", "image/jpeg" },
			{ "jpeg", "image/jpeg" },
			{ "jpg", "image/jpeg" },
			{ "js", "application/x-javascript" },
			{ "jsx", "text/jscript" },
			{ "latex", "application/x-latex" },
			{ "lit", "application/x-ms-reader" },
			{ "lpk", "application/octet-stream" },
			{ "lsf", "video/x-la-asf" },
			{ "lsx", "video/x-la-asf" },
			{ "lzh", "application/octet-stream" },
			{ "m13", "application/x-msmediaview" },
			{ "m14", "application/x-msmediaview" },
			{ "m1v", "video/mpeg" },
			{ "m3u", "audio/x-mpegurl" },
			{ "man", "application/x-troff-man" },
			{ "manifest", "application/x-ms-manifest" },
			{ "map", "text/plain" },
			{ "mdb", "application/x-msaccess" },
			{ "mdp", "application/octet-stream" },
			{ "me", "application/x-troff-me" },
			{ "mht", "message/rfc822" },
			{ "mhtml", "message/rfc822" },
			{ "mid", "audio/mid" },
			{ "midi", "audio/mid" },
			{ "mix", "application/octet-stream" },
			{ "mmf", "application/x-smaf" },
			{ "mno", "text/xml" },
			{ "mny", "application/x-msmoney" },
			{ "mov", "video/quicktime" },
			{ "movie", "video/x-sgi-movie" },
			{ "mp2", "video/mpeg" },
			{ "mp3", "audio/mpeg" },
			{ "mpa", "video/mpeg" },
			{ "mpe", "video/mpeg" },
			{ "mpeg", "video/mpeg" },
			{ "mpg", "video/mpeg" },
			{ "mpp", "application/vnd.ms-project" },
			{ "mpv2", "video/mpeg" },
			{ "ms", "application/x-troff-ms" },
			{ "msi", "application/octet-stream" },
			{ "mso", "application/octet-stream" },
			{ "mvb", "application/x-msmediaview" },
			{ "mvc", "application/x-miva-compiled" },
			{ "nc", "application/x-netcdf" },
			{ "nsc", "video/x-ms-asf" },
			{ "nws", "message/rfc822" },
			{ "ocx", "application/octet-stream" },
			{ "oda", "application/oda" },
			{ "odc", "text/x-ms-odc" },
			{ "ods", "application/oleobject" },
			{ "one", "application/onenote" },
			{ "onea", "application/onenote" },
			{ "onetoc", "application/onenote" },
			{ "onetoc2", "application/onenote" },
			{ "onetmp", "application/onenote" },
			{ "onepkg", "application/onenote" },
			{ "osdx", "application/opensearchdescription+xml" },
			{ "p10", "application/pkcs10" },
			{ "p12", "application/x-pkcs12" },
			{ "p7b", "application/x-pkcs7-certificates" },
			{ "p7c", "application/pkcs7-mime" },
			{ "p7m", "application/pkcs7-mime" },
			{ "p7r", "application/x-pkcs7-certreqresp" },
			{ "p7s", "application/pkcs7-signature" },
			{ "pbm", "image/x-portable-bitmap" },
			{ "pcx", "application/octet-stream" },
			{ "pcz", "application/octet-stream" },
			{ "pdf", "application/pdf" },
			{ "pfb", "application/octet-stream" },
			{ "pfm", "application/octet-stream" },
			{ "pfx", "application/x-pkcs12" },
			{ "pgm", "image/x-portable-graymap" },
			{ "pko", "application/vnd.ms-pki.pko" },
			{ "pma", "application/x-perfmon" },
			{ "pmc", "application/x-perfmon" },
			{ "pml", "application/x-perfmon" },
			{ "pmr", "application/x-perfmon" },
			{ "pmw", "application/x-perfmon" },
			{ "png", "image/png" },
			{ "pnm", "image/x-portable-anymap" },
			{ "pnz", "image/png" },
			{ "pot", "application/vnd.ms-powerpoint" },
			{ "potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
			{ "potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
			{ "ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
			{ "ppm", "image/x-portable-pixmap" },
			{ "pps", "application/vnd.ms-powerpoint" },
			{ "ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
			{ "ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
			{ "ppt", "application/vnd.ms-powerpoint" },
			{ "pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
			{ "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
			{ "prf", "application/pics-rules" },
			{ "prm", "application/octet-stream" },
			{ "prx", "application/octet-stream" },
			{ "ps", "application/postscript" },
			{ "psd", "application/octet-stream" },
			{ "psm", "application/octet-stream" },
			{ "psp", "application/octet-stream" },
			{ "pub", "application/x-mspublisher" },
			{ "qt", "video/quicktime" },
			{ "qtl", "application/x-quicktimeplayer" },
			{ "qxd", "application/octet-stream" },
			{ "ra", "audio/x-pn-realaudio" },
			{ "ram", "audio/x-pn-realaudio" },
			{ "rar", "application/octet-stream" },
			{ "ras", "image/x-cmu-raster" },
			{ "rf", "image/vnd.rn-realflash" },
			{ "rgb", "image/x-rgb" },
			{ "rm", "application/vnd.rn-realmedia" },
			{ "rmi", "audio/mid" },
			{ "roff", "application/x-troff" },
			{ "rpm", "audio/x-pn-realaudio-plugin" },
			{ "rtf", "application/rtf" },
			{ "rtx", "text/richtext" },
			{ "scd", "application/x-msschedule" },
			{ "sct", "text/scriptlet" },
			{ "sea", "application/octet-stream" },
			{ "setpay", "application/set-payment-initiation" },
			{ "setreg", "application/set-registration-initiation" },
			{ "sgml", "text/sgml" },
			{ "sh", "application/x-sh" },
			{ "shar", "application/x-shar" },
			{ "sit", "application/x-stuffit" },
			{ "sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
			{ "sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
			{ "smd", "audio/x-smd" },
			{ "smi", "application/octet-stream" },
			{ "smx", "audio/x-smd" },
			{ "smz", "audio/x-smd" },
			{ "snd", "audio/basic" },
			{ "snp", "application/octet-stream" },
			{ "spc", "application/x-pkcs7-certificates" },
			{ "spl", "application/futuresplash" },
			{ "src", "application/x-wais-source" },
			{ "ssm", "application/streamingmedia" },
			{ "sst", "application/vnd.ms-pki.certstore" },
			{ "stl", "application/vnd.ms-pki.stl" },
			{ "sv4cpio", "application/x-sv4cpio" },
			{ "sv4crc", "application/x-sv4crc" },
			{ "swf", "application/x-shockwave-flash" },
			{ "t", "application/x-troff" },
			{ "tar", "application/x-tar" },
			{ "tcl", "application/x-tcl" },
			{ "tex", "application/x-tex" },
			{ "texi", "application/x-texinfo" },
			{ "texinfo", "application/x-texinfo" },
			{ "tgz", "application/x-compressed" },
			{ "thmx", "application/vnd.ms-officetheme" },
			{ "thn", "application/octet-stream" },
			{ "tif", "image/tiff" },
			{ "tiff", "image/tiff" },
			{ "toc", "application/octet-stream" },
			{ "tr", "application/x-troff" },
			{ "trm", "application/x-msterminal" },
			{ "tsv", "text/tab-separated-values" },
			{ "ttf", "application/octet-stream" },
			{ "txt", "text/plain" },
			{ "u32", "application/octet-stream" },
			{ "uls", "text/iuls" },
			{ "ustar", "application/x-ustar" },
			{ "vbs", "text/vbscript" },
			{ "vcf", "text/x-vcard" },
			{ "vcs", "text/plain" },
			{ "vdx", "application/vnd.ms-visio.viewer" },
			{ "vml", "text/xml" },
			{ "vsd", "application/vnd.visio" },
			{ "vss", "application/vnd.visio" },
			{ "vst", "application/vnd.visio" },
			{ "vsto", "application/x-ms-vsto" },
			{ "vsw", "application/vnd.visio" },
			{ "vsx", "application/vnd.visio" },
			{ "vtx", "application/vnd.visio" },
			{ "wav", "audio/wav" },
			{ "wax", "audio/x-ms-wax" },
			{ "wbmp", "image/vnd.wap.wbmp" },
			{ "wcm", "application/vnd.ms-works" },
			{ "wdb", "application/vnd.ms-works" },
			{ "wks", "application/vnd.ms-works" },
			{ "wm", "video/x-ms-wm" },
			{ "wma", "audio/x-ms-wma" },
			{ "wmd", "application/x-ms-wmd" },
			{ "wmf", "application/x-msmetafile" },
			{ "wml", "text/vnd.wap.wml" },
			{ "wmlc", "application/vnd.wap.wmlc" },
			{ "wmls", "text/vnd.wap.wmlscript" },
			{ "wmlsc", "application/vnd.wap.wmlscriptc" },
			{ "wmp", "video/x-ms-wmp" },
			{ "wmv", "video/x-ms-wmv" },
			{ "wmx", "video/x-ms-wmx" },
			{ "wmz", "application/x-ms-wmz" },
			{ "wps", "application/vnd.ms-works" },
			{ "wri", "application/x-mswrite" },
			{ "wrl", "x-world/x-vrml" },
			{ "wrz", "x-world/x-vrml" },
			{ "wsdl", "text/xml" },
			{ "wvx", "video/x-ms-wvx" },
			{ "x", "application/directx" },
			{ "xaf", "x-world/x-vrml" },
			{ "xaml", "application/xaml+xml" },
			{ "xap", "application/x-silverlight-app" },
			{ "xbap", "application/x-ms-xbap" },
			{ "xbm", "image/x-xbitmap" },
			{ "xdr", "text/plain" },
			{ "xla", "application/vnd.ms-excel" },
			{ "xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
			{ "xlc", "application/vnd.ms-excel" },
			{ "xlm", "application/vnd.ms-excel" },
			{ "xls", "application/vnd.ms-excel" },
			{ "xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
			{ "xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
			{ "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
			{ "xlt", "application/vnd.ms-excel" },
			{ "xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
			{ "xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
			{ "xlw", "application/vnd.ms-excel" },
			{ "xml", "text/xml" },
			{ "xof", "x-world/x-vrml" },
			{ "xpm", "image/x-xpixmap" },
			{ "xps", "application/vnd.ms-xpsdocument" },
			{ "xsd", "text/xml" },
			{ "xsf", "text/xml" },
			{ "xsl", "text/xml" },
			{ "xslt", "text/xml" },
			{ "xsn", "application/octet-stream" },
			{ "xtp", "application/octet-stream" },
			{ "xwd", "image/x-xwindowdump" },
			{ "z", "application/x-compress" },
			{ "zip", "application/x-zip-compressed" },
		};

		#endregion
	}
}
