/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Adxstudio.Xrm.Core.Telemetry.EventSources;
using Microsoft.Owin;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Security
{
	internal static class WebNotificationCryptography
	{
		private const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
		private const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
		private const int SaltSize = 128 / 8; // 128 bits

		/* =======================
		 * HASHED PASSWORD FORMATS
		 * =======================
		 * 
		 * Version 0:
		 * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
		 * (See also: SDL crypto guidelines v5.1, Part III)
		 * Format: { 0x00, salt, subkey }
		 */

		/// <summary>
		/// Decrypts the the hashed token and checks if it matches the clear text token to determin if it is valid.
		/// </summary>
		/// <param name="secureToken">Hashed token.</param>
		/// <param name="token">Clear text token to validate the hashed token.</param>
		/// <returns>Returns true if hashed token decrypts to match the clear text token string, otherwise returns false.</returns>
		public static bool ValidateToken(string secureToken, string token)
		{
			if (secureToken == null)
			{
				WebNotificationEventSource.Log.SecureTokenInvalid(string.Empty);

				throw new ArgumentNullException("secureToken");
			}

			if (token == null)
			{
				throw new ArgumentNullException("token");
			}

			byte[] actualSubkey;
			var outputBytes = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
			byte[] hashedToken;
			try
			{
				hashedToken = Convert.FromBase64String(secureToken);
			}
			catch (Exception)
			{
				WebNotificationEventSource.Log.SecureTokenInvalid(secureToken);

				return false;
			}

			// We know ahead of time the exact length of a valid hashed token payload.
			if ((hashedToken.Length != outputBytes.Length) || (hashedToken[0] != 0))
			{
				WebNotificationEventSource.Log.SecureTokenInvalid(secureToken);

				return false; // bad size
			}

			byte[] salt = new byte[SaltSize];
			Buffer.BlockCopy(hashedToken, 1, salt, 0, SaltSize);

			byte[] expectedSubkey = new byte[Pbkdf2SubkeyLength];
			Buffer.BlockCopy(hashedToken, 1 + SaltSize, expectedSubkey, 0, Pbkdf2SubkeyLength);

			// Hash the incoming token and verify it
			using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(token, salt, Pbkdf2IterCount))
			{
				actualSubkey = bytes.GetBytes(Pbkdf2SubkeyLength);
			}
			return ByteArraysEqual(expectedSubkey, actualSubkey);
		}

		/// <summary>
		/// Inspects the Authorization header on the HTTP Request to determine if the request contains a valid token to authorize the request to invalidate the cache and rebuild the search index.
		/// </summary>
		/// <param name="requestAuthorization"></param>
		/// <returns>Returns true if the request has a valid token, otherwise returns false.</returns>
		public static bool ValidateRequest(string requestAuthorization)
		{
			// Example Authorization header:
			// Basic [TOKEN];[adx_webnotificationurlid]
			// Basic AFFb8amKu6/bmS1/v6XaaezfzC54Cs+1rZs5puaJgtD8KwgCNVo4tbYNCpvhlBAsQw==;47558300-51f0-e511-80d4-00155db24d10

			if (string.IsNullOrWhiteSpace(requestAuthorization))
			{
				WebNotificationEventSource.Log.AuthorizationHeaderMissing();

				return false;
			}

			var tokenDelimiterIndex = requestAuthorization.IndexOf(';');
			if (tokenDelimiterIndex < 0)
			{
				WebNotificationEventSource.Log.AuthorizationHeaderInvalid();

				return false;
			}

			var hashedToken = requestAuthorization.Substring(6, tokenDelimiterIndex - 6);
			var webNotificationUrlIdString = requestAuthorization.Substring(tokenDelimiterIndex + 1);

			Guid webNotificationUrlId;
			if (!Guid.TryParse(webNotificationUrlIdString, out webNotificationUrlId))
			{
				WebNotificationEventSource.Log.AuthorizationHeaderInvalid();

				return false;
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			var webNotificationUrl =
				serviceContext.CreateQuery("adx_webnotificationurl")
					.FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_webnotificationurlid") == webNotificationUrlId);

			if (webNotificationUrl == null)
			{
				WebNotificationEventSource.Log.WebNotificationUrlRecordNotFound(webNotificationUrlId);

				return false;
			}

			var token = webNotificationUrl.GetAttributeValue<string>("adx_token");

			if (string.IsNullOrWhiteSpace(token))
			{
				WebNotificationEventSource.Log.WebNotificationUrlTokenMissing(webNotificationUrlId);

				return false;
			}

			return ValidateToken(hashedToken, token);
		}

		private static bool ByteArraysEqual(byte[] a, byte[] b)
		{
			if (a == null && b == null)
			{
				return true;
			}

			if (a == null || b == null || a.Length != b.Length)
			{
				return false;
			}

			var areSame = true;

			for (var i = 0; i < a.Length; i++)
			{
				areSame &= (a[i] == b[i]);
			}

			return areSame;
		}
	}
}
