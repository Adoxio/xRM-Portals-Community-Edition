/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Microsoft.Xrm.Portal.Web;
using Site.MasterPages;

namespace Site.Areas.Products.MasterPages
{
	public partial class Products : PortalMasterPage
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
			Collections.DataSource = Children;
			Collections.DataBind();

			RelatedLinks.DataSource = Shortcuts;
			RelatedLinks.DataBind();
		}

		private static Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>> GetChildNodes()
		{
			var currentNode = SiteMap.CurrentNode;

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
	}
}
