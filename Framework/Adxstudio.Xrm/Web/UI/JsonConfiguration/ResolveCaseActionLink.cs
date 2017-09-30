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
	public class ResolveCaseActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-check-circle fa-fw' aria-hidden='true'></span> " + ResourceManager.GetString("Resolve_Case_DefaultText");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Resolve_Case_DefaultText");
		private static string DefaultDescriptionLabel = ResourceManager.GetString("Description_Of_Resolution_Label_Text");
		private static string DefaultSubjectLabel = ResourceManager.GetString("Resolution_DefaultText");

		public string DescriptionLabel { get; set; }

		public ViewResolveCaseModal Modal { get; set; }

		public string SubjectLabel { get; set; }

		public ResolveCaseActionLink()
		{
			Modal = new ViewResolveCaseModal();
			Enabled = false;
		}

		public ResolveCaseActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			ResolveCaseAction resolveCaseAction, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, resolveCaseAction, enabled, url, portalName)
		{
			if (formMetadata.ResolveCaseDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.ResolveCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.ResolveCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.ResolveCaseDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.ResolveCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.ResolveCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.ResolveCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.ResolveCaseDialog.Size;
			Modal.Title = formMetadata.ResolveCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.ResolveCaseDialog.TitleCssClass;
		}

		public ResolveCaseActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			ResolveCaseAction resolveCaseAction, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, resolveCaseAction, enabled, url, portalName)
		{
			if (gridMetadata.ResolveCaseDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.ResolveCaseDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.ResolveCaseDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.ResolveCaseDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.ResolveCaseDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.ResolveCaseDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.ResolveCaseDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.ResolveCaseDialog.Size;
			Modal.Title = gridMetadata.ResolveCaseDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.ResolveCaseDialog.TitleCssClass;
		}

		private ResolveCaseActionLink(IPortalContext portalContext, int languageCode, ResolveCaseAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.ResolveCase, enabled, url, portalName,
				DefaultButtonLabel, DefaultButtonTooltip)
		{
			Modal = new ViewResolveCaseModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("ResolveCase", "EntityAction",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });

			var subjectLabel = action.SubjectLabel.GetLocalizedString(languageCode);
			var descriptionLabel = action.DescriptionLabel.GetLocalizedString(languageCode);

			SubjectLabel = !string.IsNullOrWhiteSpace(subjectLabel) ? subjectLabel : DefaultSubjectLabel;
			DescriptionLabel = !string.IsNullOrWhiteSpace(descriptionLabel) ? descriptionLabel : DefaultDescriptionLabel;
		}
	}
}
