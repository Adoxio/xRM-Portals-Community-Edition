/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Collections.Generic
{
	/// <summary>
	/// Helper methods related to query string handling.
	/// </summary>
	public static class QueryStringExtensions
	{
		/// <summary>
		/// Builds a query string from a dictionary omitting the leading "?".
		/// </summary>
		public static string ToQueryString(this IDictionary<string, string> pairs)
		{
			if (pairs == null) return null;

			return string.Join("&", pairs.Select(kvp => "{0}={1}".FormatWith(HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode(kvp.Value))).ToArray());
		}

		/// <summary>
		/// Builds a query string from a collection omitting the leading "?".
		/// </summary>
		public static string ToQueryString(this NameValueCollection pairs)
		{
			if (pairs == null) return null;

			return string.Join("&", pairs.Cast<string>().Select(key => "{0}={1}".FormatWith(HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(pairs[key]))).ToArray());
		}

		/// <summary>
		/// Appends a query string from a dictionary to an existing URI.
		/// </summary>
		public static string AppendQueryString(this string baseUri, IDictionary<string, string> pairs)
		{
			if (pairs == null) return baseUri;

			return baseUri + GetSeparator(baseUri) + ToQueryString(pairs);
		}

		/// <summary>
		/// Appends a query string from a collection to an existing URI.
		/// </summary>
		public static string AppendQueryString(this string baseUri, NameValueCollection pairs)
		{
			if (pairs == null) return baseUri;

			return baseUri + GetSeparator(baseUri) + ToQueryString(pairs);
		}

		/// <summary>
		/// Appends a query string from a key/value pair to an existing URI.
		/// </summary>
		public static string AppendQueryString(this string baseUri, string key, string value)
		{
			return baseUri + GetSeparator(baseUri) + ToQueryString(new Dictionary<string, string> { { key, value } });
		}

		private static string GetSeparator(this string baseUri)
		{
			if (baseUri == null || !baseUri.Contains("?")) return "?";
			if (baseUri.EndsWith("?") || baseUri.EndsWith("&")) return string.Empty;
			return "&";
		}
	}
}
