/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	public interface IShoppingCart
	{
		Entity Entity { get; }

		Guid Id { get; }

		T GetAttributeValue<T>(string attributeName);

		IEnumerable<IShoppingCartItem> GetCartItems();

		IShoppingCartItem GetCartItemByID(Guid itemId);

		decimal GetCartTotal();

		void AddProductToCart(Entity product, string priceListName);

		void AddProductToCart(Guid productId, string priceListName);

		void DeactivateCart();

		void RemoveItemFromCart(Guid itemId);
	}
}
