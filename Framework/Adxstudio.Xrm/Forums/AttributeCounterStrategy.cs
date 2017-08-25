/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	public class AttributeCounterStrategy : IForumCounterStrategy
	{
		private const string _forumPostCountAttributeLogicalName = "adx_postcount";
		private const string _forumThreadCountAttributeLogicalName = "adx_threadcount";
		private const string _threadPostCountAttributeLogicalName = "adx_postcount";

		public ForumCounts GetForumCounts(OrganizationServiceContext serviceContext, Entity forum)
		{
			if (forum == null) throw new ArgumentNullException("forum");

			return new ForumCounts(
				forum.GetAttributeValue<int?>(_forumThreadCountAttributeLogicalName).GetValueOrDefault(),
				forum.GetAttributeValue<int?>(_forumPostCountAttributeLogicalName).GetValueOrDefault());
		}

		public IDictionary<Guid, ForumCounts> GetForumCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forums)
		{
			if (forums == null) throw new ArgumentNullException("forums");

			return forums.ToDictionary(e => e.Id, e => GetForumCounts(serviceContext, e));
		}

		public int GetForumThreadPostCount(OrganizationServiceContext serviceContext, Entity forumThread)
		{
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			return forumThread.GetAttributeValue<int?>(_threadPostCountAttributeLogicalName).GetValueOrDefault();
		}

		public IDictionary<Guid, int> GetForumThreadPostCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forumThreads)
		{
			if (forumThreads == null) throw new ArgumentNullException("forumThreads");

			return forumThreads.ToDictionary(e => e.Id, e => GetForumThreadPostCount(serviceContext, e));
		}
	}

	public class AttributeWithFetchFallbackCounterStrategy : IForumCounterStrategy
	{
		private const string _forumPostCountAttributeLogicalName = "adx_postcount";
		private const string _forumThreadCountAttributeLogicalName = "adx_threadcount";
		private const string _threadPostCountAttributeLogicalName = "adx_postcount";

		private readonly IForumCounterStrategy _fetchCounterStrategy = new FetchForumCounterStrategry();

		public ForumCounts GetForumCounts(OrganizationServiceContext serviceContext, Entity forum)
		{
			if (forum == null) throw new ArgumentNullException("forum");

			return GetForumCountsFromAttributes(forum) ?? _fetchCounterStrategy.GetForumCounts(serviceContext, forum);
		}

		public IDictionary<Guid, ForumCounts> GetForumCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forums)
		{
			if (forums == null) throw new ArgumentNullException("forums");

			var fromAttributes = forums.Select(e => new Tuple<Entity, ForumCounts>(e, GetForumCountsFromAttributes(e))).ToArray();
			var fallbackEntities = fromAttributes.Where(e => e.Item2 == null).Select(e => e.Item1).ToArray();

			var counts = fromAttributes.ToDictionary(e => e.Item1.Id, e => e.Item2);

			foreach (var fetchCounts in _fetchCounterStrategy.GetForumCounts(serviceContext, fallbackEntities))
			{
				counts[fetchCounts.Key] = fetchCounts.Value;
			}

			return counts;
		}

		public int GetForumThreadPostCount(OrganizationServiceContext serviceContext, Entity forumThread)
		{
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			var postCount = GetForumThreadPostCountFromAttribute(forumThread);

			return postCount == null
				? _fetchCounterStrategy.GetForumThreadPostCount(serviceContext, forumThread)
				: postCount.Value;
		}

		public IDictionary<Guid, int> GetForumThreadPostCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forumThreads)
		{
			if (forumThreads == null) throw new ArgumentNullException("forumThreads");

			var fromAttributes = forumThreads.Select(e => new Tuple<Entity, int?>(e, GetForumThreadPostCountFromAttribute(e))).ToArray();
			var fallbackEntities = fromAttributes.Where(e => e.Item2 == null).Select(e => e.Item1).ToArray();

			var counts = fromAttributes.ToDictionary(e => e.Item1.Id, e => e.Item2.GetValueOrDefault());

			foreach (var fetchCounts in _fetchCounterStrategy.GetForumThreadPostCounts(serviceContext, fallbackEntities))
			{
				counts[fetchCounts.Key] = fetchCounts.Value;
			}

			return counts;
		}

		private static ForumCounts GetForumCountsFromAttributes(Entity forum)
		{
			if (forum == null) throw new ArgumentNullException("forum");

			var threadCount = forum.GetAttributeValue<int?>(_forumThreadCountAttributeLogicalName);
			var postCount = forum.GetAttributeValue<int?>(_forumPostCountAttributeLogicalName);

			return threadCount == null || postCount == null
				? null
				: new ForumCounts(threadCount.Value, postCount.Value);
		}

		private static int? GetForumThreadPostCountFromAttribute(Entity forumThread)
		{
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			return forumThread.GetAttributeValue<int?>(_threadPostCountAttributeLogicalName);
		}
	}
}
