/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Retail
{
	public class RetailAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Retail";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
	
		}
	}
}
