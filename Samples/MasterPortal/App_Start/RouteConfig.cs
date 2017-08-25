/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Handlers;
using Adxstudio.Xrm.Web.Mvc;

namespace Site
{
	public static class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.RegisterRoutesWithLock(r =>
			{
				r.IgnoreRoute("{resource}.axd/{*pathInfo}");
				r.IgnoreRoute("css/{resource}.bundle.css");
				r.IgnoreRoute("js/{resource}.bundle.js");

				r.Add(new Route("{area}/about", null, new RouteValueDictionary(new { area = "_services" }), new AboutProductHandler()));
			});
		}
	}
}
