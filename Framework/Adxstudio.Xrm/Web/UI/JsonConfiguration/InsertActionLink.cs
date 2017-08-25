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
	/// Link for insert action
	/// </summary>
	public class InsertActionLink : FormViewActionLink
	{
		/// <summary>
		/// Setting used to configure the modal when EntityForm has been specified.
		/// </summary>
		public ViewCreateFormModal Modal { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public InsertActionLink()
		{
			Modal = new ViewCreateFormModal();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url"></param>
		/// <param name="enabled">Indicates if the link is enabled or not.</param>
		/// <param name="label">Text displayed for the button.</param>
		/// <param name="tooltip">Text displayed for a tooltip.</param>
		public InsertActionLink(UrlBuilder url = null, bool enabled = false, string label = null, string tooltip = null)
			: base(LinkActionType.Insert, enabled, url, label, tooltip, string.Empty)
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
		public InsertActionLink(EntityReference entityForm, bool enabled = false, string label = null, string tooltip = null)
			: base(entityForm, LinkActionType.Insert, enabled, label, tooltip, string.Empty)
		{
			Initialize();
		}

		public InsertActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode, CreateAction action,
			bool enabled = false, string portalName = null, string label = null, string tooltip = null)
			: base(portalContext, languageCode, action, LinkActionType.Insert, enabled, portalName, label, tooltip)
		{
			Initialize();

			if (gridMetadata.CreateFormDialog == null) return;

			Modal.CloseButtonCssClass = gridMetadata.CreateFormDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.CreateFormDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.CreateFormDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.CreateFormDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.LoadingMessage = gridMetadata.CreateFormDialog.LoadingMessage.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.CreateFormDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = gridMetadata.CreateFormDialog.PrimaryButtonText.GetLocalizedString(languageCode);
			Modal.Size = gridMetadata.CreateFormDialog.Size;
			Modal.Title = gridMetadata.CreateFormDialog.Title.GetLocalizedString(languageCode);
			Modal.TitleCssClass = gridMetadata.CreateFormDialog.TitleCssClass;
		}

		private void Initialize()
		{
			var createText = ResourceManager.GetString("Create_Text");

			if (Label == null)
			{
				Label = string.Format("<span class='fa fa-plus-circle' aria-hidden='true'></span> {0}", createText);
			}

			if (Tooltip == null)
			{
				Tooltip = createText;
			}

			Modal = new ViewCreateFormModal();
		}
	}
}
