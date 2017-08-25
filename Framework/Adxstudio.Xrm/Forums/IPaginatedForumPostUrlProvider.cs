/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Forums
{
	public interface IPaginatedForumPostUrlProvider
	{
		string GetPostUrl(IForumThread forumThread, int forumThreadPostCount, Guid forumPostId);
	}
}
