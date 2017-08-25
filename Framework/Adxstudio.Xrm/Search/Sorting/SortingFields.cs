/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortingFields.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --
namespace Adxstudio.Xrm.Search.Sorting
{
	/// <summary>
	/// Sorting fields
	/// </summary>
	public class SortingFields
	{
		/// <summary>
		/// The rating
		/// </summary>
		private static string rating = "rating";

		/// <summary>
		/// The view count
		/// </summary>
		private static string viewCount = "knowledgearticleviews";

		/// <summary>
		/// The relevance
		/// </summary>
		private static string relevance = "relevance";

		/// <summary>
		/// Gets the rating.
		/// </summary>
		/// <value>
		/// The rating.
		/// </value>
		public static string Rating
		{
			get { return rating; }
		}

		/// <summary>
		/// Gets the view count.
		/// </summary>
		/// <value>
		/// The view count.
		/// </value>
		public static string ViewCount
		{
			get { return viewCount; }
		}

		/// <summary>
		/// Gets the relevance.
		/// </summary>
		/// <value>
		/// The relevance.
		/// </value>
		public static string Relevance
		{
			get { return relevance; }
		}
	}
}
