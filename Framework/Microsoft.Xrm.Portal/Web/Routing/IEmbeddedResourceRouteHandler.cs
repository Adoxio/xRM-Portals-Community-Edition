/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.Routing;

namespace Microsoft.Xrm.Portal.Web.Routing
{
	/// <summary>
	/// Handles routes involving access to embedded resources.
	/// </summary>
	public interface IEmbeddedResourceRouteHandler : IRouteHandler
	{
		/// <summary>
		/// Description of the available embedded resource assemblies.
		/// </summary>
		IEnumerable<EmbeddedResourceAssemblyAttribute> Mappings { get; }
	}
}
