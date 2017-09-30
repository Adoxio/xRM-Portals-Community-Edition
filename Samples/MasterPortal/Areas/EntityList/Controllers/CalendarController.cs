/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;

namespace Site.Areas.EntityList.Controllers
{
	public class CalendarController : Controller
	{
		[HttpGet]
		public ActionResult Index(Guid entityListId, Guid viewId, string from, string to, string filter, string search)
		{
			DateTime fromDate;

			if (!DateTime.TryParseExact(from, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out fromDate))
			{
				return Error(ResourceManager.GetString("Unable_To_Parse_From_Parameter_Error"));
			}

			DateTime toDate;

			if (!DateTime.TryParseExact(to, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out toDate))
			{
				return Error(ResourceManager.GetString("Unable_To_Parse_Parameter_In_Required_ToDate_Format_Exception"));
			}
			try
			{
				var dataAdapter = new EntityListCalendarDataAdapter(
					new EntityReference("adx_entitylist", entityListId),
					new EntityReference("savedquery", viewId),
					new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext));

				var events = dataAdapter.SelectEvents(fromDate, toDate, filter, search).OrderBy(e => e.Start).ThenBy(e => e.Summary);

				return new JObjectResult(new JObject { { "success", 1 }, { "result", new JArray(events.Select(GetEventJson)) } });
			}
			catch (LocalizedException ex)
			{
				return Error(ex.Message);
			}
			catch
			{
				return Error(ResourceManager.GetString("CalendarControl_LoadingError"));
			}
		}

		[HttpGet]
		public ActionResult Download(Guid entityListId, Guid viewId, string from, string to, string filter, string search)
		{
			DateTime parsedFrom;

			var fromDate = DateTime.TryParseExact(from, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedFrom)
				? parsedFrom
				: DateTime.UtcNow.AddYears(-10);

			DateTime parsedTo;

			var toDate = DateTime.TryParseExact(to, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedTo)
				? parsedTo
				: DateTime.UtcNow.AddYears(10);

			var dataAdapter = new EntityListCalendarDataAdapter(
				new EntityReference("adx_entitylist", entityListId),
				new EntityReference("savedquery", viewId),
				new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext));

			var events = dataAdapter.SelectEvents(fromDate, toDate, filter, search)
				.OrderBy(e => e.Start)
				.ThenBy(e => e.Summary);

			var vcalendar = new VCalendar(events.Select(GetVEvent));

			return new ContentResult
			{
				ContentType = "text/calendar",
				Content = vcalendar.ToString()
			};
		}
		
		[HttpGet, OutputCache(Duration = 86400, VaryByParam = "none", Location = OutputCacheLocation.ServerAndClient)]
		public ActionResult Language()
		{
			return View(CultureInfo.CurrentCulture);
		}

		private JObject GetEventJson(EntityListEvent @event)
		{
			var json = new JObject
			{
				{ "id", @event.EntityReference.Id.ToString() },
				{ "class", "event-info" },
				{ "title", @event.Summary },
				{ "start", ToUnixMilliseconds(@event.Start) },
				{ "end", ToUnixMilliseconds(@event.End.GetValueOrDefault(@event.Start)) },
				{ "description", @event.Description },
				{ "location", @event.Location },
			};

			if (!string.IsNullOrEmpty(@event.Url))
			{
				json["url"] = @event.Url;
			}

			if (@event.Organizer != null)
			{
				json["organizer"] = new JObject
				{
					{ "name", @event.Organizer.Name },
					{ "email", @event.Organizer.Email },
				};
			}

			if (@event.TimeZoneDisplayMode == EntityListTimeZoneDisplayMode.SpecificTimeZone && @event.TimeZone != null)
			{
				json["timezoneoffset"] = (long)@event.TimeZone.GetUtcOffset(@event.Start).TotalMilliseconds;
				json["timezonedisplayname"] = @event.TimeZone.DisplayName;
			}

			return json;
		}

		private VEvent GetVEvent(EntityListEvent @event)
		{
			var vevent = new VEvent
			{
				Uid = "{0}@{1}".FormatWith(@event.EntityReference.Id, Request.Url.Host),
				Start = @event.Start,
				End = @event.End,
				Timestamp = DateTime.UtcNow,
				Summary = @event.Summary,
				Description = VCalendar.StripHtml(@event.Description),
				DescriptionHtml = @event.Description,
				Location = @event.Location,
				Url = string.IsNullOrEmpty(@event.Url) ? null : new UrlBuilder(@event.Url).ToString()
			};

			if (@event.Organizer != null)
			{
				vevent.Organizer = VCalendar.FormatOrganizer(@event.Organizer.Name, @event.Organizer.Email);
			}

			return vevent;
		}

		private ActionResult Error(string message)
		{
			return new JObjectResult(new JObject
			{
				{ "success", 0 },
				{ "error", message }
			});
		}

		private static long ToUnixMilliseconds(DateTime dateTime)
		{
			 return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}
	}
}
