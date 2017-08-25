/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Web.Routing
{
	/// <summary>
	/// Provides the initialization of custom <see cref="IHttpHandler"/> handlers that can be returned by the <see cref="PortalRouteHandler"/>.
	/// </summary>
	public interface IPortalRouteHandlerProvider
	{
		/// <summary>
		/// Instantiates a new custom handler based on a request or allows the caller to use a default handler.
		/// </summary>
		bool TryCreateHandler(IPortalContext portal, out IHttpHandler handler);
	}
}
