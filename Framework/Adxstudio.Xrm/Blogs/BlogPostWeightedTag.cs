/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogPostWeightedTag : BlogPostTag, IBlogPostWeightedTag
	{
		public BlogPostWeightedTag(string name, ApplicationPath applicationPath, int postCount, int weight) : base(name, applicationPath)
		{
			PostCount = postCount;
			Weight = weight;
		}

		public int PostCount { get; private set; }

		public int Weight { get; private set; }
	}
}
