/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;

namespace Microsoft.Xrm.Portal.Web
{
	internal static class Extensions
	{
		public static void RedirectAndEndResponse(this HttpApplication application, string url)
		{
			RedirectAndEndResponse(application, application.Response, url);
		}

		public static void RedirectAndEndResponse(this HttpContext context, string url)
		{
			RedirectAndEndResponse(context.ApplicationInstance, context.Response, url);
		}

		private static void RedirectAndEndResponse(HttpApplication application, HttpResponse response, string url)
		{
			response.Redirect(url, false);
			application.CompleteRequest();
		}
	}
}
