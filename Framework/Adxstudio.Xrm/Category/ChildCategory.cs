/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    /// <summary>
    /// Child Category
    /// </summary>
    public class ChildCategory : IChildCategory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildCategory"/> class.
        /// </summary>
        /// <param name="title">Child Category Title</param>
        /// <param name="url">Child Category URL</param>
        public ChildCategory(string title, string url)
        {
            this.Title = title;
            this.Url = url;
        }

        /// <summary>
        /// Child Category's Title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Child Category's URL
        /// </summary>
        public string Url { get; private set; }

    }
}
