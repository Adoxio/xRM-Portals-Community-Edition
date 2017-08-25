/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	using System.Linq;
	using System.Web;

	/// <summary>
	/// Class encapsulating the inspection of the HttpContext's cookies
	/// </summary>
	public sealed class CookieInspector : ICookieInspector
	{
		/// <summary>
		/// Key for the cookie in the cookie collection
		/// </summary>
		public const string AnalyticsCookieKey = "Dynamics365PortalAnalytics";

		/// <summary>
		/// Site setting that controls whether or not to set the Analytics cookie
		/// </summary>
		public const string AnalyticsCookieSetting = "Cookies/DisableAnalyticsCookie";

		/// <summary>
		/// HttpContext to Inspect
		/// </summary>
		private HttpContextBase Context { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CookieInspector" /> class.
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		private CookieInspector(HttpContextBase context)
		{
			this.Context = context;
		}

		/// <summary>
		/// Gets an instance of the ICookieInspector
		/// </summary>
		/// <param name="context">HttpContext to inspect</param>
		/// <returns>an instance of the ICookieInspector</returns>
		public static ICookieInspector GetInstance(HttpContextBase context)
		{
			return new CookieInspector(context);
		}

		/// <summary>
		/// Returns whether or not cookies are enabled
		/// </summary>
		public bool AreCookiesEnabled
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns the value of the cookie with the given key
		/// </summary>
		/// <param name="key">key specifying which cookie to get the value for</param>
		/// <returns>string value of the cookie</returns>
		public string GetCookieValue(string key)
		{
			var cookie = this.CheckCookie(key);
			return (cookie == null || string.IsNullOrEmpty(cookie.Value))
				? null
				: cookie.Value;
		}

		/// <summary>
		/// Gets the cookie with the given key from the CookieCollection
		/// </summary>
		/// <param name="key">key for the cookie which to get</param>
		/// <returns>HttpCookie from the CookieCollection</returns>
		private HttpCookie CheckCookie(string key)
		{
			if (this.Context == null)
			{
				return null;
			}

			// Attempt to find this key in the Response, if not there search the request
			return this.Context.Response.Cookies.AllKeys.Contains(key)
						? this.Context.Response.Cookies.Get(key)
						: this.Context.Request.Cookies.AllKeys.Contains(key)
							? this.Context.Request.Cookies.Get(key)
							: null;
		}
	}
}
