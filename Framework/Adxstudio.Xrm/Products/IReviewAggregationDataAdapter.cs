/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Enumeration of the state of the review record.
	/// </summary>
	public enum ReviewState
	{
		/// <summary>
		/// Record is active.
		/// </summary>
		Active = 0,
		/// <summary>
		/// Record is not active.
		/// </summary>
		InActive = 1
	}

	/// <summary>
	/// Provides data operations on a given set of reviews.
	/// </summary>
	public interface IReviewAggregationDataAdapter
	{
		/// <summary>
		/// Select reviews.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IReview> SelectReviews();

		/// <summary>
		/// Select reviews.
		/// </summary>
		/// <param name="startRowIndex">Starting row index to begin selecting reviews.</param>
		/// <param name="maximumRows">Maximum number rows of records to return</param>
		/// <returns></returns>
		IEnumerable<IReview> SelectReviews(int startRowIndex, int maximumRows);

		/// <summary>
		/// Selects the total number of reviews.
		/// </summary>
		/// <returns></returns>
		int SelectReviewCount();
	}
}
