/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;

namespace Adxstudio.Xrm.Blogs
{
	public class PortalConfigurationDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalConfigurationDataAdapterDependencies(string portalName = null, RequestContext requestContext = null) : base(
			PortalCrmConfigurationManager.CreateServiceContext(portalName),
			PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
			PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
			PortalCrmConfigurationManager.CreatePortalContext(portalName, requestContext),
			requestContext) { }
	}
}
