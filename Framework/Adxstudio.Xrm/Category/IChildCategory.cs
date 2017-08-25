/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    /// <summary>
    /// Child Category Interface
    /// </summary>
    public interface IChildCategory
    {
        /// <summary>
        /// Category's Title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Category's URL
        /// </summary>
        string Url { get; }
    }
}
