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
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms
{
	public abstract class DataAdapterDependencies : IDataAdapterDependencies
	{
		private readonly EntityReference _portalUser;
		private readonly ICrmEntitySecurityProvider _securityProvider;
		private readonly OrganizationServiceContext _serviceContext;
		private readonly IEntityUrlProvider _urlProvider;
		private readonly EntityReference _website;
		private RequestContext _requestContext;

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider,
			IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser = null, RequestContext requestContext = null)
		{
			if (serviceContext == null)
			{
				throw new ArgumentNullException("serviceContext");
			}

			if (securityProvider == null)
			{
				throw new ArgumentNullException("securityProvider");
			}

			if (urlProvider == null)
			{
				throw new ArgumentNullException("urlProvider");
			}

			if (website == null)
			{
				throw new ArgumentNullException("website");
			}

			_serviceContext = serviceContext;
			_securityProvider = securityProvider;
			_urlProvider = urlProvider;
			_website = website;
			_portalUser = portalUser;
			_requestContext = requestContext;
		}

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider,
			IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser = null) : this
			(serviceContext, securityProvider, urlProvider, website, portalUser, null)
		{ }

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, 
			IEntityUrlProvider urlProvider, IPortalContext portalContext) : this(
			serviceContext,
			securityProvider,
			urlProvider,
			portalContext.Website == null ? null : portalContext.Website.ToEntityReference(),
			portalContext.User == null ? null : portalContext.User.ToEntityReference())
		{
			if (portalContext == null)
			{
				throw new ArgumentNullException("portalContext");
			}
		}

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider,
			IEntityUrlProvider urlProvider, IPortalContext portalContext, RequestContext requestContext = null)
			: this(
				serviceContext,
				securityProvider,
				urlProvider,
				portalContext.Website == null ? null : portalContext.Website.ToEntityReference(),
				portalContext.User == null ? null : portalContext.User.ToEntityReference(),
				requestContext)
		{
			if (portalContext == null)
			{
				throw new ArgumentNullException("portalContext");
			}
		}

		protected string PortalName { get; set; }

		public EntityReference GetPortalUser()
		{
			return _portalUser;
		}

		public ICrmEntitySecurityProvider GetSecurityProvider()
		{
			return _securityProvider;
		}

		public OrganizationServiceContext GetServiceContext()
		{
			return _serviceContext;
		}

		public abstract OrganizationServiceContext GetServiceContextForWrite();

		public IEntityUrlProvider GetUrlProvider()
		{
			return _urlProvider;
		}

		public EntityReference GetWebsite()
		{
			return _website;
		}

		public ApplicationPath GetDeletePath(EntityReference entity)
		{
			if (entity == null || _requestContext == null) return null;

			var website = GetWebsite();

			if (website == null) return null;

			try
			{
				var pathData = RouteTable.Routes.GetVirtualPath(_requestContext, typeof(CmsEntityDeleteRouteHandler).FullName, new RouteValueDictionary
				{
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

		public ApplicationPath GetEditPath(EntityReference entity)
		{
			if (entity == null || _requestContext == null) return null;

			var website = GetWebsite();

			if (website == null) return null;

			try
			{
				var pathData = RouteTable.Routes.GetVirtualPath(_requestContext, typeof(CmsEntityRouteHandler).FullName, new RouteValueDictionary
				{
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

		public RequestContext GetRequestContext()
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
}
