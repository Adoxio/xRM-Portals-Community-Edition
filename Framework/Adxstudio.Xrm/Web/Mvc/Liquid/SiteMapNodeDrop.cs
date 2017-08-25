/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using DotLiquid;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SiteMapNodeDrop : PortalUrlDrop, IEditableCollection
	{
		private readonly Lazy<SiteMapNodeDrop[]> _breadcrumbs;
		private readonly Lazy<SiteMapNodeDrop[]> _children;
		private readonly Lazy<SiteMapNodeDrop> _parent;

		public SiteMapNodeDrop(IPortalLiquidContext portalLiquidContext, SiteMapNode node) : base(portalLiquidContext)
		{
			if (node == null) throw new ArgumentNullException("node");

			Node = node;
			
			_breadcrumbs = new Lazy<SiteMapNodeDrop[]>(GetBreadcrumbs, LazyThreadSafetyMode.None);
			_children = new Lazy<SiteMapNodeDrop[]>(GetChildren, LazyThreadSafetyMode.None);
			_parent = new Lazy<SiteMapNodeDrop>(() => GetDrop(Node.ParentNode), LazyThreadSafetyMode.None);

			var entityNode = node as CrmSiteMapNode;

			Entity = (entityNode != null && entityNode.Entity != null)
				? new EntityDrop(this, entityNode.Entity)
				: null;
		}

		public IEnumerable<SiteMapNodeDrop> Breadcrumbs
		{
			get { return _breadcrumbs.Value; }
		}

		public IEnumerable<SiteMapNodeDrop> Children
		{
			get { return _children.Value.AsEnumerable(); }
		}

		public string Description
		{
			get { return Node.Description; }
		}

		public EntityDrop Entity { get; private set; }

		public SiteMapNodeDrop Parent
		{
			get { return _parent.Value; }
		}

		public string Title
		{
			get { return Node.Title; }
		}

		public override string Url
		{
			get { return Node.Url; }
		}

		protected SiteMapNode Node { get; private set; }

		public string GetEditable(Context context, string key, EditableOptions options)
		{
			return Entity == null ? null : Entity.GetEditable(context, key, options);
		}

		private SiteMapNodeDrop[] GetBreadcrumbs()
		{
			var breadcrumbs = new List<SiteMapNodeDrop>();

			var parent = Parent;

			while (parent != null)
			{
				breadcrumbs.Add(parent);

				parent = parent.Parent;
			}

			breadcrumbs.Reverse();

			return breadcrumbs.ToArray();
		}

		private SiteMapNodeDrop[] GetChildren()
		{
			return Node.ChildNodes.Cast<SiteMapNode>().Select(GetDrop).ToArray();
		}

		private SiteMapNodeDrop GetDrop(SiteMapNode node)
		{
			return node == null ? null : new SiteMapNodeDrop(this, node);
		}
	}
}
