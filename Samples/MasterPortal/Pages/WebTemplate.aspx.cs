/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Site.Pages
{
	public partial class WebTemplate : PortalPage
	{
		protected void Page_Init(object sender, EventArgs args)
		{
			if (!System.Web.SiteMap.Enabled)
			{
				return;
			}

			var currentNode = System.Web.SiteMap.CurrentNode;
			if (currentNode == null)
			{
				return;
			}

			var templateIdString = currentNode["adx_webtemplateid"];
			if (string.IsNullOrEmpty(templateIdString))
			{
				return;
			}

			Guid templateId;
			if (!Guid.TryParse(templateIdString, out templateId))
			{
				return;
			}
			
			Liquid.Html = Html.WebTemplate(new EntityReference("adx_webtemplate", templateId));
		}
	}
}
