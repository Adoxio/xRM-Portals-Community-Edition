/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Adxstudio.Xrm.Services;
	using ContentAccess;
	using Security;
	using Services.Query;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;
	
	/// <summary>
	/// Category Data Adapter
	/// </summary>
	public class CategoryDataAdapter : CategoryAccessProvider, ICategoryDataAdapter
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryDataAdapter"/> class.
		/// </summary>
		/// <param name="category">The category to get and set data for</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public CategoryDataAdapter(EntityReference category, string portalName = null)
			: this(category, new KnowledgeArticles.PortalConfigurationDataAdapterDependencies(portalName))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryDataAdapter"/> class.
		/// </summary>
		/// <param name="category">The category to get and set data for</param>
		/// <param name="portalName">The configured name of the portal to get and set data for</param>
		public CategoryDataAdapter(Entity category, string portalName = null)
			: this(category.ToEntityReference(), portalName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryDataAdapter"/> class.
		/// </summary>
		/// <param name="category">Category Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public CategoryDataAdapter(EntityReference category, KnowledgeArticles.IDataAdapterDependencies dependencies)
		{
			category.ThrowOnNull("article");
			category.AssertLogicalName("knowledgearticle");
			dependencies.ThrowOnNull("dependencies");

			this.Category = category;
			this.Dependencies = dependencies;
		}

		/// <summary>
		/// Gets the Current Category
		/// </summary>
		/// <returns>Category Interface reference</returns>
		public virtual ICategory Select()
		{
			var category = this.GetCategoryEntity(this.Dependencies.GetServiceContext());

			return category == null
				? null
				: new CategoryFactory(this.Dependencies).Create(new[] { category }).FirstOrDefault();
		}

		/// <summary>
		/// Gets the Category Entity using the Category Id from Service Context
		/// </summary>
		/// <param name="serviceContext">Organization Service Context</param>
		/// <returns>Category as an Entity</returns>
		private Entity GetCategoryEntity(OrganizationServiceContext serviceContext)
		{
			var category = serviceContext.RetrieveSingle("category", "categoryid", this.Category.Id, FetchAttribute.All);

			return category;
		}

		/// <summary>
		/// Data Adapter Dependencies 
		/// </summary>
		protected KnowledgeArticles.IDataAdapterDependencies Dependencies { get; set; }

		/// <summary>
		/// Category Entity Reference
		/// </summary>
		protected EntityReference Category { get; set; }

		/// <summary>
		/// Gets Related Articles of a Category
		/// </summary>
		/// <returns>IEnumerable of Related Article</returns>
		public IEnumerable<RelatedArticle> SelectRelatedArticles()
		{
			var category = this.Select();

			var relatedArticlesFetch = new Fetch
			{
				Distinct = true,
				Entity = new FetchEntity
				{
					Name = "knowledgearticle",
					Attributes = new List<FetchAttribute>()
					{
						new FetchAttribute("articlepublicnumber"),
						new FetchAttribute("knowledgearticleid"),
						new FetchAttribute("title"),
						new FetchAttribute("keywords"),
						new FetchAttribute("createdon"),
						new FetchAttribute("statecode"),
						new FetchAttribute("statuscode"),
						new FetchAttribute("isrootarticle"),
						new FetchAttribute("islatestversion"),
						new FetchAttribute("isprimary"),
						new FetchAttribute("knowledgearticleviews")
					},
					Filters = new List<Filter>()
					{
						new Filter
						{
							Type = LogicalOperator.And,
							Conditions = new List<Condition>()
							{
								new Condition("isrootarticle", ConditionOperator.Equal, 0),
								new Condition("statecode", ConditionOperator.Equal, 3),
								new Condition("isinternal", ConditionOperator.Equal, 0)
							}

						},

					},
					Links = new List<Link>()
					{
						new Link
						{
							Name = "knowledgearticlescategories",
							FromAttribute = "knowledgearticleid",
							ToAttribute = "knowledgearticleid",
							Intersect = true,
							Visible = false,
							Filters = new List<Filter>()
							{
								new Filter
								{
									Type = LogicalOperator.And,
									Conditions = new List<Condition>()
									{
										new Condition("categoryid", ConditionOperator.Equal, category.Id)
									}

								}
							}
						}
					}
				}
			};

			var relatedArticles = Enumerable.Empty<RelatedArticle>();

			var serviceContext = this.Dependencies.GetServiceContext();
			var securityProvider = this.Dependencies.GetSecurityProvider();
			var urlProvider = this.Dependencies.GetUrlProvider();

			// Apply Content Access Level filtering
			var contentAccessProvider = new ContentAccessLevelProvider();
			contentAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, relatedArticlesFetch);

			// Apply Product filtering
			var productAccessProvider = new ProductAccessProvider();
			productAccessProvider.TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight.Read, relatedArticlesFetch);

			var relatedArticlesEntityCollection = relatedArticlesFetch.Execute(serviceContext as IOrganizationService);

			if (relatedArticlesEntityCollection != null && relatedArticlesEntityCollection.Entities != null && relatedArticlesEntityCollection.Entities.Any())
			{
				relatedArticles =
					relatedArticlesEntityCollection.Entities.Where(e => securityProvider.TryAssert(serviceContext, e, CrmEntityRight.Read))
						.Select(e => new { Title = e.GetAttributeValue<string>("title"), Url = urlProvider.GetUrl(serviceContext, e) })
						.Where(e => !(string.IsNullOrEmpty(e.Title) || string.IsNullOrEmpty(e.Url)))
						.Select(e => new RelatedArticle(e.Title, e.Url))
						.OrderBy(e => e.Title);
			}

			return relatedArticles;
		}

		/// <summary>
		/// Gets Child Categories of a Category
		/// </summary>
		/// <returns>IEnumerable of Child Category</returns>
		public virtual IEnumerable<ChildCategory> SelectChildCategories()
		{
			var childCategories = Enumerable.Empty<ChildCategory>();

			var serviceContext = this.Dependencies.GetServiceContext();
			var urlProvider = this.Dependencies.GetUrlProvider();

			var category = this.Select();

			var categoryFetch = GetCategoryFetchUnderParent(category.Id, null);

			var childCategoriesEntityCollection = categoryFetch.Execute(serviceContext as IOrganizationService);

			if (childCategoriesEntityCollection != null && childCategoriesEntityCollection.Entities != null && childCategoriesEntityCollection.Entities.Any())
			{
				childCategories =
					childCategoriesEntityCollection.Entities
						.Select(e => new { Title = e.GetAttributeValue<string>("title"), Url = urlProvider.GetUrl(serviceContext, e) })
						.Where(e => !string.IsNullOrEmpty(e.Title))
						.Select(e => new ChildCategory(e.Title, e.Url))
						.OrderBy(e => e.Title);
			}

			return childCategories;
		}
	}
}
