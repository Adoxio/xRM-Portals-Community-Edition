/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.Xrm.Portal.Web.Handlers;
using Microsoft.Xrm.Portal.Web.Modules;

namespace Microsoft.Xrm.Portal.Web.Routing
{
	/// <summary>
	/// Serves multiple embedded resources in a single response.
	/// </summary>
	/// <remarks>
	/// This handler concatenates the contents of separate embedded resources into a single response.
	/// </remarks>
	/// <seealso cref="PortalRoutingModule"/>
	/// <seealso cref="EmbeddedResourceHttpHandler"/>
	public sealed class CompositeEmbeddedResourceRouteHandler : IEmbeddedResourceRouteHandler
	{
		/// <summary>
		/// Description of the available embedded resource assemblies.
		/// </summary>
		public IEnumerable<EmbeddedResourceAssemblyAttribute> Mappings { get; private set; }
		private readonly string[] _paths;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mappings">The embedded resource assembly descriptions.</param>
		/// <param name="paths">The paths of the resources to serve.</param>
		public CompositeEmbeddedResourceRouteHandler(IEnumerable<EmbeddedResourceAssemblyAttribute> mappings, params string[] paths)
		{
			Mappings = mappings;
			_paths = paths;
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns></returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new EmbeddedResourceHttpHandler(Mappings, _paths);
		}
	}
}
