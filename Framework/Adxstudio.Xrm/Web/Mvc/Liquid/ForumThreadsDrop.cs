/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Forums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ForumThreadsDrop : PortalDrop
	{
		private Lazy<ForumThreadDrop[]> _threads;

		private IForumDataAdapter _adapter;

		public ForumThreadsDrop(IPortalLiquidContext portalLiquidContext,
									IDataAdapterDependencies dependencies,
									int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_lastpostdate", string sortDirection = "asc")
									: this(portalLiquidContext, dependencies, null, startRowIndex, pageSize, orderBy, sortDirection) { }

		public ForumThreadsDrop(IPortalLiquidContext portalLiquidContext, 
									IDataAdapterDependencies dependencies,
									IForum forum,
									int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_lastpostdate", string sortDirection = "asc") : base(portalLiquidContext)
		{
			if (forum != null)
			{
				Dependencies = dependencies;

				PortalLiquidContext = portalLiquidContext;

				SetParams(startRowIndex, pageSize, orderBy, sortDirection);

				Forum = forum;

				var forumAggregationDataAdapter = new ForumDataAdapter(forum.EntityReference,
					Dependencies, serviceContext => CreateThreadEntityQuery(serviceContext, Forum.EntityReference));

				_adapter = forumAggregationDataAdapter;

				_threads = new Lazy<ForumThreadDrop[]>(() => _adapter.SelectThreads(StartRowIndex, PageSize).Select(e => new ForumThreadDrop(this, Dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		
			}
			else
			{
				Dependencies = dependencies;

				PortalLiquidContext = portalLiquidContext;

				SetParams(startRowIndex, pageSize, orderBy, sortDirection);

				var forumAggregationDataAdapter = new ForumDataAdapter(Dependencies, serviceContext => CreateThreadEntityQuery(serviceContext));

				_adapter = forumAggregationDataAdapter;

				_threads = new Lazy<ForumThreadDrop[]>(() => _adapter.SelectThreads(StartRowIndex, PageSize).Select(e => new ForumThreadDrop(this, Dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		
			}
		}

		public void SetParams(int startRowIndex = 0, int pageSize = -1, string orderBy = "adx_lastpostdate", string sortDirection = "asc")
		{
			StartRowIndex = startRowIndex;  PageSize = pageSize; OrderByKey = orderBy; SortDirection = sortDirection;
		}

		internal IForum Forum { get; private set; }

		internal IPortalLiquidContext PortalLiquidContext { get; private set; }
		internal IDataAdapterDependencies Dependencies { get; private set; }

		public int StartRowIndex { get; private set; }
		public int PageSize { get; private set; }
		public string OrderByKey { get; private set; }
		public string SortDirection { get; private set; }

		public IEnumerable<ForumThreadDrop> All
		{
			get
			{
				return _threads.Value.AsEnumerable();
			}
		}

		protected IQueryable<Entity> CreateThreadEntityQuery(OrganizationServiceContext serviceContext, EntityReference forum = null)
		{
			IQueryable<Entity> query;

			if (forum != null)
			{
				query = string.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(SortDirection, "descending", StringComparison.InvariantCultureIgnoreCase)
							? from thread in serviceContext.CreateQuery("adx_communityforumthread")
							  where thread.GetAttributeValue<Guid>("adx_forumid") == forum.Id
							  orderby thread.GetAttributeValue<bool?>("adx_sticky") descending
							  orderby thread[OrderByKey] descending
							  select thread
							: from thread in serviceContext.CreateQuery("adx_communityforumthread")
							  where thread.GetAttributeValue<Guid>("adx_forumid") == forum.Id
							  orderby thread.GetAttributeValue<bool?>("adx_sticky")
							  orderby thread[OrderByKey] 
							  select thread;
			}
			else
			{
				query = string.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase)
					|| string.Equals(SortDirection, "descending", StringComparison.InvariantCultureIgnoreCase)
					? from thread in serviceContext.CreateQuery("adx_communityforumthread")
					  join forumentity in serviceContext.CreateQuery("adx_communityforum") on thread.GetAttributeValue<EntityReference>("adx_forumid").Id
						equals forumentity.GetAttributeValue<Guid>("adx_communityforumid")
						where forumentity.GetAttributeValue<EntityReference>("adx_websiteid") == Dependencies.GetWebsite()
						orderby thread.GetAttributeValue<bool?>("adx_sticky") descending
						orderby thread[OrderByKey] descending
						select thread
					: from thread in serviceContext.CreateQuery("adx_communityforumthread")
					  join forumentity in serviceContext.CreateQuery("adx_communityforum") on thread.GetAttributeValue<EntityReference>("adx_forumid").Id
						equals forumentity.GetAttributeValue<Guid>("adx_communityforumid")
						where forumentity.GetAttributeValue<EntityReference>("adx_websiteid") == Dependencies.GetWebsite()
						orderby thread.GetAttributeValue<bool?>("adx_sticky")
						orderby thread[OrderByKey]
						select thread;
			}
			
			return query;
		}
	}
}
