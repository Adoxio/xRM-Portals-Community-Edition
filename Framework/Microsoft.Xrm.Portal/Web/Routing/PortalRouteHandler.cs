/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Modules;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.Routing
{
	/// <summary>
	/// Handles requests to portal <see cref="Entity"/> objects.
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	/// <seealso cref="AnnotationHandler"/>
	public class PortalRouteHandler : IRouteHandler // MSBug #120045: Won't seal, inheritance is used extension point.
	{
		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public PortalRouteHandler(string portalName)
		{
			PortalName = portalName;
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns></returns>
		public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName, requestContext);

			portal = OnPortalLoaded(requestContext, portal);

			if (portal == null) return null;

			var isInvalidNode = portal.Entity == null || portal.Path == null;

			// there's nothing else we can really do--we'll exit with a bare-bones 404.
			if (isInvalidNode)
			{
				Tracing.FrameworkInformation("PortalRouteHandler", "GetHttpHandler", "FindSiteMapNode failed to find a non-null CrmSiteMapNode. Responding with a basic 404.");

				RenderNotFound(requestContext);

				return null;
			}

			if (portal.StatusCode == HttpStatusCode.NotFound)
			{
				var response = requestContext.HttpContext.Response;
				response.StatusCode = (int)HttpStatusCode.NotFound;
			}

			IHttpHandler handler;

			if (TryCreateHandler(portal, out handler))
			{
				return handler;
			}

			// remove the querystring

			var rewritePath = portal.Path;
			var index = rewritePath.IndexOf("?");
			var path = index > -1 ? rewritePath.Substring(0, index) : rewritePath;

			Tracing.FrameworkInformation("PortalRouteHandler", "GetHttpHandler", "path={0}".FormatWith(path));

			return CreateHandlerFromVirtualPath(path);
		}

		protected virtual bool TryCreateHandler(IPortalContext portal, out IHttpHandler handler)
		{
			if (string.Equals(portal.Entity.LogicalName, "adx_webfile", StringComparison.InvariantCulture))
			{
				handler = CreateAnnotationHandler(portal.ServiceContext.GetNote(portal.Entity));
				return true;
			}

			if (string.Equals(portal.Entity.LogicalName, "annotation", StringComparison.InvariantCulture))
			{
				handler = CreateAnnotationHandler(portal.Entity);
				return true;
			}

			handler = null;
			return false;
		}

		protected virtual IHttpHandler CreateAnnotationHandler(Entity entity)
		{
			return new AnnotationHandler(entity);
		}

		protected virtual void RenderNotFound(RequestContext requestContext)
		{
			var response = requestContext.HttpContext.Response;

			response.StatusCode = 404;
			response.ContentType = "text/plain";
			response.Write("Not Found");
			response.End();
		}

		protected IHttpHandler CreateHandlerFromVirtualPath(string path)
		{
			return BuildManager.CreateInstanceFromVirtualPath(path, typeof(Page)) as IHttpHandler;
		}

		protected virtual IPortalContext OnPortalLoaded(RequestContext requestContext, IPortalContext portal)
		{
			return portal;
		}
	}
}
