/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityViewDrop : PortalDrop
	{
		private readonly Lazy<EntityDrop[]> _entities;

		public EntityViewDrop(IPortalLiquidContext portalLiquidContext, ViewConfiguration viewConfiguration, ViewDataAdapter viewDataAdapter, Lazy<EntityListViewDrop> view) : base(portalLiquidContext)
		{
			if (viewConfiguration == null) throw new ArgumentNullException("viewConfiguration");
			if (viewDataAdapter == null) throw new ArgumentNullException("viewDataAdapter");
			if (view == null) throw new ArgumentNullException("view");

			ViewConfiguration = viewConfiguration;
			ViewDataAdapter = viewDataAdapter;
			FetchResult = new Lazy<ViewDataAdapter.FetchResult>(viewDataAdapter.FetchEntities, LazyThreadSafetyMode.None);
			View = view;

			var languageCode = new Lazy<int>(() => LanguageCode, LazyThreadSafetyMode.None);

			_entities = new Lazy<EntityDrop[]>(() => FetchResult.Value.Records.Select(e => new EntityDrop(this, e, languageCode, view)).ToArray(), LazyThreadSafetyMode.None);
		}

		public IEnumerable<EntityListViewColumnDrop> Columns
		{
			get { return View.Value.Columns; }
		}

		public bool EntityPermissionDenied
		{
			get { return FetchResult.Value.EntityPermissionDenied; }
		}

		public string EntityLogicalName
		{
			get { return View.Value.EntityLogicalName; }
		}

		public int? FirstPage
		{
			get { return TotalPages > 0 ? 1 : (int?)null; }
		}

		public string Id
		{
			get { return View.Value.Id; }
		}

		public int LanguageCode
		{
			get { return View.Value.LanguageCode; }
		}

		public int? LastPage
		{
			get { return TotalPages > 0 ? TotalPages : (int?)null; }
		}

		public string Name
		{
			get { return View.Value.Name; }
		}

		public int? NextPage
		{
			get { return Page < TotalPages ? Page + 1 : (int?)null; }
		}

		public int Page
		{
			get { return ViewDataAdapter.Page; }
		}

		public IEnumerable<int> Pages
		{
			get { return Enumerable.Range(1, TotalPages); }
		}

		public int PageSize
		{
			get { return ViewConfiguration.PageSize; }
		}

		public int? PreviousPage
		{
			get { return Page > 1 ? Page - 1 : (int?)null; }
		}

		public string PrimaryKeyLogicalName
		{
			get { return View.Value.PrimaryKeyLogicalName; }
		}

		public IEnumerable<EntityDrop> Records
		{
			get { return _entities.Value; }
		}

		public string SortExpression
		{
			get { return View.Value.SortExpression; }
		}

		public int TotalPages
		{
			get { return (TotalRecords + PageSize - 1) / PageSize; }
		}

		public int TotalRecords
		{
			get { return FetchResult.Value.TotalRecordCount; }
		}

		protected Lazy<ViewDataAdapter.FetchResult> FetchResult { get; private set; }

		protected Lazy<EntityListViewDrop> View { get; private set; }

		protected ViewConfiguration ViewConfiguration { get; private set; }

		protected ViewDataAdapter ViewDataAdapter { get; private set; }
	}
}
