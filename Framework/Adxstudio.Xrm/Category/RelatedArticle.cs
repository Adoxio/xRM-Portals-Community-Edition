/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    /// <summary>
    /// Related Knowledge Article
    /// </summary>
    public class RelatedArticle : IRelatedArticle
    {
        /// <summary>
        /// Knowledge Article's Title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Knowledge Article's URL
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedArticle"/> class.
        /// </summary>
        /// <param name="title">Knowledge Article Title</param>
        /// <param name="url">Knowledge Article URL</param>
        public RelatedArticle(string title, string url)
        {
            this.Title = title;
            this.Url = url;
        }
    }
}
