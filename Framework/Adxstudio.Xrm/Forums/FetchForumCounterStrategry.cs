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
	public class FetchForumCounterStrategry : IForumCounterStrategy
	{
		public ForumCounts GetForumCounts(OrganizationServiceContext serviceContext, Entity forum)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (forum == null) throw new ArgumentNullException("forum");

			return serviceContext.FetchForumCounts(forum.Id);
		}

		public IDictionary<Guid, ForumCounts> GetForumCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forums)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (forums == null) throw new ArgumentNullException("forums");

			return serviceContext.FetchForumCounts(forums.Select(e => e.Id));
		}

		public int GetForumThreadPostCount(OrganizationServiceContext serviceContext, Entity forumThread)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (forumThread == null) throw new ArgumentNullException("forumThread");

			return serviceContext.FetchForumThreadPostCount(forumThread.Id);
		}

		public IDictionary<Guid, int> GetForumThreadPostCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forumThreads)
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");
			if (forumThreads == null) throw new ArgumentNullException("forumThreads");

			return serviceContext.FetchForumThreadPostCounts(forumThreads.Select(e => e.Id));
		}
	}
}
