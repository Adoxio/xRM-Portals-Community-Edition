/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public interface IForumDataAdapter : IForumThreadAggregationDataAdapter
	{
		IForum Select();

		IEnumerable<IForumAnnouncement> SelectAnnouncements();

		IEnumerable<IForumThreadType> SelectThreadTypes();

		IEnumerable<ListItem> SelectThreadTypeListItems();

		IForumThread CreateThread(IForumThread forumThread, IForumPostSubmission forumPost);

		void DeleteThread(EntityReference forumThread);

		void UpdateLatestPost(IForumPost forumPost, bool incremementForumThreadCount = false);

		void UpdateLatestPost(EntityReference forumPost, bool incremementForumThreadCount = false);
	}
}
