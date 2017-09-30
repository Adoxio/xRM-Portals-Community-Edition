/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Cms.SolutionVersions;

	internal static class WebsiteConstants
	{
		public static readonly Relationship WebsiteSiteSettingRelationship = new Relationship("adx_website_sitesetting");
		public static readonly Relationship WebsiteBindingRelationship = new Relationship("adx_website_websitebinding");

		public static readonly EntityNodeColumn[] WebsiteAttributes = {
			new EntityNodeColumn("adx_websiteid", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_name", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("statecode", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_parentwebsiteid", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_headerwebtemplateid", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_footerwebtemplateid", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_website_language", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_defaultlanguage", BaseSolutionVersions.CentaurusVersion),
			new EntityNodeColumn("adx_primarydomainname", BaseSolutionVersions.PotassiumVersion)
		};

		public static readonly EntityNodeColumn[] WebsiteSettingAttributes = {
			new EntityNodeColumn("adx_name", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_value", BaseSolutionVersions.NaosAndOlderVersions)
		};

		public static readonly EntityNodeColumn[] WebsiteBindingAttributes = {
			new EntityNodeColumn("adx_websiteid", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_name", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_sitename", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_virtualpath", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_releasedate", BaseSolutionVersions.NaosAndOlderVersions),
			new EntityNodeColumn("adx_expirationdate", BaseSolutionVersions.NaosAndOlderVersions)
		};
	}

	public interface IWebsiteStore<TWebsite, TKey> : IEntityStore<TWebsite, TKey>
	{
	}

	public interface IQueryableWebsiteStore<TWebsite, TKey> : IWebsiteStore<TWebsite, TKey>
	{
		IQueryable<TWebsite> Websites { get; }
	}

	public class CrmWebsiteStore<TWebsite, TKey>
		: CrmEntityStore<TWebsite, TKey>,
		  IQueryableWebsiteStore<TWebsite, TKey>
		where TWebsite : CrmWebsite<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		public CrmWebsiteStore(CrmDbContext context)
			: base("adx_website", "adx_websiteid", "adx_name", context, new CrmEntityStoreSettings())
		{
		}

		public CrmWebsiteStore(CrmDbContext context, CrmEntityStoreSettings settings)
			: base("adx_website", "adx_websiteid", "adx_name", context, settings)
		{
		}

		#region IWebsiteStore

		protected override RetrieveRequest ToRetrieveRequest(EntityReference id)
		{
			// build the related entity queries
			var siteSettingFetch = new Fetch
			{
				Entity = new FetchEntity("adx_sitesetting", WebsiteConstants.WebsiteSettingAttributes.ToFilteredColumns(this.BaseSolutionCrmVersion))
				{
					Filters = new[] { new Filter {
						Conditions = GetActiveStateConditions().ToArray()
					} }
				}
			};

			var bindingFetch = new Fetch
			{
				Entity = new FetchEntity("adx_websitebinding", WebsiteConstants.WebsiteBindingAttributes.ToFilteredColumns(this.BaseSolutionCrmVersion))
				{
					Filters = new[] { new Filter {
						Conditions = GetActiveStateConditions().ToArray()
					} }
				}
			};

			var relatedEntitiesQuery = new RelationshipQueryCollection
			{
				{ WebsiteConstants.WebsiteSiteSettingRelationship, siteSettingFetch.ToFetchExpression() },
				{ WebsiteConstants.WebsiteBindingRelationship, bindingFetch.ToFetchExpression() },
			};

			// retrieve the local identity by ID including its related entities

			var request = new RetrieveRequest
			{
				Target = id,
				ColumnSet = new ColumnSet(this.GetWebsiteAttributes().ToArray()),
				RelatedEntitiesQuery = relatedEntitiesQuery
			};

			return request;
		}

		protected virtual IEnumerable<string> GetWebsiteAttributes()
		{
			return WebsiteConstants.WebsiteAttributes.ToFilteredColumns(this.BaseSolutionCrmVersion);
		}

		#endregion

		#region IQueryableWebsiteStore

		public IQueryable<TWebsite> Websites
		{
			get
			{
				var fetchWebsites = new Fetch
				{
					Entity = new FetchEntity(this.LogicalName, this.GetWebsiteAttributes())
					{
						Filters = new[] { new Filter {
							Conditions = GetActiveEntityConditions().ToArray()
						} }
					}
				};

				var websites = Context.Service.RetrieveMultiple(fetchWebsites);
				MergeRelatedEntities(websites.Entities, WebsiteConstants.WebsiteBindingRelationship, "adx_websitebinding", new ColumnSet(WebsiteConstants.WebsiteBindingAttributes.ToFilteredColumns(this.BaseSolutionCrmVersion)));

				return websites.Entities.Select(ToModel).ToList().AsQueryable();
			}
		}

		#endregion
	}
}
