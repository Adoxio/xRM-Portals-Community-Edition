/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Mvc
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.Security.Cryptography;
	using System.Text;
	using Adxstudio.Xrm.AspNet.Cms;
	using Adxstudio.Xrm.AspNet.Identity;
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.Owin;
	using Microsoft.Owin;
	using Microsoft.Owin.Security.Facebook;
	using Newtonsoft.Json.Linq;

	public class AuthenticationSettings
	{
		public bool IsDemoMode { get; set; }
		public bool RegistrationEnabled { get; set; }
		public bool InvitationEnabled { get; set; }
		public bool OpenRegistrationEnabled { get; set; }
		public bool LocalLoginEnabled { get; set; }
		public bool LocalLoginByEmail { get; set; }
		public bool RequireUniqueEmail { get; set; }
		public bool ExternalLoginEnabled { get; set; }
		public bool RememberMeEnabled { get; set; }
		public bool RememberBrowserEnabled { get; set; }
		public bool ResetPasswordEnabled { get; set; }
		public bool ResetPasswordRequiresConfirmedEmail { get; set; }
		public bool TriggerLockoutOnFailedPassword { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public bool MobilePhoneEnabled { get; set; }
		public bool EmailConfirmationEnabled { get; set; }
		public bool SignOutEverywhereEnabled { get; set; }
		public string LoginButtonAuthenticationType { get; set; }
		public bool ProfileRedirectEnabled { get; set; }
		public bool LoginTrackingEnabled { get; set; }
		public bool AzureADLoginEnabled { get; set; }

		public bool IsCaptchaEnabledForRegistration { get; set; }


	}

	public static class Extensions
	{
		public static AuthenticationSettings GetAuthenticationSettings(this CrmWebsite website, bool isLocal = false)
		{
			if (website == null) throw new ArgumentNullException("website");

			return new AuthenticationSettings
			{
				IsDemoMode = isLocal && website.Settings.Get<bool?>("Authentication/Registration/IsDemoMode").GetValueOrDefault(false),
				RegistrationEnabled = website.Settings.Get<bool?>("Authentication/Registration/Enabled").GetValueOrDefault(true),
				InvitationEnabled = website.Settings.Get<bool?>("Authentication/Registration/InvitationEnabled").GetValueOrDefault(true),
				OpenRegistrationEnabled = website.Settings.Get<bool?>("Authentication/Registration/OpenRegistrationEnabled").GetValueOrDefault(true),
				LocalLoginEnabled = website.Settings.Get<bool?>("Authentication/Registration/LocalLoginEnabled").GetValueOrDefault(false),
				LocalLoginByEmail = website.Settings.Get<bool?>("Authentication/Registration/LocalLoginByEmail").GetValueOrDefault(false),
				RequireUniqueEmail = website.Settings.Get<bool?>("Authentication/UserManager/UserValidator/RequireUniqueEmail").GetValueOrDefault(true),
				ExternalLoginEnabled = website.Settings.Get<bool?>("Authentication/Registration/ExternalLoginEnabled").GetValueOrDefault(true),
				RememberMeEnabled = website.Settings.Get<bool?>("Authentication/Registration/RememberMeEnabled").GetValueOrDefault(true),
				RememberBrowserEnabled = website.Settings.Get<bool?>("Authentication/Registration/RememberBrowserEnabled").GetValueOrDefault(true),
				ResetPasswordEnabled = website.Settings.Get<bool?>("Authentication/Registration/ResetPasswordEnabled").GetValueOrDefault(true),
				ResetPasswordRequiresConfirmedEmail = website.Settings.Get<bool?>("Authentication/Registration/ResetPasswordRequiresConfirmedEmail").GetValueOrDefault(false),
				TriggerLockoutOnFailedPassword = website.Settings.Get<bool?>("Authentication/Registration/TriggerLockoutOnFailedPassword").GetValueOrDefault(true),
				TwoFactorEnabled = website.Settings.Get<bool?>("Authentication/Registration/TwoFactorEnabled").GetValueOrDefault(false),
				MobilePhoneEnabled = website.Settings.Get<bool?>("Authentication/Registration/MobilePhoneEnabled").GetValueOrDefault(false),
				EmailConfirmationEnabled = website.Settings.Get<bool?>("Authentication/Registration/EmailConfirmationEnabled").GetValueOrDefault(true),
				SignOutEverywhereEnabled = website.Settings.Get<bool?>("Authentication/Registration/SignOutEverywhereEnabled").GetValueOrDefault(true),
				LoginButtonAuthenticationType = website.Settings.Get<string>("Authentication/Registration/LoginButtonAuthenticationType"),
				LoginTrackingEnabled = website.Settings.Get<bool?>("Authentication/LoginTrackingEnabled").GetValueOrDefault(false),
				ProfileRedirectEnabled = website.Settings.Get<bool?>("Authentication/Registration/ProfileRedirectEnabled").GetValueOrDefault(true),
				AzureADLoginEnabled = website.Settings.Get<bool?>("Authentication/Registration/AzureADLoginEnabled").GetValueOrDefault(true),
				IsCaptchaEnabledForRegistration = website.Settings.Get<bool?>("Authentication/Registration/CaptchaEnabled").GetValueOrDefault(false)
			};
		}

		public static CrmIdentityErrorDescriber GetIdentityErrors(this CrmWebsite website, IOwinContext context, bool isLocal = false)
		{
			if (website == null) throw new ArgumentNullException("website");

			return new CrmIdentityErrorDescriber(context);
		}

		public static UserLoginInfo GetFacebookAuthenticationType(this CrmWebsite website)
		{
			if (website == null) throw new ArgumentNullException("website");

			var authenticationType = website.Settings.Get<string>("Authentication/OpenAuth/Facebook/AuthenticationType")
				?? new FacebookAuthenticationOptions().AuthenticationType;

			return new UserLoginInfo(authenticationType, null);
		}

		public static ExternalLoginInfo GetFacebookLoginInfo(this CrmWebsite website, string signedRequest)
		{
			if (website == null) throw new ArgumentNullException("website");
			if (signedRequest == null) throw new ArgumentNullException("signedRequest");

			var token = GetFacebookToken(website, signedRequest);
			var userId = token.GetValue("user_id");

			if (userId == null) return null;

			var authenticationType = GetFacebookAuthenticationType(website);
			var providerKey = userId.Value<string>();
			var issuer = authenticationType.LoginProvider;
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, providerKey, null, issuer, issuer)
			};

			if (!string.IsNullOrWhiteSpace(authenticationType.ProviderKey))
			{
				claims.Add(new Claim(IdentityModel.Claims.ClaimTypes.IdentityProvider, authenticationType.ProviderKey, null, issuer, issuer));
			}

			var login = new ExternalLoginInfo
			{
				ExternalIdentity = new ClaimsIdentity(claims),
				DefaultUserName = providerKey,
				Login = new UserLoginInfo(issuer, providerKey),
			};

			return login;
		}

		public static JObject GetFacebookToken(this CrmWebsite website, string signedRequest)
		{
			if (website == null) throw new ArgumentNullException("website");
			if (signedRequest == null) throw new ArgumentNullException("signedRequest");

			var secret = website.Settings.Get<string>("Authentication/OpenAuth/Facebook/AppSecret")
				?? website.Settings.Get<string>("Authentication/OpenAuth/Facebook/ClientSecret");

			if (string.IsNullOrWhiteSpace(secret)) return null;

			return DecodeSignedRequest(signedRequest, secret);
		}

		private static JObject DecodeSignedRequest(string signedRequest, string secret)
		{
			var parts = signedRequest.Split('.');

			if (parts.Length != 2) return null;
			
			var encodedSig = parts[0];
			var payload = parts[1];

			if (string.IsNullOrWhiteSpace(encodedSig) || string.IsNullOrWhiteSpace(payload)) return null;

			// verify the signature by hashing the payload with the secret

			var key = Encoding.UTF8.GetBytes(secret);
			var buffer = Encoding.UTF8.GetBytes(payload);

			using (var crypto = new HMACSHA256(key))
			{
				var hash = crypto.ComputeHash(buffer);
				var decodedSig = Decode(encodedSig);

				if (hash.Length != decodedSig.Length || !hash.SequenceEqual(decodedSig)) return null;

				var decodedPayload = Decode(payload);
				var json = Encoding.UTF8.GetString(decodedPayload);
				var token = JObject.Parse(json);

				return token;
			}
		}

		private static byte[] Decode(string text)
		{
			var padded = text.PadRight(text.Length + (4 - text.Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/');
			var decoded = Convert.FromBase64String(padded);

			return decoded;
		}
	}
}
