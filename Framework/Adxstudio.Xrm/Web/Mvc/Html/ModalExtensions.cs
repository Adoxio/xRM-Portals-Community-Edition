/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.Mvc.Html
{
	public static class ModalExtensions
	{
		private static readonly string DefaultModalCreateFormTitle = "<span class='fa fa-pencil-square-o' aria-hidden='true'></span> " + ResourceManager.GetString("Create_Text");
		private static readonly string DefaultModalCreateRelatedRecordTitle = "<span class='fa fa-plus-circle' aria-hidden='true'></span> " + ResourceManager.GetString("CreateRelatedRecord_Text");
		private const string DefaultModalCreateFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static readonly string DefaultModalEditFormTitle = "<span class='fa fa-edit' aria-hidden='true'></span> " + ResourceManager.GetString("Edit_Label");
		private const string DefaultModalEditFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static readonly string DefaultModalDetailsFormTitle = "<span class='fa fa-info-circle' aria-hidden='true'></span> " + ResourceManager.GetString("View_Details_Tooltip");
		private const string DefaultModalDetailsFormLoadingMessage = "<span class='fa fa-spinner fa-spin fa-4x' aria-hidden='true'></span>";
		private static readonly string DefaultModalDeleteTitle = "<span class='fa fa-trash-o' aria-hidden='true'></span> " + ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultModalDeleteBody = ResourceManager.GetString("Record_Deletion_Confirmation_Message");
		private static readonly string DefaultModalDeletePrimaryButtonText = ResourceManager.GetString("Delete_Button_Text");
		private static readonly string DefaultModalDeleteCancelButtonText = ResourceManager.GetString("Cancel_DefaultText");
		private static readonly string DefaultErrorModalTitle = "<span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + ResourceManager.GetString("Default_Error_ModalTitle");
		private static readonly string DefaultErrorModalBody = ResourceManager.GetString("Error_Occurred_Message");
		private static readonly string DefaultGenerateQuoteBody = ResourceManager.GetString("DefaultGenerateQuoteBody_Message");
		private static readonly string DefaultLoseOpportunityBody = ResourceManager.GetString("Close_Opportunity_As_Lost_Confirmation_Message");
		private static readonly string DefaultWinOpportunityBody = ResourceManager.GetString("Set_Opportunity_To_Close_Confirmation_Message");
		private static readonly string DefaultSetOnHoldBody = ResourceManager.GetString("Set_Opportunity_On_Hold_Confirmation_Message");
		private static readonly string DefaultActivateQuoteBody = ResourceManager.GetString("Quote_Activation_Confirmation_Message");
		private static readonly string DefaultDeactivateBody = ResourceManager.GetString("Record_Deactivation_Confirmation_Message");
		private static readonly string DefaultActivateBody = ResourceManager.GetString("Record_Activation_Confirmation_Message");
		private static readonly string DefaultCalculateBody = ResourceManager.GetString("DefaultCalculateBody_Message");
		private static readonly string DefaultConvertOrderBody = ResourceManager.GetString("Create_Invoice_Confirmation_Message");
		private static readonly string DefaultConvertQuoteBody = ResourceManager.GetString("Close_Quote_Create_Order_Confirmation_Message");
		private static readonly string DefaultCancelCaseBody = ResourceManager.GetString("Case_Cancellation_Confirmation_Message");
		private static readonly string DefaultReopenCaseBody = ResourceManager.GetString("DefaultReopenCaseBody_Message");
		private static readonly string DefaultCloseCaseBody = ResourceManager.GetString("Resolve_Case_Confirmation_Message");
		private static readonly string DefaultQualifyLeadBody = ResourceManager.GetString("Qualify_Lead_As_Opportunity_Confirmation_Message");
		private static readonly string DefaultModalRunWorkflowBody = ResourceManager.GetString("Run_Workflow_Confirmation_Message");
		private static readonly string DefaultModalDisassociateBody = ResourceManager.GetString("Record_Disassociation_Confirmation_Message");
		private static readonly string DefaultReopenOpportunityBody = ResourceManager.GetString("Reopen_Opportunity_Confirmation_Message");
		private static readonly string DefaultModalDismissButtonSrText = ResourceManager.GetString("Close_DefaultText");
		private static readonly string DefaultModalCloseButtonText = ResourceManager.GetString("Close_DefaultText");
		private const string DefaultSuccessButtonCss = "btn-success";
		private const string DefaultDangerButtonCss = "btn-danger";
		

		/// <summary>
		/// Render a bootstrap modal dialog for error messages.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal.</param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="body">Content body assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="closeButtonText">Text assigned to the close button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString ErrorModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null, string title = null,
			string body = null, string dismissButtonSrText = null, string closeButtonText = null,
			string titleCssClass = "text-danger", string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultErrorModalTitle),
				body.GetValueOrDefault(DefaultErrorModalBody), string.Join(" ", new[] { "modal-error", cssClass }).TrimEnd(' '), null,
				false, dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, true, false, null,
				closeButtonText.GetValueOrDefault(DefaultModalCloseButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		/// <summary>
		/// Render a bootstrap modal dialog for displaying a create form.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="loadingMessage">Loading message.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		public static IHtmlString CreateFormModal(this HtmlHelper html,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Large, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string titleCssClass = null, string loadingMessage = null,
			IDictionary<string, string> htmlAttributes = null, string portalName = null)
		{
			var loading = new TagBuilder("div");
			loading.AddCssClass("form-loading");
			loading.InnerHtml = loadingMessage.GetValueOrDefault(DefaultModalCreateFormLoadingMessage);

			var iframe = new TagBuilder("iframe");
			iframe.MergeAttribute("src", "about:blank");
			iframe.MergeAttribute("data-page", GetPortalModalFormTemplatePath(portalName));

			var body = loading + iframe.ToString();

			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultModalCreateFormTitle), body,
				string.Join(" ", new[] { "modal-form", "modal-form-insert", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), true, true, true, null, null, titleCssClass,
				null, null, htmlAttributes);
		}

		public static IHtmlString CreateRelatedRecordModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{

			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultModalCreateRelatedRecordTitle), body.GetValueOrDefault(ResourceManager.GetString("CreateRelatedRecord_Text")),
				string.Join(" ", new[] { "modal-form-createrecord", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(primaryButtonText),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultDangerButtonCss),
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString CreateRelatedRecordModal(this HtmlHelper html,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string titleCssClass = null, string loadingMessage = null,
			IDictionary<string, string> htmlAttributes = null, string portalName = null, string filterCriteriaId = null)
		{
			var loading = new TagBuilder("div");
			loading.AddCssClass("form-loading");
			loading.InnerHtml = loadingMessage.GetValueOrDefault(DefaultModalCreateFormLoadingMessage);

			var iframe = new TagBuilder("iframe");
			iframe.MergeAttribute("src", "about:blank");
			iframe.MergeAttribute("data-page", GetPortalModalFormTemplatePath(portalName));

			if (!string.IsNullOrEmpty(filterCriteriaId))
			{
				if (htmlAttributes == null)
				{
					htmlAttributes = new Dictionary<string, string>();
				}

				htmlAttributes.Add("data-filtercriteriaid", filterCriteriaId);
			}

			var body = loading + iframe.ToString();

			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultModalCreateRelatedRecordTitle), body,
				string.Join(" ", new[] { "modal-form", "modal-form-createrecord", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), true, true, true, null, null, titleCssClass,
				null, null, htmlAttributes);
		}

		/// <summary>
		/// Render a bootstrap modal dialog for displaying a read only details form.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="loadingMessage">Loading message.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		public static IHtmlString DetailsFormModal(this HtmlHelper html,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Large, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string titleCssClass = null, string loadingMessage = null,
			IDictionary<string, string> htmlAttributes = null, string portalName = null)
		{
			var loading = new TagBuilder("div");
			loading.AddCssClass("form-loading");
			loading.InnerHtml = loadingMessage.GetValueOrDefault(DefaultModalDetailsFormLoadingMessage);

			var iframe = new TagBuilder("iframe");
			iframe.MergeAttribute("src", "about:blank");
			iframe.MergeAttribute("data-page", GetPortalModalFormTemplatePath(portalName));

			var body = loading + iframe.ToString();

			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultModalDetailsFormTitle), body,
				string.Join(" ", new[] { "modal-form", "modal-form-details", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), true, true, true, null, null, titleCssClass,
				null, null, htmlAttributes);
		}

		/// <summary>
		/// Render a bootstrap modal dialog for displaying an edit form.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="loadingMessage">Loading message.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		/// <param name="portalName">The name of the portal configuration that the control binds to.</param>
		public static IHtmlString EditFormModal(this HtmlHelper html,
			BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null,
			string title = null, string dismissButtonSrText = null, string titleCssClass = null, string loadingMessage = null,
			IDictionary<string, string> htmlAttributes = null, string portalName = null)
		{
			var loading = new TagBuilder("div");
			loading.AddCssClass("form-loading");
			loading.InnerHtml = loadingMessage.GetValueOrDefault(DefaultModalEditFormLoadingMessage);

			var iframe = new TagBuilder("iframe");
			iframe.MergeAttribute("src", "about:blank");
			iframe.MergeAttribute("data-page", GetPortalModalFormTemplatePath(portalName));

			var body = loading + iframe.ToString();

			return html.BoostrapModal(size, title.GetValueOrDefault(DefaultModalEditFormTitle), body,
				string.Join(" ", new[] { "modal-form", "modal-form-edit", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), true, true, true, null, null, titleCssClass,
				null, null, htmlAttributes);
		}

		/// <summary>
		/// Render a bootstrap modal dialog for delete requests.
		/// </summary>
		/// <param name="html">Extension method target, provides support for HTML rendering and access to view context/data.</param>
		/// <param name="size">Size of the modal. <see cref="BootstrapExtensions.BootstrapModalSize"/></param>
		/// <param name="cssClass">CSS class assigned to the modal.</param>
		/// <param name="title">Title assigned to the modal.</param>
		/// <param name="body">Content body assigned to the modal.</param>
		/// <param name="dismissButtonSrText">The text to display for the dismiss button for screen readers only.</param>
		/// <param name="primaryButtonText">Text displayed for the primary button.</param>
		/// <param name="cancelButtonText">Text displayed for the cancel button.</param>
		/// <param name="titleCssClass">CSS class assigned to the title element in the header.</param>
		/// <param name="primaryButtonCssClass">CSS class assigned to the primary button.</param>
		/// <param name="closeButtonCssClass">CSS class assigned to the close button.</param>
		/// <param name="htmlAttributes">HTML Attributes assigned to the modal.</param>
		public static IHtmlString DeleteModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default, string cssClass = null, string title = null,
			string body = null, string dismissButtonSrText = null, string primaryButtonText = null, string cancelButtonText = null,
			string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(DefaultModalDeleteTitle), body.GetValueOrDefault(DefaultModalDeleteBody),
				string.Join(" ", new[] { "modal-delete", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(DefaultModalDeletePrimaryButtonText),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString QualifyLeadModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Qualify_Lead_Text")), body.GetValueOrDefault(DefaultQualifyLeadBody),
				string.Join(" ", new[] { "modal-qualify", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Qualify_Button_Label_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ResolveCaseModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string resolutionLabel = null, string descriptionLabel = null, string dismissButtonSrText = null,
			string primaryButtonText = null, string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			var defaultBody = GetDefaultResolveCaseBody(resolutionLabel, descriptionLabel);

			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Resolve_Case_DefaultText")), body.GetValueOrDefault(defaultBody.ToString()),
				string.Join(" ", new[] { "modal-resolvecase", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Resolve_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultSuccessButtonCss),
				closeButtonCssClass, htmlAttributes, null, true);
		}

		public static IHtmlString CloseCaseModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string resolutionLabel = null, string descriptionLabel = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Close_Case_Button_Text")), body.GetValueOrDefault(DefaultCloseCaseBody),
                string.Join(" ", new[] { "modal-closecase", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Close_DefaultText")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultSuccessButtonCss),
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ReopenCaseModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string resolutionLabel = null, string descriptionLabel = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Reopen_Case_DefaultText")), body.GetValueOrDefault(DefaultReopenCaseBody),
				string.Join(" ", new[] { "modal-reopencase", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Reopen_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass, closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString CancelCaseModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string resolutionLabel = null, string descriptionLabel = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Cancel_Case_DefaultText")), body.GetValueOrDefault(DefaultCancelCaseBody),
				string.Join(" ", new[] { "modal-cancelcase", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Cancel_Case_DefaultText")),
				cancelButtonText.GetValueOrDefault(ResourceManager.GetString("Cancel_Confirmation_No")), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultDangerButtonCss),
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ConvertQuoteModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Create_Order_Button_Text")), body.GetValueOrDefault(DefaultConvertQuoteBody),
				string.Join(" ", new[] { "modal-convert-quote", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Create_Order_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ConvertOrderModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Create_Invoice_Button_Text")), body.GetValueOrDefault(DefaultConvertOrderBody),
				string.Join(" ", new[] { "modal-convert-order", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Create_Invoice_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString CalculateOpportunityModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Calculate_Text")), body.GetValueOrDefault(DefaultCalculateBody),
				string.Join(" ", new[] { "modal-calculate", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Calculate_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString DeactivateModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Deactivate_Button_Text")), body.GetValueOrDefault(DefaultDeactivateBody),
				string.Join(" ", new[] { "modal-deactivate", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Deactivate_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ActivateModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Activate_Button_Text")), body.GetValueOrDefault(DefaultActivateBody),
				string.Join(" ", new[] { "modal-activate", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Activate_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ActivateQuoteModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Activate_Quote_Button_Text")), body.GetValueOrDefault(DefaultActivateQuoteBody),
				string.Join(" ", new[] { "modal-activate-quote", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Activate_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString SetOpportunityOnHoldModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Set_On_Hold_Button_Label_Text")), body.GetValueOrDefault(DefaultSetOnHoldBody),
				string.Join(" ", new[] { "modal-set-opportunity-on-hold", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Confirm_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString ReopenOpportunityModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Reopen_Opportunity_Button_Text")), body.GetValueOrDefault(DefaultReopenOpportunityBody),
				string.Join(" ", new[] { "modal-reopen-opportunity", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Confirm_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString WinOpportunityModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(string.Format("<span class='fa fa-check-square-o' aria-hidden='true'></span> {0}", ResourceManager.GetString("Close_As_Won_Button_Text"))), body.GetValueOrDefault(DefaultWinOpportunityBody),
				string.Join(" ", new[] { "modal-win-opportunity", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Close_As_Won_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultSuccessButtonCss),
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString LoseOpportunityModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(string.Format("<span aria-hidden='true' class='fa fa-ban'></span> {0}", ResourceManager.GetString("Close_As_Lost_Button_Label"))), body.GetValueOrDefault(DefaultLoseOpportunityBody),
				string.Join(" ", new[] { "modal-lose-opportunity", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Close_As_Lost_Button_Label")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultDangerButtonCss),
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString GenerateQuoteFromOpportunityModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
			string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Generate_Quote_Button_Label")), body.GetValueOrDefault(DefaultGenerateQuoteBody),
				string.Join(" ", new[] { "modal-generate-quote-from-opportunity", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Generate_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString UpdatePipelinePhaseModal(this HtmlHelper html, OptionMetadataCollection pipelinePhases, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
			string cssClass = null, string title = null, string body = null, string pipelinePhaseLabel = null, string descriptionLabel = null, string dismissButtonSrText = null,
			string primaryButtonText = null, string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
			IDictionary<string, string> htmlAttributes = null)
		{
			var defaultBody = GetDefaultUpdatePipelinePhaseBody(pipelinePhaseLabel, descriptionLabel, pipelinePhases);

			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Update_Pipeline_Phase_Button_Label_Text")), body.GetValueOrDefault(defaultBody.ToString()),
				string.Join(" ", new[] { "modal-updatepipelinephase", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Update_Button_Label_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass,
				primaryButtonCssClass.GetValueOrDefault(DefaultSuccessButtonCss),
				closeButtonCssClass, htmlAttributes, null, true);
		}

		public static IHtmlString WorkflowModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
	string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
	string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
	IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Run_Workflow_Button_Text")), body.GetValueOrDefault(DefaultModalRunWorkflowBody),
				string.Join(" ", new[] { "modal-run-workflow", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Proceed_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}

		public static IHtmlString DissassociateModal(this HtmlHelper html, BootstrapExtensions.BootstrapModalSize size = BootstrapExtensions.BootstrapModalSize.Default,
	string cssClass = null, string title = null, string body = null, string dismissButtonSrText = null, string primaryButtonText = null,
	string cancelButtonText = null, string titleCssClass = null, string primaryButtonCssClass = null, string closeButtonCssClass = null,
	IDictionary<string, string> htmlAttributes = null)
		{
			return html.BoostrapModal(size,
				title.GetValueOrDefault(ResourceManager.GetString("Disassociate_Button_Text")), body.GetValueOrDefault(DefaultModalDisassociateBody),
				string.Join(" ", new[] { "modal-disassociate", cssClass }).TrimEnd(' '), null, false,
				dismissButtonSrText.GetValueOrDefault(DefaultModalDismissButtonSrText), false, false, false,
				primaryButtonText.GetValueOrDefault(ResourceManager.GetString("Disassociate_Button_Text")),
				cancelButtonText.GetValueOrDefault(DefaultModalDeleteCancelButtonText), titleCssClass, primaryButtonCssClass,
				closeButtonCssClass, htmlAttributes);
		}


		private static TagBuilder GetDefaultResolveCaseBody(string resolutionLabel, string descriptionLabel)
		{
			var defaultBody = new TagBuilder("span");

			//Resolution input HTML

			var resolutionDiv = new TagBuilder("div");

			resolutionDiv.AddCssClass("form-group");

			var resolutionSpan = new TagBuilder("label");

			resolutionSpan.AddCssClass("col-sm-3");
			resolutionSpan.AddCssClass("control-label");
			resolutionSpan.AddCssClass("required");

			var resolutionLabelSpan = new TagBuilder("span");

			resolutionLabelSpan.InnerHtml += resolutionLabel;

			resolutionSpan.InnerHtml += resolutionLabelSpan;

			resolutionDiv.InnerHtml += resolutionSpan.ToString();

			var resolutionInputDiv = new TagBuilder("div");

			resolutionInputDiv.AddCssClass("col-sm-9");

			var resolutionInput = new TagBuilder("input");

			resolutionInput.Attributes["type"] = "text";
			resolutionInput.Attributes["aria-required"] = "true";

			resolutionInput.AddCssClass("form-control");
			resolutionInput.AddCssClass("required");
			resolutionInput.AddCssClass("resolution-input");

			resolutionInputDiv.InnerHtml += resolutionInput.ToString();

			resolutionDiv.InnerHtml += resolutionInputDiv.ToString();

			defaultBody.InnerHtml += resolutionDiv.ToString();

			//Resolution Description input HTML

			var resolutionDescriptionDiv = new TagBuilder("div");

			resolutionDescriptionDiv.AddCssClass("form-group");

			var resolutionDescriptionSpan = new TagBuilder("label");

			resolutionDescriptionSpan.AddCssClass("col-sm-3");
			resolutionDescriptionSpan.AddCssClass("control-label");
			resolutionDescriptionSpan.AddCssClass("required");

			var descriptionLabelSpan = new TagBuilder("span");

			descriptionLabelSpan.InnerHtml += descriptionLabel;

			resolutionDescriptionSpan.InnerHtml += descriptionLabelSpan;

			resolutionDescriptionDiv.InnerHtml += resolutionDescriptionSpan.ToString();

			var resolutionDescriptionInputDiv = new TagBuilder("div");

			resolutionDescriptionInputDiv.AddCssClass("col-sm-9");

			var resolutionDescriptionInput = new TagBuilder("textarea");

			resolutionDescriptionInput.Attributes["rows"] = "6";
			resolutionDescriptionInput.Attributes["cols"] = "20";
			resolutionDescriptionInput.Attributes["aria-required"] = "true";

			resolutionDescriptionInput.AddCssClass("form-control");
			resolutionDescriptionInput.AddCssClass("required");
			resolutionDescriptionInput.AddCssClass("resolution-description-input");

			resolutionDescriptionInputDiv.InnerHtml += resolutionDescriptionInput.ToString();

			resolutionDescriptionDiv.InnerHtml += resolutionDescriptionInputDiv.ToString();

			defaultBody.InnerHtml += resolutionDescriptionDiv.ToString();
			return defaultBody;
		}

		private static TagBuilder GetDefaultUpdatePipelinePhaseBody(string pipelinePhaseLabel, string descriptionLabel, OptionMetadataCollection pipelinePhases)
		{
			var defaultBody = new TagBuilder("span");

			//Resolution input HTML

			var pipelinePhaseDiv = new TagBuilder("div");

			pipelinePhaseDiv.AddCssClass("form-group");

			var pipelinePhaseSpan = new TagBuilder("label");

			pipelinePhaseSpan.AddCssClass("col-sm-3");
			pipelinePhaseSpan.AddCssClass("control-label");
			pipelinePhaseSpan.AddCssClass("required");

			var pipelinePhaseLabelSpan = new TagBuilder("span");

			pipelinePhaseLabelSpan.InnerHtml += pipelinePhaseLabel;

			pipelinePhaseSpan.InnerHtml += pipelinePhaseLabelSpan;

			pipelinePhaseDiv.InnerHtml += pipelinePhaseSpan.ToString();

			var pipelinePhaseInputDiv = new TagBuilder("div");

			pipelinePhaseInputDiv.AddCssClass("col-sm-9");

			var pipelinePhaseInput = new TagBuilder("select");

			//pipelinePhaseInput.Attributes["type"] = "text";
			pipelinePhaseInput.Attributes["aria-required"] = "true";

			pipelinePhaseInput.AddCssClass("form-control");
			pipelinePhaseInput.AddCssClass("required");
			pipelinePhaseInput.AddCssClass("pipelinephase-input");

			foreach (var option in pipelinePhases)
			{
				var li = new TagBuilder("option");

				li.Attributes["value"] = option.Value.Value.ToString();
				li.InnerHtml = option.Label.GetLocalizedLabelString();

				pipelinePhaseInput.InnerHtml += li.ToString();
			}

			pipelinePhaseInputDiv.InnerHtml += pipelinePhaseInput.ToString();

			pipelinePhaseDiv.InnerHtml += pipelinePhaseInputDiv.ToString();

			defaultBody.InnerHtml += pipelinePhaseDiv.ToString();

			//Resolution Description input HTML

			var resolutionDescriptionDiv = new TagBuilder("div");

			resolutionDescriptionDiv.AddCssClass("form-group");

			var resolutionDescriptionSpan = new TagBuilder("label");

			resolutionDescriptionSpan.AddCssClass("col-sm-3");
			resolutionDescriptionSpan.AddCssClass("control-label");
			resolutionDescriptionSpan.AddCssClass("required");

			var descriptionLabelSpan = new TagBuilder("span");

			descriptionLabelSpan.InnerHtml += descriptionLabel;

			resolutionDescriptionSpan.InnerHtml += descriptionLabelSpan;

			resolutionDescriptionDiv.InnerHtml += resolutionDescriptionSpan.ToString();

			var resolutionDescriptionInputDiv = new TagBuilder("div");

			resolutionDescriptionInputDiv.AddCssClass("col-sm-9");

			var resolutionDescriptionInput = new TagBuilder("textarea");

			resolutionDescriptionInput.Attributes["rows"] = "6";
			resolutionDescriptionInput.Attributes["cols"] = "20";
			resolutionDescriptionInput.Attributes["aria-required"] = "true";

			resolutionDescriptionInput.AddCssClass("form-control");
			resolutionDescriptionInput.AddCssClass("required");
			resolutionDescriptionInput.AddCssClass("resolution-description-input");

			resolutionDescriptionInputDiv.InnerHtml += resolutionDescriptionInput.ToString();

			resolutionDescriptionDiv.InnerHtml += resolutionDescriptionInputDiv.ToString();

			defaultBody.InnerHtml += resolutionDescriptionDiv.ToString();
			return defaultBody;
		}

		private static string GetPortalModalFormTemplatePath(string portalName)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
			var website = portal.Website;

			var http = HttpContext.Current;

			if (http == null) return null;

			var requestContext = new RequestContext(new HttpContextWrapper(http), new RouteData());

			VirtualPathData virtualPath;

			if (website == null)
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, "PortalModalFormTemplatePath", new RouteValueDictionary());
			}
			else
			{
				virtualPath = RouteTable.Routes.GetVirtualPath(requestContext, "PortalModalFormTemplatePath", new RouteValueDictionary
				{
					{ "__portalScopeId__", website.Id }
				});
			}
			
			var path =  virtualPath == null ? null : VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath);
			return path;
		}
	
	}
}
