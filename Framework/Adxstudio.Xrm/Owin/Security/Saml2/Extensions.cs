/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Owin;

namespace Adxstudio.Xrm.Owin.Security.Saml2
{
	/// <summary>
	/// Extension methods for using <see cref="Saml2AuthenticationMiddleware"/>
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Adds the <see cref="Saml2AuthenticationMiddleware"/> into the OWIN runtime.
		/// </summary>
		/// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
		/// <param name="options">SamlAuthenticationOptions configuration options</param>
		/// <returns>The updated <see cref="IAppBuilder"/></returns>
		public static IAppBuilder UseSaml2Authentication(this IAppBuilder app, Saml2AuthenticationOptions options)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (options == null) throw new ArgumentNullException("options");

			app.Use(typeof(Saml2AuthenticationMiddleware), app, options);
			return app;
		}
	}
}
