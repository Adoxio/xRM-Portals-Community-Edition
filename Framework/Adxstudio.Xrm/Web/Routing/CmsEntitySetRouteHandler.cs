/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Handlers;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Modules;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	public class CmsEntitySetRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/portal/{__portalScopeId__}/{entityLogicalName}";

		public CmsEntitySetRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns></returns>
		public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var antiForgeryTokenValidator = new AjaxValidateAntiForgeryTokenAttribute();
			antiForgeryTokenValidator.Validate(requestContext);
			Guid parsedPortalScopeId;
			var portalScopeId = Guid.TryParse(requestContext.RouteData.Values["__portalScopeId__"] as string, out parsedPortalScopeId)
				? new Guid?(parsedPortalScopeId)
				: null;

			var entityLogicalName = requestContext.RouteData.Values["entityLogicalName"] as string;

			return new CmsEntitySetHandler(PortalName, portalScopeId, entityLogicalName);
		}

		public static string GetAppRelativePath(Guid portalScopeId, string entityLogicalName)
		{
			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "entityLogicalName");
			}

			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__", portalScopeId.ToString() },
				{ "entityLogicalName", entityLogicalName       },
			});

			return "~{0}".FormatWith(uri.PathAndQuery);
		}
	}
}
