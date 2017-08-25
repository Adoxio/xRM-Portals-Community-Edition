/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public interface IBlogPostTag
	{
		ApplicationPath ApplicationPath { get; }

		string Name { get; }
	}
}
