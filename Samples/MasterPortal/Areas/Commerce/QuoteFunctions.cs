/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adxstudio.Xrm.Commerce;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;

namespace Site.Areas.Commerce
{
	public static class QuoteFunctions
	{
		public static CommerceQuote CreateQuote(PayPalHelper payPal, bool aggregateData, bool itemizedData, Entity website, IShoppingCart cart, IPortalContext portal)
		{
			var args = new Dictionary<string, string>();

			if (aggregateData)
			{
				args.Add("item_name", "Aggregated Items");
				args.Add("amount", cart.GetCartTotal().ToString("#.00"));
			}
			// Paypal Item Data for itemized data.
			else if (itemizedData)
			{
				var cartItems = cart.GetCartItems().Select(sci => sci.Entity);

				var counter = 0;

				foreach (var item in cartItems)
				{
					counter++;

					args.Add(string.Format("item_name_{0}", counter), item.GetAttributeValue<string>("adx_name"));
					args.Add(string.Format("amount_{0}", counter), item.GetAttributeValue<Money>("adx_quotedprice") == null ? "0.00" : item.GetAttributeValue<Money>("adx_quotedprice").Value.ToString("#.00"));
					args.Add(string.Format("quantity_{0}", counter), item.GetAttributeValue<int?>("adx_quantity").GetValueOrDefault(0).ToString(CultureInfo.InvariantCulture));
					//add arguments for shipping/handling cost?
					args.Add(string.Format("item_number_{0}", counter), item.GetAttributeValue<Guid>("adx_shoppingcartitemid").ToString());
				}

				// If we are calculating the tax, this is done and added as an arg.
			}		

			// If a quote was created, pass in the quote ID.
			args.Add("invoice", cart.Id.ToString());

			return new CommerceQuote(args, portal, "PayPal");
		}
	}
}
