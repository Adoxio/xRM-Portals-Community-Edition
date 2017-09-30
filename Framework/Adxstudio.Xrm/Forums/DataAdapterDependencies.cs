/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	public class DataAdapterDependencies : Cms.DataAdapterDependencies, IDataAdapterDependencies
	{
		private readonly IForumCounterStrategy _counterStrategy = new AttributeWithFetchFallbackCounterStrategy();
		private readonly ILatestPostUrlProvider _postUrlProvider;
		private RequestContext _requestContext;

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser, ILatestPostUrlProvider postUrlProvider = null, string portalName = null, RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, website, portalUser)
		{
			_postUrlProvider = postUrlProvider ?? new AnchorLatestPostUrlProvider();
			PortalName = portalName;
			_requestContext = requestContext;
		}

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, IPortalContext portalContext, ILatestPostUrlProvider postUrlProvider = null, string portalName = null, RequestContext requestContext = null)
			: base(serviceContext, securityProvider, urlProvider, portalContext)
		{
			_postUrlProvider = postUrlProvider ?? new AnchorLatestPostUrlProvider();
			PortalName = portalName;
			_requestContext = requestContext;
		}

		public IForumCounterStrategy GetCounterStrategy()
		{
			return _counterStrategy;
		}

		public new ApplicationPath GetDeletePath(EntityReference entity)
		{
			if (entity == null) return null;

			var website = GetWebsite();

			if (website == null) return null;

			var requestContext = GetRequestContext();

			try
			{
				var pathData = RouteTable.Routes.GetVirtualPath(requestContext, typeof(CmsEntityDeleteRouteHandler).FullName,
					new RouteValueDictionary {
						{ "__portalScopeId__", website.Id.ToString() },
						{ "entityLogicalName", entity.LogicalName },
						{ "id", entity.Id.ToString() },
					});
					
				return pathData == null ? null : ApplicationPath.FromAbsolutePath(VirtualPathUtility.ToAbsolute(pathData.VirtualPath));
			}
			catch (ArgumentException)
			{
				return null;
			}
		}

		public new ApplicationPath GetEditPath(EntityReference entity)
		{
			if (entity == null) return null;

			var website = GetWebsite();

			if (website == null) return null;

			var requestContext = GetRequestContext();

			try
			{
				var pathData = RouteTable.Routes.GetVirtualPath(requestContext, typeof(CmsEntityRouteHandler).FullName, 
					new RouteValueDictionary {
						{ "__portalScopeId__", website.Id.ToString() },
						{ "entityLogicalName", entity.LogicalName },
						{ "id", entity.Id.ToString() },
					});
					
				return pathData == null ? null : ApplicationPath.FromAbsolutePath(VirtualPathUtility.ToAbsolute(pathData.VirtualPath));
			}
			catch (ArgumentException)
			{
				return null;
			}
		}

		public ILatestPostUrlProvider GetLatestPostUrlProvider()
		{
			return _postUrlProvider;
		}

		public override OrganizationServiceContext GetServiceContextForWrite()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		protected new RequestContext GetRequestContext()
		{
			if (_requestContext != null)
			{
				return _requestContext;
			}

			_requestContext = GetCurrentRequestContext();

			return _requestContext;
		}

		private static RequestContext GetCurrentRequestContext()
		{
			var current = HttpContext.Current;

			if (current == null)
			{
				return null;
			}

			var http = new HttpContextWrapper(current);
			var routeData = RouteTable.Routes.GetRouteData(http) ?? new RouteData();

			return new RequestContext(http, routeData);
		}
	}

	public class PortalContextDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalContextDataAdapterDependencies(IPortalContext portalContext, ILatestPostUrlProvider postUrlProvider = null, string portalName = null, RequestContext requestContext = null)
			: base(
				portalContext.ServiceContext,
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
				portalContext,
				postUrlProvider,
				portalName,
				requestContext) { }
	}

	public class PortalConfigurationDataAdapterDependencies : DataAdapterDependencies
	{
		public PortalConfigurationDataAdapterDependencies(ILatestPostUrlProvider postUrlProvider = null, string portalName = null, RequestContext requestContext = null)
			: base(
				PortalCrmConfigurationManager.CreateServiceContext(portalName),
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
				PortalCrmConfigurationManager.CreatePortalContext(portalName, requestContext),
				postUrlProvider,
				portalName,
				requestContext) { }
	}
}
