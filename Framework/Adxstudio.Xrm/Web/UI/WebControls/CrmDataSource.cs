/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using System.Security;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Represents a CRM web service to data-bound controls.
	/// </summary>
	[PersistChildren(false)]
	[Description("Represents a CRM web service to data-bound controls.")]
	[DefaultProperty("FetchXml")]
	[ParseChildren(true)]
	[ToolboxBitmap(typeof(ObjectDataSource))]
	[SecurityCritical]
	public class CrmDataSource : Microsoft.Xrm.Portal.Web.UI.WebControls.CrmDataSource
	{
		private const string _defaultViewName = "DefaultView";
		private static readonly string[] _defaultViewNames = { _defaultViewName };
		private ICollection _viewNames;
		private CrmDataSourceView _view;
		private bool? _isSingleSource;

		protected override DataSourceView GetView(string viewName)
		{
			if ((viewName == null) || ((viewName.Length != 0) && !string.Equals(viewName, _defaultViewName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("An invalid view was requested.", "viewName");
			}

			return GetView();
		}

		private CrmDataSourceView GetView()
		{
			if (_view == null)
			{
				_view = new CrmDataSourceView(this, _defaultViewName, Context);

				if (IsTrackingViewState)
				{
					((IStateManager)_view).TrackViewState();
				}
			}

			return _view;
		}

		protected override ICollection GetViewNames()
		{
			if (_viewNames == null)
			{
				_viewNames = _defaultViewNames;
			}

			return _viewNames;
		}

		/// <summary>
		/// Set to true to use RetrieveSingle call in CrmDataSourceView
		/// </summary>
		public bool IsSingleSource
		{
			get { return _isSingleSource != null && (bool)_isSingleSource; }
			set { _isSingleSource = value; }
		}
	}
}
