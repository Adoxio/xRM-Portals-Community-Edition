/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.ContentAccess;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;

	public class KnowledgeArticleAggregationDataAdapter : IKnowledgeArticleAggregationDataAdapter
	{
		public KnowledgeArticleAggregationDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; set; }

		public IEnumerable<IKnowledgeArticle> SelectTopArticles(int pageSize = 5, string languageLocaleCode = null)
		{
			var articleFetch = GetBaseArticleFetch(pageSize, languageLocaleCode);

			var order = new Order("rating", OrderType.Descending);

			AddOrderToFetch(articleFetch, order);

			return GetArticles(articleFetch);
		}

		public IEnumerable<IKnowledgeArticle> SelectRecentArticles(int pageSize = 5, string languageLocaleCode = null)
		{
			var articleFetch = GetBaseArticleFetch(pageSize, languageLocaleCode);

			var order = new Order("modifiedon", OrderType.Descending);

			AddOrderToFetch(articleFetch, order);

			return GetArticles(articleFetch);
		}

		public IEnumerable<IKnowledgeArticle> SelectPopularArticles(int pageSize = 5, string languageLocaleCode = null)
		{
			var articleFetch = GetBaseArticleFetch(pageSize, languageLocaleCode);

			var order = new Order("knowledgearticleviews", OrderType.Descending);

			AddOrderToFetch(articleFetch, order);

			return GetArticles(articleFetch);
		}

		private Fetch GetBaseArticleFetch(int pageSize = 5, string languageLocaleCode = null)
		{
			const int published = 3;

			// if language locale code is not provided, fallback to the site setting
			if (string.IsNullOrWhiteSpace(languageLocaleCode))
			{
				var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
				languageLocaleCode = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, "KnowledgeManagement/Article/Language");
			}
			var optionalLanguageCondition = string.IsNullOrWhiteSpace(languageLocaleCode) ? string.Empty : string.Format("<condition entityname='language_locale' attribute='code' operator='eq' value = '{0}' />", languageLocaleCode);

			var articlesFetchXmlFormat = @"
			<fetch mapping='logical' count='{0}' returntotalrecordcount='true'>
				<entity name='knowledgearticle'>
					<all-attributes /> 
					<link-entity name='languagelocale' from='languagelocaleid' to='languagelocaleid' visible='false' link-type='outer'  alias='language_locale'>
						<attribute name='localeid' />
						<attribute name='code' />
						<attribute name='region' />
						<attribute name='name' />
						<attribute name='language' />
					</link-entity>
					<filter type='and'>
						<condition attribute='isrootarticle' operator='eq' value='0' />
						<condition attribute='statecode' operator='eq' value='{1}' />
						<condition attribute='isinternal' operator='eq' value='0' />
						{2}
					</filter>
				</entity>
			</fetch>";

			var articlesFetchXml = string.Format(articlesFetchXmlFormat, pageSize, published, optionalLanguageCondition);

			var articleFetch = Fetch.Parse(articlesFetchXml);

            // Apply Content Access Level filtering
            var contentAccessProvider = new ContentAccessLevelProvider();
            contentAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, articleFetch);

            // Apply Product filtering
            var productAccessProvider = new ProductAccessProvider();
            productAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, articleFetch);

            return articleFetch;
		}

		private IEnumerable<IKnowledgeArticle> GetArticles(Fetch fetch)
		{
			var context = Dependencies.GetServiceContext();

			var articlesCollection = fetch.Execute(context as IOrganizationService, RequestFlag.AllowStaleData);

			if (articlesCollection == null || !articlesCollection.Entities.Any())
			{
				return Enumerable.Empty<IKnowledgeArticle>();
			}

			return new KnowledgeArticleFactory(Dependencies).Create(articlesCollection.Entities);
		}

		private void AddOrderToFetch(Fetch fetch, Order order)
		{
			if (fetch.Entity.Orders == null || !fetch.Entity.Orders.Any())
			{
				fetch.Entity.Orders = new List<Order> { order };
			}
			else
			{
				fetch.Entity.Orders.Add(order);
			}
		}
	}
}
