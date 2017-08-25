/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface IRatingDataAdapter
	{
		bool RatingsEnabled { get; }

		IRating SelectUserRating();

		IRating SelectVisitorRating(string visitorID);

		IEnumerable<IRating> SelectRatings();

		IRatingInfo GetRatingInfo();

		void AddRating(IRating rating);

		void AddRating(Entity entity, int rating, int maxRating, int minRating);

		void DeleteRating(Entity entity);

		/// <summary>
		/// Used to save a rating - can be used to replace on existing rating or add a new one, or both, depending on the context.
		/// this might just call AddRating or it might check for an existing Rating for that user and update it if it exists.
		/// </summary>
		/// <param name="rating"></param>
		/// <param name="maxRating"> </param>
		/// <param name="minRating"> </param>
		void SaveRating(int rating, int maxRating, int minRating);

		void SaveRating(int rating, int maxRating, int minRating, string visitorID);

		void DeleteUserRating(string visitorID);
	}
}
