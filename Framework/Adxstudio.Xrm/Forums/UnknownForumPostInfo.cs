/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class UnknownForumPostInfo : IForumPostInfo
	{
		public IEnumerable<IForumPostAttachmentInfo> AttachmentInfo
		{
			get { return new IForumPostAttachmentInfo[] { }; }
		}

		public IForumAuthor Author
		{
			get { return null; }
		}

		public EntityReference EntityReference
		{
			get { return null; }
		}

		public DateTime PostedOn
		{
			get { return DateTime.MinValue.ToUniversalTime(); }
		}

		public EntityReference ThreadEntity
		{
			get { return null; }
		}
	}
}
