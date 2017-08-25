/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Helpers for assisting with sorting a saved query view.
	/// </summary>
	public static class ViewSort
	{
		private static readonly Regex SortExpressionPattern = new Regex(@"(?<name>\w+)\s*(?<direction>(asc|ascending|desc|descending))?\s*(,)?", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

		/// <summary>
		/// Direction of the ordering
		/// </summary>
		public enum Direction
		{
			/// <summary>
			/// Ascending sort order; ASC
			/// </summary>
			Ascending,
			/// <summary>
			/// Decending sort order; DESC
			/// </summary>
			Descending
		}
		
		/// <summary>
		/// Parse a sort expression into a collection of column names and sort directions.
		/// </summary>
		/// <param name="sortExpression"></param>
		public static IEnumerable<Tuple<string, Direction>> ParseSortExpression(string sortExpression)
		{
			if (string.IsNullOrEmpty(sortExpression))
			{
				return Enumerable.Empty<Tuple<string, Direction>>();
			}

			return SortExpressionPattern.Matches(sortExpression).Cast<Match>().Select(match =>
			{
				var sortNameCapture = match.Groups["name"].Value;

				var sortDirectionCapture = match.Groups["direction"].Value;

				var sortDirection = string.IsNullOrEmpty(sortDirectionCapture) || sortDirectionCapture.StartsWith("a", StringComparison.InvariantCultureIgnoreCase)
					? Direction.Ascending
					: Direction.Descending;

				return new Tuple<string, Direction>(sortNameCapture, sortDirection);

			}).Where(sort => sort != null).ToArray();
		}
	}
}
