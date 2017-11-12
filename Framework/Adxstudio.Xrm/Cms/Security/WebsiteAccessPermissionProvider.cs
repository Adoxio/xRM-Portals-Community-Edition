/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Security
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;

	internal class WebsiteAccessPermissionProvider : ContentMapAccessProvider, IWebsiteAccessPermissionProvider
	{
		private readonly WebPageAccessControlSecurityProvider _webPageAccessControlSecurityProvider;
		private readonly Entity _website;

		public WebsiteAccessPermissionProvider(Entity website, HttpContext context)
			: this(website, context != null ? context.GetContentMapProvider() : AdxstudioCrmConfigurationManager.CreateContentMapProvider())
		{
		}

		public WebsiteAccessPermissionProvider(
			Entity website,
			IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
			_website = website;
		}

		public WebsiteAccessPermissionProvider(
			Entity website,
			CacheSupportingCrmEntitySecurityProvider webPageAccessControlSecurityProvider,
			IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
			_webPageAccessControlSecurityProvider = webPageAccessControlSecurityProvider as WebPageAccessControlSecurityProvider;
			_website = website;
		}

		

		private static readonly IDictionary<WebsiteRight, Func<WebsiteAccessNode, bool?>> RightMappings = new Dictionary<WebsiteRight, Func<WebsiteAccessNode, bool?>>
		{
			{ WebsiteRight.ManageContentSnippets, site => site.ManageContentSnippets },
			{ WebsiteRight.ManageSiteMarkers, site => site.ManageSiteMarkers },
			{ WebsiteRight.ManageWebLinkSets, site => site.ManageWebLinkSets },
			{ WebsiteRight.PreviewUnpublishedEntities, site => site.PreviewUnpublishedEntities }
		};

		public bool TryAssert(OrganizationServiceContext serviceContext, WebsiteRight right)
		{
			return this.ContentMapProvider.Using(map =>
			{
				WebsiteNode site;
				Func<WebsiteAccessNode, bool?> selectFlag;

				return map.TryGetValue(_website, out site)
					&& RightMappings.TryGetValue(right, out selectFlag)
					&& TryAssertRightProperty(site, selectFlag);
			});
		}

		protected override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			var entityName = entity.LogicalName;
			this.AddDependencies(
				dependencies,
				entity,
				new[] { "adx_webrole", "adx_webrole_contact", "adx_webrole_account", "adx_websiteaccess" });

			if (entityName == "adx_weblink")
			{
				// Change permission only comes through adx_manageweblinksets on the website.
				if (right == CrmEntityRight.Change)
				{
					return TryAssertRightProperty(context, "adx_manageweblinksets", dependencies);
				}

				WebLinkNode link;

				if (map.TryGetValue(entity, out link))
				{
					dependencies.IsCacheable = false;

					if (link.DisablePageValidation.GetValueOrDefault())
					{
						return true;
					}

					return link.WebPage == null || link.WebPage.IsReference || (_webPageAccessControlSecurityProvider != null && _webPageAccessControlSecurityProvider.TryAssert(link.WebPage, right, false));
				}
			}

			WebsiteNode site;

			if (!map.TryGetValue(_website, out site))
			{
				return false;
			}

			if (entityName == "adx_contentsnippet")
			{
				return right == CrmEntityRight.Read || TryAssertRightProperty(site, rule => rule.ManageContentSnippets);
			}

			if (entityName == "adx_weblinkset")
			{
				return right == CrmEntityRight.Read || TryAssertRightProperty(site, rule => rule.ManageWebLinkSets);
			}

			if (entityName == "adx_sitemarker")
			{
				return right == CrmEntityRight.Read || TryAssertRightProperty(site, rule => rule.ManageSiteMarkers);
			}

			return false;
		}

		private bool TryAssertRightProperty(WebsiteNode site, Func<WebsiteAccessNode, bool?> selectFlag)
		{
			// If Roles are not enabled on the site, deny permission.
			if (!Roles.Enabled)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, "Roles are not enabled for this application.Permission denied.");

                return false;
			}

			var userRoles = this.GetUserRoles();

			if (!userRoles.Any())
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No roles were found for the current user. Permission denied.");

				return false;
			}

			var rules = site.WebsiteAccesses;

			// If no access permissions are defined for this site, deny permission.
			if (rules == null || !rules.Any())
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No website access permission rules were found for the current website. Permission denied.");

				return false;
			}

			foreach (var rule in rules)
			{
				var ruleRoles = rule.WebRoles;

				if (ruleRoles == null)
				{
					continue;
				}

				var ruleRoleNames = ruleRoles.Select(role => role.Name);

				var roleIntersection = ruleRoleNames.Intersect(userRoles, StringComparer.InvariantCulture);

				// If the user is in any of the roles associated with the permission rule, and
				// the rightsPredicate evaluates to true for the given rule, grant permission.
				if (roleIntersection.Any() && selectFlag(rule).GetValueOrDefault())
				{
					return true;
				}
			}

			// If no permission rules meet the necessary conditions, deny permission.
			return false;
		}

		public bool TryAssertRightProperty(OrganizationServiceContext context, string rightPropertyName, CrmEntityCacheDependencyTrace dependencies)
		{
			// If Roles are not enabled on the site, deny permission.
			if (!Roles.Enabled)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Roles are not enabled for this application. Permission denied.");

				return false;
			}

			dependencies.AddEntitySetDependency("adx_webrole");
			dependencies.AddEntitySetDependency("adx_webrole_contact");
			dependencies.AddEntitySetDependency("adx_webrole_account");

			dependencies.AddEntityDependency(_website);

			var userRoles = this.GetUserRoles();

			if (!userRoles.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No roles were found for the current user. Permission denied.");

				return false;
			}

			var websiteaccessFetch = 
				new Fetch
				{
					Entity = new FetchEntity("adx_websiteaccess", new[] { "adx_manageweblinksets", "adx_previewunpublishedentities" })
					{
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition(
										"adx_websiteid",
										ConditionOperator.Equal,
										this._website.Id),
								}
							}
						}
					}
				};

			var rules = context.RetrieveMultiple(websiteaccessFetch).Entities;

			// If no access permissions are defined for this site, deny permission.
			if (!rules.Any())
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No website access permission rules were found for the current website. Permission denied.");

				return false;
			}

			dependencies.AddEntityDependencies(rules);

			foreach (var rule in rules)
			{
				var ruleRoles = context.RetrieveRelatedEntities(rule, "adx_websiteaccess_webrole", new[] { "adx_name" }).Entities;

				if (ruleRoles == null)
				{
					continue;
				}

				var ruleRoleNames = ruleRoles.Select(role => role.GetAttributeValue<string>("adx_name"));

				var roleIntersection = ruleRoleNames.Intersect(userRoles, StringComparer.InvariantCulture);

				// If the user is in any of the roles associated with the permission rule, and
				// the rightsPredicate evaluates to true for the given rule, grant permission.
				if (roleIntersection.Any() && rule.GetAttributeValue<bool?>(rightPropertyName).GetValueOrDefault(false))
				{
					return true;
				}
			}

			// If no permission rules meet the necessary conditions, deny permission.
			return false;
		}
	}
}
