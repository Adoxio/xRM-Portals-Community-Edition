/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    /// <summary>
    /// Related Article Interface
    /// </summary>
    public interface IRelatedArticle
    {
        /// <summary>
        /// Knowledge Article's Title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Knoweldge Article's URL
        /// </summary>
        string Url { get; }
    }
}
