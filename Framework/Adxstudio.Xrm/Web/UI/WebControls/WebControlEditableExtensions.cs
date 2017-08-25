/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	public static class WebControlEditableExtensions
	{
		public static void RegisterClientSideDependencies(this WebControl control)
		{
			var manager = SiteEditingManager.GetCurrent(control.Page);

			if (manager == null)
			{
				return;
			}

			manager.RegisterClientSideDependencies(control);
		}
	}
}
