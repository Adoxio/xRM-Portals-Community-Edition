/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Tagging;

namespace Adxstudio.Xrm.Blogs
{
	internal class BlogPostTagInfo : ITagInfo
	{
		public BlogPostTagInfo(string name, int count)
		{
			Name = name;
			TaggedItemCount = count;
		}

		public string Name { get; private set; }

		public int TaggedItemCount { get; private set; }
	}
}
