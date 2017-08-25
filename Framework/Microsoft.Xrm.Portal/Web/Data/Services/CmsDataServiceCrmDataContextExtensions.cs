/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public static class CmsDataServiceCrmDataContextExtensions
	{
		public static IQueryable<TEntity> InterceptQuery<TEntity>(this OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			// TODO: make the portalName configurable
			string portalName = null;
			var provider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<ICmsDataServiceQueryInterceptorProvider>();
			var interceptor = provider.GetInterceptor();

			return interceptor.InterceptQuery(context, queryable);
		}
	}
}
