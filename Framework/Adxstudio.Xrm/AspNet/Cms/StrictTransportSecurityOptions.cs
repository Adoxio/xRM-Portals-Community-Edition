/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;

	/// <summary>
	/// Options for Strict-Transport-Security Header
	/// </summary>
	public class StrictTransportSecurityOptions
	{
		/// <summary>
		/// The default maximum age
		/// </summary>
		private const uint DefaultMaxAge = 31536000; // 12 Month

		/// <summary>
		/// Gets or sets the max-age header value (in seconds).
		/// </summary>
		/// <value>
		/// The maximum age.
		/// </value>
		public uint MaxAge { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [include sub domains].
		/// </summary>
		/// <value>
		///   <c>true</c> if [include sub domains]; otherwise, <c>false</c>.
		/// </value>
		public bool IncludeSubDomains { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to in include the preload parameter or not.
		/// </summary>
		/// <value>
		///   <c>true</c> if preload; otherwise, <c>false</c>.
		/// </value>
		public bool Preload { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StrictTransportSecurityOptions"/> class.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <exception cref="System.ArgumentNullException">website raised</exception>
		public StrictTransportSecurityOptions(CrmWebsite website)
		{
			if (website == null)
			{
				throw new ArgumentNullException(nameof(website));
			}
			this.MaxAge = DefaultMaxAge;
			this.IncludeSubDomains = true;
		}
	}
}
