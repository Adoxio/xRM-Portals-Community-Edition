/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Web.Mvc;
	using Adxstudio.Xrm.Core.Flighting;
	using Adxstudio.Xrm.Resources;
	using Adxstudio.Xrm.Web.Mvc;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Controller for rating records
	/// </summary>
	public class RatingController : Controller
	{
		/// <summary>
		/// Rate a record
		/// </summary>
		/// <param name="entityReference">The <see cref="EntityReference"/> of the record to be rated.</param>
		/// <param name="rating">The rating value.</param>
		/// <param name="min">The minimum rating value allowed.</param>
		/// <param name="max">The maximum rating value allowed.</param>
		/// <returns>An empty result.</returns>
		[HttpPost]
		[JsonHandlerError]
		[AjaxValidateAntiForgeryToken]
		public ActionResult CreateRating(EntityReference entityReference, int rating, int min, int max)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				throw new Exception("The Feedback feature has not been enabled. Please check your configuration.");
			}

			if (entityReference == null)
			{
				throw new ArgumentNullException("entityReference");
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			var dataAdapterFactory = new RatingDataAdapterFactory(entityReference);

			var dataAdapter = dataAdapterFactory.GetAdapter(portal, Request.RequestContext);

			if (rating < min)
			{
				rating = min;
			}

			if (rating > max)
			{
				rating = max;
			}

			dataAdapter.SaveRating(rating, max, min, HttpContext.Profile.UserName);

			var ratingInfo = dataAdapter.GetRatingInfo();

			if (ratingInfo == null)
			{
				throw new Exception("Error getting Rating Info");
			}

			var json = Json(ratingInfo);

			return json;
		}
	}
}
