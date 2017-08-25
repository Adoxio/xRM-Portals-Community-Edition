/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Data.Services
{
	public sealed class SingleWebsiteCmsDataServiceQueryInterceptor : ICmsDataServiceQueryInterceptor
	{
		private readonly ICrmEntitySecurityProvider _securityProvider;
		private readonly Guid _websiteID;

		public SingleWebsiteCmsDataServiceQueryInterceptor(string portalName)
		{
			_securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
			var website = portal.Website;

			if (website == null)
			{
				throw new ArgumentException("The specified portal '{0}' does not have a configured website.".FormatWith(portalName), "portalName");
			}

			website.AssertEntityName("adx_website");

			var id = website.GetAttributeValue<Guid?>("adx_websiteid");
			_websiteID = id.HasValue ? id.Value : website.Id;
		}

		public IQueryable<TEntity> InterceptQuery<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var entityName = (typeof(TEntity)).GetEntityLogicalName();

			switch (entityName)
			{
				case "adx_contentsnippet":
					return FilterContentSnippets(context, queryable);

				case "adx_pagetemplate":
					return FilterPageTemplates(context, queryable);

				case "adx_webfile":
					return FilterWebFiles(context, queryable);

				case "adx_weblink":
					return FilterWebLinks(context, queryable);

				case "adx_weblinkset":
					return FilterWebLinkSets(context, queryable);

				case "adx_webpage":
					return FilterWebPages(context, queryable);
			}

			return queryable;
		}

		private IQueryable<TEntity> FilterByWebsiteAndSecurity<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var query =
				from e in queryable
				where e.GetAttributeValue<Guid?>("adx_websiteid") == _websiteID
				select e;

			return query.ToList().Where(e => HasReadAccess(context, e)).AsQueryable();
		}

		private IQueryable<TEntity> FilterContentSnippets<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable);
		}

		private IQueryable<TEntity> FilterPageTemplates<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable);
		}

		private IQueryable<TEntity> FilterWebFiles<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable);
		}

		private IQueryable<TEntity> FilterWebLinks<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			var webLinkSets = FilterByWebsiteAndSecurity(context, context.CreateQuery("adx_weblinkset"))
				.ToLookup(e => e.GetAttributeValue<Guid>("adx_weblinksetid"));

			var webLinks = queryable.ToList().Where(e =>
			{
				var webLinkSetID = e.GetAttributeValue<Guid?>("adx_weblinksetid");

				return webLinkSetID.HasValue && webLinkSets.Contains(webLinkSetID.Value);
			});

			return webLinks.Where(wl => HasReadAccess(context, wl)).AsQueryable();
		}

		private IQueryable<TEntity> FilterWebLinkSets<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable);
		}

		private IQueryable<TEntity> FilterWebPages<TEntity>(OrganizationServiceContext context, IQueryable<TEntity> queryable) where TEntity : Entity
		{
			return FilterByWebsiteAndSecurity(context, queryable);
		}

		private bool HasReadAccess<TEntity>(OrganizationServiceContext context, TEntity entity) where TEntity : Entity
		{
			return _securityProvider.TryAssert(context, entity, CrmEntityRight.Read);
		}
	}
}
