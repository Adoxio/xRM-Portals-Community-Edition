/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Modules;

namespace Microsoft.Xrm.Portal.Web.Routing
{
	/// <summary>
	/// Handles routes involving access to embedded resources.
	/// </summary>
	/// <seealso cref="PortalRoutingModule"/>
	/// <seealso cref="EmbeddedResourceHttpHandler"/>
	public sealed class EmbeddedResourceRouteHandler : IEmbeddedResourceRouteHandler
	{
		/// <summary>
		/// Description of the available embedded resource assemblies.
		/// </summary>
		public IEnumerable<EmbeddedResourceAssemblyAttribute> Mappings { get; private set; }

		public EmbeddedResourceRouteHandler(IEnumerable<EmbeddedResourceAssemblyAttribute> mappings)
		{
			Mappings = mappings;
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns></returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var prefix = requestContext.RouteData.Values["prefix"] as string;
			var path = requestContext.RouteData.Values["path"] as string;
			var virtualPath = "/{0}/{1}".FormatWith(prefix, path);

			return new EmbeddedResourceHttpHandler(Mappings, virtualPath);
		}
	}
}
