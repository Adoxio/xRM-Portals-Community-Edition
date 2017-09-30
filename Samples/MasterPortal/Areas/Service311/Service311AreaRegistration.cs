/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Service311
{
	public class Service311AreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Service311";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute("ServiceRequestsMap", "Service311/Map/{action}", new { controller = "Map", action = "Search" });

		}
	}
}
