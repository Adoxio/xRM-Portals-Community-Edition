/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.Routing;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Commerce
{
	public abstract class DataAdapterDependencies : Cms.DataAdapterDependencies, IDataAdapterDependencies
	{
		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser = null, RequestContext requestContext = null) : base(serviceContext, securityProvider, urlProvider, website, portalUser, requestContext) { }

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, EntityReference website, EntityReference portalUser = null) : base(serviceContext, securityProvider, urlProvider, website, portalUser) { }

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, IPortalContext portalContext) : base(serviceContext, securityProvider, urlProvider, portalContext) { }

		public DataAdapterDependencies(OrganizationServiceContext serviceContext, ICrmEntitySecurityProvider securityProvider, IEntityUrlProvider urlProvider, IPortalContext portalContext, RequestContext requestContext = null) : base(serviceContext, securityProvider, urlProvider, portalContext, requestContext) { }

		public virtual EntityReference GetPriceList()
		{
			var serviceContext = GetServiceContext();
			var website = GetWebsite();
			var priceListName = serviceContext.GetDefaultPriceListName(website != null ? website.Id : Guid.Empty);
			var priceList = serviceContext.CreateQuery("pricelevel")
				.FirstOrDefault(e => e.GetAttributeValue<string>("name") == priceListName);
			
			if (priceList == null)
			{
                throw new InvalidOperationException("Unable to retrieve the portal price list.");
			}

			return priceList.ToEntityReference();
		}
	}
}
