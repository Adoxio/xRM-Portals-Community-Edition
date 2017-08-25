/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogArchiveMonth : IBlogArchiveMonth
	{
		public BlogArchiveMonth(DateTime month, int postCount, ApplicationPath applicationPath)
		{
			Month = month;
			PostCount = postCount;
			ApplicationPath = applicationPath;
		}

		public ApplicationPath ApplicationPath { get; private set; }

		public DateTime Month { get; private set; }

		public int PostCount { get; private set; }
	}
}
