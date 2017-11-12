/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Configuration;
	using Microsoft.Xrm.Client.Messages;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Portal.Web.UI.WebControls;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using CancelEventArgs = System.ComponentModel.CancelEventArgs;
	using Adxstudio.Xrm.Activity;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.EntityForm;
	using Adxstudio.Xrm.Globalization;
	using Adxstudio.Xrm.Mapping;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc.Html;
	using Adxstudio.Xrm.Web.UI.CrmEntityFormView;
	using Adxstudio.Xrm.Web.UI.JsonConfiguration;
	using Adxstudio.Xrm.Web.UI.WebForms;

	/// <summary>
	/// Web Form control retrieves the Web Form record defined for the Web Page containing this control. Web Form definition record details the entity forms and workflow/logic within the CRM to facilitate data entry forms within the portal without the need for developer intervention.
	/// </summary>
	[Description("Web Form control retrieves the Web Form record defined for the Web Page containing this control. Web Form definition record details the entity forms and workflow/logic within the CRM to facilitate data entry forms within the portal without the need for developer intervention.")]
	[ToolboxData(@"<{0}:WebForm runat=""server""></{0}:WebForm>")]
	[DefaultProperty("")]
	public class WebForm : CompositeControl
	{
		private static readonly object _eventLoad = new object();
		private static readonly object _eventMovePrevious = new object();
		private static readonly object _eventSubmit = new object();
		private static readonly object _eventItemSaved = new object();
		private static readonly object _eventItemSaving = new object();
		private AnnotationSettings _annotationSettings;
		private AttachFileSaveOption _attachmentSaveOption;
		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		/// <summary>
		/// Event that occurs when the form has been loaded.
		/// </summary>
		public event EventHandler<WebFormLoadEventArgs> FormLoad
		{
			add { Events.AddHandler(_eventLoad, value); }
			remove { Events.RemoveHandler(_eventLoad, value); }
		}

		/// <summary>
		/// Event that occurs when the previous button has been clicked.
		/// </summary>
		public event EventHandler<WebFormMovePreviousEventArgs> MovePrevious
		{
			add { Events.AddHandler(_eventMovePrevious, value); }
			remove { Events.RemoveHandler(_eventMovePrevious, value); }
		}

		/// <summary>
		/// Event that occurs when the next/submit button has been clicked.
		/// </summary>
		public event EventHandler<WebFormSubmitEventArgs> Submit
		{
			add { Events.AddHandler(_eventSubmit, value); }
			remove { Events.RemoveHandler(_eventSubmit, value); }
		}

		/// <summary>
		/// Event that occurs when the record has been updated or inserted.
		/// </summary>
		public event EventHandler<WebFormSavedEventArgs> ItemSaved
		{
			add { Events.AddHandler(_eventItemSaved, value); }
			remove { Events.RemoveHandler(_eventItemSaved, value); }
		}

		/// <summary>
		/// Event that occurs immediately prior to updating or inserting the record.
		/// </summary>
		public event EventHandler<WebFormSavingEventArgs> ItemSaving
		{
			add { Events.AddHandler(_eventItemSaving, value); }
			remove { Events.RemoveHandler(_eventItemSaving, value); }
		}

		/// <summary>
		/// Gets or sets the Web Form Entity Reference.
		/// </summary>
		[Description("The Entity Reference of the Web Form to load.")]
		public EntityReference WebFormReference
		{
			get { return ((EntityReference)ViewState["WebFormReference"]); }
			set { ViewState["WebFormReference"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the portal configuration that the control binds to.
		/// </summary>
		[Description("The portal configuration that the control binds to.")]
		[DefaultValue("")]
		public string PortalName
		{
			get { return ((string)ViewState["PortalName"]) ?? string.Empty; }
			set { ViewState["PortalName"] = value; }
		}

		/// <summary>
		/// Gets or sets the Form's CSS Class.
		/// </summary>
		[Description("The CSS Class assigned to the Form.")]
		[DefaultValue("")]
		public string FormCssClass
		{
			get { return ((string)ViewState["FormCssClass"]) ?? string.Empty; }
			set { ViewState["FormCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the Previous button's CSS Class.
		/// </summary>
		[Description("The CSS Class assigned to the Previous button.")]
		[DefaultValue("button previous")]
		public string PreviousButtonCssClass
		{
			get { return ((string)ViewState["PreviousButtonCssClass"]) ?? "button previous"; }
			set { ViewState["PreviousButtonCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the Next button's CSS Class.
		/// </summary>
		[Description("The CSS Class assigned to the Next button.")]
		[DefaultValue("button next")]
		public string NextButtonCssClass
		{
			get { return ((string)ViewState["NextButtonCssClass"]) ?? "button next"; }
			set { ViewState["NextButtonCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the Submit button's CSS Class.
		/// </summary>
		[Description("The CSS Class assigned to the Submit button.")]
		[DefaultValue("button submit")]
		public string SubmitButtonCssClass
		{
			get { return ((string)ViewState["SubmitButtonCssClass"]) ?? "button submit"; }
			set { ViewState["SubmitButtonCssClass"] = value; }
		}

		/// <summary>
		/// Gets or sets the text of the Previous button.
		/// </summary>
		[Description("The label of the Previous button in a multi-step form.")]
		[DefaultValue("Previous")]
		public string PreviousButtonText
		{
			get
			{
				var text = (string)ViewState["PreviousButtonText"];
				return string.IsNullOrWhiteSpace(text) ? WebFormFunctions.DefaultPreviousButtonText : text;
			}
			set
			{
				ViewState["PreviousButtonText"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the text of the Next button.
		/// </summary>
		[Description("The label of the Next button in a multi-step form.")]
		[DefaultValue("Next")]
		public string NextButtonText
		{
			get
			{
				var text = (string)ViewState["NextButtonText"];
				return string.IsNullOrWhiteSpace(text) ? WebFormFunctions.DefaultNextButtonText : text;
			}
			set
			{
				ViewState["NextButtonText"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the text of the Submit button.
		/// </summary>
		[Description("The label of the Submit button.")]
		[DefaultValue("Submit")]
		public string SubmitButtonText
		{
			get
			{
				var text = (string)ViewState["SubmitButtonText"];
				return string.IsNullOrWhiteSpace(text) ? WebFormFunctions.DefaultSubmitButtonText : text;
			}
			set
			{
				ViewState["SubmitButtonText"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the text of the Submit button displayed when the page is busy submitting the form.
		/// </summary>
		[Description("The label of the Submit button when the page is busy submitting the form.")]
		public string SubmitButtonBusyText
		{
			get
			{
				var text = (string)ViewState["SubmitButtonBusyText"];
				return string.IsNullOrWhiteSpace(text) ? WebFormFunctions.DefaultSubmitButtonBusyText : text;
			}
			set
			{
				ViewState["SubmitButtonBusyText"] = value;
			}
		}

		/// <summary>
		/// Language Code
		/// </summary> 
		[Description("Language Code")]
		public int LanguageCode
		{
			get
			{
				return Context.GetCrmLcid();
			}
			set { }
		}

		public MappingFieldMetadataCollection MappingFieldCollection { get; set; }

		private bool PersistSessionHistory
		{
			get { return (bool)(ViewState["PersistSessionHistory"] ?? false); }
			set { ViewState["PersistSessionHistory"] = value; }
		}

		private bool StartNewSessionOnLoad
		{
			get { return (bool)(ViewState["StartNewSessionOnLoad"] ?? false); }
			set { ViewState["StartNewSessionOnLoad"] = value; }
		}

		private FormViewMode Mode
		{
			get { return (FormViewMode)((ViewState["Mode"]) ?? FormViewMode.Insert); }
			set { ViewState["Mode"] = value; }
		}

		private bool HideFormOnSuccess
		{
			get { return (bool)(ViewState["HideFormOnSuccess"] ?? true); }
			set { ViewState["HideFormOnSuccess"] = value; }
		}


		private static readonly string DefaultSuccessMessage = ResourceManager.GetString("Submission_Successful_Message");
		private static readonly string DefaultRecordNotFoundMessage = ResourceManager.GetString("Default_Record_NotFound_Message");
		private static readonly string DefaultEditExpiredMessage = ResourceManager.GetString("Submission_Already_Submitted_Message");
		private const FormViewMode DefaultFormViewMode = FormViewMode.Insert;

		protected Dictionary<string, AttributeTypeCode?> AttributeTypeCodeDictionary { get; private set; }
		private CrmSessionHistoryProvider SessionHistoryProvider { get; set; }

		/// <summary>
		/// Current Session History for the current user and web form.
		/// </summary>
		public SessionHistory CurrentSessionHistory
		{
			get { return (SessionHistory)(ViewState["CurrentSessionHistory"]); }
			protected set { ViewState["CurrentSessionHistory"] = value; }
		}

		/// <summary>
		/// Success message of the current step, typically the last step.
		/// </summary>
		public string SuccessMessage
		{
			get
			{
				var text = (string)ViewState["SuccessMessage"];

				return string.IsNullOrWhiteSpace(text) ? DefaultSuccessMessage : text;
			}
			set { ViewState["SuccessMessage"] = value; }
		}

		protected string LoadEventKeyName
		{
			get { return (string)(ViewState["LoadEventKeyName"] ?? string.Empty); }
			set { ViewState["LoadEventKeyName"] = value; }
		}

		protected string SubmitEventKeyName
		{
			get { return (string)(ViewState["SubmitEventKeyName"] ?? string.Empty); }
			set { ViewState["SubmitEventKeyName"] = value; }
		}

		protected string MovePreviousEventKeyName
		{
			get { return (string)(ViewState["MovePreviousEventKeyName"] ?? string.Empty); }
			set { ViewState["MovePreviousEventKeyName"] = value; }
		}

		protected string SavingEventKeyName
		{
			get { return (string)(ViewState["SavingEventKeyName"] ?? string.Empty); }
			set { ViewState["SavingEventKeyName"] = value; }
		}

		protected string SavedEventKeyName
		{
			get { return (string)(ViewState["SavedEventKeyName"] ?? string.Empty); }
			set { ViewState["SavedEventKeyName"] = value; }
		}

		protected bool SessionLoaded
		{
			get { return (bool)(ViewState["SessionLoaded"] ?? false); }
			set { ViewState["SessionLoaded"] = value; }
		}

		protected bool LoadExistingRecord
		{
			get { return (bool)(ViewState["LoadExistingRecord"] ?? false); }
			set { ViewState["LoadExistingRecord"] = value; }
		}

		protected bool SetStateOnSave
		{
			get { return (bool)(ViewState["SetStateOnSave"] ?? false); }
			set { ViewState["SetStateOnSave"] = value; }
		}

		protected int SetStateOnSaveValue
		{
			get { return (int)(ViewState["SetStateOnSaveValue"] ?? 0); }
			set { ViewState["SetStateOnSaveValue"] = value; }
		}

		protected virtual string[] ScriptIncludes
		{
			get
			{
#if TELERIKWEBUI
				return new[] {
					"~/xrm-adx/js/webform.js",
					"~/xrm-adx/js/radcaptcha.js"
				};
#else
				return new[] {
					"~/xrm-adx/js/webform.js"
				};
#endif
			}
		}

		/// <summary>
		/// Indicates whether or not the entity permission provider will assert privileges.
		/// </summary>
		protected bool EnableEntityPermissions
		{
			get { return (bool)(ViewState["EnableEntityPermissions"] ?? false); }
			set { ViewState["EnableEntityPermissions"] = value; }
		}

		protected string RecordNotFoundMessage
		{
			get { return (string)(ViewState["RecordNotFoundMessage"] ?? string.Empty); }
			set { ViewState["RecordNotFoundMessage"] = value; }
		}

		protected virtual bool EvaluateEntityPermissions(CrmEntityPermissionRight right, Entity entity)
		{
			if (!EnableEntityPermissions || !AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled) return true;

			var serviceContext = CrmConfigurationManager.CreateContext(PortalName, true);
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			return crmEntityPermissionProvider.TryAssert(serviceContext, right, entity);
		}

		/// <summary>
		/// Programmatically enable or disable the Next Button
		/// </summary>
		/// <remarks>Should be called from the Page PreRender event handler.</remarks>
		/// <param name="enable">Specify true to enable the control otherwise false will disable the control.</param>
		public void EnableDisableNextButton(bool enable)
		{
			var button = FindControl("NextButton");
			if (button != null)
			{
				var nextButton = (Button)button;
				nextButton.Enabled = enable;
			}
		}

		/// <summary>
		/// Programmatically enable or disable the Previous Button
		/// </summary>
		/// <remarks>Should be called from the Page PreRender event handler.</remarks>
		/// <param name="enable">Specify true to enable the control otherwise false will disable the control.</param>
		public void EnableDisablePreviousButton(bool enable)
		{
			var button = FindControl("PreviousButton");
			if (button != null)
			{
				var previousButton = (Button)button;
				previousButton.Enabled = enable;
			}
		}

		/// <summary>
		/// Programmatically show or hide the Next Button
		/// </summary>
		/// <remarks>Should be called from the Page PreRender event handler.</remarks>
		/// <param name="visible">Specify true to make the control visible otherwise false will hide the control.</param>
		public void ShowHideNextButton(bool visible)
		{
			var button = FindControl("NextButton");
			if (button != null)
			{
				var nextButton = (Button)button;
				nextButton.Visible = visible;
			}
		}

		/// <summary>
		/// Programmatically show or hide the Previous Button
		/// </summary>
		/// <remarks>Should be called from the Page PreRender event handler.</remarks>
		/// <param name="visible">Specify true to make the control visible otherwise false will hide the control.</param>
		public void ShowHidePreviousButton(bool visible)
		{
			var button = FindControl("PreviousButton");
			if (button != null)
			{
				var previousButton = (Button)button;
				previousButton.Visible = visible;
			}
		}

		/// <summary>
		/// Move to the previous step.
		/// </summary>
		public void MoveToPreviousStep()
		{
			var prevStepReferenceEntity = GetPreviousStepReferenceEntityDefinition();
			var prevStepEntitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(prevStepReferenceEntity.LogicalName, prevStepReferenceEntity.PrimaryKeyLogicalName, prevStepReferenceEntity.ID);
			var previousStepEventArgs = new WebFormMovePreviousEventArgs(prevStepEntitySourceDefinition, MovePreviousEventKeyName);

			OnMovePrevious(this, previousStepEventArgs);

			if (previousStepEventArgs.Cancel) return;

			MovePreviousStep();
		}

		/// <summary>
		/// Move to the next step.
		/// </summary>
		public void MoveToNextStep()
		{
			var referenceEntity = GetCurrentStepReferenceEntityDefinition();
			var entitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID);
			var submitEventArgs = new WebFormSubmitEventArgs(entitySourceDefinition, SubmitEventKeyName);

			OnSubmit(this, submitEventArgs);

			if (submitEventArgs.Cancel) return;

			if (submitEventArgs.EntityID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(submitEventArgs.EntityID);
			}

			MoveNextStep(true);
		}

		/// <summary>
		/// Move to the next step
		/// </summary>
		/// <param name="entityID">ID of the entity record created/updated</param>
		public void MoveToNextStep(Guid entityID)
		{
			var referenceEntity = GetCurrentStepReferenceEntityDefinition();
			var entitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID);
			var submitEventArgs = new WebFormSubmitEventArgs(entitySourceDefinition, SubmitEventKeyName);

			OnSubmit(this, submitEventArgs);

			if (submitEventArgs.Cancel) return;

			if (submitEventArgs.EntityID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(submitEventArgs.EntityID);
			}
			else if (entityID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(entityID);
			}

			MoveNextStep(true);
		}

		/// <summary>
		/// Move to the next step
		/// </summary>
		/// <param name="entityDefinition">Definition of the entity record created/updated</param>
		public void MoveToNextStep(WebForms.WebFormEntitySourceDefinition entityDefinition)
		{
			if (entityDefinition == null)
			{
				MoveNextStep(true);
				return;
			}
			var submitEventArgs = new WebFormSubmitEventArgs(entityDefinition, SubmitEventKeyName);
			OnSubmit(this, submitEventArgs);
			if (submitEventArgs.Cancel)
			{
				return;
			}
			if (submitEventArgs.EntityID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(submitEventArgs.EntityID);
			}
			else if (entityDefinition.ID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(entityDefinition.ID);
			}
			MoveNextStep(true);
		}

		/// <summary>
		/// Update the definition of the entity created/updated
		/// </summary>
		/// <param name="entityDefinition">Definition of the entity record created/updated</param>
		public void UpdateEntityDefinition(WebForms.WebFormEntitySourceDefinition entityDefinition)
		{
			if (entityDefinition == null)
			{
				return;
			}
			if (entityDefinition.ID != Guid.Empty)
			{
				UpdateStepHistoryReferenceEntityID(entityDefinition.ID);
			}
		}

		protected void OnItemCommand(CommandEventArgs args)
		{
			CrmEntityFormView formView;
			switch (args.CommandName)
			{
				case "MovePrevious":
					var prevStepReferenceEntity = GetPreviousStepReferenceEntityDefinition();
					var prevStepEntitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(prevStepReferenceEntity.LogicalName, prevStepReferenceEntity.PrimaryKeyLogicalName, prevStepReferenceEntity.ID);
					var previousStepEventArgs = new WebFormMovePreviousEventArgs(prevStepEntitySourceDefinition, MovePreviousEventKeyName);
					OnMovePrevious(this, previousStepEventArgs);
					if (previousStepEventArgs.Cancel) return;
					MovePreviousStep();
					break;
				case "MoveNext":
					if (!Page.IsValid) return;
					var referenceEntity = GetCurrentStepReferenceEntityDefinition();
					var entitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID);
					var submitEventArgs = new WebFormSubmitEventArgs(entitySourceDefinition, SubmitEventKeyName);
					OnSubmit(this, submitEventArgs);
					if (submitEventArgs.Cancel) return;
					if (submitEventArgs.EntityID != Guid.Empty)
					{
						UpdateSessionHistoryPrimaryRecordID(submitEventArgs.EntityID);
						UpdateStepHistoryReferenceEntityID(submitEventArgs.EntityID);
					}
					MoveNextStep(true);
					break;
				case "Update":
					if (!Page.IsValid) return;
					formView = (CrmEntityFormView)FindControl("EntityFormView");
					if (formView == null)
					{
						throw new ApplicationException("Couldn't find CrmEntityFormView control.");
					}
					formView.UpdateItem();
					break;
				case "Insert":
					if (!Page.IsValid) return;
					formView = (CrmEntityFormView)FindControl("EntityFormView");
					if (formView == null)
					{
						throw new ApplicationException("Couldn't find CrmEntityFormView control.");
					}
					formView.InsertItem();
					break;
				default:
					RaiseBubbleEvent(this, args);
					break;
			}
		}

		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			var commandEventArgs = args as CommandEventArgs;
			if (commandEventArgs != null)
			{
				OnItemCommand(commandEventArgs);
				return true;
			}
			return false;
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();

			Entity record;
			WebForms.WebFormEntitySourceDefinition entitySourceDefinition = null;
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var entity = portalContext.Entity;
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			SessionHistoryProvider = new CrmSessionHistoryProvider();
			Entity webform;

			if (WebFormReference != null)
			{
				webform = context.RetrieveSingle("adx_webform", "adx_webformid", this.WebFormReference.Id, FetchAttribute.All);

				if (webform == null)
				{
					throw new ApplicationException(string.Format("Error retrieving adx_webform where id equals {0}", WebFormReference.Id));
				}
			}
			else
			{
				if (entity.LogicalName != "adx_webpage")
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "The current entity must be of type adx_webpage. Please select the correct template for this entity type.");
					return;
				}

				// get the web form
				var webformReference = entity.GetAttributeValue<EntityReference>("adx_webform");

				if (webformReference == null)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Could not find an Web Form (adx_webform) value on Web Page (adx_webpage) where id equals {0}.", entity.Id));
					return;
				}

				webform = context.RetrieveSingle("adx_webform", "adx_webformid", webformReference.Id, FetchAttribute.All);

				if (webform == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not find an Web Form (adx_webpage_webform) value where id equals {0} on Web Page (adx_webpage) where id equals {1}.", webformReference.Id, entity.Id));
					return;
				}

				WebFormReference = webform.ToEntityReference();
			}

			if (webform.GetAttributeValue<bool?>("adx_authenticationrequired") ?? false)
			{
				RedirectToLoginIfAnonymous();
			}

			if (LanguageCode <= 0) LanguageCode = this.Context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode;

			var multipleRecordsPerUserPermitted = webform.GetAttributeValue<bool?>("adx_multiplerecordsperuserpermitted") ?? false;
			var editExpired = false;
			var editExpiredStateCode = webform.GetAttributeValue<int?>("adx_editexpiredstatecode");
			var editExpiredStatusCode = webform.GetAttributeValue<int?>("adx_editexpiredstatuscode");
			var localizedEditExpiredMessage = Localization.GetLocalizedString(webform.GetAttributeValue<string>("adx_editexpiredmessage"), LanguageCode);
			var editExpiredMessage = string.IsNullOrWhiteSpace(localizedEditExpiredMessage) ? DefaultEditExpiredMessage : localizedEditExpiredMessage;
			var startNewSessionOnLoad = webform.GetAttributeValue<bool?>("adx_startnewsessiononload") ?? false;
			StartNewSessionOnLoad = startNewSessionOnLoad;
			Guid stepid;
			var stepIdString = HttpContext.Current.Request["stepid"];
			var startStep = context.RetrieveRelatedEntity(webform, "adx_webform_startstep");
			if (startStep == null)
			{
				DisplayMessage(this, ResourceManager.GetString("Webform_StartStep_Missing_Exception"), "error alert alert-danger");
				return;
			}

			RegisterClientSideDependencies(this);

			var step = startStep;
			var nextStep = context.RetrieveRelatedEntity(
				step,
				new Relationship("adx_webformstep_nextstep") { PrimaryEntityRole = EntityRole.Referencing });

			switch (HttpContext.Current.Request["msg"])
			{
				case "edit-expired":
					DisplayMessage(this, editExpiredMessage, "alert");
					return;
				case "record-dne":
				case "source-invalid":
					var sourceStep = startStep;
					if (!string.IsNullOrWhiteSpace(stepIdString) && Guid.TryParse(stepIdString, out stepid))
					{
						var lastStep = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", stepid, new[] { new FetchAttribute("adx_recordnotfoundmessage"), });

						if (lastStep != null)
						{
							sourceStep = lastStep;
						}
					}
					var recordNotFoundMessage = Localization.GetLocalizedString(sourceStep.GetAttributeValue<string>("adx_recordnotfoundmessage"), LanguageCode);
					RecordNotFoundMessage = !string.IsNullOrWhiteSpace(recordNotFoundMessage) ? recordNotFoundMessage : DefaultRecordNotFoundMessage;
					DisplayMessage(this, RecordNotFoundMessage, "error alert alert-danger");
					return;
				case "success":
					if (!string.IsNullOrWhiteSpace(stepIdString) && Guid.TryParse(stepIdString, out stepid))
					{
						var lastStep = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", stepid, new[] { new FetchAttribute("adx_successmessage"), });

						if (lastStep != null)
						{
							var message = Localization.GetLocalizedString(lastStep.GetAttributeValue<string>("adx_successmessage"), LanguageCode);
							if (!string.IsNullOrWhiteSpace(message))
							{
								SuccessMessage = message;
							}
						}
					}
					DisplayMessage(this, SuccessMessage, "success alert alert-success");
					return;
			}

			PersistSessionHistory = (nextStep != null);

			if (CurrentSessionHistory == null)
			{
				if (PersistSessionHistory)
				{
					SessionHistory currentSessionHistory = null;

					if (startNewSessionOnLoad)
					{
						Guid sessionid;
						var sessionidString = HttpContext.Current.Request["sessionid"];
						if (!string.IsNullOrWhiteSpace(sessionidString) && Guid.TryParse(sessionidString, out sessionid))
						{
							SessionLoaded = TryGetSessionHistory(context, sessionid, out currentSessionHistory);
						}
					}
					else
					{
						SessionLoaded = TryGetSessionHistory(context, webform, startStep, out entitySourceDefinition, out record, out currentSessionHistory);
					}

					if (SessionLoaded) { CurrentSessionHistory = currentSessionHistory; }
				}
				else
				{
					// Check to see if a record has possibly been created previously
					TryGetPrimaryEntitySourceDefinition(context, startStep, out entitySourceDefinition, out record);
				}

				if (CurrentSessionHistory == null)
				{
					CurrentSessionHistory = InitializeCurrentSessionHistory(context, webform, startStep);
				}
			}

			if (!SessionLoaded)
			{
				// Session does not exist. Begin at start step

				var formMode = GetFormViewMode(startStep);

				if (entitySourceDefinition == null && (formMode == FormViewMode.Edit || formMode == FormViewMode.ReadOnly))
				{
					if (!TryGetPrimaryEntitySourceDefinition(context, startStep, out entitySourceDefinition, out record))
					{
						RedirectToSelf(new Dictionary<string, string> { { "msg", "source-invalid" } });
						return;
					}
				}
			}
			else
			{
				// Session exists.

				if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
				{
					CurrentSessionHistory = InitializeCurrentSessionHistory(context, webform, startStep);

					step = startStep;
				}
				else
				{
					step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All)
							?? startStep;

					if (entitySourceDefinition == null || step.Id != startStep.Id)
					{
						entitySourceDefinition = GetStepEntitySourceDefinition(context, step);
					}

					if (entitySourceDefinition == null)
					{
						entitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(CurrentSessionHistory.PrimaryRecord.LogicalName, CurrentSessionHistory.PrimaryRecord.PrimaryKeyLogicalName, CurrentSessionHistory.PrimaryRecord.ID);
					}
				}
			}

			var registerStartupScript = step.GetAttributeValue<string>("adx_registerstartupscript");

			if (!string.IsNullOrWhiteSpace(registerStartupScript))
			{
				var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);

				var control = new HtmlGenericControl() { };

				var script = html.ScriptAttribute(context, step, "adx_registerstartupscript");

				control.InnerHtml = script.ToString();

				Controls.Add(control);
			}

			var dataSource = CreateDataSource(entitySourceDefinition);

			// Determine if existing record can be edited

			var result = dataSource.Select();

			if (result != null)
			{
				if (SetupForEditExisting(result, editExpiredStateCode, editExpiredStatusCode, editExpired, context, startStep, multipleRecordsPerUserPermitted, webform, ref step, ref entitySourceDefinition)) return;
			}

			if (SessionLoaded && CurrentSessionHistory.CurrentStepId != startStep.Id)
			{
				// If entity source is to be from a previous step, we need to get the reference entity details from the step history except if the source is current portal user.
				var mode = step.GetAttributeValue<OptionSetValue>("adx_mode");
				var entitySourceType = step.GetAttributeValue<OptionSetValue>("adx_entitysourcetype");

				if ((mode != null && entitySourceType != null) && entitySourceType.Value == (int)WebFormStepSourceType.CurrentPortalUser)
				{
					// Current Portal User - do nothing as entity source should already be set.
				}
				else if ((mode != null && entitySourceType != null) && entitySourceType.Value == (int)WebFormStepSourceType.QueryString)
				{
					// Query string 
					UpdateStepHistoryReferenceEntityID(entitySourceDefinition.ID);
				}
				else
				{
					if (mode != null && entitySourceType != null)
					{
						if ((mode.Value == (int)WebFormStepMode.Edit || mode.Value == (int)WebFormStepMode.ReadOnly)
							&& entitySourceType.Value == (int)WebFormStepSourceType.ResultFromPreviousStep)
						{
							var entitySourceStep = step.GetAttributeValue<EntityReference>("adx_entitysourcestep");
							UpdateStepHistoryReferenceEntityID(entitySourceStep == null ? GetPreviousStepReferenceEntityID() : GetStepReferenceEntityID(entitySourceStep.Id));
						}
					}
					else if (step.GetAttributeValue<OptionSetValue>("adx_type") != null)
					{
						var stepType = step.GetAttributeValue<OptionSetValue>("adx_type");
						if (stepType != null)
						{
							if (stepType.Value == (int)WebFormStepType.LoadUserControl) // Load User Control
							{
								var entitySourceStep = step.GetAttributeValue<EntityReference>("adx_entitysourcestep");
								UpdateStepHistoryReferenceEntityID(entitySourceStep == null ? GetPreviousStepReferenceEntityID() : GetStepReferenceEntityID(entitySourceStep.Id));
							}
						}
					}

					if (GetCurrentStepReferenceEntityID() == Guid.Empty)
					{
						// Reflect on relationship name to determine if a related record already exists
						var relationshipName = step.GetAttributeValue<string>("adx_referenceentityrelationshipname") ?? string.Empty;

						if (!string.IsNullOrWhiteSpace(relationshipName))
						{
							var entitySourceStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");
							var sourceEntityDefinition = (entitySourceStep == null) ? GetPreviousStepReferenceEntityDefinition() : GetStepReferenceEntityDefinition(entitySourceStep.Id);

							if (sourceEntityDefinition != null && !string.IsNullOrWhiteSpace(sourceEntityDefinition.LogicalName) && !string.IsNullOrWhiteSpace(sourceEntityDefinition.PrimaryKeyLogicalName) && sourceEntityDefinition.ID != Guid.Empty)
							{
								var sourceEntity = context.RetrieveSingle(
									sourceEntityDefinition.LogicalName,
									sourceEntityDefinition.PrimaryKeyLogicalName,
									sourceEntityDefinition.ID,
									FetchAttribute.All);

								if (sourceEntity != null)
								{
									var targetEntity = context.RetrieveRelatedEntity(sourceEntity, relationshipName);

									if (targetEntity != null) { UpdateStepHistoryReferenceEntityID(targetEntity.Id); }
								}
							}
						}
					}

					var referenceEntity = GetCurrentStepReferenceEntityDefinition();

					entitySourceDefinition = new WebForms.WebFormEntitySourceDefinition(referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID);
				}
			}
			else
			{
				var referenceEntityDefinition = GetCurrentStepReferenceEntityDefinition();

				if (referenceEntityDefinition.ID == Guid.Empty && entitySourceDefinition != null && entitySourceDefinition.ID != Guid.Empty)
				{
					if (referenceEntityDefinition.LogicalName == entitySourceDefinition.LogicalName)
					{
						UpdateStepHistoryReferenceEntityID(entitySourceDefinition.ID);
					}
				}
			}

			EnableEntityPermissions = step.GetAttributeValue<bool?>("adx_entitypermissionsenabled").GetValueOrDefault(false);

			var notFoundMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_recordnotfoundmessage"), LanguageCode);
			RecordNotFoundMessage = !string.IsNullOrWhiteSpace(notFoundMessage) ? notFoundMessage : DefaultRecordNotFoundMessage;

			ProcessStep(context, webform, step, entitySourceDefinition);
		}

		private bool SetupForEditExisting(IEnumerable result, int? editExpiredStateCode, int? editExpiredStatusCode, bool editExpired,
			OrganizationServiceContext context, Entity startStep, bool multipleRecordsPerUserPermitted, Entity webform, ref Entity step,
			ref WebForms.WebFormEntitySourceDefinition entitySourceDefinition)
		{
			var existingRecord = result.Cast<Entity>().FirstOrDefault();

			if (existingRecord != null)
			{
				// Check if edit has expired
				var statecode = existingRecord.GetAttributeValue<OptionSetValue>("statecode");
				var statuscode = existingRecord.GetAttributeValue<OptionSetValue>("statuscode");

				if (editExpiredStateCode != null)
				{
					if (editExpiredStatusCode != null)
					{
						if (statecode != null && statuscode != null)
						{
							if (statecode.Value == editExpiredStateCode && statuscode.Value == editExpiredStatusCode) { editExpired = true; }
						}
					}
					else { if (statecode.Value == editExpiredStateCode) { editExpired = true; } }
				}

				if (!editExpired)
				{
					if (SessionLoaded)
					{
						step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All)
							?? startStep;
					}
					else
					{
						if (multipleRecordsPerUserPermitted && (Mode == FormViewMode.Insert || (entitySourceDefinition == null || entitySourceDefinition.ID == Guid.Empty)))
						{
							entitySourceDefinition = null;
							CurrentSessionHistory = InitializeCurrentSessionHistory(context, webform, startStep);
							step = startStep;
						}
						else
						{
							LoadExistingRecord = true;
							step = startStep;
						}
					}
				}
				else
				{
					if (multipleRecordsPerUserPermitted && (Mode == FormViewMode.Insert || (entitySourceDefinition == null || entitySourceDefinition.ID == Guid.Empty)))
					{
						// Start new form
						entitySourceDefinition = null;
						SessionLoaded = false;
						CurrentSessionHistory = InitializeCurrentSessionHistory(context, webform, startStep);
						step = startStep;
					}
					else
					{
						RedirectToSelf(new Dictionary<string, string> { { "msg", "edit-expired" } });
						return true;
					}
				}
			}
			return false;
		}

		protected FormViewMode GetFormViewMode(Entity step)
		{
			var formViewMode = DefaultFormViewMode;

			if (step == null) { throw new ArgumentNullException("step"); }

			step.AssertEntityName("adx_webformstep");

			var mode = step.GetAttributeValue<OptionSetValue>("adx_mode");

			if (mode != null)
			{
				switch (mode.Value)
				{
					case (int)WebFormStepMode.Insert:
						// Determine if mode must be changed to edit
						var targetEntityName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
						var referenceEntityLogicalName = string.Empty;
						var referenceEntityId = Guid.Empty;
						var referenceEntityDefinition = CurrentSessionHistory == null ? null : CurrentSessionHistory.CurrentStepIndex > 0 ? GetPreviousStepReferenceEntityDefinition() : null;
						if (referenceEntityDefinition != null)
						{
							referenceEntityLogicalName = referenceEntityDefinition.LogicalName;
							referenceEntityId = referenceEntityDefinition.ID;
						}
						var currentEntityDefinition = GetCurrentStepReferenceEntityDefinition();
						formViewMode = (targetEntityName == referenceEntityLogicalName && referenceEntityId != Guid.Empty) ||
										(SessionLoaded && currentEntityDefinition != null && currentEntityDefinition.ID != Guid.Empty) ||
										LoadExistingRecord ? FormViewMode.Edit : FormViewMode.Insert;
						break;
					case (int)WebFormStepMode.Edit:
						formViewMode = FormViewMode.Edit;
						break;
					case (int)WebFormStepMode.ReadOnly:
						formViewMode = FormViewMode.ReadOnly;
						break;
					default:
						formViewMode = DefaultFormViewMode;
						break;
				}
			}

			Mode = formViewMode;

			return formViewMode;
		}

		protected CrmDataSource CreateDataSource(WebForms.WebFormEntitySourceDefinition source)
		{
			var dataSource = new CrmDataSource { ID = string.Format("{0}_WebFormDataSource", ID), CrmDataContextName = PortalName, IsSingleSource = true };

			if (source == null || source.ID == Guid.Empty) return dataSource;

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", source.LogicalName, source.PrimaryKeyLogicalName, source.ID);

			dataSource.FetchXml = fetchXml;

			return dataSource;
		}

		protected void RenderForm(OrganizationServiceContext context, Entity webform, Entity step, FormViewMode mode, WebForms.WebFormEntitySourceDefinition entitySourceDefinition)
		{
			var stepObject = new WebFormStepObject(webform, step, LanguageCode, context);

			var showMovePreviousButton = false;
			var showMoveNextButton = false;

			var nextButtonCommandName = "MoveNext";

			var nextStepIsRedirect = false;

			HideFormOnSuccess = step.GetAttributeValue<bool?>("adx_hideformonsuccess") ?? true;

			SubmitButtonCssClass = stepObject.SubmitButtonCssClass;

			ClientIDMode = ClientIDMode.Static;

			// Toggle form Mode based on state and permissions

			if (mode == FormViewMode.Edit)
			{
				var record = context.RetrieveSingle(entitySourceDefinition.LogicalName, entitySourceDefinition.PrimaryKeyLogicalName, entitySourceDefinition.ID, FetchAttribute.All);

				if (record == null) return;

				var stateCode = ((OptionSetValue)record.Attributes["statecode"]).Value;

				if (stateCode != 0) Mode = mode = FormViewMode.ReadOnly;

				if (!EvaluateEntityPermissions(CrmEntityPermissionRight.Write, record)) { Mode = mode = FormViewMode.ReadOnly; }
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			var gridMetadataJson = step.GetAttributeValue<string>("adx_settings");

			FormActionMetadata formActionMetadata = null;
			if (!string.IsNullOrWhiteSpace(gridMetadataJson))
			{
				try
				{
					formActionMetadata = JsonConvert.DeserializeObject<FormActionMetadata>(gridMetadataJson,
						new JsonSerializerSettings
						{
							ContractResolver = JsonConfigurationContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Objects,
							Converters = new List<JsonConverter> { new JsonConfiguration.GuidConverter() },
							Binder = new ActionSerializationBinder()
						});
				}
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}
			}

			var formConfiguration = new FormConfiguration(portalContext, stepObject.EntityName, formActionMetadata, PortalName, LanguageCode, EnableEntityPermissions, stepObject.AutoGenerateStepsFromTabs);

			var formView = new CrmEntityFormView
			{
				ID = "EntityFormView",
				MappingFieldCollection = MappingFieldCollection,
				ClientIDMode = ClientIDMode.Static,
				EntityName = stepObject.EntityName,
				FormName = stepObject.FormName,
				ValidationGroup = stepObject.ValidationGroup,
				ValidationSummaryCssClass = stepObject.ValidationSummaryCssClass,
				EnableValidationSummaryLinks = stepObject.EnableValidationSummaryLinks,
				ValidationSummaryHeaderText = stepObject.LocalizedValidationSummaryHeaderText,
				ContextName = GetContextName(),
				Mode = mode,
				AutoGenerateSteps = stepObject.AutoGenerateStepsFromTabs,
				PreviousButtonCssClass = stepObject.PreviousButtonCssClass,
				NextButtonCssClass = stepObject.NextButtonCssClass,
				SubmitButtonCssClass = stepObject.SubmitButtonCssClass,
				PreviousButtonText = stepObject.PreviousButtonText,
				NextButtonText = stepObject.NextButtonText,
				SubmitButtonText = stepObject.SubmitButtonText,
				DataBindOnPostBack = true,
				WebFormMetadata = stepObject.WebFormMetadata,
				RecommendedFieldsRequired = stepObject.RecommendedFieldsRequired,
				ForceAllFieldsRequired = stepObject.ForceAllFieldsRequired,
				RenderWebResourcesInline = stepObject.RenderWebResourcesInline,
				ShowOwnerFields = stepObject.ShowOwnerFields,
				ShowUnsupportedFields = stepObject.ShowUnsupportedFields,
				ToolTipEnabled = stepObject.ToolTipEnabled,
				EnableEntityPermissions = EnableEntityPermissions,
				FormConfiguration = formConfiguration
			};

			var formPanel = new Panel { ID = "WebFormPanel" };
			var showProgress = webform.GetAttributeValue<bool?>("adx_progressindicatorenabled") ?? false;
			var progressPosition = webform.GetAttributeValue<OptionSetValue>("adx_progressindicatorposition");
			var progressControlPosition = string.Empty;

			if (!string.IsNullOrWhiteSpace(FormCssClass)) formPanel.CssClass = FormCssClass;

			progressControlPosition = GetProgressControlPosition(progressPosition, progressControlPosition);

			if (showProgress && PersistSessionHistory)
			{
				if (string.IsNullOrWhiteSpace(progressControlPosition) || progressControlPosition == "top" || progressControlPosition == "left" || progressControlPosition == "right")
				{
					RenderProgressIndicator(context, webform, this);

					if (progressControlPosition == "left") formPanel.CssClass += " right";

					if (progressControlPosition == "right") formPanel.CssClass += " left";
				}
			}

			if (!string.IsNullOrWhiteSpace(stepObject.Instructions))
			{
				var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);
				var instructionsContainer = new HtmlGenericControl("div") { InnerHtml = html.Liquid(stepObject.Instructions) };
				instructionsContainer.Attributes.Add("class", "instructions");
				Controls.Add(instructionsContainer);
			}

			var messagePanel = new Panel { ID = "MessagePanel", Visible = false, CssClass = "message alert" };
			messagePanel.Attributes.Add("role", "alert");
			var messageLabel = new System.Web.UI.WebControls.Label { ID = "MessageLabel", Text = string.Empty };
			messagePanel.Controls.Add(messageLabel);
			Controls.Add(messagePanel);

			SuccessMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_successmessage"), LanguageCode);

			if (stepObject.ConfirmOnExit)
			{
				var confirmOnExitControl = FindControl("confirmOnExit");
				if (confirmOnExitControl == null)
				{
					Controls.Add(new HiddenField { ID = "confirmOnExit", ClientIDMode = ClientIDMode.Static, Value = "true" });
					Controls.Add(new HiddenField { ID = "confirmOnExitMessage", ClientIDMode = ClientIDMode.Static, Value = stepObject.ConfirmOnExitMessage });
				}
			}

			Controls.Add(formPanel);

			if (!string.IsNullOrWhiteSpace(stepObject.TabName)) formView.TabName = stepObject.TabName;

			RenderReferenceEntityForm(context, step, formPanel);

			var dataSource = CreateDataSource(entitySourceDefinition);

			formPanel.Controls.Add(dataSource);

			formView.DataSourceID = dataSource.ID;

			if (stepObject.NextStep != null)
			{
				var type = stepObject.NextStep.GetAttributeValue<OptionSetValue>("adx_type");

				if (type != null) { if (type.Value == (int)WebFormStepType.Redirect) { nextStepIsRedirect = true; } }
			}

			if (CurrentSessionHistory.CurrentStepIndex > 0 && !stepObject.AutoGenerateStepsFromTabs && (step.GetAttributeValue<bool?>("adx_movepreviouspermitted") ?? true))
			{
				showMovePreviousButton = true;
			}

			if (!stepObject.AutoGenerateStepsFromTabs) showMoveNextButton = true;

			var location = stepObject.AttachFileStorageLocation == null
				? StorageLocation.CrmDocument
				: (StorageLocation)stepObject.AttachFileStorageLocation.Value;
			var accept = string.IsNullOrEmpty(stepObject.AttachFileAccept) ? "*/*" : stepObject.AttachFileAccept;
			var maxFileSize = stepObject.AttachFileRestrictSize && stepObject.AttachFileMaxSize.HasValue
				? Convert.ToUInt64(stepObject.AttachFileMaxSize) << 10
				: (ulong?)null;
			_annotationSettings = new AnnotationSettings(context, EnableEntityPermissions, location, accept,
				stepObject.AttachFileRestrictAccept, stepObject.AttachFileTypeErrorMessage, maxFileSize, stepObject.AttachFileSizeErrorMessage);

			_attachmentSaveOption = stepObject.AttachFileSaveOption == null ? AttachFileSaveOption.Notes : (AttachFileSaveOption)stepObject.AttachFileSaveOption.Value;

			var user = portalContext.User;

			switch (mode)
			{
				case FormViewMode.Insert:
					formView.ItemInserted += OnItemInserted;
					formView.ItemInserting += OnItemInserting;
					formView.InsertItemTemplate = new ItemTemplate(stepObject.ValidationGroup, stepObject.CaptchaRequired && (user == null || stepObject.ShowCaptchaForAuthenticatedUsers), stepObject.AttachFile, stepObject.AttachFileAllowMultiple,
					_annotationSettings.AcceptMimeTypes, _annotationSettings.RestrictMimeTypes,
					_annotationSettings.RestrictMimeTypesErrorMessage, _annotationSettings.MaxFileSize,
					_annotationSettings.MaxFileSize.HasValue, _annotationSettings.MaxFileSizeErrorMessage, stepObject.AttachFileLabel,
					stepObject.AttachFileRequired, stepObject.AttachFileRequiredErrorMessage, stepObject.AutoGenerateStepsFromTabs, "InsertButton", "Insert",
					stepObject.SubmitButtonText,
					(string.IsNullOrEmpty(stepObject.SubmitButtonCssClass) || stepObject.SubmitButtonCssClass == "button submit" || stepObject.SubmitButtonCssClass == "btn btn-primary") ? "btn btn-primary navbar-btn button submit-btn" : stepObject.SubmitButtonCssClass,
					true, stepObject.SubmitButtonBusyText);
					if (!stepObject.AutoGenerateStepsFromTabs)
					{
						formPanel.DefaultButton = "InsertButton";
					}
					nextButtonCommandName = "Insert";

					break;
				case FormViewMode.Edit:
					formView.ItemUpdating += OnItemUpdating;
					formView.ItemUpdated += OnItemUpdated;
					formView.UpdateItemTemplate = new ItemTemplate(stepObject.ValidationGroup, stepObject.CaptchaRequired && (user == null || stepObject.ShowCaptchaForAuthenticatedUsers), stepObject.AttachFile, stepObject.AttachFileAllowMultiple,
					_annotationSettings.AcceptMimeTypes, _annotationSettings.RestrictMimeTypes,
					_annotationSettings.RestrictMimeTypesErrorMessage, _annotationSettings.MaxFileSize,
					_annotationSettings.MaxFileSize.HasValue, _annotationSettings.MaxFileSizeErrorMessage, stepObject.AttachFileLabel,
					stepObject.AttachFileRequired, stepObject.AttachFileRequiredErrorMessage, stepObject.AutoGenerateStepsFromTabs, "UpdateButton", "Update",
					stepObject.SubmitButtonText,
					(string.IsNullOrEmpty(stepObject.SubmitButtonCssClass) || stepObject.SubmitButtonCssClass == "button submit" || stepObject.SubmitButtonCssClass == "btn btn-primary") ? "btn btn-primary navbar-btn button submit-btn" : stepObject.SubmitButtonCssClass,
					true, stepObject.SubmitButtonBusyText);
					if (!stepObject.AutoGenerateStepsFromTabs) { formPanel.DefaultButton = "UpdateButton"; }
					nextButtonCommandName = "Update";
					break;
				case FormViewMode.ReadOnly:
					if (stepObject.NextStep == null) { showMoveNextButton = false; }
					break;
			}

			if (LanguageCode > 0) formView.LanguageCode = LanguageCode;

			//Add Action Bar above Form
			if (formConfiguration != null && formConfiguration.TopFormActionLinks != null && formConfiguration.TopFormActionLinks.Any())
			{
				formPanel.Controls.Add(ActionButtonBarAboveForm(stepObject, formConfiguration));
			}

			formPanel.Controls.Add(formView);

			//var buttonContainer = new HtmlGenericControl("div");

			var buttonContainer = AddActionBarContainerIfApplicable(formConfiguration, stepObject, showMovePreviousButton, showMoveNextButton, nextButtonCommandName, nextStepIsRedirect, formPanel);

			if (showMoveNextButton || showMovePreviousButton)
			{
				formPanel.Controls.Add(buttonContainer);
			}

			if (showProgress && PersistSessionHistory && progressControlPosition == "bottom") { RenderProgressIndicator(context, webform, this); }

			PopulateReferenceEntityField(context, step, formView);

			ApplyStepMetadataPrepopulateValues(context, step, formView);

			OnFormLoad(this, new WebFormLoadEventArgs(entitySourceDefinition, LoadEventKeyName));
		}

		private static void AddNextButton(Control buttonContainer, string nextButtonCommandName, Entity nextStep,
			string submitButtonText, bool nextStepIsRedirect, string nextButtonText, string validationGroup,
			string submitButtonCssClass, string nextButtonCssClass, string submitButtonBusyText)
		{
			var placeHolder = new PlaceHolder();

			placeHolder.Controls.Add(new LiteralControl("<div role =\"group\" class=\"btn-group entity-action-button\" >"));

			var nextButton = new Button
			{
				ID = "NextButton",
				CommandName = nextButtonCommandName,
				Text = nextStep == null ? submitButtonText : nextStepIsRedirect ? submitButtonText : nextButtonText,
				ValidationGroup = validationGroup,
				CausesValidation = true,
				CssClass = nextStep == null ? submitButtonCssClass : nextStepIsRedirect ? submitButtonCssClass : nextButtonCssClass,
				OnClientClick = "javascript:if(typeof webFormClientValidate === 'function'){if(webFormClientValidate()){if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + submitButtonBusyText + "';}}else{return false;}}else{if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + submitButtonBusyText + "';}}",
				UseSubmitBehavior = false
			};

			if (string.IsNullOrEmpty(nextButton.CssClass) || nextButton.CssClass == "button next" || nextButton.CssClass == "button submit"
				|| nextButton.CssClass == "btn btn-primary")
			{
				nextButton.CssClass = "btn btn-primary button next submit-btn";
			}

			placeHolder.Controls.Add(nextButton);

			placeHolder.Controls.Add(new LiteralControl("</div>"));

			buttonContainer.Controls.Add(placeHolder);
		}

		private static void AddPreviousButton(string previousButtonText, string previousButtonCssClass, Control buttonContainer)
		{
			var placeHolder = new PlaceHolder();

			placeHolder.Controls.Add(new LiteralControl("<div role =\"group\" class=\"btn-group entity-action-button\" >"));

			var previousButton = new Button
			{
				ID = "PreviousButton",
				Text = previousButtonText,
				CommandName = "MovePrevious",
				CausesValidation = false,
				CssClass = previousButtonCssClass,
				UseSubmitBehavior = false
			};

			if (string.IsNullOrEmpty(previousButton.CssClass) || previousButton.CssClass == "button next" || previousButton.CssClass == "button previous"
				|| previousButton.CssClass == "btn btn-default")
			{
				previousButton.CssClass = "btn btn-default button previous previous-btn";
			}

			placeHolder.Controls.Add(previousButton);

			placeHolder.Controls.Add(new LiteralControl("</div>"));

			buttonContainer.Controls.Add(placeHolder);
		}

		private WebControl ActionButtonBarAboveForm(WebFormStepObject stepObject, FormConfiguration formConfiguration, string submitButtonID = "SubmitButton",
			string submitButtonCommandName = "", string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(formConfiguration.PortalName, Page.Request.RequestContext, Page.Response);

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			var rightContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Right);

			var navBar = FormActionControls.FormActionNoNavBar(html, formConfiguration, leftContainer, rightContainer, ActionButtonPlacement.AboveForm, submitButtonID, submitButtonCommandName,
					stepObject.ValidationGroup, stepObject.SubmitButtonBusyText, nextButtonId, previousButtonId);

			if (!string.IsNullOrEmpty(formConfiguration.TopContainerCssClass)) navBar.AddClass(formConfiguration.TopContainerCssClass);

			return navBar;
		}

		private Control AddActionBarContainerIfApplicable(FormConfiguration formConfiguration, WebFormStepObject stepObject,
			bool showMovePreviousButton, bool showMoveNextButton, string nextButtonCommandName, bool nextStepIsRedirect, Panel formPanel,
			string submitButtonID = "SubmitButton", string submitButtonCommandName = "")
		{
			if (formConfiguration != null && formConfiguration.BottomFormActionLinks != null
				&& formConfiguration.BottomFormActionLinks.Any())
			{
				var actionBar = ActionButtonBarBelowForm(formConfiguration, stepObject, showMovePreviousButton, showMoveNextButton, nextButtonCommandName, nextStepIsRedirect, formPanel, submitButtonID, submitButtonCommandName);

				return actionBar;
			}

			var container = new HtmlGenericControl("div");

			container.Attributes.Add("class", "actions");

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			if (showMovePreviousButton)
			{
				AddPreviousButton(stepObject.PreviousButtonText, stepObject.PreviousButtonCssClass, leftContainer);
			}

			if (showMoveNextButton)
			{
				AddNextButton(leftContainer, nextButtonCommandName, stepObject.NextStep, stepObject.SubmitButtonText, nextStepIsRedirect,
					stepObject.NextButtonText, stepObject.ValidationGroup, stepObject.SubmitButtonCssClass, stepObject.NextButtonCssClass,
					stepObject.SubmitButtonBusyText);

				formPanel.DefaultButton = "NextButton";
			}

			container.Controls.Add(leftContainer);

			return container;
		}

		private WebControl ActionButtonBarBelowForm(FormConfiguration formConfiguration, WebFormStepObject stepObject, bool showMovePreviousButton, bool showMoveNextButton,
			string nextButtonCommandName, bool nextStepIsRedirect, Panel formPanel, string submitButtonID = "SubmitButton", string submitButtonCommandName = "")
		{
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(formConfiguration.PortalName, Page.Request.RequestContext, Page.Response);

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			var rightContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Right);

			var dropDownLeft = FormActionControls.FormActions(html, formConfiguration, leftContainer, ActionButtonPlacement.BelowForm, ActionButtonAlignment.Left, submitButtonID, submitButtonCommandName, stepObject.ValidationGroup, stepObject.SubmitButtonBusyText);

			var dropDownRight = FormActionControls.FormActions(html, formConfiguration, rightContainer, ActionButtonPlacement.BelowForm, ActionButtonAlignment.Right, submitButtonID, submitButtonCommandName, stepObject.ValidationGroup, stepObject.SubmitButtonBusyText);

			var navBar = FormActionControls.FormActionNavbarContainerControl(formConfiguration);

			navBar.Controls.Add(dropDownLeft);

			navBar.Controls.Add(dropDownRight);

			FormActionControls.AddActionModalWindows(html, formConfiguration, navBar, ActionButtonPlacement.BelowForm);

			if (showMovePreviousButton)
			{
				AddPreviousButton(stepObject.PreviousButtonText, stepObject.PreviousButtonCssClass, dropDownLeft);
			}

			if (showMoveNextButton)
			{
				AddNextButton(dropDownLeft, nextButtonCommandName, stepObject.NextStep, stepObject.SubmitButtonText, nextStepIsRedirect,
					stepObject.NextButtonText, stepObject.ValidationGroup, stepObject.SubmitButtonCssClass, stepObject.NextButtonCssClass,
					stepObject.SubmitButtonBusyText);

				formPanel.DefaultButton = "NextButton";
			}

			if (!string.IsNullOrEmpty(formConfiguration.BottomContainerCssClass)) navBar.AddClass(formConfiguration.BottomContainerCssClass);

			return navBar;
		}

		//This will likely no longer be used.
		private static string GetProgressControlPosition(OptionSetValue progressPosition, string progressControlPosition)
		{
			if (progressPosition == null) return progressControlPosition;

			switch (progressPosition.Value)
			{
				case (int)WebFormProgressPosition.Top: // Top
					progressControlPosition = "top";
					break;
				case (int)WebFormProgressPosition.Bottom: // Bottom
					progressControlPosition = "bottom";
					break;
				case (int)WebFormProgressPosition.Left: // Left
					progressControlPosition = "left";
					break;
				case (int)WebFormProgressPosition.Right: // Right
					progressControlPosition = "right";
					break;
			}
			return progressControlPosition;
		}

		protected void RenderUserControl(OrganizationServiceContext context, Entity webform, Entity step, WebForms.WebFormEntitySourceDefinition entitySourceDefinition)
		{
			var stepObject = new WebFormStepObject(webform, step, LanguageCode, context);

			var hideFormOnSuccess = step.GetAttributeValue<bool?>("adx_hideformonsuccess") ?? true;

			if (string.IsNullOrWhiteSpace(stepObject.UserControlPath)) throw new ApplicationException("adx_webformstep.adx_usercontrolpath must not be null.");

			HideFormOnSuccess = hideFormOnSuccess;

			var messagePanel = new Panel { ID = "MessagePanel", Visible = false, CssClass = "message" };
			messagePanel.Attributes.Add("role", "alert");
			var messageLabel = new System.Web.UI.WebControls.Label { ID = "MessageLabel", Text = string.Empty };
			messagePanel.Controls.Add(messageLabel);

			SuccessMessage = Localization.GetLocalizedString(stepObject.SuccessMessage, LanguageCode);

			var formPanel = new Panel { ID = "WebFormPanel" };

			if (!string.IsNullOrWhiteSpace(FormCssClass)) { formPanel.CssClass = FormCssClass; }

			var showMovePreviousButton = false;

			var localizedUserControlTitle = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_usercontroltitle"), LanguageCode);

			const string nextButtonCommandName = "MoveNext";

			var nextStepIsRedirect = false;

			if (stepObject.NextStep != null)
			{
				var type = stepObject.NextStep.GetAttributeValue<OptionSetValue>("adx_type");

				if (type != null)
				{
					if (type.Value == (int)WebFormStepType.Redirect) { nextStepIsRedirect = true; }
				}
			}

			if (CurrentSessionHistory.CurrentStepIndex > 0 && stepObject.MovePreviousPermitted) showMovePreviousButton = true;

			var location = stepObject.AttachFileStorageLocation == null
				? StorageLocation.CrmDocument : (StorageLocation)stepObject.AttachFileStorageLocation.Value;
			var accept = string.IsNullOrEmpty(stepObject.AttachFileAccept) ? "*/*" : stepObject.AttachFileAccept;
			var maxFileSize = stepObject.AttachFileRestrictSize && stepObject.AttachFileMaxSize.HasValue
				? Convert.ToUInt64(stepObject.AttachFileMaxSize) << 10 : (ulong?)null;
			_annotationSettings = new AnnotationSettings(context, EnableEntityPermissions, location, accept,
				stepObject.AttachFileRestrictAccept, stepObject.AttachFileTypeErrorMessage, maxFileSize, stepObject.AttachFileSizeErrorMessage);

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var user = portalContext.User;

			var template = new ItemTemplate(stepObject.ValidationGroup, stepObject.CaptchaRequired && (user == null || stepObject.ShowCaptchaForAuthenticatedUsers), stepObject.AttachFile, stepObject.AttachFileAllowMultiple,
				_annotationSettings.AcceptMimeTypes, _annotationSettings.RestrictMimeTypes,
				_annotationSettings.RestrictMimeTypesErrorMessage, _annotationSettings.MaxFileSize,
				_annotationSettings.MaxFileSize.HasValue, _annotationSettings.MaxFileSizeErrorMessage, stepObject.AttachFileLabel,
				stepObject.AttachFileRequired, stepObject.AttachFileRequiredErrorMessage, false, "MoveNext", "MoveNext", stepObject.SubmitButtonText,
				stepObject.SubmitButtonCssClass, true, stepObject.SubmitButtonBusyText);

			var page = HttpContext.Current.Handler as Page;

			if (page == null) throw new ApplicationException(string.Format("Page not available. Failed to load user control  at {0}.", stepObject.UserControlPath));

			var userControl = (WebFormUserControl)page.LoadControl(@stepObject.UserControlPath);

			if (userControl == null) throw new ApplicationException(string.Format("The user control at {0} couldn't be loaded.", stepObject.UserControlPath));

			if (stepObject.ConfirmOnExit)
			{
				var confirmOnExitControl = FindControl("confirmOnExit");
				if (confirmOnExitControl == null)
				{
					Controls.Add(new HiddenField { ID = "confirmOnExit", ClientIDMode = ClientIDMode.Static, Value = "true" });
					Controls.Add(new HiddenField { ID = "confirmOnExitMessage", ClientIDMode = ClientIDMode.Static, Value = stepObject.ConfirmOnExitMessage });
				}
			}

			userControl.SetEntityReference = stepObject.SetEntityReference;
			userControl.EntityReferenceRelationshipName = stepObject.RelationshipName;
			var entityReferenceDefinition = GetUserControlReferenceEntityDefinition(context, step);
			if (entityReferenceDefinition != null)
			{
				userControl.EntityReferenceTargetEntityID = entityReferenceDefinition.ID;
				userControl.EntityReferenceTargetEntityName = entityReferenceDefinition.LogicalName;
				userControl.EntityReferenceTargetEntityPrimaryKeyName = entityReferenceDefinition.PrimaryKeyLogicalName;
			}

			userControl.ValidationGroup = stepObject.ValidationGroup;
			userControl.LanguageCode = LanguageCode;
			userControl.PortalName = PortalName;
			userControl.LoadEventKeyName = LoadEventKeyName;
			userControl.WebFormMetadata = stepObject.WebFormMetadata;

			if (CurrentSessionHistory.CurrentStepIndex > 0)
			{
				var prevStepReferenceEntity = GetPreviousStepReferenceEntityDefinition();
				if (prevStepReferenceEntity != null)
				{
					userControl.PreviousStepEntityID = prevStepReferenceEntity.ID;
					userControl.PreviousStepEntityLogicalName = prevStepReferenceEntity.LogicalName;
					userControl.PreviousStepEntityPrimaryKeyLogicalName = prevStepReferenceEntity.PrimaryKeyLogicalName;
				}
			}

			if (entitySourceDefinition != null)
			{
				userControl.CurrentStepEntityID = entitySourceDefinition.ID;
				userControl.CurrentStepEntityLogicalName = entitySourceDefinition.LogicalName;
				userControl.CurrentStepEntityPrimaryKeyLogicalName = entitySourceDefinition.PrimaryKeyLogicalName;
			}
			else
			{
				var currentStepEntityDefinition = GetCurrentStepReferenceEntityDefinition();
				if (currentStepEntityDefinition != null)
				{
					userControl.CurrentStepEntityID = currentStepEntityDefinition.ID;
					userControl.CurrentStepEntityLogicalName = currentStepEntityDefinition.LogicalName;
					userControl.CurrentStepEntityPrimaryKeyLogicalName = currentStepEntityDefinition.PrimaryKeyLogicalName;
				}
			}

			userControl.PostBackUrl = stepObject.PostBackUrl;

			MovePrevious += userControl.OnMovePrevious;

			Submit += userControl.OnSubmit;

			var showProgress = webform.GetAttributeValue<bool?>("adx_progressindicatorenabled") ?? false;
			var progressPosition = webform.GetAttributeValue<OptionSetValue>("adx_progressindicatorposition");
			var progressControlPosition = string.Empty;

			progressControlPosition = GetProgressControlPosition(progressPosition, progressControlPosition);

			if (showProgress && PersistSessionHistory)
			{
				if (string.IsNullOrWhiteSpace(progressControlPosition) || progressControlPosition == "top" || progressControlPosition == "left" || progressControlPosition == "right")
				{
					RenderProgressIndicator(context, webform, this);

					if (progressControlPosition == "left") userControl.Attributes["class"] = userControl.Attributes["class"] + " right";

					if (progressControlPosition == "right") userControl.Attributes["class"] = userControl.Attributes["class"] + " left";
				}
			}

			if (!string.IsNullOrWhiteSpace(localizedUserControlTitle))
			{
				var titleContainer = new HtmlGenericControl("fieldset");
				var control = new HtmlGenericControl("legend") { InnerText = localizedUserControlTitle };
				titleContainer.Controls.Add(control);
				Controls.Add(titleContainer);
			}

			Controls.Add(messagePanel);
			Controls.Add(formPanel);

			RenderReferenceEntityForm(context, step, formPanel);

			formPanel.Controls.Add(userControl);

			template.InstantiateIn(this);

			var buttonContainer = new HtmlGenericControl("div");

			buttonContainer.Attributes.Add("class", "actions");

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			if (showMovePreviousButton)
			{
				AddPreviousButton(stepObject.PreviousButtonText, stepObject.PreviousButtonCssClass, leftContainer);
			}

			AddNextButton(leftContainer, nextButtonCommandName, stepObject.NextStep, stepObject.SubmitButtonText, nextStepIsRedirect,
					stepObject.NextButtonText, stepObject.ValidationGroup, stepObject.SubmitButtonCssClass, stepObject.NextButtonCssClass,
					stepObject.SubmitButtonBusyText);

			buttonContainer.Controls.Add(leftContainer);

			formPanel.DefaultButton = "NextButton";

			formPanel.Controls.Add(buttonContainer);

			if (showProgress && PersistSessionHistory && progressControlPosition == "bottom")
			{
				RenderProgressIndicator(context, webform, this);
			}
		}

		protected void RenderProgressIndicator(OrganizationServiceContext context, Entity webform, Control container)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (webform == null) throw new ArgumentNullException("webform");

			if (container == null) throw new ArgumentNullException("container");

			var startStep = context.RetrieveRelatedEntity(webform, "adx_webform_startstep");

			if (startStep == null) throw new ApplicationException("Web Form must have a Start Step.");

			var showProgress = webform.GetAttributeValue<bool?>("adx_progressindicatorenabled") ?? false;

			if (!showProgress || !PersistSessionHistory) return;

			var prependStepNum = webform.GetAttributeValue<bool?>("adx_progressindicatorprependstepnum") ?? false;
			var progressPosition = webform.GetAttributeValue<OptionSetValue>("adx_progressindicatorposition");
			var progressType = webform.GetAttributeValue<OptionSetValue>("adx_progressindicatortype");
			var progressIgnoreLastStep = webform.GetAttributeValue<bool?>("adx_progressindicatorignorelaststep") ?? false;
			var progressControlPosition = string.Empty;
			var progressControlType = string.Empty;

			progressControlPosition = GetProgressControlPosition(progressPosition, progressControlPosition);

			if (progressType != null)
			{
				switch (progressType.Value)
				{
					case 756150000: // Title
						progressControlType = "title";
						break;
					case 756150001: // Numeric
						progressControlType = "numeric";
						break;
					case 756150002: // Progress Bar
						progressControlType = "progressbar";
						break;
				}
			}

			var steps = GetProgressSteps(context, startStep, CurrentSessionHistory.CurrentStepIndex, CurrentSessionHistory.StepHistory, LanguageCode);

			var progressControl = new ProgressIndicator
			{
				ID = string.Format("{0}_ProgressIndicator", ID),
				Position = progressControlPosition,
				PrependStepIndexToTitle = prependStepNum,
				Type = progressControlType,
				CountLastStepInProgress = !progressIgnoreLastStep
			};

			container.Controls.Add(progressControl);

			progressControl.DataSource = steps;

			progressControl.DataBind();
		}

		protected string GetContextName()
		{
			var portalConfig = PortalCrmConfigurationManager.GetPortalContextElement(PortalName);

			return portalConfig == null ? null : portalConfig.ContextName;
		}

		protected SessionHistory InitializeCurrentSessionHistory(OrganizationServiceContext context, Entity webform, Entity startStep)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (webform == null) throw new ArgumentNullException("webform");

			if (startStep == null) throw new ArgumentNullException("startStep");

			var logicalName = startStep.GetAttributeValue<string>("adx_targetentitylogicalname");
			var primaryKey = startStep.GetAttributeValue<string>("adx_targetentityprimarykeylogicalname");

			if (string.IsNullOrWhiteSpace(startStep.GetAttributeValue<string>("adx_targetentitylogicalname")))
			{
				throw new ApplicationException(ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));
			}

			if (string.IsNullOrWhiteSpace(primaryKey))
			{
				primaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, logicalName);
			}

			if (string.IsNullOrWhiteSpace(primaryKey))
			{
				throw new ApplicationException(string.Format("Couldn't retrieve Primary Key Attribute name on entity {0}.", logicalName));
			}

			return SessionHistoryProvider.InitializeSessionHistory(context, webform.Id, startStep.Id, 0, Guid.Empty, logicalName, primaryKey);
		}

		protected bool TryGetSessionHistory(OrganizationServiceContext context, Guid sessionId, out SessionHistory sessionHistory)
		{
			sessionHistory = SessionHistoryProvider.GetSessionHistory(context, sessionId);

			return sessionHistory != null;
		}

		protected bool TryGetSessionHistory(OrganizationServiceContext context, Entity webform, Entity startStep, out WebForms.WebFormEntitySourceDefinition entitySourceDefinition, out Entity record, out SessionHistory sessionHistory)
		{
			SessionHistory session = null;
			if (!TryGetPrimaryEntitySourceDefinition(context, startStep, out entitySourceDefinition, out record))
			{
				if (HttpContext.Current.Request.IsAuthenticated)
				{
					var portal = PortalCrmConfigurationManager.CreatePortalContext();
					if (portal.User == null) throw new ApplicationException("Couldn't load user record. Portal context User is null.");

					switch (portal.User.LogicalName)
					{
						case "contact":
							session = SessionHistoryProvider.GetSessionHistoryByContact(context, webform.Id, portal.User.Id);
							break;
						case "systemuser":
							session = SessionHistoryProvider.GetSessionHistoryBySystemUser(context, webform.Id, portal.User.Id);
							break;
						default:
							if (HttpContext.Current.User != null && !string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
							{
								session = SessionHistoryProvider.GetSessionHistoryByUserIdentityName(context, webform.Id, HttpContext.Current.User.Identity.Name);
							}
							else
							{
								throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portal.User.LogicalName));
							}
							break;
					}
				}
				else
				{
					if (HttpContext.Current.Profile != null && !string.IsNullOrWhiteSpace(HttpContext.Current.Profile.UserName))
					{
						session = SessionHistoryProvider.GetSessionHistoryByAnonymousIdentification(context, webform.Id, HttpContext.Current.Profile.UserName);
					}
				}
			}
			else
			{
				if (entitySourceDefinition.ID == Guid.Empty) throw new ApplicationException("Entity Source ID couldn't be determined.");

				session = SessionHistoryProvider.GetSessionHistoryByPrimaryRecord(context, webform.Id, entitySourceDefinition.ID);
			}

			sessionHistory = session;

			return sessionHistory != null;
		}

		protected bool TryGetPrimaryEntitySourceDefinition(OrganizationServiceContext context, Entity step, out WebForms.WebFormEntitySourceDefinition definition, out Entity record)
		{
			var id = string.Empty;

			step.AssertEntityName("adx_webformstep");

			var logicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var primaryKey = step.GetAttributeValue<string>("adx_targetentityprimarykeylogicalname");

			if (string.IsNullOrWhiteSpace(logicalName))
			{
				throw new ApplicationException(ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));
			}

			if (!string.IsNullOrWhiteSpace(logicalName) && string.IsNullOrWhiteSpace(primaryKey))
			{
				primaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, logicalName);
			}

			if (string.IsNullOrWhiteSpace(primaryKey))
			{
				throw new ApplicationException(ResourceManager.GetString("Failed_To_Determine_Target_Entity_Pk_Logical_Name_Exception"));
			}

			var mode = step.GetAttributeValue<OptionSetValue>("adx_mode");
			var entitySourceType = step.GetAttributeValue<OptionSetValue>("adx_entitysourcetype");

			if (mode != null)
			{
				if (mode.Value == (int)WebFormStepMode.Insert) // Insert
				{
					entitySourceType = null;
				}
			}

			if (entitySourceType == null || entitySourceType.Value == 100000000)
			{
				Entity existingRecord;

				var primaryKeyQueryStringParameter = step.GetAttributeValue<string>("adx_primarykeyquerystringparametername");

				if (!string.IsNullOrWhiteSpace(primaryKeyQueryStringParameter))
				{
					id = HttpContext.Current.Request[primaryKeyQueryStringParameter];
				}

				// Try find existing record either by id if provided or by current portal user association

				var recordExists = string.IsNullOrWhiteSpace(id) ? TryFindExistingRecordForCurrentPortalUser(context, step, out existingRecord) : TryFindExistingRecordByID(context, logicalName, primaryKey, id, out existingRecord);

				if (recordExists)
				{
					id = existingRecord.Id.ToString();
				}
				else
				{
					definition = null;
					record = null;
					return false;
				}

				definition = new WebForms.WebFormEntitySourceDefinition(logicalName, primaryKey, id);

				record = existingRecord;

				return true;
			}

			switch (entitySourceType.Value)
			{
				case (int)WebFormStepSourceType.QueryString: // Query String
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_primarykeyattributelogicalname")))
					{
						throw new ApplicationException("adx_webformstep.adx_primarykeyattributelogicalname must not be null.");
					}
					primaryKey = step.GetAttributeValue<string>("adx_primarykeyattributelogicalname");
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_primarykeyquerystringparametername")))
					{
						throw new ApplicationException("adx_webformstep.adx_primarykeyquerystringparametername must not be null.");
					}
					var primaryKeyQueryStringParameter = step.GetAttributeValue<string>("adx_primarykeyquerystringparametername");
					id = HttpContext.Current.Request[primaryKeyQueryStringParameter];
					Guid guid;
					if (string.IsNullOrWhiteSpace(id))
					{
						definition = null;
						record = null;
						return false;
					}
					if (!Guid.TryParse(id, out guid))
					{
						definition = null;
						record = null;
						return false;
					}
					break;
				case (int)WebFormStepSourceType.CurrentPortalUser: // Current Portal User
					var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
					if (portalContext.User == null)
					{
						throw new ApplicationException("Couldn't load user record. Portal context User is null.");
					}
					id = portalContext.User.Id.ToString();
					switch (portalContext.User.LogicalName)
					{
						case "contact":
							primaryKey = "contactid";
							break;
						case "systemuser":
							primaryKey = "systemuserid";
							break;
						default:
							throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portalContext.User.LogicalName));
					}
					break;
				case (int)WebFormStepSourceType.ResultFromPreviousStep: // Source entity from previous step
					throw new ApplicationException("adx_webformstep.adx_entitysourcetype is not valid for the start step.");
				default:
					throw new ApplicationException("adx_webformstep.adx_entitysourcetype is not valid for the start step.");
			}

			definition = new WebForms.WebFormEntitySourceDefinition(logicalName, primaryKey, id);

			Guid recordId;
			record = null;

			if (Guid.TryParse(id, out recordId))
			{
				record = context.RetrieveSingle(logicalName, primaryKey, recordId, FetchAttribute.All);

				if (record != null) return true;
				definition = null;
				return false;
			}

			return true;
		}

		protected WebForms.WebFormEntitySourceDefinition GetStepEntitySourceDefinition(OrganizationServiceContext context, Entity step)
		{
			var logicalName = string.Empty;
			var primaryKey = string.Empty;
			var id = string.Empty;

			step.AssertEntityName("adx_webformstep");

			var mode = step.GetAttributeValue<OptionSetValue>("adx_mode");
			var entitySourceType = step.GetAttributeValue<OptionSetValue>("adx_entitysourcetype");

			if (mode != null)
			{
				if (mode.Value == (int)WebFormStepMode.Insert) // None
				{
					entitySourceType = null;
				}
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();

			if (entitySourceType == null) return null;

			switch (entitySourceType.Value)
			{
				case 100000000: // None
					return null;
				case (int)WebFormStepSourceType.QueryString: // Query String
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_targetentitylogicalname")))
					{
						throw new ApplicationException(ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));
					}
					logicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_primarykeyattributelogicalname")))
					{
						throw new ApplicationException("adx_webformstep.adx_primarykeyattributelogicalname must not be null.");
					}
					primaryKey = step.GetAttributeValue<string>("adx_primarykeyattributelogicalname");
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_primarykeyquerystringparametername")))
					{
						throw new ApplicationException("adx_webformstep.adx_primarykeyquerystringparametername must not be null.");
					}
					var primaryKeyQueryStringParameter = step.GetAttributeValue<string>("adx_primarykeyquerystringparametername");
					id = HttpContext.Current.Request[primaryKeyQueryStringParameter];
					Guid guid;
					if (string.IsNullOrWhiteSpace(id))
					{
						return null;
					}
					if (!Guid.TryParse(id, out guid))
					{
						return null;
					}
					break;
				case (int)WebFormStepSourceType.CurrentPortalUser: // Current Portal User
					if (portalContext.User == null)
					{
						throw new ApplicationException("Couldn't load user record. Portal context User is null.");
					}
					id = portalContext.User.Id.ToString();
					switch (portalContext.User.LogicalName)
					{
						case "contact":
							primaryKey = "contactid";
							break;
						case "systemuser":
							primaryKey = "systemuserid";
							break;
						default:
							throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portalContext.User.LogicalName));
					}
					if (string.IsNullOrWhiteSpace(step.GetAttributeValue<string>("adx_targetentitylogicalname")))
					{
						throw new ApplicationException(ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));
					}
					logicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
					break;
				case 100000004: // Record Associated to Current Portal User 
					var relationship = step.GetAttributeValue<string>("adx_recordsourcerelationshipname");
					if (string.IsNullOrWhiteSpace(relationship))
					{
						throw new ApplicationException("Required Relationship Name has not been specified for the Record Source Type 'Record Associated to Current Portal User'.");
					}
					if (portalContext.User == null)
					{
						throw new ApplicationException("Couldn't load user record. Portal context User is null.");
					}
					string userPrimaryKey;
					switch (portalContext.User.LogicalName)
					{
						case "contact":
							userPrimaryKey = "contactid";
							break;
						case "systemuser":
							userPrimaryKey = "systemuserid";
							break;
						default:
							throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portalContext.User.LogicalName));
					}
					var user = context.RetrieveSingle(portalContext.User.LogicalName, userPrimaryKey, portalContext.User.Id, FetchAttribute.All);

					if (user == null)
					{
						throw new ApplicationException(string.Format("Couldn't load user record. Portal context User could not be found with id equal to {0}.", portalContext.User.Id));
					}
					var source = context.RetrieveRelatedEntity(user, relationship);
					if (source == null)
					{

						throw new ApplicationException(string.Format("Form failed to databind. Couldn't find a record associated to the current portal user for relationship {0}.", relationship));
					}
					id = source.Id.ToString();
					break;
				case (int)WebFormStepSourceType.ResultFromPreviousStep: // Source entity from previous step
					var referenceEntityDefinition = GetPreviousStepReferenceEntityDefinition();
					if (referenceEntityDefinition == null)
					{
						throw new ApplicationException("Previous Step Reference Entity definition is null.");
					}
					if (referenceEntityDefinition.ID == Guid.Empty)
					{
						throw new ApplicationException("Previous Step Reference Entity ID is empty.");
					}
					id = referenceEntityDefinition.ID.ToString();
					logicalName = referenceEntityDefinition.LogicalName;
					if (string.IsNullOrWhiteSpace(logicalName))
					{
						throw new ApplicationException("Entity reference target entity logical name is null.");
					}
					primaryKey = string.IsNullOrWhiteSpace(referenceEntityDefinition.PrimaryKeyLogicalName) ? MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, logicalName) : referenceEntityDefinition.PrimaryKeyLogicalName;
					if (string.IsNullOrWhiteSpace(primaryKey))
					{
						throw new ApplicationException(string.Format("Couldn't retrieve Primary Key Attribute name on entity {0}.", logicalName));
					}
					break;
			}

			return new WebForms.WebFormEntitySourceDefinition(logicalName, primaryKey, id);
		}

		protected bool TryFindExistingRecordByID(OrganizationServiceContext context, string entityLogicalName, string primaryKeyAttributeLogicalName, string id, out Entity existingRecord)
		{
			Guid entityID;

			existingRecord = null;

			if (context == null || string.IsNullOrWhiteSpace(entityLogicalName) || string.IsNullOrWhiteSpace(id))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Required Parameters are null.");

				return false;
			}

			if (string.IsNullOrWhiteSpace(primaryKeyAttributeLogicalName))
			{
				primaryKeyAttributeLogicalName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, entityLogicalName);
			}

			if (string.IsNullOrWhiteSpace(primaryKeyAttributeLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to find primary key attribute name for entity '{0}'.", entityLogicalName));

				return false;
			}

			if (!Guid.TryParse(id, out entityID))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("The id specified '{0}' is not valid.", id));

				return false;
			}

			var record = context.RetrieveSingle(entityLogicalName, primaryKeyAttributeLogicalName, entityID, FetchAttribute.All);

			existingRecord = record;

			return record != null;
		}

		protected bool TryFindExistingRecordForCurrentPortalUser(OrganizationServiceContext context, Entity step, out Entity existingRecord)
		{
			existingRecord = null;

			if (!HttpContext.Current.Request.IsAuthenticated)
			{
				return false;
			}

			var associatePortalUser = step.GetAttributeValue<bool?>("adx_associatecurrentportaluser") ?? false;
			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var portalUserLookupAttributeName = step.GetAttributeValue<string>("adx_targetentityportaluserlookupattribute");

			if (!associatePortalUser)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_targetentitylogicalname must not be null.");

				return false;
			}

			if (string.IsNullOrWhiteSpace(portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "adx_webformstep.adx_targetentityportaluserlookupattribute is null.");

				return false;
			}

			if (!MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' entity does not contain an attribute.", EntityNamePrivacy.GetEntityName(targetEntityLogicalName)));
				return false;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var portalUser = portalContext.User;

			if (portalUser != null)
			{
				var record = context.RetrieveSingle(
								new Fetch
								{
									Entity = new FetchEntity(targetEntityLogicalName)
									{
										Filters = new[] { new Filter { Conditions = new[] { new Condition(portalUserLookupAttributeName, ConditionOperator.Equal, portalUser.Id) } } },
										Orders = new[] { new Order("modifiedon", OrderType.Descending), }
									}
								});

				existingRecord = record;

				return record != null;
			}

			ADXTrace.Instance.TraceError(TraceCategory.Application, "Current Portal User is null.");

			return false;
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			SetAttributeValuesOnUpdating(context, step, e);

			SetEntityReference(context, step, e.Values);

			LogUserInfoOnUpdating(context, step, e);

			var savingEventArgs = new WebFormSavingEventArgs(e.Values, SavingEventKeyName) { EntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname") };

			OnItemSaving(sender, savingEventArgs);

			e.Cancel = savingEventArgs.Cancel;
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			SetAttributeValuesOnInserting(context, step, e);

			AssociateCurrentPortalUser(context, step, e);

			SetEntityReference(context, step, e.Values);

			SetAutoNumber(context, step, e);

			LogUserInfoOnInserting(context, step, e);

			var savingEventArgs = new WebFormSavingEventArgs(e.Values, SavingEventKeyName) { EntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname") };

			OnItemSaving(sender, savingEventArgs);

			e.Cancel = savingEventArgs.Cancel;
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var step = context.RetrieveSingle(
								new Fetch
								{
									Entity = new FetchEntity("adx_webformstep")
									{
										Filters = new[] { new Filter { Conditions = new[] { new Condition("adx_webformstepid", ConditionOperator.Equal, this.CurrentSessionHistory.CurrentStepId) } } }
									}
								});

			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			var entityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (e.Exception == null)
			{
				if (e.EntityId == null || e.EntityId == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "e.EntityId is null or empty. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps.");
				}
				else
				{

					UpdateSessionHistoryPrimaryRecordID(e.EntityId.GetValueOrDefault());

					UpdateStepHistoryReferenceEntityID(e.EntityId.GetValueOrDefault());

					AssociateEntity(context, step, e.EntityId.GetValueOrDefault());

					AttachFileOnItemInserted(context, step, sender, e);

					if (PersistSessionHistory)
					{
						SaveSessionHistory(context);
					}

					if (SetStateOnSave)
					{
						TrySetState(context, new EntityReference(entityLogicalName, e.EntityId.GetValueOrDefault()), SetStateOnSaveValue);
					}
				}
			}
			else
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("e.EntityId is null or empty. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps. {0}", e.Exception.InnerException));
			}

			var savedEventArgs = new WebFormSavedEventArgs(e.EntityId, entityLogicalName, e.Exception, false, SavedEventKeyName);
			OnItemSaved(sender, savedEventArgs);
			e.Exception = savedEventArgs.Exception;
			e.ExceptionHandled = savedEventArgs.ExceptionHandled;

			if (e.Exception != null && !e.ExceptionHandled)
			{
				DisplayMessage(this,
					"<p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " +
					Page.Server.HtmlEncode(e.Exception.InnerException == null
						? e.Exception.Message
						: e.Exception.InnerException.Message) + "</p>", "alert-danger", false);

				e.ExceptionHandled = true;

				return;
			}

			if (e.Exception == null)
			{
				MoveNextStep();
			}
		}

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);
			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			var entityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(entityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (e.Exception == null)
			{
				if (e.Entity == null || e.Entity.Id == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "e.Entity is null  or e.Entity.Id is null or empty. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps.");
				}
				else
				{
					UpdateSessionHistoryPrimaryRecordID(e.Entity.Id);

					UpdateStepHistoryReferenceEntityID(e.Entity.Id);

					AttachFileOnItemUpdated(context, step, sender, e);

					if (PersistSessionHistory)
					{
						SaveSessionHistory(context);
					}

					if (SetStateOnSave)
					{
						TrySetState(context, e.Entity.ToEntityReference(), SetStateOnSaveValue);
					}
				}
			}
			else
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("e.Entity is null  or e.Entity.Id is null or empty. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps. {0}", e.Exception.InnerException));
			}

			var savedEventArgs = new WebFormSavedEventArgs(e.Entity == null ? Guid.Empty : e.Entity.Id, entityLogicalName, e.Exception, false, SavedEventKeyName);
			OnItemSaved(sender, savedEventArgs);
			e.Exception = savedEventArgs.Exception;
			e.ExceptionHandled = savedEventArgs.ExceptionHandled;

			if (e.Exception != null && !e.ExceptionHandled)
			{
				DisplayMessage(this,
					"<p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " +
					Page.Server.HtmlEncode(e.Exception.InnerException == null
						? e.Exception.Message
						: e.Exception.InnerException.Message) + "</p>", "alert-danger", false);

				e.ExceptionHandled = true;

				return;
			}

			if (e.Exception == null)
			{
				MoveNextStep();
			}
		}

		protected void RedirectToSelf(IDictionary<string, string> appendQueryString)
		{
			var url = new UrlBuilder(HttpContext.Current.Request.Url.PathAndQuery);

			if (appendQueryString != null && appendQueryString.Count > 0)
			{
				var nameValueCollection = new NameValueCollection();
				foreach (var kvp in appendQueryString)
				{
					nameValueCollection.Add(kvp.Key, kvp.Value);
				}

				url.QueryString.Add(nameValueCollection);
			}

			if (CurrentSessionHistory != null && CurrentSessionHistory.CurrentStepId != Guid.Empty)
			{
				if (url.QueryString.Get("stepid") == null || url.QueryString.Get("stepid") == string.Empty)
				{
					url.QueryString.Add("stepid", CurrentSessionHistory.CurrentStepId.ToString());
				}
				else
				{
					url.QueryString["stepid"] = CurrentSessionHistory.CurrentStepId.ToString();
				}
			}

			if (StartNewSessionOnLoad && PersistSessionHistory)
			{
				if (CurrentSessionHistory != null && CurrentSessionHistory.Id != Guid.Empty)
				{
					if (url.QueryString.Get("sessionid") == null || url.QueryString.Get("sessionid") == string.Empty)
					{
						url.QueryString.Add("sessionid", CurrentSessionHistory.Id.ToString());
					}
				}
			}

			HttpContext.Current.Response.Redirect(url.PathWithQueryString, false);
		}

		protected void MovePreviousStep()
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				throw new ApplicationException("Step History is null.");
			}

			if (CurrentSessionHistory.StepHistory.Count == 0)
			{
				throw new ApplicationException("Step History is empty");
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var currentStep = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (currentStep == null)
			{
				throw new ApplicationException(string.Format("Couldn't retrieve adx_webformstep where ID equals {0}.", CurrentSessionHistory.CurrentStepId));
			}

			var movePreviousPermitted = currentStep.GetAttributeValue<bool?>("adx_movepreviouspermitted") ?? true;

			if (!movePreviousPermitted)
			{
				return;
			}

			UpdateStepHistoryIsActive(currentStep, false);

			var previousStepId = GetPreviousStepId();

			var previousStep = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", previousStepId, FetchAttribute.All);

			if (previousStep == null)
			{
				throw new ApplicationException(string.Format("Couldn't retrieve adx_webformstep where ID equals {0}.", previousStepId));
			}

			var type = previousStep.GetAttributeValue<OptionSetValue>("adx_type");

			CurrentSessionHistory.CurrentStepIndex--;

			CurrentSessionHistory.CurrentStepId = previousStep.Id;

			if (type != null)
			{
				switch (type.Value)
				{
					case 100000000: // Condition
						MovePreviousStep();
						break;
					case 100000001: // Load Form
						if (PersistSessionHistory)
						{
							SaveSessionHistory(context);
						}
						RedirectToSelf(null);
						break;
					case 100000002: // Load Tab
						if (PersistSessionHistory)
						{
							SaveSessionHistory(context);
						}
						RedirectToSelf(null);
						break;
					case 100000004: // Load User Control
						if (PersistSessionHistory)
						{
							SaveSessionHistory(context);
						}
						RedirectToSelf(null);
						break;
					default:
						throw new ApplicationException("adx_webformstep.adx_type is not valid.");
				}
			}
		}

		protected void MoveNextStep()
		{
			MoveNextStep(true);
		}

		protected void MoveNextStep(bool? conditionPassed)
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				throw new ApplicationException("Step History is null.");
			}

			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var currentStep = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (currentStep == null)
			{
				throw new ApplicationException("current step is null.");
			}

			var previousStepId = currentStep.Id;

			var nextStepRelationshipName = (conditionPassed ?? true) ? "adx_webformstep_nextstep" : "adx_webformstep_conditiondefaultnextstep";
			var nextStep = context.RetrieveRelatedEntity(currentStep, new Relationship(nextStepRelationshipName) { PrimaryEntityRole = EntityRole.Referencing });

			if (nextStep == null)
			{
				if (PersistSessionHistory)
				{
					SessionHistoryProvider.DeactivateSessionHistory(context, CurrentSessionHistory.Id);
				}

				if (Mode == FormViewMode.Edit && !HideFormOnSuccess)
				{
					DisplayMessage(this, SuccessMessage, "success", false);
				}
				else
				{
					RedirectToSelf(new Dictionary<string, string> { { "msg", "success" } });
				}
			}
			else
			{
				var type = nextStep.GetAttributeValue<OptionSetValue>("adx_type");

				if (type != null)
				{
					switch (type.Value)
					{
						case 100000000: // Condition
							var recordid = GetCurrentStepReferenceEntityID();
							AddStepDetailsToStepHistory(context, nextStep, recordid, previousStepId);
							context.ReAttach(nextStep);
							var result = EvaluateConditionStep(context, nextStep);
							MoveNextStep(result);
							break;
						case 100000001: // Load Form
							AddStepDetailsToStepHistory(context, nextStep, Guid.Empty, previousStepId);
							RedirectToSelf(null);
							break;
						case 100000002: // Load Tab
							AddStepDetailsToStepHistory(context, nextStep, Guid.Empty, previousStepId);
							RedirectToSelf(null);
							break;
						case 100000003: // Redirect
							AddStepDetailsToStepHistory(context, nextStep, Guid.Empty, previousStepId);
							context.ReAttach(nextStep);
							ProcessRedirectStep(context, nextStep);
							break;
						case 100000004: // Load User Control
							AddStepDetailsToStepHistory(context, nextStep, Guid.Empty, previousStepId);
							RedirectToSelf(null);
							break;
					}
				}
			}
		}

		//optionset instead
		protected void ProcessStep(OrganizationServiceContext context, Entity webform, Entity step, WebForms.WebFormEntitySourceDefinition entitySourceDefinition)
		{
			var type = step.GetAttributeValue<OptionSetValue>("adx_type");

			if (type == null)
			{
				throw new ApplicationException("Invalid step type.");
			}

			LoadEventKeyName = step.GetAttributeValue<string>("adx_loadeventkeyname") ?? string.Empty;
			SubmitEventKeyName = step.GetAttributeValue<string>("adx_submiteventkeyname") ?? string.Empty;
			MovePreviousEventKeyName = step.GetAttributeValue<string>("adx_movepreviouseventkeyname") ?? string.Empty;
			SavingEventKeyName = step.GetAttributeValue<string>("adx_savingeventkeyname") ?? string.Empty;
			SavedEventKeyName = step.GetAttributeValue<string>("adx_savedeventkeyname") ?? string.Empty;

			MappingFieldCollection = new MappingFieldMetadataCollection()
			{
				AddressLineFieldName = step.GetAttributeValue<string>("adx_geolocation_addresslinefieldname"),
				CityFieldName = step.GetAttributeValue<string>("adx_geolocation_cityfieldname"),
				CountryFieldName = step.GetAttributeValue<string>("adx_geolocation_countryfieldname"),
				CountyFieldName = step.GetAttributeValue<string>("adx_geolocation_countyfieldname"),
				FormattedLocationFieldName = step.GetAttributeValue<string>("adx_geolocation_formattedaddressfieldname"),
				LatitudeFieldName = step.GetAttributeValue<string>("adx_geolocation_latitudefieldname"),
				LongitudeFieldName = step.GetAttributeValue<string>("adx_geolocation_longitudefieldname"),
				NeightbourhoodFieldName = step.GetAttributeValue<string>("adx_geolocation_neighborhoodfieldname"),
				PostalCodeFieldName = step.GetAttributeValue<string>("adx_geolocation_postalcodefieldname"),
				StateProvinceFieldName = step.GetAttributeValue<string>("adx_geolocation_statefieldname"),
				Enabled = step.GetAttributeValue<bool>("adx_geolocation_enabled"),
				DisplayMap = step.GetAttributeValue<bool>("adx_geolocation_displaymap")
			};

			switch (type.Value)
			{
				case 100000000: // Condition
					var result = EvaluateConditionStep(context, step);
					MoveNextStep(result);
					break;
				case 100000001: // Load Form
					RenderForm(context, webform, step, GetFormViewMode(step), entitySourceDefinition);
					break;
				case 100000002: // Load Tab
					RenderForm(context, webform, step, GetFormViewMode(step), entitySourceDefinition);
					break;
				case 100000003: // Redirect
					ProcessRedirectStep(context, step);
					break;
				case 100000004: // Load User Control
					RenderUserControl(context, webform, step, entitySourceDefinition);
					break;
			}
		}

		protected bool EvaluateConditionStep(OrganizationServiceContext context, Entity step)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (step == null)
			{
				throw new ArgumentNullException("step");
			}

			var condition = step.GetAttributeValue<string>("adx_condition");

			if (string.IsNullOrWhiteSpace(condition))
			{
				throw new ApplicationException("adx_webformstep.adx_condition is expected and is null.");
			}

			var referenceEntity = GetPreviousStepReferenceEntityDefinition();

			if (referenceEntity == null)
			{
				throw new ApplicationException("Error retrieving reference entity definition from previous step.");
			}

			var entity = context.RetrieveSingle(referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID, FetchAttribute.All);

			if (entity == null)
			{
				throw new ApplicationException(string.Format("Error retrieving the entity {0} where {1} equals {2}.", referenceEntity.LogicalName, referenceEntity.PrimaryKeyLogicalName, referenceEntity.ID));
			}

			// build expression from conditional statement and evaluate.

			var evaluator = new EntityExpressionEvaluator(context, entity);

			var expression = Expression.ParseCondition(condition);

			return evaluator.Evaluate(expression);
		}

		protected void ProcessRedirectStep(OrganizationServiceContext context, Entity step)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (step == null)
			{
				throw new ArgumentNullException("step");
			}

			var existingQueryString = HttpContext.Current.Request.QueryString;
			var redirectUrl = step.GetAttributeValue<string>("adx_redirecturl");
			var appendExistingQueryString = step.GetAttributeValue<bool?>("adx_appendquerystring") ?? false;
			UrlBuilder url;

			if (string.IsNullOrWhiteSpace(redirectUrl))
			{
				var page = context.RetrieveRelatedEntity(step, "adx_webformstep_redirectwebpage");

				if (page == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_webformstep_redirectwebpage is null");
					return;
				}

				var path = context.GetUrl(page);

				if (path == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_webformstep_redirectwebpage url is null");
					return;
				}

				url = new UrlBuilder(path);
			}
			else
			{
				url = new UrlBuilder(redirectUrl.StartsWith("http") ? redirectUrl : string.Format("https://{0}", redirectUrl));
			}

			var addquerystring = step.GetAttributeValue<bool?>("adx_redirecturlappendentityidquerystring") ?? false;

			if (addquerystring)
			{
				var queryStringParameterName = step.GetAttributeValue<string>("adx_redirecturlquerystringname");

				if (!string.IsNullOrWhiteSpace(queryStringParameterName))
				{
					var referenceEntityId = GetPreviousStepReferenceEntityID();
					if (referenceEntityId == Guid.Empty)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "Entity Reference ID is empty.");
					}
					else
					{
						url.QueryString.Add(queryStringParameterName, referenceEntityId.ToString());
					}
				}
			}

			if (appendExistingQueryString && existingQueryString.HasKeys())
			{
				url.QueryString.Add(existingQueryString);
			}

			var customQueryString = step.GetAttributeValue<string>("adx_redirecturlcustomquerystring");

			if (!string.IsNullOrWhiteSpace(customQueryString))
			{
				try
				{
					var customQueryStringCollection = HttpUtility.ParseQueryString(customQueryString);

					url.QueryString.Add(customQueryStringCollection);
				}
				catch (Exception ex)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to add adx_redirecturlcustomquerystring to the Query String. {0}", ex.ToString()));
				}
			}

			var queryStringAttributeParameterName = step.GetAttributeValue<string>("adx_redirecturlquerystringattributeparamname");

			if (!string.IsNullOrWhiteSpace(queryStringAttributeParameterName))
			{
				var queryStringAttributeLogicalName = step.GetAttributeValue<string>("adx_redirecturlquerystringattribute");

				if (!string.IsNullOrWhiteSpace(queryStringAttributeLogicalName))
				{
					var previousStepEntityReference = GetPreviousStepReferenceEntityDefinition();

					if (previousStepEntityReference != null)
					{
						if (previousStepEntityReference.ID != Guid.Empty)
						{
							var entity =
								context.RetrieveSingle(previousStepEntityReference.LogicalName, previousStepEntityReference.PrimaryKeyLogicalName, previousStepEntityReference.ID, FetchAttribute.All);

							if (entity != null && entity.Attributes.ContainsKey(queryStringAttributeLogicalName))
							{
								var queryStringAttributeValue = entity[queryStringAttributeLogicalName];

								if (queryStringAttributeValue != null)
								{
									var attributeValue = TryConvertAttributeValueToString(context, previousStepEntityReference.LogicalName, queryStringAttributeLogicalName, queryStringAttributeValue);

									if (!string.IsNullOrWhiteSpace(attributeValue))
									{
										url.QueryString.Add(queryStringAttributeParameterName, attributeValue);
									}
								}
							}
						}
					}
				}
			}

			if (PersistSessionHistory)
			{
				SessionHistoryProvider.DeactivateSessionHistory(context, CurrentSessionHistory.Id);
			}

			if (string.IsNullOrWhiteSpace(redirectUrl))
			{
				HttpContext.Current.Response.Redirect(url.PathWithQueryString);
			}
			else
			{
				HttpContext.Current.Response.Redirect(url);
			}
		}

		protected void ApplyStepMetadataPrepopulateValues(OrganizationServiceContext context, Entity step, Control container)
		{
			var metadata = context.RetrieveRelatedEntities(
				step,
				"adx_webformmetadata_webformstep",
				filters: new[] { new Filter { Conditions = new[] { new Condition("adx_prepopulatetype", ConditionOperator.NotNull) } } }).Entities;

			if (!metadata.Any())
			{
				return;
			}

			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, targetEntityLogicalName);
			}

			foreach (var item in metadata)
			{
				var attributeName = item.GetAttributeValue<string>("adx_attributelogicalname");
				if (string.IsNullOrWhiteSpace(attributeName))
				{
					continue;
				}
				var type = item.GetAttributeValue<OptionSetValue>("adx_prepopulatetype");
				if (type == null)
				{
					continue;
				}
				object value;
				switch (type.Value)
				{
					case 100000000: // Value
						value = item.GetAttributeValue<string>("adx_prepopulatevalue");
						break;
					case 100000001: // Today's Date
						value = DateTime.UtcNow;
						break;
					case 100000002: // Current Portal User
						if (!HttpContext.Current.Request.IsAuthenticated)
						{
							continue;
						}
						var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
						if (portalContext.User == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, "Current portal user record is null.");
							continue;
						}
						try
						{
							var fromAttribute = item.GetAttributeValue<string>("adx_prepopulatefromattribute");
							if (string.IsNullOrWhiteSpace(fromAttribute))
							{
								continue;
							}
							if (portalContext.User.LogicalName == "contact" && fromAttribute == "contactid")
							{
								var entityReference = portalContext.User.ToEntityReference();
								if (entityReference.Name == null)
								{
									entityReference.Name = portalContext.User.GetAttributeValue<string>("fullname");
								}
								value = entityReference;
							}
							else
							{
								if (!MetadataHelper.IsAttributeLogicalNameValid(context, portalContext.User.LogicalName, fromAttribute))
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' is not a valid attribute on {1}.", fromAttribute, portalContext.User.LogicalName));
									continue;
								}
								value = portalContext.User.GetAttributeValue(fromAttribute);
							}
						}
						catch (Exception ex)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
							continue;
						}
						break;
					default:
						continue;
				}
				TrySetFieldValue(context, container, attributeName, value, AttributeTypeCodeDictionary);
			}
		}

		protected static bool TrySetFieldValue(OrganizationServiceContext serviceContext, Control container, string attributeName, object value, Dictionary<string, AttributeTypeCode?> attributeTypeCodes)
		{
			if (value == null)
			{
				return false;
			}

			if (attributeTypeCodes == null || !attributeTypeCodes.Any())
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Attribute Type Code Dictionary is null or empty");

				return false;
			}

			var attributeTypeCode = attributeTypeCodes.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Unable to recognize the attribute '{0}' specified.", attributeName));

				return false;
			}

			try
			{
				var field = container.FindControl(attributeName);

				if (field == null)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Could not find control with id equal '{0}'", attributeName));

					return false;
				}

				switch (attributeTypeCode)
				{
					case AttributeTypeCode.BigInt:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Boolean:
						if (field is CheckBox)
						{
							var control = (CheckBox)field;
							control.Checked = Convert.ToBoolean(value.ToString());
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Customer:
						if (value is EntityReference)
						{
							var entityReference = (EntityReference)value;
							return TrySetLookupFieldValue(serviceContext, container, entityReference, attributeName);
						}
						return false;
					case AttributeTypeCode.DateTime:
						if (field is TextBox && value is DateTime)
						{
							var control = (TextBox)field;
							var date = (DateTime)value;
							control.Text = date.ToString(Globalization.DateTimeFormatInfo.RoundTripPattern);
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Decimal:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Double:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Integer:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Lookup:
						if (value is EntityReference)
						{
							var entityReference = (EntityReference)value;
							return TrySetLookupFieldValue(serviceContext, container, entityReference, attributeName);
						}
						return false;
					case AttributeTypeCode.Memo:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Money:
						if (field is TextBox && value is Money)
						{
							var money = (Money)value;
							var control = (TextBox)field;
							control.Text = money.Value.ToString("0.00");
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.Picklist:
						if (field is ListControl)
						{
							var control = (ListControl)field;

							ListItem listItem = null;
							var setValue = value as OptionSetValue;
							if (setValue != null)
							{
								var optionSetValue = setValue;
								listItem = control.Items.FindByValue(optionSetValue.Value.ToString(CultureInfo.InvariantCulture));
							}
							else if (value is string)
							{
								listItem = control.Items.FindByValue((string)value) ?? control.Items.FindByText((string)value);
							}

							if (listItem != null)
							{
								control.ClearSelection();
								listItem.Selected = true;
							}
							else
							{
								return false;
							}
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.State:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported. The state attribute is created automatically when the entity is created. The options available for this attribute are read-only.", attributeTypeCode));
						break;
					case AttributeTypeCode.Status:
						if (field is ListControl)
						{
							var control = (ListControl)field;

							ListItem listItem = null;
							var setValue = value as OptionSetValue;
							if (setValue != null)
							{
								var optionSetValue = setValue;
								listItem = control.Items.FindByValue(optionSetValue.Value.ToString(CultureInfo.InvariantCulture));
							}
							else if (value is string)
							{
								listItem = control.Items.FindByValue((string)value) ?? control.Items.FindByText((string)value);
							}

							if (listItem != null)
							{
								control.ClearSelection();
								listItem.Selected = true;
							}
							else
							{
								return false;
							}
						}
						else
						{
							return false;
						}
						break;
					case AttributeTypeCode.String:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else
						{
							return false;
						}
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported.", attributeTypeCode));
						return false;
				}
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute specified is expecting a {0}. The value provided is not valid.", attributeTypeCode));

				return false;
			}

			return true;
		}

		protected static bool TrySetLookupFieldValue(OrganizationServiceContext context, Control container, EntityReference value, string attributeName)
		{
			try
			{
				var field = container.FindControl(attributeName);

				if (field == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Could not find control ");

					return false;
				}

				var list = field as DropDownList;
				var hiddenField = field as HtmlInputHidden;
				var modalLookup = hiddenField != null;

				if (list == null && hiddenField == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Field is not one of the expected control types.");

					return false;
				}

				// only set the field value if it is blank.

				if (modalLookup)
				{
					if (!string.IsNullOrWhiteSpace(hiddenField.Value))
					{
						return false;
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(list.SelectedValue))
					{
						return false;
					}
				}

				var disabled = list != null && list.CssClass.Contains("readonly");

				var text = string.IsNullOrWhiteSpace(value.Name) ? string.Empty : value.Name;

				if (disabled)
				{
					list.Items.Add(new ListItem
					{
						Value = value.Id.ToString(),
						Text = text
					});
				}

				if (!modalLookup)
				{
					list.SelectedValue = value.Id.ToString();
				}
				else
				{
					hiddenField.Value = value.Id.ToString();

					var nameField = container.FindControl(string.Format("{0}_name", attributeName));

					if (nameField != null)
					{
						var nameTextBox = nameField as TextBox;

						if (nameTextBox != null)
						{
							nameTextBox.Text = text;
						}
					}

					var entityNameField = container.FindControl(string.Format("{0}_entityname", attributeName));

					if (entityNameField != null)
					{
						var nameTextBox = entityNameField as HtmlInputHidden;

						if (nameTextBox != null)
						{
							nameTextBox.Value = value.LogicalName;
						}
					}
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Field value could not be set. {0}", e.ToString()));

				return false;
			}

			return true;
		}

		protected void PopulateReferenceEntityField(OrganizationServiceContext context, Entity step, Control container)
		{
			var populateReferenceEntityLookupField = step.GetAttributeValue<bool?>("adx_populatereferenceentitylookupfield") ?? false;

			if (!populateReferenceEntityLookupField) return;

			try
			{
				var targetAttributeName = step.GetAttributeValue<string>("adx_referencetargetlookupattributelogicalname");
				var field = container.FindControl(targetAttributeName);

				if (field == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not find control"));
					return;
				}

				var list = field as DropDownList;
				var hiddenField = field as HtmlInputHidden;
				var modalLookup = hiddenField != null;

				if (list == null && hiddenField == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Field is not one of the expected control types.");
					return;
				}

				// only set the field value if it is blank.
				if ((modalLookup) && !string.IsNullOrWhiteSpace(hiddenField.Value)) return;

				if (!string.IsNullOrWhiteSpace(list.SelectedValue)) return;

				var disabled = list != null && list.CssClass.Contains("readonly");
				var id = Guid.Empty;
				var text = string.Empty;
				var referenceEntitySourceType = step.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype");
				var referenceEntityLogicalName = step.GetAttributeValue<string>("adx_referenceentitylogicalname");
				var referenceQueryStringName = step.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
				var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
				var querystringIsPrimaryKey = step.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;
				var referenceEntityStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");
				var referenceEntityPrimaryKeyLogicalName = step.GetAttributeValue<string>("adx_referenceentityprimarykeylogicalname");
				var primaryNameAttribute = string.Empty;

				if (string.IsNullOrWhiteSpace(targetAttributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_referenctargetlookupattributelogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(referenceEntityLogicalName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_referenceentitylogicalname must not be null.");
					return;
				}

				if (disabled || modalLookup)
				{
					var entityMetadata = context.GetEntityMetadata(referenceEntityLogicalName);
					primaryNameAttribute = entityMetadata.PrimaryNameAttribute;
				}

				if (referenceEntitySourceType != null)
				{
					switch (referenceEntitySourceType.Value)
					{
						case 100000000: // Query String
							if (!querystringIsPrimaryKey)
							{
								var referenceQueryAttributeName = step.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
								var entity = context.RetrieveSingle(
											new Fetch
											{
												Entity = new FetchEntity(referenceEntityLogicalName)
												{
													Filters = new[] { new Filter { Conditions = new[] { new Condition(referenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
												}
											});

								if (entity == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not retrieve entity of type '{0}' where '{1}' equals '{2}'.", referenceEntityLogicalName, referenceQueryAttributeName, referenceQueryStringValue));
									return;
								}
								id = entity.Id;
								text = entity.GetAttributeValue<string>(primaryNameAttribute) ?? string.Empty;
							}
							else
							{
								if (!Guid.TryParse(referenceQueryStringValue, out id))
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, "id provided in the query string is not a valid guid.");
									return;
								}
							}
							break;
						case 100000001: // Previous Step
							var referenceEntityID = referenceEntityStep != null ? GetStepReferenceEntityID(referenceEntityStep.Id) : GetPreviousStepReferenceEntityID();
							if (referenceEntityID == Guid.Empty)
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, "entityReferenceId is null");
								return;
							}
							id = referenceEntityID;
							break;
					}
				}
				else
				{
					var referenceEntityID = referenceEntityStep != null ? GetStepReferenceEntityID(referenceEntityStep.Id) : GetPreviousStepReferenceEntityID();

					if (referenceEntityID == Guid.Empty)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "entityReferenceId is null");

						return;
					}

					id = referenceEntityID;
				}

				if (disabled || modalLookup)
				{
					if (text == string.Empty)
					{
						if (string.IsNullOrWhiteSpace(referenceEntityPrimaryKeyLogicalName))
						{
							referenceEntityPrimaryKeyLogicalName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, referenceEntityLogicalName);
						}

						if (string.IsNullOrWhiteSpace(referenceEntityPrimaryKeyLogicalName))
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error retrieving the Primary Key Attribute Name for '{0}'.", referenceEntityLogicalName));
							return;
						}

						var entity = context.RetrieveSingle(referenceEntityLogicalName, referenceEntityPrimaryKeyLogicalName, id, FetchAttribute.All);

						if (entity == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not retrieve entity of type '{0}' where '{1}' equals '{2}'.", referenceEntityLogicalName, referenceEntityPrimaryKeyLogicalName, id));
							return;
						}

						text = entity.GetAttributeValue<string>(primaryNameAttribute) ?? string.Empty;
					}

					if (!modalLookup)
					{
						list.Items.Add(new ListItem
						{
							Value = id.ToString(),
							Text = text
						});
					}
				}

				if (!modalLookup)
				{
					list.SelectedValue = id.ToString();
				}
				else
				{
					hiddenField.Value = id.ToString();

					var nameField = container.FindControl(string.Format("{0}_name", targetAttributeName));

					if (nameField != null)
					{
						var nameTextBox = nameField as TextBox;

						if (nameTextBox != null)
						{
							nameTextBox.Text = text;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		protected void RenderReferenceEntityForm(OrganizationServiceContext context, Entity step, Control container)
		{
			var showReadOnlyForm = step.GetAttributeValue<bool?>("adx_referenceentityshowreadonlyform") ?? false;

			if (!showReadOnlyForm) return;

			try
			{
				var id = Guid.Empty;
				var referenceEntitySourceType = step.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype");
				var targetAttributeName = step.GetAttributeValue<string>("adx_referencetargetlookupattributelogicalname");
				var referenceEntityLogicalName = step.GetAttributeValue<string>("adx_referenceentitylogicalname");
				var referenceEntityPrimaryKeyLogicalName = step.GetAttributeValue<string>("adx_referenceentityprimarykeylogicalname");
				var referenceQueryStringName = step.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
				var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
				var querystringIsPrimaryKey = step.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;
				var readOnlyFormName = step.GetAttributeValue<string>("adx_referenceentityreadonlyformname");
				var referenceEntityStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");

				if (string.IsNullOrWhiteSpace(readOnlyFormName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_referenceentityreadonlyformname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(targetAttributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_referenctargetlookupattributelogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(referenceEntityLogicalName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_referenceentitylogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(referenceEntityPrimaryKeyLogicalName))
				{
					referenceEntityPrimaryKeyLogicalName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, referenceEntityLogicalName);
				}

				if (string.IsNullOrWhiteSpace(referenceEntityPrimaryKeyLogicalName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error retrieving the Primary Key Attribute Name for '{0}'.", referenceEntityLogicalName));
					return;
				}

				if (referenceEntitySourceType != null)
				{
					switch (referenceEntitySourceType.Value)
					{
						case 100000000: // Query String
							if (!querystringIsPrimaryKey)
							{
								var referenceQueryAttributeName = step.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
								var entity = context.RetrieveSingle(
											new Fetch
											{
												Entity = new FetchEntity(referenceEntityLogicalName)
												{
													Filters = new[] { new Filter { Conditions = new[] { new Condition(referenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
												}
											});

								if (entity == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not retrieve entity of type '{0}' where '{1}' equals '{2}'.", referenceEntityLogicalName, referenceQueryAttributeName, referenceQueryStringValue));
									return;
								}
								id = entity.Id;
							}
							else
							{
								if (!Guid.TryParse(referenceQueryStringValue, out id))
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, "id provided in the query string is not a valid guid.");
									return;
								}
							}
							break;
						case 100000001: // Previous Step
							var referenceEntityID = referenceEntityStep != null ? GetStepReferenceEntityID(referenceEntityStep.Id) : GetPreviousStepReferenceEntityID();
							if (referenceEntityID == Guid.Empty)
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, "entityReferenceId is null");
								return;
							}
							id = referenceEntityID;
							break;
					}
				}
				else
				{
					var referenceEntityID = referenceEntityStep != null ? GetStepReferenceEntityID(referenceEntityStep.Id) : GetPreviousStepReferenceEntityID();

					if (referenceEntityID == Guid.Empty)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "entityReferenceId is null");
						return;
					}

					id = referenceEntityID;
				}

				var formViewDataSource = new CrmDataSource { ID = string.Format("ReadOnlyFormDataSource_{0}", referenceEntityLogicalName), CrmDataContextName = PortalName, IsSingleSource = true };
				var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", referenceEntityLogicalName, referenceEntityPrimaryKeyLogicalName, id);

				formViewDataSource.FetchXml = fetchXml;

				var formView = new CrmEntityFormView
				{
					ID = string.Format("ReadOnlyFormView_{0}", referenceEntityLogicalName),
					Mode = FormViewMode.ReadOnly,
					FormName = readOnlyFormName,
					EntityName = referenceEntityLogicalName,
					DataSourceID = formViewDataSource.ID,
					CssClass = "read-only",
					EnableEntityPermissions = EnableEntityPermissions,
					ContextName = PortalName,
					LanguageCode = LanguageCode
				};

				container.Controls.Add(formViewDataSource);

				container.Controls.Add(formView);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		private void SetEntityReference(OrganizationServiceContext context, Entity step, IDictionary<string, object> values)
		{
			var setEntityReference = step.GetAttributeValue<bool?>("adx_setentityreference") ?? false;

			if (!setEntityReference) return;

			var id = Guid.Empty;
			var targetAttributeName = step.GetAttributeValue<string>("adx_referencetargetlookupattributelogicalname");
			var referenceEntityLogicalName = step.GetAttributeValue<string>("adx_referenceentitylogicalname");
			var referenceEntityRelationshipName = step.GetAttributeValue<string>("adx_referenceentityrelationshipname") ??
				string.Empty;
			var referenceEntityStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var referenceEntitySourceType = step.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype");

			if (string.IsNullOrWhiteSpace(targetAttributeName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					"Lookup Attribute Name not provided. No Entity Reference to set. AssociateEntity will be called during OnInserted event instead.");
				return;
			}

			try
			{
				if (referenceEntitySourceType != null)
				{
					switch (referenceEntitySourceType.Value)
					{
						case 100000000: // Query String
							var referenceQueryStringName = step.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
							var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
							var querystringIsPrimaryKey = step.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;

							if (!querystringIsPrimaryKey)
							{
								var referenceQueryAttributeName = step.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
								var entity =
									context.RetrieveSingle(
											new Fetch
											{
												Entity = new FetchEntity(referenceEntityLogicalName)
												{
													Filters = new[] { new Filter { Conditions = new[] { new Condition(referenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
												}
											});

								if (entity != null) id = entity.Id;
							}
							else
							{
								Guid.TryParse(referenceQueryStringValue, out id);
							}
							break;
						case 100000001: // Previous Step
							id = referenceEntityStep != null
								? GetStepReferenceEntityID(referenceEntityStep.Id)
								: GetPreviousStepReferenceEntityID();
							break;
						case 100000002: // Record Associated to current user 
							var relationship = step.GetAttributeValue<string>("adx_referencerecordsourcerelationshipname");  //Need to add attribute
							if (string.IsNullOrWhiteSpace(relationship))
							{
								throw new ApplicationException("Required Relationship Name has not been specified for the Record Source Type 'Record Associated to Current Portal User'.");
							}
							if (portalContext.User == null)
							{
								throw new ApplicationException("Couldn't load user record. Portal context User is null.");
							}
							string userPrimaryKey;
							switch (portalContext.User.LogicalName)
							{
								case "contact":
									userPrimaryKey = "contactid";
									break;
								case "systemuser":
									userPrimaryKey = "systemuserid";
									break;
								default:
									throw new ApplicationException(string.Format("The user entity type {0} isn't supported.", portalContext.User.LogicalName));
							}
							var user = context.RetrieveSingle(portalContext.User.LogicalName, userPrimaryKey, portalContext.User.Id, FetchAttribute.All);

							if (user == null)
							{
								throw new ApplicationException(string.Format("Couldn't load user record. Portal context User could not be found with id equal to {0}.", portalContext.User.Id));
							}
							var source = context.RetrieveRelatedEntity(user, relationship);

							id = source.Id;
							break;
					}

				}
				else
				{
					id = referenceEntityStep != null
						? GetStepReferenceEntityID(referenceEntityStep.Id)
						: GetPreviousStepReferenceEntityID();
				}

				if (!string.IsNullOrWhiteSpace(targetAttributeName) && !string.IsNullOrWhiteSpace(referenceEntityLogicalName) && id != Guid.Empty)
				{
					values[targetAttributeName] = new EntityReference(referenceEntityLogicalName, id);
				}
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		protected WebForms.WebFormEntitySourceDefinition GetUserControlReferenceEntityDefinition(OrganizationServiceContext context, Entity step)
		{
			step.AssertEntityName("adx_webformstep");

			var entityName = string.Empty;
			var keyName = string.Empty;
			var id = Guid.Empty;
			var referenceEntitySourceType = step.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype");
			var referenceEntityStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");

			try
			{
				if (referenceEntitySourceType != null)
				{
					switch (referenceEntitySourceType.Value)
					{
						case 100000000: // Query String
							entityName = step.GetAttributeValue<string>("adx_referenceentitylogicalname");
							var referenceQueryStringName = step.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
							var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
							var querystringIsPrimaryKey = step.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;

							if (!querystringIsPrimaryKey)
							{
								var referenceQueryAttributeName = step.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
								var entity =
									context.RetrieveSingle(
											new Fetch
											{
												Entity = new FetchEntity(entityName)
												{
													Filters = new[] { new Filter { Conditions = new[] { new Condition(referenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
												}
											});

								if (entity != null)
								{
									id = entity.Id;
								}
							}
							else
							{
								Guid.TryParse(referenceQueryStringValue, out id);
							}
							break;
						case 100000001: // Previous Step
							if (referenceEntityStep != null)
							{
								var targetEntityDefinition = GetStepReferenceEntityDefinition(referenceEntityStep.Id);
								if (targetEntityDefinition == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to retrieve entity definition from history for step with id equal to '{0}'.", referenceEntityStep.Id));
									return null;
								}
								id = targetEntityDefinition.ID;
								entityName = targetEntityDefinition.LogicalName;
								keyName = targetEntityDefinition.PrimaryKeyLogicalName;
							}
							else
							{
								var targetEntityDefinition = GetPreviousStepReferenceEntityDefinition();
								if (targetEntityDefinition == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to retrieve entity definition from the previous step");
									return null;
								}
								id = targetEntityDefinition.ID;
								entityName = targetEntityDefinition.LogicalName;
								keyName = targetEntityDefinition.PrimaryKeyLogicalName;
							}
							break;
					}
				}
				else
				{
					if (referenceEntityStep != null)
					{
						var targetEntityDefinition = GetStepReferenceEntityDefinition(referenceEntityStep.Id);
						if (targetEntityDefinition == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to retrieve entity definition from history for step with id equal to '{0}'.", referenceEntityStep.Id));
							return null;
						}
						id = targetEntityDefinition.ID;
						entityName = targetEntityDefinition.LogicalName;
						keyName = targetEntityDefinition.PrimaryKeyLogicalName;
					}
					else
					{
						var targetEntityDefinition = GetPreviousStepReferenceEntityDefinition();
						if (targetEntityDefinition == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to retrieve entity definition from the previous step");
							return null;
						}
						id = targetEntityDefinition.ID;
						entityName = targetEntityDefinition.LogicalName;
						keyName = targetEntityDefinition.PrimaryKeyLogicalName;
					}
				}

				if (string.IsNullOrWhiteSpace(keyName) && !string.IsNullOrWhiteSpace(entityName))
				{
					keyName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, entityName);
				}
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
			}

			return new WebForms.WebFormEntitySourceDefinition(entityName, keyName, id);
		}

		protected void AssociateEntity(OrganizationServiceContext context, Entity step, Guid sourceEntityId)
		{
			var setEntityReference = step.GetAttributeValue<bool?>("adx_setentityreference") ?? false;

			if (!setEntityReference) return;

			var targetEntityId = Guid.Empty;
			var targetEntityPrimaryKey = string.Empty;
			var sourceEntityName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var sourceEntityPrimaryKey = step.GetAttributeValue<string>("adx_targetentityprimarykeylogicalname");
			var targetEntityName = step.GetAttributeValue<string>("adx_referenceentitylogicalname");
			var referenceEntitySourceType = step.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype");
			var relationshipName = step.GetAttributeValue<string>("adx_referenceentityrelationshipname") ?? string.Empty;
			var referenceEntityStep = step.GetAttributeValue<EntityReference>("adx_referenceentitystep");

			if (string.IsNullOrWhiteSpace(relationshipName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Entity Relationship Name not provided. Entity Association not required.");
				return;
			}

			try
			{
				if (referenceEntitySourceType != null)
				{
					switch (referenceEntitySourceType.Value)
					{
						case 100000000: // Query String
							var referenceQueryStringName = step.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
							var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];
							var querystringIsPrimaryKey = step.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;

							if (!querystringIsPrimaryKey)
							{
								var referenceQueryAttributeName = step.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
								var entity =
									context.RetrieveSingle(
											new Fetch
											{
												Entity = new FetchEntity(targetEntityName)
												{
													Filters = new[] { new Filter { Conditions = new[] { new Condition(referenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
												}
											});

								if (entity != null) targetEntityId = entity.Id;
							}
							else
							{
								Guid.TryParse(referenceQueryStringValue, out targetEntityId);
							}
							break;
						case 100000001: // Previous Step
							if (referenceEntityStep != null)
							{
								var targetEntityDefinition = GetStepReferenceEntityDefinition(referenceEntityStep.Id);
								if (targetEntityDefinition == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to retrieve entity definition from history for step with id equal to '{0}'.", referenceEntityStep.Id));
									return;
								}
								targetEntityId = targetEntityDefinition.ID;
								targetEntityName = targetEntityDefinition.LogicalName;
								targetEntityPrimaryKey = targetEntityDefinition.PrimaryKeyLogicalName;
							}
							else
							{
								var targetEntityDefinition = GetPreviousStepReferenceEntityDefinition();
								if (targetEntityDefinition == null)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to retrieve entity definition from the previous step");
									return;
								}
								targetEntityId = targetEntityDefinition.ID;
								targetEntityName = targetEntityDefinition.LogicalName;
								targetEntityPrimaryKey = targetEntityDefinition.PrimaryKeyLogicalName;
							}
							break;
					}
				}
				else
				{
					targetEntityId = referenceEntityStep != null ? GetStepReferenceEntityID(referenceEntityStep.Id) : GetPreviousStepReferenceEntityID();
				}

				if (sourceEntityId == Guid.Empty || targetEntityId == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Source and Target entity ids must not be null or empty.");
					return;
				}

				// get the source entity

				if (string.IsNullOrWhiteSpace(sourceEntityName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_targetentitylogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(sourceEntityPrimaryKey))
				{
					sourceEntityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, sourceEntityName);
				}

				if (string.IsNullOrWhiteSpace(sourceEntityPrimaryKey))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to determine source entity primary key logical name.");
					return;
				}

				var sourceEntity = context.RetrieveSingle(sourceEntityName, sourceEntityPrimaryKey, sourceEntityId, FetchAttribute.All);

				if (sourceEntity == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Source entity is null.");
					return;
				}

				// Get the target entity

				if (string.IsNullOrWhiteSpace(targetEntityName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Target entity name must not be null or empty.");
					return;
				}

				if (string.IsNullOrWhiteSpace(targetEntityPrimaryKey))
				{
					targetEntityPrimaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, targetEntityName);
				}

				if (string.IsNullOrWhiteSpace(targetEntityPrimaryKey))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to determine target entity primary key logical name.");
					return;
				}

				var targetEntity = context.RetrieveSingle(targetEntityName, targetEntityPrimaryKey, targetEntityId, FetchAttribute.All);

				if (targetEntity == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Target entity is null.");
					return;
				}

				context.AddLink(sourceEntity, relationshipName, targetEntity);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		protected virtual void DisplaySuccess(object sender, bool hideForm)
		{
			DisplayMessage(sender, SuccessMessage, "success", hideForm);
		}

		protected virtual void DisplayMessage(object sender, string message, string cssClass, bool hideForm)
		{
			var formView = (CrmEntityFormView)sender;
			var container = formView.Parent;
			var messagePanel = (Panel)container.FindControl("MessagePanel");
			var formPanel = (Panel)container.FindControl("WebFormPanel");
			var messageLabel = (System.Web.UI.WebControls.Label)container.FindControl("MessageLabel");

			if (messagePanel == null || formPanel == null || messageLabel == null)
			{
				return;
			}

			messageLabel.Text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			formPanel.Visible = !hideForm;

			messagePanel.Visible = true;
		}

		protected virtual void DisplayMessage(object sender, string snippetName, string defaultText, string cssClass, bool hideForm)
		{
			if (sender == null || string.IsNullOrWhiteSpace(snippetName)) return;

			var container = sender as WebForm;

			if (container == null) return;

			var messagePanel = (Panel)container.FindControl("MessagePanel");

			if (messagePanel == null)
			{
				messagePanel = new Panel { ID = "MessagePanel", Visible = false, CssClass = "message alert" };
				messagePanel.Attributes.Add("role", "alert");
				container.Controls.Add(messagePanel);
			}

			var snippet = new Snippet
			{
				SnippetName = snippetName,
				EditType = "html",
				Editable = true,
				DefaultText = defaultText ?? string.Empty
			};

			messagePanel.Controls.Add(snippet);

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			var formPanel = (Panel)container.FindControl("WebFormPanel");

			if (formPanel != null) formPanel.Visible = !hideForm;

			messagePanel.Visible = true;
		}

		protected virtual void DisplayMessage(Control container, string message, string cssClass, bool hideForm)
		{
			if (container == null || string.IsNullOrWhiteSpace(message)) return;

			var messagePanel = (Panel)container.FindControl("MessagePanel");
			var formPanel = (Panel)container.FindControl("WebFormPanel");
			var messageLabel = (System.Web.UI.WebControls.Label)container.FindControl("MessageLabel");

			if (messagePanel == null || formPanel == null || messageLabel == null) return;

			messageLabel.Text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			formPanel.Visible = !hideForm;

			messagePanel.Visible = true;
		}

		protected virtual void DisplayMessage(Control container, string message, string cssClass)
		{
			if (container == null || string.IsNullOrWhiteSpace(message)) return;

			var messagePanel = new Panel { ID = "MessagePanel", CssClass = "message" };
			var messageLabel = new System.Web.UI.WebControls.Label { ID = "MessageLabel", Text = string.IsNullOrWhiteSpace(message) ? string.Empty : message };

			if (!string.IsNullOrWhiteSpace(cssClass))
			{
				messagePanel.CssClass = string.Format("{0} {1}", messagePanel.CssClass, cssClass);
			}

			messagePanel.Controls.Add(messageLabel);

			container.Controls.Add(messagePanel);
		}

		protected void LogUserInfoOnInserting(OrganizationServiceContext context, Entity step, CrmEntityFormViewInsertingEventArgs e)
		{
			var logUserInfo = step.GetAttributeValue<bool?>("adx_loguser") ?? false;

			if (!logUserInfo) return;

			var userHostAddressAttributeName = step.GetAttributeValue<string>("adx_userhostaddressattributelogicalname");
			var userIdentityNameAttributeName = step.GetAttributeValue<string>("adx_useridentitynameattributelogicalname");
			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_targetentitylogicalname must not be null.");

				return;
			}

			if (string.IsNullOrWhiteSpace(userHostAddressAttributeName) && string.IsNullOrWhiteSpace(userIdentityNameAttributeName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "LogUserInfoOnInserting", "adx_webformstep.adx_userhostaddressattributelogicalname is null. adx_webformstep.adx_useridentitynameattributelogicalname is null.");

				return;
			}

			if (!string.IsNullOrWhiteSpace(userHostAddressAttributeName))
			{
				if (MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, userHostAddressAttributeName))
				{
					if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.UserHostName))
					{
						e.Values[userHostAddressAttributeName] = HttpContext.Current.Request.UserHostName;
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(userIdentityNameAttributeName))
			{
				if (MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, userIdentityNameAttributeName))
				{
					if (HttpContext.Current.User != null && !string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
					{
						e.Values[userIdentityNameAttributeName] = HttpContext.Current.User.Identity.Name;
					}
				}
			}
		}

		protected void LogUserInfoOnUpdating(OrganizationServiceContext context, Entity step, CrmEntityFormViewUpdatingEventArgs e)
		{
			var logUserInfo = step.GetAttributeValue<bool?>("adx_loguser") ?? false;

			if (!logUserInfo) return;

			var userHostAddressAttributeName = step.GetAttributeValue<string>("adx_userhostaddressattributelogicalname");
			var userIdentityNameAttributeName = step.GetAttributeValue<string>("adx_useridentitynameattributelogicalname");
			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_targetentitylogicalname must not be null.");

				return;
			}

			if (string.IsNullOrWhiteSpace(userHostAddressAttributeName) && string.IsNullOrWhiteSpace(userIdentityNameAttributeName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_userhostaddressattributelogicalname is null. adx_webformstep.adx_useridentitynameattributelogicalname is null.");

				return;
			}

			if (!string.IsNullOrWhiteSpace(userHostAddressAttributeName))
			{
				if (MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, userHostAddressAttributeName))
				{
					if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.UserHostName))
					{
						e.Values[userHostAddressAttributeName] = HttpContext.Current.Request.UserHostName;
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(userIdentityNameAttributeName))
			{
				if (MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, userIdentityNameAttributeName))
				{
					if (HttpContext.Current.User != null && !string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
					{
						e.Values[userIdentityNameAttributeName] = HttpContext.Current.User.Identity.Name;
					}
				}
			}
		}

		protected void SetAutoNumber(OrganizationServiceContext context, Entity step, CrmEntityFormViewInsertingEventArgs e)
		{
			var createAutoNumber = step.GetAttributeValue<bool?>("adx_createautonumber") ?? false;
			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var autoNumberDefinitionName = step.GetAttributeValue<string>("adx_autonumberdefinitionname");
			var autoNumberAttributeLogicalName = step.GetAttributeValue<string>("adx_autonumberattributelogicalname");

			if (!createAutoNumber)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_targetentitylogicalname must not be null.");

				return;
			}

			if (string.IsNullOrWhiteSpace(autoNumberDefinitionName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_autonumberdefinitionname is null.");

				return;
			}

			if (string.IsNullOrWhiteSpace(autoNumberAttributeLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformstep.adx_autonumberattributelogicalname is null.");

				return;
			}

			var autoNumber = RetrieveAutoNumber(autoNumberDefinitionName);

			if (string.IsNullOrWhiteSpace(autoNumber))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "autoNumber returned from plugin request is null.");

				return;
			}

			if (MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, autoNumberAttributeLogicalName))
			{
				e.Values[autoNumberAttributeLogicalName] = autoNumber;
			}
		}

		protected string RetrieveAutoNumber(string autoNumberDefinitionName)
		{
			try
			{
				// use command pattern - create an auto-number request

				var service = PortalCrmConfigurationManager.CreateOrganizationService(PortalName);
				var numberEntity = new Entity("adx_autonumberingrequest");

				numberEntity.Attributes["adx_name"] = autoNumberDefinitionName;

				var numberEntityId = service.Create(numberEntity);

				numberEntity = service.Retrieve("adx_autonumberingrequest", numberEntityId, new ColumnSet(new[] { "adx_name", "adx_formattednumber" }));

				var formattedNumber = numberEntity.GetAttributeValue<string>("adx_formattednumber");

				return formattedNumber;
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());

				return string.Empty;
			}
		}

		protected void AssociateCurrentPortalUser(OrganizationServiceContext context, Entity step, CrmEntityFormViewInsertingEventArgs e)
		{
			if (!HttpContext.Current.Request.IsAuthenticated) return;

			var associatePortalUser = step.GetAttributeValue<bool?>("adx_associatecurrentportaluser") ?? false;
			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var portalUserLookupAttributeName = step.GetAttributeValue<string>("adx_targetentityportaluserlookupattribute");
			var portalUserLookupAttributeIsActivityParty = step.GetAttributeValue<bool?>("adx_portaluserlookupattributeisactivityparty") ?? false;

			if (!associatePortalUser) return;

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));

				return;
			}

			if (string.IsNullOrWhiteSpace(portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "adx_webformstep.adx_targetentityportaluserlookupattribute is null.");

				return;
			}

			if (!MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' entity does not contain an attribute.", EntityNamePrivacy.GetEntityName(targetEntityLogicalName)));

				return;
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var portalUser = portalContext.User;

			if (portalUser != null)
			{
				try
				{
					if (portalUserLookupAttributeIsActivityParty)
					{
						var activityParty = new Entity("activityparty");

						activityParty["partyid"] = new EntityReference(portalUser.LogicalName, portalUser.Id);

						e.Values[portalUserLookupAttributeName] = new[] { activityParty };
					}
					else
					{
						e.Values[portalUserLookupAttributeName] = portalUser.ToEntityReference();
					}
				}
				catch (Exception ex)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
				}

			}
			else
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Current Portal User is null.");

			}
		}

		protected void AttachFileOnItemInserted(OrganizationServiceContext context, Entity step, object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			Guid? entityid = Guid.Empty;

			if (e.EntityId != null && e.EntityId != Guid.Empty)
			{
				entityid = e.EntityId;
			}

			AttachFileOnSave(context, step, sender, entityid);
		}

		protected void AttachFileOnItemUpdated(OrganizationServiceContext context, Entity step, object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			var entityid = Guid.Empty;

			if (e.Entity != null && e.Entity.Id != Guid.Empty)
			{
				entityid = e.Entity.Id;
			}

			AttachFileOnSave(context, step, sender, entityid);
		}

		protected void AttachFileOnSave(OrganizationServiceContext context, Entity step, object sender, Guid? entityid)
		{
			var attachFile = step.GetAttributeValue<bool?>("adx_attachfile") ?? false;

			if (!attachFile) return;

			if (entityid == null || entityid == Guid.Empty)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "File not saved entityid is null or empty.");

				return;
			}

			try
			{
				var logicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
				var primaryKey = step.GetAttributeValue<string>("adx_targetentityprimarykeylogicalname");

				if (string.IsNullOrWhiteSpace(logicalName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_entityname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(primaryKey))
				{
					primaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, logicalName);
				}

				if (string.IsNullOrWhiteSpace(primaryKey))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, ResourceManager.GetString("Failed_To_Determine_Target_Entity_Pk_Logical_Name_Exception"));
					return;
				}

				var entity = context.RetrieveSingle(logicalName, primaryKey, entityid.Value, FetchAttribute.All);

				var formView = (CrmEntityFormView)sender;
				var fileUpload = (FileUpload)formView.FindControl("AttachFile");

				if (!fileUpload.HasFiles) return;

				var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var portalUser = portalContext.User == null ? null : portalContext.User.ToEntityReference();

				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(
					requestContext: HttpContext.Current.Request.RequestContext, portalName: PortalName);

				var regarding = entity.ToEntityReference();

				if (_attachmentSaveOption == AttachFileSaveOption.PortalComment)
				{
					var portalCommentDataAdapter = new ActivityDataAdapter(dataAdapterDependencies);
					var crmUser = portalCommentDataAdapter.GetCRMUserActivityParty(regarding, "ownerid");
					var portalActivityPartyUser = new Entity("activityparty");
					portalActivityPartyUser["partyid"] = dataAdapterDependencies.GetPortalUser();

					var portalComment = new PortalComment
					{
						Description = string.Empty,
						From = portalActivityPartyUser,
						To = crmUser,
						Regarding = regarding,
						AttachmentSettings = _annotationSettings,
						StateCode = StateCode.Completed,
						StatusCode = StatusCode.Received,
						DirectionCode = PortalCommentDirectionCode.Incoming
					};

					var fileAttachments = new List<IAnnotationFile>();
					foreach (var uploadedFile in fileUpload.PostedFiles)
					{
						fileAttachments.Add(AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(uploadedFile), _annotationSettings.StorageLocation));
					}
					portalComment.FileAttachments = fileAttachments;

					portalCommentDataAdapter.CreatePortalComment(portalComment);
				}
				else
				{
					IAnnotationDataAdapter notesDataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);

					foreach (var uploadedFile in fileUpload.PostedFiles)
					{
						var annotation = new Annotation
						{
							Regarding = regarding,
							Subject = AnnotationHelper.BuildNoteSubject(context, portalUser),
							NoteText = AnnotationHelper.WebAnnotationPrefix,
							FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(uploadedFile), _annotationSettings.StorageLocation)
						};

						notesDataAdapter.CreateAnnotation(annotation, _annotationSettings);
					}
				}
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
			}
		}

		protected void SetAttributeValuesOnUpdating(OrganizationServiceContext context, Entity step, CrmEntityFormViewUpdatingEventArgs e)
		{
			SetAttributeValuesOnSave(context, step, e);
		}

		protected void SetAttributeValuesOnInserting(OrganizationServiceContext context, Entity step, CrmEntityFormViewInsertingEventArgs e)
		{
			SetAttributeValuesOnSave(context, step, e);
		}

		protected void SetAttributeValuesOnSave(OrganizationServiceContext context, Entity step, object e)
		{
			var metadata = context.RetrieveRelatedEntities(
				step,
				"adx_webformmetadata_webformstep",
				filters: new[] { new Filter { Conditions = new[] { new Condition("adx_setvalueonsave", ConditionOperator.Equal, true) } } }).Entities;

			if (!metadata.Any()) return;

			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, targetEntityLogicalName);
			}

			foreach (var item in metadata)
			{
				var attributeName = item.GetAttributeValue<string>("adx_attributelogicalname");
				if (string.IsNullOrWhiteSpace(attributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformmetadata.adx_attributelogicalname is null.");
					continue;
				}
				var value = GetOnSaveValue(context, item);

				if (attributeName == "statecode")
				{
					try
					{
						SetStateOnSaveValue = Convert.ToInt32(value);
						SetStateOnSave = true;
					}
					catch (Exception)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to convert statecode value to int.");
					}
				}

				var crmEntityFormViewInsertingEventArgs = e as CrmEntityFormViewInsertingEventArgs;
				if (crmEntityFormViewInsertingEventArgs != null)
				{
					TrySetAttributeValueOnInserting(context, crmEntityFormViewInsertingEventArgs, targetEntityLogicalName, attributeName, value);
				}
				var crmEntityFormViewUpdatingEventArgs = e as CrmEntityFormViewUpdatingEventArgs;
				if (crmEntityFormViewUpdatingEventArgs != null)
				{
					TrySetAttributeValueOnUpdating(context, crmEntityFormViewUpdatingEventArgs, targetEntityLogicalName, attributeName, value);
				}
			}
		}

		protected bool TrySetAttributeValueOnUpdating(OrganizationServiceContext context, CrmEntityFormViewUpdatingEventArgs e, string entityName, string attributeName, object value)
		{
			var attributeValue = TryConvertAttributeValue(context, entityName, attributeName, value);

			if (attributeValue == null) return false;

			try
			{
				e.Values[attributeName] = attributeValue;
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());

				return false;
			}

			return true;
		}

		protected bool TrySetAttributeValueOnInserting(OrganizationServiceContext context, CrmEntityFormViewInsertingEventArgs e, string entityName, string attributeName, object value)
		{
			var attributeValue = TryConvertAttributeValue(context, entityName, attributeName, value);

			if (attributeValue == null) return false;


			try
			{
				e.Values[attributeName] = attributeValue;
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());

				return false;
			}

			return true;
		}

		/// <summary>
		/// Sets the attribute values on the target entity based on the properties specified in the current steps metadata and sets state if necessary then saves changes.
		/// </summary>
		/// <param name="context">Organization Service Context</param>
		/// <param name="entity">Entity</param>
		public void SetAttributeValuesAndSave(OrganizationServiceContext context, Entity entity)
		{
			if (context == null) throw new ArgumentNullException("context");


			if (entity == null) throw new ArgumentNullException("entity");


			var step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			var metadata = context.RetrieveRelatedEntities(
				step,
				"adx_webformmetadata_webformstep",
				filters: new[] { new Filter { Conditions = new[] { new Condition("adx_setvalueonsave", ConditionOperator.Equal, true) } } }).Entities;

			if (!metadata.Any()) return;


			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (targetEntityLogicalName != entity.LogicalName)
			{
				throw new ApplicationException(string.Format("The entity LogicalName {0} doesn't match the step's Target Entity Logical Name {1}.", entity.LogicalName, targetEntityLogicalName));
			}

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, targetEntityLogicalName);
			}

			var setState = false;
			var statecodeValue = 0;

			foreach (var item in metadata)
			{
				var attributeName = item.GetAttributeValue<string>("adx_attributelogicalname");
				if (string.IsNullOrWhiteSpace(attributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformmetadata.adx_attributelogicalname is null.");
					continue;
				}
				var value = GetOnSaveValue(context, item);

				if (attributeName == "statecode")
				{
					try
					{
						statecodeValue = Convert.ToInt32(value);
						setState = true;
					}
					catch (Exception)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to convert statecode value to int.");
					}
				}

				var attributeValue = TryConvertAttributeValue(context, targetEntityLogicalName, attributeName, value);

				if (attributeValue == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Attribute '{0}' not set. Attribute value is null.", attributeName);
					continue;
				}

				try
				{
					entity[attributeName] = attributeValue;
				}
				catch (Exception ex)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
				}
			}

			if (!context.IsAttached(entity)) context.Attach(entity);

			context.UpdateObject(entity);
			context.SaveChanges();

			if (setState) TrySetState(context, entity.ToEntityReference(), statecodeValue);

		}

		/// <summary>
		/// Sets the attribute values on the target entity based on the properties specified in the current steps metadata.
		/// </summary>
		/// <param name="context">Organization Service Context</param>
		/// <param name="entity">Target Entity</param>
		/// <remarks>The logical name of the entity must match the step's target entity logical name.</remarks>
		public bool TrySetAttributeValuesFromMetadata(OrganizationServiceContext context, ref Entity entity)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			var step = context.RetrieveSingle("adx_webformstep", "adx_webformstepid", this.CurrentSessionHistory.CurrentStepId, FetchAttribute.All);

			if (step == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_webformstep where id equals {0}", CurrentSessionHistory.CurrentStepId));
			}

			var metadata = context.RetrieveRelatedEntities(
				step,
				"adx_webformmetadata_webformstep",
				filters: new[] { new Filter { Conditions = new[] { new Condition("adx_setvalueonsave", ConditionOperator.Equal, true) } } }).Entities;

			if (!metadata.Any())
			{
				return false;
			}

			var targetEntityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				throw new ApplicationException("adx_webformstep.adx_targetentitylogicalname is null.");
			}

			if (targetEntityLogicalName != entity.LogicalName)
			{
				throw new ApplicationException(string.Format("The entity LogicalName {0} doesn't match the step's Target Entity Logical Name {1}.", entity.LogicalName, targetEntityLogicalName));
			}

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, targetEntityLogicalName);
			}

			var valueSet = false;

			foreach (var item in metadata)
			{
				var attributeName = item.GetAttributeValue<string>("adx_attributelogicalname");
				if (string.IsNullOrWhiteSpace(attributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_webformmetadata.adx_attributelogicalname is null.");
					continue;
				}
				var value = GetOnSaveValue(context, item);

				var attributeValue = TryConvertAttributeValue(context, targetEntityLogicalName, attributeName, value);

				if (attributeValue == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Attribute not set. Attribute value is null.");
					continue;
				}

				try
				{
					entity[attributeName] = attributeValue;
					valueSet = true;
				}
				catch (Exception ex)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
				}
			}

			return valueSet;
		}

		protected object GetOnSaveValue(OrganizationServiceContext context, Entity item)
		{
			var type = item.GetAttributeValue<OptionSetValue>("adx_onsavetype");

			if (type != null)
			{
				switch (type.Value)
				{
					case 100000001: // Today's Date
						return DateTime.UtcNow;
					case 100000002: // Current Portal User
						if (!HttpContext.Current.Request.IsAuthenticated)
						{
							return null;
						}
						var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
						if (portalContext.User == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, "Current portal user record is null.");
							return null;
						}
						try
						{
							var fromAttribute = item.GetAttributeValue<string>("adx_onsavefromattribute");
							if (string.IsNullOrWhiteSpace(fromAttribute))
							{
								return null;
							}
							if (!MetadataHelper.IsAttributeLogicalNameValid(context, portalContext.User.LogicalName, fromAttribute))
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Not a valid attribute on {0}.", EntityNamePrivacy.GetEntityName(portalContext.User.LogicalName)));
								return null;
							}
							return portalContext.User.GetAttributeValue(fromAttribute);
						}
						catch (Exception ex)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, ex.ToString());
							return null;
						}
				}
			}

			// Value
			return item.GetAttributeValue<string>("adx_onsavevalue");
		}

		protected void TrySetState(OrganizationServiceContext context, EntityReference entityReference, int state)
		{
			try
			{
				context.SetState(state, -1, entityReference);
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to set statecode. {0}", ex.ToString()));
			}
		}

		protected dynamic TryConvertAttributeValue(OrganizationServiceContext context, string entityName, string attributeName, object value)
		{
			return EntityFormFunctions.TryConvertAttributeValue(context, entityName, attributeName, value, AttributeTypeCodeDictionary);
		}

		protected bool TrySetAttributeValue(OrganizationServiceContext context, CrmEntityFormViewInsertingEventArgs e, string entityName, string attributeName, object value)
		{
			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entityName);
			}

			var attributeTypeCode = AttributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				throw new InvalidOperationException(string.Format("Unable to recognize the attribute {0} specified in the expression.", attributeName));
			}

			try
			{
				object newValue;
				switch (attributeTypeCode)
				{
					case AttributeTypeCode.BigInt:
						newValue = value == null ? (object)null : Convert.ToInt64(value);
						break;
					case AttributeTypeCode.Boolean:
						newValue = value == null ? (object)null : Convert.ToBoolean(value);
						break;
					case AttributeTypeCode.Customer:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported.", attributeTypeCode));
						return false;
					case AttributeTypeCode.DateTime:
						newValue = value == null ? (object)null : Convert.ToDateTime(value).ToUniversalTime();
						break;
					case AttributeTypeCode.Decimal:
						newValue = value == null ? (object)null : Convert.ToDecimal(value);
						break;
					case AttributeTypeCode.Double:
						newValue = value == null ? (object)null : Convert.ToDouble(value);
						break;
					case AttributeTypeCode.Integer:
						newValue = value == null ? (object)null : Convert.ToInt32(value);
						break;
					case AttributeTypeCode.Lookup:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute specified has an unsupported type '{0}'.", attributeTypeCode));
						return false;
					case AttributeTypeCode.Memo:
						newValue = value as string;
						break;
					case AttributeTypeCode.Money:
						newValue = value == null ? (object)null : Convert.ToDecimal(value);
						break;
					case AttributeTypeCode.Picklist:
						var plMetadata = MetadataHelper.GetEntityMetadata(context, entityName);
						var plAttribute = plMetadata.Attributes.FirstOrDefault(a => a.LogicalName == attributeName);
						if (plAttribute != null)
						{
							var picklistAttribute = plAttribute as PicklistAttributeMetadata;
							if (picklistAttribute != null)
							{
								int picklistInt;
								OptionMetadata picklistValue;
								if (int.TryParse(string.Empty + value, out picklistInt))
								{
									picklistValue = picklistAttribute.OptionSet.Options.FirstOrDefault(o => o.Value == picklistInt);
								}
								else
								{
									picklistValue = picklistAttribute.OptionSet.Options.FirstOrDefault(o => o.Label.GetLocalizedLabelString() == string.Empty + value);
								}

								if (picklistValue != null && picklistValue.Value.HasValue)
								{
									newValue = value == null ? null : new OptionSetValue(picklistValue.Value.Value);
									break;
								}
							}
						}
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported. The value provided is not valid.", attributeTypeCode));
						return false;
					case AttributeTypeCode.State:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported. The state attribute is created automatically when the entity is created. The options available for this attribute are read-only.", attributeTypeCode));
						return false;
					case AttributeTypeCode.Status:
						if (value == null) return false;
						var optionSetValue = new OptionSetValue(Convert.ToInt32(value));
						newValue = optionSetValue;
						break;
					case AttributeTypeCode.String:
						newValue = value as string;
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported.", attributeTypeCode));
						return false;
				}
				e.Values[attributeName] = newValue;
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute specified is expecting a {0}. The value provided is not valid.", attributeTypeCode));
				return false;
			}
			return true;
		}

		protected string TryConvertAttributeValueToString(OrganizationServiceContext context, string entityName, string attributeName, object value)
		{
			if (context == null || string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(attributeName)) return string.Empty;


			if (AttributeTypeCodeDictionary == null)
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entityName);
			}

			var newValue = string.Empty;
			var attributeTypeCode = AttributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Unable to recognize the attribute specified.");
				return string.Empty;
			}

			try
			{
				switch (attributeTypeCode)
				{
					case AttributeTypeCode.BigInt:
						newValue = value == null ? string.Empty : Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Boolean:
						newValue = value == null ? string.Empty : Convert.ToBoolean(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Customer:
						if (!(value is EntityReference)) break;
						var entityref = value as EntityReference;
						newValue = entityref.Id.ToString();
						break;
					case AttributeTypeCode.DateTime:
						newValue = value == null ? string.Empty : Convert.ToDateTime(value).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Decimal:
						newValue = value == null ? string.Empty : Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Double:
						newValue = value == null ? string.Empty : Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Integer:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Lookup:
						if (!(value is EntityReference)) break;
						var erentityref = value as EntityReference;
						newValue = erentityref.Id.ToString();
						break;
					case AttributeTypeCode.Memo:
						newValue = value as string;
						break;
					case AttributeTypeCode.Money:
						newValue = value == null ? string.Empty : Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Picklist:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.State:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.Status:
						newValue = value == null ? string.Empty : Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						break;
					case AttributeTypeCode.String:
						newValue = value as string;
						break;
					case AttributeTypeCode.Uniqueidentifier:
						if (!(value is Guid)) break;
						var id = (Guid)value;
						newValue = id.ToString();
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute type '{0}' is unsupported.", attributeTypeCode));
						break;
				}
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute specified is expecting a {0}. The value provided is not valid.", attributeTypeCode));
			}
			return newValue;
		}

		protected virtual void RedirectToLoginIfAnonymous()
		{
			if (HttpContext.Current.Request.IsAuthenticated) return;

			HttpContext.Current.Response.ForbiddenAndEndResponse();
		}

		/// <summary>
		/// Saves the Current Session History
		/// </summary>
		/// <param name="context">OrganizationServiceContext</param>
		public void SaveSessionHistory(OrganizationServiceContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (CurrentSessionHistory != null)
			{
				CurrentSessionHistory.Id = SessionHistoryProvider.PersistSessionHistory(context, CurrentSessionHistory);
			}
		}

		protected void AddStepDetailsToStepHistory(OrganizationServiceContext context, Entity step, Guid recordId, Guid previousStepId)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (step == null)
			{
				throw new ArgumentNullException("step");
			}

			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				throw new ApplicationException("Step History is null.");
			}

			CurrentSessionHistory.CurrentStepId = step.Id;

			CurrentSessionHistory.CurrentStepIndex++;

			var entityLogicalName = step.GetAttributeValue<string>("adx_targetentitylogicalname");
			var entityPrimaryKeyLogicalName = step.GetAttributeValue<string>("adx_targetentityprimarykeylogicalname");

			if (!string.IsNullOrWhiteSpace(entityLogicalName) && string.IsNullOrWhiteSpace(entityPrimaryKeyLogicalName))
			{
				entityPrimaryKeyLogicalName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, entityLogicalName);
			}

			UpdateStepHistory(step, previousStepId, recordId, entityLogicalName, entityPrimaryKeyLogicalName, true);

			if (PersistSessionHistory)
			{
				SaveSessionHistory(context);
			}
		}

		protected Guid GetPreviousStepId()
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.CurrentStepIndex == 0)
			{
				return Guid.Empty;
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				throw new ApplicationException("Step History is null.");
			}

			if (!CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is empty");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == CurrentSessionHistory.CurrentStepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", CurrentSessionHistory.CurrentStepId));
			}

			return item.PreviousStepID;
		}

		protected void UpdateStepHistoryIsActive(Entity step, bool isActive)
		{
			if (step == null)
			{
				throw new ArgumentNullException("step");
			}

			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				CurrentSessionHistory.StepHistory = new List<SessionHistory.Step>();
			}

			if (CurrentSessionHistory.StepHistory.Any())
			{
				var history = CurrentSessionHistory.StepHistory.Find(s => s.ID == step.Id);

				if (history != null)
				{
					history.IsActive = isActive;
				}
			}
		}

		protected void UpdateStepHistory(Entity step, Guid previousStepId, Guid? recordID, string entityLogicalName, string entityPrimaryKeyLogicalName, bool isActive)
		{
			if (step == null)
			{
				throw new ArgumentNullException("step");
			}

			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null)
			{
				CurrentSessionHistory.StepHistory = new List<SessionHistory.Step>();
			}

			var referenceEntity = new SessionHistory.ReferenceEntity
			{
				ID = recordID ?? Guid.Empty,
				LogicalName = entityLogicalName,
				PrimaryKeyLogicalName = entityPrimaryKeyLogicalName
			};

			if (CurrentSessionHistory.StepHistory.Any())
			{
				var history = CurrentSessionHistory.StepHistory.Find(s => s.ID == step.Id);

				if (history == null)
				{
					CurrentSessionHistory.StepHistory.Add(new SessionHistory.Step { ID = step.Id, Index = CurrentSessionHistory.CurrentStepIndex, PreviousStepID = previousStepId, ReferenceEntity = referenceEntity, IsActive = isActive });
				}
				else
				{
					if (previousStepId != Guid.Empty)
					{
						history.PreviousStepID = previousStepId;
					}

					if (referenceEntity.ID != Guid.Empty)
					{
						history.ReferenceEntity = referenceEntity;
					}

					history.IsActive = isActive;
				}
			}
			else
			{
				CurrentSessionHistory.StepHistory.Add(new SessionHistory.Step { ID = step.Id, Index = CurrentSessionHistory.CurrentStepIndex, PreviousStepID = previousStepId, ReferenceEntity = referenceEntity, IsActive = isActive });
			}
		}

		protected void UpdateSessionHistoryPrimaryRecordID(Guid recordID)
		{
			if (CurrentSessionHistory == null || recordID == Guid.Empty)
			{
				return;
			}

			if (CurrentSessionHistory.CurrentStepIndex == 0 || CurrentSessionHistory.PrimaryRecord.ID == Guid.Empty)
			{
				CurrentSessionHistory.PrimaryRecord.ID = recordID;
			}
		}

		protected void UpdateStepHistoryReferenceEntityID(Guid entityID)
		{
			if (entityID == Guid.Empty)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "entityID is empty");
				return;
			}

			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is null or empty.");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == CurrentSessionHistory.CurrentStepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", CurrentSessionHistory.CurrentStepId));
			}

			item.ReferenceEntity.ID = entityID;
		}

		protected SessionHistory.ReferenceEntity GetCurrentStepReferenceEntityDefinition()
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is null or empty.");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == CurrentSessionHistory.CurrentStepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", CurrentSessionHistory.CurrentStepId));
			}

			return item.ReferenceEntity;
		}

		protected Guid GetCurrentStepReferenceEntityID()
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is null or empty.");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == CurrentSessionHistory.CurrentStepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", CurrentSessionHistory.CurrentStepId));
			}

			if (item.ReferenceEntity == null)
			{
				throw new ApplicationException("Step History Reference Entity definition is null.");
			}

			return item.ReferenceEntity.ID;
		}

		protected SessionHistory.ReferenceEntity GetPreviousStepReferenceEntityDefinition()
		{
			var previousStepId = GetPreviousStepId();

			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is null or empty.");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == previousStepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", previousStepId));
			}

			return item.ReferenceEntity;
		}

		protected SessionHistory.ReferenceEntity GetStepReferenceEntityDefinition(Guid stepId)
		{
			if (CurrentSessionHistory == null)
			{
				throw new ApplicationException("Current Session History is null.");
			}

			if (CurrentSessionHistory.StepHistory == null || !CurrentSessionHistory.StepHistory.Any())
			{
				throw new ApplicationException("Step History is null or empty.");
			}

			var item = CurrentSessionHistory.StepHistory.Find(s => s.ID == stepId);

			if (item == null)
			{
				throw new ApplicationException(string.Format("Step History didn't contain data with ID = {0}.", stepId));
			}

			return item.ReferenceEntity;
		}

		protected Guid GetPreviousStepReferenceEntityID()
		{
			var referenceEntity = GetPreviousStepReferenceEntityDefinition();

			if (referenceEntity == null)
			{
				throw new ApplicationException("Step History Reference Entity definition is null.");
			}

			return referenceEntity.ID;
		}

		protected Guid GetStepReferenceEntityID(Guid stepId)
		{
			var referenceEntity = GetStepReferenceEntityDefinition(stepId);

			if (referenceEntity == null)
			{
				throw new ApplicationException("Step History Reference Entity definition is null.");
			}

			return referenceEntity.ID;
		}

		protected static List<ProgressStep> GetProgressSteps(OrganizationServiceContext context, Entity startStep, int currentIndex, List<SessionHistory.Step> stepHistory, int languageCode)
		{
			var steps = new List<ProgressStep>();
			var step = startStep;
			var stepIndex = 0;
			var userStepIndex = 0;
			var hasNextStep = true;

			var localizedTitle = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_title"), languageCode);

			steps.Add(new ProgressStep
			{
				Title = string.IsNullOrWhiteSpace(localizedTitle) ? step.GetAttributeValue<string>("adx_name") ?? "Step" : localizedTitle,
				Index = userStepIndex,
				IsActive = currentIndex == stepIndex,
				IsCompleted = currentIndex > stepIndex
			});

			while (hasNextStep)
			{
				var nextStep = context.RetrieveRelatedEntity(
					step,
					new Relationship("adx_webformstep_nextstep") { PrimaryEntityRole = EntityRole.Referencing });

				if (nextStep != null)
				{
					// Check where they have been to determine if they progressed to a condition failed step.

					var item = stepHistory.Find(s => s.ID == nextStep.Id);

					if ((item == null || item.IsActive == false) || item.PreviousStepID != step.Id)
					{
						var stepType = step.GetAttributeValue<OptionSetValue>("adx_type");

						if (stepType != null)
						{
							if (stepType.Value == 100000000) // Condition
							{
								var conditionFailedStep = context.RetrieveRelatedEntity(
															step,
															new Relationship("adx_webformstep_conditiondefaultnextstep") { PrimaryEntityRole = EntityRole.Referencing });

								if (conditionFailedStep != null)
								{
									var stepVisited = stepHistory.Find(s => s.ID == conditionFailedStep.Id);

									if (stepVisited != null)
									{
										nextStep = conditionFailedStep;
									}
								}
							}
						}
					}
				}

				if (nextStep != null)
				{
					var type = nextStep.GetAttributeValue<OptionSetValue>("adx_type");

					if (type == null)
					{
						throw new ApplicationException("Invalid step type.");
					}

					stepIndex++;

					switch (type.Value)
					{
						case 100000000: // Condition
							step = nextStep;
							break;
						case 100000001: // Load Form
							step = nextStep;
							userStepIndex++;
							var localizedTitle1 = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_title"), languageCode);
							steps.Add(new ProgressStep
							{
								Title = string.IsNullOrWhiteSpace(localizedTitle1) ? step.GetAttributeValue<string>("adx_name") ?? "Step" : localizedTitle1,
								Index = userStepIndex,
								IsActive = currentIndex == stepIndex,
								IsCompleted = currentIndex > stepIndex
							});
							break;
						case 100000002: // Load Tab
							step = nextStep;
							userStepIndex++;
							var localizedTitle2 = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_title"), languageCode);
							steps.Add(new ProgressStep
							{
								Title = string.IsNullOrWhiteSpace(localizedTitle2) ? step.GetAttributeValue<string>("adx_name") ?? "Step" : localizedTitle2,
								Index = userStepIndex,
								IsActive = currentIndex == stepIndex,
								IsCompleted = currentIndex > stepIndex
							});
							break;
						case 100000003: // Redirect
							step = nextStep;
							userStepIndex++;
							break;
						case 100000004: // Load User Control
							step = nextStep;
							userStepIndex++;
							var localizedTitle3 = Localization.GetLocalizedString(step.GetAttributeValue<string>("adx_title"), languageCode);
							steps.Add(new ProgressStep
							{
								Title = string.IsNullOrWhiteSpace(localizedTitle3) ? step.GetAttributeValue<string>("adx_name") ?? "Step" : localizedTitle3,
								Index = userStepIndex,
								IsActive = currentIndex == stepIndex,
								IsCompleted = currentIndex > stepIndex
							});
							break;
					}
				}
				else
				{
					hasNextStep = false;
				}
			}

			return steps;
		}

		protected virtual void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null)
				{
					continue;
				}

				var absolutePath = VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}

		internal class ItemTemplate : ITemplate
		{
			private readonly string _validationGroup;
			private readonly bool _captchaIsRequired;
			private readonly bool _attachFile;
			private readonly bool _attachFileAllowMultiple;
			private readonly string _attachFileAccept;
			private readonly bool _attachFileRestrictAccept;
			private readonly string _attachFileAcceptErrorMessage;
			private readonly ulong? _attachFileSize;
			private readonly bool _attachFileRestrictSize;
			private readonly string _attachFileSizeErrorMessage;
			private readonly string _attachFileLabel;
			private readonly bool _attachFileIsRequired;
			private readonly string _attachFileRequiredErrorMessage;
			private readonly string _submitButtonID;
			private readonly string _submitButtonCommandName;
			private readonly string _submitButtonText;
			private readonly string _submitButtonCssClass;
			private readonly bool _submitButtonCauseValidation;
			private readonly bool _addSubmitButton;
			private readonly string _submitButtonBusyText;

			public ItemTemplate(string validationGroup, bool captchaIsRequired, bool attachFile, bool attachFileAllowMultiple, string attachFileAccept, bool attachFileRestrictAccept, string attachFileAcceptErrorMessage, ulong? attachFileSize, bool attachFileRestrictSize, string attachFileSizeErrorMessage, string attachFileLabel, bool attachFileIsRequired, string attachFileRequiredErrorMessage, bool addSubmitButton, string submitButtonID, string submitButtonCommmandName, string submitButtonText, string submitButtonCssClass, bool submitButtonCauseValidation, string submitButtonBusyText)
			{
				_validationGroup = validationGroup ?? string.Empty;
				_captchaIsRequired = captchaIsRequired;
				_attachFile = attachFile;
				_attachFileAllowMultiple = attachFileAllowMultiple;
				_attachFileAccept = attachFileAccept;
				_attachFileRestrictAccept = attachFileRestrictAccept;
				_attachFileLabel = string.IsNullOrWhiteSpace(attachFileLabel) ? WebFormFunctions.DefaultAttachFileLabel : attachFileLabel;
				_attachFileSize = attachFileSize;
				_attachFileRestrictSize = attachFileRestrictSize;
				_attachFileIsRequired = attachFileIsRequired;
				_attachFileAcceptErrorMessage = string.IsNullOrWhiteSpace(attachFileAcceptErrorMessage) ? "{0} is not of the file type(s) \"{1}\".".FormatWith(_attachFileLabel, _attachFileAccept) : attachFileAcceptErrorMessage;
				_attachFileIsRequired = attachFileIsRequired;
				_attachFileRequiredErrorMessage = string.IsNullOrWhiteSpace(attachFileRequiredErrorMessage) ? "{0} is a required field.".FormatWith(_attachFileLabel) : attachFileRequiredErrorMessage;
				_attachFileSizeErrorMessage = attachFileSizeErrorMessage;
				_addSubmitButton = addSubmitButton;
				_submitButtonID = submitButtonID;
				_submitButtonCommandName = submitButtonCommmandName;
				_submitButtonText = submitButtonText;
				_submitButtonCssClass = submitButtonCssClass;
				_submitButtonCauseValidation = submitButtonCauseValidation;
				_submitButtonBusyText = submitButtonBusyText;
			}

			//
			public void InstantiateIn(Control container)
			{
				if (_attachFile)
				{
					var row = new HtmlGenericControl("div");

					row.Attributes.Add("class", "tr");

					var cell = new HtmlGenericControl("div");

					cell.Attributes.Add("class", "cell file-cell");

					var info = new HtmlGenericControl("div");

					info.Attributes.Add("class", _attachFileIsRequired ? "info required" : "info");

					var label = new System.Web.UI.WebControls.Label { ID = "AttachFileLabel", Text = _attachFileLabel, AssociatedControlID = "AttachFile" };

					info.Controls.Add(label);

					var ctl = new HtmlGenericControl("div");

					ctl.Attributes.Add("class", "control");

					var file = new FileUpload { ID = "AttachFile", AllowMultiple = _attachFileAllowMultiple };

					file.Attributes.Add("accept", _attachFileAccept);

					ctl.Controls.Add(file);

					if (_attachFileRestrictAccept)
					{
						var validator = new CustomValidator
						{
							ID = string.Format("AttachFileAcceptValidator{0}", file.ID),
							ControlToValidate = file.ID,
							ValidationGroup = _validationGroup,
							Display = ValidatorDisplay.None,
							ErrorMessage = _attachFileAcceptErrorMessage
						};
						validator.ServerValidate += (sender, args) => ValidateFileAccept(file, args);

						ctl.Controls.Add(validator);
					}

					if (_attachFileRestrictSize)
					{
						var validator = new CustomValidator
						{
							ID = string.Format("AttachFileSizeValidator{0}", file.ID),
							ControlToValidate = file.ID,
							ValidationGroup = _validationGroup,
							Display = ValidatorDisplay.None,
							ErrorMessage = _attachFileSizeErrorMessage
						};
						validator.ServerValidate += (sender, args) => ValidateFileSize(file, args);

						ctl.Controls.Add(validator);
					}

					if (_attachFileIsRequired)
					{
						ctl.Controls.Add(new RequiredFieldValidator
						{
							ID = string.Format("RequiredFieldValidator{0}", "AttachFile"),
							ControlToValidate = file.ID,
							ValidationGroup = _validationGroup,
							Display = ValidatorDisplay.None,
							ErrorMessage = _attachFileRequiredErrorMessage,
						});
					}

					cell.Controls.Add(info);

					cell.Controls.Add(ctl);

					row.Controls.Add(cell);

					container.Controls.Add(row);
				}

				if (_captchaIsRequired)
				{
#if TELERIKWEBUI
					var row = new HtmlGenericControl("div");

					row.Attributes.Add("class", "tr");

					var cell = new HtmlGenericControl("div");

					cell.Attributes.Add("class", "cell");

					cell.Attributes.Add("class", "captcha-cell");

					RadCaptcha.RenderCaptcha(cell, "captcha", _validationGroup);

					row.Controls.Add(cell);

					container.Controls.Add(row);
#endif
				}

				if (_addSubmitButton)
				{
					container.Controls.Add(new Button
					{
						ID = _submitButtonID,
						CommandName = _submitButtonCommandName,
						Text = _submitButtonText,
						ValidationGroup = _validationGroup,
						CssClass = _submitButtonCssClass,
						CausesValidation = _submitButtonCauseValidation,
						OnClientClick = "javascript:if(typeof webFormClientValidate === 'function'){if(webFormClientValidate()){if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + _validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{return false;}}else{if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" + _validationGroup + "')){clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" + _submitButtonBusyText + "';}}",
						UseSubmitBehavior = false
					});
				}
			}

			private void ValidateFileSize(FileUpload fileUpload, ServerValidateEventArgs args)
			{
				args.IsValid = true;

				if (!_attachFileSize.HasValue) return;

				if (!fileUpload.HasFiles) return;

				foreach (var uploadedFile in fileUpload.PostedFiles)
				{
					args.IsValid = Convert.ToUInt64(uploadedFile.ContentLength) <= _attachFileSize;
					if (!args.IsValid)
					{
						break;
					}
				}
			}

			private void ValidateFileAccept(FileUpload fileUpload, ServerValidateEventArgs args)
			{
				args.IsValid = true;

				if (!fileUpload.HasFiles) return;

				var regex = AnnotationDataAdapter.GetAcceptRegex(_attachFileAccept);
				foreach (var uploadedFile in fileUpload.PostedFiles)
				{
					args.IsValid = regex.IsMatch(uploadedFile.ContentType);
					if (!args.IsValid)
					{
						break;
					}
				}
			}
		}

		internal class StepTemplate : ITemplate
		{
			private readonly string _buttonID;
			private readonly string _commandName;
			private readonly string _text;
			private readonly string _validationGroup;
			private readonly string _cssClass;
			private readonly bool _causeValidation;

			public StepTemplate(string buttonID, string commmandName, string text, string validationGroup, string cssClass, bool causeValidation)
			{
				_buttonID = buttonID;
				_commandName = commmandName;
				_text = text;
				_validationGroup = validationGroup;
				_cssClass = cssClass;
				_causeValidation = causeValidation;
			}

			public void InstantiateIn(Control container)
			{
				container.Controls.Add(new Button
				{
					ID = _buttonID,
					CommandName = _commandName,
					Text = _text,
					ValidationGroup = _validationGroup,
					CssClass = _cssClass,
					CausesValidation = _causeValidation
				});
			}
		}

		protected virtual void OnFormLoad(object sender, WebFormLoadEventArgs args)
		{
			var handler = (EventHandler<WebFormLoadEventArgs>)Events[_eventLoad];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnMovePrevious(object sender, WebFormMovePreviousEventArgs args)
		{
			var handler = (EventHandler<WebFormMovePreviousEventArgs>)Events[_eventMovePrevious];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnSubmit(object sender, WebFormSubmitEventArgs args)
		{
			var handler = (EventHandler<WebFormSubmitEventArgs>)Events[_eventSubmit];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnItemSaving(object sender, WebFormSavingEventArgs args)
		{
			var handler = (EventHandler<WebFormSavingEventArgs>)Events[_eventItemSaving];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnItemSaving(object sender, CrmEntityFormViewUpdatingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewUpdatingEventArgs>)Events[_eventItemSaving];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnItemSaving(object sender, CrmEntityFormViewInsertingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewInsertingEventArgs>)Events[_eventItemSaving];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected virtual void OnItemSaved(object sender, WebFormSavedEventArgs args)
		{
			var handler = (EventHandler<WebFormSavedEventArgs>)Events[_eventItemSaved];

			if (handler != null)
			{
				handler(this, args);
			}
		}

		protected void _Render(HtmlTextWriter writer)
		{
			try
			{
				base.Render(writer);
			}
			catch (Exception e)
			{
				var ex = e;
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				writer.Write(
					"<div class='alert alert-block alert-danger'><p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> {0}</p></div>",
					Page.Server.HtmlEncode(ex.Message));
				RenderEndTag(writer);
			}
		}
	}

	/// <summary>
	/// Event arguments passed to the load event.
	/// </summary>
	public class WebFormLoadEventArgs : EventArgs
	{
		/// <summary>
		/// WebFormLoadEventArgs class initialization.
		/// </summary>
		/// <param name="entityDefinition">Entity details that include id, logical name, primary key name.</param>
		/// <param name="keyName">Custom key associated with the current step that can be referenced to trigger custom code in the event handler.</param>
		public WebFormLoadEventArgs(WebForms.WebFormEntitySourceDefinition entityDefinition, string keyName)
		{
			KeyName = keyName;

			if (entityDefinition == null)
			{
				return;
			}

			EntityLogicalName = entityDefinition.LogicalName;
			EntityPrimaryKeyLogicalName = entityDefinition.PrimaryKeyLogicalName;
			EntityID = entityDefinition.ID;
		}

		/// <summary>
		/// Logical name of the entity.
		/// </summary>
		public string EntityLogicalName { get; private set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity.
		/// </summary>
		public string EntityPrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Unique identitifier of the entity record.
		/// </summary>
		public Guid EntityID { get; private set; }

		/// <summary>
		/// Custom key associated with the current step that can be referenced to trigger custom code in the event handler.
		/// </summary>
		public string KeyName { get; private set; }
	}

	/// <summary>
	/// Event arguments passed to the saved event.
	/// </summary>
	public class WebFormSavedEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments passed to the saved event
		/// </summary>
		/// <param name="exceptionHandled">Indicates if the exception was handled</param>
		/// <param name="entityId">The ID of the target entity updated or inserted</param>
		/// <param name="entityLogicalName">Logical Name of the target entity</param>
		/// <param name="exception">Errors occuring during update</param>
		/// /// <param name="keyName">Custom key associated with the current step that can be referenced to trigger custom code in the event handler</param>
		public WebFormSavedEventArgs(Guid? entityId, string entityLogicalName, Exception exception, bool exceptionHandled, string keyName)
		{
			EntityId = entityId;
			EntityLogicalName = entityLogicalName;
			Exception = exception;
			ExceptionHandled = exceptionHandled;
			KeyName = keyName;
		}
		/// <summary>
		/// The ID of the target entity updated or inserted
		/// </summary>
		public Guid? EntityId { get; private set; }

		/// <summary>
		/// Logical Name of the target entity.
		/// </summary>
		public string EntityLogicalName { get; private set; }

		/// <summary>
		/// Errors occuring during update.
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		/// Indicates if the exception was handled.
		/// </summary>
		public bool ExceptionHandled { get; set; }

		/// <summary>
		/// Custom key associated with the current step that can be referenced to trigger custom code in the event handler.
		/// </summary>
		public string KeyName { get; private set; }
	}

	/// <summary>
	/// Event arguments passed to the MovePrevious event.
	/// </summary>
	public class WebFormMovePreviousEventArgs : CancelEventArgs
	{
		/// <summary>
		/// WebFormMovePreviousEventArgs class initialization.
		/// </summary>
		/// <param name="entityDefinition">Entity details that include id, logical name, primary key name.</param>
		/// <param name="keyName">Custom key associated with the current step that can be referenced to trigger custom code in the event handler.</param>
		public WebFormMovePreviousEventArgs(WebForms.WebFormEntitySourceDefinition entityDefinition, string keyName)
		{
			if (entityDefinition == null) throw new ArgumentNullException("entityDefinition");

			PreviousStepEntityLogicalName = entityDefinition.LogicalName;
			PreviousStepEntityPrimaryKeyLogicalName = entityDefinition.PrimaryKeyLogicalName;
			PreviousStepEntityID = entityDefinition.ID;
			KeyName = keyName;
		}

		/// <summary>
		/// Logical name of the entity.
		/// </summary>
		public string PreviousStepEntityLogicalName { get; private set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity.
		/// </summary>
		public string PreviousStepEntityPrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Unique identitifier of the entity record.
		/// </summary>
		public Guid PreviousStepEntityID { get; private set; }

		/// <summary>
		/// Custom key associated with the current step that can be referenced to trigger custom code in the event handler.
		/// </summary>
		public string KeyName { get; private set; }
	}

	/// <summary>
	/// Event arguments passed to the submit event.
	/// </summary>
	public class WebFormSubmitEventArgs : CancelEventArgs
	{
		/// <summary>
		/// WebFormSubmitEventArgs Class Initialization.
		/// </summary>
		public WebFormSubmitEventArgs(WebForms.WebFormEntitySourceDefinition entityDefinition, string keyName)
		{
			if (entityDefinition == null) throw new ArgumentNullException("entityDefinition");

			PreviousStepEntityLogicalName = entityDefinition.LogicalName;
			PreviousStepEntityPrimaryKeyLogicalName = entityDefinition.PrimaryKeyLogicalName;
			PreviousStepEntityID = entityDefinition.ID;
			KeyName = keyName;
		}

		/// <summary>
		/// Logical name of the entity.
		/// </summary>
		public string PreviousStepEntityLogicalName { get; private set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity.
		/// </summary>
		public string PreviousStepEntityPrimaryKeyLogicalName { get; private set; }

		/// <summary>
		/// Unique identitifier of the entity record.
		/// </summary>
		public Guid PreviousStepEntityID { get; private set; }

		/// <summary>
		/// Logical name of the entity record created by the user control.
		/// </summary>
		public string EntityLogicalName { get; set; }

		/// <summary>
		/// Logical name of the Primary Key attribute of the entity record created by the user control.
		/// </summary>
		public string EntityPrimaryKeyLogicalName { get; set; }

		/// <summary>
		/// Unique identitifier of the entity record created by the user control.
		/// </summary>
		public Guid EntityID { get; set; }

		/// <summary>
		/// Custom key associated with the current step that can be referenced to trigger custom code in the event handler.
		/// </summary>
		public string KeyName { get; private set; }
	}

	/// <summary>
	/// Event arguments passed to the saving event.
	/// </summary>
	public class WebFormSavingEventArgs : CancelEventArgs
	{
		/// <summary>
		/// WebFormSavingEventArgs Class Initialization.
		/// </summary>
		/// <param name="values">Dictionary of keys and values being saved.</param>
		/// <param name="keyName">Custom key associated with the current step that can be referenced to trigger custom code in the event handler.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public WebFormSavingEventArgs(IDictionary<string, object> values, string keyName)
		{
			if (values == null) throw new ArgumentNullException("values");

			Values = values;

			KeyName = keyName;
		}

		/// <summary>
		/// Values assigned to the key to be updated or inserted.
		/// </summary>
		public IDictionary<string, object> Values { get; set; }

		/// <summary>
		/// Logical Name of the target entity.
		/// </summary>
		public string EntityLogicalName { get; set; }

		/// <summary>
		/// Custom key associated with the current step that can be referenced to trigger custom code in the event handler.
		/// </summary>
		public string KeyName { get; private set; }
	}
}
