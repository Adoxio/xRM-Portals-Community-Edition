/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.ContentAccess
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Security;
	using Services.Query;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;
	using Web.UI;
	using KnowledgeArticles;

	/// <summary>
	/// Category Access Provider - adds CAL and Product filtering to Category fetch
	/// </summary>
	public class CategoryAccessProvider : ContentAccessProvider
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAccessProvider"/> class.
        /// </summary>
        /// <param name="configuration">Category Access Procider Configuration</param>
        public CategoryAccessProvider(ContentAccessConfiguration configuration) : base(configuration)
        {
            this.ContentAccessLevelProvider = new ContentAccessLevelProvider();
            this.ProductAccessProvider = new ProductAccessProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAccessProvider"/> class. 
        /// </summary>
        public CategoryAccessProvider() : this(ContentAccessConfiguration.DefaultCategoryConfiguration())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAccessProvider"/> class.
        /// </summary>
        /// <param name="portalContext">Portal Context</param>
        /// <param name="contentAccessLevelProvider">Content Access Level Provider</param>
        /// <param name="productAccessProvider">Product Access Provider</param>
        public CategoryAccessProvider(IPortalContext portalContext, ContentAccessLevelProvider contentAccessLevelProvider, ProductAccessProvider productAccessProvider)
            : base(ContentAccessConfiguration.DefaultCategoryConfiguration(), portalContext)
        {
            this.ContentAccessLevelProvider = contentAccessLevelProvider;
            this.ProductAccessProvider = productAccessProvider;
        }

        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Applies both CAL and Product filtering to the existing FetchXML query
        /// </summary>
        /// <param name="right">Current Permission Right</param>
        /// <param name="fetchIn">FetchXML to modify</param>
        public override void TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight right, Fetch fetchIn)
        {
            // We are not calling this method anywhere as it's causing performance issues. 
            // returning with no operation in case if some other methods calling it in future.
            throw new NotImplementedException();

            // Apply filter only if Entity is "Knowledge Article" and Right is "Read"
            if (!this.IsRightEntityAndPermissionRight(right, fetchIn, this.Config.SourceEntityName, CrmEntityPermissionRight.Read))
            {
                return;
            }
            this.ApplyCategoryFilter(fetchIn);
        }


        /// <summary>
        /// Gets accessible Category IDs by applying both CAL and Product filtering
        /// </summary>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>List Category IDs as a List of Guids</returns>
        public List<Guid> GetAccessibleCategoryIds(int pageSize = 5)
        {
            var categoryIds = new List<Guid>();


            var articlesFetch = new Fetch
            {
                Distinct = true,
                Entity = new FetchEntity
                {
                    Name = "knowledgearticle",
                    Links = new List<Link>(),
					Filters = new List<Filter>
					{
						new Filter()
						{
							Conditions = new List<Condition>
							{
								new Condition("statecode", ConditionOperator.Equal, KnowledgeArticleState.Published),
								new Condition("isrootarticle", ConditionOperator.Equal, 0),
								new Condition("isinternal", ConditionOperator.Equal, 0)
							}
						}
					}
                },
                PageSize = pageSize
            };

            // If CAL is enabled, then get the accessible categoryIds by querying Articles joining KnowledgeArticleCategories
            if (this.ContentAccessLevelProvider.IsEnabled())
            {
                // Apply Content Access Level filtering
                this.ContentAccessLevelProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, articlesFetch);
            }

            // If Product Filtering is enabled, then get the accessible categoryIds by querying Articles joining KnowledgeArticleCategories
            if (this.ProductAccessProvider.IsEnabled())
            {
                // Apply Product filtering
                this.ProductAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, articlesFetch);
            }
            
            // Apply Category-KnowledgeArticle Intersect to determine whether it has any accessible articles on or under it.
            this.ApplyKnowledgeArticleCategoryIntersectToArticleFetch(articlesFetch);

            var categoryIdsCollection = articlesFetch.Execute(this.Portal.ServiceContext as IOrganizationService);

            if (categoryIdsCollection != null && categoryIdsCollection.Entities != null &&
                categoryIdsCollection.Entities.Any())
            {
                categoryIds = categoryIdsCollection.Entities.Select(e => (Guid)e.GetAttributeValue<AliasedValue>("id").Value).ToList();
            }
            if (!categoryIds.Any())
            {
                categoryIds.Add(Guid.Empty);
            }

            return categoryIds;
        }

        /// <summary>
        /// Gets the base Category Fetch under a specified parent category
        /// </summary>
        /// <param name="currentcategoryId">current Category ID</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>Constructed Fetch XML as a Fetch</returns>
        public Fetch GetCategoryFetchUnderParent(Guid? currentcategoryId, int? pageSize = 5)
        {
            var fetch = this.GetCategoryFetch(pageSize);

            var baseCategoryFilter = currentcategoryId == null
                ? new Condition("parentcategoryid", ConditionOperator.Null)
                : new Condition("parentcategoryid", ConditionOperator.Equal, currentcategoryId);

            fetch.AddFilter(new Filter
            {
                Type = LogicalOperator.And,
                Conditions = new List<Condition>
                {
                    baseCategoryFilter
                }
            });

            return fetch;
        }

        /// <summary>
        /// Gets the base Category Fetch
        /// </summary>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>Constructed Fetch XML as a Fetch</returns>
        public Fetch GetCategoryFetch(int? pageSize = 5)
        {
            var categoryFetch = new Fetch
            {
                PageSize = pageSize,
                Entity = new FetchEntity
                {
                    Name = "category",
                    Attributes = new List<FetchAttribute>
                    {
                        new FetchAttribute("categorynumber"),
                        new FetchAttribute("title"),
                        new FetchAttribute("createdon"),
                        new FetchAttribute("modifiedon"),
                        new FetchAttribute("categoryid"),
                        new FetchAttribute("parentcategoryid")
                    },
                    Filters = new List<Filter>()
                }
            };

            return categoryFetch;
        }

        /// <summary>
        /// Gets Categories under passed Parent Category Id
        /// </summary>
        /// <param name="currentcategoryId">Category ID to apply filter</param>
        /// <returns>Category collection</returns>
        public IEnumerable<Entity> GetCategoriesUnderId(Guid? currentcategoryId)
        {
            var fetch = this.GetCategoryFetchUnderParent(currentcategoryId);

            var categoryCollection = fetch.Execute(this.Portal.ServiceContext as IOrganizationService);

            return categoryCollection.Entities;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Applies Category Filter to the Fetch XML
        /// </summary>
        /// <param name="fetchIn">Existing Fetch XML</param>
        private void ApplyCategoryFilter(Fetch fetchIn)
        {
            var filter = this.GetCategoryFilterWithAccessibleCategoryIds();

            if (fetchIn.Entity.Filters == null || !fetchIn.Entity.Filters.Any())
            {
                fetchIn.Entity.Filters = new List<Filter>() { filter };
            }
            else
            {
                var existingFilters = fetchIn.Entity.Filters;
                existingFilters.FirstOrDefault().Filters = new List<Filter>() { filter };
            }
        }

        /// <summary>
        /// Applies KnowledgeArticle-Category intersect fetch to get the accessible category IDs
        /// </summary>
        /// <param name="fetchIn">Fetchxml to add addition filtering</param>
        private void ApplyKnowledgeArticleCategoryIntersectToArticleFetch(Fetch fetchIn)
        {
            var link = new Link
            {
                Name = "knowledgearticlescategories",
                FromAttribute = "knowledgearticleid",
                ToAttribute = "knowledgearticleid",
                Intersect = true,
                Visible = false,
                Type = Microsoft.Xrm.Sdk.Query.JoinOperator.Inner,
                Attributes = new List<FetchAttribute>
                {
                    new FetchAttribute
                    {
                        Name = "categoryid",
                        GroupBy = true,
                        Alias = "id"
                    }
                },
            };

            if (link != null)
            {
                fetchIn.Aggregate = true;
                fetchIn.Entity.Links.Add(link);
            }
        }

        /// <summary>
        /// Constructs and Gets the filter with accessible Category IDs
        /// </summary>
        /// <returns>Category Filter</returns>
        private Filter GetCategoryFilterWithAccessibleCategoryIds()
        {
            // Retrieve accessible Category IDs to filter further
            var categoryIds = this.GetAccessibleCategoryIds();

            var filter = new Filter
            {
                Type = LogicalOperator.Or,
                Conditions =
                    this.ConstructFilterWithConditions("categoryid", ConditionOperator.AboveOrEqual, categoryIds)
            };

            return filter;

        }

        /// <summary>
        /// Construct Filter with Conditions using the attribute, operator and List of Values
        /// /// </summary>
        /// <param name="attributeName">Name of the Attribute to add the Condition</param>
        /// <param name="conditionalOperater">Conditional Operator to check</param>
        /// <param name="values">Values to add to the Condition</param>
        /// <returns>List of constructed Condidtions</returns>
        private List<Condition> ConstructFilterWithConditions(string attributeName, ConditionOperator conditionalOperater, List<Guid> values)
        {
            var conditions = new List<Condition>();

            foreach (var value in values)
            {
                conditions.Add(new Condition(attributeName, conditionalOperater, value));
            }
            return conditions;
        }

        /// <summary>
        /// Content Access Level Provider
        /// </summary>
        private ContentAccessLevelProvider ContentAccessLevelProvider { get; set; }

        /// <summary>
        /// Product Access Provider
        /// </summary>
        private ProductAccessProvider ProductAccessProvider { get; set; }

        #endregion

    }
}
