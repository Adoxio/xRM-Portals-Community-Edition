/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ParameterCollection = System.Web.UI.WebControls.ParameterCollection;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Supports the <see cref="CrmMetadataDataSource"/> control and provides an interface for data-bound controls to retrieve data from the <see cref="CrmService"/>.
	/// </summary>
	public class CrmMetadataDataSourceView : DataSourceView, IStateManager
	{
		private static readonly string _entityNameParameterName = "EntityName";
		private static readonly string _attributeNameParameterName = "AttributeName";
		private static readonly string _metadataFlagsParameterName = "MetadataFlags";
		private static readonly string _entityFlagsParameterName = "EntityFlags";
		private static readonly string _sortExpressionParameterName = "SortExpression";

		private readonly CrmMetadataDataSource _owner;

		protected CrmMetadataDataSource Owner
		{
			get { return _owner; }
		}

		private readonly HttpContext _context;

		protected HttpContext Context
		{
			get { return _context; }
		}
		
		public CrmMetadataDataSourceView(CrmMetadataDataSource owner, string name, HttpContext context)
			: base(owner, name)
		{
			_cancelSelectOnNullParameter = false;
			_owner = owner;
			_context = context;
		}

		private string  _sortExpression;

		public string SortExpression
		{
			get { return _sortExpression; }
			set { _sortExpression = value; }
		}

		private EntityFilters _metadataFlags;

		public EntityFilters MetadataFlags
		{
			get { return _metadataFlags; }
			set { _metadataFlags = value; }
		}

		private EntityFilters _entityFlags;

		public EntityFilters EntityFlags
		{
			get { return _entityFlags; }
			set { _entityFlags = value; }
		}

		private string _entityName;

		public string EntityName
		{
			get { return _entityName; }
			set { _entityName = value; }
		}

		private string  _attributeName;

		public string  AttributeName
		{
			get { return _attributeName; }
			set { _attributeName = value; }
		}

		private bool _cancelSelectOnNullParameter;

		public bool CancelSelectOnNullParameter
		{
			get { return _cancelSelectOnNullParameter; }
			set
			{
				if (CancelSelectOnNullParameter != value)
				{
					_cancelSelectOnNullParameter = value;
					OnDataSourceViewChanged(EventArgs.Empty);
				}
			}
		}

		private static readonly object EventSelected = new object();

		public event EventHandler<CrmMetadataDataSourceStatusEventArgs> Selected
		{
			add { base.Events.AddHandler(EventSelected, value); }
			remove { base.Events.RemoveHandler(EventSelected, value); }
		}

		protected virtual void OnSelected(CrmMetadataDataSourceStatusEventArgs e)
		{
			EventHandler<CrmMetadataDataSourceStatusEventArgs> handler = base.Events[EventSelected] as EventHandler<CrmMetadataDataSourceStatusEventArgs>;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private static readonly object EventSelecting = new object();

		public event EventHandler<CrmMetadataDataSourceSelectingEventArgs> Selecting
		{
			add { base.Events.AddHandler(EventSelecting, value); }
			remove { base.Events.RemoveHandler(EventSelecting, value); }
		}

		protected virtual void OnSelecting(CrmMetadataDataSourceSelectingEventArgs args)
		{
			EventHandler<CrmMetadataDataSourceSelectingEventArgs> handler = base.Events[EventSelecting] as EventHandler<CrmMetadataDataSourceSelectingEventArgs>;

			if (handler != null)
			{
				handler(this, args);
			}
		}

		private ParameterCollection _selectParameters;

		public ParameterCollection SelectParameters
		{
			get
			{
				if (_selectParameters == null)
				{
					_selectParameters = new ParameterCollection();
					_selectParameters.ParametersChanged += SelectParametersChangedEventHandler;

					if (_tracking)
					{
						((IStateManager)_selectParameters).TrackViewState();
					}
				}

				return _selectParameters;
			}
		}

		private void SelectParametersChangedEventHandler(object sender, EventArgs args)
		{
			OnDataSourceViewChanged(args);
		}

		public IEnumerable Select(DataSourceSelectArguments arguments)
		{
			return ExecuteSelect(arguments);
		}

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "Begin");

			if (CanSort)
			{
				arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
			}

			if (CanPage)
			{
				arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
			}

			if (CanRetrieveTotalRowCount)
			{
				arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
			}

			// merge SelectParameters
			IOrderedDictionary parameters = SelectParameters.GetValues(Context, Owner);

			string sortExpression = GetNonNullOrEmpty(
				arguments.SortExpression,
				parameters[_sortExpressionParameterName] as string,
				SortExpression);
			string entityName = GetNonNullOrEmpty(
				parameters[_entityNameParameterName] as string,
				EntityName);
			string attributeName = GetNonNullOrEmpty(
				parameters[_attributeNameParameterName] as string,
				AttributeName);

			var metadataFlags = MetadataFlags;

			if (parameters.Contains(_metadataFlagsParameterName))
			{
				metadataFlags = (parameters[_metadataFlagsParameterName] as string).ToEnum<EntityFilters>();
			}

			var entityFlags = EntityFlags;

			if (parameters.Contains(_entityFlagsParameterName))
			{
				entityFlags = (parameters[_entityFlagsParameterName] as string).ToEnum<EntityFilters>();
			}

			// raise pre-event
			CrmMetadataDataSourceSelectingEventArgs selectingArgs = new CrmMetadataDataSourceSelectingEventArgs(
				Owner,
				arguments,
				entityName,
				attributeName,
				metadataFlags,
				entityFlags,
				sortExpression);
			OnSelecting(selectingArgs);

			if (selectingArgs.Cancel)
			{
				Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "Cancel");
				return null;
			}

			// merge event results
			arguments.RaiseUnsupportedCapabilitiesError(this);
			sortExpression = selectingArgs.SortExpression;
			entityName = selectingArgs.EntityName;
			attributeName = selectingArgs.AttributeName;
			metadataFlags = selectingArgs.MetadataFlags;
			entityFlags = selectingArgs.EntityFlags;

			if (CancelSelectOnNullParameter && string.IsNullOrEmpty(entityName) && string.IsNullOrEmpty(attributeName))
			{
				Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "CancelSelectOnNullParameter");
				return null;
			}

			IEnumerable result = null;
			int rowsAffected = 0;

			try
			{
				if (Owner.CacheParameters.Enabled)
				{
					var cacheKey = GetCacheKey(metadataFlags, entityFlags, entityName, attributeName, sortExpression);

					result = ObjectCacheManager.Get(cacheKey,
						cache =>
						{
							var metadata = ExecuteSelect(entityName, attributeName, sortExpression, entityFlags, metadataFlags, out rowsAffected);
							return metadata;
						},
						(cache, metadata) =>
						{
							if (metadata != null)
							{
								var dependencies = GetCacheDependencies(metadata);
								cache.Insert(cacheKey, metadata, dependencies);
							}
						});
				}
				else
				{
					result =  ExecuteSelect(entityName, attributeName, sortExpression, entityFlags, metadataFlags, out rowsAffected);
				}
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				Tracing.FrameworkError("CrmMetadataDataSourceView", "ExecuteSelect", "{0}\n\n{1}", ex.Detail.InnerXml, ex);
			}
			catch (Exception e)
			{
				Tracing.FrameworkError("CrmMetadataDataSourceView", "ExecuteSelect", "Exception: {0}", e);

				// raise post-event with exception
				CrmMetadataDataSourceStatusEventArgs selectedExceptionArgs = new CrmMetadataDataSourceStatusEventArgs(0, e);
				OnSelected(selectedExceptionArgs);

				if (!selectedExceptionArgs.ExceptionHandled)
				{
					throw;
				}

				return result;
			}

			// raise post-event
			CrmMetadataDataSourceStatusEventArgs selectedArgs = new CrmMetadataDataSourceStatusEventArgs(rowsAffected, null);
			OnSelected(selectedArgs);

			Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "End");

			return result;
		}

		public IEnumerable ExecuteSelect(
			string entityName,
			string attributeName,
			string sortExpression,
			EntityFilters entityFlags,
			EntityFilters metadataFlags,
			out int rowsAffected)
		{
			rowsAffected = 0;
			IEnumerable result = null;

			var client = OrganizationServiceContextFactory.Create(Owner.CrmDataContextName);

			if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(attributeName))
			{
				Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "RetrieveAttributeMetadata: entityName={0}, attributeName={1}, sortExpression={2}", entityName, attributeName, sortExpression);

				var metadata = client.RetrieveAttribute(entityName, attributeName);

				if (metadata is PicklistAttributeMetadata)
				{
					var picklist = metadata as PicklistAttributeMetadata;

					var options = picklist.OptionSet.Options.ToArray();

					if (!string.IsNullOrEmpty(sortExpression))
					{
						Array.Sort(
							options,
							delegate(OptionMetadata x, OptionMetadata y)
							{
								int comparison = 0;

								if (sortExpression.StartsWith("Value") || sortExpression.StartsWith("OptionValue"))
									comparison = x.Value.Value.CompareTo(y.Value.Value);
								else if (sortExpression.StartsWith("Label") || sortExpression.StartsWith("OptionLabel"))
									comparison = x.Label.UserLocalizedLabel.Label.CompareTo(y.Label.UserLocalizedLabel.Label);

								return (!sortExpression.EndsWith("DESC") ? comparison : -comparison);
							});
					}

					result = options.Select(option => new { OptionLabel = option.Label.UserLocalizedLabel.Label, OptionValue = option.Value });
					rowsAffected = options.Length;
				}
				else if (metadata is StatusAttributeMetadata)
				{
					var status = metadata as StatusAttributeMetadata;

					var options = (StatusOptionMetadata[])status.OptionSet.Options.ToArray();

					if (!string.IsNullOrEmpty(sortExpression))
					{
						Array.Sort(
							options,
							delegate(StatusOptionMetadata x, StatusOptionMetadata y)
							{
								int comparison = 0;

								if (sortExpression.StartsWith("Value") || sortExpression.StartsWith("OptionValue"))
									comparison = x.Value.Value.CompareTo(y.Value.Value);
								else if (sortExpression.StartsWith("Label") || sortExpression.StartsWith("OptionLabel"))
									comparison = x.Label.UserLocalizedLabel.Label.CompareTo(y.Label.UserLocalizedLabel.Label);
								else if (sortExpression.StartsWith("State"))
									comparison = x.State.Value.CompareTo(y.State.Value);

								return (!sortExpression.EndsWith("DESC") ? comparison : -comparison);
							});
					}

					result = options.Select(option => new { OptionLabel = option.Label.UserLocalizedLabel.Label, OptionValue = option.Value });
					rowsAffected = options.Length;
				}
				else if (metadata is StateAttributeMetadata)
				{
					var state = metadata as StateAttributeMetadata;

					var options = (StateOptionMetadata[])state.OptionSet.Options.ToArray();

					if (!string.IsNullOrEmpty(sortExpression))
					{
						Array.Sort(
							options,
							delegate(StateOptionMetadata x, StateOptionMetadata y)
							{
								int comparison = 0;

								if (sortExpression.StartsWith("Value") || sortExpression.StartsWith("OptionValue"))
									comparison = x.Value.Value.CompareTo(y.Value.Value);
								else if (sortExpression.StartsWith("Label") || sortExpression.StartsWith("OptionLabel"))
									comparison = x.Label.UserLocalizedLabel.Label.CompareTo(y.Label.UserLocalizedLabel.Label);
								else if (sortExpression.StartsWith("DefaultStatus"))
									comparison = x.DefaultStatus.Value.CompareTo(y.DefaultStatus.Value);

								return (!sortExpression.EndsWith("DESC") ? comparison : -comparison);
							});
					}

					result = options.Select(option => new { OptionLabel = option.Label.UserLocalizedLabel.Label, OptionValue = option.Value });
					rowsAffected = options.Length;
				}
			}
			else if (!string.IsNullOrEmpty(entityName))
			{
				Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "RetrieveEntityMetadata: entityName={0}, entityFlags={1}", entityName, entityFlags);

				var metadata = client.RetrieveEntity(entityName, entityFlags);
				
				result = metadata.Attributes;
				rowsAffected = metadata.Attributes.Length;
			}
			else
			{
				Tracing.FrameworkInformation("CrmMetadataDataSourceView", "ExecuteSelect", "RetrieveMetadata: metadataFlags={0}", metadataFlags);

				var metadata = client.RetrieveAllEntities(metadataFlags);

				result = metadata;
				rowsAffected = metadata.Length;
			}

			return result;
		}

		public override bool CanSort
		{
			get
			{
				return true;
			}
		}

		private string GetCacheKey(
			EntityFilters metadataFlags,
			EntityFilters entityFlags,
			string entityName,
			string attributeName,
			string sortExpression)
		{
			CacheParameters cacheParameters = Owner.GetCacheParameters();
			return cacheParameters.CacheKey.GetCacheKey(
				Context,
				Owner,
				Owner,
				delegate
				{
					string cacheKey = "MetadataFlags={0}:entityFlags={1}:EntityName={2}:AttributeName={3}:SortExpression={4}".FormatWith(
						metadataFlags,
						entityFlags,
						entityName,
						attributeName,
						sortExpression);

					return "Adxstudio:Type={0}:ID={1}:{2}".FormatWith(Owner.GetType().FullName, Owner.ID, cacheKey);
				});
		}

		private IEnumerable<string> GetCacheDependencies(IEnumerable results)
		{
			CacheParameters cacheParameters = Owner.GetCacheParameters();
			var dependencies = new List<string>();

			dependencies.Add("xrm:dependency:metadata:*");

			// get the global dependencies
			foreach (CacheKeyDependency ckd in cacheParameters.Dependencies)
			{
				dependencies.Add(ckd.GetCacheKey(Context, Owner, Owner));
			}

			// get the item dependencies
			foreach (object result in results)
			{
				foreach (CacheKeyDependency ckd in cacheParameters.ItemDependencies)
				{
					dependencies.Add(ckd.GetCacheKey(Context, Owner, result));
				}
			}

			return dependencies;
		}

		protected static string GetNonNullOrEmpty(params string[] values)
		{
			foreach (string value in values)
			{
				if (!string.IsNullOrEmpty(value))
				{
					return value;
				}
			}

			return null;
		}

		#region IStateManager Members

		private bool _tracking;

		public bool IsTrackingViewState
		{
			get { return _tracking; }
		}

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		protected virtual void LoadViewState(object savedState)
		{
			if (savedState != null)
			{
				object[] state = (object[])savedState;

				if (state[0] != null)
				{
					((IStateManager)SelectParameters).LoadViewState(state[0]);
				}

				EntityName = state[1] as string;
				AttributeName = state[2] as string;
				MetadataFlags = (EntityFilters)state[3];
				EntityFlags = (EntityFilters)state[4];
				SortExpression = state[5] as string;
			}
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object[]
			{
				(SelectParameters != null) ? ((IStateManager)SelectParameters).SaveViewState() : null,
				EntityName,
				AttributeName,
				MetadataFlags,
				EntityFlags,
				SortExpression
			};

			for (int i = 0; i < state.Length; i++)
			{
				if (state[i] != null)
				{
					return state;
				}
			}

			return null;
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		public void TrackViewState()
		{
			_tracking = true;

			if (_selectParameters != null)
			{
				((IStateManager)_selectParameters).TrackViewState();
			}
		}

		#endregion
	}
}
