/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Text;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class DateFilters
	{
		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of days to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of days represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional days. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddDays(DateTime? date, double value)
		{
			return date.HasValue
				? date.Value.AddDays(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of hours to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of hours represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional hours. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddHours(DateTime? date, double value)
		{
			return date.HasValue
				? date.Value.AddHours(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of minutes to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of minutes represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional minutes. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddMinutes(DateTime? date, double value)
		{
			return date.HasValue
				? date.Value.AddMinutes(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of months to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of months. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddMonths(DateTime? date, int value)
		{
			return date.HasValue
				? date.Value.AddMonths(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of seconds to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of seconds represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional seconds. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddSeconds(DateTime? date, double value)
		{
			return date.HasValue
				? date.Value.AddSeconds(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Returns a new <see cref="DateTime"/> that adds the specified number of years to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of years represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of years. The <paramref name="value"/> parameter can be negative or positive. </param>
		public static DateTime? DateAddYears(DateTime? date, int value)
		{
			return date.HasValue
				? date.Value.AddYears(value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Convert an input <see cref="DateTime"/> to local (server) time.
		/// </summary>
		public static DateTime? DateToLocal(DateTime? date)
		{
			return date.HasValue
				? date.Value.ToLocalTime()
				: (DateTime?)null;
		}

		/// <summary>
		/// Convert an input <see cref="DateTime"/> to from local (server) time, to UTC.
		/// </summary>
		public static DateTime? DateToUtc(DateTime? date)
		{
			return date.HasValue
				? date.Value.ToUniversalTime()
				: (DateTime?)null;
		}

		/// <summary>
		/// Convert <see cref="DateTime"/> to ISO 8601 format, as used in XML and Atom.
		/// </summary>
		public static string DateToIso8601(DateTime? date)
		{
			if (date == null)
			{
				return null;
			}

			return date.Value.Kind == DateTimeKind.Utc
				? date.Value.ToString("s") + "Z"
				: date.Value.ToString("s") + date.Value.ToString("zzz");
		}

		/// <summary>
		/// Convert <see cref="DateTime"/> to RFC 822 format, as used in RSS 2.0.
		/// </summary>
		public static string DateToRfc822(DateTime? date)
		{
			if (date == null)
			{
				return null;
			}

			if (date.Value.Kind == DateTimeKind.Utc)
			{
				return date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss Z", CultureInfo.InvariantCulture);
			}

			var builder = new StringBuilder(date.Value.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture));

			builder.Remove(builder.Length - 3, 1);

			return builder.ToString();
		}

		/// <summary>
		/// Convert <see cref="DateTime"/> to ISO 8601 format, as used in XML and Atom.
		/// </summary>
		public static string DateToXmlSchema(DateTime? date)
		{
			return DateToIso8601(date);
		}
	}
}
