/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Site.Areas.Setup
{
	public class SetupRoute : Route, IRouteWithArea
	{
		public SetupRoute(string url, IRouteHandler routeHandler)
			: base(url, routeHandler)
		{
		}

		public SetupRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
			: base(url, defaults, routeHandler)
		{
		}

		public SetupRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
			: base(url, defaults, constraints, routeHandler)
		{
		}

		public SetupRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
			: base(url, defaults, constraints, dataTokens, routeHandler)
		{
		}

		public SetupRoute(string url, object defaults, object constraints)
			: base(url, ToRouteValueDictionary(defaults), ToRouteValueDictionary(constraints), new MvcRouteHandler())
		{
		}

		public string Area
		{
			get { return "Setup"; }
		}

		private static RouteValueDictionary ToRouteValueDictionary(object values)
		{
			var dictionary = values as IDictionary<string, object>;
			return dictionary != null ? new RouteValueDictionary(dictionary) : new RouteValueDictionary(values);
		}
	}
}
