/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityHandler : CmsEntitySetHandler
	{
		public CmsEntityHandler() : this(null, null, null, null) { }

		public CmsEntityHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id) : base(portalName, portalScopeId, entityLogicalName)
		{
			Id = id;
		}

		/// <summary>
		/// The ID of the entity whose attribute is being requested.
		/// </summary>
		protected virtual Guid? Id { get; private set; }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, string entityLogicalName)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "entityLogicalName");
			}

			Guid parsedId;
			var id = Id ?? (Guid.TryParse(context.Request.Params["id"], out parsedId) ? new Guid?(parsedId) : null);

			if (id == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity ID from request.");
			}

			ProcessRequest(context, serviceProvider, portalScopeId, portal, serviceContext, new EntityReference(entityLogicalName, id.Value));
		}

		protected virtual void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference entityReference)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			if (portal == null)
			{
				throw new ArgumentNullException("portal");
			}

			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (entityReference == null)
			{
				throw new ArgumentNullException("entityReference");
			}

			var entityMetadata = new CmsEntityMetadata(serviceContext, entityReference.LogicalName);

			var entity = serviceProvider.ExecuteEntityQuery(context, portal, serviceContext, entityReference, entityMetadata);

			if (entity == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.NotFound, "Entity not found.");
			}

			var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			AssertRequestEntitySecurity(portal, serviceContext, entity, security);

			ProcessRequest(context, serviceProvider, portalScopeId, portal, serviceContext, entity, entityMetadata, security);
		}

		protected virtual void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			if (IsRequestMethod(context.Request, "GET"))
			{
				WriteResponse(context.Response, new JObject
				{
					{ "d", GetEntityJson(context, serviceProvider, portalScopeId, portal, serviceContext, entity, entityMetadata) }
				});

				return;
			}

			if (IsRequestMethod(context.Request, "POST"))
			{
				var preImage = entity.Clone(false);

				var extensions = UpdateEntityFromJsonRequestBody(context.Request, serviceContext, entity, entityMetadata);

				serviceProvider.InterceptChange(context, portal, serviceContext, entity, entityMetadata, CmsEntityOperation.Update, preImage);

				serviceContext.UpdateObject(entity);

				serviceProvider.InterceptExtensionChange(context, portal, serviceContext, entity, entityMetadata, extensions, CmsEntityOperation.Update);

				serviceContext.SaveChanges();

				WriteNoContentResponse(context.Response);

				return;
			}

			throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
		}

		protected virtual void AssertRequestEntitySecurity(IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, ICrmEntitySecurityProvider security)
		{
			if (!security.TryAssert(serviceContext, entity, CrmEntityRight.Change))
			{
				throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Entity access denied.");
			}
		}
	}
}
