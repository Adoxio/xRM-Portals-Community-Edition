/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Adxstudio.Xrm.Cms;
    using Adxstudio.Xrm.ContentAccess;
    using Adxstudio.Xrm.Resources;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Adxstudio.Xrm.Services.Query;
    using Microsoft.Xrm.Portal;
    using Microsoft.Xrm.Portal.Configuration;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Implementation of <see cref="ContentAccessProvider" />  Base class for providing the Record level Content Access
    /// </summary>
    public abstract class ContentAccessProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentAccessProvider"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for FetchXML attributes</param>
        /// <param name="portalName">Specifies portal name to generate portal context</param>
        protected ContentAccessProvider(ContentAccessConfiguration configuration, string portalName = null) : this(configuration, PortalCrmConfigurationManager.CreatePortalContext(portalName))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentAccessProvider"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for FetchXML attributes</param>
        /// <param name="portalContext">Portal Context</param>
        /// <param name="siteSettingDictionary">Site Setting Dictionary</param>
        protected ContentAccessProvider(ContentAccessConfiguration configuration, IPortalContext portalContext, Dictionary<string, string> siteSettingDictionary = null)
        {
            this.Config = configuration;
            this.Portal = portalContext;
            this.siteSettingDictionary = siteSettingDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentAccessProvider"/> class.
        /// </summary>
        protected ContentAccessProvider()
        {
        }


        /// <summary>
        /// Applies Record level filtering for Knowledge articles
        /// </summary>
        /// <param name="right">Curreent Permission Right</param>
        /// <param name="fetchIn">Existing Fetchxml to add addition filtering</param>
        public abstract void TryApplyRecordLevelFiltersToFetch(CrmEntityPermissionRight right, Fetch fetchIn);

        #region Protected Members
        /// <summary>
        /// Retrieve the entity metadata for the specified entity type name
        /// </summary>
        /// <param name="serviceContext"><see cref="OrganizationServiceContext"/> to use</param>
        /// <param name="entityLogicalName">Logical name of the entity for which the metadata should be retrieved.</param>
        /// <param name="entityFilters"><see cref="EntityFilters"/> to retrieve</param>
        /// <returns><see cref="EntityMetadata"/></returns>
        protected static EntityMetadata GetEntityMetadata(OrganizationServiceContext serviceContext, string entityLogicalName, EntityFilters entityFilters = EntityFilters.All)
        {
            var retrieveEntityRequest = new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName,
                EntityFilters = entityFilters
            };

            var response = (RetrieveEntityResponse)serviceContext.Execute(retrieveEntityRequest);

            if (response == null)
            {
                throw new ApplicationException(string.Format("RetrieveEntityRequest failed for entity type {0}.", entityLogicalName));
            }

            return response.EntityMetadata;
        }

        /// <summary>
        /// Adds <paramref name="newLink"/> as a child link of <paramref name="link"/>
        /// </summary>
        /// <param name="link">Parent link</param>
        /// <param name="newLink">Child link to be connected</param>
        protected static void AddLink(Link link, Link newLink)
        {
            if (string.IsNullOrEmpty(link.Name))
            {
                link.Name = newLink.Name;
                link.FromAttribute = newLink.FromAttribute;
                link.ToAttribute = newLink.ToAttribute;
                link.Intersect = newLink.Intersect;
                link.Visible = newLink.Visible;
                link.Type = newLink.Type;
                link.Links = newLink.Links;
                link.Alias = newLink.Alias;
            }
            else if (link.Links == null || !link.Links.Any())
            {
                link.Links = new List<Link> { newLink };
            }
            else
            {
                AddLink(link.Links.First(), newLink);
            }
        }

        /// <summary>
        /// Values of owning customer types
        /// </summary>
        protected enum OwningCustomerType
        {
            Account,
            Contact
        }

        /// <summary>
		/// Information to aid in building the link-entity elements in FetchXml.
		/// </summary>
		protected class LinkDetails
        {
            /// <summary>
            /// Primary entity logical name
            /// </summary>
            public string Entity1Name { get; private set; }

            /// <summary>
            /// Secondary entity logical name
            /// </summary>
            public string Entity2Name { get; private set; }

            /// <summary>
            /// Schema name of the relationship between the two entities.
            /// </summary>
            public string RelationshipName { get; private set; }

            /// <summary>
            /// Scope of the permission rule.
            /// </summary>
            public OwningCustomerType? Scope { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LinkDetails"/> class.
            /// </summary>
            /// <param name="entity1Name">Entity 1 Name</param>
            /// <param name="entity2Name">Entity 2 Name</param>
            /// <param name="relationshipName">Relationship Name</param>
            /// <param name="scope">Optional <see cref="EntityPermissionScope"/></param>
            public LinkDetails(string entity1Name, string entity2Name, string relationshipName, OwningCustomerType? scope = null)
            {
                this.Entity1Name = entity1Name;
                this.Entity2Name = entity2Name;
                this.RelationshipName = relationshipName;
                this.Scope = scope;
            }
        }

        /// <summary>
        /// Site Setting Dictionary
        /// </summary>
        protected Dictionary<string, string> siteSettingDictionary; 

        /// <summary>
        /// Stores the ContentAccessConfiguration used for configuring the FetchXML queries
        /// </summary>
        protected readonly ContentAccessConfiguration Config;

        /// <summary>
        /// Checks whether a site setting is enabled or not
        /// </summary>
        /// <param name="siteSettingName">Name of Site Setting</param>
        /// <returns>True if given site setting is enabled otherwise false</returns>
        protected bool IsSiteSettingEnabled(string siteSettingName)
        {
            string siteSettingEnabledString;
            if (this.siteSettingDictionary != null && this.siteSettingDictionary.ContainsKey(siteSettingName))
            {
                siteSettingEnabledString = this.siteSettingDictionary[siteSettingName];
            }
            else
            {
                var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
                siteSettingEnabledString = portalContext.ServiceContext.GetSiteSettingValueByName(portalContext.Website, siteSettingName);
            }

            // By default, if the site setting isn't configured - don't enable it
            var siteSettingEnabled = false;

            bool.TryParse(siteSettingEnabledString, out siteSettingEnabled);

            return siteSettingEnabled;
        }

        /// <summary>
        /// Checks for the current Entity Name and Permission Right to determine additional filtering needed
        /// </summary>
        /// <param name="currentRight">Current Permission Right</param>
        /// <param name="fetchIn">existing FetchXML to determine the entity name</param>
        /// <param name="entityNameToCheck">Entity name to check</param>
        /// <param name="rightToCheck">Permission right to check</param>
        /// <returns>True if it is the correct Entity and Permission</returns>
        protected bool IsRightEntityAndPermissionRight(CrmEntityPermissionRight currentRight, Fetch fetchIn, string entityNameToCheck, CrmEntityPermissionRight rightToCheck)
        {
            var currentEntityName = fetchIn.Entity.Name;
            if (string.IsNullOrWhiteSpace(currentEntityName))
            {
                throw new ApplicationException("Fetch must contain and entity element with a name property value.");
            }

            if (currentEntityName == entityNameToCheck && currentRight == rightToCheck)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether Content Access Filters are enabled or not, by checking config's site setting
        /// </summary>
        /// <returns>True if config's site setting exists and is enabled</returns>
        public virtual bool IsEnabled()
        {
            return this.Config != null && this.IsSiteSettingEnabled(this.Config.SiteSettingName);
        }

        /// <summary>
        /// Returns EntityReference of current User
        /// </summary>
        protected EntityReference CurrentUserEntityReference
        {
            get
            {
                return this.Portal.User == null
                    ? null
                    : this.Portal.User.ToEntityReference();
            }
        }

        /// <summary>
        /// Returns EntityReference of current User's parent Customer
        /// </summary>
        protected EntityReference ParentCustomerEntityReference
        {
            get
            {
                return this.Portal.User == null || !this.Portal.User.Attributes.ContainsKey("parentcustomerid")
                    ? null
                    : this.Portal.User.GetAttributeValue<EntityReference>("parentcustomerid");
            }
        }

        /// <summary>
        /// Returns Guid of current User
        /// </summary>
        protected Guid CurrentUserId
        {
            get
            {
                return this.Portal.User == null
                    ? Guid.Empty
                    : this.Portal.User.Id;
            }
        }

        /// <summary>
        /// Returns Guid of current User's parent Customer
        /// </summary>
        protected Guid ParentCustomerId
        {
            get
            {
                var parentCustomerEntityReference = this.ParentCustomerEntityReference;
                return parentCustomerEntityReference == null ? Guid.Empty : parentCustomerEntityReference.Id;
            }
        }

        /// <summary>
        /// Current Portal Context
        /// </summary>
        protected IPortalContext Portal { get; private set; }
        #endregion

        
    }
}
