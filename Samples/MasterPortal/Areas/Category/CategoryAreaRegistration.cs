/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc;

namespace Site.Areas.KnowledgeManagement
{
	public class CategoryAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Category";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapSiteMarkerRoute(
				"Category",
				"Category",
				"{number}",
				new { controller = "Category", action = "Index" });
		}
	}
}
