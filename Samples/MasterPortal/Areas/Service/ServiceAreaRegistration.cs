/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;
using System.Web.Routing;

namespace Site.Areas.Service
{
	public class ServiceAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Service"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.Routes.Add("ServiceAppointmentiCalendar", new Route("_services/icalendar/serviceappointment/{id}", new CalendarHandler.RouteHandler()));
		}
	}
}
