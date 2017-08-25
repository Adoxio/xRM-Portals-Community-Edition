/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class KnowledgeArticleFunctions
	{
		public static KnowledgeArticlesDrop Options(KnowledgeArticlesDrop articlesDrop, int pageSize = 5, string languageLocaleCode = null)
		{
			return new KnowledgeArticlesDrop(articlesDrop.PortalLiquidContext, articlesDrop.Dependencies, pageSize, languageLocaleCode);
		}

		public static IEnumerable<KnowledgeArticleDrop> Top(KnowledgeArticlesDrop articlesDrop, int pageSize = 5, string languageLocaleCode = null)
		{
			return new KnowledgeArticlesDrop(articlesDrop.PortalLiquidContext, articlesDrop.Dependencies, pageSize, languageLocaleCode).Top;
		}

		public static IEnumerable<KnowledgeArticleDrop> Recent(KnowledgeArticlesDrop articlesDrop, int pageSize = 5, string languageLocaleCode = null)
		{
			return new KnowledgeArticlesDrop(articlesDrop.PortalLiquidContext, articlesDrop.Dependencies, pageSize, languageLocaleCode).Recent;
		}

		public static IEnumerable<KnowledgeArticleDrop> Popular(KnowledgeArticlesDrop articlesDrop, int pageSize = 5, string languageLocaleCode = null)
		{
			return new KnowledgeArticlesDrop(articlesDrop.PortalLiquidContext, articlesDrop.Dependencies, pageSize, languageLocaleCode).Popular;
		}
	}
}
