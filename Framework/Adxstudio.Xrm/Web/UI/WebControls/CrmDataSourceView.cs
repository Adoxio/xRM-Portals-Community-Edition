/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Caching;
using Microsoft.Xrm.Client.Services;
using System.Web.Security.AntiXss;
using System.Globalization;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Portal.Web.UI;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using ParameterCollection = System.Web.UI.WebControls.ParameterCollection;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Supports the <see cref="CrmDataSource"/> control and provides an interface for data-bound controls to retrieve data from the <see cref="IOrganizationService"/>.
	/// </summary>
	public class CrmDataSourceView : AsyncDataSourceView, IStateManager
	{
		protected class CrmEntityWrapper
		{
			private readonly Entity _entity;

			public CrmEntityWrapper(Entity entity)
			{
				_entity = entity;
			}

			public string Name
			{
				get { return _entity.LogicalName; }
			}

			public Guid? ID
			{
				get { return _entity.Id; }
			}

			public object this[string propertyLogicalName]
			{
				get { return _entity.GetAttributeValue<object>(propertyLogicalName); }
				set { _entity.SetAttributeValue(propertyLogicalName, value); }
			}
		}

		private static readonly string _allColumnsParameterValue = "AllColumns";
		private static readonly string _attributesParameterName = "Attributes";
		private static readonly string _columnSetParameterName = "ColumnSet";
		private static readonly string _entityNameParameterName = "EntityName";
		private static readonly string _fetchXmlParameterName = "FetchXml";

		private static readonly string[] _keywords =
			{
				_fetchXmlParameterName,
				_entityNameParameterName,
				_attributesParameterName,
				_valuesParameterName,
				_columnSetParameterName,
				_ordersParameterName,
				_allColumnsParameterValue
			};

		private static readonly string _ordersParameterName = "Orders";
		private static readonly string _valuesParameterName = "Values";
		private static readonly object EventSelected = new object();
		private static readonly object EventSelecting = new object();
		private static readonly object EventInserted = new object();
		private static readonly object EventUpdated = new object();
		private static readonly object EventDeleted = new object();

		private readonly HttpContext _context;
		private readonly CrmDataSource _owner;
		private readonly CrmOrganizationServiceContext _crmDataContext;
		private string _cacheKey;
		private bool _cancelSelectOnNullParameter;

		private IOrganizationService _client;
		private CrmDataSource.QueryByAttributeParameters _queryByAttribute;
		private ParameterCollection _queryParameters;

		private Func<QueryBase, EntityCollection> _execute;
		private RetrieveMultipleRequest _request;
		private Func<Fetch, Entity> _executeSingle;
		private Fetch _fetch;
		private Func<Fetch, EntityCollection> _executeFetchMultiple;
		private ParameterCollection _selectParameters;
		private bool _tracking;

		public event EventHandler<CrmDataSourceViewInsertedEventArgs> Inserted
		{
			add { Events.AddHandler(EventInserted, value); }
			remove { Events.RemoveHandler(EventInserted, value); }
		}

		public event EventHandler<CrmDataSourceViewUpdatedEventArgs> Updated
		{
			add { Events.AddHandler(EventUpdated, value); }
			remove { Events.RemoveHandler(EventUpdated, value); }
		}

		public event EventHandler<CrmDataSourceViewDeletedEventArgs> Deleted
		{
			add { Events.AddHandler(EventDeleted, value); }
			remove { Events.RemoveHandler(EventDeleted, value); }
		}

		public CrmDataSourceView(CrmDataSource owner, string name, HttpContext context)
			: base(owner, name)
		{
			_cancelSelectOnNullParameter = true;
			_owner = owner;
			_context = context;
			_crmDataContext = OrganizationServiceContextFactory.Create(_owner.CrmDataContextName);
		}

		protected CrmDataSource Owner
		{
			get { return _owner; }
		}

		protected HttpContext Context
		{
			get { return _context; }
		}

		protected RetrieveMultipleRequest Request
		{
			get
			{
				if (_request == null)
				{
					_request = new RetrieveMultipleRequest();
				}

				return _request;
			}
		}

		protected Fetch Fetch
		{
			get
			{
				if (_fetch == null)
				{
					_fetch = new Fetch();
				}

				return _fetch;
			}
		}

		public CrmDataSource.QueryByAttributeParameters QueryByAttribute
		{
			get
			{
				if (_queryByAttribute == null)
				{
					_queryByAttribute = new CrmDataSource.QueryByAttributeParameters();

					if (_tracking)
					{
						((IStateManager)_queryByAttribute).TrackViewState();
					}
				}

				return _queryByAttribute;
			}
		}

		public ParameterCollection QueryParameters
		{
			get
			{
				if (_queryParameters == null)
				{
					_queryParameters = new ParameterCollection();
					_queryParameters.ParametersChanged += SelectParametersChangedEventHandler;

					if (_tracking)
					{
						((IStateManager)_queryParameters).TrackViewState();
					}
				}

				return _queryParameters;
			}
		}

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

		public override bool CanSort
		{
			get { return true; }
		}

		#region IStateManager Members

		public bool IsTrackingViewState
		{
			get { return _tracking; }
		}

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		#endregion

		private string GetCacheKey(QueryBase query)
		{
			if (_cacheKey == null)
			{
				CacheParameters cacheParameters = Owner.GetCacheParameters();
				_cacheKey = cacheParameters.CacheKey.GetCacheKey(
					Context,
					Owner,
					Owner,
					delegate
					{
						string serializedQuery = Serialize(query);
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("QueryByAttribute : {0}", serializedQuery.GetHashCode().ToString(CultureInfo.InvariantCulture)));
						return "Adxstudio:Type={0}:ID={1}:Hash={2}".FormatWith(Owner.GetType().FullName, Owner.ID, serializedQuery.GetHashCode()).ToLower();
					});
			}

			return _cacheKey;
		}

		public event EventHandler<CrmDataSourceStatusEventArgs> Selected
		{
			add { base.Events.AddHandler(EventSelected, value); }
			remove { base.Events.RemoveHandler(EventSelected, value); }
		}

		protected virtual void OnSelected(CrmDataSourceStatusEventArgs e)
		{
			EventHandler<CrmDataSourceStatusEventArgs> handler = base.Events[EventSelected] as EventHandler<CrmDataSourceStatusEventArgs>;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		public event EventHandler<CrmDataSourceSelectingEventArgs> Selecting
		{
			add { base.Events.AddHandler(EventSelecting, value); }
			remove { base.Events.RemoveHandler(EventSelecting, value); }
		}

		protected virtual void OnSelecting(CrmDataSourceSelectingEventArgs args)
		{
			EventHandler<CrmDataSourceSelectingEventArgs> handler = base.Events[EventSelecting] as EventHandler<CrmDataSourceSelectingEventArgs>;

			if (handler != null)
			{
				handler(this, args);
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

		protected override IAsyncResult BeginExecuteSelect(
			DataSourceSelectArguments arguments,
			AsyncCallback asyncCallback,
			object asyncState)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

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

			string fetchXml;
			QueryByAttribute query;

			InitializeParameters(out fetchXml, out query);

			// raise pre-event
			CrmDataSourceSelectingEventArgs selectingArgs = new CrmDataSourceSelectingEventArgs(
				Owner,
				arguments,
				fetchXml,
				query);

			OnSelecting(selectingArgs);

			IEnumerable selectResult = null;

			if (selectingArgs.Cancel)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Cancel");
				return new SynchronousAsyncSelectResult(selectResult, asyncCallback, asyncState);
			}

			// merge event results
			arguments.RaiseUnsupportedCapabilitiesError(this);
			fetchXml = selectingArgs.FetchXml;

			if (CancelSelectOnNullParameter && string.IsNullOrEmpty(fetchXml) && query == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "CancelSelectOnNullParameter");
				return new SynchronousAsyncSelectResult(selectResult, asyncCallback, asyncState);
			}

			try
			{
				_client = OrganizationServiceContextFactory.Create(Owner.CrmDataContextName);

				if (!string.IsNullOrEmpty(fetchXml))
				{

					var fetch = ToFetch(arguments, fetchXml);

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

					return ExecuteSelect(_client, null, fetch, asyncCallback, asyncState);
				}

				if (query != null)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "QueryByAttribute");

					// the SortExpression has high priority, apply it to the query
					AppendSortExpressionToQuery(arguments.SortExpression, order => query.Orders.Add(order));

                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

					return ExecuteSelect(_client, query, null, asyncCallback, asyncState);
				}
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("{0}\n\n{1}", ex.Detail.InnerXml, ex.ToString()));
            }
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception: {0}", e.ToString()));

                _client = null;

				return new SynchronousAsyncSelectResult(e, asyncCallback, asyncState);
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return new SynchronousAsyncSelectResult(selectResult, asyncCallback, asyncState);
		}

		protected virtual Fetch ToFetch(DataSourceSelectArguments arguments, string fetchXml)
		{
			Fetch expression = null;

			// the SortExpression has high priority, apply it to the query
			AppendSortExpressionToQuery(arguments.SortExpression, order =>
			{
				var xml = XElement.Parse(fetchXml);
				var entityElement = xml.Element("entity");

				if (entityElement != null)
				{
					var orderElement = new XElement("order",
						new XAttribute("attribute", order.AttributeName),
						new XAttribute("descending", order.OrderType == OrderType.Descending));

					entityElement.Add(orderElement);

					expression = Fetch.Parse(xml);
				}
			});

			return expression ?? Fetch.Parse(fetchXml);
		}

		protected IAsyncResult ExecuteSelect(
			IOrganizationService client,
			QueryBase query,
			Fetch fetch,
			AsyncCallback asyncCallback,
			object asyncState)
		{
			string cacheKey = string.Empty;
			if (query != null)
			{
				Request.Query = query;
				cacheKey = GetCacheKey(query);
			}
			else if (fetch != null)
			{
				Fetch.Entity = fetch.Entity;
				cacheKey = GetCacheKey(fetch.ToFetchExpression());
			}

			if (Owner.CacheParameters.Enabled)
			{
				// try load from cache
				IEnumerable selectResult = ObjectCacheManager.GetInstance().Get(cacheKey) as IEnumerable;

				if (selectResult != null)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Found in cache: {0}", cacheKey));
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

					return new SynchronousAsyncSelectResult(selectResult, asyncCallback, asyncState);
				}
			}

			if (Owner.IsSingleSource)
			{
				_executeSingle = new Func<Fetch, Entity>(f => client.RetrieveSingle(f));
				return _executeSingle.BeginInvoke(fetch, asyncCallback, asyncState);
			}
			else
			{
				if (fetch != null)
				{
					_executeFetchMultiple = new Func<Fetch, EntityCollection>(f => client.RetrieveMultiple(f));
					return _executeFetchMultiple.BeginInvoke(fetch, asyncCallback, asyncState);
				}
			}

			_execute = new Func<QueryBase, EntityCollection>(client.RetrieveMultiple);
			return _execute.BeginInvoke(query, asyncCallback, asyncState);
		}

		protected IEnumerable<Entity> ExecuteSelect(IEnumerable<Entity> entities)
		{
			foreach (Entity entity in entities)
			{
				yield return entity as Entity;
			}
		}

		protected override IEnumerable EndExecuteSelect(IAsyncResult asyncResult)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			if (_client == null)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End: _client=null");

				return null;
			}

			IEnumerable selectResult = null;
			int rowsAffected = 0;

			try
			{
				SynchronousAsyncSelectResult syncResult = asyncResult as SynchronousAsyncSelectResult;

				if (syncResult != null)
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "syncResult");

					selectResult = syncResult.SelectResult;
				}
				else
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, "EndExecute");

					if (!Owner.IsSingleSource)
					{
						var response = _execute != null ? _execute.EndInvoke(asyncResult) : _executeFetchMultiple.EndInvoke(asyncResult);

						if (response != null)
						{
							var entities = response.Entities;
							rowsAffected = entities.Count;
							selectResult = ExecuteSelect(entities).ToList();

							if (Owner.CacheParameters.Enabled)
							{
								IEnumerable<string> dependencies;
								string cacheKey;
								if (Request.Query != null)
								{
									dependencies = GetCacheDependencies(Request.Query, selectResult);
									cacheKey = GetCacheKey(Request.Query);
								}
								else
								{
									dependencies = GetCacheDependencies(Fetch, selectResult, Owner.IsSingleSource);
									cacheKey = GetCacheKey(Request.Query);
								}

								// insert into cache
								ObjectCacheManager.GetInstance().Insert(cacheKey, selectResult, dependencies);
							}
						}
					}
					else
					{
						var entity = _executeSingle.EndInvoke(asyncResult);
						if (entity != null)
						{
							selectResult = ExecuteSelect(new[] { entity }).ToList();
							if (Owner.CacheParameters.Enabled)
							{
								var dependencies = GetCacheDependencies(Fetch, selectResult, Owner.IsSingleSource);
								var cacheKey = GetCacheKey(Fetch.ToFetchExpression());

							// insert into cache
							ObjectCacheManager.GetInstance().Insert(cacheKey, selectResult, dependencies);
						}
					}
				}
			}
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("{0}\n\n{1}", ex.Detail.InnerXml, ex.ToString()));
            }
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Exception: {0}", e.ToString()));

                // raise post-event with exception
                CrmDataSourceStatusEventArgs selectedExceptionArgs = new CrmDataSourceStatusEventArgs(0, e);
				OnSelected(selectedExceptionArgs);

				if (!selectedExceptionArgs.ExceptionHandled)
				{
					throw;
				}

				return selectResult;
			}
			finally
			{
				_client = null;
			}

			// raise post-event
			CrmDataSourceStatusEventArgs selectedArgs = new CrmDataSourceStatusEventArgs(rowsAffected, null);
			OnSelected(selectedArgs);

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			return string.IsNullOrEmpty(Owner.StaticEntityWrapperTypeName) ? selectResult : CreateEntities(selectResult);
		}

		private IEnumerable<object> CreateEntities(IEnumerable entities)
		{
			if (entities == null) yield break;

			foreach (var entity in entities)
			{
				yield return CreateEntity(entity);
			}
		}

		private object CreateEntity(object entity)
		{
			// attempt to dynamically convert from DynamicEntityWrapper to the specified type
			var type = TypeExtensions.GetType(Owner.StaticEntityWrapperTypeName);

			// invoke the constructor that takes a single DynamicEntityWrapper
			return Activator.CreateInstance(type, entity);
		}

		#region Delete Members

		public override bool CanDelete
		{
			get { return true; }
		}

		/// <summary>
		/// Deletes an entity.
		/// </summary>
		/// <param name="keys">The keys by which to find the entity to be deleted. "ID" and "Name" must be specified.</param>
		/// <param name="oldValues">The entity properties before the update. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Delete(IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete(keys, oldValues);
		}

		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			if (!CanDelete) throw new NotSupportedException("Delete not supported.");

			var id = keys["ID"] as Guid?;
			var entityName = keys["Name"] as string;

			if (id == null || entityName == null) throw new ArgumentException("Delete requires the 'ID' and 'Name' to be specified as DataKeyNames.", "keys");

			var rowsAffected = 0;
			var deletedEventArgs = new CrmDataSourceViewDeletedEventArgs();

			try
			{
				var entity = _crmDataContext.Retrieve(entityName, id.Value, new ColumnSet(true));

				if (entity == null) throw new NullReferenceException("The {0} entity with ID={1} couldn't be found.".FormatWith(id, entityName));

				_crmDataContext.Attach(entity);
				
				_crmDataContext.DeleteObject(entity);
				
				var result = _crmDataContext.SaveChanges();

				rowsAffected = result.HasError ? 0 : 1;
			}
			catch (Exception e)
			{
                ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                deletedEventArgs.Exception = e;
				deletedEventArgs.ExceptionHandled = true;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "End");

			OnDeleted(deletedEventArgs);

			return rowsAffected;
		}

		protected virtual void OnDeleted(CrmDataSourceViewDeletedEventArgs args)
		{
			var handler = (EventHandler<CrmDataSourceViewDeletedEventArgs>)Events[EventDeleted];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		#endregion

		#region Insert Members

		public override bool CanInsert
		{
			get { return true; }
		}

		/// <summary>
		/// Creates a new entity.
		/// </summary>
		/// <param name="values">The entity properties. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <param name="entityName">The type of entity to create e.g. "contact"</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Insert(IDictionary values, string entityName)
		{
			values.Add("EntityName", entityName);
			
			return ExecuteInsert(values);
		}

		protected override int ExecuteInsert(IDictionary values)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			if (!CanInsert) throw new NotSupportedException("Insert not supported.");

			string entityName = values["EntityName"] as string;

			if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Insert requires an EntityName to be specified as one of the values.", "values");

			var entity = new Entity(entityName);

			SetEntityAttributes(entity, values);

			var rowsAffected = 0;
			var insertedEventArgs = new CrmDataSourceViewInsertedEventArgs();

			try
			{
				_crmDataContext.AddObject(entity);
				_crmDataContext.SaveChanges();

				insertedEventArgs.EntityId = entity.Id;

				rowsAffected = 1;
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                insertedEventArgs.Exception = e;
				insertedEventArgs.ExceptionHandled = true;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: rowsAffected={0}", rowsAffected));

			OnInserted(insertedEventArgs);

			return rowsAffected;
		}

		protected virtual void OnInserted(CrmDataSourceViewInsertedEventArgs args)
		{
			var handler = (EventHandler<CrmDataSourceViewInsertedEventArgs>)Events[EventInserted];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		#endregion

		protected virtual void SetEntityAttributes(Entity entity, IDictionary values)
		{
			var metadata = _crmDataContext.RetrieveEntity(entity.LogicalName, EntityFilters.Attributes);

			foreach (DictionaryEntry value in values)
			{
				var attributeName = value.Key.ToString().TrimStart('[').TrimEnd(']');

				if (string.Equals(attributeName, "EntityName", StringComparison.InvariantCulture))
				{
					continue;
				}

				AttributeTypeCode? attributeType;
				try
				{
					var attribute = metadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);

					if (attribute == null) // ignore subgrid, iframe or web resources 
					{
                        ADXTrace.Instance.TraceInfo(TraceCategory.Application, "No value set. Key '{0}' is not a name of an attribute on entity type {1}. This is likely a subgrid, iframe, or web resource.", attributeName, entity.LogicalName);
						continue;
					}

					attributeType = attribute.AttributeType;
				}
				catch (Exception)
				{
					throw new Exception("{0} could not be found in entity type {1}".FormatWith(attributeName, entity.LogicalName));
				}

				if (!(value.Value is EntityReference) && (
					attributeType == AttributeTypeCode.Customer ||
						attributeType == AttributeTypeCode.Lookup ||
							attributeType == AttributeTypeCode.Owner))
				{
					Guid id;

					if (value.Value == null)
					{
						entity.SetAttributeValue<EntityReference>(attributeName, null);

						continue;
					}

					if (!Guid.TryParse(value.Value.ToString(), out id))
					{
                        throw new ArgumentException("value is not of type Guid");
					}

					var entityReference = new EntityReference(entity.LogicalName, id);

					entity.SetAttributeValue<EntityReference>(attributeName, entityReference);
				}
				else if (attributeType == AttributeTypeCode.Status)
				{
					entity.SetAttributeValue<OptionSetValue>(attributeName, value.Value);
				}
				else if (attributeType == AttributeTypeCode.Picklist)
				{
					// determine if a multiselect picklist values should be saved
					var picklistvaluesfield = metadata.Attributes.FirstOrDefault(a => a.LogicalName == string.Format("{0}selectedvalues", attributeName));

					if (picklistvaluesfield == null)
					{
						entity.SetAttributeValue<OptionSetValue>(attributeName, value.Value);
					}
					else
					{
						entity[picklistvaluesfield.LogicalName] = value.Value;
					}
				}
				else if (attributeType == AttributeTypeCode.Money)
				{
					entity.SetAttributeValue<Money>(attributeName, value.Value);
				}
				else
				{
					entity[attributeName] = value.Value;
				}
			}
		}

		#region Update Members

		public override bool CanUpdate
		{
			get { return true; }
		}

		/// <summary>
		/// Updates an entity.
		/// </summary>
		/// <param name="keys">The keys by which to find the entity to be updated. "ID" and "Name" must be specified.</param>
		/// <param name="values">The entity properties to update. Key: Property name e.g. "firstname" Value: Value of the property e.g. "Jane"</param>
		/// <param name="oldValues">The entity properties before the update.</param>
		/// <returns>1 if successful; otherwise, 0.</returns>
		public int Update(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return ExecuteUpdate(keys, values, oldValues);
		}

		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Begin");

			if (!CanUpdate) throw new NotSupportedException("Update not supported.");

			var id = keys["ID"] as Guid?;
			var entityName = keys["Name"] as string;

			if (id == null || entityName == null) throw new ArgumentException("Update requires the 'ID' and 'Name' to be specified as DataKeyNames.", "keys");

			var rowsAffected = 0;
			var updatedEventArgs = new CrmDataSourceViewUpdatedEventArgs();

			try
			{
				var entity = new Entity(entityName) { Id = id.Value };

				SetEntityAttributes(entity, values);

				_crmDataContext.Attach(entity);

				_crmDataContext.UpdateObject(entity);
				
				var result = _crmDataContext.SaveChanges();

				if (result.HasError)
				{
					rowsAffected = 0;
				}
				else
				{
					updatedEventArgs.Entity = entity;
					rowsAffected = 1;
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());

                updatedEventArgs.Exception = e;
				updatedEventArgs.ExceptionHandled = true;
			}

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("End: rowsAffected={0}", rowsAffected));

			OnUpdated(updatedEventArgs);

			return rowsAffected;
		}

		protected virtual void OnUpdated(CrmDataSourceViewUpdatedEventArgs args)
		{
			var handler = (EventHandler<CrmDataSourceViewUpdatedEventArgs>)Events[EventUpdated];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		#endregion

		private IEnumerable<string> GetCacheDependencies(QueryBase query, IEnumerable entities)
		{
			CacheParameters cacheParameters = Owner.GetCacheParameters();
			var dependencies = new List<string>();

			// get the global dependencies
			foreach (CacheKeyDependency ckd in cacheParameters.Dependencies)
			{
				dependencies.Add(ckd.GetCacheKey(Context, Owner, query));
			}

			// get the item dependencies
			foreach (Entity entity in entities)
			{
				dependencies.Add("xrm:dependency:entity:{0}:id={1}".FormatWith(entity.LogicalName, entity.Id));

				foreach (CacheKeyDependency ckd in cacheParameters.ItemDependencies)
				{
					dependencies.Add(ckd.GetCacheKey(Context, Owner, entity));
				}
			}

			// add other dependencies
			var dependencyCalculator = new CacheDependencyCalculator("xrm:dependency");

			dependencies = dependencies.Concat(dependencyCalculator.GetDependenciesForObject(query)).Distinct().ToList();

			return dependencies;
		}

		private IEnumerable<string> GetCacheDependencies(Fetch fetch, IEnumerable entities, bool isSingle)
		{
			var dependencyCalculator = new CacheDependencyCalculator("xrm:dependency");

			var dependencies = dependencyCalculator.GetDependenciesForObject(fetch, isSingle).Concat(dependencyCalculator.GetDependenciesForObject(entities, isSingle)).Distinct();

			return dependencies;
		}

		private void InitializeParameters(out string fetchXml, out QueryByAttribute query)
		{
			// merge the select parameters
			IOrderedDictionary parameters = QueryParameters.GetValues(_context, _owner);

			fetchXml = GetNonNullOrEmpty(
				parameters[_fetchXmlParameterName] as string,
				_owner.FetchXml);

			if (!string.IsNullOrEmpty(fetchXml))
			{
				IOrderedDictionary selectParameters = SelectParameters.GetValues(_context, _owner);

				// apply select parameters replacement to the FetchXml
				foreach (DictionaryEntry entry in selectParameters)
				{
					if (entry.Key != null)
					{
						string key = entry.Key.ToString().Trim();

						if (!key.StartsWith("@"))
						{
							key = "@" + key;
						}

						string value = "{0}".FormatWith(entry.Value);

						if (Owner.EncodeParametersEnabled)
						{
							value = AntiXssEncoder.XmlEncode(value);
						}

						fetchXml = Regex.Replace(fetchXml, key, value, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					}
				}
			}

			// process the QueryByAttribute
			query = null;

			if (_owner.QueryByAttribute != null && !string.IsNullOrEmpty(_owner.QueryByAttribute.EntityName))
			{
				IOrderedDictionary selectParameters = SelectParameters.GetValues(_context, _owner);

				query = new QueryByAttribute();
				query.EntityName = LookupParameter(selectParameters, _owner.QueryByAttribute.EntityName);
				query.Attributes.AddRange(CopyParameters(selectParameters, _owner.QueryByAttribute.Attributes));
				query.Values.AddRange(CopyParameters(selectParameters, _owner.QueryByAttribute.Values));

				if (_owner.QueryByAttribute.ColumnSet != null && _owner.QueryByAttribute.ColumnSet.Count > 0)
				{
					// specify individual columns to load
					query.ColumnSet = new ColumnSet(CopyParameters(selectParameters, _owner.QueryByAttribute.ColumnSet));
				}
				else
				{
					// default to all columns
					query.ColumnSet = new ColumnSet(true);
				}

				if (_owner.QueryByAttribute.Orders != null && _owner.QueryByAttribute.Orders.Count > 0)
				{
					for (int i = 0; i < _owner.QueryByAttribute.Orders.Count; ++i)
					{
						OrderExpression order = new OrderExpression();
						order.AttributeName = LookupParameter(selectParameters, _owner.QueryByAttribute.Orders[i].Value);

						string orderText = LookupParameter(selectParameters, _owner.QueryByAttribute.Orders[i].Text);

						if (orderText.StartsWith("desc", StringComparison.InvariantCultureIgnoreCase))
						{
							order.OrderType = OrderType.Descending;
						}

						query.Orders.Add(order);
					}
				}

				// merge the select parameters
				string entityName = parameters[_entityNameParameterName] as string;

				if (!string.IsNullOrEmpty(entityName))
				{
					query.EntityName = entityName;
				}

				// comma delimited
				string attributes = parameters[_attributesParameterName] as string;

				if (!string.IsNullOrEmpty(attributes))
				{
					query.Attributes.Clear();
					query.Attributes.AddRange(attributes.Split(','));
				}

				// comma delimited
				string values = parameters[_valuesParameterName] as string;

				if (!string.IsNullOrEmpty(values))
				{
					query.Values.Clear();
					query.Values.AddRange(values.Split(','));
				}

				// comma delimited
				string columnSet = parameters[_columnSetParameterName] as string;

				if (!string.IsNullOrEmpty(columnSet))
				{
					if (string.Compare(columnSet, _allColumnsParameterValue, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						query.ColumnSet = new ColumnSet(true);
					}
					else
					{
						string[] parts = columnSet.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						if (parts.Length > 0)
						{
							for (int i = 0; i < parts.Length; i++)
							{
								parts[i] = parts[i].Trim();
							}

							query.ColumnSet.AddColumns(parts);
						}
						else
						{
							query.ColumnSet = new ColumnSet(true);
						}
					}
				}

				// comma delimited
				string orders = parameters[_ordersParameterName] as string;

				if (!string.IsNullOrEmpty(orders))
				{
					QueryByAttribute queryByAttribute = query;
					AppendSortExpressionToQuery(orders, order => queryByAttribute.Orders.Add(order));
					query = queryByAttribute;
				}

				// all remaining parameters are treated as key/value pairs
				Dictionary<string, object> extendedParameters = new Dictionary<string, object>();

				if (query.Attributes != null)
				{
					for (int i = 0; i < query.Attributes.Count; ++i)
					{
						extendedParameters[query.Attributes[i]] = query.Values[i];
					}
				}

				bool changed = false;

				foreach (string key in parameters.Keys)
				{
					// ignore special parameters
					if (!Array.Exists(_keywords, delegate(string value) { return string.Compare(value, key, StringComparison.InvariantCultureIgnoreCase) == 0; }))
					{
						extendedParameters[key] = parameters[key];
						changed = true;
					}
				}

				if (changed)
				{
					query.Attributes.Clear();
					query.Values.Clear();

					int i = 0;
					foreach (KeyValuePair<string, object> extendedParameter in extendedParameters)
					{
						query.Attributes[i] = extendedParameter.Key;
						query.Values[i] = extendedParameter.Value;
						++i;
					}
				}
			}
		}

		private string LookupParameter(IOrderedDictionary parameters, string value)
		{
			if (!string.IsNullOrEmpty(value) && value.StartsWith("@"))
			{
				string name = value.TrimStart('@');

				if (parameters.Contains(name))
				{
					value = parameters[name].ToString();
				}
				else if (parameters.Contains("@" + name))
				{
					value = parameters["@" + name].ToString();
				}
			}

			return value;
		}

		private string[] CopyParameters(IOrderedDictionary selectParameters, ListItemCollection list)
		{
			if (list != null && list.Count > 0)
			{
				string[] result = new string[list.Count];

				for (int i = 0; i < list.Count; ++i)
				{
					result[i] = LookupParameter(selectParameters, list[i].Value);
				}

				return result;
			}

			return null;
		}

		private static void AppendSortExpressionToQuery(string sortExpression, Action<OrderExpression> action)
		{
			string[] parts = sortExpression.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < parts.Length; ++i)
			{
				string part = parts[i].Trim();

				// attribute name and direction are separated by a space, direction is optional
				// attribute1 ascending, attribute2 descending
				string[] pairs = part.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				OrderExpression order = new OrderExpression();
				order.AttributeName = pairs[0];
				if (pairs.Length > 1 && (pairs[1].StartsWith("desc", StringComparison.InvariantCultureIgnoreCase)))
				{
					order.OrderType = OrderType.Descending;
				}

				action(order);
			}
		}

		private static string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value);
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

		protected virtual void LoadViewState(object savedState)
		{
			if (savedState != null)
			{
				Pair state = (Pair)savedState;

				if (state.First != null)
				{
					((IStateManager)SelectParameters).LoadViewState(state.First);
				}

				if (state.Second != null)
				{
					((IStateManager)QueryByAttribute).LoadViewState(state.Second);
				}
			}
		}

		protected virtual object SaveViewState()
		{
			Pair state = new Pair();
			state.First = (SelectParameters != null) ? ((IStateManager)SelectParameters).SaveViewState() : null;
			state.Second = (QueryByAttribute != null) ? ((IStateManager)QueryByAttribute).SaveViewState() : null;

			if ((state.First == null) && (state.Second == null))
			{
				return null;
			}

			return state;
		}

		public void TrackViewState()
		{
			_tracking = true;

			if (_selectParameters != null)
			{
				((IStateManager)_selectParameters).TrackViewState();
			}

			if (_queryByAttribute != null)
			{
				((IStateManager)_queryByAttribute).TrackViewState();
			}
		}
	}
}
