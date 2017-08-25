/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Json.JsonConverter;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Web;
using Newtonsoft.Json;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Configuration of links for actions on a view.
	/// </summary>
	public class ViewActionLink : IViewActionLink
	{
		/// <summary>
		/// URL target of the action link
		/// </summary>
		[JsonConverter(typeof(UrlBuilderConverter))]
		public UrlBuilder URL { get; set; }
		/// <summary>
		/// Type of action
		/// </summary>
		public LinkActionType Type { get; set; }
		/// <summary>
		/// Display label
		/// </summary>
		public string Label { get; set; }
		/// <summary>
		/// Toolipt display text
		/// </summary>
		public string Tooltip { get; set; }
		/// <summary>
		/// The name of the Query String parameter containing the record id. Not applicable to Type 'Insert'.
		/// </summary>
		public string QueryStringIdParameterName { get; set; }
		/// <summary>
		/// True indicates the action is enabled otherwise disabled.
		/// </summary>
		public bool Enabled { get; set; }

		public string ButtonCssClass { get; set; }

		public string SuccessMessage { get; set; }

		public int? ActionIndex { get; set; }

		public ActionButtonAlignment? ActionButtonAlignment { get; set; }

		public ActionButtonStyle? ActionButtonStyle { get; set; }

		public ActionButtonPlacement? ActionButtonPlacement { get; set; }

		/// <summary>
		/// Confirmation message to be displayed prior to completing the delete action.
		/// </summary>
		public string Confirmation { get; set; }

		public ShowModal ShowModal { get; set; }

		/// <summary>
		/// Filter Criteria to show/hide button.
		/// </summary>
		public string FilterCriteria { get; set; }

		private Guid _filterCriteriaId;

		/// <summary>
		/// Gets or sets the filter criteria identifier.
		/// </summary>
		/// <value>
		/// The filter criteria identifier.
		/// </value>
		public Guid FilterCriteriaId
		{
			get
			{
				if (_filterCriteriaId == Guid.Empty)
				{
					_filterCriteriaId = Guid.NewGuid();
				}
				return _filterCriteriaId;
			}
			set { _filterCriteriaId = value; }
		}

		/// <summary>
		/// Text which displayed when button clicked.
		/// </summary>
		public string BusyText { get; set; }

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		public ViewActionLink() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">Type of action</param>
		/// <param name="enabled">Indicates if the action is enabled or not</param>
		/// <param name="url">URL target of the action link</param>
		/// <param name="label">Display label</param>
		/// <param name="tooltip">Tooltip display text</param>
		/// <param name="queryStringIdParameterName">The name of the Query String parameter containing the record id. Not applicable to Type 'Insert'.</param>
		public ViewActionLink(LinkActionType type, bool enabled, UrlBuilder url, string label, string tooltip, string queryStringIdParameterName)
		{
			Type = type;
			Enabled = enabled;
			URL = url;
			Label = label;
			Tooltip = tooltip;
			QueryStringIdParameterName = !string.IsNullOrWhiteSpace(queryStringIdParameterName) ? queryStringIdParameterName : "id";
		}

		public ViewActionLink(IPortalContext portalContext, int languageCode, Action action, LinkActionType type,
			bool enabled = false, UrlBuilder url = null, string portalName = null, string label = null, string tooltip = null, string busyText = null)
		{
			var buttonLabel = action.ButtonLabel.GetLocalizedString(languageCode);
			var buttonTooltip = action.ButtonTooltip.GetLocalizedString(languageCode);
			var buttonBusyText = action.ButtonBusyLabel.GetLocalizedString(languageCode);
			ActionButtonAlignment = action.ActionButtonAlignment;
			ActionButtonPlacement = action.ActionButtonPlacement;
			ActionButtonStyle = action.ActionButtonStyle;
			ActionIndex = action.ActionIndex;
			ButtonCssClass = action.ButtonCssClass;
			Confirmation = action.Confirmation.GetLocalizedString(languageCode);
			Enabled = enabled;
			this.FilterCriteria = action.FilterCriteria;
			Label = !string.IsNullOrWhiteSpace(buttonLabel) ? buttonLabel : label == null ? GetDefaultButtonLabel() : label;
			SuccessMessage = action.SuccessMessage.GetLocalizedString(languageCode);
			ShowModal = action.ShowModal ?? ShowModal.No;
			Tooltip = !string.IsNullOrWhiteSpace(buttonTooltip) ? buttonTooltip : tooltip == null ? GetDefaultButtonTooltip() : tooltip;
			Type = type;
			URL = url;
			BusyText = !string.IsNullOrEmpty(buttonBusyText) ? buttonBusyText : busyText;
		}

		protected virtual string GetDefaultButtonLabel()
		{
			return string.Empty;
		}

		protected virtual string GetDefaultButtonTooltip()
		{
			return string.Empty;
		}
	}
}
