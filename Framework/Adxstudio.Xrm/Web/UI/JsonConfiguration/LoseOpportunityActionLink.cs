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
	public class LoseOpportunityActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-ban fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Close_As_Lost_Button_Label");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Close_As_Lost_Button_Label");

		public ViewLoseOpportunityModal Modal { get; set; }

		public LoseOpportunityActionLink()
		{
			Modal = new ViewLoseOpportunityModal();
			Enabled = false;
		}

		public LoseOpportunityActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			LoseOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.WinOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.LoseOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.LoseOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.LoseOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.LoseOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.LoseOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.LoseOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.LoseOpportunityDialog.Size;
			Modal.Title = formMetadata.LoseOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.LoseOpportunityDialog.TitleCssClass;
		}

		public LoseOpportunityActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			LoseOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.WinOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.LoseOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.LoseOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.LoseOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.LoseOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.LoseOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.LoseOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.LoseOpportunityDialog.Size;
			Modal.Title = gridMetadata.LoseOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.LoseOpportunityDialog.TitleCssClass;
		}

		private LoseOpportunityActionLink(IPortalContext portalContext, int languageCode, LoseOpportunityAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null) : base(portalContext, languageCode, action, LinkActionType.LoseOpportunity, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewLoseOpportunityModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("LoseOpportunity", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
