/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogAggregationDataAdapter : IBlogDataAdapter
	{
		IEnumerable<IBlog> SelectBlogs();

		IEnumerable<IBlog> SelectBlogs(int startRowIndex, int maximumRows = -1);

		IBlog Select(Guid blogId);

		IBlog Select(string blogName);

		int SelectBlogCount();
	}
}
