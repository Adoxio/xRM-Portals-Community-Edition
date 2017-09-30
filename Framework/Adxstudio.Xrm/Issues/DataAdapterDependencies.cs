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

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides dependencies for the various data adapters within the Adxstudio.Xrm.Issues namespace.
	/// </summary>
	public class DataAdapterDependencies : IDataAdapterDependencies
	{
		private readonly HttpContextBase _httpContext;
		private readonly EntityReference _portalUser;
		private readonly ICrmEntitySecurityProvider _securityProvider;
		private readonly OrganizationServiceContext _serviceContext;
		private readonly EntityReference _website;
		private RequestContext _requestContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serviceContext">An <see cref="OrganizationServiceContext"/>.</param>
		/// <param name="securityProvider">An <see cref="ICrmEntitySecurityProvider"/>.</param>
		/// <param name="httpContext">An <see cref="HttpContextBase"/>.</param>
		/// <param name="website">An <see cref="EntityReference"/> to a website.</param>
		/// <param name="portalUser">An <see cref="EntityReference"/> to a portal user.</param>
		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider,
			HttpContextBase httpContext, EntityReference website, EntityReference portalUser = null, RequestContext requestContext = null)
		{
			serviceContext.ThrowOnNull("serviceContext");
			securityProvider.ThrowOnNull("securityProvider");
			httpContext.ThrowOnNull("httpContext");
			website.ThrowOnNull("website");
			
			_serviceContext = serviceContext;
			_securityProvider = securityProvider;
			_httpContext = httpContext;
			_website = website;
			_portalUser = portalUser;
			_requestContext = requestContext;
		}

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider,
			HttpContextBase httpContext, EntityReference website, EntityReference portalUser = null)
			: this(serviceContext, securityProvider, httpContext, website, portalUser: portalUser, requestContext: null)
		{
			
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serviceContext">An <see cref="OrganizationServiceContext"/>.</param>
		/// <param name="securityProvider">An <see cref="ICrmEntitySecurityProvider"/>.</param>
		/// <param name="httpContext">An <see cref="HttpContextBase"/>.</param>
		/// <param name="portalContext">An <see cref="IPortalContext"/> to get user and website <see cref="EntityReference"/>s from.</param>
		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, 
			HttpContextBase httpContext, IPortalContext portalContext, RequestContext requestContext = null)
			: this(
				serviceContext,
				securityProvider,
				httpContext,
				portalContext.Website == null ? null : portalContext.Website.ToEntityReference(),
				portalContext.User != null ? portalContext.User.ToEntityReference() : null,
				requestContext: requestContext)
		{
			portalContext.ThrowOnNull("portalContext");
		}

		/// <summary>
		/// Returns an <see cref="HttpContextBase"/>.
		/// </summary>
		public HttpContextBase GetHttpContext()
		{
			return _httpContext;
		}

		/// <summary>
		/// Returns an <see cref="EntityReference"/> to a portal user.
		/// </summary>
		public EntityReference GetPortalUser()
		{
			return _portalUser;
		}

		/// <summary>
		/// Returns a security provider.
		/// </summary>
		public ICrmEntitySecurityProvider GetSecurityProvider()
		{
			return _securityProvider;
		}

		/// <summary>
		/// Returns an <see cref="OrganizationServiceContext"/>.
		/// </summary>
		public OrganizationServiceContext GetServiceContext()
		{
			return _serviceContext;
		}

		public OrganizationServiceContext GetServiceContextForWrite()
		{
			return _serviceContext;
		}

		public IEntityUrlProvider GetUrlProvider()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Retunrs an <see cref="EntityReference"/> to a website.
		/// </summary>
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
			return _requestContext;
		}
	}
}
