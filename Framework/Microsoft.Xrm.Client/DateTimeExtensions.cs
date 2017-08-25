/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Represents a granularity of rounding.
	/// </summary>
	public enum RoundTo
	{
		/// <summary>
		/// Round to the second.
		/// </summary>
		Second,

		/// <summary>
		/// Round to the minute.
		/// </summary>
		Minute,

		/// <summary>
		/// Round to the hour.
		/// </summary>
		Hour,

		/// <summary>
		/// Round to the day.
		/// </summary>
		Day,

		/// <summary>
		/// Round to the month.
		/// </summary>
		Month
	}

	/// <summary>
	/// Helper methods on the <see cref="DateTime"/> and <see cref="DateTimeOffset"/> classes.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Returns the largest discrete value less than or equal to the specified value.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static DateTime Floor(this DateTime d, RoundTo to)
		{
			if (to == RoundTo.Second) return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Kind);
			if (to == RoundTo.Minute) return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0, d.Kind);
			if (to == RoundTo.Hour) return new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, d.Kind);
			if (to == RoundTo.Day) return new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, d.Kind);
			if (to == RoundTo.Month) return new DateTime(d.Year, d.Month, 1, 0, 0, 0, d.Kind);
			return d;
		}

		/// <summary>
		/// Rounds a value to the nearest discrete value.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static DateTime Round(this DateTime d, RoundTo to)
		{
			var floor = Floor(d, to);
			if (to == RoundTo.Second && d.Millisecond >= 500) return floor.AddSeconds(1);
			if (to == RoundTo.Minute && d.Second >= 30) return floor.AddMinutes(1);
			if (to == RoundTo.Hour && d.Minute >= 30) return floor.AddHours(1);
			if (to == RoundTo.Day && d.Hour >= 12) return floor.AddDays(1);
			if (to == RoundTo.Month && d.Day >= DateTime.DaysInMonth(d.Year, d.Month) / 2) return floor.AddMonths(1);
			return d;
		}

		/// <summary>
		/// Returns the largest discrete value less than or equal to the specified value.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static DateTimeOffset Floor(this DateTimeOffset d, RoundTo to)
		{
			if (to == RoundTo.Second) return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Offset);
			if (to == RoundTo.Minute) return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0, d.Offset);
			if (to == RoundTo.Hour) return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, 0, 0, d.Offset);
			if (to == RoundTo.Day) return new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, d.Offset);
			if (to == RoundTo.Month) return new DateTimeOffset(d.Year, d.Month, 1, 0, 0, 0, d.Offset);
			return d;
		}

		/// <summary>
		/// Rounds a value to the nearest discrete value.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static DateTimeOffset Round(this DateTimeOffset d, RoundTo to)
		{
			var floor = Floor(d, to);
			if (to == RoundTo.Second && d.Millisecond >= 500) return floor.AddSeconds(1);
			if (to == RoundTo.Minute && d.Second >= 30) return floor.AddMinutes(1);
			if (to == RoundTo.Hour && d.Minute >= 30) return floor.AddHours(1);
			if (to == RoundTo.Day && d.Hour >= 12) return floor.AddDays(1);
			if (to == RoundTo.Month && d.Day >= DateTime.DaysInMonth(d.Year, d.Month) / 2) return floor.AddMonths(1);
			return d;
		}
	}
}
