/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using Microsoft.Xrm.Client;

namespace Adxstudio.Xrm.Web
{
	public class RedirectMatch : IRedirectMatch
	{
		public RedirectMatch(HttpStatusCode statusCode, string location)
		{
			StatusCode = statusCode;
			Location = location;
		}

		public RedirectMatch(int statusCode, string location) : this(statusCode.ToEnum<HttpStatusCode>(), location) { }

		public virtual string Location { get; private set; }

		public virtual HttpStatusCode StatusCode { get; private set; }

		public virtual bool Success
		{
			get { return true; }
		}
	}
}
