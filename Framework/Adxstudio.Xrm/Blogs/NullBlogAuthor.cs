/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	internal class NullBlogAuthor : IBlogAuthor
	{
		public ApplicationPath ApplicationPath
		{
			get { return null; }
		}

		public string EmailAddress
		{
			get { return null; }
		}

		public Guid Id
		{
			get { return Guid.Empty; }
		}

		public string Name
		{
			get { return null; }
		}
	}
}
