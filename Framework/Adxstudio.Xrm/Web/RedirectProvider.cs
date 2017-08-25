/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// An <see cref="IRedirectProvider"/> which implements its <see cref="Match"/> based on CRM-managed adx_redirect entities
	/// associated with a given adx_website.
	/// </summary>
	public class RedirectProvider : IRedirectProvider
	{
		public string PortalName { get; private set; }

		public RedirectProvider(string portalName)
		{
			PortalName = portalName;
		}

		protected OrganizationServiceContext CreateContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public IRedirectMatch Match(Guid websiteID, UrlBuilder url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url");
			}

			var appRelativePath = VirtualPathUtility.ToAppRelative(url.PathWithQueryString);

			using (var context = CreateContext())
			{
				var redirect = context.CreateQuery("adx_redirect")
					.Where(r => r.GetAttributeValue<EntityReference>("adx_websiteid") == new EntityReference("adx_website", websiteID))
					.ToList().FirstOrDefault(
						r => {
							// assume an inbound path that is partial should be an app relative path instead

							var inboundPath = r.GetAttributeValue<string>("adx_inboundurl");
							var isPartial = !inboundPath.StartsWith("/") && !inboundPath.StartsWith("~");
							var appRelativeInboundPath = VirtualPathUtility.ToAppRelative((isPartial ? "~/" : string.Empty) + inboundPath);

							return string.Equals(appRelativePath, appRelativeInboundPath, StringComparison.OrdinalIgnoreCase);
						});

				if (redirect == null)
				{
					return new FailedRedirectMatch();
				}

				var inboundURL = redirect.GetAttributeValue<string>("adx_inboundurl");

				var redirectURL = redirect.GetAttributeValue<string>("adx_redirecturl");

				// Eliminate infinite loop redirecting to itself.
				if (inboundURL == redirectURL)
				{
					return new FailedRedirectMatch();
				}

				var statusOption = redirect.GetAttributeValue<OptionSetValue>("adx_statuscode");
				var statusCode = statusOption == null ? (int)HttpStatusCode.Redirect : statusOption.Value;

				if (!string.IsNullOrEmpty(redirectURL))
				{
					return new RedirectMatch(statusCode, redirectURL);
				}

				var page = redirect.GetRelatedEntity(context, "adx_webpage_redirect");

				if (page != null)
				{
					var pageUrl = context.GetUrl(page);

					if (!string.IsNullOrWhiteSpace(pageUrl))
					{
						return new RedirectMatch(statusCode, pageUrl);
					}
				}

				var siteMarker = redirect.GetRelatedEntity(context, "adx_sitemarker_redirect");

				if (siteMarker != null)
				{
					page = siteMarker.GetRelatedEntity(context, "adx_webpage_sitemarker");

					if (page != null)
					{
						var pageUrl = context.GetUrl(page);

						if (!string.IsNullOrWhiteSpace(pageUrl))
						{
							return new RedirectMatch(statusCode, pageUrl);
						}
					}
				}

				return new FailedRedirectMatch();
			}
		}
	}
}
