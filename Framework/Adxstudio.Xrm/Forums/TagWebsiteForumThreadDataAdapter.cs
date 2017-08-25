/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	public class TagWebsiteForumThreadDataAdapter : IForumThreadAggregationDataAdapter
	{
		private readonly IForumThreadAggregationDataAdapter _helperDataAdapter;

		public TagWebsiteForumThreadDataAdapter(string tag, IDataAdapterDependencies dependencies)
		{
			if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Value can't be null or whitespace.", "tag");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;

			var website = dependencies.GetWebsite();

			_helperDataAdapter = new ForumThreadAggregationDataAdapter(
				dependencies,
				true,
				serviceContext => serviceContext.FetchForumCountsForWebsiteWithTag(website.Id, tag),
				serviceContext => CreateThreadEntityQuery(serviceContext, website, tag),
				serviceContext => serviceContext.FetchForumThreadTagInfoForWebsite(website.Id),
				new ForumThreadAggregationDataAdapter.ForumThreadUrlProvider(dependencies.GetUrlProvider()));
		}

		protected IDataAdapterDependencies Dependencies { get; set; }

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

		private static IQueryable<Entity> CreateThreadEntityQuery(OrganizationServiceContext serviceContext, EntityReference website, string tagName)
		{
			return from thread in serviceContext.CreateQuery("adx_communityforumthread")
				join forum in serviceContext.CreateQuery("adx_communityforum") on thread.GetAttributeValue<Guid>("adx_forumid") equals forum.GetAttributeValue<Guid>("adx_communityforumid")
				join threadTagging in serviceContext.CreateQuery("adx_communityforumthread_tag") on thread.GetAttributeValue<Guid>("adx_communityforumthreadid") equals threadTagging.GetAttributeValue<Guid>("adx_communityforumthreadid")
				join tag in serviceContext.CreateQuery("adx_tag") on threadTagging.GetAttributeValue<Guid>("adx_tagid") equals tag.GetAttributeValue<Guid>("adx_tagid")
				where tag.GetAttributeValue<string>("adx_name") == tagName
				where forum.GetAttributeValue<EntityReference>("adx_websiteid") == website
				orderby thread.GetAttributeValue<DateTime?>("adx_lastpostdate") descending
				select thread;
		}
	}
}
