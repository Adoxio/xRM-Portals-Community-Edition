/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public interface IEntityWebsiteProvider
	{
		Entity GetWebsite(OrganizationServiceContext context, Entity entity);
	}
}
