/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Threading.Tasks;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Web;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Adxstudio.Xrm.AspNet.Cms
{
	/// <summary>
	/// An <see cref="AuthenticationMiddleware{T}"/> that redirects to the login page when encountering a 403 Forbidden status code when indicated by the site map provider.
	/// </summary>
	public class SiteMapAuthenticationMiddleware : AuthenticationMiddleware<CookieAuthenticationOptions>
	{
		private class SiteMapAuthenticationHandler : AuthenticationHandler<CookieAuthenticationOptions>
		{
			private readonly ILogger _logger;

			public SiteMapAuthenticationHandler(ILogger logger)
			{
				_logger = logger;
			}

			protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
			{
				return Task.FromResult<AuthenticationTicket>(null);
			}

			protected override Task TeardownCoreAsync()
			{
				if (Response == null
					|| Response.StatusCode != (int)HttpStatusCode.Forbidden
					|| !Options.LoginPath.HasValue
					|| Request == null || Request.User == null || Request.User.Identity == null
					|| Request.User.Identity.IsAuthenticated)
				{
					return Task.FromResult(0);
				}

				var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

				if (challenge != null)
				{
					var contextLanguageInfo = Context.GetContextLanguageInfo();
					var region = contextLanguageInfo.IsCrmMultiLanguageEnabled ? "/" + contextLanguageInfo.ContextLanguage.Code : null;
					var currentUri = Request.PathBase + Request.Path + Request.QueryString;

					var baseUri = new Uri(Request.Scheme + Uri.SchemeDelimiter + Request.Host);
					var relativeUri = Request.PathBase + region + (Options.LoginPath.Value.AppendQueryString(Options.ReturnUrlParameter, currentUri));
					var loginUri = new Uri(baseUri, relativeUri).AbsoluteUri;

					var redirectContext = new CookieApplyRedirectContext(Context, Options, loginUri);

					_logger.WriteInformation(loginUri);

					Options.Provider.ApplyRedirect(redirectContext);
				}

				return Task.FromResult(0);
			}
		}

		private readonly ILogger _logger;

		public SiteMapAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, CookieAuthenticationOptions options)
			: base(next, options)
		{
			_logger = app.CreateLogger<SiteMapAuthenticationHandler>();
		}

		protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
		{
			return new SiteMapAuthenticationHandler(_logger);
		}
	}
}
