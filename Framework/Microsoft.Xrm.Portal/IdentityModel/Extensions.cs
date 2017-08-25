/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml11;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Web;
using Microsoft.IdentityModel.Web.Configuration;

namespace Microsoft.Xrm.Portal.IdentityModel
{
	public static class Extensions
	{
		/// <summary>
		/// Reconfigures the service to use a custom service certificate for cookie tranformation instead of using DPAPI.
		/// Reconfigures the service to use the <see cref="ClaimTypes.NameIdentifier"/> claim as the default identity claim.
		/// </summary>
		public static void OnServiceConfigurationCreated(object sender, ServiceConfigurationCreatedEventArgs args)
		{
			ConfigureServiceCertificateCookieTransform(sender, args);
			ConfigureNameIdentifierSecurityTokenHandlers(sender, args);
		}

		/// <summary>
		/// Reconfigures the service to use a custom service certificate for cookie tranformation instead of using DPAPI.
		/// </summary>
		public static void ConfigureServiceCertificateCookieTransform(object sender, ServiceConfigurationCreatedEventArgs args)
		{
			ConfigureServiceCertificateCookieTransform(args.ServiceConfiguration);
		}

		/// <summary>
		/// Reconfigures the service to use a custom service certificate for cookie tranformation instead of using DPAPI.
		/// </summary>
		public static void ConfigureServiceCertificateCookieTransform(this ServiceConfiguration config)
		{
			if (config.ServiceCertificate != null)
			{
				// Use the <serviceCertificate> to protect the cookies that are sent to the client.

				var sessionTransforms = new List<CookieTransform>(new CookieTransform[]
				{
					new DeflateCookieTransform(),
					new RsaEncryptionCookieTransform(config.ServiceCertificate),
					new RsaSignatureCookieTransform(config.ServiceCertificate)
				});

				var sessionHandler = new Microsoft.IdentityModel.Tokens.SessionSecurityTokenHandler(sessionTransforms.AsReadOnly());

				config.SecurityTokenHandlers.AddOrReplace(sessionHandler);
			}
		}

		/// <summary>
		/// Reconfigures the service to use the <see cref="ClaimTypes.NameIdentifier"/> claim as the default identity claim.
		/// </summary>
		public static void ConfigureNameIdentifierSecurityTokenHandlers(object sender, ServiceConfigurationCreatedEventArgs args)
		{
			ConfigureNameIdentifierSecurityTokenHandlers(args.ServiceConfiguration);
		}

		/// <summary>
		/// Reconfigures the service to use the <see cref="ClaimTypes.NameIdentifier"/> claim as the default identity claim.
		/// </summary>
		public static void ConfigureNameIdentifierSecurityTokenHandlers(this ServiceConfiguration config)
		{
			// configure the token handlers to use the NameIdentifier claim instead of the Name claim

			var saml11Handler = config.SecurityTokenHandlers[typeof(SamlSecurityToken)] as Saml11SecurityTokenHandler;
			var saml2Handler = config.SecurityTokenHandlers[typeof(Microsoft.IdentityModel.Tokens.Saml2.Saml2SecurityToken)] as Microsoft.IdentityModel.Tokens.Saml2.Saml2SecurityTokenHandler;
			if (saml11Handler != null) saml11Handler.SamlSecurityTokenRequirement.NameClaimType = ClaimTypes.NameIdentifier;
			if (saml2Handler != null) saml2Handler.SamlSecurityTokenRequirement.NameClaimType = ClaimTypes.NameIdentifier;
		}
	}
}
