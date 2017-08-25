/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms
{
	public class PortalContextDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalContextDataAdapterDependencies(IPortalContext portalContext, string portalName = null, RequestContext requestContext = null)
			: base(
			portalContext.ServiceContext,
			PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
			PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
			portalContext,
			requestContext)
		{
			PortalName = portalName;
		}

		protected new string PortalName { get; private set; }

		public override OrganizationServiceContext GetServiceContextForWrite()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}
	}
}
