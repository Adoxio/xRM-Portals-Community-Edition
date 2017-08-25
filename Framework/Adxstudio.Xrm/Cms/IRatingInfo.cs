/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adxstudio.Xrm.Cms
{
	public interface IRatingInfo
	{
		//Anything that the IRatingInfo Needs to have

		int YesCount { get; }

		int NoCount { get; }

		double AverageRating { get; }

		double AverageRatingRounded { get; }

		int RatingCount { get; }
	}
}
