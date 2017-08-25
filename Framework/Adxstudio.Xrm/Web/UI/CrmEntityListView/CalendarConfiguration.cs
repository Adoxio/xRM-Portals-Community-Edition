/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.EntityList;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.UI.CrmEntityListView
{
	/// <summary>
	/// Settings for displaying a calendar.
	/// </summary>
	public class CalendarConfiguration
	{
		private string _initialDateString;

		/// <summary>
		/// Enumeration of the types of the calendar view that can be displayed.
		/// </summary>
		public enum CalendarView
		{
			/// <summary>
			/// Display a full year view.
			/// </summary>
			Year = 756150000,
			/// <summary>
			/// Display a single month view.
			/// </summary>
			Month = 756150001,
			/// <summary>
			/// Display a single week view.
			/// </summary>
			Week = 756150002,
			/// <summary>
			/// Display a single day view.
			/// </summary>
			Day = 756150003
		}

		/// <summary>
		/// Enumeration of the styles of the calendar.
		/// </summary>
		public enum CalendarStyle
		{
			/// <summary>
			/// A full-sized calendar view will be rendered.
			/// </summary>
			Full = 756150000,
			/// <summary>
			/// Entities will be rendered as a list of events, with a supplementary calendar navigation element.
			/// </summary>
			List = 756150001,
		}

		/// <summary>
		/// Enumeration of the time zone display modes.
		/// </summary>
		public enum TimeZoneDisplayMode
		{
			/// <summary>
			/// User's local time
			/// </summary>
			UserLocalTimeZone = 756150000,
			/// <summary>
			/// Specific time zone.
			/// </summary>
			SpecificTimeZone = 756150001,
		}

		/// <summary>
		/// Indicates whether the calendar is enabled or not.
		/// </summary>
		public bool Enabled { get; set; }
		/// <summary>
		/// Indicates the start date/time of entity when rendered as an event. This field mapping is required for calendar view to function. Entities that do not have a value for this field will not be included in any calendar views.
		/// </summary>
		public string StartDateFieldName { get; set; }
		/// <summary>
		/// Indicates the end date/time of entity when rendered as an event.
		/// </summary>
		public string EndDateFieldName { get; set; }
		/// <summary>
		/// Field mapping for a short description/title of an event. This field mapping is required for calendar view to function. Entities that do not have a value for this field will not be included in any calendar views.
		/// </summary>
		public string SummaryFieldName { get; set; }
		/// <summary>
		/// Field mapping for a longer description of an event.
		/// </summary>
		public string DescriptionFieldName { get; set; }
		/// <summary>
		/// Field mapping for the organizer of an event. This can be a text field, or a lookup to a Contact record. By default, this field is only used by iCalendar exports.
		/// </summary>
		public string OrganizerFieldName { get; set; }
		/// <summary>
		/// Field mapping for the location of an event. By default, this field is only used by iCalendar exports.
		/// </summary>
		public string LocationFieldName { get; set; }
		/// <summary>
		/// Field mapping indicating whether an entity represents an "all day" event.
		/// </summary>
		public string IsAllDayFieldName { get; set; }
		/// <summary>
		/// The initial view to be shown when the user first loads the calendar. Default is "Month", but other options are "Day", "Year", and "Week".
		/// </summary>
		public CalendarView InitialView { get; set; }
		/// <summary>
		/// The string representing initial date on which the calendar view will be centered when the user first loads the page. This will be the current date by default when set to 'now'.
		/// </summary>
		public string InitialDateString
		{
			get { return string.IsNullOrWhiteSpace(_initialDateString) ? "now" : _initialDateString; }
			set { _initialDateString = value; }
		}
		/// <summary>
		/// Determines the style of user interface to be rendered. One of the following: Full calendar, Event list. Full calendar (default): A full-sized calendar view will be rendered. Event list: Entities will be rendered as a list of events, with a supplementary calendar navigation element.
		/// </summary>
		public CalendarStyle Style { get; set; }
		/// <summary>
		/// Determines the time zone display. One of the following: UserLocalTimeZone, SpecificTimeZone.
		/// </summary>
		public TimeZoneDisplayMode TimeZoneDisplay { get; set; }
		/// <summary>
		/// Standard name of a time zone. Used when TimeZoneDisplayMode is set to SpecificTimeZone.
		/// </summary>
		public string TimeZoneStandardName { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public CalendarConfiguration()
		{
			InitialView = CalendarView.Month;
			Style = CalendarStyle.Full;
			TimeZoneDisplay = TimeZoneDisplayMode.UserLocalTimeZone;
		}

		/// <summary>
		/// Constructor used by The ViewConfiguration Class
		/// </summary>
		public CalendarConfiguration(OrganizationServiceContext serviceContext, bool calendarEnabled,
						 string calendarInitialDateString, OptionSetValue calendarInitialView,
						 OptionSetValue calendarStyle, OptionSetValue calendarTimeZoneDisplayMode,
						 int? calendarDisplayTimeZone, string calendarStartDateFieldName,
						 string calendarEndDateFieldName, string calendarSummaryFieldName,
						 string calendarDescriptionFieldName, string calendarOrganizerFieldName,
						 string calendarLocationFieldName, string calendarAllDayFieldName)
		{
			Enabled = calendarEnabled;

			if (!string.IsNullOrWhiteSpace(calendarInitialDateString)) { InitialDateString = calendarInitialDateString;  }

			if (calendarInitialView != null)
			{
				if (Enum.IsDefined(typeof(CalendarView), calendarInitialView.Value))
				{
					InitialView = (CalendarView)calendarInitialView.Value;
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Calendar Initial View value '{0}' is not a valid value defined by CalendarConfiguration.CalendarView class.", calendarInitialView.Value));
				}
			}

			if (calendarStyle != null)
			{
				if (Enum.IsDefined(typeof(CalendarStyle), calendarStyle.Value))
				{
					Style = (CalendarStyle)calendarStyle.Value;
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Calendar Style value '{0}' is not a valid value defined by CalendarConfiguration.CalendarStyle class.", calendarStyle.Value));
					Style = CalendarStyle.Full;
				}
			}

			if (calendarTimeZoneDisplayMode != null)
			{
				if (Enum.IsDefined(typeof(TimeZoneDisplayMode), calendarTimeZoneDisplayMode.Value))
				{
					TimeZoneDisplay = (TimeZoneDisplayMode)calendarTimeZoneDisplayMode.Value;
				}
				else
				{
					TimeZoneDisplay = TimeZoneDisplayMode.UserLocalTimeZone;
				}
			}

			if (calendarDisplayTimeZone != null)
			{
				var calendarDisplayTimeZoneStandardName = EntityListFunctions.GetTimeZoneStandardName(serviceContext, calendarDisplayTimeZone.Value);

				if (!string.IsNullOrWhiteSpace(calendarDisplayTimeZoneStandardName))
				{
					TimeZoneStandardName = calendarDisplayTimeZoneStandardName;
				}
			}

			if (!string.IsNullOrWhiteSpace(calendarStartDateFieldName)) { StartDateFieldName = calendarStartDateFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarEndDateFieldName)) { EndDateFieldName = calendarEndDateFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarSummaryFieldName)) { SummaryFieldName = calendarSummaryFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarDescriptionFieldName)) { DescriptionFieldName = calendarDescriptionFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarOrganizerFieldName)) { OrganizerFieldName = calendarOrganizerFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarLocationFieldName)) { LocationFieldName = calendarLocationFieldName; }

			if (!string.IsNullOrWhiteSpace(calendarAllDayFieldName)) { IsAllDayFieldName = calendarAllDayFieldName; }
		}
	}
}
