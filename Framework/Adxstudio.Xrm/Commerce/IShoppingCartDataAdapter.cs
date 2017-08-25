/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Provides an interface for <see cref="IShoppingCart"/> (adx_shoppingcart) data.
	/// </summary>
	public interface IShoppingCartDataAdapter
	{
		/// <summary>
		/// Selects the current website global shopping cart for the current user/visitor.
		/// </summary>
		IShoppingCart SelectCart();

		/// <summary>
		/// Selects a shopping cart in the current website, by ID.
		/// </summary>
		IShoppingCart SelectCart(Guid id);

		/// <summary>
		/// Creates the current website global shopping cart for the current user/visitor.
		/// </summary>
		IShoppingCart CreateCart();
	}
}
