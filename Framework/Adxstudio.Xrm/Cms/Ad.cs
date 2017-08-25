/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Cms
{
	public class Ad : IAd
	{
		public Ad(Entity entity, Entity note)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			Entity = entity;

			Copy = entity.GetAttributeValue<string>("adx_copy");
			Name = entity.GetAttributeValue<string>("adx_name");
			Title = entity.GetAttributeValue<string>("adx_title");

			var imageUrl = entity.GetAttributeValue<string>("adx_image");
			if (string.IsNullOrEmpty(imageUrl) && note != null)
			{
				imageUrl = note.GetRewriteUrl();
			}

			if (!string.IsNullOrEmpty(imageUrl))
			{
				ImageUrl = System.Web.VirtualPathUtility.IsAppRelative(imageUrl)
					? System.Web.VirtualPathUtility.ToAbsolute(imageUrl)
					: imageUrl;
			}

			ImageAlternateText = entity.GetAttributeValue<string>("adx_imagealttext");
			ImageHeight = entity.GetAttributeValue<int?>("adx_imageheight");
			ImageWidth = entity.GetAttributeValue<int?>("adx_imagewidth");

			OpenInNewWindow = entity.GetAttributeValue<bool?>("adx_openinnewwindow").GetValueOrDefault(false);

			var redirectUrl = entity.GetAttributeValue<string>("adx_url");
			if (!string.IsNullOrEmpty(redirectUrl))
			{
				RedirectUrl = System.Web.VirtualPathUtility.IsAppRelative(redirectUrl)
					? System.Web.VirtualPathUtility.ToAbsolute(redirectUrl)
					: redirectUrl;
			}

			WebTemplate = entity.GetAttributeValue<EntityReference>("adx_webtemplateid");
		}

		[JsonIgnore]
		public Entity Entity { get; private set; }

		public string Copy { get; private set; }

		public string ImageAlternateText { get; private set; }

		public int? ImageHeight { get; private set; }

		public string ImageUrl { get; private set; }

		public int? ImageWidth { get; private set; }

		public string Name { get; private set; }

		public bool OpenInNewWindow { get; private set; }

		public string RedirectUrl { get; private set; }

		public string Title { get; private set; }

		[JsonIgnore]
		public EntityReference WebTemplate { get; private set; }
	}
}
