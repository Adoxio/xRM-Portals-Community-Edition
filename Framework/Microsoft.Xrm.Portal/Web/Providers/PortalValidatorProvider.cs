/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public abstract class PortalValidatorProvider : INodeValidatorProvider
	{
		public string PortalName { get; private set; }

		protected PortalValidatorProvider(string portalName)
		{
			PortalName = portalName;
		}

		public abstract ICrmSiteMapNodeValidator GetValidator(CrmSiteMapProviderBase provider);
	}
}
