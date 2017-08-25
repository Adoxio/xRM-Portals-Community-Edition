/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.ContentAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Adxstudio.Xrm.Cms;
    using Adxstudio.Xrm.Resources;
    using Adxstudio.Xrm.Security;
    using Adxstudio.Xrm.Services.Query;
    using Microsoft.Xrm.Portal.Configuration;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Adxstudio.Xrm.Web.UI;
    using Microsoft.Xrm.Portal;
    using Microsoft.Xrm.Sdk.Metadata;

    /// <summary>
    /// Implementation of <see cref="ProductAccessProvider"/>. Provides filtering based on Product associations.
    /// </summary>
    public sealed class ProductAccessProvider : ContentAccessProvider
    {
		#region Private Members
		/// <summary>
		/// DisplayArticlesWithoutAssociatedProducts Site Setting Name
		/// </summary>
		private const string DisplayArticlesWithoutAssociatedProductsSiteSettingName = "ProductFiltering/DisplayArticlesWithoutAssociatedProducts";

        /// <summary>
        /// ContactToProductRelationshipNames Site Setting Name
        /// </summary>
        private const string ContactToProductRelationshipNames = "ProductFiltering/ContactToProductRelationshipNames";

        /// <summary>
        /// AccountToProductRelationshipNames Site Setting Name
        /// </summary>
        private const string AccountToProductRelationshipNames = "ProductFiltering/AccountToProductRelationshipNames";

        /// <summary>
        /// Fallback relationship name that maps from Contact to Product for Portals that don't have the new Site Setting data
        /// </summary>
        private const string AccountToProductFallbackRelationshipName = "adx_accountproduct";

        /// <summary>
        /// Fallback relationship name that maps from Contact to Product for Portals that don't have the new Site Setting data
        /// </summary>
        private const string ContactToProductFallbackRelationshipName = "adx_contactproduct";

        /// <summary>
        /// Semicolon delimited string of relationship names
        /// </summary>
	    private readonly Dictionary<string, string> relationshipNamesDictionary;

        /// <summary>
        /// Dictionary of relationship metadata that defines relationship attributes
        /// </summary>
        private Dictionary<string, ProductAccessProvider.RelationshipMetadata> relationshipMetadataDictionary;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAccessProvider"/> class.
        /// </summary>
        public ProductAccessProvider()
            : this(ContentAccessConfiguration.DefaultProductFilteringConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAccessProvider"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for FetchXML attributes</param>
        public ProductAccessProvider(ContentAccessConfiguration configuration)
            : base(configuration)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductAccessProvider"/> class.
		/// </summary>
		/// <param name="portalContext">Configuration for FetchXML attributes</param>
		public ProductAccessProvider(IPortalContext portalContext)
            : base(ContentAccessConfiguration.DefaultProductFilteringConfiguration(), portalContext)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAccessProvider"/> class.
        /// </summary>
        /// <param name="portalContext">Configuration for FetchXML attributes</param>
        /// <param name="relationshipNamesDictionary">Semicolon delimited string of relationship names</param>
        /// <param name="relationshipMetadataDictionary">Relationship metadata that defines relationship attributes</param>
        /// <param name="siteSettingDictionary">Site Setting for Product Filtering</param>
        public ProductAccessProvider(IPortalContext portalContext, Dictionary<string, string> relationshipNamesDictionary, Dictionary<string, ProductAccessProvider.RelationshipMetadata> relationshipMetadataDictionary, Dictionary<string, string> siteSettingDictionary)
	        : base(ContentAccessConfiguration.DefaultProductFilteringConfiguration(), portalContext, siteSettingDictionary)
	    {
	        this.relationshipNamesDictionary = relationshipNamesDictionary;
	        this.relationshipMetadataDictionary = relationshipMetadataDictionary;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies the Product filtering to the existing FetchXML query
        /// </summary>
        /// <param name="right">Current Permission Right</param>
        /// <param name="fetchIn">FetchXML to modify</param>
        public override void TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight right, Fetch fetchIn)
        {
            // Apply filter only if Entity is "Knowledge Article" and Right is "Read"
            if (!this.IsRightEntityAndPermissionRight(right, fetchIn, this.Config.SourceEntityName, CrmEntityPermissionRight.Read))
            {
                return;
            }

            // If Product Filtering is not enabled
            if (!this.IsEnabled())
            {
                return;
            }

            // Retrieve Product IDs
            var userProductIDs = this.GetProducts();

            // Inject Product IDs filter into FetchXML
            this.TryRecordLevelFiltersToFetch(fetchIn, userProductIDs);
        }

        /// <summary>
        /// Retrieves the list of Products available to the current user
        /// </summary>
        /// <returns>Product entity collection</returns>
        public List<Guid> GetProducts()
        {
            // If anonymous user, return nothing
            if (this.CurrentUserEntityReference == null)
            {
                return Enumerable.Empty<Guid>().ToList();
            }

            var productFetch = new Fetch
            {
                Distinct = true,
                Entity = new FetchEntity
                {
                    Name = "product",
                    Attributes = new List<FetchAttribute>
                    {
                        new FetchAttribute("productid")
                    },
                    Filters = new List<Filter>()
                }
            };

            var associatedToAccountOrContactFilter = new Filter
            {
                Type = LogicalOperator.Or,
                Conditions = new List<Condition>(),
                Filters = new List<Filter>()
            };

			// Get alias generator instance to maintain alias names consistency
			// via postfix incrementation
			var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance();

			// Retrieve Contact to Product relationships and build Entity Permission links
			var contactToProductRelationshipNamesCollection = 
				this.GetDelimitedSiteSettingValueCollection(ContactToProductRelationshipNames, ContactToProductFallbackRelationshipName);
            var contactLink = this.BuildLinksAndFilterChain(
				contactToProductRelationshipNamesCollection, productFetch, associatedToAccountOrContactFilter, 
				this.CurrentUserEntityReference, null, OwningCustomerType.Contact, linkEntityAliasGenerator);
            productFetch.AddLink(contactLink);

            if (this.ParentCustomerEntityReference != null && this.ParentCustomerEntityReference.LogicalName == "contact")
            {
                // Retrieve parent Contact to Product relationships and build Entity Permission links
                var parentContactLink = this.BuildLinksAndFilterChain(
					contactToProductRelationshipNamesCollection, productFetch, associatedToAccountOrContactFilter,
					this.ParentCustomerEntityReference, null, OwningCustomerType.Contact, linkEntityAliasGenerator);
                productFetch.AddLink(parentContactLink);
            }
            else if (this.ParentCustomerEntityReference != null && this.ParentCustomerEntityReference.LogicalName == "account")
            {
                // Retrieve Account to Product relationships and build Entity Permission links
                var accountToProductRelationshipNamesCollection = 
					this.GetDelimitedSiteSettingValueCollection(AccountToProductRelationshipNames, AccountToProductFallbackRelationshipName);
                var accountLink = this.BuildLinksAndFilterChain(
					accountToProductRelationshipNamesCollection, productFetch, associatedToAccountOrContactFilter,
					null, this.ParentCustomerEntityReference, OwningCustomerType.Account, linkEntityAliasGenerator);
                productFetch.AddLink(accountLink);
            }

            var accountOrContactNotNullFilter = new Filter
            {
                Type = LogicalOperator.Or,
                Conditions =
                    associatedToAccountOrContactFilter.Conditions.Select(
                        condition =>
                            new Condition
                            {
                                EntityName = condition.EntityName,
                                Attribute = condition.Attribute,
                                Operator = ConditionOperator.NotNull
                            }).ToList()
            };

            // This is the AND Filter that will ensure state is Active and the Product is joined to either Contact or Account
            productFetch.AddFilter(new Filter
            {
                Type = LogicalOperator.And,
                Conditions = new List<Condition>
                {
                    new Condition("statecode", ConditionOperator.Equal, 0)
                },
                Filters = new List<Filter>
                {
                    accountOrContactNotNullFilter,
                    associatedToAccountOrContactFilter,
                }
            });

            var productsCollection = productFetch.Execute(this.Portal.ServiceContext as IOrganizationService);
            return productsCollection.Entities.Select(x => x.Id).ToList();
        }

		/// <summary>
		/// Checks if the sitesetting for allowing users to see non associated articles.
		/// </summary>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public bool DisplayArticlesWithoutAssociatedProductsEnabled()
	    {
		    return this.IsSiteSettingEnabled(DisplayArticlesWithoutAssociatedProductsSiteSettingName);
	    }

        /// <summary>
        /// Specifies relationship type between two entities
        /// </summary>
        public enum RelationshipType
        {
            OneToManyRelationship,
            ManyToOneRelationship,
            ManyToManyRelationship
        }

        /// <summary>
        /// Specifies relationship metadata between two entities
        /// </summary>
        public class RelationshipMetadata
        {
            /// <summary>
            /// Name of Intersect Entity
            /// </summary>
            public string IntersectEntityName { get; set; }

            /// <summary>
            /// Entity 1 Logical Name
            /// </summary>
            public string Entity1LogicalName { get; set; }

            /// <summary>
            /// Entity 2 Logical Name
            /// </summary>
            public string Entity2LogicalName { get; set; }

            /// <summary>
            /// Entity 1 Intersect Attribute
            /// </summary>
            public string Entity1IntersectAttribute { get; set; }

            /// <summary>
            /// Entity 2 Intersect Attribute
            /// </summary>
            public string Entity2IntersectAttribute { get; set; }

            /// <summary>
            /// Relationship Type
            /// </summary>
            public RelationshipType RelationshipType { get; set; }

            /// <summary>
            /// Referencing Attribute
            /// </summary>
            public string ReferencingAttribute { get; set; }

            /// <summary>
            /// Referenced Attribute
            /// </summary>
            public string ReferencedAttribute { get; set; }

            /// <summary>
            /// Referenced Entity
            /// </summary>
            public string ReferencedEntity { get; set; }

            /// <summary>
            /// Entity 1 Primary Id Attribute
            /// </summary>
            public string Entity1PrimaryIdAttribute { get; set; }

            /// <summary>
            /// Entity 2 Primary Id Attribute
            /// </summary>
            public string Entity2PrimaryIdAttribute { get; set; }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns the first empty innermost link or constructs new one if necessary
        /// </summary>
        /// <param name="rootLink">Root link to search</param>
        /// <returns>Empty innermost link</returns>
        private static Link GetInnermostLink(Link rootLink)
        {
            var currentLink = rootLink;
            while (currentLink.Links != null && currentLink.Links.Any())
            {
                currentLink = currentLink.Links.First();
            }

            if (!string.IsNullOrWhiteSpace(currentLink.Name))
            {
                var newInnermostLink = new Link();
                currentLink.Links = new List<Link>
                {
                        newInnermostLink
                    };
                currentLink = newInnermostLink;
            }

            return currentLink;
        }

        /// <summary>
        /// Modify a fetch and add necessary link entity elements and filter conditions to satisfy record level security trimming based on the relationship definitions.
        /// </summary>
        /// <param name="serviceContext"><see cref="OrganizationServiceContext"/> to use</param>
        /// <param name="relationshipMetadata">Relationship metadata that defines relationship attributes</param>
        /// <param name="linkDetails"><see cref="ContentAccessProvider.LinkDetails"/> to use</param>
        /// <param name="fetch">Fetch to modify</param>
        /// <param name="link">Link to construct</param>
        /// <param name="filter">Filter to construct</param>
        /// <param name="contact">Associated Contact</param>
        /// <param name="account">Associated Account</param>
        /// <param name="addCondition">Construct Account/Contact relationship filter</param>
        /// <param name="linkEntityAliasGenerator">LinkEntityAliasGenerator to track and create Aliases</param>
        private static void BuildLinksAndFilter(OrganizationServiceContext serviceContext, ProductAccessProvider.RelationshipMetadata relationshipMetadata, LinkDetails linkDetails, Fetch fetch, Link link, Filter filter, EntityReference contact, EntityReference account, bool addCondition, LinkEntityAliasGenerator linkEntityAliasGenerator)
        {
            var alias = linkEntityAliasGenerator.CreateUniqueAlias(relationshipMetadata.Entity2LogicalName);
            Link newLink = null;


            if (relationshipMetadata.RelationshipType == ProductAccessProvider.RelationshipType.ManyToManyRelationship)
            {
                var intersectLinkEntityName = relationshipMetadata.IntersectEntityName;
                string linkTargetFromAttribute;
                string linkTargetToAttribute;
                string linkIntersectFromAttribute;
                string linkIntersectToAttribute;
                if (relationshipMetadata.Entity1LogicalName == relationshipMetadata.Entity2LogicalName)
                {
                    linkIntersectFromAttribute = relationshipMetadata.Entity2IntersectAttribute;
                    linkIntersectToAttribute = relationshipMetadata.Entity1PrimaryIdAttribute;
                    linkTargetFromAttribute = relationshipMetadata.Entity1PrimaryIdAttribute;
                    linkTargetToAttribute = relationshipMetadata.Entity1IntersectAttribute;
                }
                else
                {
                    linkIntersectFromAttribute =
                        linkIntersectToAttribute = relationshipMetadata.Entity1LogicalName == linkDetails.Entity1Name
                            ? relationshipMetadata.Entity1IntersectAttribute
                            : relationshipMetadata.Entity2IntersectAttribute;
                    linkTargetFromAttribute =
                        linkTargetToAttribute = relationshipMetadata.Entity2LogicalName == linkDetails.Entity2Name
                            ? relationshipMetadata.Entity2IntersectAttribute
                            : relationshipMetadata.Entity1IntersectAttribute;
                }

                newLink = new Link
                {
                    Name = intersectLinkEntityName,
                    FromAttribute = linkIntersectFromAttribute,
                    ToAttribute = linkIntersectToAttribute,
                    Intersect = true,
                    Visible = false,
                    Type = JoinOperator.LeftOuter,
                    Links = new List<Link>
                    {
                            new Link
                            {
                                Name = relationshipMetadata.Entity2LogicalName,
                                FromAttribute = linkTargetFromAttribute,
                                ToAttribute = linkTargetToAttribute,
                                Alias = alias,
                                Type = JoinOperator.LeftOuter
                            }
                        }
                };
            }
            else if (relationshipMetadata.RelationshipType == ProductAccessProvider.RelationshipType.ManyToOneRelationship)
            {
                var linkFromAttribute = relationshipMetadata.ReferencedEntity == relationshipMetadata.Entity2LogicalName
                    ? relationshipMetadata.ReferencedAttribute
                    : relationshipMetadata.ReferencingAttribute;

                var linkToAttribute = relationshipMetadata.ReferencedEntity == relationshipMetadata.Entity2LogicalName
                    ? relationshipMetadata.ReferencingAttribute
                    : relationshipMetadata.ReferencedAttribute;

                newLink = new Link
                {
                    Name = relationshipMetadata.Entity2LogicalName,
                    FromAttribute = linkFromAttribute,
                    ToAttribute = linkToAttribute,
                    Type = JoinOperator.LeftOuter,
                    Alias = alias
                };
            }
            else if (relationshipMetadata.RelationshipType == ProductAccessProvider.RelationshipType.OneToManyRelationship)
            {
                var linkFromAttribute = relationshipMetadata.ReferencedEntity == relationshipMetadata.Entity2LogicalName
                    ? relationshipMetadata.ReferencedAttribute
                    : relationshipMetadata.ReferencingAttribute;

                var linkToAttribute = relationshipMetadata.ReferencedEntity == relationshipMetadata.Entity2LogicalName
                    ? relationshipMetadata.ReferencingAttribute
                    : relationshipMetadata.ReferencedAttribute;

                newLink = new Link
                {
                    Name = relationshipMetadata.Entity2LogicalName,
                    FromAttribute = linkFromAttribute,
                    ToAttribute = linkToAttribute,
                    Type = JoinOperator.LeftOuter,
                    Alias = alias
                };
            }
            else
            {
                throw new ApplicationException(string.Format("Retrieve relationship request failed for relationship name {0}", linkDetails.RelationshipName));
            }

            ContentAccessProvider.AddLink(link, newLink);


            if (addCondition) // Only add the condition if we are at the end of the chain
            {
                var condition = new Condition { Attribute = relationshipMetadata.Entity2PrimaryIdAttribute };

                if (linkDetails.Scope.HasValue && linkDetails.Scope.Value == OwningCustomerType.Contact)
                {
                    condition.EntityName = alias;
                    condition.Operator = ConditionOperator.Equal;
                    condition.Value = contact.Id;
                }
                else if (linkDetails.Scope.HasValue && linkDetails.Scope.Value == OwningCustomerType.Account)
                {
                    condition.EntityName = alias;
                    condition.Operator = ConditionOperator.Equal;
                    condition.Value = account.Id;
                }
                else
                {
                    condition.EntityName = alias;
                    condition.Operator = ConditionOperator.NotNull;
                }

                filter.Conditions.Add(condition);
            }

            fetch.Distinct = true;
        }

        /// <summary>
        /// Modify a fetch and add necessary link entity elements and filter conditions to satisfy record level security trimming based on the relationship definitions.
        /// </summary>
        /// <param name="serviceContext"><see cref="OrganizationServiceContext"/> to use</param>
        /// <param name="linkDetails"><see cref="ContentAccessProvider.LinkDetails"/> to use</param>
        /// <param name="fetch">Fetch to modify</param>
        /// <param name="link">Link to construct</param>
        /// <param name="filter">Filter to construct</param>
        /// <param name="contact">Associated Contact</param>
        /// <param name="account">Associated Account</param>
        /// <param name="addCondition">Construct Account/Contact relationship filter</param>
        /// <param name="linkEntityAliasGenerator">LinkEntityAliasGenerator to track and create Aliases</param>
        private void BuildLinksAndFilter(OrganizationServiceContext serviceContext, LinkDetails linkDetails, Fetch fetch, Link link, Filter filter, EntityReference contact, EntityReference account, bool addCondition, LinkEntityAliasGenerator linkEntityAliasGenerator)
        {
            var relationshipMetadata = this.BuildRelationshipMetadata(serviceContext, linkDetails);
            ProductAccessProvider.BuildLinksAndFilter(serviceContext, relationshipMetadata, linkDetails, fetch, link, filter, contact, account, addCondition, linkEntityAliasGenerator);
        }

        /// <summary>
        /// Builds relationship metadata
        /// </summary>
        /// <param name="serviceContext">Service Context</param>
        /// <param name="linkDetails">Link Details</param>
        /// <returns>Relationshi pMetadata</returns>
        private ProductAccessProvider.RelationshipMetadata BuildRelationshipMetadata(OrganizationServiceContext serviceContext, LinkDetails linkDetails)
        {
            // This is used for Mocking
            if (this.relationshipMetadataDictionary != null &&
                this.relationshipMetadataDictionary.ContainsKey(linkDetails.Entity2Name))
            {
                return this.relationshipMetadataDictionary[linkDetails.Entity2Name];
            }

            // Standard flow
            var entity1Metadata = GetEntityMetadata(serviceContext, linkDetails.Entity1Name);
            var entity2Metadata = linkDetails.Entity2Name == linkDetails.Entity1Name ? entity1Metadata : GetEntityMetadata(serviceContext, linkDetails.Entity2Name);

            var relationshipMetadata = new ProductAccessProvider.RelationshipMetadata();
            relationshipMetadata.Entity1PrimaryIdAttribute = entity1Metadata.PrimaryIdAttribute;
            relationshipMetadata.Entity2PrimaryIdAttribute = entity2Metadata.PrimaryIdAttribute;
            relationshipMetadata.Entity1LogicalName = entity1Metadata.LogicalName;
            relationshipMetadata.Entity2LogicalName = entity2Metadata.LogicalName;

            var relationshipManyToMany = entity1Metadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);
            if (relationshipManyToMany != null)
            {
                relationshipMetadata.RelationshipType = ProductAccessProvider.RelationshipType.ManyToManyRelationship;
                relationshipMetadata.Entity1LogicalName = relationshipManyToMany.Entity2LogicalName;
                relationshipMetadata.Entity2LogicalName = relationshipManyToMany.Entity1LogicalName;
                relationshipMetadata.Entity1IntersectAttribute = relationshipManyToMany.Entity2IntersectAttribute;
                relationshipMetadata.Entity2IntersectAttribute = relationshipManyToMany.Entity1IntersectAttribute;
                relationshipMetadata.IntersectEntityName = relationshipManyToMany.IntersectEntityName;
                return relationshipMetadata;
            }

            var relationshipManyToOne = entity1Metadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);
            if (relationshipManyToOne != null)
            {
                relationshipMetadata.RelationshipType = ProductAccessProvider.RelationshipType.ManyToOneRelationship;
                relationshipMetadata.ReferencedEntity = relationshipManyToOne.ReferencedEntity;
                relationshipMetadata.ReferencingAttribute = relationshipManyToOne.ReferencingAttribute;
                relationshipMetadata.ReferencedAttribute = relationshipManyToOne.ReferencedAttribute;
                return relationshipMetadata;
            }

            var relationshipOneToMany = entity1Metadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == linkDetails.RelationshipName);
            if (relationshipOneToMany != null)
            {
                relationshipMetadata.RelationshipType = ProductAccessProvider.RelationshipType.OneToManyRelationship;
                relationshipMetadata.ReferencedEntity = relationshipOneToMany.ReferencedEntity;
                relationshipMetadata.ReferencedAttribute = relationshipOneToMany.ReferencedAttribute;
                relationshipMetadata.ReferencingAttribute = relationshipOneToMany.ReferencingAttribute;
                return relationshipMetadata;
            }

            // This would be a failed case
            return null;
        }

        /// <summary>
        /// Retrieves site setting value collection as a delimited collection, if list is empty or exceeds 2 entries it will return empty collection
        /// </summary>
        /// <param name="siteSettingName">Site Setting Name</param>
        /// <param name="fallbackValue">Value to return if the Site Setting doesn't exist</param>
        /// <returns>Array of site setting values</returns>
        private string[] GetDelimitedSiteSettingValueCollection(string siteSettingName, string fallbackValue = "")
        {
            // Retrieve site setting by name
            var customerToProductRelationshipNamesString = this.relationshipNamesDictionary != null && this.relationshipNamesDictionary.ContainsKey(siteSettingName)
                ? this.relationshipNamesDictionary[siteSettingName]
                : this.Portal.ServiceContext.GetSiteSettingValueByName(this.Portal.Website, siteSettingName);

            // If site setting doesn't exist, return fallbackValue
            if (string.IsNullOrWhiteSpace(customerToProductRelationshipNamesString))
            {
                return new string[] { fallbackValue };
            }

            // Ensure that the relationship depth is <= 2 (This ensure that only one intersect is between Product and Owning Entity (Contact/Account)
            var customerToProductRelationshipNamesCollection = customerToProductRelationshipNamesString.Split(';');
            if (customerToProductRelationshipNamesCollection.Count() > 2)
            {
                // This would indicate a customer misconfiguration by specifying too many relationships
                return new string[0];
            }

            return customerToProductRelationshipNamesCollection;
        }

		/// <summary>
		/// Constructs a link-entity chain from the <paramref name="fetch"/>
		/// </summary>
		/// <param name="customerToProductRelationshipNamesCollection">Collection of relationships that map from Customer to Product</param>
		/// <param name="fetch">Fetch used to construct link chain</param>
		/// <param name="filter">Filter to inject conditions into</param>
		/// <param name="contact">Contact EntityReference</param>
		/// <param name="account">Account EntityReference</param>
		/// <param name="owningCustomerType">Owning Customer Type</param>
		/// <param name="linkEntityAliasGenerator">Single instance to maintain alias postfix incrementation</param>
		/// <returns>Root link of the constructed chain</returns>
		private Link BuildLinksAndFilterChain(string[] customerToProductRelationshipNamesCollection, 
			Fetch fetch, Filter filter, EntityReference contact, EntityReference account, 
			OwningCustomerType owningCustomerType, LinkEntityAliasGenerator linkEntityAliasGenerator)
        {
            var rootLink = new Link();
            var linkDetails = this.GetLinkDetails(customerToProductRelationshipNamesCollection, owningCustomerType);
            foreach (var linkDetail in linkDetails)
            {
                var innermostLink = GetInnermostLink(rootLink);

                var currentLinkDetailConnectsToAccountOrContact = (contact != null && (linkDetail.Entity1Name == "contact" || linkDetail.Entity2Name == "contact")) ||
                                                                  (account != null && (linkDetail.Entity1Name == "account" || linkDetail.Entity2Name == "account"));
                this.BuildLinksAndFilter(this.Portal.ServiceContext, linkDetail, fetch, innermostLink, filter, contact, account, currentLinkDetailConnectsToAccountOrContact, linkEntityAliasGenerator);
            }

            return rootLink;
        }

        /// <summary>
        /// Constructs the <see cref="LinkDetails"/> collection that maps from specified <see cref="EntityPermissionScope"/> to Product
        /// </summary>
        /// <param name="contactToProductRelationshipNamesCollection">Collection of relationship names that map from specified <see cref="EntityPermissionScope"/> to Product</param>
        /// <param name="owningCustomerType"><see cref="EntityPermissionScope"/> of owning Customer</param>
        /// <returns>Collection of <see cref="LinkDetails"/> from specified <see cref="OwningCustomerType"/> to Product</returns>
        private IEnumerable<LinkDetails> GetLinkDetails(string[] contactToProductRelationshipNamesCollection, OwningCustomerType owningCustomerType)
        {
            // Specify the corresponding Customer schema name based on passed EntityPermissionScope
            var entityPermissionScopeName = owningCustomerType == OwningCustomerType.Account
                ? "account"
                : "contact";

            var linkDetailsCollection = new List<LinkDetails>();

            if (contactToProductRelationshipNamesCollection.Count() == 1)
            {
                linkDetailsCollection.Add(new LinkDetails("product", entityPermissionScopeName, contactToProductRelationshipNamesCollection[0], owningCustomerType));
            }
            else if (contactToProductRelationshipNamesCollection.Count() == 2)
            {
                // Retrieve Customer and Product metadata
                var customerMetadata = GetEntityMetadata(this.Portal.ServiceContext, entityPermissionScopeName);
                var productMetadata = GetEntityMetadata(this.Portal.ServiceContext, "product");

                // Retrieve the intersecting entities schema name for the specified relationship name
                var customer2IntersectSchemaName = customerMetadata.OneToManyRelationships.FirstOrDefault(metadata => metadata.SchemaName == contactToProductRelationshipNamesCollection[0]);
                var product2IntersectSchemaName = productMetadata.OneToManyRelationships.FirstOrDefault(metadata => metadata.SchemaName == contactToProductRelationshipNamesCollection[1]);

                // Ensure that the insersecting entity for the specified relationships match
                if (customer2IntersectSchemaName == null || product2IntersectSchemaName == null || product2IntersectSchemaName.ReferencingEntity != customer2IntersectSchemaName.ReferencingEntity)
                {
                    return linkDetailsCollection;
                }

                // Add the specified LinkDetails to the collection
                linkDetailsCollection.Add(new LinkDetails("product", product2IntersectSchemaName.ReferencingEntity, contactToProductRelationshipNamesCollection[1]));
                linkDetailsCollection.Add(new LinkDetails(customer2IntersectSchemaName.ReferencingEntity, entityPermissionScopeName, contactToProductRelationshipNamesCollection[0], owningCustomerType));
            }

            return linkDetailsCollection;
        }

        /// <summary>
        /// Determines whether Product filtering should filter only Matching Products
        /// </summary>
        private bool FilterWithMatchingProductsOnly
        {
            get
            {
                var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
                var displayArticlesWithoutAssociatedProductsString = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, DisplayArticlesWithoutAssociatedProductsSiteSettingName);
                bool result;

                // If the site setting is missing or value is empty, default to filtering with matching Products only
                if (string.IsNullOrWhiteSpace(displayArticlesWithoutAssociatedProductsString) || !bool.TryParse(displayArticlesWithoutAssociatedProductsString, out result))
                {
                    return true;
                }

                // If it doesn't match value below, default to filtering with matching Products only
                return !result;
            }
        }

        /// <summary>
        /// Adds required Product filter into existing FetchXMl
        /// </summary>
        /// <param name="fetchIn">FetchXML that is constructed through Entity Permissions</param>
        /// <param name="userProductIDs">Collection of Product IDs</param>
        private void TryRecordLevelFiltersToFetch(Fetch fetchIn, List<Guid> userProductIDs)
        {
            // Build link to Product from Knowledge Article
            var link = new Link()
            {
                Alias = this.Config.IntersectAlias,
                Name = this.Config.IntersectEntityName,
                FromAttribute = this.Config.IntersectFromAttribute,
                ToAttribute = this.Config.IntersectToAttribute,
                Visible = false,
                Type = JoinOperator.LeftOuter,
                Intersect = true,
                Links = new List<Link>
                {
                    new Link()
                    {
                        Alias = this.Config.TargetAlias,
                        Name = this.Config.TargetEntityName,
                        FromAttribute = this.Config.TargetFromAttribute,
                        ToAttribute = this.Config.TargetToAttribute,
                        Visible = false,
                        Type = JoinOperator.LeftOuter,
                        Intersect = true,
                    }
                }
            };

            // Add Link to the FetchXML
            if (fetchIn.Entity.Links == null)
            {
                fetchIn.Entity.Links = new List<Link>();
            }
            fetchIn.Entity.Links.Add(link);

            // Build and add Filter to the FetchXML
            var filter = this.BuildFilter(userProductIDs);
            if (fetchIn.Entity.Filters == null)
            {
                fetchIn.Entity.Filters = new List<Filter>();
            }
            fetchIn.Entity.Filters.Add(filter);

			fetchIn.Distinct = true;
        }

        /// <summary>
        /// Builds the Filter to trim the Knowledge Article fetch results
        /// </summary>
        /// <param name="userProductIDs">Collection of Product IDs</param>
        /// <returns>Product filter</returns>
        private Filter BuildFilter(List<Guid> userProductIDs)
        {
            var userProductsObjectCollection = new List<object>();
            if (userProductIDs != null && userProductIDs.Any())
            {
                userProductsObjectCollection.AddRange(userProductIDs.Cast<object>().ToList());
            }
            else
            {
                userProductsObjectCollection.Add((object)Guid.Empty);
            }

            var filterConditions = new List<Condition>
            {
                new Condition { EntityName = this.Config.TargetAlias, Attribute = this.Config.TargetFromAttribute, Operator = ConditionOperator.In, Values = userProductsObjectCollection }
            };

            // If User is Authenticated and Site Setting configured for showing unassociated Articles
            if (this.CurrentUserEntityReference != null && !this.FilterWithMatchingProductsOnly)
            {
                filterConditions.Add(new Condition { EntityName = this.Config.IntersectAlias, Attribute = this.Config.IntersectFromAttribute, Operator = ConditionOperator.Null });
            }

            return new Filter()
            {
                Type = LogicalOperator.Or,
                Conditions = filterConditions
            };
        }

        /// <summary>
        /// String array of current User's associated Webroles
        /// </summary>
        private string[] CurrentUserRoleNames { get; set; }
        #endregion Helper Methods
    }
}
