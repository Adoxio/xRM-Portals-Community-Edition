/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;

namespace Adxstudio.Xrm.Web
{
	public class FailedRedirectMatch : RedirectMatch
	{
		public FailedRedirectMatch() : base(HttpStatusCode.Redirect, null) { }

		public override bool Success
		{
			get { return false; }
		}
	}
}
