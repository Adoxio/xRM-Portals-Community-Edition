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

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Represents a Microsoft Dynamics CRM web service to data-bound controls.
	/// </summary>
	[PersistChildren(false)]
	[Description("Represents a Microsoft Dynamics CRM web service to data-bound controls.")]
	[DefaultProperty("FetchXml")]
	[ParseChildren(true)]
	[ToolboxBitmap(typeof(ObjectDataSource))]
	[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmDataSource : AsyncDataSourceControl
	{
		/// <summary>
		/// Gets or sets the name of the data context used to perform service operations.
		/// </summary>
		[Category("Data")]
		public string CrmDataContextName { get; set; }

		public class QueryByAttributeParameters : IStateManager
		{
			private string _entityName;

			[DefaultValue(""), Description(""), Category("Data")]
			public string EntityName
			{
				get
				{
					return _entityName;
				}

				set
				{
					_entityName = value;
				}
			}

			private ListItemCollection _attributes;

			[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
			public ListItemCollection Attributes
			{
				get
				{
					if (_attributes == null)
					{
						_attributes = new ListItemCollection();

						if (_tracking)
						{
							(_attributes as IStateManager).TrackViewState();
						}
					}

					return _attributes;
				}
			}

			private ListItemCollection _values;

			[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
			public ListItemCollection Values
			{
				get
				{
					if (_values == null)
					{
						_values = new ListItemCollection();

						if (_tracking)
						{
							(_values as IStateManager).TrackViewState();
						}
					}

					return _values;
				}
			}

			private ListItemCollection _orders;

			[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
			public ListItemCollection Orders
			{
				get
				{
					if (_orders == null)
					{
						_orders = new ListItemCollection();

						if (_tracking)
						{
							(_orders as IStateManager).TrackViewState();
						}
					}

					return _orders;
				}
			}

			private ListItemCollection _columnSet;

			[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
			public ListItemCollection ColumnSet
			{
				get
				{
					if (_columnSet == null)
					{
						_columnSet = new ListItemCollection();

						if (_tracking)
						{
							(_columnSet as IStateManager).TrackViewState();
						}
					}

					return _columnSet;
				}
			}

			#region IStateManager Members

			private bool _tracking;

			bool IStateManager.IsTrackingViewState
			{
				get	{ return IsTrackingViewState; }
			}

			public bool IsTrackingViewState
			{
				get	{ return _tracking; }
			}

			void IStateManager.LoadViewState(object savedState)
			{
				LoadViewState(savedState);
			}

			protected virtual void LoadViewState(object savedState)
			{
				object[] state = savedState as object[];

				if (state == null)
				{
					return;
				}

				EntityName = state[0] as string;
				((IStateManager)Attributes).LoadViewState(state[1]);
				((IStateManager)Values).LoadViewState(state[2]);
				((IStateManager)Orders).LoadViewState(state[3]);
				((IStateManager)ColumnSet).LoadViewState(state[4]);
			}

			object IStateManager.SaveViewState()
			{
				return SaveViewState();
			}

			protected virtual object SaveViewState()
			{
				object[] state = new object[5];
				state[0] = EntityName;
				state[1] = ((IStateManager)Attributes).SaveViewState();
				state[2] = ((IStateManager)Values).SaveViewState();
				state[3] = ((IStateManager)Orders).SaveViewState();
				state[4] = ((IStateManager)ColumnSet).SaveViewState();

				return state;
			}

			void IStateManager.TrackViewState()
			{
				TrackViewState();
			}

			protected virtual void TrackViewState()
			{
				_tracking = true;
				((IStateManager)Attributes).TrackViewState();
				((IStateManager)Values).TrackViewState();
				((IStateManager)Orders).TrackViewState();
				((IStateManager)ColumnSet).TrackViewState();
			}

			#endregion
		}

		#region Select Members

		private string _fetchXml;

		/// <summary>
		/// Gets or sets the fetch XML query.
		/// </summary>
		[Editor("System.ComponentModel.Design.MultilineStringEditor,System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TypeConverter("System.ComponentModel.MultilineStringConverter,System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
		[Category("Data")]
		[Description("The Fetch XML query.")]
		[DefaultValue("")]
		public virtual string FetchXml
		{
			get { return _fetchXml; }

			set
			{
				if (value != null)
				{
					value = value.Trim();
				}

				if (_fetchXml != value)
				{
					_fetchXml = value;
				}
			}
		}

		/// <summary>
		/// Gets the QueryByAttribute parameters.
		/// </summary>
		[Category("Data")]
		[Description("The QueryByAttribute query.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue((string)null)]
		public QueryByAttributeParameters QueryByAttribute
		{
			get
			{
				return GetView().QueryByAttribute;
			}
		}

		[Category("Data"), Description("")]
		public event EventHandler<CrmDataSourceStatusEventArgs> Selected
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
		public event EventHandler<CrmDataSourceSelectingEventArgs> Selecting
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

		/// <summary>
		/// Gets the parameters collection that contains the parameters that are used when selecting data.
		/// </summary>
		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public ParameterCollection SelectParameters
		{
			get
			{
				return GetView().SelectParameters;
			}
		}

		/// <summary>
		/// Gets the parameters collection that contains the parameters that are used when querying data.
		/// </summary>
		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public ParameterCollection QueryParameters
		{
			get
			{
				return GetView().QueryParameters;
			}
		}

		private bool _encodeParametersEnabled = true;

		/// <summary>
		/// Gets or sets the option to HtmlEncode the parameter values when constructing the FetchXml as a means of input validation.
		/// </summary>
		public bool EncodeParametersEnabled
		{
			get { return _encodeParametersEnabled; }
			set { _encodeParametersEnabled = value; }
		}

		/// <summary>
		/// Gets or sets the type name of the static class type to emit. The DynamicEntityWrapper is converted to an object of this type.
		/// </summary>
		public string StaticEntityWrapperTypeName { get; set; }

		#endregion

		#region Delete Members

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="keys">The keys by which to find the entity. "ID" and "Name" must be specified.</param>
		/// <param name="oldValues">The entity properties before the update. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Delete(IDictionary keys, IDictionary oldValues)
		{
			return GetView().Delete(keys, oldValues);
		}

		#endregion

		#region Insert Members

		/// <summary>
		/// Creates a new entity.
		/// </summary>
		/// <param name="values">The entity properties. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <param name="entityName">The type of entity to create e.g. "contact"</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Insert(IDictionary values, string entityName)
		{
			return GetView().Insert(values, entityName);
		}

		#endregion
		
		#region Update Members

		/// <summary>
		/// Updates an entity.
		/// </summary>
		/// <param name="keys">The keys by which to find the entity to be updated. "ID" and "Name" must be specified.</param>
		/// <param name="values">The entity properties to update. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <param name="oldValues">The entity properties before the update.</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Update(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return GetView().Update(keys, values, oldValues);
		}
		
		#endregion

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
			if (CacheParameters.Dependencies.Count == 0)
			{
				CacheKeyDependency dependency = new CacheKeyDependency();
				dependency.Name = "@key";
				dependency.PropertyName = "EntityName";
				dependency.KeyFormat = "xrm:dependency:entity:@key";

				CacheParameters.Dependencies.Add(dependency);
			}

			if (CacheParameters.ItemDependencies.Count == 0)
			{
				CacheKeyDependency dependency = new CacheKeyDependency();
				dependency.Name = "@key";
				dependency.PropertyName = "Name";
				dependency.KeyFormat = "xrm:dependency:entity:@key";

				CacheParameters.ItemDependencies.Add(dependency);
			}

			return CacheParameters;
		}

		#endregion

		#region IDataSource Members

		private const string _defaultViewName = "DefaultView";
		private static readonly string[] _defaultViewNames = { _defaultViewName };
		private ICollection _viewNames;
		private CrmDataSourceView _view;

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
