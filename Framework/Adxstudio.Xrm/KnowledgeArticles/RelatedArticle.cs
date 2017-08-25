/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	public class RelatedArticle : IRelatedArticle
	{
		public string Title { get; private set; }

		public string Url { get; private set; }

		public RelatedArticle(string title, string url)
		{
			Title = title;
			Url = url;
		}
	}
}
