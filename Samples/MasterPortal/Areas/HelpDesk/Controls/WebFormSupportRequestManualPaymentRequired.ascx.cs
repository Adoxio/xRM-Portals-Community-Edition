/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Site.Areas.HelpDesk.Controls
{
	public partial class WebFormSupportRequestManualPaymentRequired : WebFormUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (WebForm != null)
			{
				WebForm.ShowHideNextButton(false);
			}
		}
	}
}
