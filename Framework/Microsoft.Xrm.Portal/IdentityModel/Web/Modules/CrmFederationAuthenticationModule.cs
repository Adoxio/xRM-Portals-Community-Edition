/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using Microsoft.IdentityModel.Web;

namespace Microsoft.Xrm.Portal.IdentityModel.Web.Modules
{
	public sealed class CrmFederationAuthenticationModule : WSFederationAuthenticationModule
	{
		public CrmFederationAuthenticationModule()
		{
		}

		public CrmFederationAuthenticationModule(HttpContext context)
			: this(context.ApplicationInstance)
		{
		}

		public CrmFederationAuthenticationModule(HttpApplication application)
		{
			ServiceConfiguration = FederatedAuthentication.ServiceConfiguration;
			Initialize(application);
		}

		private void Initialize(HttpApplication application)
		{
			InitializeModule(application);
		}

		protected override void InitializeModule(HttpApplication application)
		{
			InitializePropertiesFromConfiguration(ServiceConfiguration.Name);
		}
	}
}
