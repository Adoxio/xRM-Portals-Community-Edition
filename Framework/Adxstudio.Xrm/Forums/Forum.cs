/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class Forum : IForum
	{
		private readonly Lazy<ForumCounts> _counts;
		private readonly IPortalViewEntity _viewEntity;

		public Forum(Entity entity, IPortalViewEntity viewEntity, IForumInfo forumInfo, Func<ForumCounts> counts)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");
			if (forumInfo == null) throw new ArgumentNullException("forumInfo");
			if (counts == null) throw new ArgumentNullException("counts");

			Entity = entity;
			_viewEntity = viewEntity;

			LatestPost = forumInfo.LatestPost;

			_counts = new Lazy<ForumCounts>(counts);

			Name = entity.GetAttributeValue<string>("adx_name");
			Description = entity.GetAttributeValue<string>("adx_description");
		}

		public Forum(Entity entity, IPortalViewEntity viewEntity, IForumInfo forumInfo, ForumCounts counts)
			: this(entity, viewEntity, forumInfo, () => counts) { }

		public string Description { get; private set; }

		public Entity Entity { get; set; }

		public IForumPostInfo LatestPost { get; private set; }

		public string Name { get; private set; }

		public int PostCount
		{
			get { return _counts.Value.PostCount; }
		}

		public int ThreadCount
		{
			get { return _counts.Value.ThreadCount; }
		}

		string IPortalViewEntity.Description
		{
			get { return _viewEntity.Description; }
		}

		public bool Editable
		{
			get { return _viewEntity.Editable; }
		}

		public EntityReference EntityReference
		{
			get { return _viewEntity.EntityReference; }
		}

		public string Url
		{
			get { return _viewEntity.Url; }
		}

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			return _viewEntity.GetAttribute(attributeLogicalName);
		}
	}
}
