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
	public class ForumsDrop : PortalDrop
	{
		private readonly IForumAggregationDataAdapter _adapter;

		private readonly IForumThreadAggregationDataAdapter _aggregation;

		private Adxstudio.Xrm.Forums.IDataAdapterDependencies _dependencies;

		private readonly Lazy<ForumDrop[]> _forums;

		public ForumsDrop(IPortalLiquidContext portalLiquidContext, Adxstudio.Xrm.Forums.IDataAdapterDependencies dependencies)
			: base(portalLiquidContext)
		{
			if (dependencies == null) throw new ArgumentException("dependencies");

			_dependencies = dependencies;

			var forumDataAdapter = new WebsiteForumDataAdapter(dependencies);

			_adapter = forumDataAdapter;

			_aggregation = forumDataAdapter;

			_forums = new Lazy<ForumDrop[]>(() => _adapter.SelectForums().Select(e => new ForumDrop(this, e, dependencies)).ToArray(), LazyThreadSafetyMode.None);
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			Guid parsed;

			// If the method can be parsed as a Guid, look up the set by that.
			if (Guid.TryParse(method, out parsed))
			{
				var forumById = _adapter.Select(parsed);

				return forumById == null ? null : new ForumDrop(this, forumById, _dependencies);
			}

			var forumByName = _adapter.Select(method);

			return forumByName == null ? null : new ForumDrop(this, forumByName, _dependencies);
		}

		public ForumThreadsDrop Threads
		{
			get { return new ForumThreadsDrop(this, _dependencies); }
		}

		public IEnumerable<ForumDrop> All
		{
			get { return _forums.Value.AsEnumerable(); }
		}

		public int ThreadCount
		{
			get { return _aggregation.SelectThreadCount(); }
		}

		public int PostCount
		{
			get { return _aggregation.SelectPostCount(); }
		}
	}
}
