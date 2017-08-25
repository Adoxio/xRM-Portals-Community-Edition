/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents full, extended info about a product.
	/// </summary>
	public interface IProduct
	{
		EntityReference Brand { get; }

		string BrandName { get; }

		EntityReference Currency { get; }

		decimal CurrentPrice { get; }

		bool CurrentUserCanWriteReview { get; }

		EntityReference DefaultPriceList { get; }

		string DefaultPriceListName { get; }

		EntityReference DefaultUnit { get; }

		string Description { get; }

		Entity Entity { get; }

		EntityReference EntityReference { get; }

		string ImageURL { get; }

		string ImageThumbnailURL { get; }

		bool IsInStock { get; }

		decimal ListPrice { get; }

		string ModelNumber { get; }

		string Name { get; }

		string PartialURL { get; }

		IProductPricingInfo PricingInfo { get; }

		decimal QuantityOnHand { get; }

		IProductRatingInfo RatingInfo { get; }

		DateTime ReleaseDate { get; }

		bool RequiresSpecialInstructions { get; }

		string SKU { get; }

		string SpecialInstructions { get; }

		string Specifications { get; }

		decimal StockVolume { get; }

		decimal StockWeight { get; }

		EntityReference Subject { get; }

		EntityReference UnitGroup { get; }
	}
}
