/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	public class UpdatePipelinePhaseActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-circle fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Update_Pipeline_Phase_Button_Label_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Update_Pipeline_Phase_Button_Label_Text");
		private static string DefaultPipelinePhaseLabel = ResourceManager.GetString("Pipeline_Phase_Label_Text");
		private static string DefaultDescriptionLabel = ResourceManager.GetString("Description_Of_Update_Label_Text");

		public string PipelinePhaseLabel { get; set; }

		public string DescriptionLabel { get; set; }

		public ViewUpdatePipelinePhaseModal Modal { get; set; }

		public UpdatePipelinePhaseActionLink()
		{
			Modal = new ViewUpdatePipelinePhaseModal();
			Enabled = false;
		}

		public UpdatePipelinePhaseActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			UpdatePipelinePhaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.UpdatePipelinePhaseDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.UpdatePipelinePhaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.UpdatePipelinePhaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.UpdatePipelinePhaseDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.UpdatePipelinePhaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.UpdatePipelinePhaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.UpdatePipelinePhaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.UpdatePipelinePhaseDialog.Size;
			Modal.Title = formMetadata.UpdatePipelinePhaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.UpdatePipelinePhaseDialog.TitleCssClass;
		}

		public UpdatePipelinePhaseActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			UpdatePipelinePhaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.UpdatePipelinePhaseDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.UpdatePipelinePhaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.UpdatePipelinePhaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.UpdatePipelinePhaseDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.UpdatePipelinePhaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.UpdatePipelinePhaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.UpdatePipelinePhaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.UpdatePipelinePhaseDialog.Size;
			Modal.Title = gridMetadata.UpdatePipelinePhaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.UpdatePipelinePhaseDialog.TitleCssClass;
		}

		public UpdatePipelinePhaseActionLink(IPortalContext portalContext, int languageCode, UpdatePipelinePhaseAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.UpdatePipelinePhase, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewUpdatePipelinePhaseModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("UpdatePipelinePhase", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });

			var subjectLabel = action.StepNameLabel.GetLocalizedString(languageCode);
			var descriptionLabel = action.DescriptionLabel.GetLocalizedString(languageCode);

			PipelinePhaseLabel = !string.IsNullOrWhiteSpace(subjectLabel) ? subjectLabel : DefaultPipelinePhaseLabel;
			DescriptionLabel = !string.IsNullOrWhiteSpace(descriptionLabel) ? descriptionLabel : DefaultDescriptionLabel;
		}
	}
}
