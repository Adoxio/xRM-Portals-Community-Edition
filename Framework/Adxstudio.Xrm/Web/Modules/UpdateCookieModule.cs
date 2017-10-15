/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Adxstudio.Xrm.Web.Modules
{
public class UpdateCookieModule : IHttpModule
	{
		public void Dispose() { }
 
		public virtual void Init(HttpApplication application)
		{
			application.EndRequest += UpdateCookies;
		}
 
		private static void UpdateCookies(object sender, EventArgs e)
		{
			if (HttpContext.Current.Request.IsSecureConnection && HttpContext.Current.Response.Cookies.Count > 0)
			{
				foreach (string cookieKey in HttpContext.Current.Response.Cookies.AllKeys)
				{
					if (cookieKey != "adxPreviewUnpublishedEntities")
					{
						var httpCookie = HttpContext.Current.Response.Cookies[cookieKey];
						if (httpCookie != null && !httpCookie.Secure)
						{
							httpCookie.Secure = true;
						}
					}
				}
			}
		}
	}

}
