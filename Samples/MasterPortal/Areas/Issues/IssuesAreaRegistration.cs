/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc;

namespace Site.Areas.Issues
{
	public class IssuesAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Issues"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapSiteMarkerRoute(
				"IssueActions",
				"Issues",
				"issue/{action}/{id}",
				new { controller = "Issue", action = "Index", id = Guid.Empty });

			context.MapSiteMarkerRoute(
				"IssuesFilter",
				"Issues",
				"{issueForumPartialUrl}/filter/{filter}/{status}/{priority}",
				new { controller = "Issues", action = "Filter", filter = "open", status = "all", priority = "any" });
			
			context.MapSiteMarkerRoute(
				"Issues",
				"Issues",
				"{issueForumPartialUrl}/{issuePartialUrl}",
				new { controller = "Issues", action = "Issues", issueForumPartialUrl = UrlParameter.Optional, issuePartialUrl = UrlParameter.Optional });
		}
	}
}
