/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.KnowledgeArticles
{
	public interface IKnowledgeArticleAggregationDataAdapter
	{
		IEnumerable<IKnowledgeArticle> SelectTopArticles(int pageSize = 5, string languageLocaleCode = null);

		IEnumerable<IKnowledgeArticle> SelectRecentArticles(int pageSize = 5, string languageLocaleCode = null);
	}
}
