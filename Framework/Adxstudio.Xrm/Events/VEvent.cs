/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Text;

namespace Adxstudio.Xrm.Events
{
	public class VEvent
	{
		public DateTime? Created { get; set; }

		public string Description { get; set; }

		public string DescriptionHtml { get; set; }

		public DateTime? End { get; set; }

		public string Location { get; set; }

		public string Organizer { get; set; }

		public string RecurrenceRule { get; set; }

		public DateTime? Start { get; set; }

		public string Summary { get; set; }

		public DateTime? Timestamp { get; set; }

		public string Uid { get; set; }

		public string Url { get; set; }

		public void AppendTo(StringBuilder sb)
		{
			VCalendar.AppendField(sb, "BEGIN", "VEVENT");
			VCalendar.AppendField(sb, "UID", Uid);
			VCalendar.AppendDateField(sb, "DCREATED", Created);
			VCalendar.AppendDateField(sb, "DTSTART", Start);
			VCalendar.AppendDateField(sb, "DTEND", End);
			VCalendar.AppendDateField(sb, "DTSTAMP", Timestamp);
			VCalendar.AppendField(sb, "SUMMARY", Summary);
			VCalendar.AppendField(sb, "DESCRIPTION", Description);
			VCalendar.AppendField(sb, "X-ALT-DESC;FMTTYPE=text/html", DescriptionHtml);
			VCalendar.AppendField(sb, "LOCATION", Location);
			VCalendar.AppendField(sb, "ORGANIZER", Organizer, false);
			VCalendar.AppendField(sb, "URL", Url);
			VCalendar.AppendField(sb, "RRULE", RecurrenceRule, false);
			VCalendar.AppendField(sb, "BEGIN", "VALARM", false);
			VCalendar.AppendField(sb, "TRIGGER", "-PT15M", false);
			VCalendar.AppendField(sb, "ACTION", "DISPLAY", false);
			VCalendar.AppendField(sb, "DESCRIPTION", Summary);
			VCalendar.AppendField(sb, "END", "VALARM", false);
			VCalendar.AppendField(sb, "END", "VEVENT");
		}

		public override string ToString()
		{
			var vevent = new StringBuilder();

			AppendTo(vevent);

			return vevent.ToString();
		}
	}
}
