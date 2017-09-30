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
	public class CalculateOpportunityActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-calculator fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Calculate_Value_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Calculate_Value_Button_Text");
		
		/// <summary>
		/// Setting used to configure the modal
		/// </summary>
		public ViewCalculateOpportunityModal Modal { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public CalculateOpportunityActionLink()
		{
			Modal = new ViewCalculateOpportunityModal();
			Enabled = false;
		}

		public CalculateOpportunityActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			CalculateOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.CalculateOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.CalculateOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.CalculateOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.CalculateOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.CalculateOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.CalculateOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.CalculateOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.CalculateOpportunityDialog.Size;
			Modal.Title = formMetadata.CalculateOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.CalculateOpportunityDialog.TitleCssClass;
		}

		public CalculateOpportunityActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			CalculateOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.CalculateOpportunityDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.CalculateOpportunityDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.CalculateOpportunityDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.CalculateOpportunityDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.CalculateOpportunityDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.CalculateOpportunityDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.CalculateOpportunityDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.CalculateOpportunityDialog.Size;
			Modal.Title = gridMetadata.CalculateOpportunityDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.CalculateOpportunityDialog.TitleCssClass;
		}

		private CalculateOpportunityActionLink(IPortalContext portalContext, int languageCode,
			CalculateOpportunityAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.CalculateOpportunity, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewCalculateOpportunityModal();

			URL = EntityListFunctions.BuildControllerActionUrl("CalculateActualValueOfOpportunity", "EntityAction", new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
