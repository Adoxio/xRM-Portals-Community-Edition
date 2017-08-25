/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Data.Services;

namespace Adxstudio.Xrm.Web.Data.Services
{
	public class MultipleWebsiteCmsDataServiceQueryInterceptor : ICmsDataServiceQueryInterceptor
	{
		private readonly ICrmEntitySecurityProvider _securityProvider;

		public MultipleWebsiteCmsDataServiceQueryInterceptor(string portalName)
		{
			_securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);
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

		private IQueryable<TEntity> FilterBySecurity<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable, CrmEntityRight right) where TEntity : Entity
		{
			return queryable.ToList().Where(e => HasRight(context, e, right)).AsQueryable();
		}

		private IQueryable<TEntity> FilterContentSnippets<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Change);
		}

		private IQueryable<TEntity> FilterEvents<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterEventSchedules<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterForums<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterPageTemplates<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterPublishingStates<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebFiles<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebLinks<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var webLinkSets = FilterBySecurity(context, context.CreateQuery("adx_weblinkset"), CrmEntityRight.Read)
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
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterWebPages<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Read);
		}

		private IQueryable<TEntity> FilterShortcuts<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterBySecurity(context, queryable, CrmEntityRight.Change);
		}

		private bool HasRight<TEntity>(OrganizationServiceContext context, TEntity entity, CrmEntityRight right) where TEntity : Entity
		{
			return _securityProvider.TryAssert(context, entity, right);
		}
	}
}
