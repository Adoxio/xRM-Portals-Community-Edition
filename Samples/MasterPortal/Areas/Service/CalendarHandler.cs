/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Service
{
	public class CalendarHandler : IHttpHandler
	{
		public class RouteHandler : IRouteHandler
		{
			public IHttpHandler GetHttpHandler(RequestContext requestContext)
			{
				var idRouteValue = requestContext.RouteData.Values["id"];

				Guid id;

				if (idRouteValue == null || !Guid.TryParse(idRouteValue.ToString(), out id))
				{
					throw new InvalidOperationException("Unable to retrieve the service appointment ID from route data.");
				}

				return new CalendarHandler(id);
			}
		}

		public CalendarHandler(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; private set; }

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();

			var scheduledActivity = serviceContext.CreateQuery("serviceappointment").FirstOrDefault(s => s.GetAttributeValue<Guid>("activityid") == Id);

			if (scheduledActivity == null)
			{
				NotFound(context.Response, string.Format(ResourceManager.GetString("Service_Appointment_Not_Found"), Id));

				return;
			}

			var vevent = new VEvent
			{
				Created = scheduledActivity.GetAttributeValue<DateTime>("createdon"),
				Summary = scheduledActivity.GetRelatedEntity(serviceContext, new Relationship("service_service_appointments")).GetAttributeValue<string>("name"),
				Start = scheduledActivity.GetAttributeValue<DateTime?>("scheduledstart").GetValueOrDefault(),
				End = scheduledActivity.GetAttributeValue<DateTime?>("scheduledend").GetValueOrDefault()
			};

			context.Response.ContentType = "text/calendar";

			context.Response.Write(vevent.ToString());
		}

		private static void NotFound(HttpResponse response, string message)
		{
			response.StatusCode = 404;
			response.ContentType = "text/plain";
			response.Write(message);
			response.End();
		}

		private class VEvent
		{
			public DateTime? Created { get; set; }

			public string Description { get; set; }

			public DateTime? End { get; set; }

			public string Location { get; set; }

			public string Organizer { get; set; }

			public DateTime? Start { get; set; }

			public string Summary { get; set; }

			public string Url { get; set; }

			public override string ToString()
			{
				var vevent = new StringBuilder();

				vevent.Append("BEGIN:VCALENDAR\r\n");
				vevent.Append("VERSION:1.0\r\n");
				vevent.Append("BEGIN:VEVENT\r\n");

				AppendDateField(vevent, "DCREATED", Created);
				AppendDateField(vevent, "DTSTART", Start);
				AppendDateField(vevent, "DTEND", End);

				AppendField(vevent, "SUMMARY", Summary);
				AppendField(vevent, "DESCRIPTION", Description);
				AppendField(vevent, "LOCATION", Location);
				AppendField(vevent, "ORGANIZER", Organizer);
				AppendField(vevent, "URL", Url);

				vevent.Append("END:VEVENT\r\n");
				vevent.Append("END:VCALENDAR\r\n");

				return vevent.ToString();
			}

			private static void AppendField(StringBuilder vevent, string name, string value)
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}

				vevent.AppendFormat("{0}: {1}\r\n", name, value);
			}

			private static void AppendDateField(StringBuilder vevent, string name, DateTime? value)
			{
				if (value == null)
				{
					return;
				}

				AppendField(vevent, name, value.Value.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
			}
		}
	}
}
