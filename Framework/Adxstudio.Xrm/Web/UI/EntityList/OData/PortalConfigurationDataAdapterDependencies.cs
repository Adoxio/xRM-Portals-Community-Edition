/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.EntityList.OData
{
	/// <summary>
	/// An implementation of <see cref="DataAdapterDependencies"/> that uses <see cref="PortalCrmConfigurationManager"/> to create an <see cref="Microsoft.Xrm.Sdk.Client.OrganizationServiceContext"/>
	/// </summary>
	public class PortalConfigurationDataAdapterDependencies : DataAdapterDependencies
	{
		/// <summary>
		/// PortalConfigurationDataAdapterDependencies constructor
		/// </summary>
		public PortalConfigurationDataAdapterDependencies()
			: base(PortalCrmConfigurationManager.CreateServiceContext(), null)
		{
		}

		/// <summary>
		/// PortalConfigurationDataAdapterDependencies constructor
		/// </summary>
		/// <param name="portalName">Portal context name</param>
		public PortalConfigurationDataAdapterDependencies(string portalName = null)
			: base(PortalCrmConfigurationManager.CreateServiceContext(portalName), GetWebsite(portalName))
		{
		}
		
		/// <summary>
		/// PortalConfigurationDataAdapterDependencies constructor
		/// </summary>
		/// <param name="portalName">Portal context name</param>
		/// <param name="website">Website <see cref="EntityReference"/></param>
		public PortalConfigurationDataAdapterDependencies(string portalName = null, EntityReference website = null)
			: base(PortalCrmConfigurationManager.CreateServiceContext(portalName), website) 
		{
		}

		protected static EntityReference GetWebsite(string portalName = null)
		{
			var website = HttpContext.Current.GetWebsite();
			return website == null ? null : website.Entity.ToEntityReference();
		}
	}
}
