/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Ideas;
using Adxstudio.Xrm.Blogs;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.PublicProfile.ViewModels
{
	public class ProfileViewModel
	{
		public Entity User { get; set; }

        public Entity Website { get; set; }

		public int IdeaCount { get; set; }

		public int BlogCount { get; set; }

		public int ForumPostCount { get; set; }

		public PaginatedList<IIdea> Ideas { get; set; }

		public PaginatedList<IBlogPost> BlogPosts { get; set; }

		public PaginatedList<IForumPost> ForumPosts { get; set; }

		public bool IsIdeasEnable { get; set; }

		public bool IsBlogPostsEnable { get; set; }

		public bool IsForumPostsEnable { get; set; }
	}
}
