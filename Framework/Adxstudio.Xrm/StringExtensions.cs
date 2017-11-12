/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;

	/// <summary>
	/// Helper methods on the <see cref="string"/>/> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Get a substring of the first N characters.
		/// </summary>
		public static string Truncate(this string source, int length)
		{
			if (!string.IsNullOrEmpty(source) && source.Length > length)
			{
				source = source.Substring(0, length);
			}

			return source;
		}

		/// <summary>
		/// Retrieves the value of the current string, or if the string is null or consists of whitespace then returns the specified default value.
		/// </summary>
		public static string GetValueOrDefault(this string instance, string defaultValue = "")
		{
			return string.IsNullOrWhiteSpace(instance) ? defaultValue : instance;
		}

		/// <summary>
		/// Creates an absolute <see cref="Uri"/> from the given string, using provided base <see cref="Uri"/> if necessary
		/// </summary>
		/// <param name="uriString">String to be used as Uri</param>
		/// <param name="hostUri">Base <see cref="Uri"/>. Anything but scheme, host and port will be ignored</param>
		/// <returns>Absolute <see cref="Uri"/> constructed from the given Url string</returns>
		public static Uri AsAbsoluteUri(this string uriString, Uri hostUri)
		{
			var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
			if (uri.IsAbsoluteUri)
			{
				return uri;
			}

			var baseUri = new UriBuilder(hostUri.Scheme, hostUri.Host, hostUri.Port).Uri;
			return new Uri(baseUri, uri);
		}
	}
}
