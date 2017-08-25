/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Core
{
	public static class PriceListExtensions
	{
		public static bool IsApplicableTo(this Entity priceList, DateTime value)
		{
			priceList.AssertEntityName("pricelevel");

			var startDate = priceList.GetAttributeValue<DateTime?>("begindate");
			var endDate = priceList.GetAttributeValue<DateTime?>("enddate");

			return (new DateTimeRange(startDate, endDate)).Includes(value);
		}

		private class DateTimeRange
		{
			public DateTimeRange(DateTime? min, DateTime? max)
			{
				Min = min;
				Max = max;
			}

			public DateTime? Max { get; set; }

			public DateTime? Min { get; set; }

			public bool Includes(DateTime value)
			{
				return (!Min.HasValue || value >= Min.Value) && (!Max.HasValue || value <= Max.Value);
			}
		}
	}
}
