/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Blogs;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class BlogPostDrop : EntityDrop
	{
		public BlogPostDrop(IPortalLiquidContext portalLiquidContext, IBlogPost post)
			: base(portalLiquidContext, post.Entity)
		{
			if (post == null) throw new ArgumentNullException("post");

			Post = post;
			Author = Post.Author != null ? new AuthorDrop(portalLiquidContext, Post.Author) : null;
		}

		public AuthorDrop Author { get; private set; }

		public int CommentCount
		{
			get { return Post.CommentCount; }
		}

		public string Content
		{
			get { return Post.Content.ToString(); }
		}

		public DateTime LastUpdatedTime
		{
			get { return Post.LastUpdatedTime; }
		}

		public DateTime PublishDate
		{
			get { return Post.PublishDate; }
		}

		public string Title
		{
			get { return Post.Title; }
		}

		public override string Url
		{
			get { return Post.ApplicationPath.AbsolutePath; }
		}
		
		protected IBlogPost Post { get; private set; }
	}
}
