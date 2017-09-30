/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ForumPostsDrop : PortalDrop
	{
		private Lazy<ForumPostDrop[]> _posts;

		private IForumThreadDataAdapter _adapter;

		public ForumPostsDrop(IPortalLiquidContext portalLiquidContext,
									IDataAdapterDependencies dependencies,
									IForumThread forumThread,
									int startRowIndex = 0, int pageSize = -1) : base(portalLiquidContext)
		{
			Dependencies = dependencies;

			PortalLiquidContext = portalLiquidContext;

			SetParams(startRowIndex, pageSize);

			ForumThread = forumThread;

			var forumAggregationDataAdapter = new ForumThreadDataAdapter(forumThread.Entity.ToEntityReference(), Dependencies);

			_adapter = forumAggregationDataAdapter;

			_posts = new Lazy<ForumPostDrop[]>(() => _adapter.SelectPosts(StartRowIndex, PageSize).Select(e => new ForumPostDrop(this, Dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public void SetParams(int startRowIndex = 0, int pageSize = -1)
		{
			StartRowIndex = startRowIndex; PageSize = pageSize; 
		}

		internal IForumThread ForumThread { get; private set; }

		internal IPortalLiquidContext PortalLiquidContext { get; private set; }
		internal IDataAdapterDependencies Dependencies { get; private set; }

		public int StartRowIndex { get; private set; }
		public int PageSize { get; private set; }

		public IEnumerable<ForumPostDrop> All
		{
			get
			{
				return _posts.Value.AsEnumerable();
			}
		}
	}
}
