/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Routing;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms
{
	public interface IDataAdapterDependencies
	{
		EntityReference GetPortalUser();

		ICrmEntitySecurityProvider GetSecurityProvider();

		OrganizationServiceContext GetServiceContext();

		OrganizationServiceContext GetServiceContextForWrite();

		IEntityUrlProvider GetUrlProvider();

		EntityReference GetWebsite();

		ApplicationPath GetDeletePath(EntityReference entity);

		ApplicationPath GetEditPath(EntityReference entity);

		RequestContext GetRequestContext();
	}
}
