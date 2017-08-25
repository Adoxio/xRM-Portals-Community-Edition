/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Security
{
	internal interface ICacheSupportingCrmEntitySecurityProvider : ICrmEntitySecurityProvider
	{
		bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies);
	}
}
