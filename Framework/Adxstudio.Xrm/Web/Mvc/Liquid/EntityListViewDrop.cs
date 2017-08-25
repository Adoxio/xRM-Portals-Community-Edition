/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityListViewDrop : PortalDrop
	{
		private readonly Lazy<EntityListViewColumnDrop[]> _columns;

		public EntityListViewDrop(IPortalLiquidContext portalLiquidContext, EntityView view)
			: base(portalLiquidContext)
		{
			if (view == null) throw new ArgumentNullException("view");

			View = view;

			_columns = new Lazy<EntityListViewColumnDrop[]>(() => View.Columns.Select(e => new EntityListViewColumnDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public IEnumerable<EntityListViewColumnDrop> Columns { get { return _columns.Value; } }

		public string EntityLogicalName { get { return View.EntityLogicalName; } }

		public string Id { get { return View.Id.ToString(); } }

		public int LanguageCode { get { return View.LanguageCode; } }

		public string Name { get { return View.Name; } }

		public string DisplayName { get { return View.DisplayName; } }

		public string PrimaryKeyLogicalName { get { return View.PrimaryKeyLogicalName; } }

		public string SortExpression { get { return View.SortExpression; } }

		internal EntityView View { get; private set; }

		internal Guid ViewId { get { return View.Id; } }
	}
}
