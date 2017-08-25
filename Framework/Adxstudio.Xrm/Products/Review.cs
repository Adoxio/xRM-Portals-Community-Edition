/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Core.Flighting;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents full, extended info about a review.
	/// </summary>
	public class Review : IReview
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review entity</param>
		public Review(Entity review)
		{
			if (review == null) throw new ArgumentNullException("review");
			if (review.LogicalName != "adx_review") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), review.LogicalName), "review");

			Entity = review;
			EntityReference = review.ToEntityReference();
			Product = review.GetAttributeValue<EntityReference>("adx_product");
			ReviewerContact = review.GetAttributeValue<EntityReference>("adx_reviewercontact");

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.ProductReview, HttpContext.Current, "read_product_review", 1, review.ToEntityReference(), "read");
			}
		}

		public string Content
		{
			get
			{
				var content = Entity.GetAttributeValue<string>("adx_content");

				return string.IsNullOrWhiteSpace(content) ? string.Empty : content.Replace("\r\n", "<br />");
			}
		}

		public string CreatedByIPAddress
		{
			get { return Entity.GetAttributeValue<string>("adx_createdbyipaddress"); }
		}

		public string CreatedByUsername
		{
			get { return Entity.GetAttributeValue<string>("adx_createdbyusername"); }
		}

		public Entity Entity { get; private set; }

		public EntityReference EntityReference { get; private set; }

		public EntityReference Product { get; private set; }

		public bool PublishToWeb
		{
			get { return Entity.GetAttributeValue<bool?>("adx_publishtoweb").GetValueOrDefault(false); }
		}

		public double Rating
		{
			get { return Entity.GetAttributeValue<double?>("adx_rating").GetValueOrDefault(0); }
		}

		public double RatingRationalValue
		{
			get { return Entity.GetAttributeValue<double?>("adx_ratingrationalvalue").GetValueOrDefault(0); }
		}

		public int RatingMaximumValue
		{
			get { return Entity.GetAttributeValue<int?>("adx_maximumvalue").GetValueOrDefault(0); }
		}

		public bool Recommend
		{
			get { return Entity.GetAttributeValue<bool?>("adx_recommend").GetValueOrDefault(false); }
		}

		public EntityReference ReviewerContact { get; private set; }

		public string ReviewerEmail
		{
			get { return Entity.GetAttributeValue<string>("adx_revieweremail"); }
		}

		public string ReviewerName
		{
			get { return Entity.GetAttributeValue<string>("adx_reviewername"); }
		}

		public string ReviewerLocation
		{
			get { return Entity.GetAttributeValue<string>("adx_reviewerlocation"); }
		}

		public DateTime SubmittedOn
		{
			get { return Entity.GetAttributeValue<DateTime?>("adx_submittedon") ?? Entity.GetAttributeValue<DateTime>("createdon"); }
		}

		public string Title
		{
			get { return Entity.GetAttributeValue<string>("adx_title"); }
		}
	}
}
