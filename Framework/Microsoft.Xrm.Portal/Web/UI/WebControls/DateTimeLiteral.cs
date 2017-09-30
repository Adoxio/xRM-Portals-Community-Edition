/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public class DateTimeLiteral : Literal
	{
		public object Value { get; set; }
		public string TimeZone { get; set; }
		public string TimeZoneLabel { get; set; }
		public string Format { get; set; }
		public string PortalName { get; set; }

		private bool _useSiteTimeZone = true;

		[DefaultValue(true)]
		public bool UseSiteTimeZone 
		{
			get { return _useSiteTimeZone; }
			set { _useSiteTimeZone = value; }
		}

		private bool _outputTimeZoneLabel = true;
		[DefaultValue(true)]
		public bool OutputTimeZoneLabel
		{
			get { return _outputTimeZoneLabel; }
			set { _outputTimeZoneLabel = value; }
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if (Value == null)
			{
				base.Render(writer);
				return;
			}

			DateTime dt;

			if (Value is DateTime)
			{
				dt = (DateTime)Value;
			}
			else if (Value is DateTime?)
			{
				dt = ((DateTime?)Value).Value;
			}
			else
			{
				dt = DateTime.Parse(Value.ToString());
			}

			if (dt.Kind == DateTimeKind.Utc)
			{
				// check if we are to use the default site timezone
				if (UseSiteTimeZone)
				{
					var portal = PortalCrmConfigurationManager.CreatePortalContext(PortalName);
					var tz = portal.GetTimeZone();
					dt = TimeZoneInfo.ConvertTimeFromUtc(dt, tz);

					// if no timezone label provided, use the display name of the current timezone
					if (string.IsNullOrEmpty(TimeZoneLabel))
					{
						TimeZoneLabel = tz.DisplayName;
					}
				}
				else if (!string.IsNullOrEmpty(TimeZone))  // a specific timezone is given
				{
					var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
					dt = TimeZoneInfo.ConvertTimeFromUtc(dt, tz);

					// if no timezone label provided, use the display name of the current timezone
					if (string.IsNullOrEmpty(TimeZoneLabel))
					{	
						TimeZoneLabel = tz.DisplayName;
					}
				}
			}
			
			// output the datetime in the correct format (default if not provided)
			Text = string.IsNullOrEmpty(Format) ? dt.ToString() : dt.ToString(Format);

			// append the time zone label if it is not disabled and if we have one
			if (!string.IsNullOrEmpty(TimeZoneLabel) && OutputTimeZoneLabel)
			{
				Text = "{0} {1}".FormatWith(Text, TimeZoneLabel);
			}

			base.Render(writer);
		}
	}

}
