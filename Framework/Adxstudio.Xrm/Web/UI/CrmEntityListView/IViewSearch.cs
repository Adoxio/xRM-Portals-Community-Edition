/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Configuration of the search control
	/// </summary>
	public interface IViewSearch
	{
		/// <summary>
		/// Indicates whether the search control is enabled or not.
		/// </summary>
		bool Enabled { get; set; }
		/// <summary>
		/// Gets or sets the Query String parameter name for the search query.
		/// </summary>
		string SearchQueryStringParameterName { get; set; }
		/// <summary>
		/// Gets or sets the text assigned as the placeholder on the search input field.
		/// </summary>
		string PlaceholderText { get; set; }
		/// <summary>
		/// Gets or sets the text assigned to the tooltip on the search input field.
		/// </summary>
		string TooltipText { get; set; }
		/// <summary>
		/// Gets or sets the search button label.
		/// </summary>
		string ButtonLabel { get; set; }
	}
}
