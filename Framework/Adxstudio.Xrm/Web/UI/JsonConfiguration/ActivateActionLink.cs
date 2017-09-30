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
	public class ActivateActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-square-o fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Activate_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Activate_Button_Text");

		public ViewActivateModal Modal { get; set; }

		public ActivateActionLink()
		{
			Modal = new ViewActivateModal();
			Enabled = false;
		}

		public ActivateActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ActivateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ActivateDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ActivateDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ActivateDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ActivateDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ActivateDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ActivateDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ActivateDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ActivateDialog.Size;
			Modal.Title = formMetadata.ActivateDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ActivateDialog.TitleCssClass;
		}

		public ActivateActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ActivateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ActivateDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ActivateDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ActivateDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ActivateDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ActivateDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ActivateDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ActivateDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ActivateDialog.Size;
			Modal.Title = gridMetadata.ActivateDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ActivateDialog.TitleCssClass;
		}

		private ActivateActionLink(IPortalContext portalContext, int languageCode, ActivateAction action, bool enabled = true,
			UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.Activate, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewActivateModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("Activate", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
