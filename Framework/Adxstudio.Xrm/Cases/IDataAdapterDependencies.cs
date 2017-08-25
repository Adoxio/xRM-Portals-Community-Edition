/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cases
{
	public interface IDataAdapterDependencies : Cms.IDataAdapterDependencies
	{
		ICaseAccessPermissionScopesProvider GetPermissionScopesProviderForPortalUser();
	}

	public class DataAdapterDependencies : Cms.DataAdapterDependencies, IDataAdapterDependencies
	{
		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser, string portalName = null, RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, website, portalUser, requestContext)
		{
			PortalName = portalName;
		}

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, IPortalContext portalContext, string portalName = null, RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, portalContext, requestContext)
		{
			PortalName = portalName;
		}

		protected new string PortalName { get; private set; }

		public ICaseAccessPermissionScopesProvider GetPermissionScopesProviderForPortalUser()
		{
			var user = GetPortalUser();

			return user == null
				? (ICaseAccessPermissionScopesProvider)new NoCaseAccessPermissionScopesProvider()
				: new ContactCaseAccessPermissionScopesProvider(user, this);
		}

		public override OrganizationServiceContext GetServiceContextForWrite()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}
	}

	public class PortalContextDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalContextDataAdapterDependencies(IPortalContext portalContext, string portalName = null, RequestContext requestContext = null)
			: base(
				portalContext.ServiceContext,
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
				portalContext,
				portalName,
				requestContext) { }
	}

	public class PortalConfigurationDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalConfigurationDataAdapterDependencies(string portalName = null, RequestContext requestContext = null)
			: base(
				PortalCrmConfigurationManager.CreateServiceContext(portalName),
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
				PortalCrmConfigurationManager.CreatePortalContext(portalName, requestContext),
				portalName,
				requestContext) { }
	}
}
