/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EntityList
{
	using System;
	using Resources;
	using Microsoft.Xrm.Client;

	/// <summary>
	/// Localized exception.
	/// Used for hide sensitive data from user.
	/// </summary>
	public class LocalizedException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizedException"/> class.
		/// Standard constructor without arguments.
		/// </summary>
		public LocalizedException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizedException"/> class.
		/// </summary>
		/// <param name="resourceName">Name of string resource.</param>
		/// <param name="args">Format arguments.</param>
		public LocalizedException(string resourceName, params object[] args) 
			: base(ResourceManager.GetString(resourceName).FormatWith(args))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizedException"/> class.
		/// Constructor with resource's name and inner exception.
		/// </summary>
		/// <param name="resourceName">Name of string resource.</param>
		/// <param name="inner">Inner exception.</param>
		/// <param name="args">Format arguments.</param>
		public LocalizedException(string resourceName, Exception inner, params object[] args) 
			: base(ResourceManager.GetString(resourceName).FormatWith(args), inner)
		{
		}
	}
}
