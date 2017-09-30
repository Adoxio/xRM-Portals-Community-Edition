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
	public class ConvertOrderToInvoiceActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-clipboard fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Create_Invoice_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Create_Invoice_Button_Text");

		public ViewConvertOrderModal Modal { get; set; }

		public ConvertOrderToInvoiceActionLink()
		{
			Modal = new ViewConvertOrderModal();
			Enabled = false;
		}

		public ConvertOrderToInvoiceActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ConvertOrderToInvoiceAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ConvertOrderDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ConvertOrderDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ConvertOrderDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ConvertOrderDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ConvertOrderDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ConvertOrderDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ConvertOrderDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ConvertOrderDialog.Size;
			Modal.Title = formMetadata.ConvertOrderDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ConvertOrderDialog.TitleCssClass;
		}

		public ConvertOrderToInvoiceActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ConvertOrderToInvoiceAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ConvertOrderDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ConvertOrderDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ConvertOrderDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ConvertOrderDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ConvertOrderDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ConvertOrderDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ConvertOrderDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ConvertOrderDialog.Size;
			Modal.Title = gridMetadata.ConvertOrderDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ConvertOrderDialog.TitleCssClass;
		}

		private ConvertOrderToInvoiceActionLink(IPortalContext portalContext, int languageCode,
			ConvertOrderToInvoiceAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.ConvertOrder, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewConvertOrderModal();

			URL = EntityListFunctions.BuildControllerActionUrl("ConvertOrderToInvoice", "EntityAction",
				new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
