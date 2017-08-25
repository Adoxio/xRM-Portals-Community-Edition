/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Represents a single line item in an <see cref="IPurchasable"/>, backed by a Quote Product (quotedetail).
	/// </summary>
	public interface IPurchasableItem
	{
		decimal Amount { get; }

		decimal AmountAfterDiscount { get; }

		string Description { get; }

		IEnumerable<IDiscount> Discounts { get; }

		decimal ExtendedAmount { get; }

		bool IsOptional { get; }

		bool IsRequired { get; }

		bool IsSelected { get; }

		int LineItemNumber { get; }

		decimal ManualDiscountAmount { get; }

		string Name { get; }

		decimal PricePerUnit { get; }

		EntityReference Product { get; }

		decimal Quantity { get; }

		EntityReference QuoteProduct { get; }

		bool RequiresShipping { get; }

		bool SupportsQuantities { get; }

		decimal Tax { get; }

		decimal TotalDiscountAmount { get; }

		EntityReference UnitOfMeasure { get; }

		decimal VolumeDiscountAmount { get; }
	}
}
