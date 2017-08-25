/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Permits.Pages
{
	public partial class Permits : PortalPage
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			PermitsListView.DataSource = GetPermits();
			PermitsListView.DataBind();
		}

		private IEnumerable<SiteMapNode> GetPermits()
		{
			var currentNode = System.Web.SiteMap.CurrentNode;

			if (currentNode == null)
			{
				return new SiteMapNode[] { };
			}

			var nodes = currentNode.ChildNodes.Cast<SiteMapNode>().ToList();

			return nodes;
		}

		protected string GetThumbnailUrl(object webpageObject)
		{
			var webpageEntity = webpageObject as Entity;

			if (webpageEntity == null)
			{
				return null;
			}

			var imageReference = webpageEntity.GetAttributeValue<EntityReference>("adx_image");

			if (imageReference == null)
			{
				return null;
			}
		
			var webfile = ServiceContext.CreateQuery("adx_webfile").FirstOrDefault(file => file.GetAttributeValue<Guid>("adx_webfileid") == imageReference.Id);

			if (webfile == null)
			{
				return null;
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(webfile));

			return url.Path;
		}
	}
}
