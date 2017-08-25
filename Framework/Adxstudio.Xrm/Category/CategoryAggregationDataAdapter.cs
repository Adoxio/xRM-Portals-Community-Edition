/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using ContentAccess;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;
	using Services.Query;
	using Microsoft.Xrm.Sdk.Client;
	using Metadata;
	using Adxstudio.Xrm.Web;
	using Services;

	/// <summary>
	/// Implementation of <see cref="CategoryAggregationDataAdapter"/>. Implements methods to retrive Categories based on Category Drop filters.
	/// </summary>
	public class CategoryAggregationDataAdapter : CategoryAccessProvider, ICategoryAggregationDataAdapter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryAggregationDataAdapter"/> class.
		/// </summary>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public CategoryAggregationDataAdapter(KnowledgeArticles.IDataAdapterDependencies dependencies)
		{
			if (dependencies == null)
			{ throw new ArgumentNullException("dependencies"); }

			this.Dependencies = dependencies;
		}

		/// <summary>
		/// Data Adapter Dependencies
		/// </summary>
		protected KnowledgeArticles.IDataAdapterDependencies Dependencies { get; set; }

		/// <summary>
		/// Get Categories at Top level (ParentCategoryid = null)
		/// </summary>
		/// <param name="pageSize">Results Page Size</param>
		/// <returns>IEnumerable of Category</returns>
		public IEnumerable<ICategory> SelectTopLevelCategories(int pageSize = 5)
		{
			var categoryFetch = GetCategoryFetch(pageSize);

			categoryFetch.Entity.Filters.Add(new Filter
			{
				Conditions = new[]
				{
					new Condition("parentcategoryid", ConditionOperator.Null)
				}
			});

			return this.GetCategories(categoryFetch);
		}

		/// <summary>
		/// Get Recent Categories
		/// </summary>
		/// <param name="pageSize">Results Page Size</param>
		/// <returns>IEnumerable of Category</returns>
		public IEnumerable<ICategory> SelectRecentCategories(int pageSize = 5)
		{
			var categoryFetch = GetCategoryFetch(pageSize);

			var order = new Order("modifiedon", OrderType.Descending);

			this.AddOrderToFetch(categoryFetch, order);

			return this.GetCategories(categoryFetch);
		}

		/// <summary>
		/// Get Popular Categories - We don't have the view count on Categories so we are not filtering further and just returning the categories vs popular categories
		/// </summary>
		/// <param name="pageSize">Results Page Size</param>
		/// <returns>IEnumerable of Category</returns>
		public IEnumerable<ICategory> SelectPopularCategories(int pageSize = 5)
		{
			var categoryFetch = GetCategoryFetch(pageSize);

			return this.GetCategories(categoryFetch);
		}

		/// <summary>
		/// Gets the Child Categories under a Category
		/// </summary>
		/// <param name="categoryId">Current Category Id</param>
		/// <param name="pageSize">Results Page Size</param>
		/// <returns>IEnumerable of Category</returns>
		public IEnumerable<ICategory> SelectRelatedCategories(Guid categoryId, int pageSize = 5)
		{
			var categoryFetch = GetCategoryFetch(pageSize);

			categoryFetch.Entity.Filters.Add(new Filter
			{
				Conditions = new[]
				{
					new Condition("parentcategoryid", ConditionOperator.Equal, categoryId)
				}
			});

			return this.GetCategories(categoryFetch);
		}

		/// <summary>
		/// Get a Category by Category Number
		/// </summary>
		/// <param name="categoryNumber">Category Number</param>
		/// <returns>ICategory with corresponding <paramref name="categoryNumber"/></returns>
		public ICategory SelectByCategoryNumber(string categoryNumber)
		{
			var service = this.Dependencies.GetServiceContext();

			var categoryFetch = this.GetCategoryFetch(1);

			categoryFetch.Entity.Filters.Add(new Filter
			{
				 Conditions = new[]
				 {
					 new Condition("categorynumber", ConditionOperator.Equal, categoryNumber)
				 }
			});

			var result = service.RetrieveSingle(categoryFetch);

			if (result != null)
			{
				// Localize the Category Label if the current user's language is not the org's base language
				int lcid;
				if (this.CategoryLocalizationShouldOccur(categoryFetch, out lcid))
				{
					this.LocalizeCategoryLabel(service, lcid, result);
				}

				return new Category(result);
			}

			return null;
		}

		/// <summary>
		/// Get the Categories by executing the Fetch
		/// </summary>
		/// <param name="fetch">Fetch to execute</param>
		/// <returns>Ienumerable of Category</returns>
		private IEnumerable<ICategory> GetCategories(Fetch fetch)
		{
			var context = this.Dependencies.GetServiceContext();

			var categoryCollection = fetch.Execute(context as IOrganizationService);

			if (categoryCollection == null || !categoryCollection.Entities.Any())
			{
				return Enumerable.Empty<ICategory>();
			}

			// Localize the Category Labels if the current user's language is not the org's base language
			int lcid;
			if (this.CategoryLocalizationShouldOccur(fetch, out lcid))
			{
				this.LocalizeCategoryLabels(context, lcid, categoryCollection);
			}

			return new CategoryFactory(this.Dependencies).Create(categoryCollection.Entities);
		}

		/// <summary>
		/// Checks that user is requesting non-base language and requesting title field
		/// </summary>
		/// <param name="fetch"> Fetch for Category </param>
		/// <param name="lcid"> Lcid variable to instantiate for non-base requests </param>
		/// <returns> True, if localization of Category should occur </returns>
		private bool CategoryLocalizationShouldOccur(Fetch fetch, out int lcid)
		{
			fetch.ThrowOnNull("fetch");

			lcid = 0;

			// If the fetch is not for Category OR does not contain Title attribute, do not localize.
			if (fetch.Entity.Name != "category" || !fetch.Entity.Attributes.Any(x => x.Name == "title"))
			{
				return false;
			}

			var contextLanguageInfo = HttpContext.Current.GetContextLanguageInfo();

			// If CRM version is prior to 8.2, Category Title wasn't exposed to localization
			if (!this.CategoryTitleLocalizableCrmVersion || !contextLanguageInfo.IsCrmMultiLanguageEnabled)
			{
				return false;
			}

			// If the user's ContextLanguage is not different than the base language, do not localize since the localization would return same Title's anyways.
			var context = this.Dependencies.GetRequestContext().HttpContext;
			var organizationBaseLanguageCode = context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;
			if (contextLanguageInfo.ContextLanguage.CrmLcid == organizationBaseLanguageCode)
			{
				return false;
			}

			lcid = contextLanguageInfo.ContextLanguage.CrmLcid;

			return true;
		}

		/// <summary>
		/// Checks that CRM version is enabled for Category Title localization
		/// </summary>
		private bool CategoryTitleLocalizableCrmVersion
		{
			get
			{
				var portalDetails = HttpContext.Current.GetPortalSolutionsDetails();
				return portalDetails != null &&
						portalDetails.CrmVersion.CompareTo(Adxstudio.Xrm.Cms.SolutionVersions.BaseSolutionVersions.CentaurusVersion) 
						>= 0;
			}
		}

		/// <summary>
		/// Replaces Category labels with localized labels where available using specified <paramref name="languageCode"/>
		/// </summary>
		/// <param name="context">Organization Service Context</param>
		/// <param name="languageCode">LCID for label request</param>
		/// <param name="categoryEntityCollection">Category EntityCollection</param>
		private void LocalizeCategoryLabels(OrganizationServiceContext context, int languageCode, EntityCollection categoryEntityCollection)
		{
			// Execute the request in parallel to improve performance
			Parallel.For(0, categoryEntityCollection.Entities.Count, index => this.LocalizeCategoryLabel(context, languageCode, categoryEntityCollection[index]));
		}

		/// <summary>
		/// Replaces Category label with localized label if available using specified <paramref name="languageCode"/>
		/// </summary>
		/// <param name="context">Organization Service Context</param>
		/// <param name="languageCode">LCID for label request</param>
		/// <param name="category">Category entity</param>
		private void LocalizeCategoryLabel(OrganizationServiceContext context, int languageCode, Entity category)
		{
			var localizedLabel = context.RetrieveLocalizedLabel(new EntityReference("category", category.Id), "title", languageCode);

			if (!string.IsNullOrWhiteSpace(localizedLabel))
			{
				category.Attributes["title"] = localizedLabel;
			}
		}

		/// <summary>
		///  Adds to order to the Fetch XML
		/// </summary>
		/// <param name="fetch">Existing FetchXML</param>
		/// <param name="order">Order to add to the Fetch</param>
		private void AddOrderToFetch(Fetch fetch, Order order)
		{
			if (fetch.Entity.Orders == null)
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
