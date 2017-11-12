/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Web.UI;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public class CrmEntityDataSource : DataSourceControl
	{
		private DataSourceView _view;

		/// <summary>
		/// Gets or sets the entity data item to which this control will bind.
		/// </summary>
		public object DataItem { get; set; }

		protected override DataSourceView GetView(string viewName)
		{
			if (_view == null)
			{
				_view = new CrmEntityDataSourceView(this, viewName);
			}

			return _view;
		}

		protected class CrmEntityDataSourceView : DataSourceView
		{
			public CrmEntityDataSourceView(IDataSource owner, string viewName) : base(owner, viewName)
			{
				Owner = owner as CrmEntityDataSource;

				if (Owner == null)
				{
					throw new ArgumentException("Owner data source must be of type {0}.".FormatWith(typeof(CrmEntityDataSource).FullName));
				}
			}

			protected CrmEntityDataSource Owner { get; private set; }

			protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
			{
				var dataItem = Owner.DataItem;

				if (dataItem is CrmSiteMapNode)
				{
					return new[] { (dataItem as CrmSiteMapNode).Entity };
				}

				return new[] { dataItem };
			}
		}
	}
}


