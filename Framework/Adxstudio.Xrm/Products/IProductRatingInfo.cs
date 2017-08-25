/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Products
{
	public interface IProductRatingInfo
	{
		double Average { get; }

		double AverageRationalValue { get; }

		int Count { get; }

		int MaximumValue { get; }

		double Sum { get; }
	}
}
