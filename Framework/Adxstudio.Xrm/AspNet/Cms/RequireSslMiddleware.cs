/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.Owin;
	using Adxstudio.Xrm.Configuration;

	/// <summary>
	/// Settings for HTTPS redirect.
	/// </summary>
	public class RequireSslOptions
	{
		public RequireSslOptions(WebAppSettings webAppSettings)
		{
			this.Scheme = "https";
			this.Port = 443;
			this.RedirectStatusCode = 301;
			// require SSL in Azure web apps by default
			this.Enabled = "PortalRequireSsl".ResolveAppSetting().ToBoolean().GetValueOrDefault(webAppSettings.AzureWebAppEnabled);
		}

		public bool Enabled { get; set; }

		public string Scheme { get; set; }

		public int Port { get; set; }

		public int RedirectStatusCode { get; set; }
	}

	/// <summary>
	/// A middleware to respond with a permanent redirect to HTTPS.
	/// </summary>
	public class RequireSslMiddleware : OwinMiddleware
	{
		public RequireSslOptions Options { get; private set; }

		public RequireSslMiddleware(OwinMiddleware next, RequireSslOptions options)
			: base(next)
		{
			if (options == null) throw new ArgumentNullException("options");

			Options = options;
		}

		public override async Task Invoke(IOwinContext context)
		{
			if (Options.Enabled && !string.Equals(context.Request.Scheme, Options.Scheme, StringComparison.InvariantCultureIgnoreCase))
			{
				var redirectUrl = Options.Port != 443
					? Options.Scheme + "://" + context.Request.Uri.Host + ":" + Options.Port + context.Request.Uri.PathAndQuery
					: Options.Scheme + "://" + context.Request.Uri.Host + context.Request.Uri.PathAndQuery;

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("redirectUrl={0}", redirectUrl));

				context.Response.StatusCode = Options.RedirectStatusCode;
				context.Response.Headers.Set("Location", redirectUrl);
			}
			else
			{
				await Next.Invoke(context);
			}
		}
	}
}
