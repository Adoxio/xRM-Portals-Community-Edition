/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using Microsoft.Xrm.Client.Runtime;

namespace Microsoft.Xrm.Client
{
	/// <summary>
	/// Helper methods on the <see cref="string"/> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Invokes the <see cref="M:System.String.Format(System.IFormatProvider,System.String,System.Object[])"/> method with the 
		/// <see cref="P:System.StringComparer.InvariantCulture"/> provider.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string FormatWith(this string format, params object[] args)
		{
			return FormatWith(format, CultureInfo.InvariantCulture, args);
		}

		private static string FormatWith(this string format, IFormatProvider provider, params object[] args)
		{
			format.ThrowOnNull("format");

			return string.Format(provider, format, args);
		}

		/// <summary>
		/// Parses a name value into an <see cref="Enum"/> value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumName"></param>
		/// <returns></returns>
		public static T ToEnum<T>(this string enumName)
		{
			return (T)Enum.Parse(typeof(T), enumName);
		}

		/// <summary>
		/// Converts an integer value into an <see cref="Enum"/> value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumValue"></param>
		/// <returns></returns>
		public static T ToEnum<T>(this int enumValue)
		{
			return ToEnum<T>(enumValue.ToString());
		}
	}
}
