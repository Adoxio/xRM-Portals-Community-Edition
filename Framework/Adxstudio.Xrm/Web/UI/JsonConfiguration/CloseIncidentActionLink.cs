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
	public class CloseIncidentActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-circle fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Close_Case_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Close_Case_Button_Text");
		private static string DefaultResolutionLabel = ResourceManager.GetString("Case_Closed_DefaultResolutionLabel_Text");
		private static string DefaultResolutionDescriptionLabel = ResourceManager.GetString("Case_Closed_Via_Web_Portal_LabelText");

		public string DefaultResolution { get; set; }

		public string DefaultResolutionDescription { get; set; }

		public ViewCloseIncidentModal Modal { get; set; }

		public CloseIncidentActionLink()
		{
			Modal = new ViewCloseIncidentModal();
			Enabled = false;
		}

		public CloseIncidentActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			CloseIncidentAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.CloseIncidentDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.CloseIncidentDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.CloseIncidentDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.CloseIncidentDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.CloseIncidentDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.CloseIncidentDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.CloseIncidentDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.CloseIncidentDialog.Size;
			Modal.Title = formMetadata.CloseIncidentDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.CloseIncidentDialog.TitleCssClass;
		}

		public CloseIncidentActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			CloseIncidentAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.CloseIncidentDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.CloseIncidentDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.CloseIncidentDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.CloseIncidentDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.CloseIncidentDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.CloseIncidentDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.CloseIncidentDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.CloseIncidentDialog.Size;
			Modal.Title = gridMetadata.CloseIncidentDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.CloseIncidentDialog.TitleCssClass;
		}

		private CloseIncidentActionLink(IPortalContext portalContext, int languageCode, CloseIncidentAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.CloseIncident, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewCloseIncidentModal();
			
			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("CloseCase", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
			
			var defaultResolution = action.DefaultResolution;
			var defaultResolutionDescription = action.DefaultResolutionDescription;

			DefaultResolution = !string.IsNullOrWhiteSpace(defaultResolution) ? defaultResolution : DefaultResolutionLabel;
			DefaultResolutionDescription = !string.IsNullOrWhiteSpace(defaultResolutionDescription) ? defaultResolutionDescription : DefaultResolutionDescriptionLabel;
		}
	}
}
