/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Services.Query;

	/// <summary>
	/// Provides query access to all Forums (adx_communityforum) in a given Website (adx_website). Also provides
	/// query access to latest Forum Threads (adx_communityforumthread) across all Forums in that Website.
	/// </summary>
	public class WebsiteForumDataAdapter : ForumAggregationDataAdapter, IForumThreadAggregationDataAdapter
	{
		private readonly IForumThreadAggregationDataAdapter _helperDataAdapter;

		public WebsiteForumDataAdapter(IDataAdapterDependencies dependencies) : base(dependencies)
		{
			var website = dependencies.GetWebsite();

			_helperDataAdapter = new ForumThreadAggregationDataAdapter(
				dependencies,
				true,
				serviceContext => serviceContext.FetchForumCountsForWebsite(website.Id),
				serviceContext => CreateThreadEntityQuery(serviceContext, website),
				serviceContext => serviceContext.FetchForumThreadTagInfoForWebsite(website.Id),
				new ForumThreadAggregationDataAdapter.ForumThreadUrlProvider(dependencies.GetUrlProvider()));
		}

		public WebsiteForumDataAdapter(IDataAdapterDependencies dependencies, Func<OrganizationServiceContext, IQueryable<Entity>> selectThreadEntities)
			: base(dependencies)
		{
			var website = dependencies.GetWebsite();

			_helperDataAdapter = new ForumThreadAggregationDataAdapter(
				dependencies,
				true,
				serviceContext => serviceContext.FetchForumCountsForWebsite(website.Id),
				selectThreadEntities,
				serviceContext => serviceContext.FetchForumThreadTagInfoForWebsite(website.Id),
				new ForumThreadAggregationDataAdapter.ForumThreadUrlProvider(dependencies.GetUrlProvider()));
		}

		public int SelectPostCount()
		{
			return _helperDataAdapter.SelectPostCount();
		}

		public int SelectThreadCount()
		{
			return _helperDataAdapter.SelectThreadCount();
		}

		public IEnumerable<IForumThread> SelectThreads()
		{
			return _helperDataAdapter.SelectThreads();
		}

		public IEnumerable<IForumThread> SelectThreads(int startRowIndex, int maximumRows = -1)
		{
			return _helperDataAdapter.SelectThreads(startRowIndex, maximumRows);
		}

		public IEnumerable<IForumThreadWeightedTag> SelectWeightedTags(int weights)
		{
			return _helperDataAdapter.SelectWeightedTags(weights);
		}

		protected override Filter GetWhereExpression()
		{
			var website = Dependencies.GetWebsite();

			var filter = new Filter
			{
				Conditions = new[]
				{
					new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
					new Condition("statecode", ConditionOperator.Equal, 0)
				}
			};
			return filter;
		}

		private static IQueryable<Entity> CreateThreadEntityQuery(OrganizationServiceContext serviceContext, EntityReference website)
		{
			return from thread in serviceContext.CreateQuery("adx_communityforumthread")
				join forum in serviceContext.CreateQuery("adx_communityforum") on thread.GetAttributeValue<Guid>("adx_forumid") equals forum.GetAttributeValue<Guid>("adx_communityforumid")
				where forum.GetAttributeValue<EntityReference>("adx_websiteid") == website
					&& forum.GetAttributeValue<int?>("statecode") == 0
				where thread.GetAttributeValue<int?>("statecode") == 0
				orderby thread.GetAttributeValue<DateTime?>("adx_lastpostdate") descending
				select thread;
		}
	}
}
