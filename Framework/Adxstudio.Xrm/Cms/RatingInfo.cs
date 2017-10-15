/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Cms
{
	[Serializable]
	public class RatingInfo : IRatingInfo
	{
		public RatingInfo(int yesCount = 0, int noCount = 0, double averageRating = 0, int ratingCount = 0, int ratingSum = 0)
		{
			YesCount = yesCount;
			NoCount = noCount;
			AverageRating = (averageRating > 0) ? averageRating : ((ratingCount != 0) ? ((double)ratingSum / (double)ratingCount) : 0);
			AverageRatingRounded = AverageRating > 0 ? Math.Round(AverageRating * 2, MidpointRounding.AwayFromZero) / 2 : 0; // round to the nearest half
			RatingCount = ratingCount;
		}

		public int YesCount { get; private set; }

		public int NoCount { get; private set; }

		public double AverageRating { get; private set; }

		public double AverageRatingRounded { get; private set; }

		public int RatingCount { get; private set; }
	}
}
