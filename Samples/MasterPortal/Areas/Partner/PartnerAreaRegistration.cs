/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Site.Areas.Partner
{
	public class PartnerAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Partner"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
		}
	}
}
