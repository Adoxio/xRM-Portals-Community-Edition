/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Metadata;
using DotLiquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class NumberFormatFilters
	{
		/// <summary>
		/// Format a given numeric value as representing a value in the CRM organization base currency.
		/// </summary>
		public static string BaseCurrency(Context context, object value, string format = null)
		{
			IPortalLiquidContext portalLiquidContext;

			if (!context.TryGetPortalLiquidContext(out portalLiquidContext))
			{
				return null;
			}

			return FormatCurrency(value, format, new BaseCurrencyMoneyFormatter(portalLiquidContext.OrganizationMoneyFormatInfo));
		}

		/// <summary>
		/// Format a given numeric value according to the CRM currency settings for a given record and
		/// entity attribute.
		/// </summary>
		public static string Currency(Context context, object value, EntityDrop record, string attribute, string format = null)
		{
			if (record == null || attribute == null)
			{
				return null;
			}

			IPortalLiquidContext portalLiquidContext;

			if (!context.TryGetPortalLiquidContext(out portalLiquidContext))
			{
				return null;
			}

			var moneyAttributeMetadata = record.EntityMetadata.Attributes
				.FirstOrDefault(e => string.Equals(e.LogicalName, attribute, StringComparison.OrdinalIgnoreCase)) as MoneyAttributeMetadata;

			if (moneyAttributeMetadata == null)
			{
				return null;
			}

			return FormatCurrency(value, format, new MoneyFormatter(portalLiquidContext.OrganizationMoneyFormatInfo, record.MoneyFormatInfo, moneyAttributeMetadata));
		}

		private static string FormatCurrency(object value, string format, IFormatProvider formatProvider)
		{
			var money = value as Money;

			return string.Format(
				formatProvider,
				string.IsNullOrEmpty(format) ? "{0}" : "{{0:{0}}}".FormatWith(format),
				money ?? new Money(Convert.ToDecimal(value ?? 0)));
		}

		public static string Format(object number, string format, string culture = "")
		{
			var cultureInfo = string.IsNullOrEmpty(culture)
				? CultureInfo.CurrentUICulture
				: CultureInfo.GetCultureInfoByIetfLanguageTag(culture);
			var dec = Convert.ToDecimal(number);
			return dec.ToString(format, cultureInfo);
		}

		public static string Decimals(object number, int decimals, string culture = "")
		{
			return Format(number, string.Format("N{0}", decimals), culture);
		}

		public static string MaxDecimals(object number, int decimals, string culture = "")
		{
			var result = Decimals(number, decimals, culture);
			if (new Regex(string.Format("\\{0}", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)).IsMatch(result))
			{
				var zeroRegex = new Regex("0+$");
				var decimalRegex = new Regex(string.Format("\\{0}$", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator));
				result = zeroRegex.Replace(result, string.Empty);
				result = decimalRegex.Replace(result, string.Empty);
			}
			return result;
		}

		/// <summary>
		/// Returns the unformatted/culture invariant value for a decimal number upto a specified number of decimal places
		/// </summary>
		/// <param name="number">The decimal number</param>
		/// <param name="decimals">The number of decimal places</param>
		/// <returns></returns>
		public static string InvariantCultureDecimalValue(decimal number, int decimals)
		{
			return  decimal.Round(number, decimals).ToString(CultureInfo.InvariantCulture);			
		}
	}
}
