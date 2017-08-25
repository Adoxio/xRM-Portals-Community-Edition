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
	/// Link for details action
	/// </summary>
	public class DetailsActionLink : FormViewActionLink
	{
		/// <summary>
		/// Setting used to configure the modal when EntityForm has been specified.
		/// </summary>
		public ViewDetailsFormModal Modal { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public DetailsActionLink()
		{
			Initialize();
        }

		public DetailsActionLink(UrlBuilder url = null, bool enabled = false,
			string label = null, string tooltip = null,
			string queryStringIdParameterName = "id")
			: base(LinkActionType.Details, enabled, url, label, tooltip, queryStringIdParameterName)
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
		public DetailsActionLink(EntityReference entityForm, bool enabled = false,
			string label = null,
			string tooltip = null, string queryStringIdParameterName = "id")
			: base(entityForm, LinkActionType.Details, enabled, label, tooltip, queryStringIdParameterName)
		{
			Initialize();
        }

		/// <summary>
		/// Initializes the DetailsActionLink
		/// </summary>
		public void Initialize()
		{
			string viewDetailsText = ResourceManager.GetString("View_Details_Tooltip");
			if (Label == null) Label = string.Format("<span class='fa fa-info-circle' aria-hidden='true'></span> {0}", viewDetailsText);
			if (Tooltip == null) Tooltip = viewDetailsText;
			Modal = new ViewDetailsFormModal();
		}


		public DetailsActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode,
			DetailsAction action, bool enabled = false, string portalName = null,
			string label = null,
			string tooltip = null, string queryStringIdParameterName = "id")
			: base(portalContext, languageCode, action, LinkActionType.Details, enabled, portalName, label, tooltip)
		{
			Initialize();

			QueryStringIdParameterName = !string.IsNullOrWhiteSpace(action.RecordIdQueryStringParameterName)
				? action.RecordIdQueryStringParameterName
				: queryStringIdParameterName;

			if (gridMetadata.DetailsFormDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.DetailsFormDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.DetailsFormDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.DetailsFormDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.DetailsFormDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.LoadingMessage = gridMetadata.DetailsFormDialog.LoadingMessage.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.DetailsFormDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.DetailsFormDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.DetailsFormDialog.Size;
			Modal.Title = gridMetadata.DetailsFormDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.DetailsFormDialog.TitleCssClass;
		}
	}
}
