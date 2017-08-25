/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Globalization;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Notes;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
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
		private readonly Entity _webfile;

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="annotation">Note</param>
		/// <param name="webfile">Web File</param>
		public AnnotationHandler(Entity annotation, Entity webfile)
		{
			_annotation = annotation;
			_webfile = webfile;
		}


		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="annotation">Note</param>
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

			string portalName = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var languageCodeSetting = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website,
				"Language Code");

			if (!string.IsNullOrWhiteSpace(languageCodeSetting))
			{
				int languageCode;
				if (int.TryParse(languageCodeSetting, out languageCode))
				{
					portalName = languageCode.ToString(CultureInfo.InvariantCulture);
				}
			}

			var dataAdapterDependencies =
				new PortalConfigurationDataAdapterDependencies(requestContext: context.Request.RequestContext,
					portalName: portalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);

			dataAdapter.Download(new HttpContextWrapper(context), _annotation, _webfile);
		}
	}
}
