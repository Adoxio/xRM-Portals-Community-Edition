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
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	/// <summary>
	/// View helpers for rendering a dialog to lookup records in Adxstudio Portals applications.
	/// </summary>
	public static class EntityLookupExtensions
	{
		private static string _defaultModalLookupTitle = ResourceManager.GetString("Lookup_Records_Title_Text");
		private static string _defaultModalLookupPrimaryButtonText = ResourceManager.GetString("Select_Column_Header_Text");
		private static string _defaultModalLookupCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static string _defaultModalLookupDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static string _defaultModalLookupNewButtonText = ResourceManager.GetString("Lookup_Form_Select_New");
		private static string _defaultModalLookupRemoveValueButtonText = ResourceManager.GetString("Remove_Value_Button_Text");
		private static string _defaultModalAssociateLookupTitle = ResourceManager.GetString("Lookup_Records_Title_Text");
		private static string _defaultModalAssociateLookupSelectedRecordsTitle = ResourceManager.GetString("Selected_Records_Title_Text");
		private static string _defaultModalAssociateLookupPrimaryButtonText = ResourceManager.GetString("Add_Button_Text");
		private static string _defaultModalAssociateLookupCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static string _defaultModalAssociateLookupDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static string _defaultGridSelectColumnHeaderText = "<span class='fa fa-check' aria-hidden='true'></span> <span class='sr-only'>" + ResourceManager.GetString("Select_Column_Header_Text") + "</span>";
		private static string _defaultErrorMessage = ResourceManager.GetString("Error_Occurred_Message");
        private static CultureInfo _currentCulture = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Render a bootstrap modal for a lookup records dialog to associate records.
        /// </summary>
        /// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
        /// <param name="target"><see cref="EntityReference"/> of the target entity that the selected entity records will be associated to.</param>
        /// <param name="relationship">The <see cref="Relationship"/> used to associate the target entity and the selected entity records.</param>
        /// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
        /// <param name="serviceUrl">URL to the service to complete the associate request.</param>
        /// <param name="gridServiceUrl">URL to the service to retrieve the data.</param>
        /// <param name="user">Current portal user contact.</param>
        /// <param name="id">HTML ID attribute value that will be applied to the modal container element.</param>
        /// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
        /// <param name="title">Title text displayed in the lookup records modal header.</param>
        /// <param name="selectedRecordsTitle">Text displayed in the lookup records modal selected records heading.</param>
        /// <param name="primaryButtonText">Text displayed for the primary button.</param>
        /// <param name="cancelButtonText">Text displayed for the cancel button.</param>
        /// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
        /// <param name="gridContainerCssClass">CSS class that will be applied to the grid's container element.</param>
        /// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
        /// <param name="gridLoadingMessage">Message to be displayed during loading.</param>
        /// <param name="gridErrorMessage">Message to be displayed if an error occurs.</param>
        /// <param name="gridAccessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
        /// <param name="gridEmptyMessage">Message to be displayed if there are no records found.</param>
        /// <param name="defaultErrorMessage">Default message displayed if an unknown error occurs.</param>
        /// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
        /// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
        /// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
        /// <param name="portalName">The name of the portal configuration that the control binds to.</param>
        /// <param name="languageCode">Language code used to retrieve localized labels.</param>
        /// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
        /// <param name="size">Size of the modal to create.</param>
        public static IHtmlString AssociateModal(this HtmlHelper html, EntityReference target,
			Relationship relationship, List<ViewConfiguration> viewConfigurations, string serviceUrl, string gridServiceUrl, Entity user = null,
			string id = null, string cssClass = null, string title = null, string selectedRecordsTitle = null,
			string primaryButtonText = null, string cancelButtonText = null, string dismissButtonSrText = null, string gridContainerCssClass = null,
			string gridCssClass = null, string gridLoadingMessage = null, string gridErrorMessage = null,
			string gridAccessDeniedMessage = null, string gridEmptyMessage = null, string defaultErrorMessage = null, 
			string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			string portalName = null, int languageCode = 0, IDictionary<string, string> htmlAttributes = null, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Large)
		{
            RefreshResourceStrings();

			if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(serviceUrl) || target == null || relationship == null)
			{
				return new HtmlString(string.Empty);
			}

			var container = new TagBuilder("div");
			container.AddCssClass(string.Join(" ", new[] { "entity-associate", cssClass }).TrimEnd(' '));
			container.MergeAttribute("data-url", serviceUrl);
			
			var jsonAssociateRequest =
				new JObject(
					new JProperty("Target",
						new JObject(new JProperty("LogicalName", target.LogicalName), new JProperty("Id", target.Id.ToString()))),
					new JProperty("Relationship",
						relationship.PrimaryEntityRole == null
							? new JObject(new JProperty("SchemaName", relationship.SchemaName))
							: new JObject(new JProperty("SchemaName", relationship.SchemaName),
								new JProperty("PrimaryEntityRole", relationship.PrimaryEntityRole))));

			container.MergeAttribute("data-associate", jsonAssociateRequest.ToString());

			var defaultViewConfiguration = viewConfigurations.First();

			if (defaultViewConfiguration.AssociateActionLink == null ||
				defaultViewConfiguration.AssociateActionLink.ViewConfigurations == null ||
				!defaultViewConfiguration.AssociateActionLink.ViewConfigurations.Any())
			{
				return new HtmlString(string.Empty);
			}

			var lookupEntityGridViewConfigurations = defaultViewConfiguration.AssociateActionLink.ViewConfigurations.ToList();

			var entityGrid = EntityGridExtensions.AssociateLookupEntityGrid(html, lookupEntityGridViewConfigurations, gridServiceUrl, user, cssClass, gridCssClass,
				EntityGridExtensions.GridColumnWidthStyle.Percent, EntityGridExtensions.GridSelectMode.Multiple, _defaultGridSelectColumnHeaderText,
				gridLoadingMessage, gridErrorMessage, gridAccessDeniedMessage, gridEmptyMessage, portalName, languageCode);

			var messageError = new TagBuilder("div");
			messageError.AddCssClass("modal-error message");
			messageError.Attributes.Add("aria-hidden", "true");
			if (!string.IsNullOrWhiteSpace(defaultErrorMessage))
			{
				messageError.InnerHtml = defaultErrorMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = _defaultErrorMessage;
				messageError.InnerHtml = message.ToString();
			}

			var lookupModalBody = messageError.ToString();

			lookupModalBody += entityGrid.ToString();
			
			var selectPanel = new TagBuilder("div");
			selectPanel.AddCssClass("panel panel-default");
			selectPanel.AddCssClass("content-panel");
			var panelHeading = new TagBuilder("div");
			panelHeading.AddCssClass("panel-heading");
			var h4 = new TagBuilder("h4")
			{
				InnerHtml = selectedRecordsTitle.GetValueOrDefault(_defaultModalAssociateLookupSelectedRecordsTitle)
			};
			panelHeading.InnerHtml = h4.ToString();
			selectPanel.InnerHtml = panelHeading.ToString();
			var panelBody = new TagBuilder("div");
			panelBody.AddCssClass("panel-body");
			panelBody.AddCssClass("selected-records");
			selectPanel.InnerHtml += panelBody.ToString();

			lookupModalBody += selectPanel.ToString();
			
			var lookupModal = html.BoostrapModal(size,
				title.GetValueOrDefault(_defaultModalAssociateLookupTitle), lookupModalBody, "modal-associate", id, false,
				dismissButtonSrText.GetValueOrDefault(_defaultModalAssociateLookupDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(_defaultModalAssociateLookupPrimaryButtonText),
				cancelButtonText.GetValueOrDefault(_defaultModalAssociateLookupCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);

			container.InnerHtml = lookupModal.ToString();
			
			return new HtmlString(container.ToString());
		}

		/// <summary>
		/// Render a bootstrap modal for a lookup records dialog to associate records.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="lookupDataFieldName">ID of the lookup attribute field that will contain the lookup entity reference.</param>
		/// <param name="viewConfigurations">Collection of <see cref="ViewConfiguration"/> required to retrieve a view and configure its display.</param>
		/// <param name="gridServiceUrl">URL to the service to retrieve the data.</param>
		/// <param name="user">Current portal user contact.</param>
		/// <param name="applyRelatedRecordFilter">Indicates whether or not the view's fetch should be modified to include filter condition with a related record value defined by filterRelationshipName, filterEntityName, filterAttributeName, filterValue. Default is false.</param>
		/// <param name="allowFilterOff">Indicates whether or not the user can turn off the related record filter. Default is false.</param>
		/// <param name="filterRelationshipName">Schema name of the N:1 relationship.</param>
		/// <param name="filterEntityName">Logical name of the entity to filter.</param>
		/// <param name="filterAttributeName">Name of the attribute that contains the ID of the record to filter. Client side code will use this to find the related lookup field to retrieve the filterValue prior to making the ajax request to get the records. This field must exist on the form.</param>
		/// <param name="id">HTML ID attribute value that will be applied to the modal container element.</param>
		/// <param name="cssClass">CSS class that will be applied to the outermost container element rendered by this helper.</param>
		/// <param name="title">Title text displayed in the lookup records modal header.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="removeValueButtonText">The text to display for the remove value button.</param>
		/// <param name="newValueButtonText">The text to display for the new value button</param>
		/// <param name="gridContainerCssClass">CSS class that will be applied to the grid's container element.</param>
		/// <param name="gridCssClass">CSS class that will be applied to the grid's table element.</param>
		/// <param name="gridLoadingMessage">Message to be displayed during loading.</param>
		/// <param name="gridErrorMessage">Message to be displayed if an error occurs.</param>
		/// <param name="gridAccessDeniedMessage">Message to be displayed if the user does not have the appropriate permissions to view the records.</param>
		/// <param name="gridEmptyMessage">Message to be displayed if there are no records found.</param>
		/// <param name="gridToggleFilterText">Text to be displayed for the tooltip of the button that toggles the related record filter on and off if allowFilterOff is true.</param>
		/// <param name="defaultErrorMessage">Default message displayed if an unknown error occurs.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		/// <param name="languageCode">Language code used to retrieve localized labels.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		/// <param name="size">Size of the modal to create.</param>
		/// <param name="lookupReferenceEntityFormId">Lookup reference entity formId</param>
		/// <param name="hasCreatePrivilege">Create privilege</param>
		public static IHtmlString LookupModal(this HtmlHelper html, string lookupDataFieldName,
			List<ViewConfiguration> viewConfigurations, string gridServiceUrl, Entity user = null, bool applyRelatedRecordFilter = false,
			bool allowFilterOff = false, string filterRelationshipName = null, string filterEntityName = null,
			string filterAttributeName = null, string id = null, string cssClass = null, string title = null,
			string primaryButtonText = null, string cancelButtonText = null, string dismissButtonSrText = null,
			string removeValueButtonText = null, string newValueButtonText = null, string gridContainerCssClass = null, string gridCssClass = null,
			string gridLoadingMessage = null, string gridErrorMessage = null, string gridAccessDeniedMessage = null,
			string gridEmptyMessage = null, string gridToggleFilterText = null, string defaultErrorMessage = null, string titleCssClass = null,
			string primaryButtonCssClass = null, string closeButtonCssClass = null, string portalName = null,
			int languageCode = 0, IDictionary<string, string> htmlAttributes = null, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Large,
			Guid? lookupReferenceEntityFormId = null, bool hasCreatePrivilege = false, string additionServiceUrl = null)
		{
            RefreshResourceStrings();

			if (!viewConfigurations.Any() || string.IsNullOrWhiteSpace(gridServiceUrl) ||
				string.IsNullOrWhiteSpace(lookupDataFieldName))
			{
				return new HtmlString(string.Empty);
			}

			var container = new TagBuilder("div");
			container.AddCssClass(string.Join(" ", new[] { "entity-lookup", cssClass }).TrimEnd(' '));
			container.MergeAttribute("data-url", gridServiceUrl);
			container.MergeAttribute("data-lookup-datafieldname", lookupDataFieldName);
			container.MergeAttribute("data-lookup-reference_entityformid", lookupReferenceEntityFormId.ToString());
			container.MergeAttribute("data-languageCode", languageCode.ToString());

			if (lookupDataFieldName == "entitlementid")
			{
				container.MergeAttribute("default-entitlements-url", additionServiceUrl);
			}

			var messageError = new TagBuilder("div");
			messageError.AddCssClass("modal-error message");
			messageError.Attributes.Add("aria-hidden", "true");
			if (!string.IsNullOrWhiteSpace(defaultErrorMessage))
			{
				messageError.InnerHtml = defaultErrorMessage;
			}
			else
			{
				var message = new TagBuilder("div");
				message.AddCssClass("alert alert-block alert-danger");
				message.InnerHtml = _defaultErrorMessage;
				messageError.InnerHtml = message.ToString();
			}

			var newValueButton = default(TagBuilder);
			var lookupCreateNewModal = default(IHtmlString);

			// User privilege check for Create New button
			if (hasCreatePrivilege)
			{
				newValueButton = new TagBuilder("button");
				newValueButton.AddCssClass("btn btn-default pull-left new-value");
				newValueButton.MergeAttribute("type", "button");
				newValueButton.MergeAttribute("title", ResourceManager.GetString("New_Record_Message"));
				newValueButton.SetInnerText(newValueButtonText.GetValueOrDefault(_defaultModalLookupNewButtonText));

				lookupCreateNewModal = html.CreateFormModal(size, "modal-lookup-create-form", ResourceManager.GetString("New_Record_Message"),
				dismissButtonSrText.GetValueOrDefault(_defaultModalLookupDismissButtonSrText), titleCssClass,
				null, null, portalName);
			}

			var removeValueButton = new TagBuilder("button");
			removeValueButton.AddCssClass("btn btn-default pull-right remove-value");
			removeValueButton.MergeAttribute("type", "button");
			removeValueButton.MergeAttribute("title", removeValueButtonText.GetValueOrDefault(_defaultModalLookupRemoveValueButtonText));
			removeValueButton.InnerHtml = removeValueButtonText.GetValueOrDefault(_defaultModalLookupRemoveValueButtonText);
			Dictionary<string, string> footerButtonCollection = new Dictionary<string, string>();
			footerButtonCollection["New"] = newValueButton == null ? string.Empty : newValueButton.ToString();
			footerButtonCollection["RemoveButton"] = removeValueButton.ToString();

			var lookupModalBody = messageError.ToString();

			var entityGrid = EntityGridExtensions.LookupEntityGrid(html, viewConfigurations, gridServiceUrl, user,
				applyRelatedRecordFilter, allowFilterOff, filterRelationshipName, filterEntityName, filterAttributeName, gridToggleFilterText, cssClass,
				gridCssClass, EntityGridExtensions.GridColumnWidthStyle.Percent, EntityGridExtensions.GridSelectMode.Single,
				_defaultGridSelectColumnHeaderText, gridLoadingMessage, gridErrorMessage, gridAccessDeniedMessage, gridEmptyMessage, portalName, languageCode);

			lookupModalBody += entityGrid.ToString();

			var lookupModal = html.BoostrapModal(size,
				title.GetValueOrDefault(_defaultModalLookupTitle), lookupModalBody, "modal-lookup", id, false,
				dismissButtonSrText.GetValueOrDefault(_defaultModalLookupDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(_defaultModalLookupPrimaryButtonText),
				cancelButtonText.GetValueOrDefault(_defaultModalLookupCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes, footerButtonCollection, false, true);

			container.InnerHtml = lookupModal.ToString() + (lookupCreateNewModal == null ? string.Empty : lookupCreateNewModal.ToString());

			return new HtmlString(container.ToString());
		}

	    private static void RefreshResourceStrings()
	    {
	        if (Equals(_currentCulture, Thread.CurrentThread.CurrentUICulture)) return;

            _currentCulture = Thread.CurrentThread.CurrentUICulture;
            _defaultModalLookupTitle = ResourceManager.GetString("Lookup_Records_Title_Text");
	        _defaultModalLookupPrimaryButtonText = ResourceManager.GetString("Select_Column_Header_Text");
	        _defaultModalLookupCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
	        _defaultModalLookupDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
	        _defaultModalLookupNewButtonText = ResourceManager.GetString("Lookup_Form_Select_New");
	        _defaultModalLookupRemoveValueButtonText = ResourceManager.GetString("Remove_Value_Button_Text");
	        _defaultModalAssociateLookupTitle = ResourceManager.GetString("Lookup_Records_Title_Text");
	        _defaultModalAssociateLookupSelectedRecordsTitle = ResourceManager.GetString("Selected_Records_Title_Text");
	        _defaultModalAssociateLookupPrimaryButtonText = ResourceManager.GetString("Add_Button_Text");
	        _defaultModalAssociateLookupCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
	        _defaultModalAssociateLookupDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
	        _defaultGridSelectColumnHeaderText = "<span class='fa fa-check' aria-hidden='true'></span> <span class='sr-only'>" + ResourceManager.GetString("Select_Column_Header_Text") + "</span>";
	        _defaultErrorMessage = ResourceManager.GetString("Error_Occurred_Message");
	    }
	}
}
