/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Handlers;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Routing
{
	public class CmsEntityUrlRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/portal/{__portalScopeId__}/{entityLogicalName}/{id}/__url";

		public CmsEntityUrlRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var antiForgeryTokenValidator = new AjaxValidateAntiForgeryTokenAttribute();
			antiForgeryTokenValidator.Validate(requestContext);
			Guid parsedPortalScopeId;
			var portalScopeId = Guid.TryParse(requestContext.RouteData.Values["__portalScopeId__"] as string, out parsedPortalScopeId)
				? new Guid?(parsedPortalScopeId)
				: null;

			var entityLogicalName = requestContext.RouteData.Values["entityLogicalName"] as string;

			Guid parsedId;
			var id = Guid.TryParse(requestContext.RouteData.Values["id"] as string, out parsedId) ? new Guid?(parsedId) : null;

			return new CmsEntityUrlHandler(PortalName, portalScopeId, entityLogicalName, id);
		}

		public static string GetAppRelativePath(Guid portalScopeId, EntityReference entity)
		{
			return CmsEntityRouteHandler.GetAppRelativePath(RoutePath, portalScopeId, entity);
		}

		public static string GetAppRelativePathTemplate(Guid portalScopeId, string entityLogicalName, string idTemplateVariableName)
		{
			return CmsEntityRouteHandler.GetAppRelativePathTemplate(RoutePath, portalScopeId, entityLogicalName, idTemplateVariableName);
		}
	}
}
