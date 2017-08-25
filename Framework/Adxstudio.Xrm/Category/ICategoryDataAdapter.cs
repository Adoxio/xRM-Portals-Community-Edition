/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    using System.Collections.Generic;

    /// <summary>
    /// Category Data Adapter Interface
    /// </summary>
    public interface ICategoryDataAdapter
    {
        /// <summary>
        /// Gets the Category entity
        /// </summary>
        /// <returns>Category interface object</returns>
        ICategory Select();

        /// <summary>
        /// Gets Related Articles of a Category
        /// </summary>
        /// <returns>IEnumerable of Related Article</returns>
        IEnumerable<RelatedArticle> SelectRelatedArticles();

        /// <summary>
        /// Gets Child Categories of a Category
        /// </summary>
        /// <returns>IEnumerable of Child Category</returns>
        IEnumerable<ChildCategory> SelectChildCategories();
        
    }
}
