/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Web;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Web.Security;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Adxstudio.Xrm.Cms.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web;

	using Microsoft.Xrm.Sdk.Query;

	internal class PublishingStateAccessProvider : ContentMapAccessProvider
	{
		public PublishingStateAccessProvider(HttpContext context)
			: base(context)
		{
		}

		public PublishingStateAccessProvider(HttpContextBase context)
			: base(context.GetContentMapProvider())
		{
		}

		public PublishingStateAccessProvider(IContentMapProvider contentMapProvider)
			: base(contentMapProvider)
		{
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

		protected override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			switch (entity.LogicalName)
			{
				case "adx_weblink":
					WebLinkNode link;
					if (map.TryGetValue(entity, out link))
					{
						return TryAssert(link) || UserCanPreview(context, entity);
					}
					break;
				case "adx_webpage":
					WebPageNode page;
					if (map.TryGetValue(entity, out page))
					{
						return TryAssert(page.PublishingState) || UserCanPreview(context, entity);
					}
					break;
				case "adx_weblinkset":
					WebLinkSetNode linkset;
					if (map.TryGetValue(entity, out linkset))
					{
						return TryAssert(linkset.PublishingState) || UserCanPreview(context, entity);
					}
					break;
				case "adx_webfile":
					WebFileNode file;
					if (map.TryGetValue(entity, out file))
					{
						return TryAssert(file.PublishingState) || UserCanPreview(context, entity);
					}
					break;
				case "adx_shortcut":
					ShortcutNode shortcut;
					if (map.TryGetValue(entity, out shortcut))
					{
						return TryAssert(shortcut) || UserCanPreview(context, entity);
					}
					break;
				case "adx_communityforum":
					ForumNode forum;
					if (map.TryGetValue(entity, out forum))
					{
						return TryAssert(forum.PublishingState) || UserCanPreview(context, entity);
					}
					break;
			}

			return this.TryAssert(context, entity, right, dependencies);
		}

		private static bool TryAssert(ShortcutNode shortcut)
		{
			if (!shortcut.DisableTargetValidation.GetValueOrDefault())
			{
				if (shortcut.WebPage != null && !shortcut.WebPage.IsReference)
				{
					return TryAssert(shortcut.WebPage.PublishingState);
				}

				if (shortcut.WebFile != null && !shortcut.WebFile.IsReference)
				{
					return TryAssert(shortcut.WebFile.PublishingState);
				}
			}

			if (shortcut.Parent != null && !shortcut.Parent.IsReference)
			{
				return TryAssert(shortcut.Parent.PublishingState);
			}

			return false;
		}

		private static bool TryAssert(WebLinkNode link)
		{
			if (link.PublishingState != null && link.PublishingState.IsReference)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Exception, 
					string.Format(@"InvalidOperationException: IsReference cannot be set to true. WebLinkNode: Id:{0}, 
						WebLinkNode.PublishingState.Website.Id: {1}, WebLinkNode.PublishingState.Website.Name: {2}, 
						WebLinkNode.PublishingState.Website.WebsiteLanguage: {3}, WebLinkNode.WebPage.Website.Id: {4},
						WebLinkNode.WebPage.Website.Name: {5}, WebLinkNode.WebPage.Website.WebsiteLanguage: {6}",
					link.Id, link.PublishingState.Website.Id, link.PublishingState.Website.Name, link.PublishingState.Website.WebsiteLanguage,
					link.WebPage.Website.Id, link.WebPage.Website.Name, link.WebPage.Website.WebsiteLanguage));

				throw new InvalidOperationException();
			}

			if (!TryAssert(link.PublishingState))
			{
				// the link is in a non-visible state, check for preview access
				return false;
			}

			// the link is in a visible state, check the related web page
			if (!link.DisablePageValidation.GetValueOrDefault() && link.WebPage != null && !link.WebPage.IsReference)
			{
				// validate the link's webpage
				return TryAssert(link.WebPage.PublishingState);
			}

			// the link is visible and the page is valid (or the link is an external URL)
			return true;
		}

		private static bool TryAssert(PublishingStateNode state)
		{
			if (state != null && state.IsReference)
			{
				throw new InvalidOperationException();
			}

			var isLanguagePublished = true;

			// For Multi-Language portal, if the selected language is in draft state, we need to return false.
			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();

			if (contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				isLanguagePublished = contextLanguageInfo.ContextLanguage.IsPublished;
			}

			if (state == null || (state.IsVisible.GetValueOrDefault() && isLanguagePublished))
			{
				return true;
			}

			return false;
		}

		public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
		{
			if (entity == null || right == CrmEntityRight.Change)
			{
				return false;
			}

			dependencies.AddEntityDependency(entity);
			dependencies.AddEntitySetDependency("adx_webrole");
			dependencies.AddEntitySetDependency("adx_webrole_contact");
			dependencies.AddEntitySetDependency("adx_webrole_account");
			dependencies.AddEntitySetDependency("adx_websiteaccess");

			var entityName = entity.LogicalName;

			if (entityName == "adx_idea")
			{
				return entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
			}

			if (entityName == "adx_ideacomment")
			{
				return entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
			}

			EntityReference publishingStateReference = null;
			Entity entityPublishingState = null;

			switch (entityName)
			{
				case "adx_communityforumpost":
					publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
					break;
				case "adx_ad":
					publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
					break;

				// legacy entities
				case "adx_event":
					entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_event");
					break;
				case "adx_eventschedule":
					entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventschedule");
					break;
				case "adx_eventspeaker":
					entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventspeaker");
					break;
				case "adx_eventsponsor":
					entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventsponsor");
					break;
				case "adx_survey":
					entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_survey");
					break;
			}

			if (publishingStateReference != null)
			{
				entityPublishingState = context.RetrieveSingle(
					"adx_publishingstate",
					new[] { "adx_isvisible" },
					new Condition("adx_publishingstateid", ConditionOperator.Equal, publishingStateReference.Id));
			}

			if (entityPublishingState == null)
			{
				return true;
			}

			dependencies.AddEntityDependency(entityPublishingState);

			if (entityPublishingState.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault())
			{
				return true;
			}

			return UserCanPreview(context, entityPublishingState);
		}
		
		/// <summary>
		/// Test whether or not an Entity's publishing state is visible in the current context.
		/// </summary>
		public virtual bool TryAssert(OrganizationServiceContext context, Entity entity)
		{
			var securityContextKey = GetType().FullName;

			ICacheSupportingCrmEntitySecurityProvider underlyingProvider = new ApplicationCachingCrmEntitySecurityProvider(new UncachedProvider(), new VaryByPreviewCrmEntitySecurityCacheInfoFactory(securityContextKey));

			if (HttpContext.Current != null)
			{
				underlyingProvider = new RequestCachingCrmEntitySecurityProvider(underlyingProvider, new CrmEntitySecurityCacheInfoFactory(securityContextKey));
			}

			return underlyingProvider.TryAssert(context, entity, CrmEntityRight.Read);
		}

		internal class UncachedProvider : CacheSupportingCrmEntitySecurityProvider
		{
			public override bool TryAssert(OrganizationServiceContext context, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies)
			{
				if (entity == null || right == CrmEntityRight.Change)
				{
					return false;
				}

				dependencies.AddEntityDependency(entity);
				dependencies.AddEntitySetDependency("adx_webrole");
				dependencies.AddEntitySetDependency("adx_webrole_contact");
				dependencies.AddEntitySetDependency("adx_webrole_account");
				dependencies.AddEntitySetDependency("adx_websiteaccess");

				var entityName = entity.LogicalName;

				// Weblinks require some special handling.
				if (entityName == "adx_weblink")
				{
					var weblinkPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_weblink");

					// If a weblink has a publishing state, and that state is not visible, state access is
					// denied (unless the user can preview).
					if (weblinkPublishingState != null && !weblinkPublishingState.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault())
					{
						dependencies.AddEntityDependency(weblinkPublishingState);

						return UserCanPreview(context, entity);
					}

					var weblinkPage = context.RetrieveRelatedEntity(entity, "adx_webpage_weblink");
						
					// If a weblink has an associated page, and page validation is not disabled, return the
					// result of assertion on that page.
					if (weblinkPage != null && !entity.GetAttributeValue<bool?>("adx_disablepagevalidation").GetValueOrDefault(false))
					{
						return TryAssert(context, weblinkPage, right, dependencies);
					}
				}

				if (entityName == "adx_idea")
				{
					return entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
				}

				if (entityName == "adx_ideacomment")
				{
					return entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);
				}

				EntityReference publishingStateReference = null;
				Entity entityPublishingState = null;

				switch (entityName)
				{
					case "adx_webpage":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;
					case "adx_weblinkset":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;
					case "adx_webfile":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;
					case "adx_communityforum":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;
					case "adx_communityforumpost":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;
					case "adx_ad":
						publishingStateReference = entity.GetAttributeValue<EntityReference>("adx_publishingstateid");
						break;

					// legacy entities
					case "adx_event":
						entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_event");
						break;
					case "adx_eventschedule":
						entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventschedule");
						break;
					case "adx_eventspeaker":
						entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventspeaker");
						break;
					case "adx_eventsponsor":
						entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_eventsponsor");
						break;
					case "adx_survey":
						entityPublishingState = context.RetrieveRelatedEntity(entity, "adx_publishingstate_survey");
						break;
				}

				if (publishingStateReference != null)
				{
					entityPublishingState = context.RetrieveSingle(
						"adx_publishingstate",
						new[] { "adx_isvisible" },
						new Condition("adx_publishingstateid", ConditionOperator.Equal, publishingStateReference.Id));
				}

				if (entityPublishingState == null)
				{
					return true;
				}

				dependencies.AddEntityDependency(entityPublishingState);

				if (entityPublishingState.GetAttributeValue<bool?>("adx_isvisible").GetValueOrDefault())
				{
					return true;
				}

				return UserCanPreview(context, entityPublishingState);
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
}
