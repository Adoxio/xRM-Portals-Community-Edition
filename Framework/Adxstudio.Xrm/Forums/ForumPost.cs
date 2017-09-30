/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumPost : IForumPost
	{
		private readonly Lazy<bool> _canMarkAsAnswer;
		private readonly Lazy<bool> _canEdit;
		private readonly Lazy<bool> _editable;
		private readonly Lazy<ApplicationPath> _getDeletePath;
		private readonly Lazy<ApplicationPath> _getEditPath;
		private readonly IPortalViewEntity _viewEntity;
		private readonly string _url;

		public ForumPost(Entity entity, IPortalViewEntity viewEntity, IForumPostInfo postInfo,
			Lazy<ApplicationPath> getEditPath = null, Lazy<ApplicationPath> getDeletePath = null, 
			Lazy<bool> editable = null, Lazy<bool> canMarkAsAnswer = null, string url = null, IForumThread thread = null,
			Lazy<bool> canEdit = null)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");
			if (postInfo == null) throw new ArgumentNullException("postInfo");

			Entity = entity;
			_viewEntity = viewEntity;
			_editable = editable;
			AttachmentInfo = postInfo.AttachmentInfo ?? new IForumPostAttachmentInfo[] { };
			Author = postInfo.Author;
			_getEditPath = getEditPath ?? new Lazy<ApplicationPath>(() => null, LazyThreadSafetyMode.None);
			_getDeletePath = getDeletePath ?? new Lazy<ApplicationPath>(() => null, LazyThreadSafetyMode.None);
			_canMarkAsAnswer = canMarkAsAnswer ?? new Lazy<bool>(() => false, LazyThreadSafetyMode.None);
			_canEdit = canEdit ?? new Lazy<bool>(() => false, LazyThreadSafetyMode.None);
			_url = url;
			Thread = thread;

			Content = entity.GetAttributeValue<string>("adx_content");
			IsAnswer = entity.GetAttributeValue<bool?>("adx_isanswer").GetValueOrDefault();
			HelpfulVoteCount = entity.GetAttributeValue<int?>("adx_helpfulvotecount").GetValueOrDefault();
			Name = entity.GetAttributeValue<string>("adx_name");
			PostedOn = entity.GetAttributeValue<DateTime?>("adx_date").GetValueOrDefault(postInfo.PostedOn);
		}

		public IForumAuthor Author { get; private set; }

		public IEnumerable<IForumPostAttachmentInfo> AttachmentInfo { get; private set; }

		public string Content { get; private set; }

		public ApplicationPath DeletePath { get { return _getDeletePath.Value; } }

		public ApplicationPath EditPath { get { return _getEditPath.Value; } }

		public Entity Entity { get; private set; }

		public int HelpfulVoteCount { get; private set; }

		public bool IsAnswer { get; private set; }

		public string Name { get; private set; }

		public bool CanEdit { get { return _canEdit.Value; } }

		public bool CanMarkAsAnswer { get { return _canMarkAsAnswer.Value; } }

		public DateTime PostedOn { get; private set; }

		public EntityReference ThreadEntity { get; private set; }

		public IForumThread Thread { get; private set; }

		//string IForumPostInfo.Url { get; set; }

		public string Description { get { return _viewEntity.Description; } }

		public bool Editable { get { return _editable == null ? _viewEntity.Editable : _editable.Value; } }

		public EntityReference EntityReference { get { return _viewEntity.EntityReference; } }

		public string Url { get { return _url ?? _viewEntity.Url; } }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			return _viewEntity.GetAttribute(attributeLogicalName);
		}
	}
}
