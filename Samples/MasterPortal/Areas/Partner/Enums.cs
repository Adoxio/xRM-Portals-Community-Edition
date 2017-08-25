/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site.Areas.Partner
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
	}
}
