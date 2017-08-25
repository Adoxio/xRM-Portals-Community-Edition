/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Configuration of links for actions on a form or view.
	/// </summary>
	public class FormViewActionLink : ViewActionLink
	{
		/// <summary>
		/// Entity Reference to the Entity Form record used to define a form and configure its display.
		/// </summary>
		public EntityReference EntityForm { get; set; }

		/// <summary>
		/// Indicates the type of target, either Entity Form, Web Page, or URL.
		/// </summary>
		public TargetType? Target { get; set; }

		/// <summary>
		/// Entity Reference to the Web Page record to be redirect to instead of loading an entity form in a modal.
		/// </summary>
		public EntityReference WebPage { get; set; }

		public FormViewActionLink()
		{
		}

		public FormViewActionLink(LinkActionType type, bool enabled, UrlBuilder url, string label, string tooltip, string queryStringIdParameterName)
			: base(type, enabled, url, label, tooltip, queryStringIdParameterName)
		{
			Target = TargetType.Url;
		}

		public FormViewActionLink(EntityReference entityReference, LinkActionType type, bool enabled, string label, string tooltip, string queryStringIdParameterName)
			: base(type, enabled, null, label, tooltip, queryStringIdParameterName)
		{
			if (entityReference == null) return;
			switch (entityReference.LogicalName)
			{
				case "adx_webpage":
					WebPage = entityReference;
					Target = TargetType.WebPage;
					break;
				case "adx_entityform":
					EntityForm = entityReference;
					Target = TargetType.EntityForm;
					break;
			}
		}

		public FormViewActionLink(IPortalContext portalContext, int languageCode, FormViewAction action, LinkActionType type, bool enabled = false, string portalName = null, string label = null, string tooltip = null)
			: base(portalContext, languageCode, action, type, enabled, null, portalName, label, tooltip)
		{
			var targetType = action.TargetType.GetValueOrDefault(TargetType.EntityForm);
			string url = null;
			Enabled = enabled;
			Type = type;
			SuccessMessage = action.SuccessMessage.GetLocalizedString(languageCode);
			Target = targetType;
			var detailsButtonLabel = action.ButtonLabel.GetLocalizedString(languageCode);
			var detailsButtonTooltip = action.ButtonTooltip.GetLocalizedString(languageCode);
			Label = !string.IsNullOrWhiteSpace(detailsButtonLabel) ? detailsButtonLabel : label;
			Tooltip = !string.IsNullOrWhiteSpace(detailsButtonTooltip) ? detailsButtonTooltip : tooltip;
			
			switch (targetType)
			{
				case TargetType.EntityForm:
					if (action.EntityFormId != null)
					{
						EntityForm = new EntityReference("adx_entityform", action.EntityFormId.GetValueOrDefault());
					}
					return;
				case TargetType.WebPage:
					if (action.RedirectWebpageId != null)
					{
						url = action.GetRedirectWebPageUrl(portalContext, portalName);
					}
					break;
				case TargetType.Url:
					if (action.RedirectUrl != null)
					{
						url = action.RedirectUrl;
					}
					break;
			}

			if (string.IsNullOrEmpty(url)) return;

			var urlBuilder = new UrlBuilder(url);

			URL = urlBuilder;
		}
	}
}
