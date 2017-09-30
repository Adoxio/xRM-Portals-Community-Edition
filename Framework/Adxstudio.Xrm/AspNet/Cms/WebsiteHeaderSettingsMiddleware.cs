/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Adxstudio.Xrm.AspNet.Cms
{
	/// <summary>
	/// Provides for HTTP headers configurable through Site Settings.
	/// </summary>
	public class WebsiteHeaderSettingsMiddleware : OwinMiddleware
	{
		private static readonly IEnumerable<KeyValuePair<string, string>> HeaderSettings = new Dictionary<string, string>
		{
			{ "HTTP/Access-Control-Allow-Credentials",    "Access-Control-Allow-Credentials" },
			{ "HTTP/Access-Control-Allow-Headers",        "Access-Control-Allow-Headers" },
			{ "HTTP/Access-Control-Allow-Methods",        "Access-Control-Allow-Methods" },
			{ "HTTP/Access-Control-Allow-Origin",         "Access-Control-Allow-Origin" },
			{ "HTTP/Access-Control-Expose-Headers",       "Access-Control-Expose-Headers" },
			{ "HTTP/Access-Control-Max-Age",              "Access-Control-Max-Age" },
			{ "HTTP/Content-Security-Policy",             "Content-Security-Policy" },
			{ "HTTP/Content-Security-Policy-Report-Only", "Content-Security-Policy-Report-Only" },
			{ "HTTP/X-Frame-Options",                     "X-Frame-Options" }
		};

		public WebsiteHeaderSettingsMiddleware(OwinMiddleware next, CrmWebsite website) : base(next)
		{
			if (website == null) throw new ArgumentNullException("website");

			Website = website;
		}

		protected CrmWebsite Website { get; private set; }

		public override async Task Invoke(IOwinContext context)
		{
			foreach (var headerSetting in HeaderSettings)
			{
				var headerValue = Website.Settings.Get<string>(headerSetting.Key);

				if (string.IsNullOrWhiteSpace(headerValue))
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(context.Response.Headers.Get(headerSetting.Value)))
				{
					context.Response.Headers.Set(headerSetting.Value, headerValue);
				}
			}

			if (Next != null)
			{
				await Next.Invoke(context);
			}
		}
	}
}
