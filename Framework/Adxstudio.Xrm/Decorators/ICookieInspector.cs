/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	/// <summary>
	/// Wrapper around the HttpContext's CookieCollection
	/// </summary>
	public interface ICookieInspector
	{
		/// <summary>
		/// Returns whether or not cookies are enabled with respect to the Site Setting
		/// </summary>
		bool AreCookiesEnabled { get; }

		/// <summary>
		/// Returns the value of the cookie with the given key
		/// </summary>
		/// <param name="key">key specifying which cookie to get the value for</param>
		/// <returns>string value of the cookie</returns>
		string GetCookieValue(string key);
	}
}
