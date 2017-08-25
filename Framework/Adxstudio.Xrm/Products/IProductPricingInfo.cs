/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Products
{
	public interface IProductPricingInfo
	{
		bool IsSale { get; }

		decimal Price { get; }

		string PriceListName { get; }
		
		decimal RegularPrice { get; }

		DateTime SaleEndDate { get; }

		DateTime SaleStartDate { get; }

		decimal SalePrice { get; }

		decimal SalePriceSavingAmount { get; }
	}
}
