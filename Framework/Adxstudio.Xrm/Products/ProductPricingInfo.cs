/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents additional information pertaining to the price of a single product.
	/// </summary>
	public class ProductPricingInfo : IProductPricingInfo
	{
		/// <summary>
		/// Class Initialization
		/// </summary>
		/// <param name="price"></param>
		/// <param name="priceListName"></param>
		/// <param name="isSale"></param>
		/// <param name="regularPrice"></param>
		/// <param name="salePrice"></param>
		public ProductPricingInfo(decimal price, string priceListName, bool isSale, decimal regularPrice, decimal salePrice)
		{
			Price = price;
			PriceListName = priceListName;
			IsSale = isSale;
			RegularPrice = regularPrice;
			SalePrice = salePrice;
		}

		public decimal Price { get; set; }
		public string PriceListName { get; set; }
		public bool IsSale { get; set; }
		public decimal RegularPrice { get; set; }
		public DateTime SaleEndDate { get; set; }
		public DateTime SaleStartDate { get; set; }
		public decimal SalePrice { get; set; }
		public decimal SalePriceSavingAmount { get; set; }
	}
}
