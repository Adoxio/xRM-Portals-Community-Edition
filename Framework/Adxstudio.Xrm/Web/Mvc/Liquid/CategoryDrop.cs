/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
    using System;
    using Category;

    /// <summary>
    /// Category Drop to call from liquid templates
    /// </summary>
    public class CategoryDrop : EntityDrop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDrop"/> class.
        /// </summary>
        /// <param name="portalLiquidContext"> Portal Liquid Context</param>
        /// <param name="dependencies">Data Adapter Dependencies</param>
        /// <param name="category">current Category</param>
        public CategoryDrop(IPortalLiquidContext portalLiquidContext, KnowledgeArticles.IDataAdapterDependencies dependencies, ICategory category)
            : base(portalLiquidContext, category.Entity)
        {
            if (dependencies == null) { throw new ArgumentNullException("dependencies"); }
            if (category == null) { throw new ArgumentNullException("category"); }

            this.Category = category;
        }

        /// <summary>
        /// Category property
        /// </summary>
        protected ICategory Category { get; private set; }

        /// <summary>
        /// Category Number
        /// </summary>
        public string CategoryNumber
        {
            get { return this.Category.CategoryNumber;  }
        }

        /// <summary>
        /// Category Name
        /// </summary>
        public string Name
        {
            get { return this.Category.Title; }
        }

        /// <summary>
        /// Category Title
        /// </summary>
        public string Title
        {
            get { return this.Category.Title; }
        }
    }
}
