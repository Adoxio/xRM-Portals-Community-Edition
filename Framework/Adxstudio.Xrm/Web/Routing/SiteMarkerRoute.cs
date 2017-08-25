/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Web.Providers;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// Allows mapping of an route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this route
	/// will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
	/// </summary>
	public class SiteMarkerRoute : Route, IPortalContextRoute
	{
		private readonly string _cacheKey;

		public SiteMarkerRoute(string siteMarker, string url, IRouteHandler routeHandler, string portalName = null) : base(url, routeHandler)
		{
			if (string.IsNullOrEmpty(siteMarker))
			{
				throw new ArgumentException("Value can't be null or empty.", "siteMarker");
			}

			PortalName = portalName;
			SiteMarker = siteMarker;

			_cacheKey = GetCacheKey(GetType(), SiteMarker, PortalName);
		}

		public SiteMarkerRoute(string siteMarker, string url, RouteValueDictionary defaults, IRouteHandler routeHandler, string portalName = null) : base(url, defaults, routeHandler)
		{
			if (string.IsNullOrEmpty(siteMarker))
			{
				throw new ArgumentException("Value can't be null or empty.", "siteMarker");
			}

			PortalName = portalName;
			SiteMarker = siteMarker;

			_cacheKey = GetCacheKey(GetType(), SiteMarker, PortalName);
		}

		public SiteMarkerRoute(string siteMarker, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler, string portalName = null) : base(url, defaults, constraints, routeHandler)
		{
			if (string.IsNullOrEmpty(siteMarker))
			{
				throw new ArgumentException("Value can't be null or empty.", "siteMarker");
			}

			PortalName = portalName;
			SiteMarker = siteMarker;

			_cacheKey = GetCacheKey(GetType(), SiteMarker, PortalName);
		}

		public SiteMarkerRoute(string siteMarker, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler, string portalName = null) : base(url, defaults, constraints, dataTokens, routeHandler)
		{
			if (string.IsNullOrEmpty(siteMarker))
			{
				throw new ArgumentException("Value can't be null or empty.", "siteMarker");
			}

			PortalName = portalName;
			SiteMarker = siteMarker;

			_cacheKey = GetCacheKey(GetType(), SiteMarker, PortalName);
		}

		public string PortalName { get; private set; }

		public string SiteMarker { get; private set; }

		public string GetPortalContextPath(IPortalContext portalContext, string path)
		{
			if (portalContext == null || path == null)
			{
				return null;
			}

			var pathCache = HttpContext.Current != null ? HttpContext.Current.Items : null;
			var applicationPath = GetApplicationPathForSiteMarker(portalContext.ServiceContext, portalContext.Website, pathCache);

			return GetPortalContextPath(path, applicationPath);
		}

		public string GetPortalContextPath(ContentMap contentMap, WebsiteNode website, string path)
		{
			if (contentMap == null || website == null || path == null)
			{
				return null;
			}

			var pathCache = HttpContext.Current != null ? HttpContext.Current.Items : null;
			var applicationPath = GetApplicationPathForSiteMarker(contentMap, website, pathCache);

			return GetPortalContextPath(path, applicationPath);
		}

		private string GetPortalContextPath(string path, ApplicationPath applicationPath)
		{
			if (applicationPath == null || applicationPath.AbsolutePath == null)
			{
				return null;
			}

			// Create a route for this application path, filtering out any IRouteConstraint constraints. We
			// don't support these here, since we're only passing a mock HTTP context that doesn't support
			// the full range of properties a real one would. Only regex-based constraints can be supported
			// here.
			var route = GetRoute(applicationPath, constraint => !(constraint.Value is IRouteConstraint));

			if (route == null)
			{
				return null;
			}

			var routeData = route.GetRouteData(MockRequestHttpContext.FromPath(path));

			return routeData == null ? null : applicationPath.AbsolutePath;
		}

		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
			var route = GetRoute(GetApplicationPathForSiteMarker(httpContext));

			if (route == null)
			{
				return null;
			}

			HttpContextBase mockHttpContext = httpContext;
			bool pathHasLanguageCode;
			IWebsiteLanguage websiteLangauge;	// multilanguage is enabled if we find a language
			string pathWithoutLanguageCode = ContextLanguageInfo.StripLanguageCodeFromAbsolutePath(httpContext, out pathHasLanguageCode, out websiteLangauge);
			if (websiteLangauge != null && pathHasLanguageCode)
			{
				var urlWithoutLanguageCode = httpContext.Request.Url.GetLeftPart(UriPartial.Authority) + pathWithoutLanguageCode;
				HttpRequest request = new HttpRequest(httpContext.Request.FilePath, urlWithoutLanguageCode, httpContext.Request.Url.Query);
				mockHttpContext = new MockRequestHttpContext(new HttpRequestWrapper(request));
			}

			var routeData = route.GetRouteData(mockHttpContext);

			return routeData == null
				? null
				: WrapRouteData(routeData);
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			var siteMarkerApplicationPath = GetApplicationPathForSiteMarker(requestContext);

			var route = GetRoute(siteMarkerApplicationPath);

			if (route == null)
			{
				return null;
			}

			var pathData =  route.GetVirtualPath(requestContext, values);

			if (pathData == null)
			{
				return null;
			}

			// Trim the "~/" from the start of the app-relative path to get a virtual path.
			var siteMarkerVirtualPath = siteMarkerApplicationPath.AppRelativePath.Substring(2);

			// If our route virtual path is the same as the root site marker path, we want to force an extra trailing
			// slash, because our portal web page URLs have a trailing slash, but the routing system will leave it off.
			return string.Equals(pathData.VirtualPath, siteMarkerVirtualPath.TrimEnd('/'), StringComparison.Ordinal)
				? WrapVirtualPathData(pathData, "{0}{1}".FormatWith(pathData.VirtualPath, siteMarkerVirtualPath.EndsWith("/") ? "/" : string.Empty))
				: WrapVirtualPathData(pathData);
		}

		private RouteBase GetRoute(ApplicationPath siteMarkerApplicationPath)
		{
			return GetRoute(siteMarkerApplicationPath, constraint => true);
		}

		private RouteBase GetRoute(ApplicationPath siteMarkerApplicationPath, Func<KeyValuePair<string, object>, bool> constraintFilter)
		{
			if (constraintFilter == null) throw new ArgumentNullException("constraintFilter");

			if (siteMarkerApplicationPath == null
				|| siteMarkerApplicationPath.AppRelativePath == null
				|| siteMarkerApplicationPath.AbsolutePath == null)
			{
				return null;
			}

			// Trim the "~/" from the start of the app-relative path to get a virtual path.
			var siteMarkerVirtualPath = siteMarkerApplicationPath.AppRelativePath.Substring(2);

			if (!(string.IsNullOrEmpty(siteMarkerVirtualPath) || siteMarkerVirtualPath.EndsWith("/")))
			{
				return null;
			}

			// Add the site marker absolute path to the route values (by way of the defaults). This is done for the
			// benefit of the PortalContext system, which looks for this path value to determine the current portal
			// context entity.
			var defaults = Defaults == null ? new RouteValueDictionary() : new RouteValueDictionary(Defaults);
			defaults["path"] = siteMarkerApplicationPath.AbsolutePath;

			var constraints = Constraints == null
				? null
				: new RouteValueDictionary(Constraints.Where(constraintFilter).ToDictionary(e => e.Key, e => e.Value));

			return new Route("{0}{1}".FormatWith(siteMarkerVirtualPath, Url), defaults, constraints, DataTokens, RouteHandler);
		}

		private ApplicationPath GetApplicationPathForSiteMarker(HttpContextBase httpContext)
		{
			return GetApplicationPathForSiteMarker(new RequestContext(httpContext, new RouteData()));
		}

		private ApplicationPath GetApplicationPathForSiteMarker(RequestContext request)
		{
			return GetApplicationPathForSiteMarker(request.HttpContext.Items, () =>
			{
				var crmWebsite = request.HttpContext.GetWebsite();

				if (crmWebsite == null)
				{
					return null;
				}

				var website = crmWebsite.Entity;

				if (website == null)
				{
					return null;
				}
				
				var contentMapProvider = AdxstudioCrmConfigurationManager.CreateContentMapProvider(PortalName);

				if (contentMapProvider == null)
				{
					var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

					website = serviceContext.MergeClone(website);

					return GetApplicationPathForSiteMarker(serviceContext, website, request.HttpContext.Items);
				}

				return contentMapProvider.Using(contentMap =>
				{
					WebsiteNode websiteNode;

					return contentMap.TryGetValue(website, out websiteNode)
						? GetApplicationPathForSiteMarker(contentMap, websiteNode, request.HttpContext.Items)
						: null;
				});
			});
		}

		private ApplicationPath GetApplicationPathForSiteMarker(OrganizationServiceContext serviceContext, Entity website, IDictionary cache)
		{
			if (serviceContext == null || website == null)
			{
				return null;
			}

			return GetApplicationPathForSiteMarker(cache, () =>
			{
				var page = OrganizationServiceContextExtensions.GetPageBySiteMarkerName(serviceContext, website, SiteMarker);

				if (page == null)
				{
					return null;
				}

				var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName).GetDependency<IEntityUrlProvider>();

				return urlProvider.GetApplicationPath(serviceContext, page);
			});
		}

		private ApplicationPath GetApplicationPathForSiteMarker(ContentMap contentMap, WebsiteNode website, IDictionary cache)
		{
			return GetApplicationPathForSiteMarker(cache, () =>
			{
				var siteMarkerNode = website.SiteMarkers
					.FirstOrDefault(e => string.Equals(e.Name, SiteMarker, StringComparison.Ordinal));

				if (siteMarkerNode == null || siteMarkerNode.WebPage == null || siteMarkerNode.WebPage.IsReference)
				{
					return null;
				}

				var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(PortalName)
					.GetDependency<IContentMapEntityUrlProvider>();

				try
				{
					return urlProvider.GetApplicationPath(contentMap, siteMarkerNode.WebPage);
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			});
		}

		private ApplicationPath GetApplicationPathForSiteMarker(IDictionary cache, Func<ApplicationPath> getApplicationPath)
		{
			if (cache == null)
			{
				return getApplicationPath();
			}

			if (cache.Contains(_cacheKey))
			{
				return cache[_cacheKey] as ApplicationPath;
			}

			var path = getApplicationPath();

			cache[_cacheKey] = path;

			return path;
		}

		private RouteData WrapRouteData(RouteData routeData)
		{
			if (routeData == null)
			{
				return null;
			}

			var wrapperRouteData = new RouteData(this, routeData.RouteHandler);

			if (routeData.DataTokens != null)
			{
				foreach (var dataToken in routeData.DataTokens)
				{
					wrapperRouteData.DataTokens[dataToken.Key] = dataToken.Value;
				}
			}

			if (routeData.Values != null)
			{
				foreach (var value in routeData.Values)
				{
					wrapperRouteData.Values[value.Key] = value.Value;
				}
			}

			return wrapperRouteData;
		}

		private static string GetCacheKey(Type type, string siteMarker, string portalName = null)
		{
			return "{0}:{1}:{2}".FormatWith(type.FullName, siteMarker, portalName);
		}

		private VirtualPathData WrapVirtualPathData(VirtualPathData virtualPathData, string virtualPath = null)
		{
			if (virtualPathData == null)
			{
				return null;
			}

			virtualPath = virtualPath ?? virtualPathData.VirtualPath;

			var wrapperVirtualPathData = new VirtualPathData(this, virtualPath);

			if (virtualPathData.DataTokens != null)
			{
				foreach (var dataToken in virtualPathData.DataTokens)
				{
					wrapperVirtualPathData.DataTokens[dataToken.Key] = dataToken.Value;
				}
			}

			return wrapperVirtualPathData;
		}

		private class MockRequestHttpContext : HttpContextBase
		{
			private readonly HttpRequestBase _request;

			public MockRequestHttpContext(HttpRequestBase request)
			{
				if (request == null) throw new ArgumentNullException("request");

				_request = request;
			}

			public override HttpRequestBase Request
			{
				get { return _request; }
			}

			public static MockRequestHttpContext FromPath(string path)
			{
				var applicationPath = UrlMapping.GetApplicationPath(path);

				return new MockRequestHttpContext(new MockHttpRequest(applicationPath.AppRelativePath));
			}
		}

		private class MockHttpRequest : HttpRequestBase
		{
			private readonly string _appRelativePath;

			public MockHttpRequest(string appRelativePath)
			{
				if (appRelativePath == null) throw new ArgumentNullException("appRelativePath");

				_appRelativePath = appRelativePath;
			}

			public override string AppRelativeCurrentExecutionFilePath
			{
				get { return _appRelativePath; }
			}

			public override string PathInfo
			{
				get { return string.Empty; }
			}
		}
	}
}
