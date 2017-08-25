/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Interface representing validation logic for a <see cref="CrmSiteMapNode"/>.
	/// </summary>
	public interface ICrmSiteMapNodeValidator
	{
		bool Validate(OrganizationServiceContext context, CrmSiteMapNode node);
	}
}
