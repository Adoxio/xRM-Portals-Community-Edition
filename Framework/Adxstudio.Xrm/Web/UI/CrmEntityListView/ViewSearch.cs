/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
    /// <summary>
    /// Configuration of the search control
    /// </summary>
    public class ViewSearch : IViewSearch
    {
        private string _searchQueryStringParameterName;
        private string _buttonLabel;
        private string _placeholderText;
        private string _tooltipText;

        private readonly string buttonLabelConstant = "<span class='sr-only'>{0}</span><span class='fa fa-search' aria-hidden='true'></span>";

        /// <summary>
        /// Indicates whether the search control is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Query String parameter name for the search query.
        /// </summary>
        public string SearchQueryStringParameterName
        {
            get
            {
                return string.IsNullOrWhiteSpace(_searchQueryStringParameterName)
                    ? "query"
                    : _searchQueryStringParameterName;
            }
            set { _searchQueryStringParameterName = value; }
        }

        /// <summary>
        /// Gets or sets the text assigned as the placeholder on the search input field.
        /// </summary>
        public string PlaceholderText
        {
            get
            {
                return string.IsNullOrWhiteSpace(_placeholderText)
                    ? ResourceManager.GetString("Search_DefaultText")
                    : _placeholderText;
            }
            set { _placeholderText = value; }
        }

        /// <summary>
        /// Gets or sets the text assigned to the tooltip on the search input field.
        /// </summary>
        public string TooltipText
        {
            get
            {
                return string.IsNullOrWhiteSpace(_tooltipText)
                    ? ResourceManager.GetString("Use_Asterisk_Wildcard_Character_To_Search_Partial_Text")
                    : _tooltipText;
            }
            set { _tooltipText = value; }
        }

        /// <summary>
        /// Gets or sets the search button label.
        /// </summary>
        public string ButtonLabel
        {
            get
            {
                return string.IsNullOrWhiteSpace(_buttonLabel)
                    ? string.Format(buttonLabelConstant, ResourceManager.GetString("Search_DefaultText"))
                    : _buttonLabel;
            }
            set { _buttonLabel = value; }
        }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public ViewSearch()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="searchQueryStringParameterName"></param>
        /// <param name="placeholderText"></param>
        /// <param name="tooltipText"></param>
        /// <param name="buttonLabel"></param>
        public ViewSearch(bool enabled = false, string searchQueryStringParameterName = "query", string placeholderText = null, string tooltipText = "", string buttonLabel = null)
        {
            Enabled = enabled;
            SearchQueryStringParameterName = searchQueryStringParameterName;
            PlaceholderText = !string.IsNullOrWhiteSpace(placeholderText)
                ? placeholderText
                : ResourceManager.GetString("Search_DefaultText");
            TooltipText = tooltipText;
            ButtonLabel = !string.IsNullOrWhiteSpace(buttonLabel)
                ? buttonLabel
                : string.Format(buttonLabelConstant, ResourceManager.GetString("Search_DefaultText"));
        }

        /// <summary>
        /// Used by ViewConfiguration Class
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="searchQueryStringParameterName"></param>
        /// <param name="placeholderText"></param>
        /// <param name="tooltipText"></param>
        public ViewSearch(bool enabled, string searchQueryStringParameterName, string placeholderText, string tooltipText)
        {
            Enabled = enabled;
            SearchQueryStringParameterName = searchQueryStringParameterName;
            PlaceholderText = !string.IsNullOrWhiteSpace(placeholderText)
                ? placeholderText
                : ResourceManager.GetString("Search_DefaultText");
            TooltipText = !string.IsNullOrWhiteSpace(tooltipText)
                ? tooltipText 
                : string.Empty;
            ButtonLabel = string.Format(buttonLabelConstant, ResourceManager.GetString("Search_DefaultText"));
        }
    }
}
