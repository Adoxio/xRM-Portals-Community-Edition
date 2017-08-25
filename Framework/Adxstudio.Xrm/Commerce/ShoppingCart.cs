/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Client.Messages;

namespace Adxstudio.Xrm.Commerce
{
	public class ShoppingCart : IShoppingCart
	{
		private OrganizationServiceContext _context;

		public Entity Entity { get; private set; }

		public Guid Id { get; private set; }

		public ShoppingCart(Entity cart, OrganizationServiceContext context)
		{
			if (cart == null)
			{
				throw new ArgumentNullException("cart");
			}

			if (cart.LogicalName != "adx_shoppingcart")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", cart.LogicalName), "cart");
			}

				Entity = cart;
				Id = cart.Id;
				_context = context;
		}

		public T GetAttributeValue<T>(string attributeName)
		{
			return Entity.GetAttributeValue<T>(attributeName);
		}

		public IEnumerable<IShoppingCartItem> GetCartItems()
		{
			var items =
				_context.CreateQuery("adx_shoppingcartitem").Where(
					sci => sci.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id
						   == Entity.GetAttributeValue<Guid>("adx_shoppingcartid")).Select(sci => new ShoppingCartItem(sci, _context));

			return items;
		}

		public IShoppingCartItem GetCartItemByID(Guid id)
		{
			var findShoppingCartItems = _context.CreateQuery("adx_shoppingcartitem")
				.Where(sci => sci.GetAttributeValue<Guid>("adx_shoppingcartitemid") == id).Select(
					sci => new ShoppingCartItem(sci, _context));

			return findShoppingCartItems.FirstOrDefault();
		}

		public decimal GetCartTotal()
		{
			var cartItems = GetCartItems();

			var total = cartItems.Sum(item => item.Price == null ? 0 : item.Price.Value * item.Quantity);

			return total;
		}

		public void AddProductToCart(Entity product, string priceListName)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var uom = context.GetDefaultUomByProduct(product);

			AddProductToCart(product, uom, priceListName);
		}

		//public void AddProductToCart(Guid productID, string priceListName, Entity website)
		//{
		//    AddProductToCart(productID, (!(string.IsNullOrEmpty(priceListName)) ? priceListName : _context.GetDefaultPriceListName(website)));
		//}

		public void AddProductToCart(Guid productID, Entity uom, string priceListName)
		{
			var findProduct =
				from p in _context.CreateQuery("product")
				where p.GetAttributeValue<Guid>("productid") == productID
				select p;

			var product = findProduct.FirstOrDefault();

			if (product == null)
			{
				throw new ArgumentException("Unable to find product with ID {0}.".FormatWith(productID));
			}

			AddProductToCart(product, uom, priceListName);
			
		}

		public void AddProductToCart(Guid productID, string priceListName, int quantity)
		{
			var findProduct =
				from p in _context.CreateQuery("product")
				where p.GetAttributeValue<Guid>("productid") == productID
				select p;

			var product = findProduct.FirstOrDefault();

			if (product == null)
			{
				throw new ArgumentException("Unable to find product with ID {0}.".FormatWith(productID));
			}

			AddProductToCart(product, priceListName, quantity);
		}
		public void AddProductToCart(Entity product, string priceListName, int quantity)
		{
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;

			var uom = context.GetDefaultUomByProduct(product);

			AddProductToCart(product, uom, priceListName, quantity);
		}
		public void AddProductToCart(Entity product, Entity uom, string priceListName, int quantity)
		{
			product.AssertEntityName("product");

			var items = GetCartItems().Where(i => product.ToEntityReference().Equals(i.GetAttributeValue<EntityReference>("adx_productid")));

			// Check if this product is already in the cart
			if (items.Any())
			{
				//update the first item
				var item = items.FirstOrDefault() as ShoppingCartItem;

				item.Quantity = item.Quantity + (quantity == 0 ? 1 : quantity);
				item.UpdateItemPrice(priceListName);

				//Other items are bugs; there should be no others

				return;
			}

			//else we create a new shopping cart item
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var website = portal.Website;

			var priceListItem = _context.GetPriceListItemByPriceListNameAndUom(product, uom.Id, (!(string.IsNullOrEmpty(priceListName))
				? priceListName : _context.GetDefaultPriceListName(website))) ??
				_context.GetPriceListItemByPriceListName(product, _context.GetDefaultPriceListName(website));

			//var quotedPrice = _context.GetProductPriceByPriceListNameAndUom(product, uom.Id, (!(string.IsNullOrEmpty(priceListName))
			//    ? priceListName : _context.GetDefaultPriceListName(website))) ??
			//    _context.GetProductPriceByPriceListName(product, _context.GetDefaultPriceListName(website));

			var shoppingCartItem = new Entity("adx_shoppingcartitem");

			shoppingCartItem["adx_quantity"] = (decimal)quantity;
			shoppingCartItem["adx_name"] = "{0}-{1}-{2}".FormatWith(Entity.GetAttributeValue<string>("adx_name"), product.GetAttributeValue<string>("name"), DateTime.UtcNow);
			shoppingCartItem["adx_shoppingcartid"] = Entity.ToEntityReference();
			shoppingCartItem["adx_productid"] = product.ToEntityReference();
			shoppingCartItem["adx_uomid"] = uom.ToEntityReference();
			if (priceListItem != null)
			{
				shoppingCartItem["adx_productpricelevelid"] = priceListItem.GetAttributeValue<EntityReference>("pricelevelid");
				shoppingCartItem["adx_quotedprice"] = priceListItem.GetAttributeValue<Money>("amount");
			}

			_context.AddObject(shoppingCartItem);

			if (!_context.IsAttached(Entity)) _context.Attach(Entity);

			_context.UpdateObject(Entity);

			_context.SaveChanges();
		}

		public void AddProductToCart(Guid productID, string priceListName)
		{
			var findProduct =
				from p in _context.CreateQuery("product")
				where p.GetAttributeValue<Guid>("productid") == productID
				select p;

			var product = findProduct.FirstOrDefault();

			if (product == null)
			{
				throw new ArgumentException("Unable to find product with ID {0}.".FormatWith(productID));
			}

			AddProductToCart(product, priceListName);
		}

		public void AddProductToCart(Entity product, Entity uom, string priceListName)
		{
			product.AssertEntityName("product");

			var items = GetCartItems().Where(i => product.ToEntityReference().Equals(i.GetAttributeValue<EntityReference>("adx_productid")));

			// Check if this product is already in the cart
			if (items.Any())
			{
				//update the first item
				var item = items.FirstOrDefault() as ShoppingCartItem;

				item.Quantity = item.Quantity + 1;
				item.UpdateItemPrice(priceListName);

				//Other items are bugs; there should be no others

				return;
			}

			//else we create a new shopping cart item
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var website = portal.Website;

			var priceListItem = _context.GetPriceListItemByPriceListNameAndUom(product, uom.Id, (!(string.IsNullOrEmpty(priceListName))
				? priceListName : _context.GetDefaultPriceListName(website))) ??
				_context.GetPriceListItemByPriceListName(product, _context.GetDefaultPriceListName(website));

			//var quotedPrice = _context.GetProductPriceByPriceListNameAndUom(product, uom.Id, (!(string.IsNullOrEmpty(priceListName))
			//    ? priceListName : _context.GetDefaultPriceListName(website))) ??
			//    _context.GetProductPriceByPriceListName(product, _context.GetDefaultPriceListName(website));

			var shoppingCartItem = new Entity("adx_shoppingcartitem");

			shoppingCartItem["adx_quantity"] = (decimal)1;
			shoppingCartItem["adx_name"] = "{0}-{1}-{2}".FormatWith(Entity.GetAttributeValue<string>("adx_name"), product.GetAttributeValue<string>("name"), DateTime.UtcNow);
			shoppingCartItem["adx_shoppingcartid"] = Entity.ToEntityReference();
			shoppingCartItem["adx_productid"] = product.ToEntityReference();
			shoppingCartItem["adx_uomid"] = uom.ToEntityReference();
			if (priceListItem != null)
			{
				shoppingCartItem["adx_productpricelevelid"] = priceListItem.GetAttributeValue<EntityReference>("pricelevelid");
				shoppingCartItem["adx_quotedprice"] = priceListItem.GetAttributeValue<Money>("amount");
			}

			_context.AddObject(shoppingCartItem);

			if (!_context.IsAttached(Entity)) _context.Attach(Entity);

			_context.UpdateObject(Entity);

			_context.SaveChanges();
		}

		public void RemoveItemFromCart(Guid itemID)
		{
			var item = _context.CreateQuery("adx_shoppingcartitem").FirstOrDefault(sci => sci.GetAttributeValue<Guid>("adx_shoppingcartitemid") == itemID);

			if (item != null)
			{
				_context.DeleteObject(item);

				_context.SaveChanges();
			}
		}

		public void SetConfirmationNumber(string confirmationNumber)
		{
			Entity.Attributes["adx_confirmationnumber"] = confirmationNumber;

			if (!_context.IsAttached(Entity)) _context.Attach(Entity);

			_context.UpdateObject(Entity);

			_context.SaveChanges();
		}

		public void DeactivateCart()
		{
			if (!_context.IsAttached(Entity)) _context.Attach(Entity);

			_context.SetState(1, 2, Entity);

			_context.SaveChanges();
		}
	}
}
