/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums
{
	public interface ILatestPostUrlProvider
	{
		string GetLatestPostUrl(IForumThread forumThread, int forumThreadPostCount);
	}
}
