/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public sealed class NodeValidatorProvider : PortalValidatorProvider
	{
		public NodeValidatorProvider(string portalName)
			: base(portalName)
		{
		}

		public override ICrmSiteMapNodeValidator GetValidator(CrmSiteMapProviderBase provider)
		{
			return new SecurityCrmSiteMapNodeValidator(PortalName);
		}
	}
}
