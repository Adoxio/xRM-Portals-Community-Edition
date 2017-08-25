/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Providers;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	/// <summary>
	/// Provides dependencies pulled from an <see cref="IPortalContext"/> for the various data adapters within the Adxstudio.Xrm.KnowledgeArticles namespace.
	/// </summary>
	public class PortalContextDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalContextDataAdapterDependencies(
			IPortalContext portalContext, string 
			portalName = null,
			RequestContext requestContext = null)
			: base(
				portalContext.ServiceContext,
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
				portalContext,
				portalName,
				requestContext)
		{
		}
	}
}
