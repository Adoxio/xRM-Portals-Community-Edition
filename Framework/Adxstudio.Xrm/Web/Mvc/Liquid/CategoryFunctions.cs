/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Category Functions
    /// </summary>
    public class CategoryFunctions
    {
        /// <summary>
        /// Implementation of Category Drop "Top Level" - Retrieves categories with ParentCategoryId = null
        /// </summary>
        /// <param name="categoriesDrop">Categories Drop</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category Drop</returns>
        public static IEnumerable<CategoryDrop> TopLevel(CategoriesDrop categoriesDrop, int pageSize = 5)
        {
            return new CategoriesDrop(categoriesDrop.PortalLiquidContext, categoriesDrop.Dependencies, pageSize).TopLevel;
        }

        /// <summary>
        /// Implementation of Category Drop "Recent" 
        /// </summary>
        /// <param name="categoriesDrop">Categories Drop</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category Drop</returns>
        public static IEnumerable<CategoryDrop> Recent(CategoriesDrop categoriesDrop, int pageSize = 5)
        {
            return new CategoriesDrop(categoriesDrop.PortalLiquidContext, categoriesDrop.Dependencies, pageSize).Recent;
        }

        /// <summary>
        /// Implementation of Category Drop "Related" 
        /// </summary>
        /// <param name="categoriesDrop">Categories Drop</param>
        /// <param name="categoryId">Category Id of Parent Category</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category Drop</returns>
        public static IEnumerable<CategoryDrop> Related(CategoriesDrop categoriesDrop, Guid categoryId, int pageSize = 5)
        {
            return new CategoriesDrop(categoriesDrop.PortalLiquidContext, categoriesDrop.Dependencies, pageSize).GetRelatedCategories(categoryId, pageSize);
        }

        /// <summary>
        /// Implementation of Category Drop "Recent" 
        /// </summary>
        /// <param name="categoriesDrop">Categories Drop</param>
        /// <param name="categoryNumber">Category Number</param>
        /// <returns>IEnumerable collection of Category Drop</returns>
        public static CategoryDrop CategoryNumber(CategoriesDrop categoriesDrop, string categoryNumber)
        {
            return new CategoriesDrop(categoriesDrop.PortalLiquidContext, categoriesDrop.Dependencies).SelectByCategoryNumber(categoryNumber);
        }
    }
}
