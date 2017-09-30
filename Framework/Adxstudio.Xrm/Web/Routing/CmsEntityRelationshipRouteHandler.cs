/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Handlers;
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
	public class CmsEntityRelationshipRouteHandler : IRouteHandler
	{
		public const string RoutePath = "_services/portal/{__portalScopeId__}/{entityLogicalName}/{id}/__related/{relationshipSchemaName}";

		public CmsEntityRelationshipRouteHandler(string portalName)
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

			var relationshipSchemaName = requestContext.RouteData.Values["relationshipSchemaName"] as string;

			return new CmsEntityRelationshipHandler(PortalName, portalScopeId, entityLogicalName, id, relationshipSchemaName);
		}

		public static string GetAppRelativePath(Guid portalScopeId, EntityReference entity, Relationship relationship)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (relationship == null)
			{
				throw new ArgumentNullException("relationship");
			}

			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__",      portalScopeId.ToString()      },
				{ "entityLogicalName",      entity.LogicalName            },
				{ "id",                     entity.Id.ToString()          },
				{ "relationshipSchemaName", relationship.ToSchemaName(".") },
			});

			return "~{0}".FormatWith(uri.PathAndQuery);
		}

		public static string GetAppRelativePathTemplate(Guid portalScopeId, string entityLogicalName, string idTemplateVariableName, Relationship relationship)
		{
			var idPlaceholder = "__{0}__".FormatWith(Guid.NewGuid());

			var uriTemplate = new UriTemplate(RoutePath);

			var uri = uriTemplate.BindByName(new Uri("http://localhost/"), new Dictionary<string, string>
			{
				{ "__portalScopeId__",      portalScopeId.ToString()      },
				{ "entityLogicalName",      entityLogicalName             },
				{ "id",                     idPlaceholder                 },
				{ "relationshipSchemaName", relationship.ToSchemaName(".") },
			});

			var path = "~{0}".FormatWith(uri.PathAndQuery);

			return path.Replace(idPlaceholder, "{{{0}}}".FormatWith(idTemplateVariableName));
		}
	}
}
