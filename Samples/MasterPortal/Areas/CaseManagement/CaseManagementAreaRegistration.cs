/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.CaseManagement
{
	public class CaseManagementAreaRegistration : AreaRegistration 
	{
		public override string AreaName 
		{
			get 
			{
				return "CaseManagement";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context) 
		{
			context.MapRoute(
				"CaseManagement_default",
				"CaseManagement/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional });
		}
	}
}
