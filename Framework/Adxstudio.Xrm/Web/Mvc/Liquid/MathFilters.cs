/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class MathFilters
	{
		public static int Ceil(object value)
		{
			return value == null
				? 0
				: Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(value)));
		}

		public static int Floor(object value)
		{
			return value == null
				? 0
				: Convert.ToInt32(Math.Floor(Convert.ToDecimal(value)));
		}

		public static object Round(object value, int decimals = 0)
		{
			if (value == null)
			{
				return 0;
			}

			return decimals == 0
				? Convert.ToInt32(Math.Round(Convert.ToDecimal(value), decimals))
				: Math.Round(Convert.ToDecimal(value), decimals);
		}
	}
}
