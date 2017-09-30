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
	public class WinOpportunityActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-square-o fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Close_As_Won_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Close_As_Won_Button_Text");

		public ViewWinOpportunityModal Modal { get; set; }

		public WinOpportunityActionLink()
		{
			Modal = new ViewWinOpportunityModal();
			Enabled = false;
		}

		public WinOpportunityActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			WinOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.WinOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.WinOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.WinOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.WinOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.WinOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.WinOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.WinOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.WinOpportunityDialog.Size;
			Modal.Title = formMetadata.WinOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.WinOpportunityDialog.TitleCssClass;
		}

		public WinOpportunityActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			WinOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.WinOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.WinOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.WinOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.WinOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.WinOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.WinOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.WinOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.WinOpportunityDialog.Size;
			Modal.Title = gridMetadata.WinOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.WinOpportunityDialog.TitleCssClass;
		}

		private WinOpportunityActionLink(IPortalContext portalContext, int languageCode, WinOpportunityAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.WinOpportunity, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewWinOpportunityModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("WinOpportunity", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
