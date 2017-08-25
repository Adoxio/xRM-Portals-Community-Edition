/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.EntityList;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.EntityList.Pages
{
	public partial class VideoDetail : PortalPage
	{
		protected Video Video { get; private set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			Guid id;

			if (!Guid.TryParse(Request.QueryString["id"] ?? string.Empty, out id))
			{
				return;
			}

			var dataAdapter = new EntityListVideoDataAdapter(
				new EntityReference("adx_video", id),
				new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext));

			Video = dataAdapter.SelectVideo();

			if (Video == null)
			{
				return;
			}

			VideoHead.Visible = true;
			VideoBreadcrumbs.Visible = true;
			VideoHeader.Visible = true;
			VideoContent.Visible = true;
			PageBreadcrumbs.Visible = false;
		}
	}
}
