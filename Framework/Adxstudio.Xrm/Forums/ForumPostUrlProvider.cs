/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Adxstudio.Xrm.Collections.Generic;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Forums
{
	public class ForumPostUrlProvider : IForumPostUrlProvider
	{
		public string GetPostUrl(IForumThread forumThread, Guid forumPostId)
		{
			return "{0}#post-{1}".FormatWith(forumThread.Url, forumPostId);
		}

	}
}
