/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site.Areas.Opportunities
{
	public class Enums
	{
		/// <summary>
		/// Enumeration for the types of alerts.
		/// </summary>
		public enum AlertType
		{
			New,
			None,
			Overdue,
			PotentiallyStalled
		}

		public enum AccountClassificationCode
		{
			Default = 1,
			Partner = 100000000
		}

		public enum IncidentState
		{
			Active = 0,
			Resolved = 1,
			Canceled = 2
		}

		public enum ActivityPointerState
		{
			Open = 0,
			Completed = 1,
			Canceled = 2,
			Scheduled = 3,
		}
	}
}
