/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Diagnostics.Trace;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering notes.
	/// </summary>
	public class NotesControlTemplate : NotesCellTemplate
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="contextName"></param>
		/// <param name="bindings"></param>
		public NotesControlTemplate(FormXmlCellMetadata metadata, string contextName, IDictionary<string, CellBinding> bindings, bool isTimeline = false)
			: base(metadata)
		{
			ContextName = contextName;
			Bindings = bindings;
			IsTimeline = isTimeline;
		}

		/// <summary>
		/// Name of the context the portal binds to
		/// </summary>
		protected string ContextName { get; set; }

		/// <summary>
		/// Dictionary of the cell bindings
		/// </summary>
		protected IDictionary<string, CellBinding> Bindings { get; private set; }
		
		/// <summary>
		/// Flag determining whether this is a notes control or a timeline control
		/// </summary>
		protected bool IsTimeline { get; set; }

		/// <summary>
		/// Control instantiation
		/// </summary>
		/// <param name="container"></param>
		protected override void InstantiateControlIn(HtmlControl container)
		{
			Bindings[Metadata.ControlID + "CrmEntityId"] = new CellBinding
			{
				Get = () => null,
				Set = obj =>
				{
					var id = obj.ToString();
					Guid entityId;

					if (!Guid.TryParse(id, out entityId))
					{
						return;
					}

					var notesHtml = BuildNotesControl(entityId);
					var notes = new HtmlGenericControl("div") { ID = Metadata.ControlID, InnerHtml = notesHtml.ToString() };
					container.Controls.Add(notes);
				}
			};

			if (!container.Page.IsPostBack)
			{
				return;
			}

			// On PostBack no databinding occurs so get the id from the viewstate stored on the CrmEntityFormView control.

			var crmEntityId = Metadata.FormView.CrmEntityId;

			if (crmEntityId == null)
			{
				return;
			}

			var notesControlHtml = BuildNotesControl((Guid)crmEntityId);
			var notesControl = new HtmlGenericControl("div") { ID = Metadata.ControlID, InnerHtml = notesControlHtml.ToString() };
			container.Controls.Add(notesControl);
		}

		/// <summary>
		/// Indicates whether entity permissions permit the user to add notes to the target entity.
		/// </summary>
		protected virtual bool TryAssertAddNote(Guid regardingId)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Start Assert Add Note Privilege on: {0} {1}", Metadata.TargetEntityName, regardingId));

			if (!Metadata.FormView.EnableEntityPermissions)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. Entity Permissions have not been enabled.");
				
				return false;
			}

			var regarding = new EntityReference(Metadata.TargetEntityName, regardingId);
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies();
			var serviceContext = dataAdapterDependencies.GetServiceContext();
			var entityPermissionProvider = new CrmEntityPermissionProvider();
			
			if (!entityPermissionProvider.PermissionsExist)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. Entity Permissions have not been defined. Your request could not be completed.");

				return false;
			}

			var entityType = IsTimeline ? "adx_portalcomment" : "annotation";
			var entityMetadata = serviceContext.GetEntityMetadata(regarding.LogicalName, EntityFilters.All);
			var primaryKeyName = entityMetadata.PrimaryIdAttribute;
			var entity =
				serviceContext.CreateQuery(regarding.LogicalName)
					.First(e => e.GetAttributeValue<Guid>(primaryKeyName) == regarding.Id);
			var canAppendTo = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.AppendTo, entity, entityMetadata);
			var canCreate = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Create, entityType, regarding);
			var canAppend = entityPermissionProvider.TryAssert(serviceContext, CrmEntityPermissionRight.Append, entityType, regarding);

			if (canCreate & canAppend & canAppendTo)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Add Note Permission Granted: {0} {1}", EntityNamePrivacy.GetEntityName(Metadata.TargetEntityName), regardingId));

				return true;
			}

			if (!canCreate)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Create notes.");
			}
			else if (!canAppendTo)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Permission Denied. You do not have the appropriate Entity Permissions to Append To {0}.", EntityNamePrivacy.GetEntityName(entity.LogicalName)));
			}
			else
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Permission Denied. You do not have the appropriate Entity Permissions to Append notes.");
			}

			return false;
		}

		/// <summary>
		/// Creates the HTML to render a listing of notes associated to a given target entity record
		/// </summary>
		protected virtual IHtmlString BuildNotesControl(Guid entityId)
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(Metadata.FormView.ContextName);
			var target = new EntityReference(Metadata.TargetEntityName, entityId);
			var html = new HtmlHelper(new ViewContext(), new ViewPage());
			var settings = IsTimeline ? Metadata.TimelineSettings : Metadata.NotesSettings;
			var createEnabled = false;
			var editEnabled = false;
			var deleteEnabled = false;
			string addNoteButtonLabel = null;
			string loadMoreButtonLabel = null;
			string toolbarButtonLabel = null;
			string editNoteButtonLabel = null;
			string deleteNoteButtonLabel = null;
			string listTitle = null;
			List<Order> listOrders = null;
			string notePrivacyLabel = null;
			string accessDeniedMessage = null;
			string errorMessage = null;
			string loadingMessage = null;
			string emptyMessage = null;
			string createModalCssClass = null;
			string createModalTitle = null;
			string createModalTitleCssClass = null;
			var createModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string createModalPrimaryButtonText = null;
			string createModalDismissButtonSrText = null;
			string createModalCloseButtonText = null;
			string createModalPrimaryButtonCssClass = null;
			string createModalCloseButtonCssClass = null;
			string createModalNoteFieldLabel = null;
			var createModalDisplayPrivacyOptionField = false;
			string createModalPrivacyOptionFieldLabel = null;
			var createModalPrivacyOptionFieldDefaultValue = false;
			var createModalDisplayAttachFile = true;
			string createModalAttachFileLabel = null;
			string createModalAttachFileAccept = null;
			int? createModalNoteFieldColumns = 20;
			int? createModalNoteFieldRows = 9;
			string createModalLeftColumnCssClass = null;
			string createModalRightColumnCssClass = null;
			string editModalCssClass = null;
			string editModalTitle = null;
			string editModalTitleCssClass = null;
			var editModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string editModalPrimaryButtonText = null;
			string editModalDismissButtonSrText = null;
			string editModalCloseButtonText = null;
			string editModalPrimaryButtonCssClass = null;
			string editModalCloseButtonCssClass = null;
			string editModalNoteFieldLabel = null;
			var editModalDisplayPrivacyOptionField = false;
			string editModalPrivacyOptionFieldLabel = null;
			var editModalPrivacyOptionFieldDefaultValue = false;
			var editModalDisplayAttachFile = true;
			string editModalAttachFileLabel = null;
			string editModalAttachFileAccept = null;
			int? editModalNoteFieldColumns = 20;
			int? editModalNoteFieldRows = 9;
			string editModalLeftColumnCssClass = null;
			string editModalRightColumnCssClass = null;
			var deleteModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string deleteModalCssClass = null;
			string deleteModalTitle = null;
			string deleteModalTitleCssClass = null;
			string deleteModalConfirmation = null;
			string deleteModalPrimaryButtonText = null;
			string deleteModalDismissButtonSrText = null;
			string deleteModalCloseButtonText = null;
			string deleteModalPrimaryButtonCssClass = null;
			string deleteModalCloseButtonCssClass = null;
			AnnotationSettings attachmentSettings = null;

			if (settings != null)
			{
				accessDeniedMessage = Localization.GetLocalizedString(settings.AccessDeniedMessage, Metadata.LanguageCode);
				errorMessage = Localization.GetLocalizedString(settings.ErrorMessage, Metadata.LanguageCode);
				loadingMessage = Localization.GetLocalizedString(settings.LoadingMessage, Metadata.LanguageCode);
				emptyMessage = Localization.GetLocalizedString(settings.EmptyMessage, Metadata.LanguageCode);
				notePrivacyLabel = Localization.GetLocalizedString(settings.NotePrivacyLabel, Metadata.LanguageCode);
				addNoteButtonLabel = Localization.GetLocalizedString(settings.AddNoteButtonLabel, Metadata.LanguageCode);
				loadMoreButtonLabel = IsTimeline ? Localization.GetLocalizedString(((JsonConfiguration.TimelineMetadata)settings).LoadMoreButtonLabel, Metadata.LanguageCode) : null;
				editNoteButtonLabel = Localization.GetLocalizedString(settings.EditNoteButtonLabel, Metadata.LanguageCode);
				deleteNoteButtonLabel = Localization.GetLocalizedString(settings.DeleteNoteButtonLabel, Metadata.LanguageCode);
				listTitle = Localization.GetLocalizedString(settings.ListTitle, Metadata.LanguageCode);
				if (settings.ListOrders != null && settings.ListOrders.Any())
				{
					listOrders = settings.ListOrders;
				}
				var createModal = settings.CreateDialog;
				createModalCssClass = createModal == null ? null : createModal.CssClass;
				if (createModal != null && createModal.Size != null)
				{
					createModalSize = createModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				createModalTitle = createModal == null ? null : Localization.GetLocalizedString(createModal.Title, Metadata.LanguageCode);
				createModalTitleCssClass = createModal == null ? null : createModal.TitleCssClass;
				createModalPrimaryButtonText = createModal == null ? null : Localization.GetLocalizedString(createModal.PrimaryButtonText, Metadata.LanguageCode);
				createModalDismissButtonSrText = createModal == null ? null : Localization.GetLocalizedString(createModal.DismissButtonSrText, Metadata.LanguageCode);
				createModalCloseButtonText = createModal == null ? null : Localization.GetLocalizedString(createModal.CloseButtonText, Metadata.LanguageCode);
				createModalPrimaryButtonCssClass = createModal == null ? null : createModal.PrimaryButtonCssClass;
				createModalCloseButtonCssClass = createModal == null ? null : createModal.CloseButtonCssClass;
				createModalNoteFieldLabel = createModal == null ? null : Localization.GetLocalizedString(createModal.NoteFieldLabel, Metadata.LanguageCode);
				createModalDisplayPrivacyOptionField = createModal != null && createModal.DisplayPrivacyOptionField.GetValueOrDefault(false);
				createModalPrivacyOptionFieldLabel = createModal == null ? null : Localization.GetLocalizedString(createModal.PrivacyOptionFieldLabel, Metadata.LanguageCode);
				createModalPrivacyOptionFieldDefaultValue = createModal != null && createModal.PrivacyOptionFieldDefaultValue.GetValueOrDefault(false);
				createModalDisplayAttachFile = createModal == null || createModal.DisplayAttachFile.GetValueOrDefault(true);
				createModalAttachFileLabel = createModal == null ? null : Localization.GetLocalizedString(createModal.AttachFileLabel, Metadata.LanguageCode);
				createModalAttachFileAccept = createModal == null ? null : Localization.GetLocalizedString(createModal.AttachFileAccept, Metadata.LanguageCode);
				createModalNoteFieldColumns = createModal == null ? 20 : createModal.NoteFieldColumns.GetValueOrDefault(20);
				createModalNoteFieldRows = createModal == null ? 9 : createModal.NoteFieldRows.GetValueOrDefault(9);
				createModalLeftColumnCssClass = createModal == null ? null : createModal.LeftColumnCSSClass;
				createModalRightColumnCssClass = createModal == null ? null : createModal.RightColumnCSSClass;
				var editModal = settings.EditDialog;
				editModalCssClass = editModal == null ? null : editModal.CssClass;
				if (editModal != null && editModal.Size != null)
				{
					editModalSize = editModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				editModalTitle = editModal == null ? null : Localization.GetLocalizedString(editModal.Title, Metadata.LanguageCode);
				editModalTitleCssClass = editModal == null ? null : editModal.TitleCssClass;
				editModalPrimaryButtonText = editModal == null ? null : Localization.GetLocalizedString(editModal.PrimaryButtonText, Metadata.LanguageCode);
				editModalDismissButtonSrText = editModal == null ? null : Localization.GetLocalizedString(editModal.DismissButtonSrText, Metadata.LanguageCode);
				editModalCloseButtonText = editModal == null ? null : Localization.GetLocalizedString(editModal.CloseButtonText, Metadata.LanguageCode);
				editModalPrimaryButtonCssClass = editModal == null ? null : editModal.PrimaryButtonCssClass;
				editModalCloseButtonCssClass = editModal == null ? null : editModal.CloseButtonCssClass;
				editModalNoteFieldLabel = editModal == null ? null : Localization.GetLocalizedString(editModal.NoteFieldLabel, Metadata.LanguageCode);
				editModalDisplayPrivacyOptionField = editModal != null && editModal.DisplayPrivacyOptionField.GetValueOrDefault(false);
				editModalPrivacyOptionFieldLabel = editModal == null ? null : Localization.GetLocalizedString(editModal.PrivacyOptionFieldLabel, Metadata.LanguageCode);
				editModalPrivacyOptionFieldDefaultValue = editModal != null && editModal.PrivacyOptionFieldDefaultValue.GetValueOrDefault(false);
				editModalDisplayAttachFile = editModal == null || editModal.DisplayAttachFile.GetValueOrDefault(true);
				editModalAttachFileLabel = editModal == null ? null : Localization.GetLocalizedString(editModal.AttachFileLabel, Metadata.LanguageCode);
				editModalAttachFileAccept = editModal == null ? null : Localization.GetLocalizedString(editModal.AttachFileAccept, Metadata.LanguageCode);
				editModalNoteFieldColumns = editModal == null ? 20 : editModal.NoteFieldColumns.GetValueOrDefault(20);
				editModalNoteFieldRows = editModal == null ? 9 : editModal.NoteFieldRows.GetValueOrDefault(9);
				editModalLeftColumnCssClass = editModal == null ? null : editModal.LeftColumnCSSClass;
				editModalRightColumnCssClass = editModal == null ? null : editModal.RightColumnCSSClass;
				var deleteModal = settings.DeleteDialog;
				if (deleteModal != null && deleteModal.Size != null)
				{
					deleteModalSize = deleteModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				deleteModalCssClass = deleteModal == null ? null : deleteModal.CssClass;
				deleteModalTitle = deleteModal == null ? null : Localization.GetLocalizedString(deleteModal.Title, Metadata.LanguageCode);
				deleteModalTitleCssClass = deleteModal == null ? null : deleteModal.TitleCssClass;
				deleteModalConfirmation = deleteModal == null ? null : Localization.GetLocalizedString(deleteModal.Confirmation, Metadata.LanguageCode);
				deleteModalPrimaryButtonText = deleteModal == null ? null : Localization.GetLocalizedString(deleteModal.PrimaryButtonText, Metadata.LanguageCode);
				deleteModalDismissButtonSrText = deleteModal == null ? null : Localization.GetLocalizedString(deleteModal.DismissButtonSrText, Metadata.LanguageCode);
				deleteModalCloseButtonText = deleteModal == null ? null : Localization.GetLocalizedString(deleteModal.CloseButtonText, Metadata.LanguageCode);
				deleteModalPrimaryButtonCssClass = deleteModal == null ? null : deleteModal.PrimaryButtonCssClass;
				deleteModalCloseButtonCssClass = deleteModal == null ? null : deleteModal.CloseButtonCssClass;
				createEnabled = settings.CreateEnabled.GetValueOrDefault(false);
				editEnabled = settings.EditEnabled.GetValueOrDefault(false);
				deleteEnabled = settings.DeleteEnabled.GetValueOrDefault(false);

				attachmentSettings = new AnnotationSettings(portalContext.ServiceContext, 
					true,
					settings.AttachFileLocation.GetValueOrDefault(StorageLocation.CrmDocument),
					settings.AttachFileAccept,
					settings.AttachFileRestrictAccept.GetValueOrDefault(false),
					Localization.GetLocalizedString(settings.AttachFileRestrictErrorMessage, Metadata.LanguageCode),
					settings.AttachFileMaximumSize.HasValue ? Convert.ToUInt64(settings.AttachFileMaximumSize.Value) << 10 : (ulong?)null,
					Localization.GetLocalizedString(settings.AttachFileMaximumSizeErrorMessage, Metadata.LanguageCode),
					IsTimeline ? ((JsonConfiguration.TimelineMetadata)settings).AttachFileAcceptExtensions : string.Empty,
					isPortalComment: IsTimeline);
			}

			var canAddNotes = TryAssertAddNote(entityId);

			if (createEnabled)
			{
				createEnabled = canAddNotes;
			}

			if (deleteEnabled)
			{
				deleteEnabled = Metadata.FormView.EnableEntityPermissions;
			}

			if (editEnabled)
			{
				editEnabled = Metadata.FormView.EnableEntityPermissions;
			}

			string getServiceUrl = null;
			string addServiceUrl = null;
			string updateServiceUrl = null;
			string deleteServiceUrl = null;
			string getAttachmentsServiceUrl = null;
			bool isTimeline;
			bool useScrollingPagination;
			string entityLogicalName = null;
			string textAttributeName = null;

			if (IsTimeline)
			{
				getServiceUrl = BuildControllerActionUrl("GetActivities", "EntityActivity", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				addServiceUrl = BuildControllerActionUrl("AddPortalComment", "EntityActivity", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				updateServiceUrl = null;
				deleteServiceUrl = null;
				getAttachmentsServiceUrl = BuildControllerActionUrl("GetAttachments", "EntityActivity", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				useScrollingPagination = true;
				isTimeline = true;
				entityLogicalName = "adx_portalcomment";
				textAttributeName = "description";
			}
			else
			{
				getServiceUrl = BuildControllerActionUrl("GetNotes", "EntityNotes", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				addServiceUrl = BuildControllerActionUrl("AddNote", "EntityNotes", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				updateServiceUrl = BuildControllerActionUrl("UpdateNote", "EntityNotes", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				deleteServiceUrl = BuildControllerActionUrl("DeleteNote", "EntityNotes", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
				getAttachmentsServiceUrl = null;
				useScrollingPagination = false;
				isTimeline = false;
				entityLogicalName = "annotation";
				textAttributeName = "notetext";
			}

			var entityMetadata = portalContext.ServiceContext.GetEntityMetadata(entityLogicalName, EntityFilters.All);
			var textAttribute = (MemoAttributeMetadata)entityMetadata.Attributes.Where((att) => { return att.LogicalName == textAttributeName; }).First();

			var notesHtml = html.Notes(target, getServiceUrl, createEnabled, editEnabled, deleteEnabled, addServiceUrl,
				updateServiceUrl,  deleteServiceUrl, getAttachmentsServiceUrl, Metadata.NotesPageSize ?? 0,
				listOrders, listTitle, loadingMessage, errorMessage, accessDeniedMessage, emptyMessage, addNoteButtonLabel,
				loadMoreButtonLabel, toolbarButtonLabel, editNoteButtonLabel, deleteNoteButtonLabel, notePrivacyLabel, createModalSize,
				createModalCssClass, createModalTitle, createModalDismissButtonSrText, createModalPrimaryButtonText,
				createModalCloseButtonText, createModalTitleCssClass, createModalPrimaryButtonCssClass,
				createModalCloseButtonCssClass, createModalNoteFieldLabel, createModalDisplayAttachFile, createModalAttachFileLabel,
				createModalAttachFileAccept, createModalDisplayPrivacyOptionField, createModalPrivacyOptionFieldLabel,
				createModalPrivacyOptionFieldDefaultValue, createModalNoteFieldColumns, createModalNoteFieldRows,
				createModalLeftColumnCssClass, createModalRightColumnCssClass, null, editModalSize, editModalCssClass,
				editModalTitle, editModalDismissButtonSrText, editModalPrimaryButtonText, editModalCloseButtonText,
				editModalTitleCssClass, editModalPrimaryButtonCssClass, editModalCloseButtonCssClass, editModalNoteFieldLabel,
				editModalDisplayAttachFile, editModalAttachFileLabel, editModalAttachFileAccept, editModalDisplayPrivacyOptionField,
				editModalPrivacyOptionFieldLabel, editModalPrivacyOptionFieldDefaultValue, editModalNoteFieldColumns,
				editModalNoteFieldRows, editModalLeftColumnCssClass, editModalRightColumnCssClass, null, deleteModalSize, deleteModalCssClass,
				deleteModalTitle, deleteModalConfirmation, deleteModalDismissButtonSrText, deleteModalPrimaryButtonText,
				deleteModalCloseButtonText, deleteModalTitleCssClass, deleteModalPrimaryButtonCssClass,
				deleteModalCloseButtonCssClass, isTimeline: isTimeline, useScrollingPagination: useScrollingPagination, 
				attachmentSettings: attachmentSettings, textMaxLength: textAttribute.MaxLength);

			return notesHtml;
		}

		/// <summary>
		/// Generates a URL to a controller action
		/// </summary>
		protected string BuildControllerActionUrl(string actionName, string controllerName, object routeValues)
		{
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper);

			if (routeData == null)
			{
				routeData = new RouteData();
			}

			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action(actionName, controllerName, routeValues);
		}
	}
}
