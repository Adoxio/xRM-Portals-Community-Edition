/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering a entity annotations (notes) within Adxstudio Portals applications.
	/// </summary>
	/// <remarks>Requires Bootstrap 3, jQuery and the following files: ~/js/jquery.bootstrap-pagination.js, ~/js/entity-notes.js</remarks>
	public static class EntityNotesExtensions
	{
		private static readonly string DefaultAddNoteModalTitle = ResourceManager.GetString("Add_Note");
		private static readonly string DefaultAddCommentModalTitle = ResourceManager.GetString("Timeline_Add_A_Comment");
		private static readonly string DefaultAddNoteModalPrimaryButtonText = " " + ResourceManager.GetString("Add_Note");
		private static readonly string DefaultAddCommentModalPrimaryButtonText = " " + ResourceManager.GetString("Submit");
		private static readonly string DefaultAddNoteModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultAddCommentModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultAddNoteModalNoteFieldLabel = ResourceManager.GetString("Note_DefaultText");
		private static readonly string DefaultAddCommentModalNoteFieldLabel = ResourceManager.GetString("Comment_DefaultText");
		private static readonly string DefaultAddNoteModalPrivacyOptionFieldLabel = ResourceManager.GetString("Is_Private_Field_Label_Text");
		private static readonly string DefaultAddNoteModalAttachFileLabel = ResourceManager.GetString("Attach_A_File_DefaultText");
		private static readonly string DefaultAddCommentModalAttachFileLabel = ResourceManager.GetString("Timeline_Attach_A_File_DefaultText");
		private const string DefaultAddNoteModalFormLeftColumnCssClass = "col-sm-3";
		private const string DefaultAddNoteModalFormRightColumnCssClass = "col-sm-9";
		private static readonly string DefaultEditNoteModalTitle = ResourceManager.GetString("Default_Edit_Note_ModalTitle");
		private static readonly string DefaultEditNoteModalPrimaryButtonText = ResourceManager.GetString("Update_Note_Button_Text");
		private static readonly string DefaultEditNoteModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultEditNoteModalNoteFieldLabel = ResourceManager.GetString("Note_DefaultText");
        private static readonly string DefaultTimelineModifiedOnFieldLabel = ResourceManager.GetString("Timeline_Modified_On_Message");
        private static readonly string DefaultTimelineCreatedByFieldLabel = ResourceManager.GetString("Timeline_Created_By_Message");
        private static readonly string DefaultEditNoteModalPrivacyOptionFieldLabel = ResourceManager.GetString("Is_Private_Field_Label_Text");
		private static readonly string DefaultEditNoteModalAttachFileLabel = ResourceManager.GetString("Attach_A_File_DefaultText");
		private const string DefaultEditNoteModalFormLeftColumnCssClass = "col-sm-3";
		private const string DefaultEditNoteModalFormRightColumnCssClass = "col-sm-9";
		private static readonly string DefaultDeleteNoteModalTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Delete_Note_ModalTitle");
		private static readonly string DefaultDeleteNoteModalConfirmation = ResourceManager.GetString("Node_Deletion_Confirmation_Message");
		private static readonly string DefaultDeleteNoteModalPrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultDeleteNoteModalCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static readonly string DefaultNotesListTitle = ResourceManager.GetString("Notes_DefaultText");
        private static readonly string DefaultTimelineListTitle = ResourceManager.GetString("Timeline_DefaultText");
        private static readonly string DefaultNotesListLoadingMessage = "<span class='fa fa-spinner fa-spin' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Grid_Loading_Message");
		private static readonly string DefaultNotesListErrorMessage = ResourceManager.GetString("Error_Completing_Request_Error_Message") + "<span class='details'></span>";
		private static readonly string DefaultNotesListAccessDeniedMessage = ResourceManager.GetString("Access_Denied_No_Permissions_To_View_Notes_Message");
		private static readonly string DefaultNotesListEmptyMessage = ResourceManager.GetString("Default_Notes_List_Empty_Message");
		private static readonly string DefaultActivitiesListEmptyMessage = ResourceManager.GetString("Default_Activities_List_Empty_Message");
		private static readonly string DefaultAddNoteButtonLabel = "<span class='fa fa-plus-circle' aria-hidden='true'></span> " + ResourceManager.GetString("Add_Note");
		private static readonly string DefaultAddCommentButtonLabel = "<span class='fa fa-plus-circle' aria-hidden='true'></span> " + ResourceManager.GetString("Add_Comment");
		private static readonly string AddCommentButtonLabel = ResourceManager.GetString("Add_Comment");
		private static readonly string DefaultLoadMoreButtonLabel = "<span class='fa fa-plus-circle' aria-hidden='true'></span> " + ResourceManager.GetString("Load_More_Timeline");
		private const string DefaultToolbarButtonLabel = "<span class='fa fa-caret-down fa-fw' aria-hidden='true'></span>";
		private static readonly string DefaultEditNoteButtonLabel = "<span class='fa fa-edit fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Edit_Label");
		private static readonly string DefaultDeleteNoteButtonLabel = "<span class='fa fa-trash-o fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultNotePrivacyLabel = "<span class='fa fa-eye-slash' aria-hidden='true'></span> " + ResourceManager.GetString("Private_Label_Text");
		private const int DefaultNoteFieldColumns = 20;
		private const int DefaultNoteFieldRows = 9;

		/// <summary>
		/// Render a list of notes associated with a target entity and provide a modal dialog to add new notes.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="target">EntityReference for the target entity record <see cref="EntityReference"/></param>
		/// <param name="serviceUrlGet">URL to the service that handles the get data request.</param>
		/// <param name="addNotesEnabled">Boolean indicating whether adding notes is enabled or not.</param>
		/// <param name="editNotesEnabled">Boolean indicating whether adding notes is enabled or not.</param>
		/// <param name="deleteNotesEnabled">Boolean indicating whether adding notes is enabled or not.</param>
		/// <param name="serviceUrlAdd">URL to the service that handles the add note request.</param>
		/// <param name="serviceUrlEdit">URL to the service that handles the edit note request.</param>
		/// <param name="serviceUrlDelete">URL to the service that handles the delete note request.</param>
		/// <param name="pageSize">Number of records to display per page.</param>
		/// <param name="orders">Orderby expression. <see cref="Order"/></param>
		/// <param name="title">Title of the note list</param>
		/// <param name="loadingMessage">Message to be displayed during loading of note records.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs loading note records.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the note records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no note records found.</param>
		/// <param name="addNoteButtonLabel">Text displayed for the button that launches the add note modal.</param>
		/// <param name="toolbarButtonLabel">Text displayed for the toolbar button dropdown.</param>
		/// <param name="editNoteButtonLabel">Text displayed for the edit button.</param>
		/// <param name="deleteNoteButtonLabel">Text displayed for the delete button.</param>
		/// <param name="notePrivacyLabel">Label displayed for notes that are private.</param>
		/// <param name="modalAddNoteSize">Size of the add note modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalAddNoteCssClass">CSS class assigned to the add note modal element.</param>
		/// <param name="modalAddNoteTitle">Text assigned to the add note modal title.</param>
		/// <param name="modalAddNoteDismissButtonSrText">Text assigned to the add note modal dismiss button for screen readers only.</param>
		/// <param name="modalAddNotePrimaryButtonText">Text assigned to the add note modal primary button.</param>
		/// <param name="modalAddNoteCancelButtonText">Text assigned to the add note modal cancel button.</param>
		/// <param name="modalAddNoteTitleCssClass">CSS class assigned to the add note title.</param>
		/// <param name="modalAddNotePrimaryButtonCssClass">CSS class assigned to the add note modal primary button.</param>
		/// <param name="modalAddNoteCancelButtonCssClass">CSS class assigned to the add note modal cancel button.</param>
		/// <param name="modalAddNoteNoteFieldLabel">Text displayed for the label of the add note modal note text field.</param>
		/// <param name="modalAddNoteDisplayAttachFile">Boolean value that indicates if the add note modal should display a file input.</param>
		/// <param name="modalAddNoteAttachFileLabel">Text displayed for the label of the add note modal file input.</param>
		/// <param name="modalAddNoteAttachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="modalAddNoteDisplayPrivacyOptionField">Boolean value that indicates if the add note modal should display a privacy (is public) checkbox.</param>
		/// <param name="modalAddNotePrivacyOptionFieldLabel">Text displayed for the label of the add note modal privacy (is public) checkbox</param>
		/// <param name="modalAddNoteFormRightColumnCssClass">CSS class assigned to the add note modal form's left column.</param>
		/// <param name="modalAddNoteFormLeftColumnCssClass">CSS class assigned to the add note modal form's right column.</param>
		/// <param name="modalAddNotePrivacyOptionFieldDefaultValue">Default value assigned to the add note modal privacy (is public) checkbox</param>
		/// <param name="modalAddNoteNoteFieldColumns">Number of columns assigned to the add note modal note text field textarea.</param>
		/// <param name="modalAddNoteNoteFieldRows">Number of rows assigned to the add note modal note text field textarea.</param>
		/// <param name="modalAddNoteHtmlAttributes">Collection of HTML attributes to be assigned to the add note modal element.</param>
		/// <param name="modalEditNoteSize">Size of the edit note modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalEditNoteCssClass">CSS class assigned to the edit note modal element.</param>
		/// <param name="modalEditNoteTitle">Text assigned to the edit note modal title.</param>
		/// <param name="modalEditNoteDismissButtonSrText">Text assigned to the edit note modal dismiss button for screen readers only.</param>
		/// <param name="modalEditNotePrimaryButtonText">Text assigned to the edit note modal primary button.</param>
		/// <param name="modalEditNoteCancelButtonText">Text assigned to the edit note modal cancel button.</param>
		/// <param name="modalEditNoteTitleCssClass">CSS class assigned to the edit note title.</param>
		/// <param name="modalEditNotePrimaryButtonCssClass">CSS class assigned to the edit note modal primary button.</param>
		/// <param name="modalEditNoteCancelButtonCssClass">CSS class assigned to the edit note modal cancel button.</param>
		/// <param name="modalEditNoteNoteFieldLabel">Text displayed for the label of the edit note modal note text field.</param>
		/// <param name="modalEditNoteDisplayAttachFile">Boolean value that indicates if the edit note modal should display a file input.</param>
		/// <param name="modalEditNoteAttachFileLabel">Text displayed for the label of the edit note modal file input.</param>
		/// <param name="modalEditNoteAttachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="modalEditNoteDisplayPrivacyOptionField">Boolean value that indicates if the edit note modal should display a privacy (is public) checkbox.</param>
		/// <param name="modalEditNotePrivacyOptionFieldLabel">Text displayed for the label of the edit note modal privacy (is public) checkbox</param>
		/// <param name="modalEditNoteFormRightColumnCssClass">CSS class assigned to the edit note modal form's left column.</param>
		/// <param name="modalEditNoteFormLeftColumnCssClass">CSS class assigned to the edit note modal form's right column.</param>
		/// <param name="modalEditNotePrivacyOptionFieldDefaultValue">Default value assigned to the edit note modal privacy (is public) checkbox</param>
		/// <param name="modalEditNoteNoteFieldColumns">Number of columns assigned to the edit note modal note text field textarea.</param>
		/// <param name="modalEditNoteNoteFieldRows">Number of rows assigned to the edit note modal note text field textarea.</param>
		/// <param name="modalEditNoteHtmlAttributes">Collection of HTML attributes to be assigned to the edit note modal element.</param>
		/// <param name="modalDeleteNoteSize">Size of the delete note modal <see cref="BootstrapExtensions.BootstrapModalSize"/>.</param>
		/// <param name="modalDeleteNoteCssClass">CSS class assigned to the delete note modal element.</param>
		/// <param name="modalDeleteNoteTitle">Text assigned to the delete note modal title.</param>
		/// <param name="modalDeleteNoteConfirmation">Text displayed for the confirmation message of the delete note modal.</param>
		/// <param name="modalDeleteNoteDismissButtonSrText">Text assigned to the delete note modal dismiss button for screen readers only.</param>
		/// <param name="modalDeleteNotePrimaryButtonText">Text assigned to the delete note modal primary button.</param>
		/// <param name="modalDeleteNoteCancelButtonText">Text assigned to the delete note modal cancel button.</param>
		/// <param name="modalDeleteNoteTitleCssClass">CSS class assigned to the delete note title.</param>
		/// <param name="modalDeleteNotePrimaryButtonCssClass">CSS class assigned to the delete note modal primary button.</param>
		/// <param name="modalDeleteNoteCancelButtonCssClass">CSS class assigned to the delete note modal cancel button.</param>
		/// <param name="modalDeleteNoteHtmlAttributes">Collection of HTML attributes to be assigned to the delete note modal element.</param>
		public static IHtmlString Notes(this HtmlHelper html, EntityReference target, string serviceUrlGet, bool addNotesEnabled = false, bool editNotesEnabled = false, bool deleteNotesEnabled = false, string serviceUrlAdd = null, string serviceUrlEdit = null, string serviceUrlDelete = null, string serviceGetAttachmentsUrl = null, int pageSize = 0, List<Order> orders = null, string title = null,
			string loadingMessage = null, string errorMessage = null, string accessDeniedMessage = null,
			string emptyMessage = null, string addNoteButtonLabel = null, string loadMoreButtonLabel = null, string toolbarButtonLabel = null, string editNoteButtonLabel = null, string deleteNoteButtonLabel = null, string notePrivacyLabel = null,
			BootstrapExtensions.BootstrapModalSize modalAddNoteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalAddNoteCssClass = null, string modalAddNoteTitle = null, string modalAddNoteDismissButtonSrText = null,
			string modalAddNotePrimaryButtonText = null, string modalAddNoteCancelButtonText = null, string modalAddNoteTitleCssClass = null,
			string modalAddNotePrimaryButtonCssClass = null, string modalAddNoteCancelButtonCssClass = null, string modalAddNoteNoteFieldLabel = null,
			bool modalAddNoteDisplayAttachFile = true, string modalAddNoteAttachFileLabel = null, string modalAddNoteAttachFileAccept = null,
			bool modalAddNoteDisplayPrivacyOptionField = false, string modalAddNotePrivacyOptionFieldLabel = null,
			bool modalAddNotePrivacyOptionFieldDefaultValue = true, int? modalAddNoteNoteFieldColumns = 20, int? modalAddNoteNoteFieldRows = 6,
			string modalAddNoteFormLeftColumnCssClass = null, string modalAddNoteFormRightColumnCssClass = null,
			IDictionary<string, string> modalAddNoteHtmlAttributes = null,
			BootstrapExtensions.BootstrapModalSize modalEditNoteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalEditNoteCssClass = null, string modalEditNoteTitle = null, string modalEditNoteDismissButtonSrText = null,
			string modalEditNotePrimaryButtonText = null, string modalEditNoteCancelButtonText = null, string modalEditNoteTitleCssClass = null,
			string modalEditNotePrimaryButtonCssClass = null, string modalEditNoteCancelButtonCssClass = null, string modalEditNoteNoteFieldLabel = null,
			bool modalEditNoteDisplayAttachFile = true, string modalEditNoteAttachFileLabel = null, string modalEditNoteAttachFileAccept = null,
			bool modalEditNoteDisplayPrivacyOptionField = false, string modalEditNotePrivacyOptionFieldLabel = null,
			bool modalEditNotePrivacyOptionFieldDefaultValue = true, int? modalEditNoteNoteFieldColumns = 20, int? modalEditNoteNoteFieldRows = 6,
			string modalEditNoteFormLeftColumnCssClass = null, string modalEditNoteFormRightColumnCssClass = null,
			IDictionary<string, string> modalEditNoteHtmlAttributes = null, BootstrapExtensions.BootstrapModalSize modalDeleteNoteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteNoteCssClass = null, string modalDeleteNoteTitle = null, string modalDeleteNoteConfirmation = null, string modalDeleteNoteDismissButtonSrText = null,
			string modalDeleteNotePrimaryButtonText = null, string modalDeleteNoteCancelButtonText = null, string modalDeleteNoteTitleCssClass = null,
			string modalDeleteNotePrimaryButtonCssClass = null, string modalDeleteNoteCancelButtonCssClass = null, IDictionary<string, string> modalDeleteNoteHtmlAttributes = null,
			bool isTimeline = true, bool useScrollingPagination = true, AnnotationSettings attachmentSettings = null, int? textMaxLength = null)
		{
			var container = new TagBuilder("div");
			if (isTimeline)
			{
				container.AddCssClass("col-md-12 entity-timeline");
			}
			else
			{
				container.AddCssClass("col-md-8 entity-notes");
			}

			string fileAcceptString = null;
			if (attachmentSettings != null)
			{
				var json = JsonConvert.SerializeObject(attachmentSettings);
				container.MergeAttribute("data-attachmentsettings", Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(json), "Secure Notes Configuration").ToArray()));

				fileAcceptString = string.Format("{0},{1}", attachmentSettings.AcceptMimeTypes, attachmentSettings.AcceptExtensionTypes);
				container.MergeAttribute("data-add-accept-types", fileAcceptString);
			}
			container.MergeAttribute("data-url-get", serviceUrlGet);
			container.MergeAttribute("data-url-add", serviceUrlAdd);
			container.MergeAttribute("data-url-edit", serviceUrlEdit);
			container.MergeAttribute("data-url-delete", serviceUrlDelete);
			container.MergeAttribute("data-url-get-attachments", serviceGetAttachmentsUrl);
			container.MergeAttribute("data-add-enabled", addNotesEnabled.ToString());
			container.MergeAttribute("data-edit-enabled", editNotesEnabled.ToString());
			container.MergeAttribute("data-delete-enabled", deleteNotesEnabled.ToString());
			container.MergeAttribute("data-use-scrolling-pagination", useScrollingPagination.ToString());
			container.MergeAttribute("data-hide-field-label", isTimeline.ToString());

			if (orders == null || !orders.Any())
			{
				orders = new List<Order> { new Order("createdon") };
			}
			container.MergeAttribute("data-orders", JsonConvert.SerializeObject(orders));
			container.MergeAttribute("data-target", JsonConvert.SerializeObject(target));
			container.MergeAttribute("data-pagesize", pageSize.ToString(CultureInfo.InvariantCulture));
			

			if (!string.IsNullOrWhiteSpace(title))
			{
				var header = new TagBuilder("div");
				header.AddCssClass("page-header");
				header.AddCssClass("col-sm-9");
				header.InnerHtml = (new TagBuilder("h3") { InnerHtml = title.GetValueOrDefault(DefaultNotesListTitle) }).ToString();
				container.InnerHtml += header.ToString();
			}

			if (isTimeline && !string.IsNullOrWhiteSpace(serviceUrlAdd))
			{
				var timelineHeader = new TagBuilder("div");
				timelineHeader.AddCssClass("timelineheader");
				timelineHeader.AddCssClass("col-sm-12");

				var timelineTitle = new TagBuilder("div");
				timelineTitle.InnerHtml = (new TagBuilder("label") { InnerHtml = title.GetValueOrDefault(DefaultTimelineListTitle) }).ToString();
				timelineTitle.MergeAttribute("for", "timeline");
				timelineTitle.MergeAttribute("id", "timeline_label");
				timelineTitle.AddCssClass("col-sm-9 title");

				timelineHeader.InnerHtml += timelineTitle.ToString();

				if (addNotesEnabled)
				{
					var buttonContainer = new TagBuilder("div");
					buttonContainer.AddCssClass("col-sm-3 buttoncontainer");
					var button = new TagBuilder("a");

					button.Attributes.Add("tabindex", "0");
					button.Attributes.Add("href", "#");
					button.AddCssClass("btn btn-primary");
					button.Attributes.Add("role", "button");
					button.AddCssClass("addnote");
					button.InnerHtml = addNoteButtonLabel.GetValueOrDefault(DefaultAddCommentButtonLabel);
					buttonContainer.InnerHtml = button.ToString();
					timelineHeader.InnerHtml += buttonContainer.ToString();
				}

				container.InnerHtml += timelineHeader.ToString();
			}
			
			container.InnerHtml += (isTimeline) ? BuildTimelineTemplate() : 
				BuildNotesTemplate(notePrivacyLabel, editNotesEnabled, deleteNotesEnabled,
					toolbarButtonLabel, editNoteButtonLabel, deleteNoteButtonLabel);

			var messageEmpty = new TagBuilder("div");
			messageEmpty.AddCssClass("notes-empty message");
			if (!string.IsNullOrWhiteSpace(emptyMessage))
			{
				messageEmpty.InnerHtml = emptyMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-warning");
				message.InnerHtml = isTimeline ? DefaultActivitiesListEmptyMessage : DefaultNotesListEmptyMessage;
				messageEmpty.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageEmpty.ToString();

			var messageAccessDenied = new TagBuilder("div");
			messageAccessDenied.AddCssClass("notes-access-denied message");
			if (!string.IsNullOrWhiteSpace(accessDeniedMessage))
			{
				messageAccessDenied.InnerHtml = accessDeniedMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = DefaultNotesListAccessDeniedMessage;
				messageAccessDenied.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageAccessDenied.ToString();

			var messageError = new TagBuilder("div");
			messageError.AddCssClass("notes-error message");
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				messageError.InnerHtml = errorMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = DefaultNotesListErrorMessage;
				messageError.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageError.ToString();

			var messageLoading = new TagBuilder("div");
			messageLoading.AddCssClass("notes-loading message text-center");
			messageLoading.InnerHtml = !string.IsNullOrWhiteSpace(loadingMessage)
				? loadingMessage
				: DefaultNotesListLoadingMessage;

			container.InnerHtml += messageLoading.ToString();

			var notes = new TagBuilder("div");
			notes.AddCssClass("notes");
			notes.Attributes.Add("tabindex", "0");
			container.InnerHtml += notes.ToString();
			
			var messageLoadingMore = new TagBuilder("div");
			messageLoadingMore.AddCssClass("notes-loading-more message text-center");
			messageLoadingMore.InnerHtml = !string.IsNullOrWhiteSpace(loadingMessage)
				? loadingMessage
				: DefaultNotesListLoadingMessage;

			container.InnerHtml += messageLoadingMore.ToString();

			if (useScrollingPagination)
			{
				var loadMoreRow = new TagBuilder("div");
				loadMoreRow.AddCssClass("note-actions row");
				var buttonContainer = new TagBuilder("div");
				buttonContainer.AddCssClass("col-sm-7");
				buttonContainer.AddCssClass("center-block");
				var button = new TagBuilder("a");
				button.AddCssClass("btn btn-primary");
				button.AddCssClass("loadmore");
				button.InnerHtml = loadMoreButtonLabel.GetValueOrDefault(DefaultLoadMoreButtonLabel);
				buttonContainer.InnerHtml = button.ToString();

				loadMoreRow.InnerHtml += buttonContainer.ToString();
				container.InnerHtml += loadMoreRow.ToString();
			}
			else
			{
				var actionRow = new TagBuilder("div");
				actionRow.AddCssClass("note-actions row");
				if (addNotesEnabled && !string.IsNullOrWhiteSpace(serviceUrlAdd))
				{
					var buttonContainer = new TagBuilder("div");
					buttonContainer.AddCssClass("col-sm-3");
					var button = new TagBuilder("a");
					button.AddCssClass("btn btn-default");
					button.AddCssClass("addnote");
					button.InnerHtml = addNoteButtonLabel.GetValueOrDefault(DefaultAddNoteButtonLabel);
					buttonContainer.InnerHtml = button.ToString();
					actionRow.InnerHtml = buttonContainer.ToString();
				}

				var pagination = new TagBuilder("div");
				pagination.AddCssClass("notes-pagination col-sm-9");
				pagination.MergeAttribute("data-pages", "1");
				pagination.MergeAttribute("data-pagesize", pageSize.ToString(CultureInfo.InvariantCulture));
				pagination.MergeAttribute("data-current-page", "1");
				actionRow.InnerHtml += pagination.ToString();
				container.InnerHtml += actionRow.ToString();
			}
		
			if (addNotesEnabled && !string.IsNullOrWhiteSpace(serviceUrlAdd))
			{
				var addNoteModal = AddNoteModal(html, target, serviceUrlAdd, modalAddNoteSize, modalAddNoteCssClass, modalAddNoteTitle,
					modalAddNoteDismissButtonSrText,
					modalAddNotePrimaryButtonText, modalAddNoteCancelButtonText, modalAddNoteTitleCssClass, modalAddNotePrimaryButtonCssClass,
					modalAddNoteCancelButtonCssClass, modalAddNoteNoteFieldLabel, modalAddNoteDisplayAttachFile, modalAddNoteAttachFileLabel, fileAcceptString,
					modalAddNoteDisplayPrivacyOptionField, modalAddNotePrivacyOptionFieldLabel, modalAddNotePrivacyOptionFieldDefaultValue,
					modalAddNoteFormLeftColumnCssClass,
					modalAddNoteFormRightColumnCssClass, modalAddNoteNoteFieldColumns, modalAddNoteNoteFieldRows, modalAddNoteHtmlAttributes, isTimeline, textMaxLength);

				container.InnerHtml += addNoteModal;
			}
			
			if (editNotesEnabled && !string.IsNullOrWhiteSpace(serviceUrlEdit))
			{
				var editNoteModal = EditNoteModal(html, target, serviceUrlEdit, modalEditNoteSize, modalEditNoteCssClass,
					modalEditNoteTitle, modalEditNoteDismissButtonSrText, modalEditNotePrimaryButtonText, modalEditNoteCancelButtonText,
					modalEditNoteTitleCssClass, modalEditNotePrimaryButtonCssClass, modalEditNoteCancelButtonCssClass,
					modalEditNoteNoteFieldLabel, modalEditNoteDisplayAttachFile, modalEditNoteAttachFileLabel,
					modalEditNoteAttachFileAccept, modalEditNoteDisplayPrivacyOptionField, modalEditNotePrivacyOptionFieldLabel,
					modalEditNotePrivacyOptionFieldDefaultValue, modalEditNoteFormLeftColumnCssClass,
					modalEditNoteFormRightColumnCssClass, modalEditNoteNoteFieldColumns, modalEditNoteNoteFieldRows,
					modalEditNoteHtmlAttributes);

				container.InnerHtml += editNoteModal;
			}

			if (deleteNotesEnabled && !string.IsNullOrWhiteSpace(serviceUrlDelete))
			{
				var deleteNoteModal = html.DeleteModal(modalDeleteNoteSize, string.Join(" ", new[] { "modal-deletenote", modalDeleteNoteCssClass }).TrimEnd(' '),
					modalDeleteNoteTitle.GetValueOrDefault(DefaultDeleteNoteModalTitle),
					modalDeleteNoteConfirmation.GetValueOrDefault(DefaultDeleteNoteModalConfirmation),
					modalDeleteNoteDismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText),
					modalDeleteNotePrimaryButtonText.GetValueOrDefault(DefaultDeleteNoteModalPrimaryButtonText),
					modalDeleteNoteCancelButtonText.GetValueOrDefault(DefaultDeleteNoteModalCancelButtonText),
					modalDeleteNoteTitleCssClass, modalDeleteNotePrimaryButtonCssClass, modalDeleteNoteCancelButtonCssClass,
					modalDeleteNoteHtmlAttributes);

				container.InnerHtml += deleteNoteModal;
			}
		

			return new HtmlString(container.ToString());
		}

		/// <summary>
		/// Render a bootstrap modal dialog for adding notes.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceUrl">URL to the service that handles the add note request.</param>
		/// <param name="target">EntityReference of the target entity the note is regarding. <see cref="EntityReference"/></param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="noteFieldLabel">Text displayed for the note field label.</param>
		/// <param name="displayAttachFile">Indicates whether the attach file control is displayed or not.</param>
		/// <param name="attachFileLabel">Text displayed for the attach file label.</param>
		/// <param name="attachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="displayPrivacyOptionField">Indicates whether the privacy option field (is private) is displayed or not.</param>
		/// <param name="privacyOptionFieldLabel">Text displayed for the privacy option field (is private) label.</param>
		/// <param name="privacyOptionFieldDefaultValue">Default value assigned to the privacy option field (is private).</param>
		/// <param name="formLeftColumnCssClass">CSS class applied to the form's left column.</param>
		/// <param name="formRightColumnCssClass">CSS class applied to the form's right column.</param>
		/// <param name="noteFieldColumns">Number of columns to assign to the note field textarea.</param>
		/// <param name="noteFieldRows">Number of rows to assign to the note field textarea.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString AddNoteModal(this HtmlHelper html, EntityReference target, string serviceUrl,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null,
			string closeButtonCssClass = null, string noteFieldLabel = null, bool displayAttachFile = true,
			string attachFileLabel = null, string attachFileAccept = null, bool displayPrivacyOptionField = true,
			string privacyOptionFieldLabel = null, bool privacyOptionFieldDefaultValue = true,
			string formLeftColumnCssClass = null, string formRightColumnCssClass = null, int? noteFieldColumns = 20,
			int? noteFieldRows = 6, IDictionary<string, string> htmlAttributes = null, bool isTimeline = false, int? textMaxLength = null)
		{
			var body = new TagBuilder("div");
			body.AddCssClass("addnote");
			body.AddCssClass("form-horizontal");
			body.MergeAttribute("data-url", serviceUrl);
			var entityReference = new JObject(new JProperty("LogicalName", target.LogicalName),
				new JProperty("Id", target.Id.ToString()));
			body.MergeAttribute("data-target", entityReference.ToString());

			var row1 = new TagBuilder("div");
			row1.AddCssClass("form-group");
			var noteInputLabel = new TagBuilder("label");
			noteInputLabel.MergeAttribute("id", "note_label");
			noteInputLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormLeftColumnCssClass));
			noteInputLabel.AddCssClass("control-label");
			noteInputLabel.AddCssClass("required");
            noteInputLabel.InnerHtml = noteFieldLabel.GetValueOrDefault(isTimeline ? DefaultAddCommentModalNoteFieldLabel : DefaultAddNoteModalNoteFieldLabel);
			var noteInputContainer = new TagBuilder("div");
			noteInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormRightColumnCssClass));
			var noteInput = new TagBuilder("textarea");
			noteInput.MergeAttribute("name", "text");
			noteInput.AddCssClass("form-control");
			noteInput.MergeAttribute("aria-label", AddCommentButtonLabel);
			noteInput.MergeAttribute("tabindex", "0");
			noteInput.MergeAttribute("cols", noteFieldColumns.GetValueOrDefault(DefaultNoteFieldColumns).ToString(CultureInfo.InvariantCulture));
			noteInput.MergeAttribute("rows", noteFieldRows.GetValueOrDefault(DefaultNoteFieldRows).ToString(CultureInfo.InvariantCulture));
			if (textMaxLength != null) {
				noteInput.MergeAttribute("maxlength", ((int)textMaxLength).ToString());
			}
			noteInputContainer.InnerHtml = noteInput.ToString();
			row1.InnerHtml = noteInputLabel.ToString();
			row1.InnerHtml += noteInputContainer.ToString();
			body.InnerHtml += row1.ToString();

			if (displayPrivacyOptionField)
			{
				var row2 = new TagBuilder("div");
				row2.AddCssClass("form-group");
				var privacyOptionLabel = new TagBuilder("label");
				privacyOptionLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormLeftColumnCssClass));
				privacyOptionLabel.AddCssClass("control-label");
				privacyOptionLabel.InnerHtml = privacyOptionFieldLabel.GetValueOrDefault(DefaultAddNoteModalPrivacyOptionFieldLabel);
				var privacyOptionContainer = new TagBuilder("div");
				privacyOptionContainer.AddCssClass(
				formRightColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormRightColumnCssClass));
				var privacyOption = new TagBuilder("input");
				privacyOption.MergeAttribute("name", "isPrivate");
				privacyOption.AddCssClass("checkbox");
				privacyOption.MergeAttribute("type", "checkbox");
				privacyOptionContainer.InnerHtml = privacyOption.ToString();
				row2.InnerHtml = privacyOptionLabel.ToString();
				row2.InnerHtml += privacyOptionContainer.ToString();
				body.InnerHtml += row2.ToString();
			}

			if (displayAttachFile)
			{
				var row3 = new TagBuilder("div");
				row3.AddCssClass("form-group");                
				var fileInputLabel = new TagBuilder("label");
				fileInputLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormLeftColumnCssClass));
				fileInputLabel.AddCssClass("control-label");
				fileInputLabel.InnerHtml = attachFileLabel.GetValueOrDefault(isTimeline ? DefaultAddCommentModalAttachFileLabel : DefaultAddNoteModalAttachFileLabel);
				fileInputLabel.MergeAttribute("aria-label", DefaultAddCommentModalAttachFileLabel);
				var fileInputContainer = new TagBuilder("div");
				fileInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultAddNoteModalFormRightColumnCssClass));
				var fileInputBlock = new TagBuilder("div");
				fileInputBlock.AddCssClass("form-control-static");
				var fileInput = new TagBuilder("input");
				fileInput.MergeAttribute("name", "file");
				fileInput.MergeAttribute("type", "file");
				fileInput.MergeAttribute("aria-label", DefaultAddNoteModalAttachFileLabel);
				fileInput.MergeAttribute("tabindex", "0");
				fileInput.MergeAttribute("multiple", string.Empty);
				if (!string.IsNullOrWhiteSpace(attachFileAccept))
				{
					fileInput.MergeAttribute("accept", attachFileAccept);
				}
				fileInputBlock.InnerHtml = fileInput.ToString(TagRenderMode.SelfClosing);
				fileInputContainer.InnerHtml = fileInputBlock.ToString();
				row3.InnerHtml = fileInputLabel.ToString();
				row3.InnerHtml += fileInputContainer.ToString();
				body.InnerHtml += row3.ToString();
			}

			if (isTimeline)
			{
				return html.BoostrapModal(BootstrapExtensions.BootstrapModalSize.Default,
					title.GetValueOrDefault(DefaultAddCommentModalTitle), body.ToString(),
					string.Join(" ", new[] { "modal-addnote", cssClass }).TrimEnd(' '), null, false,
					dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
					primaryButtonText.GetValueOrDefault(DefaultAddCommentModalPrimaryButtonText),
					cancelButtonText.GetValueOrDefault(DefaultAddCommentModalCancelButtonText), titleCssClass, primaryButtonCssClass,
					closeButtonCssClass, htmlAttributes);
			}
			else
			{
				return html.BoostrapModal(BootstrapExtensions.BootstrapModalSize.Default,
					title.GetValueOrDefault(DefaultAddNoteModalTitle), body.ToString(),
					string.Join(" ", new[] { "modal-addnote", cssClass }).TrimEnd(' '), null, false,
					dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
					primaryButtonText.GetValueOrDefault(DefaultAddNoteModalPrimaryButtonText),
					cancelButtonText.GetValueOrDefault(DefaultAddNoteModalCancelButtonText), titleCssClass, primaryButtonCssClass,
					closeButtonCssClass, htmlAttributes);

			}
		}

		/// <summary>
		/// Render a bootstrap modal dialog for editing notes.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="serviceUrl">URL to the service that handles the add note request.</param>
		/// <param name="target">EntityReference of the target entity the note is regarding. <see cref="EntityReference"/></param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="noteFieldLabel">Text displayed for the note field label.</param>
		/// <param name="displayAttachFile">Indicates whether the attach file control is displayed or not.</param>
		/// <param name="attachFileLabel">Text displayed for the attach file label.</param>
		/// <param name="attachFileAccept">The accept attribute specifies the MIME types of files that the server accepts through file upload. 
		/// To specify more than one value, separate the values with a comma (e.g. audio/*,video/*,image/*).</param>
		/// <param name="displayPrivacyOptionField">Indicates whether the privacy option field (is private) is displayed or not.</param>
		/// <param name="privacyOptionFieldLabel">Text displayed for the privacy option field (is private) label.</param>
		/// <param name="privacyOptionFieldDefaultValue">Default value assigned to the privacy option field (is private).</param>
		/// <param name="formLeftColumnCssClass">CSS class applied to the form's left column.</param>
		/// <param name="formRightColumnCssClass">CSS class applied to the form's right column.</param>
		/// <param name="noteFieldColumns">Number of columns to assign to the note field textarea.</param>
		/// <param name="noteFieldRows">Number of rows to assign to the note field textarea.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString EditNoteModal(this HtmlHelper html, EntityReference target, string serviceUrl,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null,
			string closeButtonCssClass = null, string noteFieldLabel = null, bool displayAttachFile = true,
			string attachFileLabel = null, string attachFileAccept = null, bool displayPrivacyOptionField = true,
			string privacyOptionFieldLabel = null, bool privacyOptionFieldDefaultValue = true,
			string formLeftColumnCssClass = null, string formRightColumnCssClass = null, int? noteFieldColumns = 20,
			int? noteFieldRows = 6, IDictionary<string, string> htmlAttributes = null)
		{
			var body = new TagBuilder("div");
			body.AddCssClass("editnote");
			body.AddCssClass("form-horizontal");
			body.MergeAttribute("data-url", serviceUrl);
			var entityReference = new JObject(new JProperty("LogicalName", target.LogicalName),
				new JProperty("Id", target.Id.ToString()));
			body.MergeAttribute("data-target", entityReference.ToString());

			var row1 = new TagBuilder("div");
			row1.AddCssClass("form-group");
			var noteInputLabel = new TagBuilder("label");
			noteInputLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormLeftColumnCssClass));
			noteInputLabel.AddCssClass("control-label");
			noteInputLabel.AddCssClass("required");
			noteInputLabel.MergeAttribute("id", "note_label");
			noteInputLabel.InnerHtml = noteFieldLabel.GetValueOrDefault(DefaultEditNoteModalNoteFieldLabel);
			var noteInputContainer = new TagBuilder("div");
			noteInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormRightColumnCssClass));
			var noteInput = new TagBuilder("textarea");
			noteInput.MergeAttribute("name", "text");
			noteInput.MergeAttribute("aria-label", AddCommentButtonLabel);
			noteInput.MergeAttribute("tabindex", "0");
			noteInput.AddCssClass("form-control");
			noteInput.MergeAttribute("cols", noteFieldColumns.GetValueOrDefault(DefaultNoteFieldColumns).ToString(CultureInfo.InvariantCulture));
			noteInput.MergeAttribute("rows", noteFieldRows.GetValueOrDefault(DefaultNoteFieldRows).ToString(CultureInfo.InvariantCulture));
			noteInputContainer.InnerHtml = noteInput.ToString();
			row1.InnerHtml = noteInputLabel.ToString();
			row1.InnerHtml += noteInputContainer.ToString();
			body.InnerHtml += row1.ToString();

			if (displayPrivacyOptionField)
			{
				var row2 = new TagBuilder("div");
				row2.AddCssClass("form-group");
				var privacyOptionLabel = new TagBuilder("label");
				privacyOptionLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormLeftColumnCssClass));
				privacyOptionLabel.AddCssClass("control-label");
				privacyOptionLabel.InnerHtml = privacyOptionFieldLabel.GetValueOrDefault(DefaultEditNoteModalPrivacyOptionFieldLabel);
				var privacyOptionContainer = new TagBuilder("div");
				privacyOptionContainer.AddCssClass(
					formRightColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormRightColumnCssClass));
				var privacyOption = new TagBuilder("input");
				privacyOption.MergeAttribute("name", "isPrivate");
				privacyOption.AddCssClass("checkbox");
				privacyOption.MergeAttribute("type", "checkbox");
				privacyOptionContainer.InnerHtml = privacyOption.ToString();
				row2.InnerHtml = privacyOptionLabel.ToString();
				row2.InnerHtml += privacyOptionContainer.ToString();
				body.InnerHtml += row2.ToString();
			}

			if (displayAttachFile)
			{
				var row3 = new TagBuilder("div");
				row3.AddCssClass("form-group");
				var fileInputLabel = new TagBuilder("label");
				fileInputLabel.AddCssClass(formLeftColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormLeftColumnCssClass));
				fileInputLabel.AddCssClass("control-label");
				fileInputLabel.InnerHtml = attachFileLabel.GetValueOrDefault(DefaultEditNoteModalAttachFileLabel);
				fileInputLabel.MergeAttribute("aria-label", DefaultAddCommentModalAttachFileLabel);
				var fileInputContainer = new TagBuilder("div");
				fileInputContainer.AddCssClass(formRightColumnCssClass.GetValueOrDefault(DefaultEditNoteModalFormRightColumnCssClass));
				fileInputContainer.MergeAttribute("tabindex", "0");
				var fileInputBlock = new TagBuilder("div");
				fileInputBlock.AddCssClass("form-control-static");
				var fileInput = new TagBuilder("input");
				fileInput.MergeAttribute("name", "file");
				fileInput.MergeAttribute("type", "file");
				fileInput.MergeAttribute("aria-label", DefaultAddNoteModalAttachFileLabel);
				fileInput.MergeAttribute("tabindex", "0");
				if (!string.IsNullOrWhiteSpace(attachFileAccept))
				{
					fileInput.MergeAttribute("accept", attachFileAccept);
				}
				fileInputBlock.InnerHtml = fileInput.ToString();
				fileInputContainer.InnerHtml = fileInputBlock.ToString();
				row3.InnerHtml = fileInputLabel.ToString();
				row3.InnerHtml += fileInputContainer.ToString();
				body.InnerHtml += row3.ToString();
			}

			return html.BoostrapModal(BootstrapExtensions.BootstrapModalSize.Default,
				title.GetValueOrDefault(DefaultEditNoteModalTitle), body.ToString(),
				string.Join(" ", new[] { "modal-editnote", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(DefaultEditNoteModalPrimaryButtonText),
				cancelButtonText.GetValueOrDefault(DefaultEditNoteModalCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		/// <summary>
		/// Helper for creating notes control template
		/// </summary>
		/// <param name="notePrivacyLabel"></param>
		/// <param name="editNotesEnabled"></param>
		/// <param name="deleteNotesEnabled"></param>
		/// <param name="toolbarButtonLabel"></param>
		/// <param name="editNoteButtonLabel"></param>
		/// <param name="deleteNoteButtonLabel"></param>
		/// <returns></returns>
		private static string BuildNotesTemplate(string notePrivacyLabel, bool editNotesEnabled, bool deleteNotesEnabled, string toolbarButtonLabel, string editNoteButtonLabel,
			string deleteNoteButtonLabel)
		{
			var template = new TagBuilder("script");
			template.MergeAttribute("id", "notes-template");
			template.MergeAttribute("type", "text/x-handlebars-template");
			template.InnerHtml = "{{#each Records}}";
			var noteBlock = new TagBuilder("div");
			noteBlock.AddCssClass("note");
			noteBlock.MergeAttribute("data-id", "{{Id}}");
			noteBlock.MergeAttribute("data-canedit", "{{CanWrite}}");
			noteBlock.MergeAttribute("data-candelete", "{{CanDelete}}");
			noteBlock.MergeAttribute("data-subject", "{{Subject}}");
			noteBlock.MergeAttribute("data-unformattedtext", "{{UnformattedText}}");
			noteBlock.MergeAttribute("data-isprivate", "{{IsPrivate}}");
			noteBlock.MergeAttribute("data-hasattachment", "{{HasAttachment}}");
			noteBlock.MergeAttribute("data-attachmentfilename", "{{AttachmentFileName}}");
			noteBlock.MergeAttribute("data-attachmentfilesize", "{{AttachmentSizeDisplay}}");
			noteBlock.MergeAttribute("data-attachmenturl", "{{AttachmentUrlWithTimeStamp}}");
			noteBlock.MergeAttribute("data-attachmentisimage", "{{AttachmentIsImage}}");
			var noteRow = new TagBuilder("div");
			noteRow.AddCssClass("row");
			var noteMetadata = new TagBuilder("div");
			noteMetadata.AddCssClass("col-sm-3 metadata");
			var noteDateContainer = new TagBuilder("div");
			noteDateContainer.AddCssClass("postedon");
			var noteDateAbbr = new TagBuilder("abbr");
			noteDateAbbr.AddCssClass("timeago");
			noteDateAbbr.MergeAttribute("title", "{{CreatedOnDisplay}}");
			noteDateAbbr.MergeAttribute("data-datetime", "{{CreatedOnDisplay}}");
			noteDateAbbr.InnerHtml = "{{CreatedOnDisplay}}";
			noteDateContainer.InnerHtml = noteDateAbbr.ToString();
			noteMetadata.InnerHtml = noteDateContainer.ToString();
			noteMetadata.InnerHtml += "{{#if PostedByName}}";
			var notePostedBy = new TagBuilder("div");
			notePostedBy.AddCssClass("createdby text-muted");
			notePostedBy.InnerHtml = "{{PostedByName}}";
			noteMetadata.InnerHtml += notePostedBy.ToString();
			noteMetadata.InnerHtml += "{{/if}}";
			noteMetadata.InnerHtml += "{{#if IsPrivate}}";
			var notePrivacy = new TagBuilder("div");
			notePrivacy.AddCssClass("label label-warning");
			notePrivacy.InnerHtml = notePrivacyLabel.GetValueOrDefault(DefaultNotePrivacyLabel);
			noteMetadata.InnerHtml += notePrivacy.ToString();
			noteMetadata.InnerHtml += "{{/if}}";
			noteRow.InnerHtml = noteMetadata.ToString();
			var noteContent = new TagBuilder("div");
			noteContent.AddCssClass("col-sm-9 content");
			if (editNotesEnabled || deleteNotesEnabled)
			{
				noteContent.InnerHtml = "{{#if DisplayToolbar}}";
				var dropdown = new TagBuilder("div");
				dropdown.AddCssClass("toolbar dropdown pull-right");
				var dropdownLink = new TagBuilder("a");
				dropdownLink.AddCssClass("btn btn-default btn-xs");
				dropdownLink.MergeAttribute("href", "#");
				dropdownLink.MergeAttribute("data-toggle", "dropdown");
				dropdownLink.InnerHtml += toolbarButtonLabel.GetValueOrDefault(DefaultToolbarButtonLabel);
				dropdown.InnerHtml += dropdownLink.ToString();
				var dropdownMenu = new TagBuilder("ul");
				dropdownMenu.AddCssClass("dropdown-menu");
				dropdownMenu.MergeAttribute("role", "menu");
				if (editNotesEnabled)
				{
					dropdownMenu.InnerHtml += "{{#if CanWrite}}";
					var editItem = new TagBuilder("li");
					editItem.MergeAttribute("role", "presentation");
					var editLink = new TagBuilder("a");
					editLink.AddCssClass("edit-link");
					editLink.MergeAttribute("role", "menuitem");
					editLink.MergeAttribute("tabindex", "-1");
					editLink.MergeAttribute("href", "#");
					editLink.InnerHtml = editNoteButtonLabel.GetValueOrDefault(DefaultEditNoteButtonLabel);
					editItem.InnerHtml = editLink.ToString();
					dropdownMenu.InnerHtml += editItem.ToString();
					dropdownMenu.InnerHtml += "{{/if}}";
				}
				if (deleteNotesEnabled)
				{
					dropdownMenu.InnerHtml += "{{#if CanDelete}}";
					var deleteItem = new TagBuilder("li");
					deleteItem.MergeAttribute("role", "presentation");
					var deleteLink = new TagBuilder("a");
					deleteLink.AddCssClass("delete-link");
					deleteLink.MergeAttribute("role", "menuitem");
					deleteLink.MergeAttribute("tabindex", "-1");
					deleteLink.MergeAttribute("href", "#");
					deleteLink.InnerHtml = deleteNoteButtonLabel.GetValueOrDefault(DefaultDeleteNoteButtonLabel);
					deleteItem.InnerHtml = deleteLink.ToString();
					dropdownMenu.InnerHtml += deleteItem.ToString();
					dropdownMenu.InnerHtml += "{{/if}}";
				}
				dropdown.InnerHtml += dropdownMenu.ToString();
				noteContent.InnerHtml += dropdown.ToString();
				noteContent.InnerHtml += "{{/if}}";
			}
			var noteTextBlock = new TagBuilder("div");
			noteTextBlock.AddCssClass("text");
			noteTextBlock.InnerHtml = "{{{Text}}}";
			noteContent.InnerHtml += noteTextBlock.ToString();
			noteContent.InnerHtml += "{{#if HasAttachment}}";
			var attachmentContainer = new TagBuilder("div");
			attachmentContainer.AddCssClass("attachment alert alert-block alert-info clearfix");
			attachmentContainer.InnerHtml = "{{#if AttachmentIsImage}}";
			var imageContainer = new TagBuilder("div");
			imageContainer.AddCssClass("img pull-left");
			var imageLink = new TagBuilder("a");
			imageLink.AddCssClass("thumbnail");
			imageLink.MergeAttribute("target", "_blank");
			imageLink.MergeAttribute("href", "{{AttachmentUrlWithTimeStamp}}");
			var image = new TagBuilder("img");
			image.MergeAttribute("src", "{{AttachmentUrlWithTimeStamp}}");
			imageLink.InnerHtml = image.ToString();
			imageContainer.InnerHtml = imageLink.ToString();
			attachmentContainer.InnerHtml += imageContainer.ToString();
			attachmentContainer.InnerHtml += "{{/if}}";
			var attachmentLinkContainer = new TagBuilder("div");
			attachmentLinkContainer.AddCssClass("link pull-left");
			attachmentLinkContainer.InnerHtml = "<span class='fa fa-file' aria-hidden='true'></span> ";
			var attachmentLink = new TagBuilder("a");
			attachmentLink.MergeAttribute("target", "_blank");
			attachmentLink.MergeAttribute("href", "{{AttachmentUrlWithTimeStamp}}");
			attachmentLink.InnerHtml = "{{AttachmentFileName}} ({{AttachmentSizeDisplay}})";
			attachmentLinkContainer.InnerHtml += attachmentLink.ToString();
			attachmentContainer.InnerHtml += attachmentLinkContainer.ToString();
			noteContent.InnerHtml += attachmentContainer.ToString();
			noteContent.InnerHtml += "{{/if}}";
			noteRow.InnerHtml += noteContent.ToString();
			noteBlock.InnerHtml = noteRow.ToString();
			template.InnerHtml += noteBlock.ToString();
			template.InnerHtml += "{{/each}}";

			return template.ToString();
		}

		/// <summary>
		/// Helper for creating timeline template
		/// </summary>
		/// <returns></returns>
		private static string BuildTimelineTemplate()
		{
			var template = new TagBuilder("script");
			template.MergeAttribute("id", "notes-template");
			template.MergeAttribute("type", "text/x-handlebars-template");
			template.InnerHtml = @"{{#each Records}}
<div class=""note"" data-candelete=""{{CanDelete}}"" data-canedit=""{{CanWrite}}"" data-id=""{{Id}}"" data-isprivate=""{{IsPrivate}}"" data-subject=""{{Subject}}"" data-unformattedtext=""{{UnformattedText}}"">
	<div class=""row"">
		{{#if_eq ViewFields.activitytypecode ""email""}}
			<div class=""col-md-3 col-xs-12 header"">
				<div class=""col-md-12 col-xs-3 emailicon""><span class=""glyphicon glyphicon-envelope""></span></div>
				<div class=""col-md-12 col-xs-9 metadata"">
					<div class=""postedon"">
						<div class=""timeago"" data-datetime=""{{CreatedOnDisplay}}"" title=""{{CreatedOnDisplay}}"">{{CreatedOnDisplay}}</div>
					</div>
					{{#if ViewFields.modifiedon}}
						{{#if_not_eq ViewFields.modifiedon ViewFields.createdon}}
							<div class=""modifiedon"">" + string.Format(DefaultTimelineModifiedOnFieldLabel, "{{#dateTimeFormatter ViewFields.modifiedon}}") + @"{{this}}{{/dateTimeFormatter}}</div>
						{{/if_not_eq}}
					{{/if}}
				</div>
			</div>
			<div class=""col-md-9 col-xs-12 content"">
				<div class=""from"">
					{{#if ViewFields.from}}
						{{ViewFields.from.Name}}
					{{/if}}
					{{#if ViewFields.to}}
						<span class=""glyphicon glyphicon-arrow-right""></span> {{#with ViewFields.to}}{{#commaSeparatedList this}}{{this}}{{/commaSeparatedList}}{{/with}}
					{{/if}}
				</div>				
				{{#if ViewFields.cc}}
					<div class=""cc"">CC: {{#with ViewFields.cc}}{{#commaSeparatedList this}}{{this}}{{/commaSeparatedList}}{{/with}}</div>
				{{/if}}
				<div class=""subject""><b>{{ViewFields.subject}}</b></div>
				<div class=""description clearfix"">{{{ViewFields.description}}}</div>
				{{#attachments ViewFields.activityid ViewFields.activitytypecode}}
				{{/attachments}}
			</div>
		{{/if_eq}}
		{{#if_eq ViewFields.activitytypecode ""phonecall""}}
			<div class=""col-md-3 col-xs-12 header"">
				<div class=""col-md-12 col-xs-3 phonecallicon""><span class=""glyphicon glyphicon-earphone""></span></div>
				<div class=""col-md-12 col-xs-9 metadata"">
					<div class=""postedon"">
						<div class=""timeago"" data-datetime=""{{CreatedOnDisplay}}"" title=""{{CreatedOnDisplay}}"">{{CreatedOnDisplay}}</div>
					</div>
					{{#if ViewFields.modifiedon}}
						{{#if_not_eq ViewFields.modifiedon ViewFields.createdon}}
							<div class=""modifiedon"">" + string.Format(DefaultTimelineModifiedOnFieldLabel, "{{#dateTimeFormatter ViewFields.modifiedon}}") + @"{{this}}{{/dateTimeFormatter}}</div>
						{{/if_not_eq}}
					{{/if}}
				</div>
			</div>
			<div class=""col-md-9 col-xs-12 content"">
				<div class=""from"">{{ViewFields.from.Name}} <span class=""glyphicon glyphicon-arrow-right""></span> {{#with ViewFields.to}}{{#commaSeparatedList this}}{{this}}{{/commaSeparatedList}}{{/with}}</div>
				<div class=""subject""><b>{{ViewFields.subject}}</b></div>
				<div class=""description"">{{{ViewFields.description}}}</div>
				{{#if PostedByName}}
					<div class=""createdby text-muted"">" + string.Format(DefaultTimelineCreatedByFieldLabel, "{{PostedByName}}") + @"</div>
				{{/if}}
			</div>
		{{/if_eq}}
		{{#if_eq ViewFields.activitytypecode ""appointment""}}
			<div class=""col-md-3 col-xs-12 header"">
				<div class=""col-md-12 col-xs-3 appointmenticon""><span class=""glyphicon glyphicon-calendar""></span></div>
				<div class=""col-md-12 col-xs-9 metadata"">
					<div class=""postedon"">
						<div class=""timeago"" data-datetime=""{{CreatedOnDisplay}}"" title=""{{CreatedOnDisplay}}"">{{CreatedOnDisplay}}</div>
					</div>
				</div>
			</div>
			<div class=""col-md-9 col-xs-12 content"">
				<div class=""requiredattendees"">
					{{#if ViewFields.from}}
						{{ViewFields.from.Name}}
					{{/if}}
					{{#if ViewFields.requiredattendees}}
						<span class=""glyphicon glyphicon-arrow-right""></span> {{#with ViewFields.requiredattendees}}{{#commaSeparatedList this}}{{this}}{{/commaSeparatedList}}{{/with}}
					{{/if}}
				</div>
				<div class=""scheduledstartandend"">{{#dateTimeFormatter ViewFields.scheduledstart}}{{this}}{{/dateTimeFormatter}} - {{#dateTimeFormatter ViewFields.scheduledend}}{{this}}{{/dateTimeFormatter}}</div>
				<div class=""subject""><b>{{{ViewFields.subject}}}</b></div>
				<div class=""description"">{{{ViewFields.description}}}</div>
			</div>
		{{/if_eq}}
		{{#if_eq ViewFields.activitytypecode ""adx_portalcomment""}}
			<div class=""col-md-3 col-xs-12 header"">
				<div class=""col-md-12 col-xs-3 portalcommenticon""><span class=""glyphicon glyphicon-user""></span></div>
				<div class=""col-md-12 col-xs-9 metadata"">
					<div class=""postedon"">
						<div class=""timeago"" data-datetime=""{{CreatedOnDisplay}}"" title=""{{CreatedOnDisplay}}"">{{CreatedOnDisplay}}</div>
					</div>
					{{#if ViewFields.modifiedon}}
						{{#if_not_eq ViewFields.modifiedon ViewFields.createdon}}
							<div class=""modifiedon"">" + string.Format(DefaultTimelineModifiedOnFieldLabel, "{{#dateTimeFormatter ViewFields.modifiedon}}") + @"{{this}}{{/dateTimeFormatter}}</div>
						{{/if_not_eq}}
					{{/if}}
				</div>
			</div>
			<div class=""col-md-9 col-xs-12 content"">
				<div class=""from"">
					<h5>
						{{#if ViewFields.from}}
							{{ViewFields.from.Name}}
						{{/if}}
						{{#if ViewFields.to}}
							<span class=""glyphicon glyphicon-arrow-right""></span> {{#with ViewFields.to}}{{#commaSeparatedList this}}{{this}}{{/commaSeparatedList}}{{/with}}
						{{/if}}
					</h5>
				</div>
				<div class=""description"">{{{ViewFields.description}}}</div>
				{{#if PostedByName}}
					<div class=""createdby text-muted"">" + string.Format(DefaultTimelineCreatedByFieldLabel, "{{PostedByName}}") + @"</div>
				{{/if}}
				{{#attachments ViewFields.activityid ViewFields.activitytypecode}}
				{{/attachments}}
			</div>
		{{/if_eq}}
		{{#if IsCustomActivity}}
			<div class=""col-md-3 col-xs-12 header"">
				<div class=""col-md-12 col-xs-3 asteriskicon""><span class=""glyphicon glyphicon-asterisk""></span></div>
				<div class=""col-md-12 col-xs-9 metadata"">
					<div class=""postedon"">
						<div class=""timeago"" data-datetime=""{{CreatedOnDisplay}}"" title=""{{CreatedOnDisplay}}"">{{CreatedOnDisplay}}</div>
					</div>
					{{#if ViewFields.modifiedon}}
						{{#if_not_eq ViewFields.modifiedon ViewFields.createdon}}
							<div class=""modifiedon"">" + string.Format(DefaultTimelineModifiedOnFieldLabel, "{{#dateTimeFormatter ViewFields.modifiedon}}") + @"{{this}}{{/dateTimeFormatter}}</div>
						{{/if_not_eq}}
					{{/if}}
				</div>
			</div>
			<div class=""col-md-9 col-xs-12 content"">
				<div class=""subject""><b>{{ViewFields.subject}}</b></div>
				<div class=""description"">{{{ViewFields.description}}}</div>
			</div>
		{{/if}}
	</div>
</div>
{{/each}}";

			var attachmentTemplate = new TagBuilder("script");
			attachmentTemplate.MergeAttribute("id", "notes-attachment-template");
			attachmentTemplate.MergeAttribute("type", "text/x-handlebars-template");

			attachmentTemplate.InnerHtml = @"{{#each this}}
<div class=""col-md-12 col-xs-12 note-attachment"" data-attachmentfilename=""{{AttachmentFileName}}"" data-attachmentfilesize=""{{AttachmentSizeDisplay}}"" data-attachmentisimage=""{{AttachmentIsImage}}"" data-attachmenturl=""{{AttachmentUrlWithTimeStamp}}"" data-hasattachment=""{{HasAttachment}}"">
	<div class=""row"">
		<div class=""attachment alert alert-block alert-info clearfix"">
			{{#if AttachmentIsImage}}
				<div class=""img pull-left"">
					<a class=""thumbnail"" href=""{{AttachmentUrlWithTimeStamp}}"" target=""_blank"">
						<img src=""{{AttachmentUrlWithTimeStamp}}""></img>
					</a>
				</div>
			{{/if}}
			<div class=""link pull-left"">
				<span class='fa fa-file' aria-hidden='true'></span> <a href=""{{AttachmentUrlWithTimeStamp}}"" target=""_blank"">{{AttachmentFileName}} ({{AttachmentSizeDisplay}})</a>
			</div>
		</div>
	</div>
</div>
</div>
{{/each}}";

			return template.ToString() + attachmentTemplate.ToString();
		}
	}
}
