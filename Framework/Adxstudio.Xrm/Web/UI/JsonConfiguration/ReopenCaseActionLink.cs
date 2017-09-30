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
	public class ReopenCaseActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-reply fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Reopen_Case_DefaultText");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Reopen_Case_DefaultText");

		public ViewReopenCaseModal Modal { get; set; }

		public ReopenCaseActionLink()
		{
			Modal = new ViewReopenCaseModal();
			Enabled = false;
		}

		public ReopenCaseActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ReopenCaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ReopenCaseDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ReopenCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ReopenCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ReopenCaseDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ReopenCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ReopenCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ReopenCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ReopenCaseDialog.Size;
			Modal.Title = formMetadata.ReopenCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ReopenCaseDialog.TitleCssClass;
		}

		public ReopenCaseActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ReopenCaseAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ReopenCaseDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ReopenCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ReopenCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ReopenCaseDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ReopenCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ReopenCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ReopenCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ReopenCaseDialog.Size;
			Modal.Title = gridMetadata.ReopenCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ReopenCaseDialog.TitleCssClass;
		}

		private ReopenCaseActionLink(IPortalContext portalContext, int languageCode, ReopenCaseAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.ReopenCase, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewReopenCaseModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ReopenCase", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
