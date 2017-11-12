/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering an grid of entity records based on an entity savedquery view from CRM within Adxstudio Portals applications.
	/// </summary>
	/// <remarks>Requires Bootstrap 3, jQuery and the following files: ~/js/jquery.bootstrap-pagination.js, ~/js/entity-grid.js, ~/js/entity-lookup.js</remarks>
	public static class EntityGridExtensions
	{
		private static string _defaultModalCreateFormTitle = "<span class='fa fa-pencil-square-o' aria-hidden='true'></span> " + ResourceManager.GetString("Create_Text");
		private static string _defaultModalCreateRelatedRecordTitle = "<span class='fa fa-pencil-square-o' aria-hidden='true'></span> " + ResourceManager.GetString("CreateRelatedRecord_Text");
		private const string DefaultModalCreateFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static string _defaultModalEditFormTitle = "<span class='fa fa-edit'></span> " + ResourceManager.GetString("Edit_Label");
		private const string DefaultModalEditFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static string _defaultModalDetailsFormTitle = "<span class='fa fa-info-circle' aria-hidden='true'></span> " + ResourceManager.GetString("View_Details_Tooltip");
		private const string DefaultModalDetailsFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static string _defaultModalDeleteTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");
		private static string _defaultModalDeleteBody = ResourceManager.GetString("Record_Deletion_Confirmation_Message");
		private static string _defaultModalDeletePrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
		private static string _defaultModalDeleteCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static string _defaultErrorModalTitle = "<span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Error_ModalTitle");
		private static string _defaultErrorModalBody = ResourceManager.GetString("Error_Occurred_Message");
		private static string _defaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static string _defaultModalCloseButtonText = ResourceManager.GetString("Close_DefaultText");
		private static string _defaultGridSelectColumnHeaderText = "<span class='fa fa-check' aria-hidden='true'></span> <span class='sr-only'>" + ResourceManager.GetString("Select_Column_Header_Text") + "</span>";
		private static string _defaultGridLoadingMessage = "<span class='fa fa-spinner fa-spin' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Grid_Loading_Message");
		private static string _defaultGridErrorMessage = ResourceManager.GetString("Error_Completing_Request_Error_Message") + "<span class='details'></span>";
		private static string _defaultGridAccessDeniedMessage = ResourceManager.GetString("Access_Denied_No_Permissions_To_View_These_Records_Message");
		private static string _defaultGridEmptyMessage = ResourceManager.GetString("Default_Grid_Empty_Message");
		private static string _defaultLookupEntityGridToggleFilterText = ResourceManager.GetString("Toggle_Filter_Text");
		private static string _defaultLookupEntityGridToggleFilterDisplayName = ResourceManager.GetString("Toggle_Filter_Display_Name");
        private static CultureInfo _currentCulture = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Selection Mode of a grid
        /// </summary>
        public enum GridSelectMode
		{
			/// <summary>
			/// No selection available
			/// </summary>
			None,
			/// <summary>
			/// Single record can be selected
			/// </summary>
			Single,
			/// <summary>
			/// Multiple records can be selected
			/// </summary>
			Multiple
		}

		/// <summary>
		/// The style for the width of columns in a grid
		/// </summary>
		public enum GridColumnWidthStyle
		{
			/// <summary>
			/// Width will be set to pixels
			/// </summary>
			Pixels,
			/// <summary>
			/// Width will be set to percent
			/// </summary>
			Percent
		}

		/// <summary>
		/// Renders an HTML structure for displaying a grid of records.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="viewConfiguration"><see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to retrieve the data.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="deferLoading">Indicates whether loading data should or should not occurr on startup.</param>
		/// <param name="enableActions">Determines whether actions (create, associate) are enabled or not.</param>
		/// <param name="modalDetailsFormSize">Size of the details form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDetailsFormCssClass">A CSS class applied to the details form's modal element.</param>
		/// <param name="modalDetailsFormTitle">Text displayed in the details form's modal title.</param>
		/// <param name="modalDetailsFormLoadingMessage">Message displayed in the details form's modal during loading.</param>
		/// <param name="modalDetailsFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDetailsFormTitleCssClass">A CSS class applied to the details form's modal title.</param>
		/// <param name="modalCreateFormSize">Size of the create form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalCreateFormCssClass">A CSS class applied to the create form's modal element.</param>
		/// <param name="modalCreateFormTitle">Text displayed in the create form's modal title.</param>
		/// <param name="modalCreateFormLoadingMessage">Message displayed in the create form's modal during loading.</param>
		/// <param name="modalCreateFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalCreateFormTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalEditFormSize">Size of the edit form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalEditFormCssClass">A CSS class applied to the edit form's modal element.</param>
		/// <param name="modalEditFormTitle">Text displayed in the edit form's modal title.</param>
		/// <param name="modalEditFormLoadingMessage">Message displayed in the edit form's modal during loading.</param>
		/// <param name="modalEditFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalEditFormTitleCssClass">A CSS class applied to the edit form's modal title.</param>
		/// <param name="modalDeleteSize">Size of the delete modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDeleteCssClass">A CSS class applied to the delete modal element.</param>
		/// <param name="modalDeleteTitle">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeleteBody">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeletePrimaryButtonText">Text displayed for the delete modal's primary button.</param>
		/// <param name="modalDeleteCloseButtonText">Text displayed for the delete modal's close button.</param>
		/// <param name="modalDeleteDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDeleteTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalErrorSize">Size of the error modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalErrorCssClass">CSS class that will be applied to the error modal element.</param>
		/// <param name="modalErrorTitle">Title text displayed in the error modal header.</param>
		/// <param name="modalErrorBody">Default content body text displayed in the error modal body.</param>
		/// <param name="modalErrorDismissButtonSrText">The text to display for the error modal dismiss button for screen readers only.</param>
		/// <param name="modalErrorCloseButtonText">Text to display for the error modal close button.</param>
		/// <param name="modalErrorTitleCssClass">CSS class assigned to the error modal title.</param>
		public static IHtmlString EntityGrid(this HtmlHelper html, ViewConfiguration viewConfiguration, string serviceUrl, Entity user = null,
			string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true,
			BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null, BootstrapExtensions.BootstrapModalSize modalWorkflowModalSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalWorkflowModalCssClass = null, string modalWorkflowModalTitle = null)
		{
			if (viewConfiguration == null || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var grid = Grid(html, new List<ViewConfiguration> { viewConfiguration }, serviceUrl, user, cssClass, gridCssClass,
				columnWidthStyle, selectMode, selectColumnHeaderText, loadingMessage, errorMessage, accessDeniedMessage,
				emptyMessage, portalName, languageCode, deferLoading, enableActions, null, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass,
				modalWorkflowModalSize, modalWorkflowModalCssClass, modalWorkflowModalTitle);

			return new HtmlString(grid.ToString());
		}

		/// <summary>
		/// Renders an HTML structure for displaying a grid of records.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to retrieve the data.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="deferLoading">Indicates whether loading data should or should not occurr on startup.</param>
		/// <param name="enableActions">Determines whether actions (create, associate) are enabled or not.</param>
		/// <param name="modalDetailsFormSize">Size of the details form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDetailsFormCssClass">A CSS class applied to the details form's modal element.</param>
		/// <param name="modalDetailsFormTitle">Text displayed in the details form's modal title.</param>
		/// <param name="modalDetailsFormLoadingMessage">Message displayed in the details form's modal during loading.</param>
		/// <param name="modalDetailsFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDetailsFormTitleCssClass">A CSS class applied to the details form's modal title.</param>
		/// <param name="modalCreateFormSize">Size of the create form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalCreateFormCssClass">A CSS class applied to the create form's modal element.</param>
		/// <param name="modalCreateFormTitle">Text displayed in the create form's modal title.</param>
		/// <param name="modalCreateFormLoadingMessage">Message displayed in the create form's modal during loading.</param>
		/// <param name="modalCreateFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalCreateFormTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalEditFormSize">Size of the edit form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalEditFormCssClass">A CSS class applied to the edit form's modal element.</param>
		/// <param name="modalEditFormTitle">Text displayed in the edit form's modal title.</param>
		/// <param name="modalEditFormLoadingMessage">Message displayed in the edit form's modal during loading.</param>
		/// <param name="modalEditFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalEditFormTitleCssClass">A CSS class applied to the edit form's modal title.</param>
		/// <param name="modalDeleteSize">Size of the delete modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDeleteCssClass">A CSS class applied to the delete modal element.</param>
		/// <param name="modalDeleteTitle">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeleteBody">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeletePrimaryButtonText">Text displayed for the delete modal's primary button.</param>
		/// <param name="modalDeleteCloseButtonText">Text displayed for the delete modal's close button.</param>
		/// <param name="modalDeleteDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDeleteTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalErrorSize">Size of the error modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalErrorCssClass">CSS class that will be applied to the error modal element.</param>
		/// <param name="modalErrorTitle">Title text displayed in the error modal header.</param>
		/// <param name="modalErrorBody">Default content body text displayed in the error modal body.</param>
		/// <param name="modalErrorDismissButtonSrText">The text to display for the error modal dismiss button for screen readers only.</param>
		/// <param name="modalErrorCloseButtonText">Text to display for the error modal close button.</param>
		/// <param name="modalErrorTitleCssClass">CSS class assigned to the error modal title.</param>
		public static IHtmlString EntityGrid(this HtmlHelper html, List<ViewConfiguration> viewConfigurations,
			string serviceUrl, Entity user = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true, BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalWorkflowModalSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalWorkflowModalCssClass = null, string modalWorkflowModalTitle = null)
		{
			if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var grid = Grid(html, viewConfigurations, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode,
				selectColumnHeaderText, loadingMessage, errorMessage, accessDeniedMessage, emptyMessage, portalName, languageCode,
				deferLoading, enableActions, null, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass);

			return new HtmlString(grid.ToString());
		}

		/// <summary>
		/// Renders an HTML structure for displaying a subgrid of records that are not related to the source entity.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to retrieve the data.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="deferLoading">Indicates whether loading data should or should not occurr on startup.</param>
		/// <param name="enableActions">Determines whether actions (create, associate) are enabled or not.</param>
		/// <param name="modalDetailsFormSize">Size of the details form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDetailsFormCssClass">A CSS class applied to the details form's modal element.</param>
		/// <param name="modalDetailsFormTitle">Text displayed in the details form's modal title.</param>
		/// <param name="modalDetailsFormLoadingMessage">Message displayed in the details form's modal during loading.</param>
		/// <param name="modalDetailsFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDetailsFormTitleCssClass">A CSS class applied to the details form's modal title.</param>
		/// <param name="modalCreateFormSize">Size of the create form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalCreateFormCssClass">A CSS class applied to the create form's modal element.</param>
		/// <param name="modalCreateFormTitle">Text displayed in the create form's modal title.</param>
		/// <param name="modalCreateFormLoadingMessage">Message displayed in the create form's modal during loading.</param>
		/// <param name="modalCreateFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalCreateFormTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalEditFormSize">Size of the edit form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalEditFormCssClass">A CSS class applied to the edit form's modal element.</param>
		/// <param name="modalEditFormTitle">Text displayed in the edit form's modal title.</param>
		/// <param name="modalEditFormLoadingMessage">Message displayed in the edit form's modal during loading.</param>
		/// <param name="modalEditFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalEditFormTitleCssClass">A CSS class applied to the edit form's modal title.</param>
		/// <param name="modalDeleteSize">Size of the delete modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDeleteCssClass">A CSS class applied to the delete modal element.</param>
		/// <param name="modalDeleteTitle">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeleteBody">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeletePrimaryButtonText">Text displayed for the delete modal's primary button.</param>
		/// <param name="modalDeleteCloseButtonText">Text displayed for the delete modal's close button.</param>
		/// <param name="modalDeleteDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDeleteTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalErrorSize">Size of the error modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalErrorCssClass">CSS class that will be applied to the error modal element.</param>
		/// <param name="modalErrorTitle">Title text displayed in the error modal header.</param>
		/// <param name="modalErrorBody">Default content body text displayed in the error modal body.</param>
		/// <param name="modalErrorDismissButtonSrText">The text to display for the error modal dismiss button for screen readers only.</param>
		/// <param name="modalErrorCloseButtonText">Text to display for the error modal close button.</param>
		/// <param name="modalErrorTitleCssClass">CSS class assigned to the error modal title.</param>
		public static IHtmlString EntitySubGrid(this HtmlHelper html, List<ViewConfiguration> viewConfigurations,
			string serviceUrl, Entity user = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true, BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateRelatedRecordSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateRelatedRecordCssClass = null, string modalCreateRelatedRecordTitle = null,
			string modalCreateRelatedRecordLoadingMessage = null, string modalCreateRelatedRecordDismissButtonSrText = null,
			string modalCreateRelatedRecordTitleCssClass = null)
		{
			if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var grid = Grid(html, viewConfigurations, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode,
				selectColumnHeaderText, loadingMessage, errorMessage, accessDeniedMessage, emptyMessage, portalName, languageCode,
				deferLoading, enableActions, null, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass,
				modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass);

			return new HtmlString(grid.ToString());
		}

		/// <summary>
		/// Renders an HTML structure for displaying a subgrid of records that are related to the source entity as indicated by the relationship.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="source"><see cref="EntityReference"/> of the source entity that the subgrid records are related to</param>
		/// <param name="relationship"><see cref="Relationship"/> between the source entity and the related records listed in the subgrid.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to retrieve the data.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="cssClass">A CSS class attribute value that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">A CSS class attribute value that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="deferLoading">Indicates whether loading data should or should not occurr on startup.</param>
		/// <param name="enableActions">Determines whether actions (create, associate) are enabled or not.</param>
		/// <param name="modalDetailsFormSize">Size of the details form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDetailsFormCssClass">A CSS class applied to the details form's modal element.</param>
		/// <param name="modalDetailsFormTitle">Text displayed in the details form's modal title.</param>
		/// <param name="modalDetailsFormLoadingMessage">Message displayed in the details form's modal during loading.</param>
		/// <param name="modalDetailsFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDetailsFormTitleCssClass">A CSS class applied to the details form's modal title.</param>
		/// <param name="modalCreateFormSize">Size of the create form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalCreateFormCssClass">A CSS class applied to the create form's modal element.</param>
		/// <param name="modalCreateFormTitle">Text displayed in the create form's modal title.</param>
		/// <param name="modalCreateFormLoadingMessage">Message displayed in the create form's modal during loading.</param>
		/// <param name="modalCreateFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalCreateFormTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalEditFormSize">Size of the edit form's modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalEditFormCssClass">A CSS class applied to the edit form's modal element.</param>
		/// <param name="modalEditFormTitle">Text displayed in the edit form's modal title.</param>
		/// <param name="modalEditFormLoadingMessage">Message displayed in the edit form's modal during loading.</param>
		/// <param name="modalEditFormDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalEditFormTitleCssClass">A CSS class applied to the edit form's modal title.</param>
		/// <param name="modalDeleteSize">Size of the delete modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalDeleteCssClass">A CSS class applied to the delete modal element.</param>
		/// <param name="modalDeleteTitle">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeleteBody">Text displayed in the delete modal's title.</param>
		/// <param name="modalDeletePrimaryButtonText">Text displayed for the delete modal's primary button.</param>
		/// <param name="modalDeleteCloseButtonText">Text displayed for the delete modal's close button.</param>
		/// <param name="modalDeleteDismissButtonSrText">Text displayed for the modal's dismiss button for screen readers.</param>
		/// <param name="modalDeleteTitleCssClass">A CSS class applied to the create form's modal title.</param>
		/// <param name="modalErrorSize">Size of the error modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="modalErrorCssClass">CSS class that will be applied to the error modal element.</param>
		/// <param name="modalErrorTitle">Title text displayed in the error modal header.</param>
		/// <param name="modalErrorBody">Default content body text displayed in the error modal body.</param>
		/// <param name="modalErrorDismissButtonSrText">The text to display for the error modal dismiss button for screen readers only.</param>
		/// <param name="modalErrorCloseButtonText">Text to display for the error modal close button.</param>
		/// <param name="modalErrorTitleCssClass">CSS class assigned to the error modal title.</param>
		/// <param name="modalAssociateCssClass">A CSS class applied to the associate modal element.</param>
		/// <param name="modalAssociateTitle">Title text displayed in the associate records modal header. If viewConfiguration has an associate action enabled.</param>
		/// <param name="modalAssociateSelectedRecordsTitle">Text displayed in the associate records modal selected records heading. If viewConfiguration has an associate action enabled.</param>
		/// <param name="modalAssociatePrimaryButtonText">Text displyaed in the associate records modal primary button. If viewConfiguration has an associate action enabled.</param>
		/// <param name="modalAssociateCancelButtonText">Text displayed in the associate records modal cancel button. If viewConfiguration has an associate action enabled.</param>
		/// <param name="modalAssociateDismissButtonSrText">The text to display for the associate records modal dismiss button for screen readers only.</param>
		/// <param name="modalAssociateGridContainerCssClass">CSS class that will be applied to the outermost container element rendered for the grid in the associate modal.</param>
		/// <param name="modalAssociateGridCssClass">CSS class that will be applied to the associate modal grid's table element.</param>
		/// <param name="modalAssociateGridLoadingMessage">Message to be displayed during loading of the grid on the the associate modal.</param>
		/// <param name="modalAssociateGridErrorMessage">Message to be displayed if an error occurs loading the grid on the associate modal.</param>
		/// <param name="modalAssociateGridAccessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records of the grid on the associate modal.</param>
		/// <param name="modalAssociateGridEmptyMessage">Message to be displayed if there are no records found in the grid on the associate modal.</param>
		/// <param name="modalAssociateDefaultErrorMessage">Default message displayed if an unknown error occurs.</param>
		/// <param name="modalAssociateTitleCssClass">A CSS class applied to the associate records modal title.</param>
		/// <param name="modalAssociatePrimaryButtonCssClass">A CSS class applied to the associate records modal primary button.</param>
		/// <param name="modalAssociateCloseButtonCssClass">A CSS class applied to the associate records modal close button.</param>
		/// <param name="modalAssociateSize">Size of the associate records modal to create.</param>
		public static IHtmlString EntitySubGrid(this HtmlHelper html, EntityReference source, Relationship relationship,
			List<ViewConfiguration> viewConfigurations, string serviceUrl, Entity user = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true, BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null,
			string modalAssociateCssClass = null, string modalAssociateTitle = null, string modalAssociateSelectedRecordsTitle = null,
			string modalAssociatePrimaryButtonText = null, string modalAssociateCancelButtonText = null,
			string modalAssociateDismissButtonSrText = null, string modalAssociateTitleCssClass = null,
			string modalAssociatePrimaryButtonCssClass = null, string modalAssociateCloseButtonCssClass = null, string modalAssociateGridContainerCssClass = null,
			string modalAssociateGridCssClass = null, string modalAssociateGridLoadingMessage = null,
			string modalAssociateGridErrorMessage = null, string modalAssociateGridAccessDeniedMessage = null,
			string modalAssociateGridEmptyMessage = null, string modalAssociateDefaultErrorMessage = null,
			BootstrapExtensions.BootstrapModalSize modalAssociateSize = BootstrapExtensions.BootstrapModalSize.Large,
			BootstrapExtensions.BootstrapModalSize modalCreateRelatedRecordSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateRelatedRecordCssClass = null, string modalCreateRelatedRecordTitle = null,
			string modalCreateRelatedRecordLoadingMessage = null, string modalCreateRelatedRecordDismissButtonSrText = null,
			string modalCreateRelatedRecordTitleCssClass = null)
		{
			if (source == null || relationship == null || !viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var defaultViewConfiguration = viewConfigurations.First();

			var grid = SubGrid(html, viewConfigurations, source, relationship, serviceUrl, user, cssClass, gridCssClass,
				columnWidthStyle, selectMode, selectColumnHeaderText, loadingMessage, errorMessage, accessDeniedMessage,
				emptyMessage, portalName, languageCode, deferLoading, enableActions, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass, modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass);

			if (grid == null)
			{
				return new HtmlString(string.Empty);
			}

			grid.MergeAttribute("data-ref-entity", source.LogicalName);
			grid.MergeAttribute("data-ref-id", source.Id.ToString());
			grid.MergeAttribute("data-ref-rel", relationship.SchemaName);

			if (relationship.PrimaryEntityRole != null & relationship.PrimaryEntityRole.HasValue)
			{
				grid.MergeAttribute("data-ref-rel-role", relationship.PrimaryEntityRole.GetValueOrDefault().ToString());
			}

			var associateModal = html.AssociateModal(source, relationship, viewConfigurations,
				(defaultViewConfiguration.AssociateActionLink.URL != null) ? defaultViewConfiguration.AssociateActionLink.URL.PathWithQueryString : null, serviceUrl, user, null,
				string.Join(" ", new[] { "associate-lookup", modalAssociateCssClass }).TrimEnd(' '), modalAssociateTitle,
				modalAssociateSelectedRecordsTitle, modalAssociatePrimaryButtonText, modalAssociateCancelButtonText,
				modalAssociateDismissButtonSrText, modalAssociateGridContainerCssClass, modalAssociateGridCssClass,
				modalAssociateGridLoadingMessage, modalAssociateGridErrorMessage, modalAssociateGridAccessDeniedMessage,
				modalAssociateGridEmptyMessage, modalAssociateDefaultErrorMessage,
				modalAssociateTitleCssClass, modalAssociatePrimaryButtonCssClass, modalAssociateCloseButtonCssClass, portalName, languageCode, null, modalAssociateSize);

			grid.InnerHtml += associateModal;

			return new HtmlString(grid.ToString());
		}




		/// <summary>
		/// Render a list of records in a lookup records modal.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to complete the associate request.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		public static IHtmlString AssociateLookupEntityGrid(HtmlHelper html, List<ViewConfiguration> viewConfigurations,
			string serviceUrl, Entity user = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent,
			GridSelectMode selectMode = GridSelectMode.Multiple, string selectColumnHeaderText = null,
			string loadingMessage = null, string errorMessage = null, string accessDeniedMessage = null,
			string emptyMessage = null, string portalName = null, int languageCode = 0)
        {
            RefreshResourceStrings();

            if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var grid = Grid(html, viewConfigurations, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode,
				selectColumnHeaderText.GetValueOrDefault(_defaultGridSelectColumnHeaderText), loadingMessage, errorMessage,
				accessDeniedMessage, emptyMessage, portalName, languageCode, true, false);

			return new HtmlString(grid.ToString());
		}

		/// <summary>
		/// Render a list of records in a lookup records modal.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="serviceUrl">URL to the service to complete the associate request.</param>
		/// <param name="applyRelatedRecordFilter">Indicates whether or not the view's fetch should be modified to include filter condition with a related record value defined by filterRelationshipName, filterEntityName, filterAttributeName, filterValue. Default is false.</param>
		/// <param name="allowFilterOff">Indicates whether or not the user can turn off the related record filter. Default is false.</param>
		/// <param name="filterRelationshipName">Schema name of the N:1 relationship.</param>
		/// <param name="filterEntityName">Logical name of the entity to filter.</param>
		/// <param name="filterAttributeName">Name of the attribute that contains the ID of the record to filter. Client side code will use this to find the related lookup field to retrieve the filterValue prior to making the ajax request to get the records. This field must exist on the form.</param>
		/// <param name="toggleFilterText">Text to be displayed for the tooltip of the button that toggles the related record filter on and off if allowFilterOff is true.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="columnWidthStyle">Style of the column widths; Pixels or Percent</param>
		/// <param name="selectMode">Indicates whether rows are selectable and whether single or multiple is permitted.</param>
		/// <param name="selectColumnHeaderText">Text displayed for the select column.</param>
		/// <param name="loadingMessage">Message to be displayed during loading.</param>
		/// <param name="errorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="accessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="emptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="user">The current portal user contact.</param>
		public static IHtmlString LookupEntityGrid(HtmlHelper html, List<ViewConfiguration> viewConfigurations,
			string serviceUrl, Entity user = null, bool applyRelatedRecordFilter = false, bool allowFilterOff = false, string filterRelationshipName = null,
			string filterEntityName = null, string filterAttributeName = null, string toggleFilterText = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent,
			GridSelectMode selectMode = GridSelectMode.Multiple, string selectColumnHeaderText = null,
			string loadingMessage = null, string errorMessage = null, string accessDeniedMessage = null,
			string emptyMessage = null, string portalName = null, int languageCode = 0)
        {
            RefreshResourceStrings();

            if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new HtmlString(string.Empty);
			}

			var grid = Grid(html, viewConfigurations, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode,
				selectColumnHeaderText.GetValueOrDefault(_defaultGridSelectColumnHeaderText), loadingMessage, errorMessage,
				accessDeniedMessage, emptyMessage, portalName, languageCode, true, false, filterAttributeName);

			grid.MergeAttribute("data-apply-related-record-filter", applyRelatedRecordFilter.ToString().ToLower());
			grid.MergeAttribute("data-filter-relationship-name", filterRelationshipName);
			grid.MergeAttribute("data-filter-entity-name", filterEntityName);
			grid.MergeAttribute("data-filter-attribute-name", filterAttributeName);
			grid.MergeAttribute("data-allow-filter-off", allowFilterOff.ToString().ToLower());
			grid.MergeAttribute("data-toggle-filter-text", toggleFilterText.GetValueOrDefault(_defaultLookupEntityGridToggleFilterText));

			return new HtmlString(grid.ToString());
		}

		/// <summary>
		/// Renders an HTML structure for displaying a grid of records.
		/// </summary>
		private static TagBuilder Grid(HtmlHelper html, List<ViewConfiguration> viewConfigurations, string serviceUrl, Entity user = null,
			string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true, string filterAttributeName = null,
			BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateRelatedRecordSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateRelatedRecordCssClass = null, string modalCreateRelatedRecordTitle = null,
			string modalCreateRelatedRecordLoadingMessage = null, string modalCreateRelatedRecordDismissButtonSrText = null,
			string modalCreateRelatedRecordTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalWorkflowModalSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalWorkflowModalCssClass = null, string modalWorkflowModalTitle = null)
        {
            RefreshResourceStrings();

            if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new TagBuilder("div");
			}

			var layouts =
				viewConfigurations.Select(c =>
					{
						try
						{
							return new ViewLayout(c, null, portalName, languageCode, selectMode != GridSelectMode.None,
								enableActions && (c.ItemActionLinks != null && c.ItemActionLinks.Any()),
								selectColumnHeaderText.GetValueOrDefault(_defaultGridSelectColumnHeaderText));
						}
						catch (SavedQueryNotFoundException ex)
						{
							ADXTrace.Instance.TraceWarning(TraceCategory.Application, ex.Message);
							return null;
						}
					}).Where(l => l != null);

			return BuildGrid(html, layouts, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode, loadingMessage,
				errorMessage, accessDeniedMessage, emptyMessage, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass, deferLoading,
				enableActions, portalName, modalWorkflowModalSize, modalWorkflowModalCssClass, modalWorkflowModalTitle,
				modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass,
				filterAttributeName);
		}

		private static TagBuilder SubGrid(HtmlHelper html, List<ViewConfiguration> viewConfigurations, EntityReference source,
			Relationship relationship, string serviceUrl, Entity user = null, string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string selectColumnHeaderText = null, string loadingMessage = null, string errorMessage = null,
			string accessDeniedMessage = null, string emptyMessage = null, string portalName = null, int languageCode = 0,
			bool deferLoading = false, bool enableActions = true, BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateRelatedRecordSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateRelatedRecordCssClass = null, string modalCreateRelatedRecordTitle = null,
			string modalCreateRelatedRecordLoadingMessage = null, string modalCreateRelatedRecordDismissButtonSrText = null,
			string modalCreateRelatedRecordTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalWorkflowModalSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalWorkflowModalCssClass = null, string modalWorkflowModalTitle = null)
        {
            RefreshResourceStrings();

            if (!viewConfigurations.Any() || source == null || relationship == null)
			{
				return new TagBuilder("div");
			}

			var layouts =
				viewConfigurations.Select(c =>
					{
						try
						{
							return new SubgridViewLayout(c, source, relationship, c.EntityName, null, portalName, languageCode,
								selectMode != GridSelectMode.None,
								enableActions && (c.ItemActionLinks != null && c.ItemActionLinks.Any()),
								selectColumnHeaderText.GetValueOrDefault(_defaultGridSelectColumnHeaderText));
						}
						catch (SavedQueryNotFoundException ex)
						{
							ADXTrace.Instance.TraceWarning(TraceCategory.Application, ex.Message);
							return null;
						}
					}).Where(l => l != null);

			return BuildGrid(html, layouts, serviceUrl, user, cssClass, gridCssClass, columnWidthStyle, selectMode, loadingMessage,
				errorMessage, accessDeniedMessage, emptyMessage, modalDetailsFormSize, modalDetailsFormCssClass,
				modalDetailsFormTitle, modalDetailsFormLoadingMessage, modalDetailsFormDismissButtonSrText,
				modalDetailsFormTitleCssClass, modalCreateFormSize, modalCreateFormCssClass, modalCreateFormTitle,
				modalCreateFormLoadingMessage, modalCreateFormDismissButtonSrText, modalCreateFormTitleCssClass, modalEditFormSize,
				modalEditFormCssClass, modalEditFormTitle, modalEditFormLoadingMessage, modalEditFormDismissButtonSrText,
				modalEditFormTitleCssClass, modalDeleteSize, modalDeleteCssClass, modalDeleteTitle, modalDeleteBody,
				modalDeletePrimaryButtonText, modalDeleteCloseButtonText, modalDeleteDismissButtonSrText, modalDeleteTitleCssClass,
				modalErrorSize, modalErrorCssClass, modalErrorTitle, modalErrorBody, modalErrorDismissButtonSrText,
				modalErrorCloseButtonText, modalErrorTitleCssClass, deferLoading,
				enableActions, portalName, modalWorkflowModalSize, modalWorkflowModalCssClass, modalWorkflowModalTitle,
				modalCreateRelatedRecordSize, modalCreateRelatedRecordCssClass, modalCreateRelatedRecordTitle, modalCreateRelatedRecordLoadingMessage,
				modalCreateRelatedRecordDismissButtonSrText, modalCreateRelatedRecordTitleCssClass);
		}

		/// <summary>
		/// Renders an HTML structure for displaying a grid of records.
		/// </summary>
		private static TagBuilder BuildGrid(HtmlHelper html, IEnumerable<ViewLayout> viewLayouts, string serviceUrl, Entity user = null,
			string cssClass = null, string gridCssClass = null,
			GridColumnWidthStyle columnWidthStyle = GridColumnWidthStyle.Percent, GridSelectMode selectMode = GridSelectMode.None,
			string loadingMessage = null, string errorMessage = null, string accessDeniedMessage = null,
			string emptyMessage = null, BootstrapExtensions.BootstrapModalSize modalDetailsFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalDetailsFormCssClass = null, string modalDetailsFormTitle = null,
			string modalDetailsFormLoadingMessage = null, string modalDetailsFormDismissButtonSrText = null,
			string modalDetailsFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalCreateFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateFormCssClass = null, string modalCreateFormTitle = null,
			string modalCreateFormLoadingMessage = null, string modalCreateFormDismissButtonSrText = null,
			string modalCreateFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalEditFormSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalEditFormCssClass = null, string modalEditFormTitle = null, string modalEditFormLoadingMessage = null,
			string modalEditFormDismissButtonSrText = null, string modalEditFormTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalDeleteSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalDeleteCssClass = null, string modalDeleteTitle = null, string modalDeleteBody = null,
			string modalDeletePrimaryButtonText = null, string modalDeleteCloseButtonText = null,
			string modalDeleteDismissButtonSrText = null, string modalDeleteTitleCssClass = null,
			BootstrapExtensions.BootstrapModalSize modalErrorSize = BootstrapExtensions.BootstrapModalSize.Default,
			string modalErrorCssClass = null, string modalErrorTitle = null, string modalErrorBody = null,
			string modalErrorDismissButtonSrText = null, string modalErrorCloseButtonText = null,
			string modalErrorTitleCssClass = null, bool deferLoading = false, bool enableActions = true, string portalName = null,
			BootstrapExtensions.BootstrapModalSize modalWorkflowModalSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalWorkflowModalCssClass = null, string modalWorkflowModalTitle = null,
			BootstrapExtensions.BootstrapModalSize modalCreateRelatedRecordSize = BootstrapExtensions.BootstrapModalSize.Large,
			string modalCreateRelatedRecordCssClass = null, string modalCreateRelatedRecordTitle = null,
			string modalCreateRelatedRecordLoadingMessage = null, string modalCreateRelatedRecordDismissButtonSrText = null,
			string modalCreateRelatedRecordTitleCssClass = null,
			string filterAttributeName = null)
        {
            RefreshResourceStrings();

            var layouts = viewLayouts.ToList();

			if (!layouts.Any() || string.IsNullOrWhiteSpace(serviceUrl))
			{
				return new TagBuilder("div");
			}

			CloseIncidentActionLink closeCaseAction = null;
			ResolveCaseActionLink resolveCaseAction = null;
			ReopenCaseActionLink reopenCaseAction = null;
			CancelCaseActionLink cancelCaseAction = null;
			QualifyLeadActionLink qualifyLeadAction = null;
			ConvertQuoteToOrderActionLink convertQuoteAction = null;
			ConvertOrderToInvoiceActionLink convertOrderAction = null;
			CalculateOpportunityActionLink calculateOpportunityAction = null;
			DeactivateActionLink deactivateAction = null;
			ActivateActionLink activateAction = null;
			ActivateQuoteActionLink activateQuoteAction = null;
			SetOpportunityOnHoldActionLink setOpportunityOnHoldAction = null;
			ReopenOpportunityActionLink reopenOpportunityAction = null;
			WinOpportunityActionLink winOpportunityAction = null;
			LoseOpportunityActionLink loseOpportunityAction = null;
			GenerateQuoteFromOpportunityActionLink generateQuoteFromOpportunityAction = null;
			DisassociateActionLink disassociateAction = null;
			WorkflowActionLink workflowAction = null;
			List<CreateRelatedRecordActionLink> createRelatedRecordActionLinks = null;

			var defaultFilterDisplayNameChanged = false;
			var defaultFilterDisplayName = _defaultLookupEntityGridToggleFilterDisplayName;

			foreach (var layout in layouts)
			{
				if (layout.Configuration.CloseIncidentActionLink.Enabled)
				{
					closeCaseAction = layout.Configuration.CloseIncidentActionLink;
				}
				if (layout.Configuration.ResolveCaseActionLink.Enabled)
				{
					resolveCaseAction = layout.Configuration.ResolveCaseActionLink;
				}
				if (layout.Configuration.ReopenCaseActionLink.Enabled)
				{
					reopenCaseAction = layout.Configuration.ReopenCaseActionLink;
				}
				if (layout.Configuration.CancelCaseActionLink.Enabled)
				{
					cancelCaseAction = layout.Configuration.CancelCaseActionLink;
				}
				if (layout.Configuration.QualifyLeadActionLink.Enabled)
				{
					qualifyLeadAction = layout.Configuration.QualifyLeadActionLink;
				}
				if (layout.Configuration.ConvertQuoteToOrderActionLink.Enabled)
				{
					convertQuoteAction = layout.Configuration.ConvertQuoteToOrderActionLink;
				}
				if (layout.Configuration.ConvertOrderToInvoiceActionLink.Enabled)
				{
					convertOrderAction = layout.Configuration.ConvertOrderToInvoiceActionLink;
				}
				if (layout.Configuration.CalculateOpportunityActionLink.Enabled)
				{
					calculateOpportunityAction = layout.Configuration.CalculateOpportunityActionLink;
				}
				if (layout.Configuration.DeactivateActionLink.Enabled)
				{
					deactivateAction = layout.Configuration.DeactivateActionLink;
				}
				if (layout.Configuration.ActivateActionLink.Enabled)
				{
					activateAction = layout.Configuration.ActivateActionLink;
				}
				if (layout.Configuration.ActivateQuoteActionLink.Enabled)
				{
					activateQuoteAction = layout.Configuration.ActivateQuoteActionLink;
				}
				if (layout.Configuration.SetOpportunityOnHoldActionLink.Enabled)
				{
					setOpportunityOnHoldAction = layout.Configuration.SetOpportunityOnHoldActionLink;
				}
				if (layout.Configuration.ReopenOpportunityActionLink.Enabled)
				{
					reopenOpportunityAction = layout.Configuration.ReopenOpportunityActionLink;
				}
				if (layout.Configuration.WinOpportunityActionLink.Enabled)
				{
					winOpportunityAction = layout.Configuration.WinOpportunityActionLink;
				}
				if (layout.Configuration.LoseOpportunityActionLink.Enabled)
				{
					loseOpportunityAction = layout.Configuration.LoseOpportunityActionLink;
				}
				if (layout.Configuration.GenerateQuoteFromOpportunityActionLink.Enabled)
				{
					generateQuoteFromOpportunityAction = layout.Configuration.GenerateQuoteFromOpportunityActionLink;
				}
				if (layout.Configuration.DisassociateActionLink.Enabled)
				{
					disassociateAction = layout.Configuration.DisassociateActionLink;
				}
				var workflowItemAction = layout.Configuration.ItemActionLinks.FirstOrDefault(a => a.Type == LinkActionType.Workflow && a.Enabled);
				if (workflowItemAction != null)
				{
					workflowAction = workflowItemAction as WorkflowActionLink;
				}
				if (filterAttributeName != null)
				{
					var filteredColumnDisplayName =
						layout.Columns.Where(c => c.LogicalName == filterAttributeName).Select(c => c.Name).FirstOrDefault();

					if (!string.IsNullOrEmpty(filteredColumnDisplayName))
					{
						defaultFilterDisplayNameChanged = true;
						defaultFilterDisplayName = string.Format(defaultFilterDisplayName, filteredColumnDisplayName);
					}
				}

				if (layout.Configuration.CreateRelatedRecordActionLinks != null && layout.Configuration.CreateRelatedRecordActionLinks.Any())
				{
					if (createRelatedRecordActionLinks == null)
					{
						createRelatedRecordActionLinks = new List<CreateRelatedRecordActionLink>();
					}
					
					foreach (var item in layout.Configuration.CreateRelatedRecordActionLinks)
					{
						var link = item as CreateRelatedRecordActionLink;
						if (link != null)
						{
							createRelatedRecordActionLinks.Add(link);
						}
					}
				}
			}

			var json = JsonConvert.SerializeObject(layouts);
			var container = new TagBuilder("div");
			container.AddCssClass(string.Join(" ", new[] { "entity-grid", cssClass }).TrimEnd(' '));
			container.MergeAttribute("data-view-layouts", json);
			container.MergeAttribute("data-grid-class", gridCssClass);
			container.MergeAttribute("data-get-url", serviceUrl);
			container.MergeAttribute("data-selected-view", layouts.First().Id.ToString());
			container.MergeAttribute("data-defer-loading", deferLoading.ToString().ToLower());
			container.MergeAttribute("data-enable-actions", enableActions.ToString().ToLower());
			container.MergeAttribute("data-select-mode", selectMode.ToString());
			container.MergeAttribute("data-column-width-style", columnWidthStyle.ToString());

			if (!string.IsNullOrEmpty(filterAttributeName))
			{
				if (!defaultFilterDisplayNameChanged)
				{
					defaultFilterDisplayName = string.Empty;
				}
				container.MergeAttribute("data-toggle-filter-display-name", defaultFilterDisplayName);
			}

			if (user != null)
			{
				if (user.LogicalName == "contact")
				{
					var parentAccount = user.GetAttributeValue<EntityReference>("parentcustomerid");
					if (parentAccount != null)
					{
						container.MergeAttribute("data-user-parent-account-name", parentAccount.Name ?? string.Empty);
					}

					container.MergeAttribute("data-user-isauthenticated", "true");
				}
			}

			var grid = new TagBuilder("div");
			grid.AddCssClass("view-grid");
			container.InnerHtml += grid.ToString();

			var messageEmpty = new TagBuilder("div");
			messageEmpty.AddCssClass("view-empty message");
			if (!string.IsNullOrWhiteSpace(emptyMessage))
			{
				messageEmpty.InnerHtml = emptyMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-warning");
				message.InnerHtml = _defaultGridEmptyMessage;
				messageEmpty.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageEmpty.ToString();

			var messageAccessDenied = new TagBuilder("div");
			messageAccessDenied.AddCssClass("view-access-denied message");
			if (!string.IsNullOrWhiteSpace(accessDeniedMessage))
			{
				messageAccessDenied.InnerHtml = accessDeniedMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = _defaultGridAccessDeniedMessage;
				messageAccessDenied.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageAccessDenied.ToString();

			var messageError = new TagBuilder("div");
			messageError.AddCssClass("view-error message");
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				messageError.InnerHtml = errorMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = _defaultGridErrorMessage;
				messageError.InnerHtml = message.ToString();
			}

			container.InnerHtml += messageError.ToString();

			var messageLoading = new TagBuilder("div");
			messageLoading.AddCssClass("view-loading message text-center");
			messageLoading.InnerHtml = !string.IsNullOrWhiteSpace(loadingMessage)
				? loadingMessage
				: _defaultGridLoadingMessage;

			container.InnerHtml += messageLoading.ToString();

			var pagination = new TagBuilder("div");
			pagination.AddCssClass("view-pagination");
			pagination.MergeAttribute("data-pages", "1");
			pagination.MergeAttribute("data-pagesize", string.Empty);
			pagination.MergeAttribute("data-current-page", "1");

			container.InnerHtml += pagination.ToString();

			if (enableActions)
			{
				container.InnerHtml += html.CreateFormModal(modalCreateFormSize, modalCreateFormCssClass,
					modalCreateFormTitle.GetValueOrDefault(_defaultModalCreateFormTitle),
					modalCreateFormDismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText), modalCreateFormTitleCssClass,
					modalCreateFormLoadingMessage.GetValueOrDefault(DefaultModalCreateFormLoadingMessage), null, portalName);
				container.InnerHtml += html.EditFormModal(modalEditFormSize, modalEditFormCssClass,
					modalEditFormTitle.GetValueOrDefault(_defaultModalEditFormTitle),
					modalEditFormDismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText), modalEditFormTitleCssClass,
					modalEditFormLoadingMessage.GetValueOrDefault(DefaultModalEditFormLoadingMessage), null, portalName);
				container.InnerHtml += html.DetailsFormModal(modalDetailsFormSize, modalDetailsFormCssClass,
					modalDetailsFormTitle.GetValueOrDefault(_defaultModalDetailsFormTitle),
					modalDetailsFormDismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText), modalDetailsFormTitleCssClass,
					modalDetailsFormLoadingMessage.GetValueOrDefault(DefaultModalDetailsFormLoadingMessage), null, portalName);
				container.InnerHtml += html.DeleteModal(modalDeleteSize, modalDeleteCssClass,
					modalDeleteTitle.GetValueOrDefault(_defaultModalDeleteTitle),
					modalDeleteBody.GetValueOrDefault(_defaultModalDeleteBody),
					modalDeleteDismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText),
					modalDeletePrimaryButtonText.GetValueOrDefault(_defaultModalDeletePrimaryButtonText),
					modalDeleteCloseButtonText.GetValueOrDefault(_defaultModalDeleteCancelButtonText), modalDeleteTitleCssClass);

				if (createRelatedRecordActionLinks != null && createRelatedRecordActionLinks.Any())
				{
					foreach (var item in createRelatedRecordActionLinks)
					{
							var crrModal = item.Modal;
							container.InnerHtml += html.CreateRelatedRecordModal(crrModal.Size.Value, crrModal.CssClass,
								crrModal.Title.GetValueOrDefault(_defaultModalCreateRelatedRecordTitle),
								crrModal.DismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText), crrModal.TitleCssClass,
								crrModal.LoadingMessage.GetValueOrDefault(DefaultModalCreateFormLoadingMessage), null, portalName, item.FilterCriteriaId.ToString());
					}
				}

				if (workflowAction != null && workflowAction.Enabled && workflowAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml +=
						html.WorkflowModal(workflowAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
							workflowAction.Modal.CssClass, workflowAction.Modal.Title, workflowAction.Confirmation, workflowAction.Modal.DismissButtonSrText, workflowAction.Modal.PrimaryButtonText, workflowAction.Modal.CloseButtonText,
							workflowAction.Modal.TitleCssClass, workflowAction.Modal.PrimaryButtonCssClass, workflowAction.Modal.CloseButtonCssClass);
				}

				if (disassociateAction != null && disassociateAction.Enabled && disassociateAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.DissassociateModal(disassociateAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						disassociateAction.Modal.CssClass, disassociateAction.Modal.Title, disassociateAction.Confirmation, disassociateAction.Modal.DismissButtonSrText, disassociateAction.Modal.PrimaryButtonText, disassociateAction.Modal.CloseButtonText,
						disassociateAction.Modal.TitleCssClass, disassociateAction.Modal.PrimaryButtonCssClass, disassociateAction.Modal.CloseButtonCssClass);
				}

				if (qualifyLeadAction != null && qualifyLeadAction.Enabled && qualifyLeadAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.QualifyLeadModal(qualifyLeadAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						qualifyLeadAction.Modal.CssClass, qualifyLeadAction.Modal.Title, qualifyLeadAction.Confirmation,
						qualifyLeadAction.Modal.DismissButtonSrText, qualifyLeadAction.Modal.PrimaryButtonText, qualifyLeadAction.Modal.CloseButtonText, qualifyLeadAction.Modal.TitleCssClass, qualifyLeadAction.Modal.PrimaryButtonCssClass, qualifyLeadAction.Modal.CloseButtonCssClass);
				}

				if (closeCaseAction != null && closeCaseAction.Enabled && closeCaseAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml +=
						html.CloseCaseModal(closeCaseAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
							closeCaseAction.Modal.CssClass, closeCaseAction.Modal.Title, closeCaseAction.Confirmation, null, null,
							closeCaseAction.Modal.DismissButtonSrText, closeCaseAction.Modal.PrimaryButtonText, closeCaseAction.Modal.CloseButtonText,
							closeCaseAction.Modal.TitleCssClass, closeCaseAction.Modal.PrimaryButtonCssClass, closeCaseAction.Modal.CloseButtonCssClass);
				}

				if (resolveCaseAction != null && resolveCaseAction.Enabled)
				{
					container.InnerHtml += html.ResolveCaseModal(resolveCaseAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						resolveCaseAction.Modal.CssClass, resolveCaseAction.Modal.Title, resolveCaseAction.Confirmation,
						resolveCaseAction.SubjectLabel, resolveCaseAction.DescriptionLabel, resolveCaseAction.Modal.DismissButtonSrText, resolveCaseAction.Modal.PrimaryButtonText, resolveCaseAction.Modal.CloseButtonText, resolveCaseAction.Modal.TitleCssClass, resolveCaseAction.Modal.PrimaryButtonCssClass, resolveCaseAction.Modal.CloseButtonCssClass);
				}

				if (reopenCaseAction != null && reopenCaseAction.Enabled && reopenCaseAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ReopenCaseModal(reopenCaseAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenCaseAction.Modal.CssClass, reopenCaseAction.Modal.Title, reopenCaseAction.Confirmation,
						null, null, reopenCaseAction.Modal.DismissButtonSrText, reopenCaseAction.Modal.PrimaryButtonText, reopenCaseAction.Modal.CloseButtonText, reopenCaseAction.Modal.TitleCssClass, reopenCaseAction.Modal.PrimaryButtonCssClass, reopenCaseAction.Modal.CloseButtonCssClass);
				}

				if (cancelCaseAction != null && cancelCaseAction.Enabled && cancelCaseAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.CancelCaseModal(cancelCaseAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						cancelCaseAction.Modal.CssClass, cancelCaseAction.Modal.Title, cancelCaseAction.Confirmation,
						null, null, cancelCaseAction.Modal.DismissButtonSrText, cancelCaseAction.Modal.PrimaryButtonText, cancelCaseAction.Modal.CloseButtonText, cancelCaseAction.Modal.TitleCssClass, cancelCaseAction.Modal.PrimaryButtonCssClass, cancelCaseAction.Modal.CloseButtonCssClass);
				}

				if (convertQuoteAction != null && convertQuoteAction.Enabled && convertQuoteAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ConvertQuoteModal(convertQuoteAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertQuoteAction.Modal.CssClass, convertQuoteAction.Modal.Title, convertQuoteAction.Confirmation, convertQuoteAction.Modal.DismissButtonSrText, convertQuoteAction.Modal.PrimaryButtonText, convertQuoteAction.Modal.CloseButtonText, convertQuoteAction.Modal.TitleCssClass, convertQuoteAction.Modal.PrimaryButtonCssClass, convertQuoteAction.Modal.CloseButtonCssClass);
				}

				if (convertOrderAction != null && convertOrderAction.Enabled && convertOrderAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ConvertOrderModal(convertOrderAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						convertOrderAction.Modal.CssClass, convertOrderAction.Modal.Title, convertOrderAction.Confirmation, convertOrderAction.Modal.DismissButtonSrText, convertOrderAction.Modal.PrimaryButtonText, convertOrderAction.Modal.CloseButtonText, convertOrderAction.Modal.TitleCssClass, convertOrderAction.Modal.PrimaryButtonCssClass, convertOrderAction.Modal.CloseButtonCssClass);
				}

				if (calculateOpportunityAction != null && calculateOpportunityAction.Enabled && calculateOpportunityAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.CalculateOpportunityModal(calculateOpportunityAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						calculateOpportunityAction.Modal.CssClass, calculateOpportunityAction.Modal.Title, calculateOpportunityAction.Confirmation, calculateOpportunityAction.Modal.DismissButtonSrText, calculateOpportunityAction.Modal.PrimaryButtonText, calculateOpportunityAction.Modal.CloseButtonText,
						calculateOpportunityAction.Modal.TitleCssClass, calculateOpportunityAction.Modal.PrimaryButtonCssClass, calculateOpportunityAction.Modal.CloseButtonCssClass);
				}

				if (deactivateAction != null && deactivateAction.Enabled && deactivateAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.DeactivateModal(deactivateAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						deactivateAction.Modal.CssClass, deactivateAction.Modal.Title, deactivateAction.Confirmation, deactivateAction.Modal.DismissButtonSrText, deactivateAction.Modal.PrimaryButtonText, deactivateAction.Modal.CloseButtonText, deactivateAction.Modal.TitleCssClass, deactivateAction.Modal.PrimaryButtonCssClass, deactivateAction.Modal.CloseButtonCssClass);
				}

				if (activateAction != null && activateAction.Enabled && activateAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ActivateModal(activateAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateAction.Modal.CssClass, activateAction.Modal.Title, activateAction.Confirmation, activateAction.Modal.DismissButtonSrText, activateAction.Modal.PrimaryButtonText, activateAction.Modal.CloseButtonText, activateAction.Modal.TitleCssClass, activateAction.Modal.PrimaryButtonCssClass, activateAction.Modal.CloseButtonCssClass);
				}

				if (activateQuoteAction != null && activateQuoteAction.Enabled && activateQuoteAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ActivateQuoteModal(activateQuoteAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						activateQuoteAction.Modal.CssClass, activateQuoteAction.Modal.Title, activateQuoteAction.Confirmation, activateQuoteAction.Modal.DismissButtonSrText, activateQuoteAction.Modal.PrimaryButtonText, activateQuoteAction.Modal.CloseButtonText,
						activateQuoteAction.Modal.TitleCssClass, activateQuoteAction.Modal.PrimaryButtonCssClass, activateQuoteAction.Modal.CloseButtonCssClass);
				}

				if (setOpportunityOnHoldAction != null && setOpportunityOnHoldAction.Enabled && setOpportunityOnHoldAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.SetOpportunityOnHoldModal(setOpportunityOnHoldAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						setOpportunityOnHoldAction.Modal.CssClass, setOpportunityOnHoldAction.Modal.Title, setOpportunityOnHoldAction.Confirmation, setOpportunityOnHoldAction.Modal.DismissButtonSrText, setOpportunityOnHoldAction.Modal.PrimaryButtonText, setOpportunityOnHoldAction.Modal.CloseButtonText,
						setOpportunityOnHoldAction.Modal.TitleCssClass, setOpportunityOnHoldAction.Modal.PrimaryButtonCssClass, setOpportunityOnHoldAction.Modal.CloseButtonCssClass);
				}

				if (reopenOpportunityAction != null && reopenOpportunityAction.Enabled && reopenOpportunityAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.ReopenOpportunityModal(reopenOpportunityAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						reopenOpportunityAction.Modal.CssClass, reopenOpportunityAction.Modal.Title, reopenOpportunityAction.Confirmation, reopenOpportunityAction.Modal.DismissButtonSrText, reopenOpportunityAction.Modal.PrimaryButtonText, reopenOpportunityAction.Modal.CloseButtonText,
						reopenOpportunityAction.Modal.TitleCssClass, reopenOpportunityAction.Modal.PrimaryButtonCssClass, reopenOpportunityAction.Modal.CloseButtonCssClass);
				}

				if (winOpportunityAction != null && winOpportunityAction.Enabled && winOpportunityAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.WinOpportunityModal(winOpportunityAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						winOpportunityAction.Modal.CssClass, winOpportunityAction.Modal.Title, winOpportunityAction.Confirmation, winOpportunityAction.Modal.DismissButtonSrText, winOpportunityAction.Modal.PrimaryButtonText, winOpportunityAction.Modal.CloseButtonText,
						winOpportunityAction.Modal.TitleCssClass, winOpportunityAction.Modal.PrimaryButtonCssClass, winOpportunityAction.Modal.CloseButtonCssClass);
				}

				if (loseOpportunityAction != null && loseOpportunityAction.Enabled && loseOpportunityAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.LoseOpportunityModal(loseOpportunityAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						loseOpportunityAction.Modal.CssClass, loseOpportunityAction.Modal.Title, loseOpportunityAction.Confirmation, loseOpportunityAction.Modal.DismissButtonSrText, loseOpportunityAction.Modal.PrimaryButtonText, loseOpportunityAction.Modal.CloseButtonText,
						loseOpportunityAction.Modal.TitleCssClass, loseOpportunityAction.Modal.PrimaryButtonCssClass, loseOpportunityAction.Modal.CloseButtonCssClass);
				}

				if (generateQuoteFromOpportunityAction != null && generateQuoteFromOpportunityAction.Enabled && generateQuoteFromOpportunityAction.ShowModal == ShowModal.Yes)
				{
					container.InnerHtml += html.GenerateQuoteFromOpportunityModal(generateQuoteFromOpportunityAction.Modal.Size ?? BootstrapExtensions.BootstrapModalSize.Default,
						generateQuoteFromOpportunityAction.Modal.CssClass, generateQuoteFromOpportunityAction.Modal.Title, generateQuoteFromOpportunityAction.Confirmation, generateQuoteFromOpportunityAction.Modal.DismissButtonSrText, generateQuoteFromOpportunityAction.Modal.PrimaryButtonText, generateQuoteFromOpportunityAction.Modal.CloseButtonText,
						generateQuoteFromOpportunityAction.Modal.TitleCssClass, generateQuoteFromOpportunityAction.Modal.PrimaryButtonCssClass, generateQuoteFromOpportunityAction.Modal.CloseButtonCssClass);
				}
			}

			var modalError = html.ErrorModal(modalErrorSize, modalErrorCssClass, modalErrorTitle.GetValueOrDefault(_defaultErrorModalTitle),
				modalErrorBody.GetValueOrDefault(_defaultErrorModalBody),
				modalErrorDismissButtonSrText.GetValueOrDefault(_defaultModalDismissButtonSrText),
				modalErrorCloseButtonText.GetValueOrDefault(_defaultModalCloseButtonText),
				string.Join(" ", new[] { "text-danger", modalErrorTitleCssClass }).TrimEnd(' '));

			container.InnerHtml += modalError.ToString();

			return container;
		}

        private static void RefreshResourceStrings()
        {
            if (Equals(_currentCulture, Thread.CurrentThread.CurrentUICulture)) return;

            _currentCulture = Thread.CurrentThread.CurrentUICulture;
            _defaultModalCreateFormTitle = "<span class='fa fa-pencil-square-o' aria-hidden='true'></span> " + ResourceManager.GetString("Create_Text");
            _defaultModalCreateRelatedRecordTitle = "<span class='fa fa-pencil-square-o' aria-hidden='true'></span> " + ResourceManager.GetString("CreateRelatedRecord_Text");
            _defaultModalEditFormTitle = "<span class='fa fa-edit'></span> " + ResourceManager.GetString("Edit_Label");
            _defaultModalDetailsFormTitle = "<span class='fa fa-info-circle' aria-hidden='true'></span> " + ResourceManager.GetString("View_Details_Tooltip");
            _defaultModalDeleteTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");
            _defaultModalDeleteBody = ResourceManager.GetString("Record_Deletion_Confirmation_Message");
            _defaultModalDeletePrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
            _defaultModalDeleteCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
            _defaultErrorModalTitle = "<span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Error_ModalTitle");
            _defaultErrorModalBody = ResourceManager.GetString("Error_Occurred_Message");
            _defaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
            _defaultModalCloseButtonText = ResourceManager.GetString("Close_DefaultText");
            _defaultGridSelectColumnHeaderText = "<span class='fa fa-check' aria-hidden='true'></span> <span class='sr-only'>" + ResourceManager.GetString("Select_Column_Header_Text") + "</span>";
            _defaultGridLoadingMessage = "<span class='fa fa-spinner fa-spin' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Grid_Loading_Message");
            _defaultGridErrorMessage = ResourceManager.GetString("Error_Completing_Request_Error_Message") + "<span class='details'></span>";
            _defaultGridAccessDeniedMessage = ResourceManager.GetString("Access_Denied_No_Permissions_To_View_These_Records_Message");
            _defaultGridEmptyMessage = ResourceManager.GetString("Default_Grid_Empty_Message");
            _defaultLookupEntityGridToggleFilterText = ResourceManager.GetString("Toggle_Filter_Text");
            _defaultLookupEntityGridToggleFilterDisplayName = ResourceManager.GetString("Toggle_Filter_Display_Name");
        }
    }
}
