/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering an grid of SharePoint folders and files within Adxstudio Portals applications.
	/// </summary>
	/// <remarks>Requires Bootstrap 3, jQuery and the following files: ~/js/jquery.bootstrap-pagination.js, ~/js/sharepoint-grid.js</remarks>
	public static class SharePointGridExtensions
	{
		private static readonly string DefaultAddFilesModalTitle = ResourceManager.GetString("Add_Files");
		private static readonly string DefaultAddFilesModalPrimaryButtonText = ResourceManager.GetString("Add_Files");
		private static readonly string DefaultAddFilesModalAttachFileLabel = ResourceManager.GetString("Default_AddFiles_ModalAttachFileLabel");
		private static readonly string DefaultAddFilesModalOverwriteFieldLabel = ResourceManager.GetString("Overwrite_Existing_Files_Label_Text");
		private static readonly string DefaultAddFolderModalTitle = ResourceManager.GetString("New_Folder_Button_Text");
		private static readonly string DefaultAddFolderModalPrimaryButtonText = ResourceManager.GetString("Default_AddFolder_Button_Text");
		private static readonly string DefaultAddFolderModalNameLabel = ResourceManager.GetString("Name_DefaultText");
		private static readonly string DefaultAddModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultAddModalDestinationLabel = ResourceManager.GetString("Destination_Label_Text");
		private const string DefaultAddModalFormLeftColumnCssClass = "col-sm-3";
		private const string DefaultAddModalFormRightColumnCssClass = "col-sm-9";
		private static readonly string DefaultDeleteFileModalTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Delete_File_ModalTitle");
		private static readonly string DefaultDeleteFileModalConfirmation = ResourceManager.GetString("File_Deletion_Confirmation_Message");
		private static readonly string DefaultDeleteFolderModalTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Delete_Folder_ModalTitle");
		private static readonly string DefaultDeleteFolderModalConfirmation = ResourceManager.GetString("Folder_Deletion_Confirmation_Message");
		private static readonly string DefaultDeleteModalPrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultDeleteModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultFileNameColumnTitle = ResourceManager.GetString("Name_DefaultText");
		private static readonly string DefaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static readonly string DefaultModifiedColumnTitle = ResourceManager.GetString("Default_Modified_Column_Title");
		private static readonly string DefaultParentFolderPrefix = ResourceManager.GetString("Upto_Text");
		private static readonly string DefaultSharePointGridTitle = ResourceManager.GetString("Document_Library_Title_Text");
		private static readonly string DefaultSharePointGridLoadingMessage = "<span class='fa fa-spinner fa-pulse' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Grid_Loading_Message");
		private static readonly string DefaultSharePointGridErrorMessage = ResourceManager.GetString("Error_Completing_Request_Error_Message");
		private static readonly string DefaultSharePointGridAccessDeniedMessage = ResourceManager.GetString("Access_Denied_No_Permissions_To_View_These_Folders_Message");
		private static readonly string DefaultSharePointGridEmptyMessage = ResourceManager.GetString("Default_SharePoint_Grid_Empty_Message");
		private static readonly string DefaultAddFilesButtonLabel = "<span class='fa fa-plus-circle' aria-hidden='true'></span> " + ResourceManager.GetString("Add_Files");
		private static readonly string DefaultAddFolderButtonLabel = "<span class='fa fa-folder' aria-hidden='true'></span> " + ResourceManager.GetString("New_Folder_Button_Text");
		private const string DefaultToolbarButtonLabel = "<span class='fa fa-chevron-circle-down fa-fw' aria-hidden='true'></span>";
		private static readonly string DefaultDeleteButtonLabel = "<span class='fa fa-trash-o fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");

		/// <summary>
		/// Render a SharePoint grid of files and folders associated with a target entity and provide a modal dialog to add new files.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="target">EntityReference for the target entity record <see cref="EntityReference"/></param>
		/// <param name="serviceUrlGet">URL to the service that handles the get data request.</param>
		/// <param name="createEnabled">Boolean indicating whether adding files and folders is enabled or not.</param>
		/// <param name="deleteEnabled">Boolean indicating whether deleting files and folders is enabled or not.</param>
		/// <param name="serviceUrlAddFiles">URL to the service that handles the add files request.</param>
		/// <param name="serviceUrlAddFolder">URL to the service that handles the add folder request.</param>
		/// <param name="serviceUrlDelete">URL to the service that handles the delete item request.</param>
		/// <param name="pageSize">Number of records to display per page.</param>
		/// <param name="title">Title of the SharePoint grid.</param>
		/// <param name="fileNameColumnLabel">Text displayed for the file name column header.</param>
		/// <param name="modifiedColumnLabel">Text displayed for the modified on column header.</param>
		/// <param name="parentFolderPrefix">Text displayed for the parent folder.</param>
		/// <param name="loadingMessage">Message to be displayed during loading of SharePoint grid.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs loading SharePoint grid.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the SharePoint grid.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no files or folders found.</param>
		/// <param name="addFilesButtonLabel">Text displayed for the button that launches the add files modal.</param>
		/// <param name="addFolderButtonLabel">Text displayed for the button that launches the add folder modal.</param>
		/// <param name="toolbarButtonLabel">Text displayed for the toolbar button dropdown.</param>
		/// <param name="deleteButtonLabel">Text displayed for the delete button.</param>
		/// <param name="modalAddFilesSize">Size of the add files modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalAddFilesCssClass">CSS class assigned to the add files modal element.</param>
		/// <param name="modalAddFilesTitle">Text assigned to the add files modal title.</param>
		/// <param name="modalAddFilesDismissButtonSrText">Text assigned to the add files modal dismiss button for screen readers only.</param>
		/// <param name="modalAddFilesPrimaryButtonText">Text assigned to the add files modal primary button.</param>
		/// <param name="modalAddFilesCancelButtonText">Text assigned to the add files modal cancel button.</param>
		/// <param name="modalAddFilesTitleCssClass">CSS class assigned to the add files title.</param>
		/// <param name="modalAddFilesPrimaryButtonCssClass">CSS class assigned to the add files modal primary button.</param>
		/// <param name="modalAddFilesCancelButtonCssClass">CSS class assigned to the add files modal cancel button.</param>
		/// <param name="modalAddFilesAttachFileLabel">Text displayed for the label of the add files modal file input.</param>
		/// <param name="modalAddFilesAttachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="modalAddFilesDisplayOverwriteField">Boolean value that indicates if the add files modal should display an overwrite checkbox.</param>
		/// <param name="modalAddFilesOverwriteFieldLabel">Text displayed for the label of the add files modal overwrite existing file checkbox.</param>
		/// <param name="modalAddFilesOverwriteFieldDefaultValue">Default value assigned to the add files modal overwrite checkbox.</param>
		/// <param name="modalAddFilesDestinationFolderLabel">Text displayed for the destination folder label.</param>
		/// <param name="modalAddFilesFormRightColumnCssClass">CSS class assigned to the add files modal form's left column.</param>
		/// <param name="modalAddFilesFormLeftColumnCssClass">CSS class assigned to the add files modal form's right column.</param>
		/// <param name="modalAddFilesHtmlAttributes">Collection of HTML attributes to be assigned to the add files modal element.</param>
		/// <param name="modalAddFolderSize">Size of the add folder modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalAddFolderCssClass">CSS class assigned to the add folder modal element.</param>
		/// <param name="modalAddFolderTitle">Text assigned to the add folder modal title.</param>
		/// <param name="modalAddFolderDismissButtonSrText">Text assigned to the add folder modal dismiss button for screen readers only.</param>
		/// <param name="modalAddFolderPrimaryButtonText">Text assigned to the add folder modal primary button.</param>
		/// <param name="modalAddFolderCancelButtonText">Text assigned to the add folder modal cancel button.</param>
		/// <param name="modalAddFolderTitleCssClass">CSS class assigned to the add folder title.</param>
		/// <param name="modalAddFolderPrimaryButtonCssClass">CSS class assigned to the add folder modal primary button.</param>
		/// <param name="modalAddFolderCancelButtonCssClass">CSS class assigned to the add folder modal cancel button.</param>
		/// <param name="modalAddFolderNameLabel">Text displayed for the label of the add folder modal file input.</param>
		/// <param name="modalAddFolderDestinationFolderLabel">Text displayed for the destination folder label.</param>
		/// <param name="modalAddFolderFormRightColumnCssClass">CSS class assigned to the add folder modal form's left column.</param>
		/// <param name="modalAddFolderFormLeftColumnCssClass">CSS class assigned to the add folder modal form's right column.</param>
		/// <param name="modalAddFolderHtmlAttributes">Collection of HTML attributes to be assigned to the add folder modal element.</param>
		/// <param name="modalDeleteFileSize">Size of the delete file modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalDeleteFileCssClass">CSS class assigned to the delete file modal element.</param>
		/// <param name="modalDeleteFileTitle">Text assigned to the delete file modal title.</param>
		/// <param name="modalDeleteFileConfirmation">Text displayed for the confirmation message of the delete file modal.</param>
		/// <param name="modalDeleteFileDismissButtonSrText">Text assigned to the delete file modal dismiss button for screen readers only.</param>
		/// <param name="modalDeleteFilePrimaryButtonText">Text assigned to the delete file modal primary button.</param>
		/// <param name="modalDeleteFileCancelButtonText">Text assigned to the delete file modal cancel button.</param>
		/// <param name="modalDeleteFileTitleCssClass">CSS class assigned to the delete file title.</param>
		/// <param name="modalDeleteFilePrimaryButtonCssClass">CSS class assigned to the delete file modal primary button.</param>
		/// <param name="modalDeleteFileCancelButtonCssClass">CSS class assigned to the delete file modal cancel button.</param>
		/// <param name="modalDeleteFileHtmlAttributes">Collection of HTML attributes to be assigned to the delete file modal element.</param>
		/// <param name="modalDeleteFolderSize">Size of the delete folder modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalDeleteFolderCssClass">CSS class assigned to the delete folder modal element.</param>
		/// <param name="modalDeleteFolderTitle">Text assigned to the delete folder modal title.</param>
		/// <param name="modalDeleteFolderConfirmation">Text displayed for the confirmation message of the delete folder modal.</param>
		/// <param name="modalDeleteFolderDismissButtonSrText">Text assigned to the delete folder modal dismiss button for screen readers only.</param>
		/// <param name="modalDeleteFolderPrimaryButtonText">Text assigned to the delete folder modal primary button.</param>
		/// <param name="modalDeleteFolderCancelButtonText">Text assigned to the delete folder modal cancel button.</param>
		/// <param name="modalDeleteFolderTitleCssClass">CSS class assigned to the delete folder title.</param>
		/// <param name="modalDeleteFolderPrimaryButtonCssClass">CSS class assigned to the delete folder modal primary button.</param>
		/// <param name="modalDeleteFolderCancelButtonCssClass">CSS class assigned to the delete folder modal cancel button.</param>
		/// <param name="modalDeleteFolderHtmlAttributes">Collection of HTML attributes to be assigned to the delete folder modal element.</param>
		public static IHtmlString SharePointGrid(this HtmlHelper html, EntityReference target, string serviceUrlGet, bool createEnabled = false, bool deleteEnabled = false,
			string serviceUrlAddFiles = null, string serviceUrlAddFolder = null, string serviceUrlDelete = null, int pageSize = 0,
			string title = null, string fileNameColumnLabel = null, string modifiedColumnLabel = null, string parentFolderPrefix = null,
			string loadingMessage = null, string errorMessage = null, string accessDeniedMessage = null, string emptyMessage = null,
			string addFilesButtonLabel = null, string addFolderButtonLabel = null, string toolbarButtonLabel = null, string deleteButtonLabel = null,
			BootstrapExtensions.BootstrapModalSize modalAddFilesSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalAddFilesCssClass = null, string modalAddFilesTitle = null, string modalAddFilesDismissButtonSrText = null,
			string modalAddFilesPrimaryButtonText = null, string modalAddFilesCancelButtonText = null, string modalAddFilesTitleCssClass = null,
			string modalAddFilesPrimaryButtonCssClass = null, string modalAddFilesCancelButtonCssClass = null,
			string modalAddFilesAttachFileLabel = null, string modalAddFilesAttachFileAccept = null,
			bool modalAddFilesDisplayOverwriteField = true, string modalAddFilesOverwriteFieldLabel = null,
			bool modalAddFilesOverwriteFieldDefaultValue = true, string modalAddFilesDestinationFolderLabel = null,
			string modalAddFilesFormLeftColumnCssClass = null, string modalAddFilesFormRightColumnCssClass = null,
			IDictionary<string, string> modalAddFilesHtmlAttributes = null,
			BootstrapExtensions.BootstrapModalSize modalAddFolderSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalAddFolderCssClass = null, string modalAddFolderTitle = null, string modalAddFolderDismissButtonSrText = null,
			string modalAddFolderPrimaryButtonText = null, string modalAddFolderCancelButtonText = null, string modalAddFolderTitleCssClass = null,
			string modalAddFolderPrimaryButtonCssClass = null, string modalAddFolderCancelButtonCssClass = null,
			string modalAddFolderNameLabel = null, string modalAddFolderDestinationFolderLabel = null,
			string modalAddFolderFormLeftColumnCssClass = null, string modalAddFolderFormRightColumnCssClass = null,
			IDictionary<string, string> modalAddFolderHtmlAttributes = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteFileSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteFileCssClass = null, string modalDeleteFileTitle = null, string modalDeleteFileConfirmation = null, string modalDeleteFileDismissButtonSrText = null,
			string modalDeleteFilePrimaryButtonText = null, string modalDeleteFileCancelButtonText = null, string modalDeleteFileTitleCssClass = null,
			string modalDeleteFilePrimaryButtonCssClass = null, string modalDeleteFileCancelButtonCssClass = null, IDictionary<string, string> modalDeleteFileHtmlAttributes = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteFolderSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteFolderCssClass = null, string modalDeleteFolderTitle = null, string modalDeleteFolderConfirmation = null, string modalDeleteFolderDismissButtonSrText = null,
			string modalDeleteFolderPrimaryButtonText = null, string modalDeleteFolderCancelButtonText = null, string modalDeleteFolderTitleCssClass = null,
			string modalDeleteFolderPrimaryButtonCssClass = null, string modalDeleteFolderCancelButtonCssClass = null, IDictionary<string, string> modalDeleteFolderHtmlAttributes = null)
		{
			var container = new TagBuilder("div");
			container.AddCssClass("sharepoint-grid");
			container.AddCssClass("subgrid");
			container.MergeAttribute("data-url-get", serviceUrlGet);
			container.MergeAttribute("data-url-add-files", serviceUrlAddFiles);
			container.MergeAttribute("data-url-add-folder", serviceUrlAddFolder);
			container.MergeAttribute("data-url-delete", serviceUrlDelete);
			container.MergeAttribute("data-add-enabled", createEnabled.ToString());
			container.MergeAttribute("data-delete-enabled", deleteEnabled.ToString());
			container.MergeAttribute("data-target", JsonConvert.SerializeObject(target));
			container.MergeAttribute("data-pagesize", pageSize.ToString(CultureInfo.InvariantCulture));

			if (!string.IsNullOrWhiteSpace(title))
			{
				var header = new TagBuilder("div");
				header.AddCssClass("page-header");
				header.InnerHtml = (new TagBuilder("h3") { InnerHtml = title.GetValueOrDefault(DefaultSharePointGridTitle) }).ToString();
				container.InnerHtml += header.ToString();
			}

			var template = new TagBuilder("script");
			template.MergeAttribute("id", "sharepoint-template");
			template.MergeAttribute("type", "text/x-handlebars-template");
			var grid = new TagBuilder("div");
			grid.AddCssClass("view-grid");
			var table = new TagBuilder("table");
			table.AddCssClass("table");
			table.AddCssClass("table-striped");
			table.MergeAttribute("aria-live", "polite");
			table.MergeAttribute("aria-relevant", "additions");
			var thead = new TagBuilder("thead");
			var theadRow = new TagBuilder("tr");
			var fileNameColumnHeader = new TagBuilder("th");
			fileNameColumnHeader.MergeAttribute("data-sort-name", "FileLeafRef");
			fileNameColumnHeader.AddCssClass("sort-enabled");
			var fileNameColumnLink = new TagBuilder("a");
			fileNameColumnLink.MergeAttribute("href", "#");
			fileNameColumnLink.InnerHtml = fileNameColumnLabel.GetValueOrDefault(DefaultFileNameColumnTitle);
			fileNameColumnHeader.InnerHtml = fileNameColumnLink.ToString();
			theadRow.InnerHtml += fileNameColumnHeader.ToString();
			var modifiedColumnHeader = new TagBuilder("th");
			modifiedColumnHeader.MergeAttribute("data-sort-name", "Modified");
			modifiedColumnHeader.AddCssClass("sort-enabled");
			var modifiedColumnLink = new TagBuilder("a");
			modifiedColumnLink.MergeAttribute("href", "#");
			modifiedColumnLink.InnerHtml = modifiedColumnLabel.GetValueOrDefault(DefaultModifiedColumnTitle);
			modifiedColumnHeader.InnerHtml = modifiedColumnLink.ToString();
			theadRow.InnerHtml += modifiedColumnHeader.ToString();
			if (deleteEnabled)
			{
				var actionsHeader = new TagBuilder("th");
				actionsHeader.AddCssClass("sort-disabled");
				actionsHeader.InnerHtml = "<span class='sr-only'>Actions</span>";
				theadRow.InnerHtml += actionsHeader.ToString();
			}
			thead.InnerHtml = theadRow.ToString();
			table.InnerHtml += thead.ToString();
			table.InnerHtml += "{{#each SharePointItems}}";
			table.InnerHtml += "{{#if IsFolder}}";
			var folderRow = new TagBuilder("tr");
			folderRow.AddCssClass("sp-item");
			folderRow.MergeAttribute("data-id", "{{Id}}");
			folderRow.MergeAttribute("data-foldername", "{{Name}}");
			folderRow.MergeAttribute("data-folderpath", "{{FolderPath}}");
			var folderNameCell = new TagBuilder("td");
			folderNameCell.MergeAttribute("data-type", "System.String");
			folderNameCell.MergeAttribute("data-value", "{{Name}}");
			var folderLink = new TagBuilder("a");
			folderLink.AddCssClass("folder-link");
			folderLink.MergeAttribute("data-folderpath", "{{FolderPath}}");
			folderLink.MergeAttribute("href", "#");
			folderLink.InnerHtml += "{{#if IsParent}}";
			folderLink.InnerHtml += "<span class='fa fa-level-up text-primary' aria-hidden='true'></span> ";
			folderLink.InnerHtml += parentFolderPrefix.GetValueOrDefault(DefaultParentFolderPrefix);
			folderLink.InnerHtml += "{{Name}}";
			folderLink.InnerHtml += "{{else}}";
			folderLink.InnerHtml += "<span class='fa fa-folder text-primary' aria-hidden='true'></span> {{Name}}";
			folderLink.InnerHtml += "{{/if}}";
			folderNameCell.InnerHtml += folderLink.ToString();
			folderRow.InnerHtml += folderNameCell.ToString();
			folderRow.InnerHtml += "{{#if IsParent}}";
			folderRow.InnerHtml += new TagBuilder("td").ToString();
			folderRow.InnerHtml += "{{else}}";
			var folderModifiedCell = new TagBuilder("td");
			folderModifiedCell.AddCssClass("postedon");
			var folderModifiedAbbr = new TagBuilder("abbr");
			folderModifiedAbbr.AddCssClass("timeago");
			folderModifiedAbbr.MergeAttribute("title", "{{ModifiedOnDisplay}}");
			folderModifiedAbbr.MergeAttribute("data-datetime", "{{ModifiedOnDisplay}}");
			folderModifiedAbbr.InnerHtml = "{{ModifiedOnDisplay}}";
			folderModifiedCell.InnerHtml = folderModifiedAbbr.ToString();
			folderRow.InnerHtml += folderModifiedCell.ToString();
			folderRow.InnerHtml += "{{/if}}";
			if (deleteEnabled)
			{
				folderRow.InnerHtml += ActionCell(toolbarButtonLabel, deleteButtonLabel).ToString();
			}
			table.InnerHtml += folderRow.ToString();
			table.InnerHtml += "{{else}}";
			var fileRow = new TagBuilder("tr");
			fileRow.AddCssClass("sp-item");
			fileRow.MergeAttribute("data-id", "{{Id}}");
			fileRow.MergeAttribute("data-filename", "{{Name}}");
			fileRow.MergeAttribute("data-url", "{{Url}}");
			var fileNameCell = new TagBuilder("td");
			fileNameCell.MergeAttribute("data-type", "System.String");
			fileNameCell.MergeAttribute("data-value", "{{Name}}");
			var fileLink = new TagBuilder("a");
			fileLink.MergeAttribute("target", "_blank");
			fileLink.MergeAttribute("href", "{{Url}}");
			fileLink.InnerHtml = "<span class='fa fa-file-o' aria-hidden='true'></span> {{Name}} <small>({{FileSizeDisplay}})</small>";
			fileNameCell.InnerHtml += fileLink.ToString();
			fileRow.InnerHtml += fileNameCell.ToString();
			var fileModifiedCell = new TagBuilder("td");
			fileModifiedCell.AddCssClass("postedon");
			var fileModifiedAbbr = new TagBuilder("abbr");
			fileModifiedAbbr.AddCssClass("timeago");
			fileModifiedAbbr.MergeAttribute("title", "{{ModifiedOnDisplay}}");
			fileModifiedAbbr.MergeAttribute("data-datetime", "{{ModifiedOnDisplay}}");
			fileModifiedAbbr.InnerHtml = "{{ModifiedOnDisplay}}";
			fileModifiedCell.InnerHtml = fileModifiedAbbr.ToString();
			fileRow.InnerHtml += fileModifiedCell.ToString();
			if (deleteEnabled)
			{
				fileRow.InnerHtml += ActionCell(toolbarButtonLabel, deleteButtonLabel).ToString();
			}
			table.InnerHtml += fileRow.ToString();
			table.InnerHtml += "{{/if}}";
			table.InnerHtml += "{{/each}}";
			grid.InnerHtml += table.ToString();
			template.InnerHtml = grid.GetHTML();
			container.InnerHtml += template.ToString();

			var actionRow = new TagBuilder("div");
			actionRow.AddCssClass("view-toolbar grid-actions clearfix");
			if (createEnabled)
			{
				if (!string.IsNullOrWhiteSpace(serviceUrlAddFolder))
				{
					var button = new TagBuilder("a");
					button.AddCssClass("btn btn-info pull-right action");
					button.AddCssClass("add-folder");
					button.MergeAttribute("title", addFolderButtonLabel.GetValueOrDefault(DefaultAddFolderButtonLabel));
					button.InnerHtml = addFolderButtonLabel.GetValueOrDefault(DefaultAddFolderButtonLabel);
					actionRow.InnerHtml += button.ToString();
				}

				if (!string.IsNullOrWhiteSpace(serviceUrlAddFiles))
				{
					var button = new TagBuilder("a");
					button.AddCssClass("btn btn-primary pull-right action");
					button.AddCssClass("add-file");
					button.MergeAttribute("title", addFilesButtonLabel.GetValueOrDefault(DefaultAddFilesButtonLabel));
					button.InnerHtml = addFilesButtonLabel.GetValueOrDefault(DefaultAddFilesButtonLabel);
					actionRow.InnerHtml += button.ToString();
				}
			}
			container.InnerHtml += actionRow.ToString();

			var sharePointBreadcrumbs = new TagBuilder("ol");
			sharePointBreadcrumbs.AddCssClass("sharepoint-breadcrumbs");
			sharePointBreadcrumbs.AddCssClass("breadcrumb");
			sharePointBreadcrumbs.MergeAttribute("style", "display: none;");
			container.InnerHtml += sharePointBreadcrumbs.ToString();

			var sharePointData = new TagBuilder("div");
			sharePointData.AddCssClass("sharepoint-data");
			container.InnerHtml += sharePointData.ToString();

			var messageEmpty = new TagBuilder("div");
			messageEmpty.AddCssClass("sharepoint-empty message");
			messageEmpty.MergeAttribute("style", "display: none;");
			if (!string.IsNullOrWhiteSpace(emptyMessage))
			{
				messageEmpty.InnerHtml = emptyMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-warning");
				message.InnerHtml = DefaultSharePointGridEmptyMessage;
				messageEmpty.InnerHtml = message.ToString();
			}
			container.InnerHtml += messageEmpty.ToString();

			var messageAccessDenied = new TagBuilder("div");
			messageAccessDenied.AddCssClass("sharepoint-access-denied message");
			messageAccessDenied.MergeAttribute("style", "display: none;");
			if (!string.IsNullOrWhiteSpace(accessDeniedMessage))
			{
				messageAccessDenied.InnerHtml = accessDeniedMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = DefaultSharePointGridAccessDeniedMessage;
				messageAccessDenied.InnerHtml = message.ToString();
			}
			container.InnerHtml += messageAccessDenied.ToString();

			var messageError = new TagBuilder("div");
			messageError.AddCssClass("sharepoint-error message");
			messageError.MergeAttribute("style", "display: none;");
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				messageError.InnerHtml = errorMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = DefaultSharePointGridErrorMessage;
				messageError.InnerHtml = message.ToString();
			}
			container.InnerHtml += messageError.ToString();

			var messageLoading = new TagBuilder("div");
			messageLoading.AddCssClass("sharepoint-loading message text-center");
			messageLoading.InnerHtml = !string.IsNullOrWhiteSpace(loadingMessage)
				? loadingMessage
				: DefaultSharePointGridLoadingMessage;
			container.InnerHtml += messageLoading.ToString();

			var pagination = new TagBuilder("div");
			pagination.AddCssClass("sharepoint-pagination");
			pagination.MergeAttribute("data-pages", "1");
			pagination.MergeAttribute("data-pagesize", pageSize.ToString(CultureInfo.InvariantCulture));
			pagination.MergeAttribute("data-current-page", "1");
			container.InnerHtml += pagination;

			if (createEnabled)
			{
				if (!string.IsNullOrWhiteSpace(serviceUrlAddFiles))
				{
					var addFileModal = AddFilesModal(html, target, serviceUrlAddFiles, modalAddFilesSize, modalAddFilesCssClass, modalAddFilesTitle,
						modalAddFilesDismissButtonSrText, modalAddFilesPrimaryButtonText, modalAddFilesCancelButtonText, modalAddFilesTitleCssClass,
						modalAddFilesPrimaryButtonCssClass, modalAddFilesCancelButtonCssClass, modalAddFilesAttachFileLabel, modalAddFilesAttachFileAccept,
						modalAddFilesDisplayOverwriteField, modalAddFilesOverwriteFieldLabel, modalAddFilesOverwriteFieldDefaultValue, modalAddFilesDestinationFolderLabel,
						modalAddFilesFormLeftColumnCssClass, modalAddFilesFormRightColumnCssClass, modalAddFilesHtmlAttributes);

					container.InnerHtml += addFileModal;
				}

				if (!string.IsNullOrWhiteSpace(serviceUrlAddFolder))
				{
					var addFolderModal = AddFolderModal(html, target, serviceUrlAddFolder, modalAddFolderSize, modalAddFolderCssClass, modalAddFolderTitle,
						modalAddFolderDismissButtonSrText, modalAddFolderPrimaryButtonText, modalAddFolderCancelButtonText, modalAddFolderTitleCssClass,
						modalAddFolderPrimaryButtonCssClass, modalAddFolderCancelButtonCssClass, modalAddFolderNameLabel, modalAddFolderDestinationFolderLabel,
						modalAddFolderFormLeftColumnCssClass, modalAddFolderFormRightColumnCssClass, modalAddFolderHtmlAttributes);

					container.InnerHtml += addFolderModal;
				}
			}

			if (deleteEnabled && !string.IsNullOrWhiteSpace(serviceUrlDelete))
			{
				var deleteFileModal = html.DeleteModal(modalDeleteFileSize, string.Join(" ", new[] { "modal-delete-file", modalDeleteFileCssClass }).TrimEnd(' '),
					modalDeleteFileTitle.GetValueOrDefault(DefaultDeleteFileModalTitle),
					modalDeleteFileConfirmation.GetValueOrDefault(DefaultDeleteFileModalConfirmation),
					modalDeleteFileDismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText),
					modalDeleteFilePrimaryButtonText.GetValueOrDefault(DefaultDeleteModalPrimaryButtonText),
					modalDeleteFileCancelButtonText.GetValueOrDefault(DefaultDeleteModalCancelButtonText),
					modalDeleteFileTitleCssClass, modalDeleteFilePrimaryButtonCssClass, modalDeleteFileCancelButtonCssClass,
					modalDeleteFileHtmlAttributes);

				container.InnerHtml += deleteFileModal;

				var deleteFolderModal = html.DeleteModal(modalDeleteFolderSize, string.Join(" ", new[] { "modal-delete-folder", modalDeleteFolderCssClass }).TrimEnd(' '),
					modalDeleteFolderTitle.GetValueOrDefault(DefaultDeleteFolderModalTitle),
					modalDeleteFolderConfirmation.GetValueOrDefault(DefaultDeleteFolderModalConfirmation),
					modalDeleteFolderDismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText),
					modalDeleteFolderPrimaryButtonText.GetValueOrDefault(DefaultDeleteModalPrimaryButtonText),
					modalDeleteFolderCancelButtonText.GetValueOrDefault(DefaultDeleteModalCancelButtonText),
					modalDeleteFolderTitleCssClass, modalDeleteFolderPrimaryButtonCssClass, modalDeleteFolderCancelButtonCssClass,
					modalDeleteFolderHtmlAttributes);

				container.InnerHtml += deleteFolderModal;
			}

			return new HtmlString(container.ToString());
		}

		/// <summary>
		/// Render a bootstrap modal dialog for adding files.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceUrl">URL to the service that handles the add file request.</param>
		/// <param name="target">EntityReference of the target entity the file is regarding. <see cref="EntityReference"/></param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="attachFileLabel">Text displayed for the attach file label.</param>
		/// <param name="attachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="displayOverwriteField">Boolean value that indicates if the overwrite checkbox is displayed.</param>
		/// <param name="overwriteFieldLabel">Text displayed for the label of the overwrite existing file checkbox.</param>
		/// <param name="overwriteFieldDefaultValue">Default value assigned to the overwrite checkbox.</param>
		/// <param name="destinationFolderLabel">Text displayed for the destination folder label.</param>
		/// <param name="formLeftColumnCssClass">CSS class applied to the form's left column.</param>
		/// <param name="formRightColumnCssClass">CSS class applied to the form's right column.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString AddFilesModal(this HtmlHelper html, EntityReference target, string serviceUrl,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null,
			string closeButtonCssClass = null, string attachFileLabel = null, string attachFileAccept = null,
			bool displayOverwriteField = true, string overwriteFieldLabel = null, bool overwriteFieldDefaultValue = true, string destinationFolderLabel = null,
			string formLeftColumnCssClass = null, string formRightColumnCssClass = null, IDictionary<string, string> htmlAttributes = null)
		{
			var body = new TagBuilder("div");
			body.AddCssClass("add-file");
			body.AddCssClass("form-horizontal");
			body.MergeAttribute("data-url", serviceUrl);
			var entityReference = new JObject(new JProperty("LogicalName", target.LogicalName),
				new JProperty("Id", target.Id.ToString()));
			body.MergeAttribute("data-target", entityReference.ToString());

			var row1 = new TagBuilder("div");
			row1.AddCssClass("form-group");
			var fileInputLabel = new TagBuilder("label");
			fileInputLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddModalFormLeftColumnCssClass));
			fileInputLabel.AddCssClass("control-label");
			fileInputLabel.InnerHtml = attachFileLabel.GetValueOrDefault(DefaultAddFilesModalAttachFileLabel);
			var fileInputContainer = new TagBuilder("div");
			fileInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddModalFormRightColumnCssClass));
			var fileInput = new TagBuilder("input");
			fileInput.MergeAttribute("name", "file");
			fileInput.MergeAttribute("type", "file");
			fileInput.MergeAttribute("multiple", "multiple");
			if (!string.IsNullOrWhiteSpace(attachFileAccept))
			{
				fileInput.MergeAttribute("accept", attachFileAccept);
			}
			fileInputContainer.InnerHtml = fileInput.ToString();
			row1.InnerHtml = fileInputLabel.ToString();
			row1.InnerHtml += fileInputContainer.ToString();
			body.InnerHtml += row1.ToString();

			if (displayOverwriteField)
			{
				var row2 = new TagBuilder("div");
				row2.AddCssClass("form-group");
				var emptyLeft = new TagBuilder("div");
				emptyLeft.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddModalFormLeftColumnCssClass));
				row2.InnerHtml = emptyLeft.ToString();
				var overwriteContainer = new TagBuilder("div");
				overwriteContainer.AddCssClass("checkbox");
				overwriteContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddModalFormRightColumnCssClass));
				var overwriteLabel = new TagBuilder("label");
				var overwrite = new TagBuilder("input");
				overwrite.MergeAttribute("name", "overwrite");
				overwrite.MergeAttribute("type", "checkbox");
				if (overwriteFieldDefaultValue)
				{
					overwrite.MergeAttribute("checked", "checked");
				}
				overwriteLabel.InnerHtml = overwrite.ToString();
				overwriteLabel.InnerHtml += overwriteFieldLabel.GetValueOrDefault(DefaultAddFilesModalOverwriteFieldLabel);
				overwriteContainer.InnerHtml = overwriteLabel.ToString();
				row2.InnerHtml += overwriteContainer.ToString();
				body.InnerHtml += row2.ToString();
			}

			var row3 = new TagBuilder("div");
			row3.AddCssClass("form-group");
			row3.AddCssClass("destination-group");
			var destinationLabel = new TagBuilder("label");
			destinationLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddModalFormLeftColumnCssClass));
			destinationLabel.AddCssClass("control-label");
			destinationLabel.InnerHtml = destinationFolderLabel.GetValueOrDefault(DefaultAddModalDestinationLabel);
			var destinationContainer = new TagBuilder("div");
			destinationContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddModalFormRightColumnCssClass));
			var destination = new TagBuilder("p");
			destination.AddCssClass("destination-folder");
			destination.AddCssClass("form-control-static");
			destinationContainer.InnerHtml = destination.ToString();
			row3.InnerHtml = destinationLabel.ToString();
			row3.InnerHtml += destinationContainer.ToString();
			body.InnerHtml += row3.ToString();

			return html.BoostrapModal(BootstrapExtensions.BootstrapModalSize.Default,
				title.GetValueOrDefault(DefaultAddFilesModalTitle), body.ToString(),
				string.Join(" ", new[] { "modal-add-file", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(DefaultAddFilesModalPrimaryButtonText),
				cancelButtonText.GetValueOrDefault(DefaultAddModalCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		/// <summary>
		/// Render a bootstrap modal dialog for adding a folder.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceUrl">URL to the service that handles the add folder request.</param>
		/// <param name="target">EntityReference of the target entity the folder is regarding. <see cref="EntityReference"/></param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="nameLabel">Text displayed for the folder name label.</param>
		/// <param name="destinationFolderLabel">Text displayed for the destination folder label.</param>
		/// <param name="formLeftColumnCssClass">CSS class applied to the form's left column.</param>
		/// <param name="formRightColumnCssClass">CSS class applied to the form's right column.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString AddFolderModal(this HtmlHelper html, EntityReference target, string serviceUrl,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null,
			string closeButtonCssClass = null, string nameLabel = null, string destinationFolderLabel = null,
			string formLeftColumnCssClass = null, string formRightColumnCssClass = null, IDictionary<string, string> htmlAttributes = null)
		{
			var body = new TagBuilder("div");
			body.AddCssClass("add-folder");
			body.AddCssClass("form-horizontal");
			body.MergeAttribute("data-url", serviceUrl);
			var entityReference = new JObject(new JProperty("LogicalName", target.LogicalName),
				new JProperty("Id", target.Id.ToString()));
			body.MergeAttribute("data-target", entityReference.ToString());

			var row1 = new TagBuilder("div");
			row1.AddCssClass("form-group");
			var nameFieldLabel = new TagBuilder("label");
			nameFieldLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddModalFormLeftColumnCssClass));
			nameFieldLabel.AddCssClass("control-label");
			nameFieldLabel.MergeAttribute("for", "FolderName");
			nameFieldLabel.InnerHtml = nameLabel.GetValueOrDefault(DefaultAddFolderModalNameLabel);
			var nameInputContainer = new TagBuilder("div");
			nameInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddModalFormRightColumnCssClass));
			var nameInput = new TagBuilder("input");
			nameInput.AddCssClass("form-control");
			nameInput.MergeAttribute("id", "FolderName");
			nameInput.MergeAttribute("type", "text");
			nameInput.MergeAttribute("placeholder", nameLabel.GetValueOrDefault(DefaultAddFolderModalNameLabel));
			nameInputContainer.InnerHtml = nameInput.ToString();
			row1.InnerHtml = nameFieldLabel.ToString();
			row1.InnerHtml += nameInputContainer.ToString();
			body.InnerHtml += row1.ToString();

			var row2 = new TagBuilder("div");
			row2.AddCssClass("form-group");
			row2.AddCssClass("destination-group");
			var destinationLabel = new TagBuilder("label");
			destinationLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddModalFormLeftColumnCssClass));
			destinationLabel.AddCssClass("control-label");
			destinationLabel.InnerHtml = destinationFolderLabel.GetValueOrDefault(DefaultAddModalDestinationLabel);
			var destinationContainer = new TagBuilder("div");
			destinationContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddModalFormRightColumnCssClass));
			var destination = new TagBuilder("p");
			destination.AddCssClass("destination-folder");
			destination.AddCssClass("form-control-static");
			destinationContainer.InnerHtml = destination.ToString();
			row2.InnerHtml = destinationLabel.ToString();
			row2.InnerHtml += destinationContainer.ToString();
			body.InnerHtml += row2.ToString();

			return html.BoostrapModal(BootstrapExtensions.BootstrapModalSize.Default,
				title.GetValueOrDefault(DefaultAddFolderModalTitle), body.ToString(),
				string.Join(" ", new[] { "modal-add-folder", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(DefaultAddFolderModalPrimaryButtonText),
				cancelButtonText.GetValueOrDefault(DefaultAddModalCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		private static TagBuilder ActionCell(string toolbarButtonLabel, string deleteButtonLabel)
		{
			var actionCell = new TagBuilder("td");
			actionCell.InnerHtml += "{{#unless IsParent}}";
			var dropdown = new TagBuilder("div");
			dropdown.AddCssClass("toolbar {{#if @last}} dropup {{else}} dropdown {{/if}} pull-right");
			var dropdownLink = new TagBuilder("a");
			dropdownLink.AddCssClass("btn btn-default btn-xs");
			dropdownLink.MergeAttribute("href", "#");
			dropdownLink.MergeAttribute("data-toggle", "dropdown");
			dropdownLink.InnerHtml += toolbarButtonLabel.GetValueOrDefault(DefaultToolbarButtonLabel);
			dropdown.InnerHtml += dropdownLink.ToString();
			var dropdownMenu = new TagBuilder("ul");
			dropdownMenu.AddCssClass("dropdown-menu");
			dropdownMenu.MergeAttribute("role", "menu");
			var deleteItem = new TagBuilder("li");
			deleteItem.MergeAttribute("role", "presentation");
			var deleteLink = new TagBuilder("a");
			deleteLink.AddCssClass("delete-link");
			deleteLink.MergeAttribute("role", "menuitem");
			deleteLink.MergeAttribute("tabindex", "-1");
			deleteLink.MergeAttribute("href", "#");
			deleteLink.InnerHtml = deleteButtonLabel.GetValueOrDefault(DefaultDeleteButtonLabel);
			deleteItem.InnerHtml = deleteLink.ToString();
			dropdownMenu.InnerHtml += deleteItem.ToString();
			dropdown.InnerHtml += dropdownMenu.ToString();
			actionCell.InnerHtml += dropdown.ToString();
			actionCell.InnerHtml += "{{/unless}}";
			return actionCell;
		}
	}
	public static class TagBuilderExtensions
	{
		public static string GetHTML(this TagBuilder tb)
		{
			return tb.ToString().Replace("&#32;", " "); // Fix error in handlebars template
		}
	}
}
