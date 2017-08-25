/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class ForumThreadSubmission : IForumThread
	{
		public ForumThreadSubmission(string name, DateTime postedOn, IForumAuthor author, IForumThreadType threadType)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value can't be null or empty.", "name");
			if (postedOn.Kind != DateTimeKind.Utc) throw new ArgumentException("Value must be UTC.", "postedOn");
			if (author == null) throw new ArgumentNullException("author");
			
			Name = name;
			PostedOn = postedOn;
			Author = author;
			ThreadType = threadType;
		}

		public string Description { get { return string.Empty; } }

		public bool Editable { get { return false; } }

		public EntityReference EntityReference { get { return null; } }

		public string Url { get { return null; } }

		public IForumAuthor Author { get; private set; }

		public IForumPostInfo LatestPost { get { return null; } }

		public string LatestPostUrl { get { return null; } }

		public DateTime PostedOn { get; private set; }

		public IEnumerable<IForumThreadTag> Tags { get; set; }

		public IForumThreadType ThreadType { get; private set; }

		public Entity Entity { get { return null; } }

		public bool IsAnswered { get; set; }

		public bool IsSticky { get; set; }

		public bool Locked { get; set; }

		public string Name { get; private set; }

		public int PostCount { get { return 0; } }

		public int ReplyCount { get { return 0; } }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			throw new NotSupportedException();
		}
	}
}
