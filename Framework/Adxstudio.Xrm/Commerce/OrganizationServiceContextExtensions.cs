/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	public static class OrganizationServiceContextExtensions
	{
		public static IEnumerable<Entity> GetCartsForContact(this OrganizationServiceContext context, Entity contact, Entity website)
		{
			contact.AssertEntityName("contact");
			website.AssertEntityName("adx_website");

			var findShoppingCarts =
				from sc in context.CreateQuery("adx_shoppingcart")
				where sc.GetAttributeValue<EntityReference>("adx_contactid") == contact.ToEntityReference()
					&& sc.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()
					&& sc.GetAttributeValue<OptionSetValue>("statuscode") != null && sc.GetAttributeValue<OptionSetValue>("statuscode").Value == 1
					&& sc.GetAttributeValue<bool?>("adx_system").GetValueOrDefault(false) == false
				select sc;

			return findShoppingCarts;
		}

		public static IEnumerable<Entity> GetCartsForVisitor(this OrganizationServiceContext context, string visitorId, Entity website)
		{
			website.AssertEntityName("adx_website");

			var findShoppingCarts =
				from sc in context.CreateQuery("adx_shoppingcart")
				where sc.GetAttributeValue<string>("adx_visitorid") == visitorId
					&& sc.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference()
					&& sc.GetAttributeValue<OptionSetValue>("statuscode") != null && sc.GetAttributeValue<OptionSetValue>("statuscode").Value == 1
					&& sc.GetAttributeValue<bool?>("adx_system").GetValueOrDefault(false) == false
				select sc;

			return findShoppingCarts;
		}

		public static Entity GetCartItemByID(this OrganizationServiceContext context, Guid id)
		{
			var findShoppingCartItem =
				from sc in context.CreateQuery("adx_shoppingcartitem")
				where sc.GetAttributeValue<Guid>("adx_shoppingcartitemid") == id
				select sc;

			return findShoppingCartItem.FirstOrDefault();
		}

		public static IEnumerable<Entity> GetOrdersForContact(this OrganizationServiceContext context, Entity contact)
		{

			var findOrders =
				from o in context.CreateQuery("salesorder")
				where o.GetAttributeValue<EntityReference>("customerid") == contact.ToEntityReference()
				select o;

			return findOrders;
		}

		public static IEnumerable<Entity> GetQuotesForContact(this OrganizationServiceContext context, Entity contact)
		{

			var findOrders =
				from o in context.CreateQuery("quote")
				where o.GetAttributeValue<EntityReference>("customerid") == contact.ToEntityReference()
				select o;

			return findOrders;
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Entity product, Entity website, string priceListName)
		{
			product.AssertEntityName("product");

			website.AssertEntityName("adx_website");

			var priceListItem = context.GetPriceListItemByPriceListName(product, priceListName);

			var returnPrice = priceListItem != null ? priceListItem.GetAttributeValue<Money>("amount") : null;

			if (returnPrice == null)
			{
				priceListItem = context.GetPriceListItemByPriceListName(product, context.GetDefaultPriceListName(website));

				returnPrice = priceListItem != null ? priceListItem.GetAttributeValue<Money>("amount") : null;
			}

			return returnPrice;
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Guid productID, Entity website, string priceListName)
		{
			website.AssertEntityName("adx_website");

			var product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == productID);

			return context.GetProductPriceByPriceListName(product, website, priceListName);
		}

		public static string GetPriceListNameForParentAccount(this OrganizationServiceContext context, Entity contact)
		{
			var priceLevel = context.GetPriceListForParentAccount(contact);

			return priceLevel != null ? priceLevel.GetAttributeValue<string>("name") : null;
		}

		public static Entity GetPriceListForParentAccount(this OrganizationServiceContext context, Entity contact)
		{
			contact.AssertEntityName("contact");

			var priceLevels = from pricelevel in context.CreateQuery("pricelevel")
								join account in context.CreateQuery("account")
								on pricelevel.GetAttributeValue<Guid>("pricelevelid") equals account.GetAttributeValue<EntityReference>("defaultpricelevelid").Id
								where account.GetAttributeValue<Guid>("accountid") == (contact.GetAttributeValue<EntityReference>("parentcustomerid") == null ? Guid.Empty : contact.GetAttributeValue<EntityReference>("parentcustomerid").Id)
								select pricelevel;

			return priceLevels.FirstOrDefault();
		}

		public static string GetDefaultPriceListName(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var defaultPriceLevelSetting = context.CreateQuery("adx_sitesetting")
				.Where(ss => ss.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference())
				.FirstOrDefault(purl => purl.GetAttributeValue<string>("adx_name") == "Ecommerce/DefaultPriceLevelName");

			return defaultPriceLevelSetting == null || string.IsNullOrWhiteSpace(defaultPriceLevelSetting.GetAttributeValue<string>("adx_value")) ? "Web" : defaultPriceLevelSetting.GetAttributeValue<string>("adx_value");
		}

		public static string GetDefaultPriceListName(this OrganizationServiceContext context, Guid websiteid)
		{
			var defaultPriceLevelSetting = context.CreateQuery("adx_sitesetting")
				.Where(ss => ss.GetAttributeValue<EntityReference>("adx_websiteid") == new EntityReference("adx_website", websiteid))
				.FirstOrDefault(purl => purl.GetAttributeValue<string>("adx_name") == "Ecommerce/DefaultPriceLevelName");

			return defaultPriceLevelSetting == null || string.IsNullOrWhiteSpace(defaultPriceLevelSetting.GetAttributeValue<string>("adx_value")) ? "Web" : defaultPriceLevelSetting.GetAttributeValue<string>("adx_value");
		}

		public static Entity GetDefaultUomByProduct(this OrganizationServiceContext context, Entity product)
		{
			var uom = context.CreateQuery("uom").FirstOrDefault(um => um.GetAttributeValue<Guid>("uomid") == product.GetAttributeValue<EntityReference>("defaultuomid").Id);

			return uom;
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Entity product, string priceListName)
		{
			product.AssertEntityName("product");

			return context.GetProductPriceByPriceListName(product.GetAttributeValue<Guid>("productid"), priceListName);
		}

		public static Money GetProductPriceByPriceListName(this OrganizationServiceContext context, Guid productID, string priceListName)
		{
			var priceListItem = context.GetPriceListItemByPriceListName(productID, priceListName);

			return priceListItem != null ? priceListItem.GetAttributeValue<Money>("amount") : null;
		}

		public static Entity GetPriceListItemByPriceListName(this OrganizationServiceContext context, Guid productID, string priceListName)
		{
			var product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == productID);

			return context.GetPriceListItemByPriceListName(product, priceListName);
		}

		public static Entity GetPriceListItemByPriceListName(this OrganizationServiceContext context, Entity product, string priceListName)
		{
			product.AssertEntityName("product");

			var defaultUOM = context.GetDefaultUomByProduct(product);

			return context.GetPriceListItemByPriceListNameAndUom(product.GetAttributeValue<Guid>("productid"), defaultUOM.GetAttributeValue<Guid>("uomid"), priceListName);
		}

		public static Entity GetPriceListItemByPriceListNameAndUom(this OrganizationServiceContext context, Guid productID, Guid uomid, string priceListName)
		{
			var product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == productID);

			return context.GetPriceListItemByPriceListNameAndUom(product, uomid, priceListName);
		}

		public static Entity GetPriceListItemByPriceListNameAndUom(this OrganizationServiceContext context, Entity product, Guid uomid, string priceListName)
		{
			var priceListItems =
				from pl in context.CreateQuery("pricelevel")
				join ppl in context.CreateQuery("productpricelevel") on pl.GetAttributeValue<Guid>("pricelevelid") equals ppl.GetAttributeValue<EntityReference>("pricelevelid").Id
				where
					ppl.GetAttributeValue<EntityReference>("pricelevelid") != null &&
					ppl.GetAttributeValue<EntityReference>("productid") != null &&
					ppl.GetAttributeValue<EntityReference>("productid") == product.ToEntityReference() &&
					ppl.GetAttributeValue<EntityReference>("uomid") == new EntityReference("uom", uomid)
				where
					pl.GetAttributeValue<string>("name") == priceListName &&
					((pl.GetAttributeValue<DateTime?>("begindate") == null ||
					pl.GetAttributeValue<DateTime?>("begindate") <= DateTime.UtcNow) &&
					(pl.GetAttributeValue<DateTime?>("enddate") == null ||
					pl.GetAttributeValue<DateTime?>("enddate") >= DateTime.UtcNow))
				select ppl;
			
			return priceListItems.FirstOrDefault();
		}
	}
}
