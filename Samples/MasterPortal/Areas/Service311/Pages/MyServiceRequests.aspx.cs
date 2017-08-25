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

namespace Site.Areas.Service311.Pages
{
	public partial class MyServiceRequests : PortalPage
	{
		private const string SavedQueryName = "Status Web View";
		private const string EntityName = "adx_servicerequest";

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();
		}

		protected void Page_Init(object sender, EventArgs args)
		{
			if (Session != null && Session.SessionID != null)
			{
				ViewStateUserKey = Session.SessionID;
			}
		}
	}
}
