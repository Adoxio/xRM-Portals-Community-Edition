/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumThread : IForumThread
	{
		private readonly Lazy<int> _postCount;
		private readonly string _url;
		private readonly IPortalViewEntity _viewEntity;
		
		public ForumThread(Entity entity, IPortalViewEntity viewEntity, IForumThreadInfo threadInfo, Func<int> postCount, string url = null)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");
			if (threadInfo == null) throw new ArgumentNullException("threadInfo");
			if (postCount == null) throw new ArgumentNullException("postCount");

			Entity = entity;
			_viewEntity = viewEntity;
			
			Author = threadInfo.Author;
			LatestPost = threadInfo.LatestPost;
			PostedOn = threadInfo.PostedOn;
			Tags = threadInfo.Tags;
			ThreadType = threadInfo.ThreadType;

			_postCount = new Lazy<int>(postCount);
			_url = url;

			Name = entity.GetAttributeValue<string>("adx_name");
			IsAnswered = entity.GetAttributeValue<bool?>("adx_isanswered").GetValueOrDefault();
			IsSticky = entity.GetAttributeValue<bool?>("adx_sticky").GetValueOrDefault();
			Locked = entity.GetAttributeValue<bool?>("adx_locked").GetValueOrDefault();
		}

		public ForumThread(Entity entity, IPortalViewEntity viewEntity, IForumThreadInfo threadInfo, int postCount, string url = null)
			: this(entity, viewEntity, threadInfo, () => postCount, url) { }

		public IForumAuthor Author { get; private set; }

		public Entity Entity { get; set; }

		public bool IsAnswered { get; private set; }

		public bool IsSticky { get; private set; }

		public bool Locked { get; private set; }

		public IForumPostInfo LatestPost { get; private set; }

		public string LatestPostUrl { get; set; }

		public string Name { get; private set; }

		public int PostCount { get { return _postCount.Value; } }

		public int ReplyCount { get { return PostCount - 1; } }

		public DateTime PostedOn { get; private set; }

		public IEnumerable<IForumThreadTag> Tags { get; private set; }

		public IForumThreadType ThreadType { get; private set; }

		public string Description { get { return _viewEntity.Description; } }

		public bool Editable { get { return _viewEntity.Editable; } }

		public EntityReference EntityReference { get { return _viewEntity.EntityReference; } }

		public string Url { get { return _url ?? _viewEntity.Url; } }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName) { return _viewEntity.GetAttribute(attributeLogicalName); }
	}
}
