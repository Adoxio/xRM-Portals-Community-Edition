/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumAnnouncement : IForumAnnouncement
	{
		private readonly IPortalViewEntity _viewEntity;

		public ForumAnnouncement(Entity entity, IPortalViewEntity viewEntity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");

			Entity = entity;
			_viewEntity = viewEntity;

			Name = entity.GetAttributeValue<string>("adx_name");
			Content = entity.GetAttributeValue<string>("adx_content");
			PostedOn = entity.GetAttributeValue<DateTime?>("adx_date");
		}

		public string Content { get; private set; }

		public Entity Entity { get; set; }

		public string Name { get; private set; }

		public DateTime? PostedOn { get; private set; }

		public string Description
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
