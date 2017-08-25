/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Represents a potential purchase, backed by a Quote (quote).
	/// </summary>
	public interface IPurchasable
	{
		IPurchaseAddress BillToAddress { get; }

		decimal DiscountAmount { get; }

		IEnumerable<IDiscount> Discounts { get; }

		bool HasOptions { get; }

		IEnumerable<IPurchasableItem> Items { get; }

		EntityReference PriceList { get; }

		EntityReference Quote { get; }

		bool RequiresShipping { get; }

		IPurchaseAddress ShipToAddress { get; }

		decimal ShippingAmount { get; }

		bool SupportsQuantities { get; }

		decimal TotalAmount { get; }

		decimal TotalDiscount { get; }

		decimal TotalDiscountAmount { get; }

		decimal TotalLineItemAmount { get; }

		decimal TotalLineItemDiscountAmount { get; }

		decimal TotalPreShippingAmount { get; }

		decimal TotalTax { get; }
	}

	public interface IPurchaseAddress
	{
		string City { get; }

		string Country { get; }

		string Name { get; }

		string PostalCode { get; }

		string StateOrProvince { get; }

		string Line1 { get; }

		string Line2 { get; }

		string Line3 { get; }
	}

	public class PurchaseAddress : IPurchaseAddress
	{
		public string City { get; set; }

		public string Country { get; set; }

		public string Name { get; set; }

		public string PostalCode { get; set; }

		public string StateOrProvince { get; set; }

		public string Line1 { get; set; }

		public string Line2 { get; set; }

		public string Line3 { get; set; }
	}
}
