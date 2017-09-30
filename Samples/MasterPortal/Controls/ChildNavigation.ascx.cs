/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;

namespace Site.Controls
{
	public partial class ChildNavigation : PortalUserControl
	{
		private readonly Lazy<Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>> _childNodes;

		public ChildNavigation()
		{
			_childNodes = new Lazy<Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>>(GetChildNodes, LazyThreadSafetyMode.None);
			ShowChildren = true;
			ShowShortcuts = true;
		}

		public string Exclude { get; set; }

		public bool ShowChildren { get; set; }

		public bool ShowDescriptions { get; set; }

		public bool ShowShortcuts { get; set; }

		protected IEnumerable<SiteMapNode> Children
		{
			get { return _childNodes.Value.Item1; }
		}

		protected IEnumerable<SiteMapNode> Shortcuts
		{
			get { return _childNodes.Value.Item2; }
		}

		private Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>> GetChildNodes()
		{
			var currentNode = SiteMap.CurrentNode;

			if (currentNode == null)
			{
				return new Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>(new SiteMapNode[] { }, new SiteMapNode[] { });
			}

			var excludeLogicalNames = string.IsNullOrEmpty(Exclude)
				? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
				: new HashSet<string>(Exclude.Split(',').Select(name => name.Trim()), StringComparer.InvariantCultureIgnoreCase);

			var shortcutNodes = new List<SiteMapNode>();
			var otherNodes = new List<SiteMapNode>();

			foreach (SiteMapNode childNode in currentNode.ChildNodes)
			{
				var entityNode = childNode as CrmSiteMapNode;

				if (entityNode != null && excludeLogicalNames.Any(entityNode.HasCrmEntityName))
				{
					continue;
				}

				if (entityNode != null && entityNode.HasCrmEntityName("adx_shortcut"))
				{
					shortcutNodes.Add(childNode);

					continue;
				}
				
				otherNodes.Add(childNode);
			}

			return new Tuple<IEnumerable<SiteMapNode>, IEnumerable<SiteMapNode>>(otherNodes, shortcutNodes);
		}

		protected object GetDescription(SiteMapNode node)
		{
			if (node == null)
			{
				return null;
			}

			var entityNode = node as CrmSiteMapNode;

			if (entityNode == null || entityNode.Entity == null)
			{
				return node.Description;
			}

			var entity = XrmContext.MergeClone(entityNode.Entity);

			switch (entityNode.Entity.LogicalName)
			{
			case "adx_communityforum":
				return Html.TextAttribute(XrmContext, entity, "adx_description");
			case "adx_shortcut":
				return Html.HtmlAttribute(XrmContext, entity, "adx_description");
			case "adx_blog":
			case "adx_event":
			case "adx_webfile":
			case "adx_webpage":
				return Html.HtmlAttribute(XrmContext, entity, "adx_summary");
			default:
				return node.Description;
			}
		}
	}
}
