/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogPostTag : IBlogPostTag
	{
		public BlogPostTag(string name, ApplicationPath applicationPath)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value can't be null or whitespace.", "name");
			}

			Name = name;
			ApplicationPath = applicationPath;
		}

		public ApplicationPath ApplicationPath { get; private set; }

		public string Name { get; private set; }
	}
}
