/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	public sealed class HiddenFromSiteMapCrmSiteMapNodeValidator : ICrmSiteMapNodeValidator
	{
		public bool Validate(OrganizationServiceContext context, CrmSiteMapNode node)
		{
			var propertyValue = node.Entity.GetAttributeValue<bool?>("adx_hiddenfromsitemap");

			if (propertyValue == null)
			{
				return true;
			}

			return !propertyValue.GetValueOrDefault();
		}
	}
}
