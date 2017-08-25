/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using Microsoft.Xrm.Portal.Configuration;

namespace Adxstudio.Xrm.Web.Handlers
{
	internal static class Utility
	{
		public static void SetResponseCachePolicy(
			HttpCachePolicyElement policy,
			HttpResponseBase response,
			HttpCacheability defaultCacheability,
			string defaultMaxAge = "01:00:00",
			string defaultVaryByParams = null,
			string defaultVaryByContentEncodings = null)
		{
			if (!string.IsNullOrWhiteSpace(policy.CacheExtension))
			{
				response.Cache.AppendCacheExtension(policy.CacheExtension);
			}

			var maxAge = policy.MaxAge ?? defaultMaxAge;

			if (!string.IsNullOrWhiteSpace(maxAge))
			{
				var maxAgeSpan = TimeSpan.Parse(maxAge);
				response.Cache.SetExpires(DateTime.Now.Add(maxAgeSpan));
				response.Cache.SetMaxAge(maxAgeSpan);
			}

			if (!string.IsNullOrWhiteSpace(policy.Expires))
			{
				var expires = DateTime.Parse(policy.Expires);
				response.Cache.SetExpires(expires);
			}

			var cacheability = policy.Cacheability != null ? policy.Cacheability.Value : defaultCacheability;

			response.Cache.SetCacheability(cacheability);

			if (policy.Revalidation != null)
			{
				response.Cache.SetRevalidation(policy.Revalidation.Value);
			}

			if (policy.SlidingExpiration != null)
			{
				response.Cache.SetSlidingExpiration(policy.SlidingExpiration.Value);
			}

			if (policy.ValidUntilExpires != null)
			{
				response.Cache.SetValidUntilExpires(policy.ValidUntilExpires.Value);
			}

			if (!string.IsNullOrWhiteSpace(policy.VaryByCustom))
			{
				response.Cache.SetVaryByCustom(policy.VaryByCustom);
			}

			// encoding based output caching should be disabled for annotations

			var varyByContentEncodings = policy.VaryByContentEncodings == null || policy.VaryByContentEncodings == "gzip;x-gzip;deflate"
				? defaultVaryByContentEncodings
				: policy.VaryByContentEncodings;

			if (!string.IsNullOrWhiteSpace(varyByContentEncodings))
			{
				ForEachParam(varyByContentEncodings, param => response.Cache.VaryByContentEncodings[param] = true);
			}

			if (!string.IsNullOrWhiteSpace(policy.VaryByHeaders))
			{
				ForEachParam(policy.VaryByHeaders, param => response.Cache.VaryByHeaders[param] = true);
			}

			var varyByParams = policy.VaryByParams ?? defaultVaryByParams;

			if (!string.IsNullOrWhiteSpace(varyByParams))
			{
				ForEachParam(varyByParams, param => response.Cache.VaryByParams[param] = true);
			}
		}

		private static void ForEachParam(string parameters, Action<string> action)
		{
			var split = SplitOnSemicolon(parameters);

			foreach (var parameter in split)
			{
				action(parameter);
			}
		}

		private static IEnumerable<string> SplitOnSemicolon(string parameters)
		{
			return parameters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());
		}

		public static string ComputeETag(Stream stream)
		{
			using (var sha256 = new SHA256CryptoServiceProvider())
			{
				var hash = Convert.ToBase64String(sha256.ComputeHash(stream));
				return hash;
			}
		}

		public static string ComputeETag(byte[] data)
		{
			using (var sha256 = new SHA256CryptoServiceProvider())
			{
				var hash = Convert.ToBase64String(sha256.ComputeHash(data));
				return hash;
			}
		}

		public static string ComputeETag(Assembly assembly, string resourceName)
		{
			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (resourceStream != null)
				{
					return ComputeETag(resourceStream);
				}
			}

			return null;
		}

		public static void Write(HttpResponseBase response, byte[] data)
		{
			response.OutputStream.Write(data, 0, data.Length);
		}
	}
}
