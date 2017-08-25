/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using Adxstudio.Xrm.Web.Mvc;

namespace Site.Areas.PublicProfile
{
	public class PublicProfileAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "PublicProfile"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapSiteMarkerRoute(
				"PublicProfileBlogPosts",
				"Public Profile",
				"{contactId}/blog-posts",
				new { controller = "PublicProfile", action = "ProfileBlogPosts", contactId = Guid.Empty });

			context.MapSiteMarkerRoute(
				"PublicProfileIdeas",
				"Public Profile",
				"{contactId}/ideas",
				new { controller = "PublicProfile", action = "ProfileIdeas", contactId = Guid.Empty });

			context.MapSiteMarkerRoute(
				"PublicProfileForumPosts",
				"Public Profile",
				"{contactId}/forum-posts",
				new { controller = "PublicProfile", action = "ProfileForumPosts", contactId = Guid.Empty });

			context.MapSiteMarkerRoute(
				"RedirectToProfile",
				"Public Profile",
				"{contactId}",
				new { controller = "PublicProfile", action = "ProfileRedirect", contactId = Guid.Empty });
		}
	}
}
