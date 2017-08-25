/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Search;
using System.Security;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	[PersistChildren(false)]
	[Description("")]
	[ParseChildren(true)]
	[ToolboxBitmap(typeof(ObjectDataSource))]
	[SecurityCritical]
	public class SearchDataSource : DataSourceControl
	{
		private SearchDataSourceInfoView _infoView;
		private ParameterCollection _selectParameters;
		private SearchDataSourceView _view;

		[Description(""), Category("Data"), DefaultValue((string)null)]
		public string LogicalNames
		{
			get { return ViewState["LogicalNames"] as string; }
			set { ViewState["LogicalNames"] = value; }
		}

		[Description(""), Category("Data"), DefaultValue((string)null)]
		public string Filter
		{
			get { return ViewState["Filter"] as string; }
			set { ViewState["Filter"] = value; }
		}

		[Description(""), Category("Data"), DefaultValue((string)null)]
		public string Query
		{
			get { return ViewState["Query"] as string; }
			set { ViewState["Query"] = value; }
		}

		[Description(""), Category("Data"), DefaultValue((string)null)]
		public string SearchProvider
		{
			get { return ViewState["SearchProvider"] as string; }
			set { ViewState["SearchProvider"] = value; }
		}

		private static readonly object _selected = new object();

		/// <summary>
		/// Occurs after the source data is queried.
		/// </summary>
		[Category("Data"), Description("")]
		public event EventHandler<SearchDataSourceStatusEventArgs> Selected
		{
			add { Events.AddHandler(_selected, value); }
			remove { Events.RemoveHandler(_selected, value); }
		}

		public virtual void OnSelected(SearchDataSourceStatusEventArgs e)
		{
			var handler = Events[_selected] as EventHandler<SearchDataSourceStatusEventArgs>;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private static readonly object _selecting = new object();

		/// <summary>
		/// Occurs before the source data is queried.
		/// </summary>
		[Category("Data"), Description("")]
		public event EventHandler<SearchDataSourceSelectingEventArgs> Selecting
		{
			add { Events.AddHandler(_selecting, value); }
			remove { Events.RemoveHandler(_selecting, value); }
		}

		public virtual void OnSelecting(SearchDataSourceSelectingEventArgs args)
		{
			var handler = Events[_selecting] as EventHandler<SearchDataSourceSelectingEventArgs>;

			if (handler != null)
			{
				handler(this, args);
			}
		}

		/// <summary>
		/// Gets the parameters collection that contains the parameters that are used when selecting data.
		/// </summary>
		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public ParameterCollection SelectParameters
		{
			get
			{
				if (_selectParameters == null)
				{
					_selectParameters = new ParameterCollection();

					if (IsTrackingViewState)
					{
						((IStateManager)_selectParameters).TrackViewState();
					}
				}

				return _selectParameters;
			}
		}

		protected override DataSourceView GetView(string viewName)
		{
			if (string.Equals(viewName, "count", StringComparison.InvariantCultureIgnoreCase) || string.Equals(viewName, "info", StringComparison.InvariantCultureIgnoreCase))
			{
				if (_infoView == null)
				{
					_infoView = new SearchDataSourceInfoView(this, viewName);
				}

				return _infoView;
			}

			if (_view == null)
			{
				_view = new SearchDataSourceView(this, viewName);
			}

			return _view;
		}
	}

	[SecurityCritical]
	public class SearchDataSourceSelectingEventArgs : CancelEventArgs
	{
		public SearchDataSourceSelectingEventArgs(SearchProvider provider, ICrmEntityQuery query)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			if (query == null)
			{
				throw new ArgumentNullException("query");
			}

			Provider = provider;
			Query = query;
		}

		public SearchProvider Provider { get; private set; }

		public ICrmEntityQuery Query { get; private set; }
	}

	[SecurityCritical]
	public class SearchDataSourceStatusEventArgs : EventArgs
	{
		public SearchDataSourceStatusEventArgs(SearchProvider provider, ICrmEntityQuery query, ICrmEntitySearchResultPage results)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			if (query == null)
			{
				throw new ArgumentNullException("query");
			}

			if (results == null)
			{
				throw new ArgumentNullException("results");
			}

			Provider = provider;
			Query = query;
			Results = results;
		}

		public Exception Exception { get; set; }

		public bool ExceptionHandled { get; set; }

		public SearchProvider Provider { get; private set; }

		public ICrmEntityQuery Query { get; private set; }

		public ICrmEntitySearchResultPage Results { get; private set; }
	}
}
