/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web
{
	public static class WebsitePathUtility
	{
		/// <summary>
		/// Converts the given URL path into a website specific path.
		/// </summary>
		public static string ToAbsolute(Entity website, string urlPath)
		{
			website.AssertEntityName("adx_website");

			if (urlPath == null) return null;

			var websitePath = website.GetAttributeValue<string>("adx_partialurl");

			websitePath = websitePath == null ? string.Empty : websitePath.TrimStart('/');

			return string.IsNullOrEmpty(websitePath)
				? urlPath
				: "/{0}{1}".FormatWith(websitePath, ToRelative(website, urlPath));
		}

		/// <summary>
		/// Removes the website's path from the given URL path.
		/// </summary>
		public static string ToRelative(Entity website, string urlPath)
		{
			return ToRelative(website, (UrlBuilder)urlPath).Path;
		}

		/// <summary>
		/// Removes the website's path from the given URL.
		/// </summary>
		public static UrlBuilder ToRelative(Entity website, UrlBuilder url)
		{
			website.AssertEntityName("adx_website");

			url.ThrowOnNull("url");

			var clonedUrl = url.Clone();

			var websitePath = website.GetAttributeValue<string>("adx_partialurl");

			if (string.IsNullOrEmpty(websitePath))
			{
				return clonedUrl;
			}

			websitePath = websitePath.TrimStart('/');

			var trimmedUrlPath = clonedUrl.Path.TrimStart('/');

			if (trimmedUrlPath.StartsWith(websitePath, StringComparison.InvariantCultureIgnoreCase))
			{
				clonedUrl.Path = trimmedUrlPath.Substring(websitePath.Length);

				clonedUrl.QueryString.Set("websitepath", websitePath);
			}

			return clonedUrl;
		}
	}
}
