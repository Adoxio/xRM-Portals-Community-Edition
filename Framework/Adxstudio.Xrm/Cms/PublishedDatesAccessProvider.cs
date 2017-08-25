/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Security;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Configuration;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Cms
{
	internal class PublishedDatesAccessProvider : ContentMapAccessProvider
	{
		private readonly IContentMapProvider _contentMapProvider;

		public PublishedDatesAccessProvider(HttpContext context)
			: this(context != null ? context.GetContentMapProvider() : AdxstudioCrmConfigurationManager.CreateContentMapProvider())
		{
		}

		public PublishedDatesAccessProvider(IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
			_contentMapProvider = contentMapProvider;
		}

		/// <summary>
		/// Test whether or not an Entity's published dates are visible in the current context.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="entity">CRM Entity</param>
		/// <param name="right">Entity Right</param>
		/// <param name="dependencies">Cache Dependency Trace</param>
		/// <param name="map">Content Map</param>
		/// <returns></returns>
		protected override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			if (entity == null || right == CrmEntityRight.Change)
			{
				return false;
			}

			dependencies.AddEntityDependency(entity);
			dependencies.AddEntitySetDependency("adx_webrole");
			dependencies.AddEntitySetDependency("adx_websiteaccess");

			var entityName = entity.LogicalName;

			var releaseDate = DateTime.MinValue;
			var expiryDate = DateTime.MaxValue;

			if (entityName == "adx_webpage" ||
				entityName == "adx_webfile" ||
				entityName == "adx_event" ||
				entityName == "adx_survey" ||
				entityName == "adx_ad")
			{
				releaseDate = entity.GetAttributeValue<DateTime?>("adx_releasedate").GetValueOrDefault(DateTime.MinValue);
				expiryDate = entity.GetAttributeValue<DateTime?>("adx_expirationdate").GetValueOrDefault(DateTime.MaxValue);
			}
			else if (entityName == "adx_weblink")
			{
				if (!entity.GetAttributeValue<bool?>("adx_disablepagevalidation").GetValueOrDefault(false))
				{
					var pageReference = entity.GetAttributeValue<EntityReference>("adx_pageid");

					if (pageReference != null)
					{
						WebPageNode rootPage;
						if (map.TryGetValue(pageReference, out rootPage))
						{
							var weblinkWebPage = HttpContext.Current.GetContextLanguageInfo().FindLanguageSpecificWebPageNode(rootPage, false);

							if (weblinkWebPage != null)
							{
								return TryAssert(context, weblinkWebPage.ToEntity(), right, dependencies);
							}
						}
					}
				}

				return true;
			}
			else if (entityName == "adx_shortcut")
			{
				if (!entity.GetAttributeValue<bool?>("adx_disabletargetvalidation").GetValueOrDefault(false))
				{

					var pageReference = entity.GetAttributeValue<EntityReference>("adx_webpageid");
					var webFileReference = entity.GetAttributeValue<EntityReference>("adx_webfileid");

					if (pageReference != null)
					{
						WebPageNode rootPage;
						if (map.TryGetValue(pageReference, out rootPage))
						{
							var shortcutWebPage = HttpContext.Current.GetContextLanguageInfo().FindLanguageSpecificWebPageNode(rootPage, false);

							if (shortcutWebPage != null)
							{
								return TryAssert(context, shortcutWebPage.ToEntity(), right, dependencies);
							}
						}
					}
					else if (webFileReference != null)
					{
						WebFileNode webFile;
						if (map.TryGetValue(webFileReference, out webFile))
						{
							return TryAssert(context, webFile.ToEntity(), right, dependencies);
						}
					}
				}

				var parentPageReference = entity.GetAttributeValue<EntityReference>("adx_parentpage_webpageid");

				if (parentPageReference != null)
				{
					WebPageNode rootPage;
					if (map.TryGetValue(parentPageReference, out rootPage))
					{
						var parentPage = HttpContext.Current.GetContextLanguageInfo().FindLanguageSpecificWebPageNode(rootPage, false);

						return TryAssert(context, parentPage.ToEntity(), right, dependencies);
					}

				}

				return true;
			}

			return UserCanPreview(context, entity) || InnerTryAssert(releaseDate, expiryDate);
		}

		/// <summary>
		/// Test whether or not an Entity's published dates are visible in the current context.
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		/// <param name="entity">CRM Entity</param>
		/// <returns></returns>
		public virtual bool TryAssert(OrganizationServiceContext context, Entity entity)
		{
			return TryAssert(context, entity, CrmEntityRight.Read);
		}

		private static bool InnerTryAssert(DateTime? releaseDate, DateTime? expiryDate)
		{
			var now = DateTime.UtcNow;
			return (expiryDate ?? DateTime.MaxValue) >= now && (releaseDate ?? DateTime.MinValue) <= now;
		}

		private static bool UserCanPreview(OrganizationServiceContext context, Entity entity)
		{
			var website = context.GetWebsite(entity);

			if (website == null)
			{
				return false;
			}

			var preview = new PreviewPermission(context, website);

			return preview.IsEnabledAndPermitted;
		}
	}
}
