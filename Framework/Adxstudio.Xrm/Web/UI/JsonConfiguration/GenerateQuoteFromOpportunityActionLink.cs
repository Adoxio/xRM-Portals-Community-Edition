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
	public class GenerateQuoteFromOpportunityActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-clipboard fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Generate_Quote_Button_Label");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Generate_Quote_Button_Label");

		public ViewGenerateQuoteFromOpportunityModal Modal { get; set; }

		public GenerateQuoteFromOpportunityActionLink()
		{
			Modal = new ViewGenerateQuoteFromOpportunityModal();
			Enabled = false;
		}

		public GenerateQuoteFromOpportunityActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			GenerateQuoteFromOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.GenerateQuoteFromOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.GenerateQuoteFromOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.GenerateQuoteFromOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.GenerateQuoteFromOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.GenerateQuoteFromOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.GenerateQuoteFromOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.GenerateQuoteFromOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.GenerateQuoteFromOpportunityDialog.Size;
			Modal.Title = formMetadata.GenerateQuoteFromOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.GenerateQuoteFromOpportunityDialog.TitleCssClass;
		}

		public GenerateQuoteFromOpportunityActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			GenerateQuoteFromOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.GenerateQuoteFromOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.GenerateQuoteFromOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.GenerateQuoteFromOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.GenerateQuoteFromOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.GenerateQuoteFromOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.GenerateQuoteFromOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.GenerateQuoteFromOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.GenerateQuoteFromOpportunityDialog.Size;
			Modal.Title = gridMetadata.GenerateQuoteFromOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.GenerateQuoteFromOpportunityDialog.TitleCssClass;
		}

		private GenerateQuoteFromOpportunityActionLink(IPortalContext portalContext, int languageCode,
			GenerateQuoteFromOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.GenerateQuoteFromOpportunity, enabled, url, portalName,
				DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewGenerateQuoteFromOpportunityModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("GenerateQuoteFromOpportunity", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
