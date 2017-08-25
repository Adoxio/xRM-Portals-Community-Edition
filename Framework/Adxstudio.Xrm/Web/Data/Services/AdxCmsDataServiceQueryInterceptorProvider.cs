/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web.Data.Services;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Data.Services
{
	public class AdxCmsDataServiceQueryInterceptorProvider : ICmsDataServiceQueryInterceptorProvider
	{
		public string PortalName { get; private set; }

		public AdxCmsDataServiceQueryInterceptorProvider(string portalName)
		{
			PortalName = portalName;
		}

		protected static bool WebsiteSpecifiedInConfigurationHasChildWebsites
		{
			get { return PortalContext.Current.Website.GetRelatedEntities(PortalContext.Current.ServiceContext, "adx_website_parentwebsite", EntityRole.Referenced).Any(); }
		}

		public ICmsDataServiceQueryInterceptor GetInterceptor()
		{
			return WebsiteSpecifiedInConfigurationHasChildWebsites
				? (ICmsDataServiceQueryInterceptor)new MultipleWebsiteCmsDataServiceQueryInterceptor(PortalName)
				: new SingleWebsiteCmsDataServiceQueryInterceptor(PortalName);
		}
	}
}
