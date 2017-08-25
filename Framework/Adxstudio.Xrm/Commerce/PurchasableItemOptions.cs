/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	public class PurchasableItemOptions : IPurchasableItemOptions
	{
		public PurchasableItemOptions(EntityReference quoteProduct, bool? isSelected = null, decimal? quantity = null, string instructions = null)
		{
			if (quoteProduct == null) throw new ArgumentNullException("quoteProduct");

			QuoteProduct = quoteProduct;
			IsSelected = isSelected;
			Quantity = quantity;
			Instructions = instructions;
		}

		public string Instructions { get; private set; }

		public bool? IsSelected { get; private set; }

		public decimal? Quantity { get; private set; }

		public EntityReference QuoteProduct { get; private set; }
	}
}
