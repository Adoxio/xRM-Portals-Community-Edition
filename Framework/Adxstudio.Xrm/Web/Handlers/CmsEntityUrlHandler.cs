/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityUrlHandler : CmsEntityHandler
	{
		public CmsEntityUrlHandler() { }

		public CmsEntityUrlHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id) : base(portalName, portalScopeId, entityLogicalName, id) { }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference entityReference)
		{
			var entityMetadata = new CmsEntityMetadata(serviceContext, entityReference.LogicalName);

			var query = serviceContext.CreateQuery(entityReference.LogicalName);

			// If the target entity is scoped to a website, filter the query by the current website.
			if (entityMetadata.HasAttribute("adx_websiteid"))
			{
				query = query.Where(e => e.GetAttributeValue<EntityReference>("adx_websiteid") == portal.Website.ToEntityReference());
			}

			var entity = query.FirstOrDefault(e => e.GetAttributeValue<Guid>(entityMetadata.PrimaryIdAttribute) == entityReference.Id);

			if (entity == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.NotFound, "Entity not found.");
			}

			var security = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(PortalName);

			if (!security.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
				throw new CmsEntityServiceException(HttpStatusCode.Forbidden, "Entity access denied.");
			}

			var url = serviceContext.GetUrl(entity);

			if (url == null)
			{
				throw new CmsEntityServiceException(HttpStatusCode.NotFound, "URL for entity not found.");
			}

			WriteResponse(context.Response, new JObject
			{
				{ "d",  new JObject { { "Url", new JValue(url) } } }
			});
		}
	}
}
