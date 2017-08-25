/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.ContentAccess
{
    using System;
    using Adxstudio.Xrm.Services.Query;

    /// <summary>
    /// Represents configuration details for customizing the link entities used by Content Access Providers
    /// </summary>
    public class ContentAccessConfiguration
    {
        /// <summary>
        /// Site Setting Name
        /// </summary>
        public string SiteSettingName { get; set; }

        /// <summary>
        /// Entity Name
        /// </summary>
        public string SourceEntityName { get; set; }

        /// <summary>
        /// Target Entity Name
        /// </summary>
        public string TargetEntityName { get; set; }

        /// <summary>
        /// Target To Atttribute
        /// </summary>
        public string TargetToAttribute { get; set; }

        /// <summary>
        /// Traget From Atttribute
        /// </summary>
        public string TargetFromAttribute { get; set; }

        /// <summary>
        /// Intersect Entity Name
        /// </summary>
        public string IntersectEntityName { get; set; }

        /// <summary>
        /// Intersect To Atttribute
        /// </summary>
        public string IntersectToAttribute { get; set; }

        /// <summary>
        /// Intersect From Atttribute
        /// </summary>
        public string IntersectFromAttribute { get; set; }

        /// <summary>
        /// Intersect Alias
        /// </summary>
        public string IntersectAlias { get; set; }

        /// <summary>
        /// Target Alias
        /// </summary>
        public string TargetAlias { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentAccessConfiguration"/> class.
        /// </summary>
        /// <param name="siteSettingName">Site Setting Name</param>
        /// <param name="sourceEntityName">Source Entity Name</param>
        /// <param name="targetEntityName">Target Entity Name</param>
        /// <param name="targetFromAttribute">Target From Attribute</param>
        /// <param name="targetToAttribute">Target To Attribute</param>
        /// <param name="intersectEntityName">Intersect Entity Name</param>
        /// <param name="intersectFromAttribute">Intersect From Attribute</param>
        /// <param name="intersectToAttribute">Intersect To Attribute</param>
        public ContentAccessConfiguration(string siteSettingName, string sourceEntityName, string targetEntityName, string targetFromAttribute, string targetToAttribute, string intersectEntityName, string intersectFromAttribute, string intersectToAttribute)
        {
            var linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance();
            this.SiteSettingName = siteSettingName;
            this.SourceEntityName = sourceEntityName;
            this.TargetEntityName = targetEntityName;
            this.TargetFromAttribute = targetFromAttribute;
            this.TargetToAttribute = targetToAttribute;
            this.TargetAlias = linkEntityAliasGenerator.CreateUniqueAlias(targetEntityName);
            this.IntersectEntityName = intersectEntityName;
            this.IntersectFromAttribute = intersectFromAttribute;
            this.IntersectToAttribute = intersectToAttribute;
            this.IntersectAlias = linkEntityAliasGenerator.CreateUniqueAlias(intersectEntityName);
        }

        /// <summary>
        /// Provides default ContentAccessConfiguration for Product Filtering
        /// </summary>
        /// <returns>Default Product Filtering ContentAccessConfiguration</returns>
        public static ContentAccessConfiguration DefaultProductFilteringConfiguration()
        {
            return new ContentAccessConfiguration("ProductFiltering/Enabled", "knowledgearticle", "product", "productid", "record1id", "connection", "record2id", "knowledgearticleid");
        }

        /// <summary>
        /// Provides default ContentAccessConfiguration for Product Filtering
        /// </summary>
        /// <returns>Default Content Access Level ContentAccessConfiguration</returns>
        public static ContentAccessConfiguration DefaultContentAccessLevelConfiguration()
        {
            return new ContentAccessConfiguration("KnowledgeManagement/ContentAccessLevel/Enabled", "knowledgearticle", "adx_contentaccesslevel", "adx_contentaccesslevelid", "adx_contentaccesslevelid", "adx_knowledgearticlecontentaccesslevel", "knowledgearticleid", "knowledgearticleid");
        }

        /// <summary>
        ///  Provides default ContentAccessConfiguration to Category entity for CAL and Product Filtering
        /// </summary>
        /// <returns>Default Category ContentAccessConfiguration</returns>
        public static ContentAccessConfiguration DefaultCategoryConfiguration()
        {
            return new ContentAccessConfiguration(string.Empty, "category", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }
    }
}
