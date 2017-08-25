/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Globalization;
using Adxstudio.Xrm.Resources.ResourceFiles;

namespace Adxstudio.Xrm.Resources
{
	/// <summary>
	/// Wrapper class for string resources
	/// </summary>
	public class ResourceManager
	{
		/// <summary>
		/// Looks up the localized string for the specified locale
		/// </summary>
		/// <param name="key">The key for the string</param>
		/// <param name="culture">The CultureInfo object for the locale</param>
		/// <returns>The string pertaining to the specified key and locale. If no locale is specified, uses default instead </returns>
		public static string GetString(string key, CultureInfo culture = null)
		{
			if (culture == null)
			{
				return strings.ResourceManager.GetString(key, strings.Culture);
			}

			return strings.ResourceManager.GetString(key, culture);
		}

	}
}
