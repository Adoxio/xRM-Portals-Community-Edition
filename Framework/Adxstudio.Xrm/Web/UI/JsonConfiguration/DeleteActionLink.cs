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
	/// <summary>
	/// Link for delete action
	/// </summary>
	public class DeleteActionLink : RedirectActionLink
	{
		private static string DefaultButtonLabel = "<span class='fa fa-trash-o fa-fw' aria-hidden='true'></span>" + ResourceManager.GetString("Delete_Button_Text");
		private static string DefaultButtonTooltip = ResourceManager.GetString("Delete_Button_Text");
		private static string DefaultConfirmation = ResourceManager.GetString("Record_Deletion_Confirmation_Message");

		public ViewDeleteModal Modal { get; set; }

		public DeleteActionLink()
		{
			Modal = new ViewDeleteModal();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">URL to the service to complete the delete request.</param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string parameter that will contain the id of the record.</param>
		/// <param name="confirmation">Confirmation message to be displayed in the modal prompt before the delete request is attempted.</param>
		public DeleteActionLink(UrlBuilder url, bool enabled = false, string label = null, 
			string tooltip = null, string queryStringIdParameterName = "id", string confirmation = null)
		{
			Confirmation = confirmation == null ? DefaultConfirmation : confirmation;
			Modal = new ViewDeleteModal();
			Type = LinkActionType.Delete;
			Enabled = enabled;
			URL = url;
			Label = label == null ? DefaultButtonLabel : label;
			Tooltip = tooltip == null ? DefaultButtonTooltip : tooltip;
			QueryStringIdParameterName = queryStringIdParameterName;
		}

		public DeleteActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode, DeleteAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (gridMetadata.DeleteDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.DeleteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.DeleteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.DeleteDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.DeleteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.DeleteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.DeleteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.DeleteDialog.Size;
			Modal.Title = gridMetadata.DeleteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.DeleteDialog.TitleCssClass;
		}

		public DeleteActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode,
			DeleteAction action, bool enabled = true, UrlBuilder url = null, string portalName = null)
			: this(portalContext, languageCode, action, enabled, url, portalName)
		{
			if (formMetadata.DeleteDialog == null) return;

			Modal.CloseButtonCssClass = formMetadata.DeleteDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.DeleteDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.DeleteDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.DeleteDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.DeleteDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = formMetadata.DeleteDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = formMetadata.DeleteDialog.Size;
			Modal.Title = formMetadata.DeleteDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = formMetadata.DeleteDialog.TitleCssClass;
		}

		public DeleteActionLink(IPortalContext portalContext, int languageCode, DeleteAction action,
			bool enabled = true, UrlBuilder url = null, string portalName = null)
			: base(
				portalContext, languageCode, action, LinkActionType.Delete, enabled, url, portalName, DefaultButtonLabel,
				DefaultButtonTooltip)
		{
			if (string.IsNullOrWhiteSpace(Confirmation)) Confirmation = DefaultConfirmation;

			Modal = new ViewDeleteModal();

			if (url == null)
				URL = EntityListFunctions.BuildControllerActionUrl("Delete", "EntityGrid",
					new { area = "Portal", __portalScopeId__ = portalContext.Website.Id });
		}
	}
}
