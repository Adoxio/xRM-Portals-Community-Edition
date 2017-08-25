/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumPostInfo
	{
		IEnumerable<IForumPostAttachmentInfo> AttachmentInfo { get; }

		IForumAuthor Author { get; }

		EntityReference EntityReference { get; }

		DateTime PostedOn { get; }

		EntityReference ThreadEntity { get; }
	}
}
