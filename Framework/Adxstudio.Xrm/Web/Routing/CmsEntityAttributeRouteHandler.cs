/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Handlers;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Modules;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	public class CmsEntityAttributeRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/portal/{__portalScopeId__}/{entityLogicalName}/{id}/{attributeLogicalName}";

		public CmsEntityAttributeRouteHandler(string portalName)
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

			Guid parsedId;
			var id = Guid.TryParse(requestContext.RouteData.Values["id"] as string, out parsedId) ? new Guid?(parsedId) : null;

			var attributeLogicalName = requestContext.RouteData.Values["attributeLogicalName"] as string;

			return new CmsEntityAttributeHandler(PortalName, portalScopeId, entityLogicalName, id, attributeLogicalName);
		}

		public static string GetAppRelativePath(Guid portalScopeId, EntityReference entity, string attributeLogicalName)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (string.IsNullOrWhiteSpace(attributeLogicalName))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "attributeLogicalName");
			}

			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__",    portalScopeId.ToString() },
				{ "entityLogicalName",    entity.LogicalName      },
				{ "id",                   entity.Id.ToString()    },
				{ "attributeLogicalName", attributeLogicalName    },
			});

			return "~{0}".FormatWith(uri.PathAndQuery);
		}

		public static string GetAppRelativePathTemplate(Guid portalScopeId, string entityLogicalName, string idTemplateVariableName, string attributeLogicalName)
		{
			var idPlaceholder = "__{0}__".FormatWith(Guid.NewGuid());

			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__",    portalScopeId.ToString() },
				{ "entityLogicalName",    entityLogicalName       },
				{ "id",                   idPlaceholder           },
				{ "attributeLogicalName", attributeLogicalName    },
			});

			var path = "~{0}".FormatWith(uri.PathAndQuery);

			return path.Replace(idPlaceholder, "{{{0}}}".FormatWith(idTemplateVariableName));
		}
	}
}
