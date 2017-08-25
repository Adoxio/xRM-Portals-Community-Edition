/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Site.Areas.KnowledgeBase.ViewModels
{
	public class RelatedArticle
	{
		public RelatedArticle(string title, string url)
		{
			Title = title;
			Url = url;
		}

		public string Title { get; private set; }

		public string Url { get; private set; }
	}
}
