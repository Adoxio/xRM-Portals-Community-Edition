/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumCounterStrategy
	{
		ForumCounts GetForumCounts(OrganizationServiceContext serviceContext, Entity forum);

		IDictionary<Guid, ForumCounts> GetForumCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forums);

		int GetForumThreadPostCount(OrganizationServiceContext serviceContext, Entity forumThread);

		IDictionary<Guid, int> GetForumThreadPostCounts(OrganizationServiceContext serviceContext, IEnumerable<Entity> forumThreads);
	}
}
