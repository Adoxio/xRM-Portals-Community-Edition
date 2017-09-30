/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class SiteMarkerTarget : ISiteMarkerTarget
	{
		private readonly Lazy<IPortalViewEntity> _viewEntity;

		public SiteMarkerTarget(Entity entity, Lazy<IPortalViewEntity> viewEntity, ApplicationPath applicationPath)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (viewEntity == null) throw new ArgumentNullException("viewEntity");

			if (applicationPath != null)
			{
				Url = applicationPath.AbsolutePath;
			}

			Entity = entity;
			_viewEntity = viewEntity;

			object nameValue;

			Description = entity.Attributes.TryGetValue("adx_name", out nameValue) && nameValue != null
				? nameValue.ToString()
				: null;
		}

		public SiteMarkerTarget(Entity entity, IPortalViewEntity viewEntity, ApplicationPath applicationPath)
			: this(entity, new Lazy<IPortalViewEntity>(() => viewEntity, LazyThreadSafetyMode.None), applicationPath) { }

		public string Description { get; private set; }

		public bool Editable
		{
			get { return _viewEntity.Value != null && _viewEntity.Value.Editable; }
		}

		public EntityReference EntityReference
		{
			get { return Entity.ToEntityReference(); }
		}

		public Entity Entity { get; private set; }

		public string Url { get; private set; }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			return _viewEntity.Value == null ? null : _viewEntity.Value.GetAttribute(attributeLogicalName);
		}
	}
}
