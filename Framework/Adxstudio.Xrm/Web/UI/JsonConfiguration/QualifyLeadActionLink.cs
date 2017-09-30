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
	public class QualifyLeadActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Qualify_Button_Label_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Qualify_Button_Label_Text");

		public ViewQualifyLeadModal Modal { get; set; }

		public QualifyLeadActionLink()
		{
			Modal = new ViewQualifyLeadModal();
			Enabled = false;
		}

		public QualifyLeadActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			QualifyLeadAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.QualifyLeadDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.QualifyLeadDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.QualifyLeadDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.QualifyLeadDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.QualifyLeadDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.QualifyLeadDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.QualifyLeadDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.QualifyLeadDialog.Size;
			Modal.Title = formMetadata.QualifyLeadDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.QualifyLeadDialog.TitleCssClass;
		}

		public QualifyLeadActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			QualifyLeadAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.QualifyLeadDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.QualifyLeadDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.QualifyLeadDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.QualifyLeadDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.QualifyLeadDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.QualifyLeadDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.QualifyLeadDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.QualifyLeadDialog.Size;
			Modal.Title = gridMetadata.QualifyLeadDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.QualifyLeadDialog.TitleCssClass;
		}

		private QualifyLeadActionLink(IPortalContext portalContext, int languageCode, QualifyLeadAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.QualifyLead, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewQualifyLeadModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("QualifyLead", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
