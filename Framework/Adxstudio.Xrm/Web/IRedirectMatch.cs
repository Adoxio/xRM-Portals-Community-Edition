/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// The return value of a <see cref="IRedirectProvider.Match"/> operation, indicating success or
	/// failure of the attempted match, and information about the redirect to be performed on a
	/// successful match.
	/// </summary>
	public interface IRedirectMatch
	{
		/// <summary>
		/// The location (URL) to which a request should be redirected.
		/// </summary>
		string Location { get; }

		/// <summary>
		/// The <see cref="HttpStatusCode"/> to be used when performing a redirect based on this
		/// match (e.g., 301, 302).
		/// </summary>
		HttpStatusCode StatusCode { get; }

		/// <summary>
		/// A Boolean value indicating whether or not the match was successful (i.e., if true, a
		/// redirect should be performed)
		/// </summary>
		bool Success { get; }
	}
}
