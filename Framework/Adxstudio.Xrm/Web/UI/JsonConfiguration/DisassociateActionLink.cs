/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Link for disassociate action
	/// </summary>
	public class DisassociateActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-unlink' aria-hidden='true'></span> " + ResourceManager.GetString("Disassociate_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Disassociate_Button_Text");

		public ViewDisassociateModal Modal { get; set; }
		
		/// <summary>
		/// The relationship for the associate request
		/// </summary>
		public Relationship Relationship { get; set; }

		public DisassociateActionLink()
		{
			Modal = new ViewDisassociateModal();
			Enabled = false;
		}

		public DisassociateActionLink(Relationship relationship, IPortalContext portalContext, GridMetadata gridMetadata,
			int languageCode, DisassociateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(relationship, portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.DisassociateDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.DisassociateDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.DisassociateDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.DisassociateDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.DisassociateDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.DisassociateDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.DisassociateDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.DisassociateDialog.Size;
			Modal.Title = gridMetadata.DisassociateDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.DisassociateDialog.TitleCssClass;
		}

		/// <summary>
		/// Constructor used by ViewConfiguration
		/// </summary>
		public DisassociateActionLink(Relationship relationship, IPortalContext portalContext, int languageCode,
			DisassociateAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.Disassociate, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			Modal = new ViewDisassociateModal();

			Relationship = relationship;

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("Disassociate", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
