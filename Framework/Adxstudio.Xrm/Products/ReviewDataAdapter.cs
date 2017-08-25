/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Provides data operations for a single review.
	/// </summary>
	public class ReviewDataAdapter : IReviewDataAdapter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ReviewDataAdapter(EntityReference review, IDataAdapterDependencies dependencies)
		{
			if (review == null) throw new ArgumentNullException("review");
			if (review.LogicalName != "adx_review") throw new ArgumentException(string.Format(ResourceManager.GetString("Value_Missing_For_LogicalName"), review.LogicalName), "review");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Review = review;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ReviewDataAdapter(Entity review, IDataAdapterDependencies dependencies) : this(review.ToEntityReference(), dependencies) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		public ReviewDataAdapter(IProduct review, IDataAdapterDependencies dependencies) : this(review.Entity, dependencies) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ReviewDataAdapter(EntityReference review, string portalName = null) : this(review, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ReviewDataAdapter(Entity review, string portalName = null) : this(review, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="review">Review Entity Reference</param>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public ReviewDataAdapter(IProduct review, string portalName = null) : this(review, new PortalConfigurationDataAdapterDependencies(portalName)) { }

		/// <summary>
		/// Report a review as abusive.
		/// </summary>
		/// <param name="remarks"></param>
		public virtual void ReportAbuse(string remarks)
		{
			var httpContext = Dependencies.GetRequestContext().HttpContext;
			var user = Dependencies.GetPortalUser();
			var username = httpContext.Request.IsAuthenticated && user != null ? user.Name : httpContext.Request.AnonymousID;
			var title = string.Format("Abuse Reported on {0} by {1}", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), username);

			IAnnotationDataAdapter da = new AnnotationDataAdapter(Dependencies);
			da.CreateAnnotation(Review, title, remarks);
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }
		
		protected EntityReference Review { get; set; }
	}
}
