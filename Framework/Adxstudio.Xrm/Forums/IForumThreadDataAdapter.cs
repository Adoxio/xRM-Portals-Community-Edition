/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumThreadDataAdapter : IForumPostAggregationDataAdapter
	{
		IForumThread Select();

		IForumPost SelectPost(Guid forumPostId);

		IForumPost SelectFirstPost();

		IForumPost SelectLatestPost();

		void CreateAlert(EntityReference user);

		IForumPost CreatePost(IForumPostSubmission forumPost, bool incrementForumThreadCount = false);

		void DeleteAlert(EntityReference user);

		void DeletePost(EntityReference forumPost);

		bool HasAlert(EntityReference user);

		void MarkAsAnswer(Guid forumPostId);

		void UnMarkAsAnswer(Guid forumPostId);

		void UpdatePost(IForumPostSubmission forumPost);
	}
}
