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
	public class CancelCaseActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-times fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Cancel_Case_DefaultText");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Cancel_Case_DefaultText");

		public ViewCancelCaseModal Modal { get; set; }

		public CancelCaseActionLink()
		{
			Modal = new ViewCancelCaseModal();
			Enabled = false;
		}

		public CancelCaseActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			CancelCaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.CancelCaseDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.CancelCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.CancelCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.CancelCaseDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.CancelCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.CancelCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.CancelCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.CancelCaseDialog.Size;
			Modal.Title = formMetadata.CancelCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.CancelCaseDialog.TitleCssClass;
		}

		public CancelCaseActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			CancelCaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.CancelCaseDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.CancelCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.CancelCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.CancelCaseDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.CancelCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.CancelCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.CancelCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.CancelCaseDialog.Size;
			Modal.Title = gridMetadata.CancelCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.CancelCaseDialog.TitleCssClass;
		}

		public CancelCaseActionLink(IPortalContext portalContext, int languageCode, CancelCaseAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.CancelCase, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewCancelCaseModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("CancelCase", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
