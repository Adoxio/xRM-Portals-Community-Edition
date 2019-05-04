/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using Microsoft.Xrm.Client;  //despite what resharper says this using is required
	using Microsoft.Xrm.Client.Configuration;
	using Microsoft.Xrm.Client.Diagnostics;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Portal.Web.UI.WebControls;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Messages; //needed
	using Microsoft.Xrm.Sdk.Metadata;
	using Microsoft.Xrm.Sdk.Query;
	using Newtonsoft.Json;
	using Adxstudio.Xrm.Activity;
	using Adxstudio.Xrm.Cms;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Core;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Adxstudio.Xrm.EntityForm;
	using Adxstudio.Xrm.Mapping;
	using Adxstudio.Xrm.Metadata;
	using Adxstudio.Xrm.Notes;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Security;
	using Adxstudio.Xrm.Services;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Web.Mvc.Html;
	using Adxstudio.Xrm.Web.UI.CrmEntityFormView;
	using Adxstudio.Xrm.Web.UI.EntityForm;
	using Adxstudio.Xrm.Web.UI.JsonConfiguration;
	using Adxstudio.Xrm.Web.UI.WebForms;

	/// <summary>
	/// Entity Form control retrieves the Entity Form record defined for the Web Page containing this control. Users can add data entry forms within the portal without the need for developer intervention.
	/// </summary>
	[Description("Entity Form control retrieves the Entity Form record defined for the Web Page containing this control. Users can add data entry forms within the portal without the need for developer intervention.")]
	[ToolboxData(@"<{0}:EntityForm runat=""server""></{0}:EntityForm>")]
	[DefaultProperty("")]
	public class EntityForm : CompositeControl
	{
		private static readonly object _eventLoad = new object();
		private static readonly object _eventItemSaved = new object();
		private static readonly object _eventItemSaving = new object();
		private MappingFieldMetadataCollection _mappingFieldCollection;
		private AnnotationSettings _annotationSettings;
		private AttachFileSaveOption _attachmentSaveOption;

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		/// <summary>
		/// Event that occurs when the form has been loaded.
		/// </summary>
		public event EventHandler<EntityFormLoadEventArgs> FormLoad
		{
			add { Events.AddHandler(_eventLoad, value); }
			remove { Events.RemoveHandler(_eventLoad, value); }
		}

		/// <summary>
		/// Event that occurs when the record has been updated or inserted.
		/// </summary>
		public event EventHandler<EntityFormSavedEventArgs> ItemSaved
		{
			add { Events.AddHandler(_eventItemSaved, value); }
			remove { Events.RemoveHandler(_eventItemSaved, value); }
		}

		/// <summary>
		/// Event that occurs immediately prior to updating or inserting the record.
		/// </summary>
		public event EventHandler<EntityFormSavingEventArgs> ItemSaving
		{
			add { Events.AddHandler(_eventItemSaving, value); }
			remove { Events.RemoveHandler(_eventItemSaving, value); }
		}

		/// <summary>
		/// Gets or sets the Entity Form Entity Reference.
		/// </summary>
		[Description("The Entity Reference of the Entity Form to load.")]
		public EntityReference EntityFormReference
		{
			get { return ((EntityReference)ViewState["EntityFormReference"]); }
			set { ViewState["EntityFormReference"] = value; }
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

		[Description("The CSS Class assigned to the Form.")]
		[DefaultValue("")]
		public string FormCssClass
		{
			get { return ((string)ViewState["FormCssClass"]) ?? string.Empty; }
			set { ViewState["FormCssClass"] = value; }
		}

		[Description("The CSS Class assigned to the Previous button.")]
		[DefaultValue("button previous")]
		public string PreviousButtonCssClass
		{
			get { return ((string)ViewState["PreviousButtonCssClass"]) ?? "button previous"; }
			set { ViewState["PreviousButtonCssClass"] = value; }
		}

		[Description("The CSS Class assigned to the Next button.")]
		[DefaultValue("button next")]
		public string NextButtonCssClass
		{
			get { return ((string)ViewState["NextButtonCssClass"]) ?? "button next"; }
			set { ViewState["NextButtonCssClass"] = value; }
		}

		[Description("The CSS Class assigned to the Submit button.")]
		[DefaultValue("button submit")]
		public string SubmitButtonCssClass
		{
			get { return ((string)ViewState["SubmitButtonCssClass"]) ?? "button submit"; }
			set { ViewState["SubmitButtonCssClass"] = value; }
		}

		[Description("The label of the Previous button in a multi-step form.")]
		[DefaultValue("Previous")]
		public string PreviousButtonText
		{
			get
			{
				var text = (string)ViewState["PreviousButtonText"];
				return string.IsNullOrWhiteSpace(text) ? EntityFormFunctions.DefaultPreviousButtonText : text;
			}
			set { ViewState["PreviousButtonText"] = value; }
		}

		[Description("The label of the Next button in a multi-step form.")]
		[DefaultValue("Next")]
		public string NextButtonText
		{
			get
			{
				var text = (string)ViewState["NextButtonText"];
				return string.IsNullOrWhiteSpace(text) ? EntityFormFunctions.DefaultNextButtonText : text;
			}
			set { ViewState["NextButtonText"] = value; }
		}

		[Description("The label of the Submit button.")]
		[DefaultValue("Submit")]
		public string SubmitButtonText
		{
			get
			{
				var text = (string)ViewState["SubmitButtonText"];
				return string.IsNullOrWhiteSpace(text) ? EntityFormFunctions.DefaultSubmitButtonText : text;
			}
			set { ViewState["SubmitButtonText"] = value; }
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
				return string.IsNullOrWhiteSpace(text) ? EntityFormFunctions.DefaultSubmitButtonBusyText : text;
			}
			set { ViewState["SubmitButtonBusyText"] = value; }
		}

		[Description("Language Code")]
		public int LanguageCode
		{
			get
			{
				// Entity forms only supports CRM languages, so use the CRM Lcid rather than the potentially custom language Lcid.
				return Context.GetCrmLcid();
			}
			set { }
		}

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
			set
			{
				ViewState["ReadAccessDeniedSnippetName"] = value;
			}
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
			set
			{
				ViewState["CreateAccessDeniedSnippetName"] = value;
			}
		}

		/// <summary>
		/// Definition of the entity record for databinding.
		/// </summary>
		public FormEntitySourceDefinition EntitySourceDefinition
		{
			get { return (FormEntitySourceDefinition)(ViewState["EntitySourceDefinition"]); }
			set { ViewState["EntitySourceDefinition"] = value; }
		}

		public MappingFieldMetadataCollection MappingFieldCollection
		{
			get
			{
				if (_mappingFieldCollection == null)
				{
					var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);
					var attributes = new[]
					{
						new FetchAttribute("adx_geolocation_addresslinefieldname"), new FetchAttribute("adx_geolocation_cityfieldname"),
						new FetchAttribute("adx_geolocation_countryfieldname"), new FetchAttribute("adx_geolocation_countyfieldname"),
						new FetchAttribute("adx_geolocation_formattedaddressfieldname"), new FetchAttribute("adx_geolocation_latitudefieldname"),
						new FetchAttribute("adx_geolocation_longitudefieldname"), new FetchAttribute("adx_geolocation_neighborhoodfieldname"),
						new FetchAttribute("adx_geolocation_postalcodefieldname"), new FetchAttribute("adx_geolocation_statefieldname"),
						new FetchAttribute("adx_geolocation_enabled"), new FetchAttribute("adx_geolocation_displaymap")
					};

					context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, attributes);

					var entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, attributes);

					var mappingFieldCollection = new MappingFieldMetadataCollection()
					{
						AddressLineFieldName = entityform.GetAttributeValue<string>("adx_geolocation_addresslinefieldname"),
						CityFieldName = entityform.GetAttributeValue<string>("adx_geolocation_cityfieldname"),
						CountryFieldName = entityform.GetAttributeValue<string>("adx_geolocation_countryfieldname"),
						CountyFieldName = entityform.GetAttributeValue<string>("adx_geolocation_countyfieldname"),
						FormattedLocationFieldName = entityform.GetAttributeValue<string>("adx_geolocation_formattedaddressfieldname"),
						LatitudeFieldName = entityform.GetAttributeValue<string>("adx_geolocation_latitudefieldname"),
						LongitudeFieldName = entityform.GetAttributeValue<string>("adx_geolocation_longitudefieldname"),
						NeightbourhoodFieldName = entityform.GetAttributeValue<string>("adx_geolocation_neighborhoodfieldname"),
						PostalCodeFieldName = entityform.GetAttributeValue<string>("adx_geolocation_postalcodefieldname"),
						StateProvinceFieldName = entityform.GetAttributeValue<string>("adx_geolocation_statefieldname"),
						Enabled = entityform.GetAttributeValue<bool>("adx_geolocation_enabled"),
						DisplayMap = entityform.GetAttributeValue<bool>("adx_geolocation_displaymap")
					};

					_mappingFieldCollection = mappingFieldCollection;
				}
				return _mappingFieldCollection;
			}
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
		private const FormViewMode DefaultFormViewMode = FormViewMode.Insert;
		private const string DefaultReadAccessDeniedSnippetName = "EntitySecurity/Record/ReadAccessDeniedMessage";
		private const string DefaultWriteAccessDeniedSnippetName = "EntitySecurity/Record/WriteAccessDeniedMessage";
		private const string DefaultCreateAccessDeniedSnippetName = "EntitySecurity/Record/CreateAccessDeniedMessage";

		protected Dictionary<string, AttributeTypeCode?> AttributeTypeCodeDictionary { get; private set; }
		protected string RecordNotFoundMessage { get; set; }

		/// <summary>
		/// Message displayed on successful save.
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

		/// <summary>
		/// Indicates whether or not the entity permission provider will assert privileges.
		/// </summary>
		protected bool EnableEntityPermissions
		{
			get { return (bool)(ViewState["EnableEntityPermissions"] ?? false); }
			set { ViewState["EnableEntityPermissions"] = value; }
		}

		protected bool AssociateToCurrentPortalUserOnItemInserted
		{
			get { return (bool)(ViewState["AssociateToCurrentPortalUserOnItemInserted"] ?? false); }
			set { ViewState["AssociateToCurrentPortalUserOnItemInserted"] = value; }
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

		public EntityForm() { }

		public EntityForm(EntityReference entityReference, FormEntitySourceDefinition formEntitySourceDefinition)
		{
			EntityFormReference = entityReference;
			EntitySourceDefinition = formEntitySourceDefinition;
		}

		protected void OnItemCommand(CommandEventArgs args)
		{
			CrmEntityFormView formView;
			switch (args.CommandName)
			{
				case "Next":
					if (!Page.IsValid) return;

					formView = (CrmEntityFormView)FindControl(this.ID + "_EntityFormView");
					if (formView == null) throw new ApplicationException("Couldn't find CrmEntityFormView control.");

					formView.ActiveStepIndex++;
					break;
				case "Previous":
					formView = (CrmEntityFormView)FindControl(this.ID + "_EntityFormView");
					if (formView == null) throw new ApplicationException("Couldn't find CrmEntityFormView control.");

					formView.ActiveStepIndex--;
					break;
				case "Update":
					if (!Page.IsValid) return;

					formView = (CrmEntityFormView)FindControl(this.ID + "_EntityFormView");
					if (formView == null) throw new ApplicationException("Couldn't find CrmEntityFormView control.");

					formView.UpdateItem();
					break;
				case "Insert":
					if (!Page.IsValid) return;

					formView = (CrmEntityFormView)FindControl(this.ID + "_EntityFormView");
					if (formView == null) throw new ApplicationException("Couldn't find CrmEntityFormView control.");

					formView.InsertItem();
					break;
				default: RaiseBubbleEvent(this, args); break;
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

			RegisterClientSideDependencies(this);

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
			var sitemapNodeEntity = portalContext.Entity;
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			Entity entityform;

			if (EntityFormReference != null)
			{
				entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, FetchAttribute.All);

				if (entityform == null)
				{
					throw new ApplicationException(string.Format("Couldn't find an Entity Form (adx_entityform) record where id equals {0}.", EntityFormReference.Id));
				}
			}
			else
			{
				if (sitemapNodeEntity.LogicalName != "adx_webpage")
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "The current entity must be of type adx_webpage. Please select the correct template for this entity type.");
					return;
				}

				var entityformReference = sitemapNodeEntity.GetAttributeValue<EntityReference>("adx_entityform");

				if (entityformReference == null)
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, "CreateChildControls", string.Format("Could not find an Entity Form (adx_entityform) value on Web Page (adx_webpage) where id equals {0}.", sitemapNodeEntity.Id));
					return;
				}

				entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", entityformReference.Id, FetchAttribute.All);

				if (entityform == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not find an Entity Form (adx_webpage_entityform) value where id equals {0} on Web Page (adx_webpage) where id equals {1}.", entityformReference.Id, sitemapNodeEntity.Id));
					return;
				}

				EntityFormReference = entityform.ToEntityReference();
			}

			if (LanguageCode <= 0) { LanguageCode = this.Context.GetPortalSolutionsDetails().OrganizationBaseLanguageCode; }

			var registerStartupScript = entityform.GetAttributeValue<string>("adx_registerstartupscript");

			if (!string.IsNullOrWhiteSpace(registerStartupScript))
			{
				var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);

				var control = new HtmlGenericControl() { };

				var script = html.ScriptAttribute(context, entityform, "adx_registerstartupscript");

				control.InnerHtml = script.ToString();

				Controls.Add(control);
			}

			EnableEntityPermissions = entityform.GetAttributeValue<bool?>("adx_entitypermissionsenabled").GetValueOrDefault(false);

			var modeOption = entityform.GetAttributeValue<OptionSetValue>("adx_mode");

			if (modeOption != null)
			{
				switch (modeOption.Value)
				{
					case 100000000: Mode = FormViewMode.Insert; break;
					case 100000001: Mode = FormViewMode.Edit; break;
					case 100000002: Mode = FormViewMode.ReadOnly; break;
					default: Mode = DefaultFormViewMode; break;
				}
			}

			var recordNotFoundMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("adx_recordnotfoundmessage"), LanguageCode);
			RecordNotFoundMessage = !string.IsNullOrWhiteSpace(recordNotFoundMessage) ? recordNotFoundMessage : DefaultRecordNotFoundMessage;

			try
			{
				EntitySourceDefinition = EntitySourceDefinition ?? GetEntitySourceDefinition(context, entityform);
			}
			catch (ApplicationException e)
			{
				Tracing.FrameworkError(GetType().FullName, "CreateChildControls", "{0}", e);
			}

			// Toggle form Mode based on state and permissions

			if (Mode == FormViewMode.Edit || Mode == FormViewMode.ReadOnly)
			{
				if (EntitySourceDefinition == null)
				{
					EntityFormFunctions.DisplayMessage(this, RecordNotFoundMessage, "error alert alert-danger", false);

					return;
				}

				Entity record = null;
				try
				{
					record = context.RetrieveSingle(new EntityReference(EntitySourceDefinition.LogicalName, EntitySourceDefinition.ID), new ColumnSet(true));
				}
				catch (Exception e)
				{
					EntityFormFunctions.DisplayMessage(this, this.GetErrorMessage(e), "error alert alert-danger", false);
					return;
				}

				if (record == null)
				{
					EntityFormFunctions.DisplayMessage(this, RecordNotFoundMessage, "error alert alert-danger", false);

					return;
				}

				if (Mode == FormViewMode.Edit)
				{
					if (record.Attributes.ContainsKey("statecode"))
					{
						var stateCode = ((OptionSetValue)record.Attributes["statecode"]).Value;

						if (stateCode != 0) // Record is not active
						{
							Mode = FormViewMode.ReadOnly;
						}
					}

					if (!EvaluateEntityPermissions(CrmEntityPermissionRight.Write, record))
					{
						Mode = FormViewMode.ReadOnly;
					}
				}
			}

			if (EntitySourceDefinition == null) return;

			RenderForm(context, entityform, Mode, EntitySourceDefinition);
		}

		protected CrmDataSource CreateDataSource(FormEntitySourceDefinition source)
		{
			var dataSource = new CrmDataSource { ID = string.Format("{0}_EntityFormDataSource", ID), CrmDataContextName = PortalName, IsSingleSource = true };

			if (source != null && source.ID != Guid.Empty)
			{
				var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", source.LogicalName, source.PrimaryKeyLogicalName, source.ID);

				dataSource.FetchXml = fetchXml;
			}

			return dataSource;
		}

		protected virtual bool EvaluateEntityPermissions(CrmEntityPermissionRight right, Entity entity)
		{
			if (!EnableEntityPermissions || !AdxstudioCrmConfigurationManager.GetCrmSection().ContentMap.Enabled)
			{
				return true;
			}

			var serviceContext = CrmConfigurationManager.CreateContext(PortalName, true);
			var crmEntityPermissionProvider = new CrmEntityPermissionProvider();

			return crmEntityPermissionProvider.TryAssert(serviceContext, right, entity);
		}

		protected virtual void RegisterClientSideDependencies(Control control)
		{
			foreach (var script in ScriptIncludes)
			{
				var scriptManager = ScriptManager.GetCurrent(control.Page);

				if (scriptManager == null) continue;

				var absolutePath = VirtualPathUtility.ToAbsolute(script);

				scriptManager.Scripts.Add(new ScriptReference(absolutePath));
			}
		}

		//
		protected void RenderForm(OrganizationServiceContext context, Entity entityform, FormViewMode mode, FormEntitySourceDefinition entitySourceDefinition)
		{
			var formObject = new EntityFormObject(entityform, LanguageCode, context, PreviousButtonCssClass, NextButtonCssClass, SubmitButtonCssClass, PreviousButtonText, NextButtonText, SubmitButtonText, SubmitButtonBusyText);

			HideFormOnSuccess = formObject.HideFormOnSuccess;
			SuccessMessage = formObject.SuccessMessage;

			var dataSource = CreateDataSource(entitySourceDefinition);

			ClientIDMode = ClientIDMode.Static;

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);

			var user = portalContext.User;

			var formMetadataJson = entityform.GetAttributeValue<string>("adx_settings");

			FormActionMetadata formActionMetadata = null;
			if (!string.IsNullOrWhiteSpace(formMetadataJson))
			{
				try
				{
					formActionMetadata = JsonConvert.DeserializeObject<FormActionMetadata>(formMetadataJson,
						new JsonSerializerSettings
						{
							ContractResolver = JsonConfigurationContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Objects,
							Converters = new List<JsonConverter> { new JsonConfiguration.GuidConverter() },
							Binder = new ActionSerializationBinder()
						});

                    var submitAction = formActionMetadata.Actions.FirstOrDefault(a => a is SubmitAction);
                    if (submitAction != null)
                    {
                        SuccessMessage = Localization.GetLocalizedString(submitAction.SuccessMessage, LanguageCode);
                    }
                }
				catch (Exception e)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
				}
			}

			var entityName = entityform.GetAttributeValue<string>("adx_entityname");

			var formConfiguration = new FormConfiguration(portalContext, entityName, formActionMetadata,
				PortalName, LanguageCode, EnableEntityPermissions, formObject.AutoGenerateStepsFromTabs, true, true);

			var formView = new CrmEntityFormView
			{
				ID = this.ID + "_EntityFormView",
				DataSourceID = dataSource.ID,
				DataBindOnPostBack = true,
				EntityName = formObject.EntityName,
				FormName = formObject.FormName,
				ValidationGroup = formObject.ValidationGroup,
				ValidationSummaryCssClass = formObject.ValidationSummaryCssClass,
				ValidationSummaryHeaderText = formObject.LocalizedValidationSummaryHeaderText,
				EnableValidationSummaryLinks = formObject.ValidationSummaryLinksEnabled,
				ContextName = GetContextName(),
				TabName = formObject.TabName ?? string.Empty,
				Mode = mode,
				AutoGenerateSteps = formObject.AutoGenerateStepsFromTabs,
				PreviousButtonCssClass = formObject.PreviousButtonCssClass,
				NextButtonCssClass = formObject.NextButtonCssClass,
				PreviousButtonText = formObject.PreviousButtonText,
				SubmitButtonCssClass = formObject.SubmitButtonCssClass,
				NextButtonText = formObject.NextButtonText,
				SubmitButtonText = formObject.SubmitButtonText,
				RecommendedFieldsRequired = formObject.RecommendedFieldsRequired,
				ForceAllFieldsRequired = formObject.ForceAllFieldsRequired,
				RenderWebResourcesInline = formObject.RenderWebResourcesInline,
				ShowOwnerFields = formObject.ShowOwnerFields,
				ShowUnsupportedFields = formObject.ShowUnsupportedFields,
				ToolTipEnabled = formObject.ToolTipEnabled,
				WebFormMetadata = formObject.EntityFormMetadata,
				MappingFieldCollection = MappingFieldCollection,
				ClientIDMode = ClientIDMode.Static,
				FormConfiguration = formConfiguration,
				EnableEntityPermissions = EnableEntityPermissions
			};

			if (formActionMetadata != null && formActionMetadata.ShowSaveChangesWarningOnExit.GetValueOrDefault(false))
			{
				formView.ConfirmOnExit = formActionMetadata.ShowSaveChangesWarningOnExit.GetValueOrDefault(false);
				if (formView.ConfirmOnExit && formActionMetadata.SaveChangesWarningMessage != null)
				{
					var warningMessage = Localization.GetLocalizedString(formActionMetadata.SaveChangesWarningMessage, LanguageCode);
					if (!string.IsNullOrEmpty(warningMessage))
					{
						formView.ConfirmOnExitMessage = warningMessage;
					}
				}
			}

			var formPanel = new Panel { ID = "EntityFormPanel", CssClass = FormCssClass ?? string.Empty };
			var messagePanel = new Panel { ID = "MessagePanel", Visible = false, CssClass = "message alert alert-info" };
			messagePanel.Attributes.Add("role", "alert");
			var messageLabel = new System.Web.UI.WebControls.Label { ID = "MessageLabel", Text = string.Empty };

			if (!string.IsNullOrWhiteSpace(formObject.Instructions))
			{
				var html = Mvc.Html.EntityExtensions.GetHtmlHelper(PortalName, Page.Request.RequestContext, Page.Response);
				var instructionsContainer = new HtmlGenericControl("div") { InnerHtml = html.Liquid(formObject.Instructions) };
				instructionsContainer.Attributes.Add("class", "instructions");
				Controls.Add(instructionsContainer);
			}

			messagePanel.Controls.Add(messageLabel);
			Controls.Add(messagePanel);
			Controls.Add(formPanel);
			formPanel.Controls.Add(dataSource);
			var submitButtonCommandName = string.Empty;
			var submitButtonID = "SubmitButton";
			var nextButtonID = "NextButton";
			var previousButtonID = "PreviousButton";

			var location = formObject.AttachFileStorageLocation == null
				? StorageLocation.CrmDocument : (StorageLocation)formObject.AttachFileStorageLocation.Value;

			var acceptExtensionTypes = string.IsNullOrWhiteSpace(formObject.AttachFileAcceptExtensions)
				? string.Empty
				: formObject.AttachFileAcceptExtensions;

			var accept = string.IsNullOrWhiteSpace(formObject.AttachFileAccept) && string.IsNullOrWhiteSpace(acceptExtensionTypes)
				? "*/*"
				: string.Join(",", formObject.AttachFileAccept, acceptExtensionTypes);

			var maxFileSize = formObject.AttachFileRestrictSize && formObject.AttachFileMaxSize.HasValue
				? Convert.ToUInt64(formObject.AttachFileMaxSize) << 10 : (ulong?)null;
			_annotationSettings = new AnnotationSettings(context, EnableEntityPermissions, location, accept,
				formObject.AttachFileRestrictAccept, formObject.AttachFileTypeErrorMessage, maxFileSize,
				formObject.AttachFileSizeErrorMessage, acceptExtensionTypes);

			// Utilize whitelist of PortalTimeline enabled portals.
			// This allows fallback to the Notes entity which is CRM OOB entity
			if (PortalTimeline.EnabledPortalsByWebsiteGuid.Contains(portalContext.Website.Id))
			{
				// Get the value from the field since field if not null or default to Portal Comment if not specified
				_attachmentSaveOption = formObject.AttachFileSaveOption == null
					? AttachFileSaveOption.PortalComment
					: (AttachFileSaveOption)formObject.AttachFileSaveOption.Value;
			}
			else
			{
				// Portal does not contain "AttachFileSaveOption" field since it's not PortalTimeline enabled.
				// Fallback to Notes entity
				_attachmentSaveOption = AttachFileSaveOption.Notes;
			}

			switch (mode)
			{
				case FormViewMode.Insert:
					formView.ItemInserted += OnItemInserted;
					formView.ItemInserting += OnItemInserting;
					submitButtonCommandName = "Insert";
					submitButtonID = "InsertButton";
					formView.InsertItemTemplate = new ItemTemplate(formObject.ValidationGroup, formObject.CaptchaRequired && (user == null || formObject.ShowCaptchaForAuthenticatedUsers), formObject.AttachFile,
						formObject.AttachFileAllowMultiple, _annotationSettings.AcceptMimeTypes, _annotationSettings.RestrictMimeTypes,
						_annotationSettings.RestrictMimeTypesErrorMessage, _annotationSettings.MaxFileSize, _annotationSettings.MaxFileSize.HasValue,
						_annotationSettings.MaxFileSizeErrorMessage, formObject.AttachFileLabel, formObject.AttachFileRequired,
						formObject.AttachFileRequiredErrorMessage, formObject.AutoGenerateStepsFromTabs && string.IsNullOrEmpty(formMetadataJson), submitButtonID, submitButtonCommandName,
						formObject.SubmitButtonText, formObject.SubmitButtonCssClass, true, formObject.SubmitButtonBusyText);
					break;
				case FormViewMode.Edit:
					formView.ItemUpdating += OnItemUpdating;
					formView.ItemUpdated += OnItemUpdated;
					submitButtonCommandName = "Update";
					submitButtonID = "UpdateButton";
					formView.UpdateItemTemplate = new ItemTemplate(formObject.ValidationGroup, formObject.CaptchaRequired && (user == null || formObject.ShowCaptchaForAuthenticatedUsers), formObject.AttachFile,
						formObject.AttachFileAllowMultiple, _annotationSettings.AcceptMimeTypes, _annotationSettings.RestrictMimeTypes,
						_annotationSettings.RestrictMimeTypesErrorMessage, _annotationSettings.MaxFileSize, _annotationSettings.MaxFileSize.HasValue,
						_annotationSettings.MaxFileSizeErrorMessage, formObject.AttachFileLabel, formObject.AttachFileRequired,
						formObject.AttachFileRequiredErrorMessage, formObject.AutoGenerateStepsFromTabs && string.IsNullOrEmpty(formMetadataJson), submitButtonID, submitButtonCommandName,
						formObject.SubmitButtonText, formObject.SubmitButtonCssClass, true, formObject.SubmitButtonBusyText);

					break;
				case FormViewMode.ReadOnly:
					break;
			}

			if (!string.IsNullOrEmpty(formMetadataJson))
			{
				formView.NextStepTemplate = new EmptyTemplate();
				formView.PreviousStepTemplate = new EmptyTemplate();
			}

			if (LanguageCode > 0) { formView.LanguageCode = LanguageCode; }

			//Add Action Bar above Form
			if (formConfiguration != null && formConfiguration.TopFormActionLinks != null && formConfiguration.TopFormActionLinks.Any() && mode != FormViewMode.Insert)
			{
				formPanel.Controls.Add(ActionButtonBarAboveForm(formObject, formConfiguration, submitButtonID, submitButtonCommandName, nextButtonID, previousButtonID));
			}

			formPanel.Controls.Add(formView);

			var lastStep = formView.ActiveStepIndex == (formView.StepCount - 1);

			var buttonContainer = AddActionBarContainerIfApplicable(formObject, formConfiguration, mode, submitButtonID, submitButtonCommandName, nextButtonID, previousButtonID);

			if (Mode != FormViewMode.ReadOnly && formConfiguration.SubmitActionLink.Enabled)
			{
				formPanel.DefaultButton = (formObject.AutoGenerateStepsFromTabs && !lastStep) ? nextButtonID : submitButtonID;
			}

			formPanel.Controls.Add(buttonContainer);

			//		formPanel.Controls.Add(new HtmlGenericControl("br"));

			if ((Mode != FormViewMode.ReadOnly && string.IsNullOrEmpty(formMetadataJson) && !formObject.AutoGenerateStepsFromTabs)  ||
				(Mode == FormViewMode.Insert && !formObject.AutoGenerateStepsFromTabs))
			{
				var sumbitButtonText = formObject.SubmitButtonText;
				var submitButtonTooltip = string.Empty;
				var submitButtonBusyText = formObject.SubmitButtonBusyText;

				if (formConfiguration != null && formConfiguration.SubmitActionLink != null)
				{
					if (!string.IsNullOrEmpty(formConfiguration.SubmitActionLink.Label))
					{
						sumbitButtonText = formConfiguration.SubmitActionLink.Label;
					}

					if (!string.IsNullOrEmpty(formConfiguration.SubmitActionLink.BusyText))
					{
						submitButtonBusyText = formConfiguration.SubmitActionLink.BusyText;
					}

					if (!string.IsNullOrEmpty(formConfiguration.SubmitActionLink.Tooltip))
					{
						submitButtonTooltip = formConfiguration.SubmitActionLink.Tooltip;
					}
				}

				var submitButton = new Button
				{
					ID = submitButtonID,
					CommandName = submitButtonCommandName,
					Text = sumbitButtonText,
					ValidationGroup = formObject.ValidationGroup,
					CssClass = formObject.SubmitButtonCssClass,
					CausesValidation = true,
					OnClientClick =
						"javascript:if(typeof entityFormClientValidate === 'function'){if(entityFormClientValidate()){if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" +
						formObject.ValidationGroup + "')){clearIsDirty();disableButtons();this.value = '" +
						submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" +
						submitButtonBusyText +
						"';}}else{return false;}}else{if(typeof Page_ClientValidate === 'function'){if(Page_ClientValidate('" +
						formObject.ValidationGroup + "')){clearIsDirty();disableButtons();this.value = '" +
						submitButtonBusyText + "';}}else{clearIsDirty();disableButtons();this.value = '" +
						submitButtonBusyText + "';}}",
					UseSubmitBehavior = false,
				};

				if (!string.IsNullOrEmpty(submitButtonTooltip))
				{
					submitButton.ToolTip = submitButtonTooltip;
				}

				buttonContainer.Controls.AddAt(0, submitButton);

				formPanel.DefaultButton = submitButtonID;

				formPanel.Controls.Add(buttonContainer);
			}

			PopulateReferenceEntityField(context, formObject, formView);

			ApplyMetadataPrepopulateValues(context, formObject, formView);

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage) && (Mode == FormViewMode.Edit || Mode == FormViewMode.Insert))
			{
                string action = formView.Mode.ToString();

                if (formView.Mode.ToString() == "Insert")
                {
                    action = "create";
                }
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forms, this.Context, action.ToLower() + "_" + entityName, 1, new EntityReference(entitySourceDefinition.LogicalName, entitySourceDefinition.ID), action.ToLower());
			}

			OnFormLoad(this, new EntityFormLoadEventArgs(entitySourceDefinition));
		}

		private WebControl ActionButtonBarAboveForm(EntityFormObject formObject, FormConfiguration formConfiguration, string submitButtonID = "SubmitButton",
			string submitButtonCommandName = "", string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(formConfiguration.PortalName, Page.Request.RequestContext, Page.Response);

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			var rightContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Right);

			var navBar = FormActionControls.FormActionNoNavBar(html, formConfiguration, leftContainer, rightContainer, ActionButtonPlacement.AboveForm, submitButtonID, submitButtonCommandName,
					formObject.ValidationGroup, formObject.SubmitButtonBusyText, nextButtonId, previousButtonId);

			if (!string.IsNullOrEmpty(formConfiguration.TopContainerCssClass)) navBar.AddClass(formConfiguration.TopContainerCssClass);

			return navBar;
		}

		private Control AddActionBarContainerIfApplicable(EntityFormObject formObject, FormConfiguration formConfiguration, FormViewMode mode, string submitButtonID = "SubmitButton",
			string submitButtonCommandName = "", string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			if (formConfiguration != null && formConfiguration.BottomFormActionLinks != null && formConfiguration.BottomFormActionLinks.Any() && mode != FormViewMode.Insert)
			{
				var actionBar = ActionButtonBarBelowForm(formObject, formConfiguration, submitButtonID, submitButtonCommandName, nextButtonId, previousButtonId);

				return actionBar;
			}

			var container = new HtmlGenericControl("div");

			container.Attributes.Add("class", "actions");

			return container;
		}

		private WebControl ActionButtonBarBelowForm(EntityFormObject formObject, FormConfiguration formConfiguration, string submitButtonID = "SubmitButton",
			string submitButtonCommandName = "", string nextButtonId = "NextButton", string previousButtonId = "PreviousButton")
		{
			var html = Mvc.Html.EntityExtensions.GetHtmlHelper(formConfiguration.PortalName, Page.Request.RequestContext, Page.Response);

			var leftContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Left);

			var rightContainer = FormActionControls.ActionNavBarControl(ActionButtonAlignment.Right);

			var navBar = FormActionControls.FormActionNoNavBar(html, formConfiguration, leftContainer, rightContainer, ActionButtonPlacement.BelowForm, submitButtonID, submitButtonCommandName,
					formObject.ValidationGroup, formObject.SubmitButtonBusyText, nextButtonId, previousButtonId);

			if (!string.IsNullOrEmpty(formConfiguration.BottomContainerCssClass)) navBar.AddClass(formConfiguration.BottomContainerCssClass);

			return navBar;
		}

		protected string GetContextName()
		{
			var portalConfig = PortalCrmConfigurationManager.GetPortalContextElement(PortalName);

			return portalConfig == null ? null : portalConfig.ContextName;
		}

		protected FormEntitySourceDefinition GetEntitySourceDefinition(OrganizationServiceContext context, Entity entityform)
		{
			entityform.AssertEntityName("adx_entityform");
			AssociateToCurrentPortalUserOnItemInserted = false;

			string id = Guid.Empty.ToString();
			var logicalName = entityform.GetAttributeValue<string>("adx_entityname");
			var primaryKey = entityform.GetAttributeValue<string>("adx_primarykeyname");

			if (string.IsNullOrWhiteSpace(logicalName))
			{
				throw new ApplicationException("adx_entityform.adx_entityname must not be null.");
			}

			if (!string.IsNullOrWhiteSpace(logicalName) && string.IsNullOrWhiteSpace(primaryKey))
			{
				primaryKey = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, logicalName);
			}

			if (string.IsNullOrWhiteSpace(primaryKey))
			{
				throw new ApplicationException(ResourceManager.GetString("Failed_To_Determine_Target_Entity_Pk_Logical_Name_Exception"));
			}

			if (Mode == FormViewMode.Insert)
			{
				return new FormEntitySourceDefinition(logicalName, primaryKey, Guid.Empty);
			}

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var entitySourceType = entityform.GetAttributeValue<OptionSetValue>("adx_entitysourcetype");

			var recordNotFoundMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("adx_recordnotfoundmessage"), LanguageCode);
			RecordNotFoundMessage = !string.IsNullOrWhiteSpace(recordNotFoundMessage) ? recordNotFoundMessage : DefaultRecordNotFoundMessage;

			switch (entitySourceType.Value)
			{
				case 756150001: // Query String
					var queryStringParameter = entityform.GetAttributeValue<string>("adx_recordidquerystringparametername");
					id = HttpContext.Current.Request[queryStringParameter];
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
				case 756150002: // Current Portal User
					if (portalContext.User == null)
					{
						EntityFormFunctions.DisplayMessage(this, RecordNotFoundMessage, "error alert alert-danger", false);
						return null;
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
				case 756150003: // Record Associated to Current Portal User
					var relationship = entityform.GetAttributeValue<string>("adx_recordsourcerelationshipname");
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
					var user = context.RetrieveSingle(
						portalContext.User.LogicalName,
						userPrimaryKey,
						portalContext.User.Id,
						FetchAttribute.All);

					if (user == null)
					{
						throw new ApplicationException(string.Format("Couldn't load user record. Portal context User could not be found with id equal to {0}.", portalContext.User.Id));
					}
					var source = context.RetrieveRelatedEntity(user, relationship);
					if (source == null)
					{
						var allowCreate = entityform.GetAttributeValue<bool?>("adx_recordsourceallowcreateonnull").GetValueOrDefault();
						if (allowCreate)
						{
							AssociateToCurrentPortalUserOnItemInserted = true;
							Mode = FormViewMode.Insert;
							return new FormEntitySourceDefinition(logicalName, primaryKey, Guid.Empty);
						}
					}
					else
					{
						id = source.Id.ToString();
					}
					break;
				default:
					throw new ApplicationException("adx_entityform.adx_entitysourcetype is not valid."); 
			}

			return new FormEntitySourceDefinition(logicalName, primaryKey, id);
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, FetchAttribute.All);

			if (entityform == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_entityform where id equals {0}", EntityFormReference.Id));
			}

			SetAttributeValuesOnUpdating(context, entityform, e);

			SetEntityReference(context, entityform, e.Values);

			var savingEventArgs = new EntityFormSavingEventArgs(e.Values);

			OnItemSaving(sender, savingEventArgs);

			e.Cancel = savingEventArgs.Cancel;
		}

		protected void OnItemInserting(object sender, CrmEntityFormViewInsertingEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, FetchAttribute.All);

			if (entityform == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_entityform where id equals {0}", EntityFormReference.Id));
			}

			AssociateCurrentPortalUserOnInserting(context, entityform, e);

			SetAttributeValuesOnInserting(context, entityform, e);

			SetEntityReference(context, entityform, e.Values);

			SetAssociateReference(context, e);

			var savingEventArgs = new EntityFormSavingEventArgs(e.Values);

			OnItemSaving(sender, savingEventArgs);

			e.Cancel = savingEventArgs.Cancel;
		}

		protected void OnItemInserted(object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, FetchAttribute.All);

			if (entityform == null) { throw new ApplicationException(string.Format("Error retrieving adx_entityform where id equals {0}", EntityFormReference.Id)); }

			var entityLogicalName = entityform.GetAttributeValue<string>("adx_entityname");

			if (e.Exception == null)
			{
				if (e.EntityId == null || e.EntityId == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Saving record failed. EntityId is null. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps.");
				}
				else
				{
					if (AssociateToCurrentPortalUserOnItemInserted)
					{
						var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
						if (portalContext.User == null)
						{
							throw new ApplicationException("Couldn't load user record. Portal context User is null.");
						}
						var relationshipName = entityform.GetAttributeValue<string>("adx_recordsourcerelationshipname");
						if (string.IsNullOrWhiteSpace(relationshipName))
						{
							throw new ApplicationException("Required Relationship Name has not been specified for the Record Source Type 'Record Associated to Current Portal User'.");
						}
						var sourceUpdate = new Entity(EntitySourceDefinition.LogicalName)
						{
							Id = e.EntityId.Value
						};
						var targetUpdate = new Entity(portalContext.User.LogicalName)
						{
							Id = portalContext.User.Id
						};
						context.Attach(sourceUpdate);
						context.Attach(targetUpdate);
						context.AddLink(sourceUpdate, new Relationship(relationshipName), targetUpdate);
						context.SaveChanges();
					}

					EntityFormFunctions.AssociateEntity(context, entityform, e.EntityId.GetValueOrDefault());

					EntityFormFunctions.Associate(context, new EntityReference(entityLogicalName, e.EntityId.GetValueOrDefault()));

					AttachFileOnItemInserted(context, entityform, sender, e);

					if (SetStateOnSave)
					{
						EntityFormFunctions.TrySetState(context, new EntityReference(entityLogicalName, e.EntityId.GetValueOrDefault()), SetStateOnSaveValue);
					}

					if (entityLogicalName == "opportunityproduct")
					{
						if (HttpContext.Current.Request["refentity"] == "opportunity")
						{
							EntityFormFunctions.CalculateValueOpportunity();
						}
					}
				}
			}
			else
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Saving record failed. EntityId is null. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps. {0}", e.Exception.InnerException));
			}

			EntitySourceDefinition.ID = e.EntityId == null ? Guid.Empty : e.EntityId.GetValueOrDefault();

			string entityDisplayName = ((Adxstudio.Xrm.Web.UI.WebControls.CrmEntityFormView)(sender)).EntityDisplayName;
			var savedEventArgs = new EntityFormSavedEventArgs(EntitySourceDefinition.ID, EntitySourceDefinition.LogicalName, e.Exception, false, entityDisplayName);
			OnItemSaved(sender, savedEventArgs);
			e.Exception = savedEventArgs.Exception;
			e.ExceptionHandled = savedEventArgs.ExceptionHandled;

			if (e.Exception != null)
			{
				if (!e.ExceptionHandled)
				{
					EntityFormFunctions.DisplayMessage(this,
						"<p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " +
						Page.Server.HtmlEncode(GetErrorMessage(e.Exception)) + "</p>", "alert-danger", false);

					e.ExceptionHandled = true;

					return;
				}
			}
			else
			{
				OnSuccess(context, entityform, EntitySourceDefinition);

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
				{
					PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forms, this.Context, "create_" + entityLogicalName, 1, new EntityReference(entityLogicalName, EntitySourceDefinition.ID), "create");
				}
			}
		}

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			var context = PortalCrmConfigurationManager.CreateServiceContext(PortalName);

			var entityform = context.RetrieveSingle("adx_entityform", "adx_entityformid", this.EntityFormReference.Id, FetchAttribute.All);

			if (entityform == null)
			{
				throw new ApplicationException(string.Format("Error retrieving adx_entityform where id equals {0}", EntityFormReference.Id));
			}

			if (e.Exception == null)
			{
				if (e.Entity == null || e.Entity.Id == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Saving record failed. Entity.Id is null. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps.");
				}
				else
				{
					AttachFileOnItemUpdated(context, entityform, sender, e);

					if (SetStateOnSave)
					{
						EntityFormFunctions.TrySetState(context, e.Entity.ToEntityReference(), SetStateOnSaveValue);
					}
				}
			}
			else
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Saving record failed. Entity.Id is null. This error usually indicates a plugin has failed or a system required field was not provided a value. Please check the system jobs in CRM for possible failed plugin steps. {0}", e.Exception.InnerException));
			}

			EntitySourceDefinition.ID = e.Entity == null ? Guid.Empty : e.Entity.Id;

			var savedEventArgs = new EntityFormSavedEventArgs(EntitySourceDefinition.ID, EntitySourceDefinition.LogicalName, e.Exception, false);
			OnItemSaved(sender, savedEventArgs);
			e.Exception = savedEventArgs.Exception;
			e.ExceptionHandled = savedEventArgs.ExceptionHandled;

			if (e.Exception != null)
			{
				if (!e.ExceptionHandled)
				{
					EntityFormFunctions.DisplayMessage(this,
						"<p class='text-danger'><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " +
						Page.Server.HtmlEncode(GetErrorMessage(e.Exception)) + "</p>", "alert-danger", false);

					e.ExceptionHandled = true;

					return;
				}
			}
			else
			{
				OnSuccess(context, entityform, EntitySourceDefinition);

				if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
				{
					var entityLogicalName = entityform.GetAttributeValue<string>("adx_entityname");

					PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Forms, this.Context, "edit_" + entityLogicalName, 1, new EntityReference(entityLogicalName, EntitySourceDefinition.ID), "edit");
				}
			}
		}

		private string GetErrorMessage(Exception exception)
		{
			var guid = WebEventSource.Log.GenericErrorException(exception);
			var message = string.Format(ResourceManager.GetString("Generic_Error_Message"), guid);
			var faultException = exception.InnerException as FaultException<OrganizationServiceFault>;
			if (faultException != null)
			{
				switch (faultException.Detail.ErrorCode)
				{
					// parsing message based on web service error codes
					case -2147157752:
						message = ResourceManager.GetString("You_Can_Only_Add_Active_Products");
						break;
					case -2147206387:
						message = ResourceManager.GetString("Unit_Id_Missing");
						break;
				}
			}
			return message;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			var formView = (CrmEntityFormView)FindControl(this.ID + "_EntityFormView");
			if (formView == null) return;

			var firstStep = formView.ActiveStepIndex == 0;
			var lastStep = formView.ActiveStepIndex == (formView.StepCount - 1);

			var submitButton = (Button)FindControl("UpdateButton") ?? (Button)FindControl("InsertButton") ?? (Button)FindControl("SubmitButton");

			if (submitButton != null) submitButton.Visible = (!formView.AutoGenerateSteps || lastStep) && (formView.Mode != FormViewMode.ReadOnly);

			var nextButton = (Button)FindControl("NextButton");

			if (nextButton != null) nextButton.Visible = formView.AutoGenerateSteps && !lastStep;

			var previousButton = (Button)FindControl("PreviousButton");

			if (previousButton != null) previousButton.Visible = formView.AutoGenerateSteps && !firstStep;
		}

		protected void OnSuccess(OrganizationServiceContext context, Entity entityform, FormEntitySourceDefinition entitySourceDefinition)
		{
			var onSuccessOption = entityform.GetAttributeValue<OptionSetValue>("adx_onsuccess");

			if (onSuccessOption == null) return;

			switch (onSuccessOption.Value)
			{
				case 756150000: // Display Success Message
					var hideForm = HideFormOnSuccess;
					EntityFormFunctions.DisplayMessage(this, SuccessMessage, "success alert alert-success", hideForm);
					break;
				case 756150001: // Redirect
					ProcessRedirect(context, entityform, entitySourceDefinition);
					break;
			}
		}

		protected void ProcessRedirect(OrganizationServiceContext context, Entity entityform, FormEntitySourceDefinition entitySourceDefinition)
		{
			var existingQueryString = HttpContext.Current.Request.QueryString;
			var redirectUrl = entityform.GetAttributeValue<string>("adx_redirecturl");
			var appendExistingQueryString = entityform.GetAttributeValue<bool?>("adx_appendquerystring") ?? false;
			UrlBuilder url;

			if (string.IsNullOrWhiteSpace(redirectUrl))
			{
				var pageReference = entityform.GetAttributeValue<EntityReference>("adx_redirectwebpage");

				if (pageReference == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_entityform_redirectwebpage is null");
					return;
				}

				var page = context.RetrieveSingle(
					pageReference.LogicalName,
					FetchAttribute.None,
					new Condition("adx_webpageid", ConditionOperator.Equal, pageReference.Id));

				if (page == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not find web page with id equal to '{0}'", pageReference.Id));
					return;
				}

				var path = context.GetUrl(page);

				if (path == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_entityform_redirectwebpage URL is null");
					return;
				}

				url = new UrlBuilder(path);
			}
			else
			{
				url = new UrlBuilder(redirectUrl.StartsWith("http") ? redirectUrl : string.Format("https://{0}", redirectUrl));
			}

			var addquerystring = entityform.GetAttributeValue<bool?>("adx_redirecturlappendentityidquerystring") ?? false;

			if (addquerystring)
			{
				var queryStringParameterName = entityform.GetAttributeValue<string>("adx_redirecturlquerystringname");

				if (entitySourceDefinition.ID == Guid.Empty)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Record ID not appended to query string. ID is null.");
				}
				else
				{
					url.QueryString.Add(queryStringParameterName, entitySourceDefinition.ID.ToString());
				}
			}

			if (appendExistingQueryString && existingQueryString.HasKeys())
			{
				url.QueryString.Add(existingQueryString);
			}

			var customQueryString = entityform.GetAttributeValue<string>("adx_redirecturlcustomquerystring");

			if (!string.IsNullOrWhiteSpace(customQueryString))
			{
				try
				{
					var customQueryStringCollection = HttpUtility.ParseQueryString(customQueryString);
					url.QueryString.Add(customQueryStringCollection);
				}
				catch (Exception ex)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to add Custom Query String (adx_redirecturlcustomquerystring) to the Query String. {0}", ex.ToString()));
				}
			}

			var queryStringAttributeParameterName = entityform.GetAttributeValue<string>("adx_redirecturlquerystringattributeparamname");

			if (!string.IsNullOrWhiteSpace(queryStringAttributeParameterName))
			{
				var queryStringAttributeLogicalName = entityform.GetAttributeValue<string>("adx_redirecturlquerystringattribute");

				if (!string.IsNullOrWhiteSpace(queryStringAttributeLogicalName))
				{
					if (entitySourceDefinition.ID != Guid.Empty && !string.IsNullOrWhiteSpace(entitySourceDefinition.LogicalName) && !string.IsNullOrWhiteSpace(entitySourceDefinition.PrimaryKeyLogicalName))
					{
						var record = context.RetrieveSingle(
							entitySourceDefinition.LogicalName,
							entitySourceDefinition.PrimaryKeyLogicalName,
							entitySourceDefinition.ID,
							new[] { new FetchAttribute(queryStringAttributeLogicalName) });

						if (record == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to add attribute value to the Query String. Record could not be found with ID equal to '{0}'", entitySourceDefinition.ID));
						}
						else
						{
							if (record.Attributes.ContainsKey(queryStringAttributeLogicalName))
							{
								var queryStringAttributeValue = record[queryStringAttributeLogicalName];

								if (queryStringAttributeValue != null)
								{
									var attributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entitySourceDefinition.LogicalName);
									var attributeValue = EntityFormFunctions.TryConvertAttributeValueToString(context, attributeTypeCodeDictionary, entitySourceDefinition.LogicalName, queryStringAttributeLogicalName, queryStringAttributeValue);

									if (!string.IsNullOrWhiteSpace(attributeValue))
									{
										url.QueryString.Add(queryStringAttributeParameterName, attributeValue);
									}
									else
									{
										ADXTrace.Instance.TraceError(TraceCategory.Application, "Could not add attribute value to the Query String. Value is null.");
									}
								}
								else
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to add attribute value to the Query String. Attribute value does not exist.");
								}
							}
							else
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Failed to add attribute value to the Query String. Record could not be found or entity does not contain attribute '{0}'", queryStringAttributeLogicalName));
							}
						}
					}
					else
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to add attribute value to the Query String. entitySourceDefinition is not valid.");
					}
				}
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

		protected void ApplyMetadataPrepopulateValues(OrganizationServiceContext context, IEntityForm entityform, Control container)
		{
			var metadata = entityform.EntityFormMetadata.Where(m => m.GetAttributeValue<OptionSetValue>("adx_prepopulatetype") != null).ToList();

			if (!metadata.Any()) return;

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entityform.EntityName);
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
				TrySetFieldValue(context, container, attributeName, value);
			}
		}

		protected bool TrySetFieldValue(OrganizationServiceContext serviceContext, Control container, string attributeName, object value)
		{
			if (value == null) return false;

			if (AttributeTypeCodeDictionary == null || !AttributeTypeCodeDictionary.Any())
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "Attribute Type Code Dictionary is null or empty");
				return false;
			}

			var attributeTypeCode = AttributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Unable to recognize the attribute '{0}' specified.", attributeName));
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
						else return false;
						break;
					case AttributeTypeCode.Boolean:
						if (field is CheckBox)
						{
							var control = (CheckBox)field;
							control.Checked = Convert.ToBoolean(value.ToString());
						}
						else return false;
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
						else return false;
						break;
					case AttributeTypeCode.Decimal:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else return false;
						break;
					case AttributeTypeCode.Double:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else return false;
						break;
					case AttributeTypeCode.Integer:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else return false;
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
						else return false;
						break;
					case AttributeTypeCode.Money:
						if (field is TextBox && value is Money)
						{
							var money = (Money)value;
							var control = (TextBox)field;
							control.Text = money.Value.ToString("0.00");
						}
						else return false;
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
							else { return false; }
						}
						else { return false; }
						break;
					case AttributeTypeCode.State:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute '{0}' type '{1}' is unsupported. The state attribute is created automatically when the entity is created. The options available for this attribute are read-only.", attributeName, attributeTypeCode));
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
							else { return false; }
						}
						else { return false; }
						break;
					case AttributeTypeCode.String:
						if (field is TextBox)
						{
							var control = (TextBox)field;
							control.Text = value.ToString();
						}
						else return false;
						break;
					default:
						ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Attribute '{0}' type '{1}' is unsupported.", attributeName, attributeTypeCode));
						return false;
				}
			}
			catch (Exception ex)
			{
				WebEventSource.Log.GenericWarningException(ex, string.Format("Attribute '{0}' specified is expecting a {1}. The value provided is not valid.", attributeName, attributeTypeCode));
				return false;
			}

			return true;
		}

		protected bool TrySetLookupFieldValue(OrganizationServiceContext context, Control container, EntityReference value, string attributeName)
		{
			try
			{
				var field = container.FindControl(attributeName);

				if (field == null)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Could not find control with id equal '{0}'", attributeName));

					return false;
				}

				var list = field as DropDownList;
				var hiddenField = field as HtmlInputHidden;
				var modalLookup = hiddenField != null;

				if (list == null && hiddenField == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' field is not one of the expected control types.", attributeName));

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
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' field value could not be set. {1}", attributeName, e.ToString()));

				return false;
			}

			return true;
		}

		protected void SetAttributeValuesOnUpdating(OrganizationServiceContext context, Entity step, CrmEntityFormViewUpdatingEventArgs e)
		{
			SetAttributeValuesOnSave(context, step, e);
		}

		protected void SetAttributeValuesOnInserting(OrganizationServiceContext context, Entity step, CrmEntityFormViewInsertingEventArgs e)
		{
			SetAttributeValuesOnSave(context, step, e);
		}

		protected void SetAttributeValuesOnSave(OrganizationServiceContext context, Entity entityform, object e)
		{
			var metadata = context.RetrieveRelatedEntities(
				entityform, 
				"adx_entityformmetadata_entityform", 
				filters: new[]
						{
							new Filter
							{
								Conditions = new[]
								{
									new Condition("adx_setvalueonsave", ConditionOperator.Equal, true)
								}
							}
						}).Entities;

			if (!metadata.Any()) return;

			var targetEntityLogicalName = entityform.GetAttributeValue<string>("adx_entityname");

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				throw new ApplicationException("adx_entityform.adx_entityname is null.");
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
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityformmetadata.adx_attributelogicalname is null.");
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

		protected void PopulateReferenceEntityField(OrganizationServiceContext context, IEntityForm entityform, Control container)
		{
			var populateReferenceEntityLookupField = entityform.PopulateReferenceEntityLookupField;

			if (!populateReferenceEntityLookupField) return;

			try
			{
				var field = container.FindControl(entityform.TargetAttributeName);

				if (field == null)
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "Could not find control ");
					return;
				}

				var list = field as DropDownList;
				var hiddenField = field as HtmlInputHidden;
				var modalLookup = hiddenField != null;

				if (list == null && hiddenField == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' field is not one of the expected control types.", entityform.TargetAttributeName));
					return;
				}

				// only set the field value if it is blank.

				if (modalLookup) { if (!string.IsNullOrWhiteSpace(hiddenField.Value)) return; }
				else { if (!string.IsNullOrWhiteSpace(list.SelectedValue)) return; }

				var disabled = list != null && list.CssClass.Contains("readonly");
				var id = Guid.Empty;
				var text = string.Empty;
				var referenceQueryStringValue = HttpContext.Current.Request[entityform.ReferenceQueryStringName];
				var primaryNameAttribute = string.Empty;

				if (string.IsNullOrWhiteSpace(entityform.TargetAttributeName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_referenctargetlookupattributelogicalname must not be null.");
					return;
				}

				if (string.IsNullOrWhiteSpace(entityform.ReferenceEntityLogicalName))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_referenceentitylogicalname must not be null.");
					return;
				}

				if (disabled || modalLookup)
				{
					var entityMetadata = context.GetEntityMetadata(entityform.ReferenceEntityLogicalName);
					primaryNameAttribute = entityMetadata.PrimaryNameAttribute;
				}

				if (!entityform.QuerystringIsPrimaryKey)
				{
					var entity = context.RetrieveSingle(
										new Fetch
										{
											Entity = new FetchEntity(entityform.ReferenceEntityLogicalName)
											{
												Filters = new[] { new Filter { Conditions = new[] { new Condition(entityform.ReferenceQueryAttributeName, ConditionOperator.Equal, referenceQueryStringValue) } } }
											}
										});

					if (entity == null)
					{
						ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not retrieve entity of type '{0}' where '{1}' equals '{2}'.", entityform.ReferenceEntityLogicalName, entityform.ReferenceQueryAttributeName, referenceQueryStringValue));
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

				if (disabled || modalLookup)
				{
					if (text == string.Empty)
					{
						if (string.IsNullOrWhiteSpace(entityform.ReferenceEntityPrimaryKeyLogicalName))
						{
							entityform.ReferenceEntityPrimaryKeyLogicalName = MetadataHelper.GetEntityPrimaryKeyAttributeLogicalName(context, entityform.ReferenceEntityLogicalName);
						}

						if (string.IsNullOrWhiteSpace(entityform.ReferenceEntityPrimaryKeyLogicalName))
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error retrieving the Primary Key Attribute Name for '{0}'.", entityform.ReferenceEntityLogicalName));
							return;
						}

						var entity = context.RetrieveSingle(
							entityform.ReferenceEntityLogicalName,
							entityform.ReferenceEntityPrimaryKeyLogicalName,
							id,
							FetchAttribute.All);

						if (entity == null)
						{
							ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Could not retrieve entity of type '{0}' where '{1}' equals '{2}'.", entityform.ReferenceEntityLogicalName, entityform.ReferenceEntityPrimaryKeyLogicalName, id));
							return;
						}

						text = entity.GetAttributeValue<string>(primaryNameAttribute) ?? string.Empty;
					}

					if (!modalLookup) { list.Items.Add(new ListItem { Value = id.ToString(), Text = text }); }
				}

				if (!modalLookup)
				{
					list.SelectedValue = id.ToString();
				}
				else
				{
					hiddenField.Value = id.ToString();

					var nameField = container.FindControl(string.Format("{0}_name", entityform.TargetAttributeName));

					if (nameField != null)
					{
						var nameTextBox = nameField as TextBox;

						if (nameTextBox != null) { nameTextBox.Text = text; }
					}
				}
			}
			catch (Exception ex)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("PopulateReferenceEntityField: {0}", ex.ToString()));
			}
		}

		protected void SetEntityReference(OrganizationServiceContext context, Entity entityform, IDictionary<string, object> values)
		{
			var setEntityReference = entityform.GetAttributeValue<bool?>("adx_setentityreference") ?? false;

			if (!setEntityReference) return;

			var id = Guid.Empty;
			var targetAttributeName = entityform.GetAttributeValue<string>("adx_referencetargetlookupattributelogicalname") ?? string.Empty;
			var referenceEntityLogicalName = entityform.GetAttributeValue<string>("adx_referenceentitylogicalname");
			var referenceEntityRelationshipName = entityform.GetAttributeValue<string>("adx_referenceentityrelationshipname") ?? string.Empty;

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var referenceEntitySourceType = entityform.GetAttributeValue<OptionSetValue>("adx_referenceentitysourcetype"); //NEED TO ADD ATTRIBUTE VALUE

			if (string.IsNullOrWhiteSpace(targetAttributeName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Entity Relationship Name is provided. No Entity Reference to set. AssociateEntity will be called during OnInserted event instead.");
				return;
			}

			try
			{
				if (referenceEntitySourceType == null) referenceEntitySourceType = new OptionSetValue(100000000);  //Needs to be set to actual value

				switch (referenceEntitySourceType.Value)
				{
					case 756150000: // Query String
						var referenceQueryStringName = entityform.GetAttributeValue<string>("adx_referencequerystringname") ?? string.Empty;
						var referenceQueryStringValue = HttpContext.Current.Request[referenceQueryStringName];

						if (string.IsNullOrEmpty(referenceQueryStringValue) && HttpContext.Current.Request["refentity"] == referenceEntityLogicalName)
						{
							referenceQueryStringValue = HttpContext.Current.Request["refid"];
						}

						var querystringIsPrimaryKey = entityform.GetAttributeValue<bool?>("adx_referencequerystringisprimarykey") ?? false;

						if (!querystringIsPrimaryKey)
						{
							var referenceQueryAttributeName = entityform.GetAttributeValue<string>("adx_referencequeryattributelogicalname");
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
					case 756150001: // Record Associated to current user 
						var relationship = entityform.GetAttributeValue<string>("adx_referencerecordsourcerelationshipname");  
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

		protected void SetAssociateReference(OrganizationServiceContext context, CrmEntityFormViewInsertingEventArgs e)
		{
			var targetEntityLogicalName = HttpContext.Current.Request["refentity"];
			var targetEntityId = HttpContext.Current.Request["refid"];
			var relationshipName = HttpContext.Current.Request["refrel"];
			var relationshipRole = HttpContext.Current.Request["refrelrole"];
			Guid targetId;

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName) || string.IsNullOrWhiteSpace(targetEntityId) ||
				string.IsNullOrWhiteSpace(relationshipName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Request did not contain parameters 'refentity', 'refid', 'refrel'");
				return;
			}

			var targetMetadataRequest = new RetrieveEntityRequest
			{
				LogicalName = targetEntityLogicalName,
				EntityFilters = EntityFilters.All
			};

			var targetMetadataResponse = (RetrieveEntityResponse)context.Execute(targetMetadataRequest);

			var metadataOneToMany = targetMetadataResponse.EntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == relationshipName);

			if (metadataOneToMany != null)
			{
				if (!Guid.TryParse(targetEntityId, out targetId))
				{
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Request did not contain a valid guid 'refid'");
					return;
				}

				e.Values[metadataOneToMany.ReferencingAttribute] = new EntityReference(targetEntityLogicalName, targetId);
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

		protected dynamic TryConvertAttributeValue(OrganizationServiceContext context, string entityName, string attributeName, object value)
		{
			return EntityFormFunctions.TryConvertAttributeValue(context, entityName, attributeName, value, AttributeTypeCodeDictionary);
		}

		protected void DisplaySuccess(object sender, bool hideForm)
		{
			EntityFormFunctions.DisplayMessage(sender, SuccessMessage, "success", hideForm);
		}

		protected void AssociateCurrentPortalUserOnInserting(OrganizationServiceContext context, Entity entityform, CrmEntityFormViewInsertingEventArgs e)
		{
			if (!HttpContext.Current.Request.IsAuthenticated) return;

			var bAssociatePortalUser = entityform.GetAttributeValue<bool?>("adx_associatecurrentportaluser") ?? false;
			var targetEntityLogicalName = entityform.GetAttributeValue<string>("adx_entityname");
			var portalUserLookupAttributeName = entityform.GetAttributeValue<string>("adx_targetentityportaluserlookupattribute");
			var portalUserLookupAttributeIsActivityParty = entityform.GetAttributeValue<bool?>("adx_portaluserlookupattributeisactivityparty") ?? false;

			if (!bAssociatePortalUser) return;

			if (string.IsNullOrWhiteSpace(targetEntityLogicalName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "adx_entityform.adx_entityname must not be null.");
				return;
			}

			if (string.IsNullOrWhiteSpace(portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceWarning(TraceCategory.Application, "adx_entityform.adx_targetentityportaluserlookupattribute is null.");
				return;
			}

			if (!MetadataHelper.IsAttributeLogicalNameValid(context, targetEntityLogicalName, portalUserLookupAttributeName))
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("'{0}' entity does not contain an attribute with the logical name '{1}'.", targetEntityLogicalName, portalUserLookupAttributeName));
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

		protected void AttachFileOnItemInserted(OrganizationServiceContext context, Entity entityform, object sender, CrmEntityFormViewInsertedEventArgs e)
		{
			Guid? entityid = Guid.Empty;

			if (e.EntityId != null && e.EntityId != Guid.Empty) { entityid = e.EntityId; }

			AttachFileOnSave(context, entityform, sender, entityid);
		}

		protected void AttachFileOnItemUpdated(OrganizationServiceContext context, Entity entityform, object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			var entityid = Guid.Empty;

			if (e.Entity != null && e.Entity.Id != Guid.Empty) { entityid = e.Entity.Id; }

			AttachFileOnSave(context, entityform, sender, entityid);
		}

		protected void AttachFileOnSave(OrganizationServiceContext context, Entity entityform, object sender, Guid? entityid)
		{
			var attachFile = entityform.GetAttributeValue<bool?>("adx_attachfile") ?? false;

			if (!attachFile) return;

			if (entityid == null || entityid == Guid.Empty)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, "File not saved entityid is null or empty.");
				return;
			}

			try
			{
				var logicalName = entityform.GetAttributeValue<string>("adx_entityname");
				var primaryKey = entityform.GetAttributeValue<string>("adx_primarykeyname");

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
					ADXTrace.Instance.TraceError(TraceCategory.Application, "Failed to determine target entity primary key logical name.");
					return;
				}

				var formView = (CrmEntityFormView)sender;
				var fileUpload = (FileUpload)formView.FindControl("AttachFile");

				if (!fileUpload.HasFiles) return;

				var portalContext = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
				var portalUser = portalContext.User == null ? null : portalContext.User.ToEntityReference();

				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(
					requestContext: HttpContext.Current.Request.RequestContext, portalName: PortalName);

				var regarding = new EntityReference(logicalName, entityid.Value);
				
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

		protected virtual void OnFormLoad(object sender, EntityFormLoadEventArgs args)
		{
			var handler = (EventHandler<EntityFormLoadEventArgs>)Events[_eventLoad];

			if (handler != null) handler(this, args);
		}

		protected virtual void OnItemSaving(object sender, EntityFormSavingEventArgs args)
		{
			var handler = (EventHandler<EntityFormSavingEventArgs>)Events[_eventItemSaving];

			if (handler != null) handler(this, args);
		}

		protected virtual void OnItemSaving(object sender, CrmEntityFormViewUpdatingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewUpdatingEventArgs>)Events[_eventItemSaving];

			if (handler != null) handler(this, args);
		}

		protected virtual void OnItemSaving(object sender, CrmEntityFormViewInsertingEventArgs args)
		{
			var handler = (EventHandler<CrmEntityFormViewInsertingEventArgs>)Events[_eventItemSaving];

			if (handler != null) handler(this, args);
		}

		protected virtual void OnItemSaved(object sender, EntityFormSavedEventArgs args)
		{
			var handler = (EventHandler<EntityFormSavedEventArgs>)Events[_eventItemSaved];

			if (handler != null) handler(this, args);
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
}
