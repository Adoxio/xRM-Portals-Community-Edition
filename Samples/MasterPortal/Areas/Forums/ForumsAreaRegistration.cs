/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Forums
{
	public class ForumsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Forums"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
		}
	}
}
