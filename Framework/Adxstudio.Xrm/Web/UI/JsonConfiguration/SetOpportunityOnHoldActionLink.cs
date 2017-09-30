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
	public class SetOpportunityOnHoldActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-exclamation-circle fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Set_On_Hold_Button_Label_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Set_On_Hold_Button_Label_Text");

		public ViewSetOpportunityOnHoldModal Modal { get; set; }

		public SetOpportunityOnHoldActionLink()
		{
			Modal = new ViewSetOpportunityOnHoldModal();
			Enabled = false;
		}

		public SetOpportunityOnHoldActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			SetOpportunityOnHoldAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.SetOpportunityOnHoldDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.SetOpportunityOnHoldDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.SetOpportunityOnHoldDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.SetOpportunityOnHoldDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.SetOpportunityOnHoldDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.SetOpportunityOnHoldDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.SetOpportunityOnHoldDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.SetOpportunityOnHoldDialog.Size;
			Modal.Title = formMetadata.SetOpportunityOnHoldDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.SetOpportunityOnHoldDialog.TitleCssClass;
		}

		public SetOpportunityOnHoldActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			SetOpportunityOnHoldAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.SetOpportunityOnHoldDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.SetOpportunityOnHoldDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.SetOpportunityOnHoldDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.SetOpportunityOnHoldDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.SetOpportunityOnHoldDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.SetOpportunityOnHoldDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.SetOpportunityOnHoldDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.SetOpportunityOnHoldDialog.Size;
			Modal.Title = gridMetadata.SetOpportunityOnHoldDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.SetOpportunityOnHoldDialog.TitleCssClass;
		}

		private SetOpportunityOnHoldActionLink(IPortalContext portalContext, int languageCode,
			SetOpportunityOnHoldAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.SetOpportunityOnHold, enabled, url, portalName, DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewSetOpportunityOnHoldModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("SetOpportunityOnHold", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
