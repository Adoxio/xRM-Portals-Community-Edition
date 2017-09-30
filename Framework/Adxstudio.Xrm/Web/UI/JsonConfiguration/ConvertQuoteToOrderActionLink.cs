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
	public class ConvertQuoteToOrderActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-clipboard fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Create_Order_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Create_Order_Button_Text");

		public ViewConvertQuoteModal Modal { get; set; }

		public ConvertQuoteToOrderActionLink()
		{
			Modal = new ViewConvertQuoteModal();
			Enabled = false;
		}

		public ConvertQuoteToOrderActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ConvertQuoteToOrderAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ConvertQuoteDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ConvertQuoteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ConvertQuoteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ConvertQuoteDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ConvertQuoteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ConvertQuoteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ConvertQuoteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ConvertQuoteDialog.Size;
			Modal.Title = formMetadata.ConvertQuoteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ConvertQuoteDialog.TitleCssClass;
		}

		public ConvertQuoteToOrderActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ConvertQuoteToOrderAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ConvertQuoteDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ConvertQuoteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ConvertQuoteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ConvertQuoteDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ConvertQuoteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ConvertQuoteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ConvertQuoteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ConvertQuoteDialog.Size;
			Modal.Title = gridMetadata.ConvertQuoteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ConvertQuoteDialog.TitleCssClass;
		}

		private ConvertQuoteToOrderActionLink(IPortalContext portalContext, int languageCode, ConvertQuoteToOrderAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.ConvertQuote, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewConvertQuoteModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ConvertQuoteToOrder", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
