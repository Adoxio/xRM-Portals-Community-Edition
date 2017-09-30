/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Providers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityAttributeHandler : CmsEntityHandler
	{
		public CmsEntityAttributeHandler() : this(null, null, null, null, null) { }

		public CmsEntityAttributeHandler(string portalName, Guid? portalScopeId, string entityLogicalName, Guid? id, string attributeLogicalName) : base(portalName, portalScopeId, entityLogicalName, id)
		{
			AttributeLogicalName = attributeLogicalName;
		}

		/// <summary>
		/// The CRM logical name of the attribute being requested.
		/// </summary>
		protected virtual string AttributeLogicalName { get; private set; }

		protected override void ProcessRequest(HttpContext context, ICmsEntityServiceProvider serviceProvider, Guid portalScopeId, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, ICrmEntitySecurityProvider security)
		{
			var attributeLogicalName = string.IsNullOrWhiteSpace(AttributeLogicalName) ? context.Request.Params["attributeLogicalName"] : AttributeLogicalName;

			if (string.IsNullOrWhiteSpace(attributeLogicalName))
			{
				throw new CmsEntityServiceException(HttpStatusCode.BadRequest, "Unable to determine entity attribute logical name from request.");
			}

			if (entityMetadata.HasAttribute(AttributeLogicalName))
			{
				if (!IsRequestMethod(context.Request, "GET"))
				{
					throw new CmsEntityServiceException(HttpStatusCode.MethodNotAllowed, "Request method {0} not allowed for this resource.".FormatWith(context.Request.HttpMethod));
				}

				var value = entity.Attributes.Contains(attributeLogicalName) ? entity.Attributes[attributeLogicalName] : null;

				var json = new JObject
				{
					{
						"d", new JObject
						{
							{ attributeLogicalName, GetValueJson(value) }
						}
					},
				};

				WriteResponse(context.Response, json);

				return;
			}

			throw new CmsEntityServiceException(HttpStatusCode.NotFound, "Entity attribute not found.");
		}
	}
}
