/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Site.Pages
{
	public partial class Error : PortalPage
	{
		public string AuthorizeNetError
		{
			get { return Html.Encode(Request["AuthorizeNetError"]); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			LoadError();
		}

		private void LoadError()
		{
			if (!string.IsNullOrEmpty(AuthorizeNetError))
			{
				AuthorizeNetErrorMessage.Visible = true;
				AuthorizeNetErrorMessage.Text = AuthorizeNetError;
			}
		}
	}
}
