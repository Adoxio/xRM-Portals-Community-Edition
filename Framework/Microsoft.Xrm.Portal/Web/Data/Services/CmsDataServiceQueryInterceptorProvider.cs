/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public sealed class CmsDataServiceQueryInterceptorProvider : ICmsDataServiceQueryInterceptorProvider
	{
		public string PortalName { get; private set; }

		public CmsDataServiceQueryInterceptorProvider(string portalName)
		{
			PortalName = portalName;
		}

		public ICmsDataServiceQueryInterceptor GetInterceptor()
		{
			return new SingleWebsiteCmsDataServiceQueryInterceptor(PortalName);
		}
	}
}
