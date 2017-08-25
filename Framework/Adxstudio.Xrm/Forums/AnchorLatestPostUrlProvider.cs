/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Forums
{
	internal class AnchorLatestPostUrlProvider : ForumPostUrlProvider, ILatestPostUrlProvider
	{
		public string GetLatestPostUrl(IForumThread forumThread, int forumThreadPostCount)
		{
			if (forumThread == null)
			{
				return null;
			}

			var latestPost = forumThread.LatestPost;

			if (latestPost == null || latestPost.EntityReference == null)
			{
				return forumThread.Url;
			}

			var latestPostId = latestPost.EntityReference.Id;

			return GetPostUrl(forumThread, latestPostId);
		}
	}
}
