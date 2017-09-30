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
	public class ActivateQuoteActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-square-o fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Activate_Quote_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Activate_Quote_Button_Text");

		public ViewActivateModal Modal { get; set; }

		public ActivateQuoteActionLink()
		{
			Modal = new ViewActivateModal();
			Enabled = false;
		}

		public ActivateQuoteActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ActivateQuoteAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ActivateQuoteDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ActivateQuoteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ActivateQuoteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ActivateQuoteDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ActivateQuoteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ActivateQuoteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ActivateQuoteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ActivateQuoteDialog.Size;
			Modal.Title = formMetadata.ActivateQuoteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ActivateQuoteDialog.TitleCssClass;
		}

		public ActivateQuoteActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ActivateQuoteAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ActivateQuoteDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ActivateQuoteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ActivateQuoteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ActivateQuoteDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ActivateQuoteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ActivateQuoteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ActivateQuoteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ActivateQuoteDialog.Size;
			Modal.Title = gridMetadata.ActivateQuoteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ActivateQuoteDialog.TitleCssClass;
		}

		private ActivateQuoteActionLink(IPortalContext portalContext, int languageCode, ActivateQuoteAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.ActivateQuote, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewActivateModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ActivateQuote", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
