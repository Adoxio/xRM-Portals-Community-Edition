/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	/// <summary>
	/// Server-side implementation of elFinder 1.2 connector protocol (http://elrte.org/redmine/projects/elfinder/wiki/Client-Server_Protocol_EN).
	/// </summary>
	public class ElFinderRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/portal/{__portalScopeId__}/elFinder/connector";
		public const string DialogPath = SiteSettings.XrmFilesRootPath + "/filebrowser.html";

		private readonly string _portalName;

		public ElFinderRouteHandler(string portalName)
		{
			_portalName = portalName;
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var antiForgeryTokenValidator = new AjaxValidateAntiForgeryTokenAttribute();
			antiForgeryTokenValidator.Validate(requestContext);
			return new HttpHandler(_portalName, requestContext);
		}

		public static string GetAppRelativePath(Guid portalScopeId)
		{
			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__", portalScopeId.ToString() },
			});

			return "~{0}".FormatWith(uri.PathAndQuery);
		}
	}
}
