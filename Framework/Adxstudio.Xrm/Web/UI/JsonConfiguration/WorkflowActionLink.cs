/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;
using System.Text.RegularExpressions;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Link for a workflow action
	/// </summary>
	public class WorkflowActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = ResourceManager.GetString("Run_Workflow_Button_Text");		

		/// <summary>
		/// Setting used to configure the modal
		/// </summary>
		public ViewWorkflowModal Modal { get; set; }

		/// <summary>
		/// Entity Reference to the Workflow that should be executed.
		/// </summary>
		public EntityReference Workflow { get; set; }

		/// <summary>
		/// Setting used to configure modal dialog with client attributes
		/// </summary>
		public bool CustomizeModal { get; set; }

		/// <summary>
		/// Parameterless Constructor
		/// </summary>
		public WorkflowActionLink()
		{
			Modal = new ViewWorkflowModal();
			Enabled = false;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="workflow">The entity reference to the workflow record.</param>
		/// <param name="url">Url to the service to complete the execute workflow request.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		public WorkflowActionLink(EntityReference workflow, UrlBuilder url, string label, string tooltip, bool enabled = true)
		{
			Workflow = workflow;
			Modal = new ViewWorkflowModal();
			Type = LinkActionType.Workflow;
			Enabled = enabled;
			URL = url;
			Label = label;
			Tooltip = tooltip;
			QueryStringIdParameterName = null;
		}

		public WorkflowActionLink(IPortalContext portalContext, EntityReference workflow, FormActionMetadata formMetadata,
			int languageCode, WorkflowAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, workflow, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.WorkflowDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.WorkflowDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.WorkflowDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.WorkflowDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.WorkflowDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.WorkflowDialog.PrimaryButtonCssClass;
			Modal.Size = formMetadata.WorkflowDialog.Size;
			Modal.TitleCssClass = formMetadata.WorkflowDialog.TitleCssClass;

			var customPrimaryButtonText = action.WorkflowDialogPrimaryButtonText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonText = !string.IsNullOrEmpty(customPrimaryButtonText)
				? customPrimaryButtonText
				: formMetadata.WorkflowDialog.PrimaryButtonText.GetLocalizedString(languageCode);

			var customCloseButtonTest = action.WorkflowDialogCloseButtonText.GetLocalizedString(languageCode);
			Modal.CloseButtonText = !string.IsNullOrEmpty(customCloseButtonTest)
				? customCloseButtonTest
				: formMetadata.WorkflowDialog.CloseButtonText.GetLocalizedString(languageCode);

			var customTitle = action.WorkflowDialogTitle.GetLocalizedString(languageCode);
			Modal.Title = !string.IsNullOrEmpty(customTitle)
				? customTitle
				: formMetadata.WorkflowDialog.Title.GetLocalizedString(languageCode);
		}

		public WorkflowActionLink(IPortalContext portalContext, EntityReference workflow, GridMetadata gridMetadata,
			int languageCode, WorkflowAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, workflow, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.WorkflowDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.WorkflowDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.WorkflowDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.WorkflowDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.WorkflowDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.WorkflowDialog.PrimaryButtonCssClass;
			Modal.Size = gridMetadata.WorkflowDialog.Size;
			Modal.TitleCssClass = gridMetadata.WorkflowDialog.TitleCssClass;

			var customPrimaryButtonText = action.WorkflowDialogPrimaryButtonText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonText = !string.IsNullOrEmpty(customPrimaryButtonText)
				? customPrimaryButtonText
				: gridMetadata.WorkflowDialog.PrimaryButtonText.GetLocalizedString(languageCode);

			var customCancelButtonText = action.WorkflowDialogCloseButtonText.GetLocalizedString(languageCode);
			Modal.CloseButtonText = !string.IsNullOrEmpty(customCancelButtonText)
				? customCancelButtonText
				: gridMetadata.WorkflowDialog.CloseButtonText.GetLocalizedString(languageCode);

			var customTitle = action.WorkflowDialogTitle.GetLocalizedString(languageCode);
			Modal.Title = !string.IsNullOrEmpty(customTitle)
				? customTitle
				: gridMetadata.WorkflowDialog.Title.GetLocalizedString(languageCode);

		}

		public WorkflowActionLink(IPortalContext portalContext, EntityReference workflow, int languageCode,
			WorkflowAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Workflow, enabled, url, portalName, DefaultButtonLabel, DefaultButtonLabel)
		{
			Modal = new ViewWorkflowModal();

			CustomizeModal = action.WorkflowDialogTitle != null || action.WorkflowDialogPrimaryButtonText != null || action.WorkflowDialogCloseButtonText != null;

			Workflow = workflow;

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ExecuteWorkflow", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });

			var buttonLabel = action.ButtonLabel.GetLocalizedString(languageCode);
			var buttonTooltip = action.ButtonTooltip.GetLocalizedString(languageCode);

			if (!string.IsNullOrWhiteSpace(buttonLabel) && !string.IsNullOrWhiteSpace(buttonTooltip) && action.WorkflowId != Guid.Empty) return;

			var wrkflow = portalContext.ServiceContext.CreateQuery("workflow")
				.FirstOrDefault(w => w.GetAttributeValue<Guid>("workflowid") == action.WorkflowId);

			if (wrkflow == null) return;
			
			if (string.IsNullOrWhiteSpace(buttonLabel)) Label = DefaultButtonLabel;
			
			// Try to extract the text only if the label has HTML
			string nohtmlLabel = Regex.Replace(Label, @"<[^>]*>", string.Empty);
			if (string.IsNullOrWhiteSpace(buttonTooltip)) Tooltip = nohtmlLabel;
		}
	}
}
