/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Security;
using Adxstudio.Xrm.SharePoint;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Portal.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Template used when rendering a subgrid.
	/// </summary>
	public class SharePointDocumentsControlTemplate : SharePointDocumentsCellTemplate
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="contextName"></param>
		/// <param name="bindings"></param>
		public SharePointDocumentsControlTemplate(FormXmlCellMetadata metadata, string contextName, IDictionary<string, CellBinding> bindings)
			: base(metadata)
		{
			ContextName = contextName;
			Bindings = bindings;
		}

		protected string ContextName { get; set; }

		protected IDictionary<string, CellBinding> Bindings { get; private set; }

		protected FileUpload FileUpload { get; private set; }

		protected override void InstantiateControlIn(HtmlControl container)
		{
			var grid = new HtmlGenericControl("div") { ID = Metadata.ControlID };
			grid.Attributes.Add("class", "subgrid");

			if (Metadata.FormView.Mode == FormViewMode.Insert)
			{
				AddFileUpload(container);
			}
			else
			{
				container.Controls.Add(grid);
			}

			Bindings[Metadata.ControlID + "_CrmEntityId_SharePointDocuments"] = new CellBinding
			{
				Get = () =>
				{
					if (FileUpload != null && FileUpload.HasFiles)
					{
						Metadata.FormView.ItemInserted += FormViewInserted;
					}

					return null;
				},
				Set = obj =>
				{
					grid.InnerHtml = BuildSharePointGrid(obj);
				}
			};

			if (!container.Page.IsPostBack) return;

			// On PostBack no databinding occurs so get the id from the viewstate stored on the CrmEntityFormView control.
			grid.InnerHtml = BuildSharePointGrid(Metadata.FormView.CrmEntityId);
		}

		private void AddFileUpload(Control container)
		{
			var result = new SharePointResult(new EntityReference(Metadata.TargetEntityName, Guid.Empty), new CrmEntityPermissionProvider(), PortalCrmConfigurationManager.CreatePortalContext(Metadata.FormView.ContextName).ServiceContext);

			if (!result.PermissionsExist || !result.CanCreate || !result.CanAppend || !result.CanWrite) return;
			
			FileUpload = new FileUpload
			{
				ID = Metadata.ControlID,
				CssClass = CssClass ?? string.Empty,
				AllowMultiple = true
			};
			container.Controls.Add(FileUpload);
		}

		private string BuildSharePointGrid(object id)
		{
			if (id == null) return string.Empty;

			Guid entityId;

			if (!Guid.TryParse(id.ToString(), out entityId)) return string.Empty;

			var portalContext = PortalCrmConfigurationManager.CreatePortalContext(Metadata.FormView.ContextName);
			var target = new EntityReference(Metadata.TargetEntityName, entityId);
			var html = new HtmlHelper(new ViewContext(), new ViewPage());
			var settings = Metadata.SharePointSettings;
			var createEnabled = Metadata.FormView.Mode != FormViewMode.ReadOnly; // Change to false when settings are available.
			var deleteEnabled = Metadata.FormView.Mode != FormViewMode.ReadOnly; // Change to false when settings are available.
			string addFilesButtonLabel = null;
			string addFolderButtonLabel = null;
			string toolbarButtonLabel = null;
			string deleteButtonLabel = null;
			string gridTitle = null;
			string fileNameColumnLabel = null;
			string modifiedColumnLabel = null;
			string parentFolderPrefix = null;
			string accessDeniedMessage = null;
			string errorMessage = null;
			string loadingMessage = null;
			string emptyMessage = null;
			string addFilesModalCssClass = null;
			string addFilesModalTitle = null;
			string addFilesModalTitleCssClass = null;
			var addFilesModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string addFilesModalPrimaryButtonText = null;
			string addFilesModalDismissButtonSrText = null;
			string addFilesModalCloseButtonText = null;
			string addFilesModalPrimaryButtonCssClass = null;
			string addFilesModalCloseButtonCssClass = null;
			string addFilesModalAttachFileLabel = null;
			string addFilesModalAttachFileAccept = null;
			var addFilesModalDisplayOverwriteField = true;
			string addFilesModalOverwriteFieldLabel = null;
			var addFilesModalOverwriteFieldDefaultValue = true;
			string addFilesModalDestinationFolderLabel = null;
			string addFilesModalLeftColumnCssClass = null;
			string addFilesModalRightColumnCssClass = null;
			string addFolderModalCssClass = null;
			string addFolderModalTitle = null;
			string addFolderModalTitleCssClass = null;
			var addFolderModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string addFolderModalPrimaryButtonText = null;
			string addFolderModalDismissButtonSrText = null;
			string addFolderModalCloseButtonText = null;
			string addFolderModalPrimaryButtonCssClass = null;
			string addFolderModalCloseButtonCssClass = null;
			string addFolderModalNameLabel = null;
			string addFolderModalDestinationFolderLabel = null;
			string addFolderModalLeftColumnCssClass = null;
			string addFolderModalRightColumnCssClass = null;
			var deleteFileModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string deleteFileModalCssClass = null;
			string deleteFileModalTitle = null;
			string deleteFileModalTitleCssClass = null;
			string deleteFileModalConfirmation = null;
			string deleteFileModalPrimaryButtonText = null;
			string deleteFileModalDismissButtonSrText = null;
			string deleteFileModalCloseButtonText = null;
			string deleteFileModalPrimaryButtonCssClass = null;
			string deleteFileModalCloseButtonCssClass = null;
			var deleteFolderModalSize = BootstrapExtensions.BootstrapModalSize.Default;
			string deleteFolderModalCssClass = null;
			string deleteFolderModalTitle = null;
			string deleteFolderModalTitleCssClass = null;
			string deleteFolderModalConfirmation = null;
			string deleteFolderModalPrimaryButtonText = null;
			string deleteFolderModalDismissButtonSrText = null;
			string deleteFolderModalCloseButtonText = null;
			string deleteFolderModalPrimaryButtonCssClass = null;
			string deleteFolderModalCloseButtonCssClass = null;

			if (settings != null)
			{
				accessDeniedMessage = Localization.GetLocalizedString(settings.AccessDeniedMessage, Metadata.LanguageCode);
				errorMessage = Localization.GetLocalizedString(settings.ErrorMessage, Metadata.LanguageCode);
				loadingMessage = Localization.GetLocalizedString(settings.LoadingMessage, Metadata.LanguageCode);
				emptyMessage = Localization.GetLocalizedString(settings.EmptyMessage, Metadata.LanguageCode);
				addFilesButtonLabel = Localization.GetLocalizedString(settings.AddFileButtonLabel, Metadata.LanguageCode);
				deleteButtonLabel = Localization.GetLocalizedString(settings.DeleteFileButtonLabel, Metadata.LanguageCode);
				gridTitle = Localization.GetLocalizedString(settings.GridTitle, Metadata.LanguageCode);
				fileNameColumnLabel = Localization.GetLocalizedString(settings.FileNameColumnLabel, Metadata.LanguageCode);
				modifiedColumnLabel = Localization.GetLocalizedString(settings.ModifiedColumnLabel, Metadata.LanguageCode);
				parentFolderPrefix = Localization.GetLocalizedString(settings.ParentFolderPrefix, Metadata.LanguageCode);
				createEnabled = settings.CreateEnabled.GetValueOrDefault(false);
				deleteEnabled = settings.DeleteEnabled.GetValueOrDefault(false);
				var addFilesModal = settings.AddFilesDialog;
				addFilesModalCssClass = addFilesModal == null ? null : addFilesModal.CssClass;
				if (addFilesModal != null && addFilesModal.Size != null)
				{
					addFilesModalSize = addFilesModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				addFilesModalTitle = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.Title, Metadata.LanguageCode);
				addFilesModalTitleCssClass = addFilesModal == null ? null : addFilesModal.TitleCssClass;
				addFilesModalPrimaryButtonText = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.PrimaryButtonText, Metadata.LanguageCode);
				addFilesModalDismissButtonSrText = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.DismissButtonSrText, Metadata.LanguageCode);
				addFilesModalCloseButtonText = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.CloseButtonText, Metadata.LanguageCode);
				addFilesModalPrimaryButtonCssClass = addFilesModal == null ? null : addFilesModal.PrimaryButtonCssClass;
				addFilesModalCloseButtonCssClass = addFilesModal == null ? null : addFilesModal.CloseButtonCssClass;
				addFilesModalAttachFileLabel = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.AttachFileLabel, Metadata.LanguageCode);
				addFilesModalAttachFileAccept = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.AttachFileAccept, Metadata.LanguageCode);
				addFilesModalDisplayOverwriteField = addFilesModal != null && addFilesModal.DisplayOverwriteField.GetValueOrDefault(true);
				addFilesModalOverwriteFieldLabel = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.OverwriteFieldLabel, Metadata.LanguageCode);
				addFilesModalOverwriteFieldDefaultValue = addFilesModal != null && addFilesModal.OverwriteFieldDefaultValue.GetValueOrDefault(true);
				addFilesModalDestinationFolderLabel = addFilesModal == null ? null : Localization.GetLocalizedString(addFilesModal.DestinationFolderLabel, Metadata.LanguageCode);
				addFilesModalLeftColumnCssClass = addFilesModal == null ? null : addFilesModal.LeftColumnCSSClass;
				addFilesModalRightColumnCssClass = addFilesModal == null ? null : addFilesModal.RightColumnCSSClass;
				var addFolderModal = settings.AddFolderDialog;
				addFolderModalCssClass = addFolderModal == null ? null : addFolderModal.CssClass;
				if (addFolderModal != null && addFolderModal.Size != null)
				{
					addFolderModalSize = addFolderModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				addFolderModalTitle = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.Title, Metadata.LanguageCode);
				addFolderModalTitleCssClass = addFolderModal == null ? null : addFolderModal.TitleCssClass;
				addFolderModalPrimaryButtonText = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.PrimaryButtonText, Metadata.LanguageCode);
				addFolderModalDismissButtonSrText = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.DismissButtonSrText, Metadata.LanguageCode);
				addFolderModalCloseButtonText = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.CloseButtonText, Metadata.LanguageCode);
				addFolderModalPrimaryButtonCssClass = addFolderModal == null ? null : addFolderModal.PrimaryButtonCssClass;
				addFolderModalCloseButtonCssClass = addFolderModal == null ? null : addFolderModal.CloseButtonCssClass;
				addFolderModalNameLabel = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.NameLabel, Metadata.LanguageCode);
				addFolderModalDestinationFolderLabel = addFolderModal == null ? null : Localization.GetLocalizedString(addFolderModal.DestinationFolderLabel, Metadata.LanguageCode);
				addFolderModalLeftColumnCssClass = addFolderModal == null ? null : addFolderModal.LeftColumnCSSClass;
				addFolderModalRightColumnCssClass = addFolderModal == null ? null : addFolderModal.RightColumnCSSClass;
				var deleteFileModal = settings.DeleteFileDialog;
				if (deleteFileModal != null && deleteFileModal.Size != null)
				{
					deleteFileModalSize = deleteFileModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				deleteFileModalCssClass = deleteFileModal == null ? null : deleteFileModal.CssClass;
				deleteFileModalTitle = deleteFileModal == null ? null : Localization.GetLocalizedString(deleteFileModal.Title, Metadata.LanguageCode);
				deleteFileModalTitleCssClass = deleteFileModal == null ? null : deleteFileModal.TitleCssClass;
				deleteFileModalConfirmation = deleteFileModal == null ? null : Localization.GetLocalizedString(deleteFileModal.Confirmation, Metadata.LanguageCode);
				deleteFileModalPrimaryButtonText = deleteFileModal == null ? null : Localization.GetLocalizedString(deleteFileModal.PrimaryButtonText, Metadata.LanguageCode);
				deleteFileModalDismissButtonSrText = deleteFileModal == null ? null : Localization.GetLocalizedString(deleteFileModal.DismissButtonSrText, Metadata.LanguageCode);
				deleteFileModalCloseButtonText = deleteFileModal == null ? null : Localization.GetLocalizedString(deleteFileModal.CloseButtonText, Metadata.LanguageCode);
				deleteFileModalPrimaryButtonCssClass = deleteFileModal == null ? null : deleteFileModal.PrimaryButtonCssClass;
				deleteFileModalCloseButtonCssClass = deleteFileModal == null ? null : deleteFileModal.CloseButtonCssClass;
				var deleteFolderModal = settings.DeleteFolderDialog;
				if (deleteFolderModal != null && deleteFolderModal.Size != null)
				{
					deleteFolderModalSize = deleteFolderModal.Size.GetValueOrDefault(BootstrapExtensions.BootstrapModalSize.Default);
				}
				deleteFolderModalCssClass = deleteFolderModal == null ? null : deleteFolderModal.CssClass;
				deleteFolderModalTitle = deleteFolderModal == null ? null : Localization.GetLocalizedString(deleteFolderModal.Title, Metadata.LanguageCode);
				deleteFolderModalTitleCssClass = deleteFolderModal == null ? null : deleteFolderModal.TitleCssClass;
				deleteFolderModalConfirmation = deleteFolderModal == null ? null : Localization.GetLocalizedString(deleteFolderModal.Confirmation, Metadata.LanguageCode);
				deleteFolderModalPrimaryButtonText = deleteFolderModal == null ? null : Localization.GetLocalizedString(deleteFolderModal.PrimaryButtonText, Metadata.LanguageCode);
				deleteFolderModalDismissButtonSrText = deleteFolderModal == null ? null : Localization.GetLocalizedString(deleteFolderModal.DismissButtonSrText, Metadata.LanguageCode);
				deleteFolderModalCloseButtonText = deleteFolderModal == null ? null : Localization.GetLocalizedString(deleteFolderModal.CloseButtonText, Metadata.LanguageCode);
				deleteFolderModalPrimaryButtonCssClass = deleteFolderModal == null ? null : deleteFolderModal.PrimaryButtonCssClass;
				deleteFolderModalCloseButtonCssClass = deleteFolderModal == null ? null : deleteFolderModal.CloseButtonCssClass;
			}

			var regarding = portalContext.ServiceContext.CreateQuery(Metadata.TargetEntityName).First(e => e.GetAttributeValue<Guid>(Metadata.TargetEntityPrimaryKeyName) == entityId);

			var result = new SharePointResult(regarding.ToEntityReference(), new CrmEntityPermissionProvider(), portalContext.ServiceContext);

			if (createEnabled)
			{
				createEnabled = result.PermissionsExist
					&& result.CanCreate
					&& result.CanAppend
					&& result.CanAppendTo
					&& result.CanWrite;
			}

			if (deleteEnabled)
			{
				deleteEnabled = result.PermissionsExist
					&& result.CanDelete;
			}

			var sharePointGridHtml = html.SharePointGrid(target,
				BuildControllerActionUrl("GetSharePointData", "SharePointGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }),
				createEnabled, deleteEnabled, 
				BuildControllerActionUrl("AddSharePointFiles", "SharePointGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }),
				BuildControllerActionUrl("AddSharePointFolder", "SharePointGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }),
				BuildControllerActionUrl("DeleteSharePointItem", "SharePointGrid", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id }),
				Metadata.SharePointGridPageSize ?? 0, gridTitle, fileNameColumnLabel, modifiedColumnLabel, parentFolderPrefix,
				loadingMessage, errorMessage, accessDeniedMessage, emptyMessage,
				addFilesButtonLabel, addFolderButtonLabel, toolbarButtonLabel, deleteButtonLabel, addFilesModalSize,
				addFilesModalCssClass, addFilesModalTitle, addFilesModalDismissButtonSrText, addFilesModalPrimaryButtonText,
				addFilesModalCloseButtonText, addFilesModalTitleCssClass, addFilesModalPrimaryButtonCssClass,
				addFilesModalCloseButtonCssClass, addFilesModalAttachFileLabel, addFilesModalAttachFileAccept,
				addFilesModalDisplayOverwriteField, addFilesModalOverwriteFieldLabel, addFilesModalOverwriteFieldDefaultValue,
				addFilesModalDestinationFolderLabel, addFilesModalLeftColumnCssClass, addFilesModalRightColumnCssClass,
				null, addFolderModalSize, addFolderModalCssClass, addFolderModalTitle, addFolderModalDismissButtonSrText,
				addFolderModalPrimaryButtonText, addFolderModalCloseButtonText, addFolderModalTitleCssClass,
				addFolderModalPrimaryButtonCssClass, addFolderModalCloseButtonCssClass, addFolderModalNameLabel,
				addFolderModalDestinationFolderLabel, addFolderModalLeftColumnCssClass, addFolderModalRightColumnCssClass,
				null, deleteFileModalSize, deleteFileModalCssClass, deleteFileModalTitle, deleteFileModalConfirmation,
				deleteFileModalDismissButtonSrText, deleteFileModalPrimaryButtonText, deleteFileModalCloseButtonText,
				deleteFileModalTitleCssClass, deleteFileModalPrimaryButtonCssClass, deleteFileModalCloseButtonCssClass, null,
				deleteFolderModalSize, deleteFolderModalCssClass, deleteFolderModalTitle, deleteFolderModalConfirmation,
				deleteFolderModalDismissButtonSrText, deleteFolderModalPrimaryButtonText, deleteFolderModalCloseButtonText,
				deleteFolderModalTitleCssClass, deleteFolderModalPrimaryButtonCssClass, deleteFolderModalCloseButtonCssClass);

			return sharePointGridHtml.ToString();
		}

		private void FormViewInserted(object sender, CrmEntityFormViewInsertedEventArgs args)
		{
			if (FileUpload == null || !FileUpload.HasFiles || !args.EntityId.HasValue) return;

			var regarding = new EntityReference(Metadata.TargetEntityName, args.EntityId.Value);

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies();
			var dataAdapter = new SharePointDataAdapter(dataAdapterDependencies);
			dataAdapter.AddFiles(regarding, FileUpload.PostedFiles.Select(file => new HttpPostedFileWrapper(file) as HttpPostedFileBase).ToList());
		}

		/// <summary>
		/// Generates a URL to a controller action
		/// </summary>
		protected string BuildControllerActionUrl(string actionName, string controllerName, object routeValues)
		{
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			var routeData = RouteTable.Routes.GetRouteData(httpContextWrapper) ?? new RouteData();
			var urlHelper = new UrlHelper(new RequestContext(httpContextWrapper, routeData));

			return urlHelper.Action(actionName, controllerName, routeValues);
		}
	}
}
