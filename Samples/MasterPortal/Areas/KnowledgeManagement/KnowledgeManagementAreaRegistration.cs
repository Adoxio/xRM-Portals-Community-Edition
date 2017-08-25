/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc;

namespace Site.Areas.KnowledgeManagement
{
	public class KnowledgeManagementAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "KnowledgeManagement"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{

			context.MapSiteMarkerRoute(
				"KnowledgeArticleWithLegacyLang",
				"Knowledge Article",
				"{number}/{lang}",
				new { controller = "Article", action = "Article", number = UrlParameter.Optional, lang = UrlParameter.Optional, page = UrlParameter.Optional });

			context.MapSiteMarkerRoute(
				"KnowledgeArticleActions",
				"Knowledge Article",
				"{number}/{action}/{id}",
				new { controller = "Article", action = "Article", number = UrlParameter.Optional, id = Guid.Empty });

			context.MapSiteMarkerRoute(
				"KnowledgeArticleActionsWithLegacyLang",
				"Knowledge Article",
				"{number}/{lang}/{action}/{id}",
				new { controller = "Article", action = "Article", number = UrlParameter.Optional, lang = UrlParameter.Optional, id = Guid.Empty });
		}
	}
}
