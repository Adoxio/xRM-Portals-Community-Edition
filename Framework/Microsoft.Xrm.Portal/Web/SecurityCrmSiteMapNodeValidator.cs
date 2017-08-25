/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Validates that a given <see cref="ICrmEntitySecurityProvider"/> grants <see cref="CrmEntityRight.Read"/>
	/// privileges to the <see cref="CrmEntity"/> attached to any <see cref="CrmSiteMapNode"/> to be validated.
	/// </summary>
	public sealed class SecurityCrmSiteMapNodeValidator : ICrmSiteMapNodeValidator
	{
		private readonly ICrmEntitySecurityProvider _securityProvider;

		public SecurityCrmSiteMapNodeValidator(string portalName) : this(PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName)) { }

		public SecurityCrmSiteMapNodeValidator(ICrmEntitySecurityProvider securityProvider)
		{
			securityProvider.ThrowOnNull("securityProvider");

			_securityProvider = securityProvider;
		}

		public bool Validate(OrganizationServiceContext context, CrmSiteMapNode node)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (node == null || node.Entity == null)
			{
				return false;
			}

			var entity = context.MergeClone(node.Entity);

			return _securityProvider.TryAssert(context, entity, CrmEntityRight.Read);
		}
	}
}
