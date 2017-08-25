/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogPostWeightedTag : IBlogPostTag
	{
		int PostCount { get; }

		int Weight { get; }
	}
}
