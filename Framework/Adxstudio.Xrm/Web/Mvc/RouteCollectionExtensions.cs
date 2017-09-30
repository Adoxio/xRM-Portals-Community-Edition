/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Extensions to <see cref="RouteCollection"/> for portal applications.
	/// </summary>
	public static class RouteCollectionExtensions
	{
		internal static string GetPortalContextPath(this RouteCollection routes, IPortalContext portalContext, string path)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", path));

			var match = routes.OfTypeWithLock<IPortalContextRoute>()
				.Select(portalContextRoute => portalContextRoute.GetPortalContextPath(portalContext, path))
				.FirstOrDefault(contextPath => contextPath != null);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", path));

			return match;
		}

		internal static string GetPortalContextPath(this RouteCollection routes, ContentMap contentMap, WebsiteNode website, string path)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start: {0}", path));

			var match = routes.OfTypeWithLock<IPortalContextRoute>()
				.Select(portalContextRoute => portalContextRoute.GetPortalContextPath(contentMap, website, path))
				.FirstOrDefault(contextPath => contextPath != null);

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: {0}", path));

			return match;
		}

		/// <summary>
		/// Enumerates and returns the items of the route collection under a read lock.
		/// </summary>
		/// <typeparam name="T">The type of the route collection items.</typeparam>
		/// <param name="routes">The route collection.</param>
		/// <returns>A list of the route collection items.</returns>
		internal static IList<T> OfTypeWithLock<T>(this RouteCollection routes)
		{
			return UseWithReadLock(routes, r => r.OfType<T>().ToArray());
		}

		/// <summary>
		/// Performs an action with a read lock on the route collection.
		/// </summary>
		/// <typeparam name="T">The result type.</typeparam>
		/// <param name="routes">The route collection.</param>
		/// <param name="action">The action to run.</param>
		/// <returns>The action result.</returns>
		internal static T UseWithReadLock<T>(this RouteCollection routes, Func<RouteCollection, T> action)
		{
			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Read: Requested");

			T result;

			using (routes.GetReadLock())
			{
				ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Read: Acquired");

				result = action(routes);
			}

			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Write: Released");

			return result;
		}

		/// <summary>
		/// Performs an action with a write lock on the route collection.
		/// </summary>
		/// <param name="routes">The route collection.</param>
		/// <param name="action">The action to run.</param>
		internal static void UseWithWriteLock(this RouteCollection routes, Action<RouteCollection> action)
		{
			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Write: Requested");

			using (routes.GetWriteLock())
			{
				ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Write: Acquired");

				action(routes);
			}

			ADXTrace.Instance.TraceVerbose(TraceCategory.Application, "RouteCollection lock: Write: Released");
		}

		/// <summary>
		/// Registers routes under a write lock.
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="action"></param>
		public static void RegisterRoutesWithLock(this RouteCollection routes, Action<RouteCollection> action)
		{
			UseWithWriteLock(routes, action);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, string portalName = null)
		{
			return MapSiteMarkerRoute(routes, name, siteMarkerName, url, null, (object)null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, object defaults, string portalName = null)
		{
			return MapSiteMarkerRoute(routes, name, siteMarkerName, url, defaults, (object)null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="constraints">A set of expressions that specify values for the <paramref name="url"/> parameter.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, object defaults, object constraints, string portalName = null)
		{
			return MapSiteMarkerRoute(routes, name, siteMarkerName, url, defaults, constraints, null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, string[] namespaces, string portalName = null)
		{
			return MapSiteMarkerRoute(routes, name, siteMarkerName, url, null, null, namespaces, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, object defaults, string[] namespaces, string portalName = null)
		{
			return MapSiteMarkerRoute(routes, name, siteMarkerName, url, defaults, null, namespaces, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="routes">Route collection to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="constraints">A set of expressions that specify values for the <paramref name="url"/> parameter.</param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this RouteCollection routes, string name, string siteMarkerName, string url, object defaults, object constraints, string[] namespaces, string portalName = null)
		{
			var route = new SiteMarkerRoute(siteMarkerName, url, 
				new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), new RouteValueDictionary(),
				new MvcRouteHandler(), portalName);

			if (namespaces != null && namespaces.Length > 0)
			{
				route.DataTokens["Namespaces"] = namespaces;
			}

			routes.UseWithWriteLock(r =>
			{
				r.Add(name, route);
			});

			return route;
		}
	}
}
