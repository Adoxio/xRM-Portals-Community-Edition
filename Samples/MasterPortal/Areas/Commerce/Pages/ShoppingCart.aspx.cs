/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Commerce;
using Site.Pages;

namespace Site.Areas.Commerce.Pages
{
	public partial class ShoppingCartPage : PortalPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var dataAdapter = new ShoppingCartDataAdapter(
				new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext),
				Context.Profile.UserName);

			EditableShoppingCart.Cart = dataAdapter.SelectCart();
		}
	}
}
