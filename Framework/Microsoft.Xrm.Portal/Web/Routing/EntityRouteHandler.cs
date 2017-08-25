/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Modules;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Routing
{
	/// <summary>
	/// Handles requests to <see cref="Entity"/> objects.
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	/// <seealso cref="AnnotationHandler"/>
	public class EntityRouteHandler : IRouteHandler // MSBug #120045: Won't seal, inheritance is used extension point.
	{
		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public virtual string PortalName { get; private set; }

		public EntityRouteHandler(string portalName)
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
			var logicalName = requestContext.RouteData.Values["logicalName"] as string;
			var id = Guid.Empty;

			try
			{
				id = new Guid(requestContext.RouteData.Values["id"] as string);
			}
			catch { }

			if (!string.IsNullOrWhiteSpace(logicalName) && id != Guid.Empty)
			{
				var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

				IHttpHandler handler;

				if (TryCreateHandler(context, logicalName, id, out handler))
				{
					return handler;
				}
			}

			RenderNotFound(requestContext);

			return null;
		}

		protected virtual bool TryCreateHandler(OrganizationServiceContext context, string logicalName, Guid id, out IHttpHandler handler)
		{
			if (string.Equals(logicalName, "annotation", StringComparison.InvariantCulture))
			{
				var entity = context.CreateQuery(logicalName).FirstOrDefault(e => e.GetAttributeValue<Guid>("annotationid") == id);

				if (entity != null)
				{
					handler = CreateAnnotationHandler(entity);
					return true;
				}
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
	}
}
