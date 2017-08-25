/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Commerce
{
	public class ShoppingCartItem : IShoppingCartItem
	{
		private OrganizationServiceContext _context;

		public Entity Entity { get; private set; }

		public Guid Id { get; private set; }

		public Money Price
		{
			get { return Entity.GetAttributeValue<Money>("adx_quotedprice"); }
			set 
			{ 
				Entity.Attributes["adx_quotedprice"] = value;
				if (!_context.IsAttached(Entity)) _context.Attach(Entity);
				_context.UpdateObject(Entity);
				_context.SaveChanges();
			}
		}

		public decimal Quantity
		{
			get { return Entity.GetAttributeValue<decimal>("adx_quantity"); }
			set
			{
				Entity.Attributes["adx_quantity"] = value;
				if (!_context.IsAttached(Entity)) _context.Attach(Entity);
				_context.UpdateObject(Entity);
				_context.SaveChanges();
			}
		}

		public ShoppingCartItem(Entity cartItem, OrganizationServiceContext context)
		{
			if (cartItem == null)
			{
				throw new ArgumentNullException("cartItem");
			}

			if (cartItem.LogicalName != "adx_shoppingcartitem")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", cartItem.LogicalName), "cartItem");
			}

			Entity = cartItem;
			Id = cartItem.Id;
			_context = context;
		}

		public T GetAttributeValue<T>(string attributeName)
		{
			return Entity.GetAttributeValue<T>(attributeName);
		}

		public Entity GetRelatedProduct()
		{
			return _context.CreateQuery("product")
				.FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == Entity.GetAttributeValue<EntityReference>("adx_productid").Id);
		}

		public void UpdateItemPrice(string priceListName)
		{
			var product = GetRelatedProduct();

			Price = _context.GetProductPriceByPriceListName(product, priceListName) ??
				_context.GetProductPriceByPriceListName(product, "Web") ?? null;
		}

	}
}
