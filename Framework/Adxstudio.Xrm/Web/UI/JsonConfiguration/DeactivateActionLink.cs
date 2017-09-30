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
	public class DeactivateActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-ban fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Deactivate_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Deactivate_Button_Text");

		public ViewDeactivateModal Modal { get; set; }

		public DeactivateActionLink()
		{
			Modal = new ViewDeactivateModal();
			Enabled = false;
		}

		public DeactivateActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			DeactivateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.DeactivateDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.DeactivateDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.DeactivateDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.DeactivateDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.DeactivateDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.DeactivateDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.DeactivateDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.DeactivateDialog.Size;
			Modal.Title = formMetadata.DeactivateDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.DeactivateDialog.TitleCssClass;
		}

		public DeactivateActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			DeactivateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.DeactivateDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.DeactivateDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.DeactivateDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.DeactivateDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.DeactivateDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.DeactivateDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.DeactivateDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.DeactivateDialog.Size;
			Modal.Title = gridMetadata.DeactivateDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.DeactivateDialog.TitleCssClass;
		}

		private DeactivateActionLink(IPortalContext portalContext, int languageCode, DeactivateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.Deactivate, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewDeactivateModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("Deactivate", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
