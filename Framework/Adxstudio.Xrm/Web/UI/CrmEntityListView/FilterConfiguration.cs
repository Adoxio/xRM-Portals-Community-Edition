/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Settings for displaying a metadata filter.
	/// </summary>
	public class FilterConfiguration
	{
		private string _filterQueryStringParameterName;

		public enum FilterOrientation
		{
			Horizontal = 756150000,
			Vertical = 756150001,
		}

		/// <summary>
		/// Indicates whether the filter control is enabled or not.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the Query String parameter name for the filter.
		/// </summary>
		public string FilterQueryStringParameterName
		{
			get
			{
				return string.IsNullOrWhiteSpace(_filterQueryStringParameterName) ? "mf" : _filterQueryStringParameterName;
			}
			set
			{
				_filterQueryStringParameterName = value;
			}
		}

		/// <summary>
		/// Specifies the general layout of items.
		/// </summary>
		public FilterOrientation? Orientation { get; set; }

		/// <summary>
		/// The definition of the filter options.
		/// </summary>
		public string Definition { get; set; }

		/// <summary>
		/// The apply filter button label.
		/// </summary>
		public string ApplyButtonLabel { get; set; }
	}
}
