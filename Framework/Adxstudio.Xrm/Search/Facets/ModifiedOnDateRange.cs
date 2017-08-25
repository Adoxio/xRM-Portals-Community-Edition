/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModifiedOnDateRange.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Search.Facets
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Lucene.Net.Documents;

	using Resources;

	/// <summary>
	/// Supporting class for Date Modified facet.
	/// </summary>
	internal class ModifiedOnDateRange
	{

		/// <summary>
		/// Range for the facet view
		/// </summary>
		public static IEnumerable<ModifiedOnDateRange> Ranges
		{
			get
			{
				var today = DateTools.DateToString(DateTime.Today, DateTools.Resolution.DAY);
				yield return Create(today, today, ResourceManager.GetString("Facet_Today"));
				yield return Create(FromDays(7), today, ResourceManager.GetString("Facet_Past_Week"));
				yield return Create(FromDays(30), today, ResourceManager.GetString("Facet_Past_Month"));
				yield return Create(FromDays(90), today, string.Format(ResourceManager.GetString("Facet_Past_Months_String_Format"), 3));
				yield return Create(FromDays(180), today, string.Format(ResourceManager.GetString("Facet_Past_Months_String_Format"), 6));
				yield return new ModifiedOnDateRange("[* TO *]", ResourceManager.GetString("Facet_All"));
			}
		}

		/// <summary>
		/// Get ModifiedOnDateRange.DisplayName by the range name
		/// </summary>
		/// <param name="range">range name</param>
		/// <returns>display name for the range</returns>
		public static string GetRangeDisplayName(string range)
		{
			var currentRange = Ranges.FirstOrDefault(dateRange => dateRange.Name == range);
			return currentRange == null ? range : currentRange.DisplayName;
		}

		/// <summary>
		/// Create range
		/// </summary>
		/// <param name="startDate">start date</param>
		/// <param name="endDate">end date</param>
		/// <param name="displayName">display name</param>
		/// <returns> return object </returns>
		private static ModifiedOnDateRange Create(string startDate, string endDate, string displayName)
		{
			return new ModifiedOnDateRange(string.Format(Format, startDate, endDate), displayName);
		}

		/// <summary>
		/// Create date from days
		/// </summary>
		/// <param name="fromDays">days to subtract</param>
		/// <returns>string date</returns>
		private static string FromDays(int fromDays)
		{
			return DateTools.DateToString(DateTime.Today.Subtract(TimeSpan.FromDays(fromDays)), DateTools.Resolution.DAY);
		}

		/// <summary>
		/// Special format
		/// </summary>
		private static string Format
		{
			get { return "[{0} TO {1}]"; }
		}

		/// <summary>
		/// Calss name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Display class name
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ModifiedOnDateRange" /> class.
		/// </summary>
		/// <param name="name">date period name</param>
		/// <param name="displayName">display name</param>
		public ModifiedOnDateRange(string name, string displayName)
		{
			this.Name = name;
			this.DisplayName = displayName;
		}
	}
}
