/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc;

namespace Site.Areas.KnowledgeBase
{
	public class KnowledgeBaseAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "KnowledgeBase"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapSiteMarkerRoute(
				"KnowledgeBaseArticle",
				"Knowledge Base",
				"{number}",
				new { controller = "Article", action = "Index" });
		}
	}
}
