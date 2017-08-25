/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Ideas
{
	/// <summary>
	/// Provides dependencies pulled from an <see cref="IPortalContext"/> for the various data adapters within the Adxstudio.Xrm.Ideas namespace.
	/// </summary>
	public class PortalContextDataAdapterDependencies : DataAdapterDependencies
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="portalContext">The <see cref="IPortalContext"/> to get dependencies from.</param>
		/// <param name="portalName">The configured name of the portal to get a security provider dependency from.</param>
		public PortalContextDataAdapterDependencies(IPortalContext portalContext, string portalName = null)
			: base(
				portalContext.ServiceContext,
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : null,
				portalContext)
		{
		}
	}
}
