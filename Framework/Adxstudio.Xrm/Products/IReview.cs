/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents a review with rating
	/// </summary>
	public interface IReview
	{
		/// <summary>
		/// The review content body
		/// </summary>
		string Content { get; }

		/// <summary>
		/// IP Address
		/// </summary>
		/// <remarks>HttpContext.Current.Request.UserHostName</remarks>
		string CreatedByIPAddress { get; }

		/// <summary>
		/// Username
		/// </summary>
		/// <remarks>HttpContext.Current.User.Identity.Name</remarks>
		string CreatedByUsername { get; }

		/// <summary>
		/// <see cref="Entity">Review Entity</see>
		/// </summary>
		Entity Entity { get; }

		/// <summary>
		/// <see cref="EntityReference">Review Entity Reference</see>
		/// </summary>
		EntityReference EntityReference { get; }

		/// <summary>
		/// <see cref="EntityReference">Product Entity Reference</see>
		/// </summary>
		EntityReference Product { get; }

		/// <summary>
		/// Indicates whether or not the review is to be published on the web.
		/// </summary>
		bool PublishToWeb { get; }

		/// <summary>
		/// The value of the reviewer's rating
		/// </summary>
		double Rating { get; }

		/// <summary>
		/// The rational value of the reviewer's rating
		/// </summary>
		double RatingRationalValue { get; }

		/// <summary>
		/// The maximum possible value of the reviewer's rating
		/// </summary>
		int RatingMaximumValue { get; }

		/// <summary>
		/// Indicate whether the reviewer would recommend the item reviewed or not.
		/// </summary>
		bool Recommend { get; }

		/// <summary>
		/// Contact record of the reviewer if not anonymous
		/// </summary>
		EntityReference ReviewerContact { get; }

		/// <summary>
		/// Email address of the reviewer
		/// </summary>
		string ReviewerEmail { get; }

		/// <summary>
		/// Name (Nickname) of the reviewer
		/// </summary>
		string ReviewerName { get; }

		/// <summary>
		/// Location of the reviewer
		/// </summary>
		string ReviewerLocation { get; }

		/// <summary>
		/// Date the review was submitted
		/// </summary>
		DateTime SubmittedOn { get; }

		/// <summary>
		/// Title of the review
		/// </summary>
		string Title { get; }
	}
}
