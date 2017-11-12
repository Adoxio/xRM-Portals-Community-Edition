/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Adxstudio.Xrm.Events
{
	public class VCalendar
	{
		public VCalendar(IEnumerable<VEvent> events)
		{
			Events = events.ToArray();
		}

		public IEnumerable<VEvent> Events { get; private set; }

		public override string ToString()
		{
			var sb = new StringBuilder();

			AppendField(sb, "BEGIN", "VCALENDAR");
			AppendField(sb, "VERSION", "2.0");
			AppendField(sb, "PRODID", "-//Adxstudio Inc.//Adxstudio Portals//EN");

			foreach (var @event in Events)
			{
				@event.AppendTo(sb);
			}

			AppendField(sb, "END", "VCALENDAR");

			return sb.ToString();
		}

		public static void AppendField(StringBuilder vevent, string name, string value, bool encodeValue = true)
		{
			// If there's no value, don't append the field at all.
			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			// Get all the characters of the field (name + value), and break it into 72-character lines.
			var fieldCharacters = (name + ":" + (encodeValue ? EncodeValue(value) : value)).ToCharArray();

			for (var i = 0; i < fieldCharacters.Length; i++)
			{
				vevent.Append(fieldCharacters[i]);

				// If we are at the end of the field, write a CRLF.
				if (i == (fieldCharacters.Length - 1))
				{
					vevent.Append("\r\n");
				}
					// If we are at a 72 character boundary (but not the end), write a CRLF plus a space.
				else if (i != 0 && i % 72 == 0)
				{
					vevent.Append("\r\n ");
				}
			}
		}

		public static void AppendDateField(StringBuilder vevent, string name, DateTime? value)
		{
			if (value == null)
			{
				return;
			}

			AppendField(vevent, name, value.Value.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
		}

		public static string EncodeValue(string value)
		{
			if (value == null) throw new ArgumentNullException("value");

			// Replace illegal characters with escaped versions.
			value = Regex.Replace(value, @"([\\;,])", @"\$1");

			// Replace line breaks with escaped version.
			value = Regex.Replace(value, "(\n|\r|\r\n)", "\\n");

			return value;
		}

		public static string FormatOrganizer(string name = null, string email = null)
		{
			var parts = new List<string>();

			if (!string.IsNullOrEmpty(name))
			{
				parts.Add(string.Format(CultureInfo.InvariantCulture, "CN={0}", EncodeValue(name)));
			}

			if (!string.IsNullOrEmpty(email))
			{
				parts.Add(string.Format(CultureInfo.InvariantCulture, "MAILTO={0}", EncodeValue(email)));
			}

			if (parts.Count < 1)
			{
				return string.Empty;
			}
			
			return string.Join(";", parts);
		}

		public static string StripHtml(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}

			var document = new HtmlDocument();

			document.LoadHtml(html);

			return Regex.Replace(document.DocumentNode.InnerText, @"[\s\r\n]+", " ").Trim();
		}
	}
}
