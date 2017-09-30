/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public static class TypeFilters
	{
		private static readonly IDictionary<string, bool> BooleanValueMappings = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "on", true },
			{ "enabled", true },
			{ "yes", true },
			{ "off", false },
			{ "disabled", false },
			{ "no", false },
		};

		/// <summary>
		/// Try to convert an input value to a <see cref="bool"/>.
		/// </summary>
		public static bool? Boolean(object input)
		{
			if (input == null)
			{
				return null;
			}

			try
			{
				return Convert.ToBoolean(input);
			}
			catch (FormatException) { }
			catch (InvalidCastException) { }
			
			bool parsed;

			if (BooleanValueMappings.TryGetValue(input.ToString(), out parsed))
			{
				return parsed;
			}

			return null;
		}

		/// <summary>
		/// Try to convert an input value to a <see cref="decimal"/>.
		/// </summary>
		public static decimal? Decimal(object input)
		{
			if (input == null)
			{
				return null;
			}

			try
			{
				return Convert.ToDecimal(input);
			}
			catch (FormatException) { }
			catch (InvalidCastException) { }

			return null;
		}

		/// <summary>
		/// Try to convert an input value to a <see cref="int"/>.
		/// </summary>
		public static int? Integer(object input)
		{
			if (input == null)
			{
				return null;
			}

			try
			{
				return Convert.ToInt32(input);
			}
			catch (FormatException) { }
			catch (InvalidCastException) { }

			return null;
		}

		/// <summary>
		/// Try to convert an input value to a <see cref="string"/>.
		/// </summary>
		public static string String(object input)
		{
			return Convert.ToString(input);
		}
	}
}
