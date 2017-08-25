/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Link for edit action
	/// </summary>
	public class EditActionLink : FormViewActionLink
	{
		/// <summary>
		/// Setting used to configure the modal when EntityForm has been specified.
		/// </summary>
		public ViewEditFormModal Modal { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public EditActionLink()
		{
			Modal = new ViewEditFormModal();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">Url to the page containing the form.</param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string parameter that will contain the id of the record.</param>
		public EditActionLink(UrlBuilder url = null, bool enabled = false,
			string label = null, string tooltip = null,
			string queryStringIdParameterName = "id")
			: base(LinkActionType.Edit, enabled, url, label, tooltip, queryStringIdParameterName)
		{
			Initialize();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="entityForm"><see cref="EntityReference"/>Entity Form used to configure a form for display.</param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string parameter that will contain the id of the record.</param>
		public EditActionLink(EntityReference entityForm, bool enabled = false, string label = null,
			string tooltip = null, string queryStringIdParameterName = "id")
			: base(entityForm, LinkActionType.Edit, enabled, label, tooltip, queryStringIdParameterName)
		{
			Initialize();
		}

		public EditActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode, EditAction action,
			bool enabled = false, string portalName = null,
			string label = null, string tooltip = null,
			string queryStringIdParameterName = "id")
			: base(portalContext, languageCode, action, LinkActionType.Edit, enabled, portalName, label, tooltip)
		{
			Initialize();

			QueryStringIdParameterName = !string.IsNullOrWhiteSpace(action.RecordIdQueryStringParameterName)
				? action.RecordIdQueryStringParameterName
				: queryStringIdParameterName;

			if (gridMetadata.EditFormDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.EditFormDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.EditFormDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.EditFormDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.EditFormDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.LoadingMessage = gridMetadata.EditFormDialog.LoadingMessage.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.EditFormDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.EditFormDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.EditFormDialog.Size;
			Modal.Title = gridMetadata.EditFormDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.EditFormDialog.TitleCssClass;
		}

		private void Initialize()
		{
			var editText = ResourceManager.GetString("Edit_Label");

			if (Label == null)
			{
				Label = string.Format("<span class='fa fa-edit' aria-hidden='true'></span> {0}", editText);
			}

			if (Tooltip == null)
			{
				Tooltip = editText;
			}

			Modal = new ViewEditFormModal();
		}
	}
}
