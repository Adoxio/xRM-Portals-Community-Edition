/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using DotLiquid;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class UrlFilters
	{
		private static readonly Uri PlaceholderBaseUri = new Uri("http://example.com/");

		public static string AddQuery(string url, string parameterName, object value)
		{
			if (url == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(parameterName))
			{
				return url;
			}

			Uri uri;

			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
				return null;
			}

			var inputIsAbsolute = uri.IsAbsoluteUri;

			try
			{
				var urlBuilder = new UrlBuilder(inputIsAbsolute ? uri : new Uri(PlaceholderBaseUri, uri));

				urlBuilder.QueryString.Set(parameterName, value == null ? string.Empty : value.ToString());

				return inputIsAbsolute ? urlBuilder.ToString() : urlBuilder.PathWithQueryString;
			}
			catch
			{
				return url;
			}
		}

		public static string Base(string url)
		{
			if (url == null)
			{
				return null;
			}

			Uri uri;

			return Uri.TryCreate(url, UriKind.Absolute, out uri)
				? uri.GetLeftPart(UriPartial.Authority)
				: null;
		}

		public static string RemoveQuery(string url, string parameterName)
		{
			if (url == null)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(parameterName))
			{
				return url;
			}

			Uri uri;

			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
				return null;
			}

			var inputIsAbsolute = uri.IsAbsoluteUri;

			try
			{
				var urlBuilder = new UrlBuilder(inputIsAbsolute ? uri : new Uri(PlaceholderBaseUri, uri));

				urlBuilder.QueryString.Remove(parameterName);

				return inputIsAbsolute ? urlBuilder.ToString() : urlBuilder.PathWithQueryString;
			}
			catch
			{
				return url;
			}
		}

		public static string Host(string url)
		{
			if (url == null)
			{
				return null;
			}

			try
			{
				return new UrlBuilder(url).Host;
			}
			catch
			{
				return null;
			}
		}

		public static string Path(string url)
		{
			if (url == null)
			{
				return null;
			}

			try
			{
				return new UrlBuilder(url).Path;
			}
			catch
			{
				return null;
			}
		}

		public static int? Port(string url)
		{
			if (url == null)
			{
				return null;
			}

			try
			{
				return new UrlBuilder(url).Port;
			}
			catch
			{
				return null;
			}
		}

		public static string PathAndQuery(string url)
		{
			if (url == null)
			{
				return null;
			}

			try
			{
				return new UrlBuilder(url).PathWithQueryString;
			}
			catch
			{
				return null;
			}
		}

		public static string Scheme(string url)
		{
			if (url == null)
			{
				return null;
			}

			try
			{
				return new UrlBuilder(url).Scheme;
			}
			catch
			{
				return null;
			}
		}

		public static bool IsSitemapAncestor(Context context, string url)
		{
			IPortalLiquidContext portalLiquidContext;

			return context.TryGetPortalLiquidContext(out portalLiquidContext)
				&& portalLiquidContext.PortalViewContext.IsAncestorSiteMapNode(url);
		}

		public static bool IsSitemapCurrent(Context context, string url)
		{
			IPortalLiquidContext portalLiquidContext;

			return context.TryGetPortalLiquidContext(out portalLiquidContext)
				&& portalLiquidContext.PortalViewContext.IsCurrentSiteMapNode(url);
		}
	}
}
