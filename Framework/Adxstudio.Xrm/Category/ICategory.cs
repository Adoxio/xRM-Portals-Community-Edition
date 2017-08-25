/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Category Interface
    /// </summary>
    public interface ICategory
    {
        /// <summary>
        /// Category ID
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Category Title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Category Number
        /// </summary>
        string CategoryNumber { get; }

        /// <summary>
        /// Entity to hold Category entity
        /// </summary>
        Entity Entity { get; }

        /// <summary>
        /// Entity Reference to hold the Category entity reference
        /// </summary>
        EntityReference EntityReference { get; }

        /// <summary>
        /// Category URL
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Entity Reference to hold the Parent category reference
        /// </summary>
        EntityReference ParentCategory { get; }
    }
}
