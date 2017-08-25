/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.Owin;

	/// <summary>
	/// Appends Strict-Transport-Security header
	/// </summary>
	/// <seealso cref="Microsoft.Owin.OwinMiddleware" />
	public class StrictTransportSecurityMiddleware : OwinMiddleware
	{
		/// <summary>
		/// The strict transport security header name
		/// </summary>
		private const string StrictTransportSecurityHeaderName = "Strict-Transport-Security";

		/// <summary>
		/// The settings
		/// </summary>
		private readonly StrictTransportSecurityOptions settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="StrictTransportSecurityMiddleware"/> class.
		/// </summary>
		/// <param name="next">The next.</param>
		/// <param name="options">Strict-Transport-Security header optionons.</param>
		public StrictTransportSecurityMiddleware(OwinMiddleware next, StrictTransportSecurityOptions options) : base(next)
		{
			this.settings = options;
		}

		/// <summary>
		/// Process an individual request.
		/// </summary>
		/// <param name="context">Owin context</param>
		/// <returns>The next</returns>
		public override async Task Invoke(IOwinContext context)
		{
			if (context.Request.IsSecure)
			{
				context.Response.OnSendingHeaders(action => ConstructHeader(this.settings, context), this.settings);
			}
			if (this.Next != null)
			{
				await this.Next.Invoke(context);
			}
		}

		/// <summary>
		/// Constructs the header.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="context">The context.</param>
		private static void ConstructHeader(StrictTransportSecurityOptions settings, IOwinContext context)
		{
			var options = settings;
			var response = context.Response;
			response.Headers[StrictTransportSecurityHeaderName] = ConstructHeaderValue(options);
		}

		/// <summary>
		/// Constructs the header value.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <returns>constructed header string</returns>
		private static string ConstructHeaderValue(StrictTransportSecurityOptions options)
		{
			var age = MaxAgeDirective(options.MaxAge);
			var subDomains = IncludeSubDomainsDirective(options.IncludeSubDomains);
			var preload = PreloadDirective(options.Preload);
			return $"{age}{subDomains}{preload}";
		}

		/// <summary>
		/// Maximums the age.
		/// </summary>
		/// <param name="seconds">The seconds.</param>
		/// <returns>Constructed max-age option directive</returns>
		private static string MaxAgeDirective(uint seconds)
		{
			return $"max-age={seconds}";
		}

		/// <summary>
		/// Includes the sub domains.
		/// </summary>
		/// <param name="includeSubDomains">if set to <c>true</c> [include sub domains].</param>
		/// <returns>Constructed includeSubDomains directive</returns>
		private static string IncludeSubDomainsDirective(bool includeSubDomains)
		{
			return includeSubDomains ? "; includeSubDomains" : string.Empty;
		}

		/// <summary>
		/// Preloads the specified preload.
		/// </summary>
		/// <param name="preload">if set to <c>true</c> [preload].</param>
		/// <returns>Constructed preload directive</returns>
		private static string PreloadDirective(bool preload)
		{
			return preload ? "; preload" : string.Empty;
		}
	}
	
}
