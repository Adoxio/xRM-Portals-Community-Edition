/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Routing;

namespace Microsoft.Xrm.Portal.Web.Modules
{
	/// <summary>
	/// Manages the URL routing for a standard portal website.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Example configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="microsoft.xrm.portal" type="Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal"/>
	///  </configSections>
	///  
	///  <system.webServer>
	///   <modules runAllManagedModulesForAllRequests="true">
	///    <add name="PortalRouting" type="Microsoft.Xrm.Portal.Web.Modules.PortalRoutingModule, Microsoft.Xrm.Portal" preCondition="managedHandler"/>
	///   </modules>
	///  </system.webServer>
	///  
	///  <microsoft.xrm.portal rewriteVirtualPathEnabled="true" [false | true] />
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="PortalRouteHandler"/>
	/// <seealso cref="EmbeddedResourceRouteHandler"/>
	/// <seealso cref="CompositeEmbeddedResourceRouteHandler"/>
	/// <seealso cref="EntityRouteHandler"/>
	public class PortalRoutingModule : IHttpModule // MSBug #120030: Won't seal, inheritance is used extension point.
	{
		private const string _prefix = "xrm";

		private static readonly string[] _paths = new[]
		{
			"js/xrm.js",
			"js/editable/utilities.js",
			"js/editable/data.js",
			"js/editable/ui.js",
			"js/editable/datetimepicker.js",
			"js/editable/attribute.js",
			"js/editable/attributes/text.js",
			"js/editable/attributes/html.js",
			"js/editable/entity.js",
			"js/editable/entity_form.js",
			"js/editable/entity_handler.js",
			"js/editable/entities/adx_webfile.js",
			"js/editable/entities/adx_weblinkset.js",
			"js/editable/entities/adx_webpage.js",
			"js/editable/entities/sitemapchildren.js",
			"js/editable/editable.js",
			"js/xrm-activate.js",
		};

		private static RouteCollection _routes;

		/// <summary>
		/// The name of the <see cref="PortalContextElement"/> specifying the current portal.
		/// </summary>
		public string PortalName { get; set; }

		/// <summary>
		/// Enables routing to the <see cref="EmbeddedResourceRouteHandler"/> for serving embedded resources.
		/// </summary>
		public bool UseEmbeddedResourceVirtualPathProvider { get; set; }

		/// <summary>
		/// Enables routing to the <see cref="CacheInvalidationHandler"/> by specifying the path '/Cache.axd' followed by querystring parameters.
		/// </summary>
		public bool IncludeCacheInvalidationHandler { get; set; }

		/// <summary>
		/// Enables detection of incoming virtual paths (those prefixed by "~/") from the client. If found, the context URL is rewritten to the virtual path.
		/// </summary>
		protected virtual bool RewriteVirtualPathEnabled
		{
			get { return PortalCrmConfigurationManager.GetPortalCrmSection().RewriteVirtualPathEnabled; }
		}

		public void Dispose() { }

		public virtual void Init(HttpApplication application)
		{
			application.PostAuthenticateRequest += RewriteVirtualPath;

			LazyInitializer.EnsureInitialized(ref _routes, Register);
		}

		/// <summary>
		/// Detects the presence of a virtual path within the request URL. If found, the context URL is rewritten to the virtual path.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void RewriteVirtualPath(object sender, EventArgs args)
		{
			if (!RewriteVirtualPathEnabled) return;

			var application = sender as HttpApplication;
			var context = application.Context;
			var url = context.Request.Url.PathAndQuery;

			var match = Regex.Match(url, @"~/.*");

			if (match.Success)
			{
				var rewritePath = match.Value;

				Tracing.FrameworkInformation("PortalRoutingModule", "RewriteVirtualPath", "Redirecting '{0}' to '{1}'", url, rewritePath);

				// perform a redirect to prevent ~/ from appearing in the client address as well as redundant URLs

				context.RedirectAndEndResponse(rewritePath);
			}
		}

		private RouteCollection Register()
		{
			var mappings = Utility.GetEmbeddedResourceMappingAttributes().ToList();
			var paths = GetPaths().ToArray();

			var routes = Register(
				RouteTable.Routes,
				new PortalRouteHandler(PortalName),
				new EmbeddedResourceRouteHandler(mappings),
				new CompositeEmbeddedResourceRouteHandler(mappings, paths));

			Tracing.FrameworkInformation("PortalRoutingModule", "Init", "Added '{0}' route entries.", routes.Count);

			return routes;
		}

		protected virtual RouteCollection Register(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			RegisterStaticRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);

			RegisterEmbeddedResourceRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);

			RegisterIgnoreRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);

			RegisterCustomRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);

			RegisterDefaultRoutes(routes, portalRouteHandler, embeddedResourceRouteHandler, scriptHandler);

			return routes;
		}

		protected virtual void Register(
			RouteCollection routes,
			string prefix,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			if (UseEmbeddedResourceVirtualPathProvider)
			{
				// ignore the prefix to allow the EmbeddedResourceVirtualPathProvider to handle the request

				routes.Ignore(prefix + "/{*pathInfo}");
			}
			else
			{
				// add the combined script handler

				routes.Add(
					"type={0},prefix={1}".FormatWith(scriptHandler, prefix),
					new Route(prefix + "/js/xrm-combined.js/{*pathInfo}", null, null, scriptHandler));

				// keep this route until the xrm-combined-js.aspx is removed from the files project

				routes.Add(
					"type={0},prefix={1},extension=aspx".FormatWith(scriptHandler, prefix),
					new Route(prefix + "/js/xrm-combined-js.aspx/{*pathInfo}", null, null, scriptHandler));

				// add the embedded resource handler

				routes.Add(
					"type={0},prefix={1}".FormatWith(embeddedResourceRouteHandler, prefix),
					new Route(
						"{prefix}/{*path}",
						null,
						new RouteValueDictionary(new { prefix }),
						embeddedResourceRouteHandler));
			}
		}

		protected static IEnumerable<string> GetPaths(string prefix, IEnumerable<string> paths)
		{
			return paths.Select(p => "~/{0}/{1}".FormatWith(prefix, p));
		}

		protected virtual IEnumerable<string> GetPaths()
		{
			return GetPaths(_prefix, _paths);
		}

		protected virtual void RegisterStaticRoutes(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			if (IncludeCacheInvalidationHandler)
			{
				// add the cache invalidation handler

				var cacheInvalidationHandler = new CacheInvalidationHandler();

				routes.Add(
					cacheInvalidationHandler.GetType().FullName,
					new Route("Cache.axd/{*pathInfo}", null, null, cacheInvalidationHandler));
			}
		}

		protected virtual void RegisterEmbeddedResourceRoutes(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			// register embedded resource handler routes

			Register(routes, _prefix, embeddedResourceRouteHandler, scriptHandler);
		}

		protected virtual void RegisterIgnoreRoutes(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			routes.Ignore("{resource}.axd/{*pathInfo}");
			routes.Ignore("{resource}.svc/{*pathInfo}");
		}

		protected virtual void RegisterCustomRoutes(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			var entityRouteHandler = new EntityRouteHandler(PortalName);
			routes.Add(
				entityRouteHandler.GetType().FullName,
				new Route("_entity/{logicalName}/{id}", entityRouteHandler));
		}

		protected virtual void RegisterDefaultRoutes(
			RouteCollection routes,
			IRouteHandler portalRouteHandler,
			IEmbeddedResourceRouteHandler embeddedResourceRouteHandler,
			IEmbeddedResourceRouteHandler scriptHandler)
		{
			routes.Add(portalRouteHandler.GetType().FullName, new Route("{*path}", portalRouteHandler));
		}
	}
}
