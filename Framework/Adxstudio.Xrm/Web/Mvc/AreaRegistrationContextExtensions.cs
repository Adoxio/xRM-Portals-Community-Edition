/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Adxstudio.Xrm.Web.Mvc
{
	/// <summary>
	/// Extensions to <see cref="AreaRegistrationContext"/> for portal applications.
	/// </summary>
	public static class AreaRegistrationContextExtensions
	{
		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, string portalName = null)
		{
			return MapSiteMarkerRoute(context, name, siteMarkerName, url, null, (object)null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, object defaults, string portalName = null)
		{
			return MapSiteMarkerRoute(context, name, siteMarkerName, url, defaults, (object)null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="constraints">A set of expressions that specify values for the <paramref name="url"/> parameter.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, object defaults, object constraints, string portalName = null)
		{
			return MapSiteMarkerRoute(context, name, siteMarkerName, url, defaults, constraints, null, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, string[] namespaces, string portalName = null)
		{
			return MapSiteMarkerRoute(context, name, siteMarkerName, url, null, null, namespaces, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="siteMarkerName">The name (adx_name) of the Site Marker (adx_sitemarker) to map.</param>
		/// <param name="url">
		/// The URL pattern for the route. This specifies the path portion that comes after the specified Site Marker.
		/// </param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <param name="portalName">Portal configuration name to be used by this route.</param>
		/// <returns>The mapped <see cref="Route"/> instance.</returns>
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, object defaults, string[] namespaces, string portalName = null)
		{
			return MapSiteMarkerRoute(context, name, siteMarkerName, url, defaults, null, namespaces, portalName);
		}

		/// <summary>
		/// Allows mapping of an MVC route with a portal Site Marker (adx_sitemarker) as a root path. When mapped, this
		/// route will take over route processing for the URL of the site marker target, and all sub-paths of that URL.
		/// </summary>
		/// <param name="context">Area to which this route will be added.</param>
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
		public static Route MapSiteMarkerRoute(this AreaRegistrationContext context, string name, string siteMarkerName, string url, object defaults, object constraints, string[] namespaces, string portalName = null)
		{
			if (namespaces == null && context.Namespaces != null)
			{
				namespaces = context.Namespaces.ToArray();
			}

			var route = context.Routes.MapSiteMarkerRoute(name, siteMarkerName, url, defaults, constraints, namespaces);

			route.DataTokens["area"] = context.AreaName;
			route.DataTokens["UseNamespaceFallback"] = (namespaces == null || namespaces.Length == 0) ? 1 : 0;

			return route;
		}
	}
}
