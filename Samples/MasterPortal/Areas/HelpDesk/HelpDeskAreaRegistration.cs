/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.HelpDesk
{
	public class HelpDeskAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "HelpDesk"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
		}
	}
}
