/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumThreadInfo
	{
		IForumAuthor Author { get; }

		IForumPostInfo LatestPost { get; }

		string LatestPostUrl { get; }

		DateTime PostedOn { get; }

		IEnumerable<IForumThreadTag> Tags { get; }

		IForumThreadType ThreadType { get; }
	}

	internal class UnknownForumThreadInfo : IForumThreadInfo
	{
		public IForumAuthor Author
		{
			get { return null; }
		}

		public IForumPostInfo LatestPost
		{
			get { return null; }
		}

		public string LatestPostUrl
		{
			get { return null; }
		}

		public DateTime PostedOn
		{
			get { return DateTime.MinValue.ToUniversalTime(); }
		}

		public IEnumerable<IForumThreadTag> Tags
		{
			get { return new IForumThreadTag[] { }; }
		}

		public IForumThreadType ThreadType
		{
			get { return new UnknownForumThreadType(); }
		}
	}
}
