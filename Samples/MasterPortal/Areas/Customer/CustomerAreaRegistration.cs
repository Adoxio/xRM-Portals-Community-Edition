/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


using System.Web.Mvc;

namespace Site.Areas.Customer
{
	public class CustomerAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Customer"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
		}
	}
}
