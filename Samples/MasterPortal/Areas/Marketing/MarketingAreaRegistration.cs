/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Marketing
{
	public class MarketingAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Marketing"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"ManageSubscriptions",
				"_Marketing/ManageSubscriptions/{encodedEmail}/{signature}",
				new { action = "ManageSubscriptions", controller= "Marketing" });

			context.MapRoute(
				"Unsubscribe",
				"_Marketing/Unsubscribe/{encodedEmail}/{encodedList}/{signature}",
				new { action = "Unsubscribe", controller= "Marketing" });
		}
	}
}
