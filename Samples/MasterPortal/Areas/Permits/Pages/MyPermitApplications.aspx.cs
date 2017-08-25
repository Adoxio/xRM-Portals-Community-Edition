/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.UI;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Permits.Pages
{
	public partial class MyPermitApplications : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();
		}

	}
}
