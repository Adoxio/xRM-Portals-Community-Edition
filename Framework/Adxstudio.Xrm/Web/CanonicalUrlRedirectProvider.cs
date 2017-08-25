/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Provides redirect logic for canonical web page URLs. Web page URLs must have a trailing slash -- if we
	/// can find a matching we page by appending a slash to the failed request URL, redirect to that page.
	/// </summary>
	public class CanonicalUrlRedirectProvider : IRedirectProvider
	{
		public IRedirectMatch Match(Guid websiteID, UrlBuilder url)
		{
			// If the path already ends with '/', we have nothing to try.
			if (url.Path.EndsWith("/"))
			{
				return new FailedRedirectMatch();
			}

			var node = SiteMap.Provider.FindSiteMapNode(url.Path + "/");

			if (node == null)
			{
				return new FailedRedirectMatch();
			}

			// Build a redirect URL from the site map node URL.
			var redirectUrl = new UrlBuilder(node.Url);

			// Preserve any querystring values in the original URL in the redirect URL -- unless the redirect URL
			// has a query key with the same name, in which case the value in the redirect URL takes precedence.
			foreach (var key in url.QueryString.AllKeys)
			{
				var redirectUrlValue = redirectUrl.QueryString[key];

				if (string.IsNullOrEmpty(redirectUrlValue))
				{
					redirectUrl.QueryString.Set(key, url.QueryString[key]);
				}
			}

			// Preserve any fragment on the original URL, if the redirect URL has none.
			if (!string.IsNullOrEmpty(url.Fragment) && string.IsNullOrEmpty(redirectUrl.Fragment))
			{
				redirectUrl.Fragment = url.Fragment;
			}

			var entityNode = node as CrmSiteMapNode;

			// If the found node was not one for an adx_webpage, we'll still be helpful and redirect, but won't
			// have the confidence to say MovedPermanently.
			if (entityNode == null || entityNode.Entity == null || entityNode.Entity.LogicalName != "adx_webpage")
			{
				return new RedirectMatch(HttpStatusCode.Redirect, redirectUrl.PathWithQueryString);
			}

			// If we found a page but don't have access to it, redirect to the URL with terminating slash and
			// let the access denied/sign-in redirect infra handle things from here.
			if (entityNode.StatusCode == HttpStatusCode.Forbidden)
			{
				var forbiddenUrl = new UrlBuilder(url) { Path = url.Path + "/" };

				// Preserve any querystring values in the original URL in the redirect URL.
				foreach (var key in url.QueryString.AllKeys)
				{
					forbiddenUrl.QueryString.Set(key, url.QueryString[key]);
				}

				return new RedirectMatch(HttpStatusCode.Redirect, forbiddenUrl.PathWithQueryString);
			}

			// If we got the 404 page, don't redirect.
			if (entityNode.StatusCode == HttpStatusCode.NotFound)
			{
				return new FailedRedirectMatch();
			}

			// It's an adx_webpage, redirect permanently to the canonical URL.
			return new RedirectMatch(HttpStatusCode.MovedPermanently, redirectUrl.PathWithQueryString);
		}
	}
}
