/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogDataAdapter
	{
		IBlog Select();

		IEnumerable<IBlogPost> SelectPosts();

		IEnumerable<IBlogPost> SelectPosts(int startRowIndex, int maximumRows = -1);

		int SelectPostCount();

		IEnumerable<IBlogArchiveMonth> SelectArchiveMonths();

		IEnumerable<IBlogPostWeightedTag> SelectWeightedTags(int weights);
	}
}
