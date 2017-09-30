/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using CancelEventArgs = System.ComponentModel.CancelEventArgs;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Renders a form for a given entity name, using CRM metadata.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This control currently only supports insertion (no updates, etc.), and should generally use
	/// a <see cref="CrmDataSource"/> for its data source.
	/// </para>
	/// <para>
	/// This control currently supports the following field types: single-line text (with required
	/// field validation), multi-line text (with required field validation), picklists, boolean
	/// (i.e., checkboxes), datetime (supports date-only fields), integer (with required field
	/// and range validation), and money (with required field and range validation).
	/// </para>
	/// <para>
	/// Other field types (e.g., lookups) will not be rendered.
	/// </para>
	/// </remarks>
	[DefaultProperty("DataSource"), ParseChildren(true), PersistChildren(false)]
	public class CrmEntityFormView : DataBoundControl, INamingContainer
	{
		private int _defaultLangaugeCode;

		private static readonly object _eventItemInserted = new object();
		private static readonly object _eventItemInserting = new object();

		private string _cellTemplateFactoryType = typeof(CellTemplateFactory).FullName;
		private ITemplate _insertItemTemplate;
		private readonly IDictionary<string, CellBinding> _cellBindings = new Dictionary<string, CellBinding>();
		private string _validationGroup;

		public event EventHandler<CrmEntityFormViewInsertedEventArgs> ItemInserted
		{
			add { Events.AddHandler(_eventItemInserted, value); }
			remove { Events.RemoveHandler(_eventItemInserted, value); }
		}

		public event EventHandler<CrmEntityFormViewInsertingEventArgs> ItemInserting
		{
			add { Events.AddHandler(_eventItemInserting, value); }
			remove { Events.RemoveHandler(_eventItemInserting, value); }
		}

		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public ITemplate InsertItemTemplate
		{
			get { return _insertItemTemplate ?? new DefaultInsertItemTemplate(ValidationGroup); }
			set { _insertItemTemplate = value; }
		}

		protected IDictionary<string, CellBinding> CellBindings
		{
			get { return _cellBindings; }
		}

		public string CellTemplateFactoryType
		{
			get { return _cellTemplateFactoryType; }
			set { _cellTemplateFactoryType = value; }
		}

		protected EntityMetadata EntityMetadata { get; set; }

		public string EntityName
		{
			get
			{
				var entityName = ViewState["EntityName"] as string;

				if (!string.IsNullOrEmpty(entityName))
				{
					return entityName;
				}

				if (string.IsNullOrEmpty(EntityType))
				{
					return null;
				}

				var entityType = Type.GetType(EntityType, true);

				ViewState["EntityName"] = entityType.GetEntityLogicalName();

				return ViewState["EntityName"] as string;
			}
			set { ViewState["EntityName"] = value; }
		}

		public string EntityType
		{
			get { return ViewState["EntityType"] as string; }
			set { ViewState["EntityType"] = value; }
		}

		public int LanguageCode
		{
			get { return (int)(ViewState["LanguageCode"] ?? _defaultLangaugeCode); }
			set { ViewState["LanguageCode"] = value; }
		}

		public string SavedQueryName
		{
			get { return ViewState["SavedQueryName"] as string; }
			set { ViewState["SavedQueryName"] = value; }
		}

		public bool ShowUnsupportedFields { get; set; }

		public string TabName
		{
			get { return ViewState["TabName"] as string; }
			set { ViewState["TabName"] = value; }
		}

		public string ValidationGroup
		{
			get { return _validationGroup ?? "CrmEntityFormView_{0}".FormatWith(EntityName); }
			set { _validationGroup = value; }
		}

		///<summary>
		/// Trigger the insertion of the form view
		///</summary>
		public virtual void InsertItem()
		{
			HandleInsert(null);
		}

		protected virtual ICellTemplateFactory CreateCellTemplateFactory()
		{
			var factoryType = Type.GetType(CellTemplateFactoryType, true, true);

			return (ICellTemplateFactory)Activator.CreateInstance(factoryType);
		}

		protected override void CreateChildControls()
		{
			if (string.IsNullOrEmpty(EntityName))
			{
				throw new InvalidOperationException("EntityName can not be null or empty.");
			}

			Controls.Add(new ValidationSummary
			{
				CssClass = "validation-summary",
				ValidationGroup = ValidationGroup,
				DisplayMode = ValidationSummaryDisplayMode.BulletList
			});

			GetFormTemplate().InstantiateIn(this);

			InsertItemTemplate.InstantiateIn(this);
		}

		protected virtual ITemplate GetFormTemplate()
		{
			var context = OrganizationServiceContextFactory.Create() as OrganizationServiceContext;

			var cellTemplateFactory = CreateCellTemplateFactory();

			if (!string.IsNullOrEmpty(TabName))
			{
				var formXml = context.CreateQuery("systemform")
					.Single(form => form.GetAttributeValue<string>("objecttypecode") == EntityName
						&& form.GetAttributeValue<OptionSetValue>("type").Value == 2)
					.GetAttributeValue<string>("formxml");

				var sections = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab").Where(
					tab => tab.XPathSelectElements("labels/label").Any(
						label => label.Attributes("description").Any(description => description.Value == TabName))).SelectMany(tab => tab.XPathSelectElements("columns/column/sections/section"));

				cellTemplateFactory.Initialize(this, new FormXmlCellMetadataFactory(), CellBindings, LanguageCode, ValidationGroup, ShowUnsupportedFields);

				var rowTemplateFactory = new RowTemplateFactory(LanguageCode);

				var sectionTemplates = sections.Select(s => new SectionTemplate(s, LanguageCode, EntityMetadata, cellTemplateFactory, rowTemplateFactory));

				return new CompositeTemplate(sectionTemplates);
			}

			if (!string.IsNullOrEmpty(SavedQueryName))
			{
				cellTemplateFactory.Initialize(this, new SavedQueryCellMetadataFactory(), CellBindings, LanguageCode, ValidationGroup, ShowUnsupportedFields);

				var layoutXml = context.CreateQuery("savedquery")
					.Single(view => view.GetAttributeValue<string>("name") == SavedQueryName)
					.GetAttributeValue<string>("layoutxml");

				var rows = XDocument.Parse(layoutXml).XPathSelectElements("grid/row");

				var rowTemplates = rows.Select(r => new SavedQueryRowTemplate(r, LanguageCode, EntityMetadata, cellTemplateFactory));

				return new CompositeTemplate(rowTemplates);
			}

			return new EmptyTemplate();
		}

		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			if (!(args is CommandEventArgs))
			{
				return false;
			}

			OnItemCommand(args as CommandEventArgs);

			return true;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			var context = OrganizationServiceContextFactory.Create() as OrganizationServiceContext;

			EntityMetadata = context.RetrieveEntity(EntityName, EntityFilters.Attributes);

			_defaultLangaugeCode = LanguageCode > 0
				? LanguageCode 
				: EntityMetadata.DisplayName.UserLocalizedLabel.LanguageCode;
		}

		protected virtual void OnItemCommand(CommandEventArgs args)
		{
			if (args.CommandName == "Insert")
			{
				HandleInsert(args);
			}
		}

		protected virtual void OnItemInserted(CrmEntityFormViewInsertedEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewInsertedEventArgs>)Events[_eventItemInserted];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnItemInserting(CrmEntityFormViewInsertingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewInsertingEventArgs>)Events[_eventItemInserting];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		private void HandleInsert(CommandEventArgs args)
		{
			var dataSource = GetDataSource();

			if (dataSource == null)
			{
				throw new InvalidOperationException("Control must have a data source.");
			}

			// Expand the cell bindings to retrieve all form values.
			var values = CellBindings.ToDictionary(cell => cell.Key, cell => cell.Value.Get());

			var insertingEventArgs = new CrmEntityFormViewInsertingEventArgs(values);

			OnItemInserting(insertingEventArgs);

			if ((!Page.IsValid) || insertingEventArgs.Cancel)
			{
				return;
			}

			var dataSourceView = dataSource.GetView(DataMember) as CrmDataSourceView;

			if (dataSourceView == null)
			{
				return;
			}

			dataSourceView.Inserted += DataSourceViewInserted;

			dataSourceView.Insert(values, EntityName);
		}

		private void DataSourceViewInserted(object sender, CrmDataSourceViewInsertedEventArgs e)
		{
			var insertedEventArgs = new CrmEntityFormViewInsertedEventArgs
			{
				EntityId = e.EntityId,
				Exception = e.Exception,
				ExceptionHandled = e.ExceptionHandled
			};

			OnItemInserted(insertedEventArgs);
		}

		public class DefaultInsertItemTemplate : ITemplate
		{
			private readonly string _validationGroup;

			public DefaultInsertItemTemplate(string validationGroup)
			{
				_validationGroup = validationGroup;
			}

			public void InstantiateIn(Control container)
			{
				container.Controls.Add(new Button
				{
					CommandName = "Insert",
					Text = "Submit",
					ValidationGroup = _validationGroup,
					CausesValidation = true
				});
			}
		}
	}

	public class CrmEntityFormViewInsertedEventArgs : EventArgs
	{
		public Guid? EntityId { get; set; }

		public Exception Exception { get; set; }

		public bool ExceptionHandled { get; set; }
	}

	public class CrmEntityFormViewInsertingEventArgs : CancelEventArgs
	{
		public CrmEntityFormViewInsertingEventArgs(IDictionary<string, object> values)
		{
			values.ThrowOnNull("values");

			Values = values;
		}

		public IDictionary<string, object> Values { get; private set; }
	}
}
