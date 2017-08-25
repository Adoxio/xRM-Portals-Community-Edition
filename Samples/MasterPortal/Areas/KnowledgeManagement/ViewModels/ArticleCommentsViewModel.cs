/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.KnowledgeArticles;

namespace Site.Areas.KnowledgeManagement.ViewModels
{
	public class ArticleCommentsViewModel
	{
		public PaginatedList<IComment> Comments { get; set; }
		
		public IKnowledgeArticle KnowledgeArticle { get; set; }
	}
}
