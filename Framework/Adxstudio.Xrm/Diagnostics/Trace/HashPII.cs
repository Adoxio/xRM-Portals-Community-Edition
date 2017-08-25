/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Trace
{
	using System;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using Microsoft.AspNet.Identity;

	/// <summary>
	/// Class used to HASH personally identifying information (PII) with SHA256 encryption
	/// </summary>
	public class HashPii
	{
		/// <summary>
		/// Method used to HASH PII
		/// </summary>
		/// <param name="piiToBeHashed">PII field to HASH</param>
		/// <returns>Returns SHA256 hashed value</returns>
		public static byte[] ComputeHashPiiSha256(byte[] piiToBeHashed)
		{
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(piiToBeHashed);
			}
		}

		/// <summary>
		/// Returns the SHA256 hashed pii.
		/// </summary>
		/// <param name="piiToBeHashed">The pii to be hashed.</param>
		/// <returns>The hashed pii.</returns>
		public static string ComputeHashPiiSha256(string piiToBeHashed)
		{
			return !string.IsNullOrWhiteSpace(piiToBeHashed)
				? Convert.ToBase64String(ComputeHashPiiSha256(Encoding.UTF8.GetBytes(piiToBeHashed)))
				: string.Empty;
		}

		/// <summary>
		/// Returns the SHA256 hashed user ID.
		/// </summary>
		/// <param name="context">The web context.</param>
		/// <returns>The hashed user ID.</returns>
		public static string GetHashedUserId(HttpContextBase context)
		{
			return context != null && context.User.Identity.IsAuthenticated && !string.IsNullOrWhiteSpace(context.User.Identity.GetUserId())
				? ComputeHashPiiSha256(context.User.Identity.GetUserId())
				: string.Empty;
		}

		/// <summary>
		/// Returns the SHA256 hashed IP Address.
		/// </summary>
		/// <param name="context">The web context.</param>
		/// <returns>The hashed IP Address.</returns>
		public static string GetHashedIpAddress(HttpContextBase context)
		{
			return context == null || context.Request == null || context.Request.UserHostAddress == null
				? string.Empty
				: ComputeHashPiiSha256(context.Request.UserHostAddress);
		}
	}
}
