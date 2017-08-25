/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ForumThreadDrop : PortalViewEntityDrop
	{
		private readonly IForumThreadDataAdapter _adapter;
		private readonly IDataAdapterDependencies _dependencies;
		private readonly Lazy<ForumPostDrop> _firstPost;
		private readonly Lazy<ForumPostDrop> _latestPost;
		
		public ForumThreadDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, IForumThread thread)
			: base(portalLiquidContext, thread)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			_dependencies = dependencies;

			Thread = thread;
			Author = thread.Author != null ? new AuthorDrop(portalLiquidContext, Thread.Author) : null;
			
			_adapter = new ForumThreadDataAdapter(thread.EntityReference, dependencies);

			_firstPost = new Lazy<ForumPostDrop>(() => new ForumPostDrop(this, _dependencies, _adapter.SelectFirstPost()), LazyThreadSafetyMode.None);
			_latestPost = new Lazy<ForumPostDrop>(() => new ForumPostDrop(this, _dependencies, _adapter.SelectLatestPost()), LazyThreadSafetyMode.None);
		}

		protected IForumThread Thread { get; private set; }

		public AuthorDrop Author { get; private set; }

		public ForumPostsDrop Posts { get { return new ForumPostsDrop(this, _dependencies, Thread); } }

		public ForumPostDrop LatestPost { get { return _latestPost.Value; } }

		public ForumPostDrop FirstPost { get { return _firstPost.Value; } }

		public int PostCount { get { return Thread.PostCount; } }

		public int ReplyCount { get { return Thread.ReplyCount;  } }

		public bool IsAnswered { get { return Thread.IsAnswered; } }

		public bool IsSticky { get { return Thread.IsSticky; } }

		public bool Locked { get { return Thread.Locked;  } }

		public string Name { get { return Thread.Name; } }
	}
}
