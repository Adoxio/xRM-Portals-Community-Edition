/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Products.Pages
{
	public partial class ProductCollections : PortalPage
	{
		private readonly Lazy<Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>> _childNodes = new Lazy<Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>>(GetChildNodes, LazyThreadSafetyMode.None);

		protected IEnumerable<SiteMapNode> Children
		{
			get { return _childNodes.Value.Item1; }
		}

		protected IEnumerable<SiteMapNode> Shortcuts
		{
			get { return _childNodes.Value.Item2; }
		}

		protected void Page_Load(object sender, EventArgs args)
		{
			ChildView.DataSource = Children;
			ChildView.DataBind();
		}

		private static Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>> GetChildNodes()
		{
			var currentNode = System.Web.SiteMap.CurrentNode;

			if (currentNode == null)
			{
				return new Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>(new SiteMapNode[] { }, new SiteMapNode[] { });
			}

			var shortcutNodes = new List<SiteMapNode>();
			var otherNodes = new List<SiteMapNode>();

			foreach (SiteMapNode childNode in currentNode.ChildNodes)
			{
				var entityNode = childNode as CrmSiteMapNode;

				if (entityNode != null && entityNode.HasCrmEntityName("adx_shortcut"))
				{
					shortcutNodes.Add(childNode);
				}
				else
				{
					otherNodes.Add(childNode);
				}
			}

			return new Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>(otherNodes, shortcutNodes);
		}

		protected string BuildImageTag(Entity entity)
		{
			const string defaultUrl = "/image-not-available-150x150.png/";
			var url = string.Empty;

			if (entity == null || string.IsNullOrWhiteSpace(entity.LogicalName))
			{
				return string.Empty;
			}

			if (entity.LogicalName == "adx_webpage")
			{
				var webFileReference = entity.GetAttributeValue<EntityReference>("adx_image");
				if (webFileReference == null)
				{
					var thumbnailURL = entity.GetAttributeValue<string>("adx_imageurl");
					url =  string.IsNullOrWhiteSpace(thumbnailURL) ? string.Empty : thumbnailURL;
				}
				else
				{
					var webFile = ServiceContext.CreateQuery("adx_webfile").FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webfileid") == webFileReference.Id);
					if (webFile != null)
					{
						url = ServiceContext.GetUrl(webFile);
					}
				}

				if (string.IsNullOrWhiteSpace(url))
				{
					url = defaultUrl;
				}
			}

			return string.IsNullOrWhiteSpace(url) ? string.Empty : string.Format("<img alt='' src='{0}' runat='server' />", url);
		}
	}
}
