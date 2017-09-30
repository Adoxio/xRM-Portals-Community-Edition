/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;

namespace Site.Pages
{
	public partial class CategoryTopicPage : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e) { }

		protected bool IsCurrentNode(object dataItem)
		{
			if (dataItem == null)
			{
				return false;
			}

			var node = dataItem as SiteMapNode;

			if (node == null)
			{
				return false;
			}

			if (!System.Web.SiteMap.Enabled)
			{
				return false;
			}

			var currentNode = System.Web.SiteMap.CurrentNode;

			if (currentNode == null)
			{
				return false;
			}

			return string.Equals(node.Key, currentNode.Key, StringComparison.InvariantCulture);
		}
	}
}
