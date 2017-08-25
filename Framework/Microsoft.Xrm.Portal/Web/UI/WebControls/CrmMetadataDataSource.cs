/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Drawing.Design;
using System.Security.Permissions;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Represents a Microsoft Dynamics CRM web service to data-bound controls.
	/// </summary>
	[PersistChildren(false)]
	[Description("Represents a Microsoft Dynamics CRM metadata web service to data-bound controls.")]
	[ParseChildren(true)]
	[ToolboxBitmap(typeof(ObjectDataSource))]
	[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmMetadataDataSource : DataSourceControl
	{
		/// <summary>
		/// Gets or sets the name of the data context to be used to connect to Microsoft Dynamics CRM.
		/// </summary>
		[Category("Data")]
		public string CrmDataContextName { get; set; }

		[Category("Data"), Description(""), DefaultValue((string)null)]
		public string SortExpression
		{
			get
			{
				return GetView().SortExpression;
			}

			set
			{
				GetView().SortExpression = value;
			}
		}

		[Category("Data"), Description(""), DefaultValue((string)null)]
		public string EntityName
		{
			get
			{
				return GetView().EntityName;
			}

			set
			{
				GetView().EntityName = value;
			}
		}

		[Category("Data"), Description(""), DefaultValue((string)null)]
		public string AttributeName
		{
			get
			{
				return GetView().AttributeName;
			}

			set
			{
				GetView().AttributeName = value;
			}
		}

		[Category("Data"), Description(""), DefaultValue(EntityFilters.All)]
		public EntityFilters MetadataFlags
		{
			get
			{
				return GetView().MetadataFlags;
			}

			set
			{
				GetView().MetadataFlags = value;
			}
		}

		[Category("Data"), Description(""), DefaultValue(EntityFilters.All)]
		public EntityFilters EntityFlags
		{
			get
			{
				return GetView().EntityFlags;
			}

			set
			{
				GetView().EntityFlags = value;
			}
		}

		[Category("Data"), Description("")]
		public event EventHandler<CrmMetadataDataSourceStatusEventArgs> Selected
		{
			add
			{
				GetView().Selected += value;
			}

			remove
			{
				GetView().Selected -= value;
			}
		}

		[Category("Data"), Description("")]
		public event EventHandler<CrmMetadataDataSourceSelectingEventArgs> Selecting
		{
			add
			{
				GetView().Selecting += value;
			}

			remove
			{
				GetView().Selecting -= value;
			}
		}

		public IEnumerable Select()
		{
			return Select(DataSourceSelectArguments.Empty);
		}

		public IEnumerable Select(DataSourceSelectArguments arguments)
		{
			return GetView().Select(arguments);
		}

		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public ParameterCollection SelectParameters
		{
			get
			{
				return GetView().SelectParameters;
			}
		}

		#region Cache Members

		private CacheParameters _cacheParameters;

		/// <summary>
		/// Gets the cache settings.
		/// </summary>
		public CacheParameters CacheParameters
		{
			get
			{
				if (_cacheParameters == null)
				{
					_cacheParameters = new CacheParameters();

					if (IsTrackingViewState)
					{
						((IStateManager)_cacheParameters).TrackViewState();
					}
				}

				return _cacheParameters;
			}
		}

		public virtual CacheParameters GetCacheParameters()
		{
			return CacheParameters;
		}

		#endregion

		#region IDataSource Members

		private const string _defaultViewName = "DefaultView";
		private static readonly string[] _defaultViewNames = { _defaultViewName };
		private ICollection _viewNames;
		private CrmMetadataDataSourceView _view;

		protected override DataSourceView GetView(string viewName)
		{
			if ((viewName == null) || ((viewName.Length != 0) && !string.Equals(viewName, _defaultViewName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("An invalid view was requested.", "viewName");
			}

			return GetView();
		}

		private CrmMetadataDataSourceView GetView()
		{
			if (_view == null)
			{
				_view = new CrmMetadataDataSourceView(this, _defaultViewName, Context);

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

		#endregion

		#region IStateManager Members

		protected override void LoadViewState(object savedState)
		{
			Pair state = (Pair)savedState;

			if (savedState == null)
			{
				base.LoadViewState(null);
			}
			else
			{
				base.LoadViewState(state.First);

				if (state.Second != null)
				{
					((IStateManager)GetView()).LoadViewState(state.Second);
				}
			}
		}

		protected override object SaveViewState()
		{
			Pair state = new Pair();
			state.First = base.SaveViewState();

			if (_view != null)
			{
				state.Second = ((IStateManager)_view).SaveViewState();
			}

			if ((state.First == null) && (state.Second == null))
			{
				return null;
			}

			return state;
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			((IStateManager)GetView()).TrackViewState();
		}

		#endregion
	}
}
