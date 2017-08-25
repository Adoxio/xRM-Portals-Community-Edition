/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides dependencies pulled from the portal's configuration for the various data adapters within the Adxstudio.Xrm.Issues namespace.
	/// </summary>
	public class PortalConfigurationDataAdapterDependencies : DataAdapterDependencies
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="portalName">The configured name of the portal to get dependencies from.</param>
		public PortalConfigurationDataAdapterDependencies(string portalName = null)
			: base(
				PortalCrmConfigurationManager.CreateServiceContext(portalName),
				PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
				HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : null,
				PortalCrmConfigurationManager.CreatePortalContext(portalName))
		{
		}
	}
}
