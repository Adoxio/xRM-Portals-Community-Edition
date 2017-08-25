/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Category Aggregation Data Adapter Interface
    /// </summary>
    public interface ICategoryAggregationDataAdapter
    {
        /// <summary>
        /// Get Root level Categories
        /// </summary>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category</returns>
        IEnumerable<ICategory> SelectTopLevelCategories(int pageSize);

        /// <summary>
        /// Get Recent Categories
        /// </summary>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category</returns>
        IEnumerable<ICategory> SelectRecentCategories(int pageSize);

        /// <summary>
        /// Get Popular Categories
        /// </summary>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category</returns>
        IEnumerable<ICategory> SelectPopularCategories(int pageSize);

        /// <summary>
        /// Get Related Categories
        /// </summary>
        /// <param name="categoryId">Category Id</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>IEnumerable collection of Category</returns>
        IEnumerable<ICategory> SelectRelatedCategories(Guid categoryId, int pageSize = 5);

        /// <summary>
        /// Get a Category by Category Number
        /// </summary>
        /// <param name="categoryNumber">Category Number</param>
        /// <returns>ICategory with corresponding <paramref name="categoryNumber"/></returns>
        ICategory SelectByCategoryNumber(string categoryNumber);
    }
}
