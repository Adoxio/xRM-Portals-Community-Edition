/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumInfo : IForumInfo
	{
		public ForumInfo(IForumPostInfo latestPost)
		{
			if (latestPost == null) throw new ArgumentNullException("latestPost");

			LatestPost = latestPost;
		}

		public IForumPostInfo LatestPost { get; private set; }
	}
}
