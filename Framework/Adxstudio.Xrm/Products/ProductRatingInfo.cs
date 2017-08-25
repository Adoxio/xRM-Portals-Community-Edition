/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Products
{
	public class ProductRatingInfo : IProductRatingInfo
	{
		public ProductRatingInfo(double average, double averageRationalValue, int count, int maximumValue, double sum)
		{
			Average = average;
			AverageRationalValue = averageRationalValue;
			Count = count;
			MaximumValue = maximumValue;
			Sum = sum;
		}

		public double Average { get; private set; }
		public double AverageRationalValue { get; private set; }
		public int Count { get; private set; }
		public int MaximumValue { get; private set; }
		public double Sum { get; private set; }
	}
}
