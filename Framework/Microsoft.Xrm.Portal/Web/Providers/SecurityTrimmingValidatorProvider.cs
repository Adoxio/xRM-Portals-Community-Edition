/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public sealed class SecurityTrimmingValidatorProvider : PortalValidatorProvider
	{
		public SecurityTrimmingValidatorProvider(string portalName)
			: base(portalName)
		{
		}

		public override ICrmSiteMapNodeValidator GetValidator(CrmSiteMapProviderBase provider)
		{
			return provider.SecurityTrimmingEnabled
				? new SecurityCrmSiteMapNodeValidator(PortalName)
				: new AlwaysValidCrmSiteMapNodeValidator() as ICrmSiteMapNodeValidator;
		}
	}
}
