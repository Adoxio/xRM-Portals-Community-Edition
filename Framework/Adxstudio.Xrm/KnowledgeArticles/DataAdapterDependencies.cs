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

namespace Adxstudio.Xrm.KnowledgeArticles
{
	/// <summary>
	/// Provides dependencies for the various data adapters within the Adxstudio.Xrm.KnowledgeArticles namespace.
	/// </summary>
	public class DataAdapterDependencies : Cms.DataAdapterDependencies, IDataAdapterDependencies
	{
		public DataAdapterDependencies(
			OrganizationServiceContext serviceContext, 
			ICrmEntitySecurityProvider securityProvider,
			IEntityUrlProvider urlProvider, 
			EntityReference website, 
			EntityReference portalUser, 
			string portalName = null,
			RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, website, portalUser, requestContext)
		{
			PortalName = portalName;
		}

		public DataAdapterDependencies(
			OrganizationServiceContext serviceContext, 
			ICrmEntitySecurityProvider securityProvider,
			IEntityUrlProvider urlProvider, 
			IPortalContext portalContext, 
			string portalName = null,
			RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, portalContext, requestContext)
		{
			PortalName = portalName;
		}

		public override OrganizationServiceContext GetServiceContextForWrite()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		protected new string PortalName { get; private set; }
	}
}
