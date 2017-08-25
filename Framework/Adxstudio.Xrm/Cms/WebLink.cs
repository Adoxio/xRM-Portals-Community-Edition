/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class WebLink : IWebLink
	{
		public WebLink(Entity entity, IPortalViewEntity viewEntity, ApplicationPath applicationPath, IEnumerable<IWebLink> childWebLinks = null)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (viewEntity == null)
			{
				throw new ArgumentNullException("viewEntity");
			}

			Entity = entity;

			Description = viewEntity.GetAttribute("adx_description");
			Name = viewEntity.GetAttribute("adx_name");

			if (applicationPath != null)
			{
				if (applicationPath.ExternalUrl != null)
				{
					Url = applicationPath.ExternalUrl;
					IsExternal = true;
				}
				else
				{
					Url = applicationPath.AbsolutePath;
					IsExternal = false;
				}
			}

			WebLinks = childWebLinks == null ? Enumerable.Empty<IWebLink>() : childWebLinks.ToArray();

			ImageAlternateText = entity.GetAttributeValue<string>("adx_imagealttext");
			ImageUrl = entity.GetAttributeValue<string>("adx_imageurl");
			ImageHeight = entity.GetAttributeValue<int?>("adx_imageheight");
			ImageWidth = entity.GetAttributeValue<int?>("adx_imagewidth");
			DisplayImageOnly = entity.GetAttributeValue<bool?>("adx_displayimageonly").GetValueOrDefault(false);
			Page = entity.GetAttributeValue<EntityReference>("adx_pageid");

			HasImage = !string.IsNullOrEmpty(ImageUrl);

			NoFollow = !entity.GetAttributeValue<bool?>("adx_robotsfollowlink").GetValueOrDefault(true);
			OpenInNewWindow = entity.GetAttributeValue<bool?>("adx_openinnewwindow").GetValueOrDefault(false);
			ToolTip = entity.GetAttributeValue<string>("adx_name");
			DisplayPageChildLinks = entity.GetAttributeValue<bool?>("adx_displaypagechildlinks").GetValueOrDefault(false);
		}

		public IPortalViewAttribute Description { get; private set; }

		public bool DisplayImageOnly { get; private set; }

		public bool DisplayPageChildLinks { get; private set; }

		public Entity Entity { get; private set; }

		public IPortalViewAttribute Name { get; private set; }

		public string ImageAlternateText { get; private set; }

		public int? ImageHeight { get; private set; }

		public string ImageUrl { get; private set; }

		public int? ImageWidth { get; private set; }

		public bool IsExternal { get; private set; }

		public bool HasImage { get; private set; }

		public bool NoFollow { get; private set; }

		public bool OpenInNewWindow { get; private set; }

		public EntityReference Page { get; private set; }

		public string ToolTip { get; private set; }

		public string Url { get; private set; }

		public IEnumerable<IWebLink> WebLinks { get; private set; }
	}
}
