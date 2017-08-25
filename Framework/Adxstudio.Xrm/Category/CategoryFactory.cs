/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.Xrm.Sdk; 

    /// <summary>
    /// Category Factory
    /// </summary>
    public class CategoryFactory
    {
        /// <summary>
        /// Data Adapter Dependencies
        /// </summary>
        private readonly KnowledgeArticles.IDataAdapterDependencies dependencies;

        /// <summary>
        /// Http Context
        /// </summary>
        private readonly HttpContextBase httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryFactory"/> class.
        /// </summary>
        /// <param name="dependencies">Data Adapter Dependencies</param>
        public CategoryFactory(KnowledgeArticles.IDataAdapterDependencies dependencies)
        {
            dependencies.ThrowOnNull("dependencies");

            this.dependencies = dependencies;

            var request = this.dependencies.GetRequestContext();
            this.httpContext = request == null ? null : request.HttpContext;
        }

        /// <summary>
        /// Converts an Entity Collection to a Category Collection
        /// </summary>
        /// <param name="categoryEntities">IEnumerable collection of Category Entities</param>
        /// <returns>IEnumerable collection of Category</returns>
        public IEnumerable<ICategory> Create(IEnumerable<Entity> categoryEntities)
        {
            var categories = categoryEntities.ToArray();
            var categoryIds = categories.Select(e => e.Id).ToArray();

            return categories.Select(e =>
            {
                return new Category(e);
            }).ToArray();
        }
    }
}
