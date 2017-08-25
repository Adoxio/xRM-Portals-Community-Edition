/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Web;
using ITfoxtec.Saml2;
using ITfoxtec.Saml2.Bindings;
using ITfoxtec.Saml2.Schemas;
using ITfoxtec.Saml2.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.WsFederation;
using Adxstudio.Xrm.Web;

namespace Adxstudio.Xrm.Owin.Security.Saml2
{
	/// <summary>
	/// A per-request authentication handler for the Saml2AuthenticationMiddleware.
	/// </summary>
	public class Saml2AuthenticationHandler : WsFederationAuthenticationHandler
	{
		private class InternalSaml2AuthnResponse : Saml2AuthnResponse
		{
			public InternalSaml2AuthnResponse(Saml2SecurityTokenHandler saml2SecurityTokenHandler)
			{
				Saml2SecurityTokenHandler = saml2SecurityTokenHandler;
			}
		}

		private class Saml2ResponseIssuerNameRegistry : IssuerNameRegistry
		{
			public override string GetIssuerName(SecurityToken securityToken, string requestedIssuerName)
			{
				return requestedIssuerName;
			}

			public override string GetIssuerName(SecurityToken securityToken)
			{
				throw new InvalidOperationException();
			}
		}

		private const string _relayStateReturnUrl = "ReturnUrl";
		private const string _relayStateWctx = "Saml2OwinState";
		private const string _relayStateRedirectUri = "RedirectUri";
		private WsFederationConfiguration _configuration;

		public Saml2AuthenticationHandler(ILogger logger)
			: base(logger)
		{
		}

		protected override async Task ApplyResponseGrantAsync()
		{
			var options = Options as Saml2AuthenticationOptions;

			if (options == null)
			{
				return;
			}

			// handle sign-out response

			if (options.SingleLogoutServiceResponsePath.HasValue && options.SingleLogoutServiceResponsePath == (Request.PathBase + Request.Path))
			{
				await ApplyResponseLogoutAsync();
				return;
			}

			// handle sign-out request

			if (options.SingleLogoutServiceRequestPath.HasValue && options.SingleLogoutServiceRequestPath == (Request.PathBase + Request.Path))
			{
				await ApplyRequestLogoutAsync();
				return;
			}

			var signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);

			if (signout == null)
			{
				return;
			}

			if (_configuration == null)
			{
				_configuration = await options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
			}

			// reusing the SingleSignOnService location from the configuration to determine the destination

			var issuer = options.Wtrealm;
			var destination = _configuration.TokenEndpoint ?? string.Empty;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("issuer={0}", "destination={1}", issuer, destination));

			var properties = signout.Properties;

			if (string.IsNullOrEmpty(properties.RedirectUri))
			{
				properties.RedirectUri = options.SignOutWreply ?? GetCurrentUri();
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("RedirectUri={0}", properties.RedirectUri));

			var state = new Dictionary<string, string>
			{
				{ _relayStateWctx, Options.StateDataFormat.Protect(properties) }
			};

			var binding = new Saml2RedirectBinding();
			binding.SetRelayStateQuery(state);

			var redirectBinding = binding.Bind(new Saml2LogoutRequest
			{
				Issuer = new EndpointReference(issuer),
				Destination = new EndpointAddress(destination)
			}, options.SigningCertificate);

			var redirectLocation = redirectBinding.RedirectLocation.AbsoluteUri;

			if (!Uri.IsWellFormedUriString(redirectLocation, UriKind.Absolute))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("The sign-out redirect URI is malformed: {0}", redirectLocation));
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("redirectLocation={0}", redirectLocation));

			Response.Redirect(redirectLocation);
		}

		protected virtual async Task ApplyResponseLogoutAsync()
		{
			var options = Options as Saml2AuthenticationOptions;

			if (options == null)
			{
				return;
			}

			if (_configuration == null)
			{
				_configuration = await options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
			}

			var request = Context.Get<HttpContextBase>(typeof(HttpContextBase).FullName).Request;

			foreach (var signingKey in _configuration.SigningKeys.OfType<X509SecurityKey>())
			{
				var binding = new Saml2PostBinding();
				Saml2LogoutResponse response = null;

				try
				{
					response = binding.Unbind(request, new Saml2LogoutResponse(), signingKey.Certificate) as Saml2LogoutResponse;
				}
				catch (Saml2ResponseException)
				{
				}

				if (response == null || response.Status != Saml2StatusCodes.Success) continue;

				var relayState = binding.GetRelayStateQuery();
				var properties = relayState.ContainsKey(_relayStateWctx)
					? Options.StateDataFormat.Unprotect(relayState[_relayStateWctx])
					: new AuthenticationProperties();

				if (string.IsNullOrWhiteSpace(properties.RedirectUri))
				{
					properties.RedirectUri = GetRedirectUri(binding, options);
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("RedirectUri={0}", properties.RedirectUri));

				Response.Redirect(properties.RedirectUri);

				return;
			}
		}

		protected virtual async Task ApplyRequestLogoutAsync()
		{
			var options = Options as Saml2AuthenticationOptions;

			if (options == null)
			{
				return;
			}

			if (_configuration == null)
			{
				_configuration = await options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
			}

			var issuer = options.Wtrealm;
			var destination = _configuration.TokenEndpoint ?? string.Empty;

			var request = Context.Get<HttpContextBase>(typeof(HttpContextBase).FullName).Request;

			foreach (var signingKey in _configuration.SigningKeys.OfType<X509SecurityKey>())
			{
				Saml2StatusCodes status;

				var requestBinding = new Saml2PostBinding();
				var logoutRequest = new Saml2LogoutRequest();

				try
				{
					try
					{
						requestBinding.Unbind(request, logoutRequest, signingKey.Certificate);
					}
					catch (Saml2ResponseException)
					{
						continue;
					}

					status = Saml2StatusCodes.Success;
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
					status = Saml2StatusCodes.RequestDenied;
				}

				var responsebinding = new Saml2RedirectBinding { RelayState = requestBinding.RelayState };

				var saml2LogoutResponse = new Saml2LogoutResponse
				{
					InResponseTo = logoutRequest.Id,
					Status = status,
					Issuer = new EndpointReference(issuer),
					Destination = new EndpointAddress(destination)
				};

				Context.Authentication.SignOut();

				var redirectBinding = responsebinding.Bind(saml2LogoutResponse, options.SigningCertificate);
				var redirectLocation = redirectBinding.RedirectLocation.AbsoluteUri;

				if (!Uri.IsWellFormedUriString(redirectLocation, UriKind.Absolute))
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("The sign-out redirect URI is malformed: {0}", redirectLocation));
				}

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("redirectLocation={0}", redirectLocation));

				Response.Redirect(redirectLocation);
			}
		}

		protected override async Task ApplyResponseChallengeAsync()
		{
			if (Response.StatusCode != 401)
			{
				return;
			}

			var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

			if (challenge == null)
			{
				return;
			}

			var options = Options as Saml2AuthenticationOptions;

			if (options == null)
			{
				return;
			}

			if (_configuration == null)
			{
				_configuration = await options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
			}

			var issuer = options.Wtrealm;
			var destination = _configuration.TokenEndpoint ?? string.Empty;
			var assertionConsumerServiceUrl = options.Wreply;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("issuer={0}, destination={1}, assertionConsumerServiceUrl={2}", issuer, destination, assertionConsumerServiceUrl));

			var properties = challenge.Properties;

			if (string.IsNullOrEmpty(properties.RedirectUri))
			{
				properties.RedirectUri = GetCurrentUri();
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("RedirectUri={0}", properties.RedirectUri));

			var state = new Dictionary<string, string>
			{
				{ _relayStateWctx, Options.StateDataFormat.Protect(properties) }
			};

			var binding = new Saml2RedirectBinding();
			binding.SetRelayStateQuery(state);

			var redirectBinding = binding.Bind(new Saml2AuthnRequest
			{
				ForceAuthn = options.ForceAuthn,
				NameIdPolicy = options.NameIdPolicy,

				RequestedAuthnContext = new RequestedAuthnContext
				{
					Comparison = options.Comparison,
					AuthnContextClassRef = options.AuthnContextClassRef,
				},

				Issuer = new EndpointReference(issuer),
				Destination = new EndpointAddress(destination),
				AssertionConsumerServiceUrl = new EndpointAddress(assertionConsumerServiceUrl)
			});

			var redirectLocation = redirectBinding.RedirectLocation.AbsoluteUri;

			if (!Uri.IsWellFormedUriString(redirectLocation, UriKind.Absolute))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("The sign-in redirect URI is malformed: {0}", redirectLocation));
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("redirectLocation={0}", redirectLocation));

			Response.Redirect(redirectLocation);
		}

		protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			var options = Options as Saml2AuthenticationOptions;

			if (options == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "AuthenticateCoreAsync:options == null");

				return null;
			}

			if (options.CallbackPath.HasValue && options.CallbackPath != (Request.PathBase + Request.Path))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application,
					string.Format(
						"AuthenticateCoreAsync:options.CallbackPath.HasValue && options.CallbackPath != (Request.PathBase: {0} + Request.Path: {1})",
						Request.PathBase, Request.Path));
				return null;
			}

			if (_configuration == null)
			{
				_configuration = await options.ConfigurationManager.GetConfigurationAsync(Context.Request.CallCancelled);
			}

			if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrWhiteSpace(Request.ContentType)
				&& Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
				&& Request.Body.CanRead)
			{
				var request = GetHttpRequestBase();
				var signingKeys = _configuration.SigningKeys.OfType<X509SecurityKey>().ToList();
				var totalKeys = signingKeys.Count;
				var keyIndex = 0;
				foreach (var signingKey in signingKeys)
				{
					keyIndex++;
					var binding = new Saml2PostBinding();
					Saml2AuthnResponse response = null;

					try
					{
						response = binding.Unbind(request, GetSaml2AuthnResponse(options), signingKey.Certificate) as Saml2AuthnResponse;
					}
					catch (Saml2ResponseException saml2ResponseException)
					{
						WebEventSource.Log.GenericWarningException(saml2ResponseException);
					}
					
					if (response == null || response.Status != Saml2StatusCodes.Success) continue;

					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						string.Format("Received the response for signing key with index:{0} out of:{1}", keyIndex, totalKeys));


					var relayState = binding.GetRelayStateQuery();
					var properties = relayState.ContainsKey(_relayStateWctx)
						? Options.StateDataFormat.Unprotect(relayState[_relayStateWctx]) ?? new AuthenticationProperties()
						: new AuthenticationProperties();

					if (string.IsNullOrWhiteSpace(properties.RedirectUri))
					{
						properties.RedirectUri = GetRedirectUri(binding, options);
					}

					var claimsIdentity = new ClaimsIdentity(response.ClaimsIdentity.Claims, options.TokenValidationParameters.AuthenticationType, response.ClaimsIdentity.NameClaimType, response.ClaimsIdentity.RoleClaimType);
					var ticket = new AuthenticationTicket(claimsIdentity, properties);

					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Identity={0}", ticket.Identity.Name));
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("RedirectUri={0}", ticket.Properties.RedirectUri));

					if (options.UseTokenLifetime)
					{
						var issued = response.Saml2SecurityToken.ValidFrom;

						if (issued != DateTime.MinValue)
						{
							ticket.Properties.IssuedUtc = issued.ToUniversalTime();
						}

						var expires = response.Saml2SecurityToken.ValidTo;

						if (expires != DateTime.MinValue)
						{
							ticket.Properties.ExpiresUtc = expires.ToUniversalTime();
						}

						ticket.Properties.AllowRefresh = false;
					}

					return ticket;
				}
				ADXTrace.Instance.TraceWarning(TraceCategory.Application,
					string.Format("No response received for any signing keys. totalKeys found:{0}", totalKeys));

			}

			return null;
		}

		/// <summary>
		/// This function return HttpRequestBase. Make sure that request is ready to use before first usage.
		/// </summary>
		/// <returns>return HttpRequestBase</returns>
		private HttpRequestBase GetHttpRequestBase()
		{
			var httpRequestBase = Context.Get<HttpContextBase>(typeof(HttpContextBase).FullName).Request;

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("ReadEntityBodyMode={0}", httpRequestBase.ReadEntityBodyMode));

			if (httpRequestBase.ReadEntityBodyMode == ReadEntityBodyMode.Buffered)
			{
				var inputstream = httpRequestBase.GetBufferedInputStream();

				if (inputstream != null)
				{
					//read stream to the end
					using (var stream = new StreamReader(inputstream))
					{
						stream.ReadToEnd();
						//make sure that input stream is ready
						var ignore = httpRequestBase.InputStream;
					}
				}
			}

			return httpRequestBase;
		}

		private static Saml2AuthnResponse GetSaml2AuthnResponse(Saml2AuthenticationOptions options)
		{
			var handler = new Saml2ResponseSecurityTokenHandler
			{
				Configuration = new SecurityTokenHandlerConfiguration
				{
					SaveBootstrapContext = false,
					AudienceRestriction = GetAudienceRestriction(options),
					IssuerNameRegistry = new Saml2ResponseIssuerNameRegistry(),
					CertificateValidationMode = X509CertificateValidationMode.None,
					RevocationMode = X509RevocationMode.NoCheck,
					CertificateValidator = options.TokenValidationParameters.CertificateValidator ?? X509CertificateValidator.None,
					DetectReplayedTokens = false,
				},
				SamlSecurityTokenRequirement = { NameClaimType = ClaimTypes.NameIdentifier }
			};

			return new InternalSaml2AuthnResponse(handler);
		}

		private static AudienceRestriction GetAudienceRestriction(Saml2AuthenticationOptions options)
		{
			if (!options.TokenValidationParameters.ValidateAudience) return new AudienceRestriction(AudienceUriMode.Never);

			var audienceRestriction = new AudienceRestriction(AudienceUriMode.Always);

			if (!string.IsNullOrWhiteSpace(options.Wtrealm))
			{
				audienceRestriction.AllowedAudienceUris.Add(new Uri(options.Wtrealm));
			}

			if (!string.IsNullOrWhiteSpace(options.TokenValidationParameters.ValidAudience))
			{
				audienceRestriction.AllowedAudienceUris.Add(new Uri(options.TokenValidationParameters.ValidAudience));
			}

			if (options.TokenValidationParameters.ValidAudiences != null)
			{
				foreach (var audience in options.TokenValidationParameters.ValidAudiences)
				{
					audienceRestriction.AllowedAudienceUris.Add(new Uri(audience));
				}
			}

			return audienceRestriction;
		}

		private static string GetRedirectUri(Saml2Binding binding, Saml2AuthenticationOptions options)
		{
			var relayState = binding.GetRelayStateQuery();

			if (relayState.ContainsKey(_relayStateRedirectUri))
			{
				var redirectUri = relayState[_relayStateRedirectUri];
				return redirectUri;
			}

			if (relayState.ContainsKey(_relayStateReturnUrl))
			{
				var returnUrl = relayState[_relayStateReturnUrl];
				var redirectUri = options.ExternalLoginCallbackPath.Add(new QueryString(_relayStateReturnUrl, returnUrl));
				return redirectUri;
			}

			return options.ExternalLoginCallbackPath.Value;
		}

		private string GetCurrentUri()
		{
			var baseUri = Request.Scheme + Uri.SchemeDelimiter + Request.Host + Request.PathBase;
			return baseUri + Request.Path + Request.QueryString;
		}
	}
}
