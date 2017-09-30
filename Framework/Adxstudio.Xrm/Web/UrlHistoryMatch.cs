/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Net;

namespace Adxstudio.Xrm.Web
{
	public class UrlHistoryMatch : RedirectMatch
	{
		public UrlHistoryMatch(string location) : base(HttpStatusCode.Moved, location) { }
	}
}
