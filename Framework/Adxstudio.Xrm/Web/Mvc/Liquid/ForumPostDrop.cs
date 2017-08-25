/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ForumPostDrop : PortalViewEntityDrop
	{
		private readonly IDataAdapterDependencies _dependencies;
		private readonly Lazy<ForumThreadDrop> _thread;

		public ForumPostDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IForumPost post)
			: base(portalLiquidContext, post)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			_dependencies = dependencies;

			Post = post;
			Author = post.Author != null ? new AuthorDrop(portalLiquidContext, Post.Author) : null;

			_thread = new Lazy<ForumThreadDrop>(() => new ForumThreadDrop(this, _dependencies, Post.Thread), LazyThreadSafetyMode.None);
		}

		public AuthorDrop Author { get; private set; }

		public string Content { get { return Post.Content;  } }

		public bool CanEdit { get { return Post.CanEdit;  } }

		public bool CanMarkAsAnswer { get { return Post.CanMarkAsAnswer; } }

		public int HelpfulVoteCount { get { return Post.HelpfulVoteCount; } }

		public bool IsAnswer { get { return Post.IsAnswer;  } }

		public string Name { get { return Post.Name;  } }

		public ForumThreadDrop Thread
		{
			get { return _thread.Value; }
		}

		protected IForumPost Post { get; private set; }
	}
}
