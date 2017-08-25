/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumPostAggregationDataAdapter
	{
		int SelectPostCount();

		IEnumerable<IForumPost> SelectPosts();

		IEnumerable<IForumPost> SelectPosts(bool descending);

		IEnumerable<IForumPost> SelectPosts(int startRowIndex, int maximumRows = -1);

		IEnumerable<IForumPost> SelectPosts(bool descending, int startRowIndex, int maximumRows = -1);

		IEnumerable<IForumPost> SelectPostsDescending();

		IEnumerable<IForumPost> SelectPostsDescending(int startRowIndex, int maximumRows = -1);
	}
}
