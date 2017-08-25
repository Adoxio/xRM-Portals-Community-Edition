/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Areas.Service311
{
	public static class ServiceRequestHelpers
	{
		public static string BuildServiceRequestTypeThumbnailImageTag(OrganizationServiceContext context, Entity entity, string cssClass = null)
		{
			var imageUrl = GetServiceRequestTypeThumbnailImageUrl(context, entity);

			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return string.Empty;
			}

			var tag = new TagBuilder("img");

			tag.Attributes["src"] = imageUrl;
			tag.Attributes["alt"] = entity.GetAttributeValue<string>("adx_name");

			if (!string.IsNullOrEmpty(cssClass))
			{
				tag.AddCssClass(cssClass);
			}

			return tag.ToString(TagRenderMode.SelfClosing);
		}

		public static string GetServiceRequestTypeThumbnailImageUrl(OrganizationServiceContext context, Entity entity)
		{
			if (entity == null)
			{
				return string.Empty;
			}

			var url = entity.GetAttributeValue<string>("adx_thumbnailimageurl");

			if (string.IsNullOrWhiteSpace(url))
			{
				return string.Empty;
			}

			if (url.StartsWith("http", true, CultureInfo.InvariantCulture))
			{
				return url;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			return WebsitePathUtility.ToAbsolute(portalContext.Website, url);
		}

		public static string GetPushpinImageUrl(OrganizationServiceContext context, Entity entity)
		{
			var serviceRequestType = entity.GetAttributeValue<EntityReference>("adx_servicerequesttype");

			if (serviceRequestType == null)
			{
				return string.Empty;
			}

			var type = context.CreateQuery("adx_servicerequesttype").FirstOrDefault(s => s.GetAttributeValue<Guid>("adx_servicerequesttypeid") == serviceRequestType.Id);

			if (type == null)
			{
				return string.Empty;
			}

			var url = type.GetAttributeValue<string>("adx_mapiconimageurl");

			if (string.IsNullOrWhiteSpace(url))
			{
				return string.Empty;
			}

			if (url.StartsWith("http", true, CultureInfo.InvariantCulture))
			{
				return url;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			return WebsitePathUtility.ToAbsolute(portalContext.Website, url);
		}

		public static string GetAlertPushpinImageUrl()
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var context = PortalCrmConfigurationManager.CreateServiceContext();
			var website = context.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == portalContext.Website.Id);
			var alertMapIconUrl = context.GetSiteSettingValueByName(website, "ServiceRequestMap/AlertMapIconUrl");

			if (string.IsNullOrWhiteSpace(alertMapIconUrl))
			{
				return string.Empty;
			}

			if (alertMapIconUrl.StartsWith("http", true, CultureInfo.InvariantCulture))
			{
				return alertMapIconUrl;
			}

			return WebsitePathUtility.ToAbsolute(website, alertMapIconUrl);
		}

		public static string GetCheckStatusUrl(Entity serviceRequest)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var context = PortalCrmConfigurationManager.CreateServiceContext();
			var website = context.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == portalContext.Website.Id);
			var statusPage = context.GetPageBySiteMarkerName(website, "check-status");

			if (statusPage == null)
			{
				return string.Empty;
			}

			var statusUrl = new UrlBuilder(context.GetUrl(statusPage));

			statusUrl.QueryString.Add("refnum", serviceRequest.GetAttributeValue<string>("adx_servicerequestnumber"));

			return WebsitePathUtility.ToAbsolute(website, statusUrl.PathWithQueryString);
		}
	}
}
