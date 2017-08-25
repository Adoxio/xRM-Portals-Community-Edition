/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Interface that provides a matching operation that indicates whether a given URL should be redirected
	/// to an alternate URL.
	/// </summary>
	public interface IRedirectProvider
	{
		/// <summary>
		/// Indicates whether or not a given <see cref="UrlBuilder">URL</see> should be redirected to an
		/// alternate URL (and what that alternate URL is).
		/// </summary>
		/// <param name="websiteID">
		/// The <see cref="Guid">ID</see> website in which to find a match.
		/// </param>
		/// <param name="url">
		/// The URL to be matched for possible redirection.
		/// </param>
		/// <returns>
		/// An <see cref="IRedirectMatch"/>, which will indicate whether or not there was a successful match
		/// (i.e., a redirect should be performed) and, if so, the precise nature of that redirect (HTTP
		/// status code, redirect location, etc.).
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="url"/> is null.
		/// </exception>
		IRedirectMatch Match(Guid websiteID, UrlBuilder url);
	}
}
