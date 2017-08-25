/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	public interface IShoppingCartItem
	{
		Entity Entity { get; }

		Guid Id { get; }

		Money Price { get; set;  }

		decimal Quantity { get; set; }

		T GetAttributeValue<T>(string attributeName);
	}
}
