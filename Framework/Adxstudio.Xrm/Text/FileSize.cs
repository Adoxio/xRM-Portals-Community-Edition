/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;

namespace Adxstudio.Xrm.Text
{
	public struct FileSize : IFormattable
	{
		private const int _defaultPrecision = 2;

		private static readonly string[] _units = new[]
		{
			"bytes", "KB", "MB", "GB", "TB"
		};

		private readonly ulong _value;

		public FileSize(ulong value)
		{
			_value = value;
		}

		public static explicit operator FileSize(ulong value)
		{
			return new FileSize(value);
		}

		public static implicit operator ulong(FileSize fileSize)
		{
			return fileSize._value;
		}

		override public string ToString()
		{
			return ToString(null, null);
		}

		public string ToString(string format)
		{
			return ToString(format, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			int precision;

			if (string.IsNullOrEmpty(format))
			{
				return ToString(_defaultPrecision, formatProvider);
			}

			if (int.TryParse(format, out precision))
			{
				return ToString(precision, formatProvider);
			}
				
			return _value.ToString(format, formatProvider);
		}

		/// <summary>
		/// Formats the FileSize using the given number of decimals.
		/// </summary>
		public string ToString(int precision, IFormatProvider formatProvider = null)
		{
			var pow = Math.Floor((_value > 0 ? Math.Log(_value) : 0) / Math.Log(1024));

			pow = Math.Min(pow, _units.Length - 1);

			var value = _value / Math.Pow(1024, pow);

			var precisionString = formatProvider == null
				? precision.ToString(CultureInfo.CurrentCulture)
				: precision.ToString(formatProvider);

			return value.ToString(Math.Abs(pow - 0) < double.Epsilon ? "F0" : "F" + precisionString) + " " + _units[(int)pow];
		}
	}
}
