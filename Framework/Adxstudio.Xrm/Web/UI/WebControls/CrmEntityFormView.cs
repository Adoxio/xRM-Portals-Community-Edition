/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Globalization;
	using Adxstudio.Xrm.Mapping;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc;
	using Adxstudio.Xrm.Web.UI.CrmEntityFormView;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Configuration;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web.UI.WebControls;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using CellTemplateFactory = Adxstudio.Xrm.Web.UI.CrmEntityFormView.CellTemplateFactory;
	using FormXmlCellMetadataFactory = Adxstudio.Xrm.Web.UI.CrmEntityFormView.FormXmlCellMetadataFactory;
	using ICellTemplateFactory = Adxstudio.Xrm.Web.UI.CrmEntityFormView.ICellTemplateFactory;

	/// <summary>
	/// Renders a form for a given CRM entity
	/// </summary>
	public class CrmEntityFormView : Microsoft.Xrm.Portal.Web.UI.WebControls.CrmEntityFormView
	{
		private string _cellTemplateFactoryType = typeof(CellTemplateFactory).FullName;
		private static readonly object _eventItemUpdated = new object();
		private static readonly object _eventItemUpdating = new object();
		private string _entityDisplayName;
		private const string JapaneseFormPostfix = " - Japanese";

		private ITemplate _readOnlyItemTemplate;
		private ITemplate _nextStepTemplate;
		private ITemplate _previousStepTemplate;
		private ITemplate _updateItemTemplate;
		private ITemplate _insertItemTemplate;

		internal static string DefaultSubmitButtonCssClass = "btn btn-primary navbar-btn button submit";
		private readonly string DefaultSubmitButtonText = ResourceManager.GetString("Submit_Button_Label_Text");
		private const string DefaultValidationSummaryCssClass = "validation-summary alert alert-error alert-danger alert-block";
		private readonly string DefaultValidationHeaderText = ResourceManager.GetString("Form_Could_Not_Be_Submitted_For_The_Following_Reasons");
		private const string DefaultReadAccessDeniedSnippetName = "EntitySecurity/Record/ReadAccessDeniedMessage";
		private const string DefaultWriteAccessDeniedSnippetName = "EntitySecurity/Record/WriteAccessDeniedMessage";
		private const string DefaultCreateAccessDeniedSnippetName = "EntitySecurity/Record/CreateAccessDeniedMessage";

		private int? _stepCount;

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		public object CrmEntityId
		{
			get { return ViewState["CrmEntityId"]; }
			set { ViewState["CrmEntityId"] = value; }
		}

		/// <summary>
		/// The private field to hold the FormXmlCellMetatdata factory.
		/// </summary>
		private FormXmlCellMetadataFactory _formXmlCellMetadatFactory;

		/// <summary>
		/// The FormXmlCellMetadataFactory instance.
		/// </summary>
		private FormXmlCellMetadataFactory formXmlCellMetadataFactory
		{
			get { return _formXmlCellMetadatFactory ?? (_formXmlCellMetadatFactory = new FormXmlCellMetadataFactory()); }
		}

		private Lazy<PortalViewContext> _portalViewContext;

		protected PortalViewContext PortalViewContext
		{
			get { return _portalViewContext.Value; }
		}
		/// <summary>
		/// Event that occurs when the record has been updated.
		/// </summary>
		public event EventHandler<CrmEntityFormViewUpdatedEventArgs> ItemUpdated
		{
			add { Events.AddHandler(_eventItemUpdated, value); }
			remove { Events.RemoveHandler(_eventItemUpdated, value); }
		}

		/// <summary>
		/// Event that occurs immediately prior to updating the record.
		/// </summary>
		public event EventHandler<CrmEntityFormViewUpdatingEventArgs> ItemUpdating
		{
			add { Events.AddHandler(_eventItemUpdating, value); }
			remove { Events.RemoveHandler(_eventItemUpdating, value); }
		}

		/// <summary>
		/// Indicates whether or not the entity permission provider will assert privileges.
		/// </summary>
		[Description("Indicates whether or not the entity permission provider will assert privileges.")]
		public bool EnableEntityPermissions
		{
			get { return (bool)(ViewState["EnableEntityPermissions"] ?? false); }
			set { ViewState["EnableEntityPermissions"] = value; }
		}

		/// <summary>
		/// Indicates whether or not any Owner (ownerid) field on the form will be shown.
		/// </summary>
		[Category("Behavior"), Description("Indicates whether or not any Owner (ownerid) field on the form will be shown."), DefaultValue(false)]
		public bool ShowOwnerFields
		{
			get { return (bool)(ViewState["ShowOwnerFields"] ?? false); }
			set { ViewState["ShowOwnerFields"] = value; }
		}

		/// <summary>
		/// Zero based index for the current step. It is incremented for postback on Init.
		/// </summary>
		[Category("Behavior")] [Description("Zero based index for the current step. It is incremented for postback on Init.")]
		public int ActiveStepIndex
		{
			get { return (int)(ViewState["ActiveStepIndex"] ?? 0); }
			set { ViewState["ActiveStepIndex"] = value; }
		}

		/// <summary>
		/// The name of the context referenced when creating an Organization Service Context to be used by this control.
		/// </summary>
		[Category("Behavior")] [Description("The name of the context referenced when creating an Organization Service Context to be used by this control.")]
		public string ContextName
		{
			get { return ViewState["ContextName"] as string; }
			set { ViewState["ContextName"] = value; }
		}

		/// <summary>
		/// The name of the form on an entity to be loaded.
		/// </summary>
		[Category("Behavior")] [Description("The name of the form on an entity to be loaded.")]
		public string FormName
		{
			get { return ViewState["FormName"] as string; }
			set { ViewState["FormName"] = value; }
		}

		/// <summary>
		///The type of the form on an entity to be loaded.
		/// </summary>
		[Category("Behavior")]
		[Description("The type of the form on an entity to be loaded.")]
		[DefaultValue("false")]
		public bool? IsQuickForm
		{
			get { return ViewState["IsQuickForm"] as bool?; }
			set { ViewState["IsQuickForm"] = value; }
		}

		/// <summary>
		/// The name of newly created entity.
		/// </summary>
		[Category("EntityDisplayName")] [Description("The name of the newly created entity.")]
		public string EntityDisplayName
		{
			get { return _entityDisplayName; }
			set { _entityDisplayName = value; }
		}

		/// <summary>
		/// The description of the field in the metadata will be used as the text for fields' tooltip.
		/// </summary>
		[Category("Behavior")] [Description("The description of the field in the metadata will be used as the text for fields' tooltip.")]
		[DefaultValue("false")]
		public bool? ToolTipEnabled
		{
			get { return ViewState["ToolTipEnabled"] as bool?;  }
			set { ViewState["ToolTipEnabled"] = value; }
		}

		/// <summary>
		/// Fields with requirement level set to 'Business Recommended' can be made required when this value is set to true.
		/// </summary>
		[Category("Behavior")] [Description("Fields with requirement level set to 'Business Recommended' can be made required when this value is set to true.")]
		[DefaultValue("false")]
		public bool? RecommendedFieldsRequired
		{
			get { return ViewState["RecommendedFieldsRequired"] as bool?; }
			set { ViewState["RecommendedFieldsRequired"] = value; }
		}

		/// <summary>
		/// Web Resources by default are rendered in an iframe. Specify a value of true will remove the iframe and render the web resource inline.
		/// </summary>
		[Category("Behavior")] [Description("Web Resources by default are rendered in an iframe. Specify a value of true will remove the iframe and render the web resource inline.")]
		[DefaultValue("false")]
		public bool? RenderWebResourcesInline
		{
			get { return ViewState["RenderWebResourcesInline"] as bool?; }
			set { ViewState["RenderWebResourcesInline"] = value; }
		}

		/// <summary>
		/// Text to be assigned to validator control that renders next to the control that is invalid during validation.
		/// </summary>
		[Category("Behavior")] [Description("Text to be assigned to validator control that renders next to the control that is invalid during validation.")]
		[DefaultValue("*")]
		public string ValidationText
		{
			get { return ViewState["ValidationText"] as string; }
			set { ViewState["ValidationText"] = value; }
		}

		/// <summary>
		/// Mode of the form; 'FormViewMode.Insert', 'FormViewMode.Edit' or 'FormViewMode.ReadOnly'. FormViewMode.Insert is the default mode.
		/// </summary>
		[Category("Behavior")] [Description("Mode of the form; 'Insert', 'Edit' or 'ReadOnly'. Insert is the default mode.")]
		public FormViewMode? Mode
		{
			get { return ViewState["Mode"] as FormViewMode? ?? FormViewMode.Insert; }
			set { ViewState["Mode"] = value; }
		}

		[Category("Behavior")] [Description("Allows rebinding of data on postback.")] [DefaultValue("false")]
		public bool DataBindOnPostBack
		{
			get { return (bool)(ViewState["DataBindOnPostBack"] ?? false); }
			set { ViewState["DataBindOnPostBack"] = value; }
		}

		/// <summary>
		/// Creates step templates from tabs on a form.
		/// </summary>
		[Category("Behavior")] [Description("Creates step templates from tabs on a form.")] [DefaultValue("true")]
		public bool AutoGenerateSteps
		{
			get { return (bool)(ViewState["AutoGenerateSteps"] ?? true); }
			set { ViewState["AutoGenerateSteps"] = value; }
		}

		[Category("Appearance")] [Description("The Css Class assigned to the Previous button.")] [DefaultValue("button previous")]
		public string PreviousButtonCssClass
		{
			get { return ((string)ViewState["PreviousButtonCssClass"]) ?? "button previous"; }
			set { ViewState["PreviousButtonCssClass"] = value; }
		}

		[Category("Appearance")] [Description("The Css Class assigned to the Next button.")] [DefaultValue("button next")]
		public string NextButtonCssClass
		{
			get { return ((string)ViewState["NextButtonCssClass"]) ?? "button next"; }
			set { ViewState["NextButtonCssClass"] = value; }
		}

		[Category("Appearance")] [Description("The Css Class assigned to the Submit button.")] [DefaultValue("button submit")]
		public string SubmitButtonCssClass
		{
			get { return ((string)ViewState["SubmitButtonCssClass"]) ?? "button submit"; }
			set { ViewState["SubmitButtonCssClass"] = value; }
		}

		[Category("Appearance")] [Description("The label of the Previous button in a multi-step form.")] [DefaultValue("Previous")]
		public string PreviousButtonText
		{
			get 
			{ 
				var text = (string)ViewState["PreviousButtonText"];
				return string.IsNullOrWhiteSpace(text) ? DefaultPreviousStepTemplate.DefaultPreviousButtonText : text;
			}
			set { ViewState["PreviousButtonText"] = value; }
		}

		[Category("Appearance")] [Description("The label of the Next button in a multi-step form.")] [DefaultValue("Next")]
		public string NextButtonText
		{
			get
			{
				var text = (string)ViewState["NextButtonText"];
				return string.IsNullOrWhiteSpace(text) ? DefaultNextStepTemplate.DefaultNextButtonText : text;
			}
			set { ViewState["NextButtonText"] = value; }
		}

		[Category("Appearance")] [Description("The label of the Submit button.")] [DefaultValue("Submit")]
		public string SubmitButtonText
		{
			get
			{
				var text = (string)ViewState["SubmitButtonText"];
				return string.IsNullOrWhiteSpace(text) ? DefaultSubmitButtonText : text;
			}
			set { ViewState["SubmitButtonText"] = value; }
		}

		/// <summary>
		/// Marks all fields as required if set to true.
		/// </summary>
		[Category("Behavior")] [Description("Marks all fields as required if set to true.")] [DefaultValue(false)]
		public bool ForceAllFieldsRequired
		{
			get { return (bool)(ViewState["ForceAllFieldsRequired"] ?? false); }
			set { ViewState["ForceAllFieldsRequired"] = value; }
		}

		/// <summary>
		/// Enable (default) or disable the rendering of anchor links in the validation summary.
		/// </summary>
		[Category("Behavior")] [Description("Enable (default) or disable the rendering of anchor links in the validation summary.")] [DefaultValue(true)]
		public bool EnableValidationSummaryLinks
		{
			get { return (bool)(ViewState["EnableValidationSummaryLinks"] ?? true); }
			set { ViewState["EnableValidationSummaryLinks"] = value; }
		}

		/// <summary>
		/// The text of the links to control anchors of the validation summary items.
		/// </summary>
		[Category("Appearance")] [Description("The CSS Class Name to be appliced to the validation summary.")] [DefaultValue("validation-summary")]
		public string ValidationSummaryCssClass
		{
			get
			{
				var text = (string)ViewState["ValidationSummaryCssClass"];
				return string.IsNullOrWhiteSpace(text) ? DefaultValidationSummaryCssClass : text;
			}
			set { ViewState["ValidationSummaryCssClass"] = value; }
		}

		[Category("Appearance")] [Description("The header text for the validation summary.")]
		[DefaultValue("The form could not be submitted for the following reasons:")]
		public string ValidationSummaryHeaderText
		{
			get
			{
				var text = (string)ViewState["ValidationHeaderText"];
				return string.IsNullOrWhiteSpace(text) ? DefaultValidationHeaderText : text;
			}
			set { ViewState["ValidationHeaderText"] = value; }
		}

		/// <summary>
		/// Indicates if a message should be displayed when the user attempts to close the browser or refresh the page while changes have not been saved.
		/// </summary>
		[Category("Behavior")]
		[Description("Indicates if a message should be displayed when the user attempts to close the browser or refresh the page while changes have not been saved.")]
		[DefaultValue(false)]
		public bool ConfirmOnExit
		{
			get { return (bool)(ViewState["ConfirmOnExit"] ?? false); }
			set { ViewState["ConfirmOnExit"] = value; }
		}

		/// <summary>
		/// Message to be displayed if ConfirmOnExit is true and the user attempts to close the browser or refresh the page while changes have not been saved.
		/// </summary>
		[Category("Behavior")]
		[Description("Message to be displayed if ConfirmOnExit is true and the user attempts to close the browser or refresh the page while changes have not been saved.")]
		[DefaultValue("")]
		public string ConfirmOnExitMessage
		{
			get { return ViewState["ConfirmOnExitMessage"] as string; }
			set { ViewState["ConfirmOnExitMessage"] = value; }
		}


		/// <summary>
		/// Allows for a collection of Mapping field names to be programmatically passed to CrmEntityFormView. specifics fields used for Bing Mapping.
		/// </summary>
		public MappingFieldMetadataCollection MappingFieldCollection { get; set; }

		/// <summary>
		/// Gets or sets the name of the snippet record that contains the access denied message for the read permission.
		/// </summary>
		[Description("Name of the snippet record that contains the access denied message for the read permission.")]
		[DefaultValue("EntitySecurity/Record/ReadAccessDeniedMessage")]
		public string ReadAccessDeniedSnippetName
		{
			get
			{
				var text = (string)ViewState["ReadAccessDeniedSnippetName"];
				return string.IsNullOrWhiteSpace(text) ? DefaultReadAccessDeniedSnippetName : text;
			}
			set { ViewState["ReadAccessDeniedSnippetName"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the snippet record that contains the access denied message for the write permission.
		/// </summary>
		[Description("Name of the snippet record that contains the access denied message for the write permission.")]
		[DefaultValue("EntitySecurity/Record/WriteAccessDeniedMessage")]
		public string WriteAccessDeniedSnippetName
		{
			get
			{
				var text = (string)ViewState["WriteAccessDeniedSnippetName"];

				return string.IsNullOrWhiteSpace(text) ? DefaultWriteAccessDeniedSnippetName : text;
			}
			set
			{
				ViewState["WriteAccessDeniedSnippetName"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the snippet record that contains the access denied message for the create permission.
		/// </summary>
		[Description("Name of the snippet record that contains the access denied message for the create permission.")]
		[DefaultValue("EntitySecurity/Record/CreateAccessDeniedMessage")]
		public string CreateAccessDeniedSnippetName
		{
			get
			{
				var text = (string)ViewState["CreateAccessDeniedSnippetName"];
				return string.IsNullOrWhiteSpace(text) ? DefaultCreateAccessDeniedSnippetName : text;
			}
			set { ViewState["CreateAccessDeniedSnippetName"] = value; }
		}

		/// <summary>
		/// Collection of Web Form Metadata.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
		public IEnumerable<Entity> WebFormMetadata;

		/// <summary>
		/// Collection of form fields.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
		public Collection<CrmEntityFormViewField> Fields
		{
			get
			{
				var fields = ViewState["Fields"] as Collection<CrmEntityFormViewField>;

				if (fields == null)
				{
					fields = new Collection<CrmEntityFormViewField>();
					ViewState["Fields"] = fields;
				}

				return fields;
			}
		}

		/// <summary>
		/// Collection of messages.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
		public Collection<CrmEntityFormViewMessage> Messages
		{
			get
			{
				var messages = ViewState["Messages"] as Collection<CrmEntityFormViewMessage>;

				if (messages == null)
				{
					messages = new Collection<CrmEntityFormViewMessage>();
					ViewState["Messages"] = messages;
				}

				return messages;
			}
		}

		/// <summary>
		/// Template used to render a read-only form.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public virtual ITemplate ReadOnlyItemTemplate
		{
			get { return _readOnlyItemTemplate ?? new DefaultReadOnlyItemTemplate(); }
			set { _readOnlyItemTemplate = value; }
		}

		/// <summary>
		/// Template used to render a create form.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public new ITemplate InsertItemTemplate
		{
			get { return _insertItemTemplate ?? new UI.CrmEntityFormView.DefaultInsertItemTemplate(ValidationGroup, SubmitButtonText, SubmitButtonCssClass); }
			set { _insertItemTemplate = value; }
		}

		/// <summary>
		/// Template used to render an edit form.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public ITemplate UpdateItemTemplate
		{
			get { return _updateItemTemplate ?? new DefaultUpdateItemTemplate(ValidationGroup, string.IsNullOrWhiteSpace(SubmitButtonText) ? DefaultUpdateItemTemplate.DefaultUpdateButtonText : SubmitButtonText, SubmitButtonCssClass); }
			set { _updateItemTemplate = value; }
		}

		/// <summary>
		/// Template used to render an action for navigating to the next step.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public ITemplate NextStepTemplate
		{
			get { return _nextStepTemplate ?? new DefaultNextStepTemplate(ValidationGroup, NextButtonText, NextButtonCssClass); }
			set { _nextStepTemplate = value; }
		}

		/// <summary>
		/// Template used to render an action for navigating to the previous step.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string)null)]
		public ITemplate PreviousStepTemplate
		{
			get { return _previousStepTemplate ?? new DefaultPreviousStepTemplate(PreviousButtonText, PreviousButtonCssClass); }
			set { _previousStepTemplate = value; }
		}

		/// <summary>
		/// Collection of settings to get a view and configure its display.
		/// </summary>
		[PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false), DefaultValue(null)]
		public FormConfiguration FormConfiguration;

		/// <summary>
		/// CellTemplateFactory class type name to create an instance of.
		/// </summary>
		public new string CellTemplateFactoryType
		{
			get { return _cellTemplateFactoryType; }
			set { _cellTemplateFactoryType = value; }
		}

		internal int StepCount
		{
			get
			{
				if (_stepCount == null)
				{
					_stepCount = GetStepTemplates().ToList().Count;
				}
				return (int)_stepCount;
			}
		}

		protected Entity CrmEntity { get; set; }

		private HiddenField _entityNameHiddenField;

		private HiddenField _entityIdHiddenField;

		private HiddenField _entityStateHiddenField;

		private HiddenField _entityStatusHiddenField;

		private HtmlGenericControl _formConfigurationHtmlGenericControl;

		public int Baseorganizationlanguagecode
		{
			get
			{
				return this.Context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;
			}
		}

		protected HiddenField EntityNameField
		{
			get
			{
				if (_entityNameHiddenField != null) return _entityNameHiddenField;
				return this.FindControl(string.Format("{0}_EntityName", ID)) as HiddenField;
			}
			set { _entityNameHiddenField = value; }
		}

		protected HiddenField EntityIdField
		{
			get
			{
				if (_entityIdHiddenField != null) return _entityIdHiddenField;
				return this.FindControl(string.Format("{0}_EntityID", ID)) as HiddenField;
			}
			set { _entityIdHiddenField = value; }
		}

		protected HiddenField EntityStateField
		{
			get
			{
				if (_entityStateHiddenField != null) return _entityStateHiddenField;
				return this.FindControl(string.Format("{0}_EntityState", ID)) as HiddenField;
			}
			set { _entityStateHiddenField = value; }
		}

		protected HiddenField EntityStatusField
		{
			get
			{
				if (_entityStatusHiddenField != null) return _entityStatusHiddenField;
				return this.FindControl(string.Format("{0}_EntityStatus", ID)) as HiddenField;
			}
			set { _entityStatusHiddenField = value; }
		}

		protected HtmlGenericControl FormConfigurationField
		{
			get
			{
				if (_formConfigurationHtmlGenericControl != null) return _formConfigurationHtmlGenericControl;
				return this.FindControl(string.Format("{0}_EntityLayoutConfig", ID)) as HtmlGenericControl;
			}
			set { _formConfigurationHtmlGenericControl = value; }
		}

		public virtual PortalViewContext GetPortalViewContext()
		{
			return
				new PortalViewContext(
					new PortalContextDataAdapterDependencies(PortalCrmConfigurationManager.CreatePortalContext(ContextName),
						ContextName, Context.Request.RequestContext));
		}

		///<summary>
		/// Trigger the update of the form view
		///</summary>
		public virtual void UpdateItem()
		{
			HandleUpdate(null);
		}

		///<summary>
		/// Trigger the insertion of the form view
		///</summary>
		public override void InsertItem()
		{
			HandleInsert(null);
		}

		public virtual ICellTemplateFactory CellTemplateFactory { get; set; }

		protected new virtual ICellTemplateFactory CreateCellTemplateFactory()
		{
			var factoryType = Type.GetType(CellTemplateFactoryType, true, true);

			return (ICellTemplateFactory)Activator.CreateInstance(factoryType);
		}

		protected override void CreateChildControls()
		{
			CssClass = string.Join(" ", new[] { "entity-form", CssClass }).TrimEnd(' ');

			if (string.IsNullOrEmpty(EntityName)) throw new InvalidOperationException("EntityName can't be null or empty.");

			if (Mode == FormViewMode.Insert && !EvaluateEntityPermissions(CrmEntityPermissionRight.Create))
			{
				AddAccessDeniedSnippet(CreateAccessDeniedSnippetName);

				var button = Parent.FindControl("InsertButton") ?? Parent.FindControl("NextButton");

				if (button != null) { button.Visible = false; } return;
			}

			AddConfigFields();
			
			if (ConfirmOnExit)
			{
				var confirmOnExitControl = FindControl("confirmOnExit");
				if (confirmOnExitControl == null)
				{
					Controls.Add(new HiddenField { ID = "confirmOnExit", ClientIDMode = ClientIDMode.Static, Value = "true" });
					Controls.Add(new HiddenField { ID = "confirmOnExitMessage", ClientIDMode = ClientIDMode.Static, Value = ConfirmOnExitMessage });
				}
			}

			var validationSummary = new ValidationSummary
			{
				ID = string.Format("ValidationSummary{0}", ID),
				CssClass = ValidationSummaryCssClass,
				ValidationGroup = ValidationGroup,
				DisplayMode = ValidationSummaryDisplayMode.BulletList,
				HeaderText = "<h4 class='validation-header'><span role='presentation' class='fa fa-info-circle'></span> " + ValidationSummaryHeaderText + "</h4>"
			};
			validationSummary.Attributes["role"] = "alert";
            validationSummary.Attributes["tabIndex"] = "0";
            validationSummary.Attributes["aria-label"] = ValidationSummaryHeaderText;

            Controls.Add(validationSummary);

			Control container = this;

			if (string.IsNullOrEmpty(FormName) || !AutoGenerateSteps)
			{
				GetFormTemplate().InstantiateIn(this);

				switch (Mode)
				{
					case FormViewMode.Edit:
						UpdateItemTemplate.InstantiateIn(container);
						break;
					case FormViewMode.Insert:
						InsertItemTemplate.InstantiateIn(container);
						break;
					case FormViewMode.ReadOnly:
						CssClass = string.Join(" ", new[] { "form-readonly", CssClass }).TrimEnd(' ');
						ReadOnlyItemTemplate.InstantiateIn(container);
						MakeControlsReadonly(this);
						break;
				}
				return;
			}

			var stepIndex = 0;

			var steps = GetStepTemplates();

			foreach (var template in steps)
			{
				var stepContainer = new StepContainer(stepIndex++);
				Controls.Add(stepContainer);
				template.InstantiateIn(stepContainer);
			}

			var previousStepContainer = new StepNavigationContainer(StepNavigationType.Previous);
			container.Controls.Add(previousStepContainer);
			PreviousStepTemplate.InstantiateIn(previousStepContainer);

			var nextStepContainer = new StepNavigationContainer(StepNavigationType.Next);
			container.Controls.Add(nextStepContainer);
			NextStepTemplate.InstantiateIn(nextStepContainer);

			var submitStepContainer = new StepNavigationContainer(StepNavigationType.Submit);
			container.Controls.Add(submitStepContainer);

			switch (Mode)
			{
				case FormViewMode.Edit:
					UpdateItemTemplate.InstantiateIn(submitStepContainer);
					break;
				case FormViewMode.Insert:
					InsertItemTemplate.InstantiateIn(submitStepContainer);
					break;
				case FormViewMode.ReadOnly:
					CssClass = string.Join(" ", new[] { "form-readonly", CssClass }).TrimEnd(' ');
					ReadOnlyItemTemplate.InstantiateIn(submitStepContainer);
					MakeControlsReadonly(this);
					break;
			}
		}

		private void AddConfigFields()
		{
			EntityNameField = new HiddenField { ID = string.Format("{0}_EntityName", ID), ClientIDMode = ClientIDMode.Static, Value = EntityName };
			
			Controls.Add(EntityNameField);

			EntityIdField = new HiddenField { ID = string.Format("{0}_EntityID", ID), ClientIDMode = ClientIDMode.Static };

			Controls.Add(EntityIdField);

			Guid crmEntityId;
			if (CrmEntityId != null && Guid.TryParse(CrmEntityId.ToString(), out crmEntityId))
			{
				EntityIdField.Value = crmEntityId.ToString();
			}

			EntityStateField = new HiddenField { ID = string.Format("{0}_EntityState", ID), ClientIDMode = ClientIDMode.Static };

			Controls.Add(EntityStateField);

			EntityStatusField = new HiddenField { ID = string.Format("{0}_EntityStatus", ID), ClientIDMode = ClientIDMode.Static };

			Controls.Add(EntityStatusField);

			if (CrmEntity != null && CrmEntity.Attributes.ContainsKey("statecode"))
			{
				var stateCode = ((OptionSetValue)CrmEntity.Attributes["statecode"]).Value;

				var statusCode = ((OptionSetValue)CrmEntity.Attributes["statuscode"]).Value;

				EntityStateField.Value = stateCode.ToString();

				EntityStatusField.Value = statusCode.ToString();
			}

			FormConfigurationField = new HtmlGenericControl("span") { ID = string.Format("{0}_EntityLayoutConfig", ID), ClientIDMode = ClientIDMode.Static };

			Controls.Add(FormConfigurationField);

			if (FormConfiguration != null)
			{
				var serviceContext = CrmConfigurationManager.CreateContext(ContextName, true);

				if (CrmEntityId != null && Guid.TryParse(CrmEntityId.ToString(), out crmEntityId))
				{
					FormConfiguration.DisableActionsBasedOnPermissions(serviceContext, EntityName, Guid.Parse(CrmEntityId.ToString()));
				}

				var json = JsonConvert.SerializeObject(FormConfiguration);

				FormConfigurationField.Attributes["data-form-layout"] = json;
			}
		}

		private void AddAccessDeniedSnippet(string snippetName)
		{
			var accessDeniedSnippet = new Snippet
				{
					SnippetName = snippetName,
					DisplayName = snippetName,
					EditType = "html",
					Editable = true,
					DefaultText =
						"<div class='alert alert-block alert-danger'><span class='fa fa-lock' aria-hidden='true'></span>" + ResourceManager.GetString("Access_Denied_Error") + "</div>"
				};

			Controls.Add(accessDeniedSnippet);
		}

		protected override ITemplate GetFormTemplate()
		{
			if (string.IsNullOrWhiteSpace(FormName) && string.IsNullOrWhiteSpace(SavedQueryName))
			{
				return new EmptyTemplate();
			}

			var context = CrmConfigurationManager.CreateContext(ContextName, true);

			var cellTemplateFactory = CellTemplateFactory ?? CreateCellTemplateFactory();

			cellTemplateFactory.Initialize(this, Fields, formXmlCellMetadataFactory, CellBindings, LanguageCode, ValidationGroup, ShowUnsupportedFields,
				ToolTipEnabled, RecommendedFieldsRequired, ValidationText, ContextName, RenderWebResourcesInline, WebFormMetadata, ForceAllFieldsRequired, EnableValidationSummaryLinks, ConvertMessageCollectionToDictionary(), ShowOwnerFields, Baseorganizationlanguagecode);

			if (!string.IsNullOrEmpty(SavedQueryName))
			{
				cellTemplateFactory.Initialize(this, new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.SavedQueryCellMetadataFactory(), CellBindings, LanguageCode, ValidationGroup, ShowUnsupportedFields);

				var fetch = new Fetch
				{
					Entity = new FetchEntity("savedquery")
					{
						Filters = new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition("name", ConditionOperator.Equal, this.SavedQueryName),
									new Condition("returnedtypecode", ConditionOperator.Equal, this.EntityName)
								}
							}
						}
					}
				};

				var savedQuery = context.RetrieveSingle(fetch);

				if (savedQuery == null)
				{
					throw new ArgumentException("A saved query named {0} couldn't be found for the entity {1}.".FormatWith(FormName, EntityName));
				}

				var layoutXml = savedQuery.GetAttributeValue<string>("layoutxml");

				var rows = XDocument.Parse(layoutXml).XPathSelectElements("grid/row");

				var rowTemplates = rows.Select(r => new Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.SavedQueryRowTemplate(r, LanguageCode, EntityMetadata, cellTemplateFactory));

				return new CompositeTemplate(rowTemplates);
			}

			if (!string.IsNullOrWhiteSpace(FormName))
			{
				var filterExpression = new FilterExpression();
				filterExpression.FilterOperator = LogicalOperator.And;
				filterExpression.Conditions.Add(new ConditionExpression("type", ConditionOperator.NotNull));
				filterExpression.Conditions.Add(new ConditionExpression("objecttypecode", ConditionOperator.Equal, EntityName));

				filterExpression = AddNameCondition(filterExpression, FormName);

				if (IsQuickForm.GetValueOrDefault())
				{
					filterExpression.Conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, 6));
				}
				else
				{
					filterExpression.Conditions.Add(new ConditionExpression("type", ConditionOperator.NotEqual, 5));
				}

				var systemForms = Adxstudio.Xrm.Metadata.OrganizationServiceContextExtensions.GetMultipleSystemFormsWithAllLabels(filterExpression, context);

				var systemForm = PickLocalizedForm(systemForms);

				if (systemForm == null)
				{
					throw new ArgumentException("A form named {0} couldn't be found on the entity {1}.".FormatWith(FormName, EntityName));
				}

				var formXml = systemForm.GetAttributeValue<string>("formxml");

				if (!AutoGenerateSteps)
				{
					IEnumerable<XElement> tabs;

					if (string.IsNullOrWhiteSpace(TabName))
					{
						tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();

						var tabTemplates = tabs.Select(t => new TabTemplate(t, LanguageCode, EntityMetadata, cellTemplateFactory, WebFormMetadata) { MappingFieldCollection = MappingFieldCollection });

						return new CompositeTemplate(tabTemplates);
					}

					tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab[@name='" + TabName + "']").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();

					if (!tabs.Any())
					{
						tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab[labels[label[@description='" + TabName + "']]]").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();
					}

					// fallback to showing all tabs if the TabName could not be found
					if (!tabs.Any())
					{
						tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();
					}

					var tab = tabs.FirstOrDefault();
					var template = new TabTemplate(tab, LanguageCode, EntityMetadata, cellTemplateFactory, WebFormMetadata) { MappingFieldCollection = MappingFieldCollection };
					return new CompositeTemplate(Enumerable.Repeat(template, 1));
				}

				if (!string.IsNullOrEmpty(TabName))
				{
					var sections = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true"))
					.SelectMany(tab => tab.XPathSelectElements("columns/column/sections/section"));

					var rowTemplateFactory = new TableLayoutRowTemplateFactory(LanguageCode);

					var sectionTemplates = sections.Select(s => new SectionTemplate(s, LanguageCode, EntityMetadata, cellTemplateFactory, rowTemplateFactory));

					return new CompositeTemplate(sectionTemplates);
				}
			}

			return new EmptyTemplate();
		}

		/// <summary>
		/// Picks one SystemForm entity from the collection, depending on current locale. Nullable if collection contains no elements.
		/// </summary>
		/// <param name="systemForms">DataCollection of SystemForm entities.</param>
		/// <returns>Returns SystemForm entity epending on current locale. Returns null if collection contains no elements.</returns>
		private static Entity PickLocalizedForm(DataCollection<Entity> systemForms)
		{
			var isJapanese = CultureInfo.CurrentUICulture.LCID == LocaleIds.Japanese;
			if (!isJapanese)
			{
				var systemForm = systemForms.FirstOrDefault();

				return systemForm;
			}
			else
			{
				var japaneseSpecificForm =
					systemForms.FirstOrDefault(x => ((string)x.Attributes["name"]).EndsWith(JapaneseFormPostfix));

				var mainForm = systemForms.FirstOrDefault(x => !((string)x.Attributes["name"]).EndsWith(JapaneseFormPostfix));

				var systemForm = japaneseSpecificForm ?? mainForm;

				return systemForm;
			}

		}

		/// <summary>
		/// Adds filter condition to filter expression to find entity form by name. Takes into account that Japanese localisation requires special form.
		/// </summary>
		/// <param name="filterExpression">filterExpression which should have name condition.</param>
		/// <param name="formName">Name of the form.</param>
		/// <returns>Returns modified filterExpression.</returns>
		private FilterExpression AddNameCondition(FilterExpression filterExpression, string formName)
		{
			var isJapanese = CultureInfo.CurrentUICulture.LCID == LocaleIds.Japanese;
			if (!isJapanese)
			{
				filterExpression.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, formName));
			}
			else
			{
				var localizedFormNameFilter = filterExpression.AddFilter(LogicalOperator.Or);
				localizedFormNameFilter.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, formName));
				localizedFormNameFilter.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, formName + JapaneseFormPostfix));
			}

			return filterExpression;
		}

		protected virtual IEnumerable<ITemplate> GetStepTemplates()
		{
			var context = CrmConfigurationManager.CreateContext(ContextName, true);

			var filterExpression = new FilterExpression();
			filterExpression.FilterOperator = LogicalOperator.And;
			filterExpression.Conditions.Add(new ConditionExpression("type", ConditionOperator.NotNull));
			filterExpression.Conditions.Add(new ConditionExpression("type", ConditionOperator.NotEqual, 5));
			filterExpression.Conditions.Add(new ConditionExpression("objecttypecode", ConditionOperator.Equal, EntityName));

			filterExpression = AddNameCondition(filterExpression, FormName);

			var systemForms = Adxstudio.Xrm.Metadata.OrganizationServiceContextExtensions.GetMultipleSystemFormsWithAllLabels(filterExpression, context);

			var systemForm = PickLocalizedForm(systemForms);

			if (systemForm == null)
			{
				throw new ArgumentException("A form named {0} couldn't be found on the entity {1}.".FormatWith(FormName, EntityName));
			}

			var formXml = systemForm.GetAttributeValue<string>("formxml");

			IEnumerable<XElement> tabs;

			const string xPathSelectAllTabs = "form/tabs/tab";

			if (string.IsNullOrWhiteSpace(TabName))
			{
				tabs = XDocument.Parse(formXml).XPathSelectElements(xPathSelectAllTabs).Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();
			}
			else
			{
				tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab[@name='" + TabName + "']").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();

				if (!tabs.Any())
				{
					tabs = XDocument.Parse(formXml).XPathSelectElements("form/tabs/tab[labels[label[@description='" + TabName + "']]]").Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();
				}

				// fallback to showing all tabs if the TabName could not be found
				if (!tabs.Any())
				{
					tabs = XDocument.Parse(formXml).XPathSelectElements(xPathSelectAllTabs).Where(t => t.HasAttributes && (t.Attribute("visible") == null || t.Attribute("visible").Value == "true")).ToList();
				}
			}

			var cellTemplateFactory = CellTemplateFactory ?? CreateCellTemplateFactory();

			cellTemplateFactory.Initialize(this, Fields, formXmlCellMetadataFactory, CellBindings, LanguageCode, ValidationGroup, ShowUnsupportedFields,
				ToolTipEnabled, RecommendedFieldsRequired, ValidationText, ContextName, RenderWebResourcesInline, WebFormMetadata, ForceAllFieldsRequired, EnableValidationSummaryLinks, ConvertMessageCollectionToDictionary(), ShowOwnerFields, Baseorganizationlanguagecode);

			if (EntityMetadata == null) EntityMetadata = context.RetrieveEntity(EntityName, EntityFilters.Attributes);

			if (!AutoGenerateSteps)
			{
				var tab = tabs.FirstOrDefault();
				var template = new TabTemplate(tab, LanguageCode, EntityMetadata, cellTemplateFactory, WebFormMetadata) { MappingFieldCollection = MappingFieldCollection };
				return Enumerable.Repeat(template, 1);
			}
			return tabs.Select(tab => new TabTemplate(tab, LanguageCode, EntityMetadata, cellTemplateFactory, WebFormMetadata) { MappingFieldCollection = MappingFieldCollection });
		}

		protected override void OnInit(EventArgs e)
		{
			var context = CrmConfigurationManager.CreateContext(ContextName, true);

			EntityMetadata = context.RetrieveEntity(EntityName, EntityFilters.Attributes);

			// Entity forms only supports CRM languages, so use the CRM Lcid rather than the potentially custom language Lcid.
			LanguageCode = Context.GetCrmLcid();

			_portalViewContext = new Lazy<PortalViewContext>(GetPortalViewContext, LazyThreadSafetyMode.None);
		}

		protected override void OnItemCommand(CommandEventArgs args)
		{
			switch (args.CommandName)
			{
				case "Next":
					if (!Page.IsValid) return;
					ActiveStepIndex++;
					break;
				case "Previous":
					ActiveStepIndex--;
					break;
				case "Insert":
					HandleInsert(args);
					break;
				case "Update":
					HandleUpdate(args);
					break;
				default:
					base.OnItemCommand(args);
					break;
			}
		}

		protected virtual void OnItemUpdated(CrmEntityFormViewUpdatedEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewUpdatedEventArgs>)Events[_eventItemUpdated];

			if (handler != null) handler(this, args);
		}

		protected virtual void OnItemUpdating(CrmEntityFormViewUpdatingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewUpdatingEventArgs>)Events[_eventItemUpdating];

			if (handler != null) handler(this, args);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			var steps = Controls.OfType<StepContainer>().ToList();

			foreach (var step in steps)
			{
				step.Visible = step.StepIndex == ActiveStepIndex;
			}

			var firstStep = ActiveStepIndex == 0;
			var lastStep = ActiveStepIndex == (steps.Count() - 1);

			foreach (var navigationControl in Controls.OfType<StepNavigationContainer>())
			{
				navigationControl.Visible =
					(!firstStep && navigationControl.NavigationType == StepNavigationType.Previous)
					|| (!lastStep && navigationControl.NavigationType == StepNavigationType.Next)
					|| (lastStep && navigationControl.NavigationType == StepNavigationType.Submit);
			}
		}

		protected override void PerformDataBinding(IEnumerable data)
		{
			base.PerformDataBinding(data);

			if (Mode == FormViewMode.Insert)
			{
				foreach (var key in CellBindings.Keys)
				{
					if (key.Contains("CrmEntityId"))
					{
						BindQuickView(key, Guid.Empty);
					}
				}
			}

			if (data == null || (Page.IsPostBack && !DataBindOnPostBack)) return;

			var dataItems = data.Cast<Entity>().ToList();

			if (dataItems.Count() != 1) throw new NotSupportedException("The {0} must be bound to one data item. Either the record doesn't exist or the record ID is null.".FormatWith(GetType().FullName));

			EnsureChildControls();

			var dataItem = dataItems.First();

			foreach (var key in CellBindings.Keys)
			{
				if (key.Contains("CrmEntityId"))
				{
					BindQuickView(key, dataItem.Id);
					continue;
				}
				if (key == "fullname_fullname" || (key.StartsWith("address") && key.EndsWith("composite")))
				{
					CellBindings[key].Set(dataItem);
				}
				else if (Mode != FormViewMode.ReadOnly && key == "fullname_firstname" || key == "fullname_lastname")
				{
					CellBindings[key].Set(dataItem);
				}
				else if (Mode != FormViewMode.ReadOnly && (key.StartsWith("address") && key.Contains("composite")))
				{
					CellBindings[key].Set(dataItem);
				}
				else
				{
					var value = dataItem.GetAttributeValue(key);

					if (value == null) continue;

					CellBindings[key].Set(GetCellValue(dataItem, value));
				}
			}

			CrmEntityId = dataItem.Id;

			CrmEntity = dataItem;

			EntityIdField.Value = CrmEntityId.ToString();

			if (CrmEntity.Attributes.ContainsKey("statecode"))
			{
				var stateCode = ((OptionSetValue)CrmEntity.Attributes["statecode"]).Value;

				var statusCode = ((OptionSetValue)CrmEntity.Attributes["statuscode"]).Value;

				// Toggle form Mode based on state

				if (Mode == FormViewMode.Edit)
				{
					if (stateCode != 0)
					{
						// Record is not active
						Mode = FormViewMode.ReadOnly;
						Controls.Clear();
						CreateChildControls();
						PerformDataBinding(data);
						return;
					}
				}

				EntityStateField.Value = stateCode.ToString();

				EntityStatusField.Value = statusCode.ToString();
			}

			// End - Toggle form Mode based on state

			var right = CrmEntityPermissionRight.Write;
			var accessDeniedSnippetName = WriteAccessDeniedSnippetName;

			var serviceContext = CrmConfigurationManager.CreateContext(ContextName, true);

			if (FormConfiguration != null)
			{
				FormConfiguration.DisableActionsBasedOnPermissions(serviceContext, EntityName, Guid.Parse(CrmEntityId.ToString()));

				FormConfiguration.DisableActionsBasedOnFilterCriteria(serviceContext, EntityName, Guid.Parse(CrmEntityId.ToString()));

				var json = JsonConvert.SerializeObject(FormConfiguration);

				FormConfigurationField.Attributes["data-form-layout"] = json;
			}

			switch (Mode)
			{
				case FormViewMode.Insert:
					return;
				case FormViewMode.Edit:
					right = CrmEntityPermissionRight.Write;
					if (EvaluateEntityPermissions(right)) return;
					Mode = FormViewMode.ReadOnly;
					Controls.Clear();
					CreateChildControls();
					PerformDataBinding(data);
					return;
				case FormViewMode.ReadOnly:
					right = CrmEntityPermissionRight.Read;
					accessDeniedSnippetName = ReadAccessDeniedSnippetName;
					if (EvaluateEntityPermissions(right)) return;
					break;
			}
			
			Controls.Clear();

			var button = Parent.FindControl("UpdateButton") ?? Parent.FindControl("NextButton");

			if (button != null) { button.Visible = false; }

			AddAccessDeniedSnippet(accessDeniedSnippetName);
		}

		private void BindQuickView(string key, Guid id)
		{
			CellBindings[key].Set(id);
		}

		protected virtual bool EvaluateEntityPermissions(CrmEntityPermissionRight right)
		{
			if (!EnableEntityPermissions || !AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return true;
			}

			var serviceContext = CrmConfigurationManager.CreateContext(ContextName, true);
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			return right == CrmEntityPermissionRight.Create ? crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Create, EntityName)
				: crmEntityPermissionProvider.TryAssert(serviceContext, right, CrmEntity);

		}

		private object GetCellValue(Entity dataItem, object value)
		{
			var moneyValue = value as Money;

			// Inlude the record data along with currency values, so we can retrieve the currency for the record.
			if (moneyValue != null)
			{
				return new Tuple<Entity, Money>(dataItem, moneyValue);
			}

			return value;
		}

		private void AddErrorMessageToValidationSummary(string errorSnippetName, string defaultErrorMessage)
		{
			if (string.IsNullOrWhiteSpace(errorSnippetName))
			{
				return;
			}

			var snippet = PortalViewContext.Snippets.Select(errorSnippetName);

			var errorMessage = (snippet == null || snippet.Value == null || snippet.Value.Value == null ||
											string.IsNullOrWhiteSpace(snippet.Value.Value.ToString()))
				? defaultErrorMessage
				: snippet.Value.Value.ToString();

			AddErrorMessageToValidationSummary(errorMessage);
		}

		private void AddErrorMessageToValidationSummary(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			var customValidator = new CustomValidator
			{
				IsValid = false,
				ErrorMessage = message,
				ValidationGroup = ValidationGroup
			};

			Page.Validators.Add(customValidator);
		}

		private enum Operation
		{
			Insert = 1,
			Update = 2
		}

		private bool AssertEntityPermissionsOnInsert(Dictionary<string, object> values)
		{
			return AssertEntityPermissions(Operation.Insert, values);
		}

		private bool AssertEntityPermissionsOnUpdate(Dictionary<string, object> values, EntityReference entityReference)
		{
			return AssertEntityPermissions(Operation.Update, values, entityReference);
		}

		private bool AssertEntityPermissions(Operation operation, Dictionary<string, object> values, EntityReference entityReference = null)
		{
			if (!EnableEntityPermissions)
			{
				return true;
			}

			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(ContextName);
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider(ContextName);
			var rightGranted = false;

			switch (operation)
			{
				case Operation.Insert:
					rightGranted = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Create, EntityName);
					break;
				case Operation.Update:
					rightGranted = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Write, entityReference, EntityMetadata);
					break;
			}

			if (!rightGranted)
			{
				return false;
			}

			// Determine if lookups are being assigned values and if there are any applicable entity permission rules that require assertion

			var lookups = values.Where(e => e.Value != null && e.Value is EntityReference).Select(e => new KeyValuePair<string, EntityReference>(e.Key, (EntityReference)e.Value)).ToArray();

			if (!lookups.Any())
			{
				return true;
			}

			var entityMetadata = serviceContext.RetrieveEntity(EntityName, EntityFilters.Relationships);
			var manyToOneRelationships = entityMetadata.ManyToOneRelationships;

			foreach (var lookup in lookups)
			{
				var lookupEntityReference = lookup.Value;
				var lookupAttributeLogicalName = lookup.Key;
				var relationship = manyToOneRelationships.FirstOrDefault(e => e.ReferencedEntity == lookupEntityReference.LogicalName && e.ReferencingAttribute == lookupAttributeLogicalName);

				if (relationship == null)
				{
					continue;
				}

				var assertAssociate = operation == Operation.Insert
					? crmEntityPermissionProvider.IsAssociateAssertRequiredOnInsert(EntityName, relationship.SchemaName)
					: crmEntityPermissionProvider.IsAssociateAssertRequiredOnUpdate(EntityName, relationship.SchemaName);

				if (assertAssociate)
				{
					var canAssociate = false;

					switch (operation)
					{
						case Operation.Insert:
							var appendTo = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.AppendTo, EntityName);
							var append = crmEntityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Append, lookupEntityReference);
							canAssociate = appendTo && append;
							break;
						case Operation.Update:
							var lookupRelationship = new Relationship(relationship.SchemaName);
							canAssociate = crmEntityPermissionProvider.TryAssertAssociation(serviceContext, entityReference, lookupRelationship, lookupEntityReference);
							break;
					}
					
					if (!canAssociate)
					{
						return false;
					}
				}
			}

			return true;
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

			//Changing keys of fullname control fields to contact entity keys
			InsertFullNameControlValues(values);
			InsertAddressCompositeControlValues(values);

			// Remove the fields that are rendered as readonly or disabled to avoid readonly values being saved to CRM.
			RemoveReadOnlyCells(values);

			// Remove null entitlement field when creating a case entity as CRM adds the attribute to the entity and this causes duplicate keys
			if (EntityName == "incident")
			{
				RemoveNullEntitlement(values);
			}

			object entityDisplayName;

			if (EntityMetadata != null
				&& !string.IsNullOrEmpty(EntityMetadata.PrimaryNameAttribute)
				&& values.TryGetValue(EntityMetadata.PrimaryNameAttribute, out entityDisplayName)
				&& entityDisplayName != null)
			{
				EntityDisplayName = entityDisplayName.ToString();
			}

			if (!AssertEntityPermissionsOnInsert(values))
			{
				AddErrorMessageToValidationSummary(CreateAccessDeniedSnippetName, ResourceManager.GetString("Access_Denied_Error"));

				return;
			}

			var insertingEventArgs = new CrmEntityFormViewInsertingEventArgs(values);

			OnItemInserting(insertingEventArgs);

			if ((!Page.IsValid) || insertingEventArgs.Cancel) return;

			var dataSourceView = dataSource.GetView(DataMember);

			AddDataSourceViewInserted(dataSourceView as Microsoft.Xrm.Portal.Web.UI.WebControls.CrmDataSourceView, values);
			AddDataSourceViewInserted(dataSourceView as CrmDataSourceView, values);
		}

		private void AddDataSourceViewInserted(Microsoft.Xrm.Portal.Web.UI.WebControls.CrmDataSourceView dataSourceView, IDictionary values)
		{
			if (dataSourceView == null) return;

			dataSourceView.Inserted += DataSourceViewInserted;

			dataSourceView.Insert(values, EntityName);
		}

		private void AddDataSourceViewInserted(CrmDataSourceView dataSourceView, IDictionary values)
		{
			if (dataSourceView == null) return;

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

		private void HandleUpdate(CommandEventArgs args)
		{
			var dataSource = GetDataSource();

			if (dataSource == null)
			{
				throw new InvalidOperationException("Control must have a data source.");
			}

			Guid crmEntityId;

			if (CrmEntityId == null || !Guid.TryParse(CrmEntityId.ToString(), out crmEntityId))
			{
				throw new InvalidOperationException("Control must have a data source.");
			}

			var entityReference = new EntityReference(EntityName, crmEntityId);

			var keys = new Dictionary<string, object>
			{
				{ "ID", CrmEntityId },
				{ "Name", EntityName }
			};

			// Expand the cell bindings to retrieve all form values.
			var values = CellBindings.ToDictionary(cell => cell.Key, cell => cell.Value.Get());

			// Remove the fields that are rendered as readonly or disabled to avoid readonly values being saved to CRM.
			RemoveReadOnlyCells(values);

			//Changing keys of fullname control fields to contact entity keys
			InsertFullNameControlValues(values);
			InsertAddressCompositeControlValues(values);

			//For Write-in OpportunityProduct, the productid will be null. In the update operation, when the attribute list contains productid, the platform is triggering a plug-in assuming 
			// that the productid is changed and the validation within the plug-in is failing while converting the null value into Guid. So, removing the productid attribute from attribute list in Write-in scenario.
			if (EntityName == "opportunityproduct")
			{
				RemoveNullProduct(values);
			}

			//For opportunity, when parentcontactid(Account) attribute is not dirty and the parentcontactid(Contact) is modified, passing parentcontactid with null value being assumed as parentcontactid change and platform validation is failing. 
			//Removing the parentcontactid from the attribute list when it is not dirty and null.
			if (EntityName == "opportunity")
			{
				RemoveNullAccountOnOpportunity(values, crmEntityId);
			}

			if (!AssertEntityPermissionsOnUpdate(values, entityReference))
			{
				AddErrorMessageToValidationSummary(WriteAccessDeniedSnippetName, ResourceManager.GetString("Access_Denied_Error"));

				return;
			}

			var updatingEventArgs = new CrmEntityFormViewUpdatingEventArgs(values);

			OnItemUpdating(updatingEventArgs);

			if ((!Page.IsValid) || updatingEventArgs.Cancel) return;

			var dataSourceView = dataSource.GetView(DataMember);

			AddDataSourceViewUpdated(dataSourceView as Microsoft.Xrm.Portal.Web.UI.WebControls.CrmDataSourceView, keys, values);
			AddDataSourceViewUpdated(dataSourceView as CrmDataSourceView, keys, values);
		}

		private void AddDataSourceViewUpdated(Microsoft.Xrm.Portal.Web.UI.WebControls.CrmDataSourceView dataSourceView, IDictionary keys, IDictionary values)
		{
			if (dataSourceView == null) return;

			dataSourceView.Updated += DataSourceViewUpdated;

			dataSourceView.Update(keys, values, null);
		}

		private void AddDataSourceViewUpdated(CrmDataSourceView dataSourceView, IDictionary keys, IDictionary values)
		{
			if (dataSourceView == null) return;

			dataSourceView.Updated += DataSourceViewUpdated;

			dataSourceView.Update(keys, values, null);
		}

		private void DataSourceViewUpdated(object sender, CrmDataSourceViewUpdatedEventArgs e)
		{
			var updatedEventArgs = new CrmEntityFormViewUpdatedEventArgs
			{
				Entity = e.Entity,
				Exception = e.Exception,
				ExceptionHandled = e.ExceptionHandled
			};

			OnItemUpdated(updatedEventArgs);
		}

		private Dictionary<string, string> ConvertMessageCollectionToDictionary()
		{
			if (Messages == null || !Messages.Any()) { return new Dictionary<string, string>(); }
			return Messages.ToDictionary(m => m.MessageType, m => m.FormatString, StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>
		///  Remove cell values that are readonly at form level
		/// </summary>
		/// <param name="values"></param>
		private void RemoveReadOnlyCells(IDictionary<string, object> values)
		{
			foreach (var key in values.Select(kvp => kvp.Key).ToList())
			{
				FormXmlCellMetadata metadata;

				if (!formXmlCellMetadataFactory.TryGetCellMetadata(key, out metadata)) continue;

				var isReadonly = Mode == FormViewMode.ReadOnly || metadata.ReadOnly || metadata.Disabled;

				if (isReadonly) values.Remove(key);
			}
		}

		private void RemoveNullEntitlement(IDictionary<string, object> values)
		{
			if (values.ContainsKey("entitlementid") && values["entitlementid"] == null)
			{
				values.Remove("entitlementid");
			}
		}

		private void RemoveNullProduct(IDictionary<string, object> values)
		{
			if (values.ContainsKey("productdescription") && values["productdescription"] != null && !string.IsNullOrWhiteSpace(values["productdescription"].ToString()) && values.ContainsKey("productid") && values["productid"] == null)
			{
				values.Remove("productid");
			}
		}

		private void RemoveNullAccountOnOpportunity(IDictionary<string, object> values, Guid entityId)
		{
			if (values.ContainsKey("parentaccountid") && values["parentaccountid"] == null && values.ContainsKey("parentcontactid") && values["parentcontactid"] != null)
			{
				var serviceContext = CrmConfigurationManager.CreateContext(ContextName, true);

				EntityReference accountEntityReference = null;
				var entityRetrieveResponse = (RetrieveResponse)serviceContext.Execute(new RetrieveRequest() { ColumnSet = new ColumnSet(new string[] { "parentaccountid" }), Target = new EntityReference("opportunity", entityId) });

				if (null != entityRetrieveResponse && null != entityRetrieveResponse.Entity)
				{
					accountEntityReference = entityRetrieveResponse.Entity.GetAttributeValue<EntityReference>("parentaccountid");
				}

				if (accountEntityReference == null)
				{
					values.Remove("parentaccountid");
				}
			}
		}

		private void InsertAddressCompositeControlValues(IDictionary<string, object> values)
		{
			var adressControlvalues = values.Keys.Where(key => key.StartsWith("address") && key.Contains("composite")).ToList();

			//Selecting list of Address Composite Controls
			var controls = (from key in adressControlvalues let keySplit = key.Split('_') where keySplit.Length == 2 select key).ToList();
			foreach (var control in controls)
			{
				FormXmlCellMetadata fieldMetadata;
				this.formXmlCellMetadataFactory.TryGetCellMetadata(control, out fieldMetadata);
				// Don't update data if control is readonly
				if (fieldMetadata == null || fieldMetadata.ReadOnly) continue;

				var controlPrefix = control.Split('_').FirstOrDefault();

				foreach (var key in adressControlvalues)
				{
					var fieldKey = key.Split(new[] { "composite" }, StringSplitOptions.RemoveEmptyEntries).Last();
					if (string.IsNullOrEmpty(fieldKey) || fieldKey.Contains("composite") || !fieldKey.Contains(controlPrefix)) continue;

					var fieldValue = values[key];
					values.Remove(key);
					values.Remove(fieldKey);
					values.Add(fieldKey, fieldValue.ToString());
				}
			}
		}

		private void InsertFullNameControlValues(IDictionary<string, object> values)
		{
			FormXmlCellMetadata fieldMetadata;
			this.formXmlCellMetadataFactory.TryGetCellMetadata("fullname", out fieldMetadata);

			if (fieldMetadata == null || fieldMetadata.ReadOnly) { return; }

			var fullNameControlKeys = values.Keys.Where(key => key.StartsWith("fullname")).ToList();

			foreach (var field in fullNameControlKeys)
			{
				var nameField = field.Split('_').LastOrDefault();
				object fieldValue;
				if (values.TryGetValue(field, out fieldValue) && !fieldMetadata.ReadOnly && nameField != "fullname")
				{
					values.Remove(field);
					values.Remove(nameField);
					values.Add(nameField, fieldValue.ToString());
				}
			}
		}

		private static void MakeControlsReadonly(Control control)
		{
			var webControl = control as WebControl;
			if (webControl != null)
			{
				webControl.Attributes.Add("readonly", "readonly");
				if (webControl is RadioButtonList || webControl is DropDownList || webControl is CheckBoxList || webControl is RadioButton || webControl is CheckBox)
				{
					webControl.Enabled = false;
				}
			}

			foreach (Control child in control.Controls)
			{
				MakeControlsReadonly(child);
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			try { base.Render(writer); }
			catch (Exception e)
			{
				var ex = e;
				while (ex.InnerException != null) { ex = ex.InnerException; }
				writer.Write(
					"<div class='alert alert-block alert-danger'><p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> {0}</p></div>",
					Page.Server.HtmlEncode(ex.Message));
				RenderEndTag(writer);
			}
		}
	}

	public class CrmEntityFormViewUpdatedEventArgs : UI.CrmEntityFormView.CrmEntityFormViewUpdatedEventArgs { }

	public class CrmEntityFormViewUpdatingEventArgs : UI.CrmEntityFormView.CrmEntityFormViewUpdatingEventArgs { public CrmEntityFormViewUpdatingEventArgs(IDictionary<string, object> values) : base(values) { } }
}
