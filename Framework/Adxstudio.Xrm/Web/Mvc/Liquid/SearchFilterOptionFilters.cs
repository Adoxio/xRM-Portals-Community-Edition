/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchFilterOptionFilters.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The servicing class for "search/filters"-site setting in Liquid templates (as example in 'Search"-web template).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	using System;
	using System.Linq;
	using DotLiquid;

	/// <summary>
	/// "SearchFilterOptionFilters" - class
	/// </summary>
	public static class SearchFilterOptionFilters
	{
		/// <summary>
		/// Gets a collection of search logical name filter options, parsed from "input" Site Setting (adx_sitesetting).
		/// </summary>
		/// <param name="input">
		/// The site setting value should be in the form of name/value pairs, with name and value separated by a colon, and pairs
		/// separated by a semicolon. For example: "Forums:adx_communityforum,adx_communityforumthread,adx_communityforumpost;Blogs:adx_blog,adx_blogpost,adx_blogpostcomment".
		/// </param>
		/// <returns>
		/// The List of <see cref="SearchFilterOptionDrop"/> or null.
		/// </returns>
		public static object SearchFilterOptions(object input)
		{
			var inputToString = Convert.ToString(input);
			if (string.IsNullOrEmpty(inputToString))
			{
				return null;
			}

			var splitedOptions = Html.SettingExtensions.SplitSearchFilterOptions(inputToString);

			return splitedOptions.Any() ? splitedOptions.Select(parsedOption => new SearchFilterOptionDrop(Extensions.LocalizeRecordTypeName(parsedOption.Value.Split(',')[0]), parsedOption.Value)) : null;
		}

		/// <summary>
		/// "SearchFilterOptionDrop" - class
		/// </summary>
		public class SearchFilterOptionDrop : Drop
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="SearchFilterOptionDrop" /> class.
			/// </summary>
			/// <param name="optionDisplayName">Display name of the option.</param>
			/// <param name="optionValue">The option value.</param>
			public SearchFilterOptionDrop(string optionDisplayName, string optionValue)
			{
				this.DisplayName = optionDisplayName;
				this.Value = optionValue;
			}

			/// <summary>
			/// Gets or sets the value.
			/// </summary>
			/// <value>
			/// The value.
			/// </value>
			public string Value { get; set; }

			/// <summary>
			/// Gets or sets the display name.
			/// </summary>
			/// <value>
			/// The display name.
			/// </value>
			public string DisplayName { get; set; }
		}
	}
}
