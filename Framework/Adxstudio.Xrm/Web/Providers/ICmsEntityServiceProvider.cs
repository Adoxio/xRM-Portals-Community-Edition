/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;
using Adxstudio.Xrm.Web.Handlers;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Providers
{
	public enum CmsEntityOperation { Create, Update }

	public interface ICmsEntityServiceProvider
	{
		Entity ExecuteEntityQuery(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, EntityReference entityReference, CmsEntityMetadata entityMetadata);

		IEnumerable<Entity> ExecuteEntitySetQuery(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, string entityLogicalName, CmsEntityMetadata entityMetadata, string filter = null);

		void InterceptChange(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, CmsEntityOperation operation, Entity preImage = null);

		void ExtendEntityJson(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, JObject extensions);

		void InterceptExtensionChange(HttpContext context, IPortalContext portal, OrganizationServiceContext serviceContext, Entity entity, CmsEntityMetadata entityMetadata, JObject extensions, CmsEntityOperation operation);
	}
}
