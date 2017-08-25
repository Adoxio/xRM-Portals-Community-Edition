/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Data.Services;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms.Security;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Data.Services;

namespace Adxstudio.Xrm.Web.Data.Services
{
	public class SingleWebsiteCmsDataServiceQueryInterceptor : ICmsDataServiceQueryInterceptor
	{
		private readonly ICrmEntitySecurityProvider _securityProvider;
		private readonly EntityReference _website;

		public SingleWebsiteCmsDataServiceQueryInterceptor(string portalName)
		{
			_securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
			var website = portal.Website;

			if (website == null)
			{
				throw new ArgumentException("The specified portal {0} doesn't have a configured website.".FormatWith(portalName), "portalName");
			}

			website.AssertEntityName("adx_website");

			_website = website.ToEntityReference();
		}

		public IQueryable<TEntity> InterceptQuery<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var entityName = (typeof(TEntity)).GetEntityLogicalName();

			switch (entityName)
			{
			case "adx_contentsnippet":
				return FilterContentSnippets(context, queryable);

			case "adx_event":
				return FilterEvents(context, queryable);

			case "adx_eventschedule":
				return FilterEventSchedules(context, queryable);

			case "adx_communityforum":
				return FilterForums(context, queryable);

			case "adx_pagetemplate":
				return FilterPageTemplates(context, queryable);

			case "adx_publishingstate":
				return FilterPublishingStates(context, queryable);

			case "adx_webfile":
				return FilterWebFiles(context, queryable);

			case "adx_weblink":
				return FilterWebLinks(context, queryable);

			case "adx_weblinkset":
				return FilterWebLinkSets(context, queryable);

			case "adx_webpage":
				return FilterWebPages(context, queryable);

			case "adx_shortcut":
				return FilterShortcuts(context, queryable);
			}

			return queryable;
		}

		private IQueryable<TEntity> FilterByWebsiteAndSecurity<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable, CrmEntityRight right) where TEntity : Entity
		{
			var query = from e in queryable
				where e.GetAttributeValue<EntityReference>("adx_websiteid") == _website
				select e;

			return query.ToList().Where(e => HasRight(context, e, right)).AsQueryable();
		}

		private IQueryable<TEntity> FilterContentSnippets<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Change);
		}

		private IQueryable<TEntity> FilterEvents<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterEventSchedules<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var query = from es in queryable
				join e in context.CreateQuery("adx_event") on es.GetAttributeValue<EntityReference>("adx_eventid").Id equals e.GetAttributeValue<Guid?>("adx_eventid")
				where es.GetAttributeValue<EntityReference>("adx_eventid") != null
				where e.GetAttributeValue<EntityReference>("adx_websiteid") == _website
				select es;

			return query.ToList().Where(e => HasRight(context, e, CrmEntityRight.Read)).AsQueryable();
		}

		private IQueryable<TEntity> FilterForums<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterPageTemplates<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterPublishingStates<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			Guid fromStateID;

			if (Guid.TryParse(HttpContext.Current.Request.QueryString["FromStateID"], out fromStateID))
			{
				return FilterByTransitionalRulesAllowed(context, queryable, CrmEntityRight.Read, fromStateID);
			}

			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterByTransitionalRulesAllowed<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable, CrmEntityRight right, Guid fromStateID) where TEntity : Entity
		{
			var transitionProvider =
					PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IPublishingStateTransitionSecurityProvider>();

			var website = context.CreateQuery("adx_website").First(ws => ws.GetAttributeValue<Guid?>("adx_websiteid") == _website.Id);

			var query = queryable.ToList().Where(e => transitionProvider.TryAssert(context, website, fromStateID,
				e.GetAttributeValue<EntityReference>("adx_publishingstateid") == null ? Guid.Empty : e.GetAttributeValue<EntityReference>("adx_publishingstateid").Id)).AsQueryable();

			return FilterByWebsiteAndSecurity(context, query, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebFiles<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebLinks<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var webLinkSets = FilterByWebsiteAndSecurity(context, context.CreateQuery("adx_weblinkset"), CrmEntityRight.Read)
				.ToLookup(e => e.GetAttributeValue<Guid>("adx_weblinksetid"));

			var webLinks = queryable.ToList().Where(e =>
			{
				var webLinkSetID = e.GetAttributeValue<Guid?>("adx_weblinksetid");

				return webLinkSetID.HasValue && webLinkSets.Contains(webLinkSetID.Value);
			});

			return webLinks.Where(e => HasRight(context, e, CrmEntityRight.Change)).AsQueryable();
		}

		private IQueryable<TEntity> FilterWebLinkSets<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebPages<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterShortcuts<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable, CrmEntityRight.Change);
		}

		private bool HasRight<TEntity>(OrganizationServiceContext context, TEntity entity, CrmEntityRight right) where TEntity : Entity
		{
			return _securityProvider.TryAssert(context, entity, right);
		}
	}
}
