/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Provides an iCalendar 2.0 (http://tools.ietf.org/html/rfc5545) representation of a given adx_eventschedule.
	/// </summary>
	/// <remarks>
	/// This handler assumes the presence of route parameter "eventScheduleId", which is the ID of the adx_eventschedule.
	/// </remarks>
	public class CalendarRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var eventScheduleIdRouteValue = requestContext.RouteData.Values["eventScheduleId"];

			Guid eventScheduleId;

			if (eventScheduleIdRouteValue == null || !Guid.TryParse(eventScheduleIdRouteValue.ToString(), out eventScheduleId))
			{
				throw new InvalidOperationException("Unable to retrieve the event schedule ID from route data.");
			}

			return new CalendarHandler(eventScheduleId);
		}
	}
}
