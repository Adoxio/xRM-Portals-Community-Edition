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
	public class ReopenOpportunityActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-reply fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Reopen_Opportunity_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Reopen_Opportunity_Button_Text");

		public ViewReopenOpportunityModal Modal { get; set; }

		public ReopenOpportunityActionLink()
		{
			Modal = new ViewReopenOpportunityModal();
			Enabled = false;
		}

		public ReopenOpportunityActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ReopenOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.ReopenOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ReopenOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ReopenOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ReopenOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ReopenOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ReopenOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ReopenOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ReopenOpportunityDialog.Size;
			Modal.Title = formMetadata.ReopenOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ReopenOpportunityDialog.TitleCssClass;
		}

		public ReopenOpportunityActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ReopenOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.ReopenOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ReopenOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ReopenOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ReopenOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ReopenOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ReopenOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ReopenOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ReopenOpportunityDialog.Size;
			Modal.Title = gridMetadata.ReopenOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ReopenOpportunityDialog.TitleCssClass;
		}

		private ReopenOpportunityActionLink(IPortalContext portalContext, int languageCode, ReopenOpportunityAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.ReopenOpportunity, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewReopenOpportunityModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ReopenOpportunity", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
