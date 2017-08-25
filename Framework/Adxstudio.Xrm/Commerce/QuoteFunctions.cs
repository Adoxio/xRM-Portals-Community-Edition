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
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Commerce
{
	public static class QuoteFunctions
	{
		public static decimal GetDecimalFromMoney(Entity quote, string attributeLogicalName, decimal defaultValue = 0)
		{
			var value = quote.GetAttributeValue<Money>(attributeLogicalName);

			return value == null ? defaultValue : value.Value;
		}

		public static EntityReference GetPriceListCurrency(OrganizationServiceContext serviceContext, EntityReference priceList)
		{
			if (priceList == null) throw new ArgumentNullException("priceList");
			
			var priceListEntity = serviceContext.CreateQuery("pricelevel")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("pricelevelid") == priceList.Id);

			if (priceListEntity != null)
			{
				return priceListEntity.GetAttributeValue<EntityReference>("transactioncurrencyid");
			}
			throw new NotSupportedException("No currency was found for the price list with the ID of {0}.".FormatWith(priceList.Id));
		}

		public static EntityReference GetQuoteCustomer(OrganizationServiceContext serviceContext, EntityReference user, string visitorId)
		{
			if (user != null) { return user; }

			if (string.IsNullOrEmpty(visitorId))
			{
				throw new InvalidOperationException("Unable to create anonymous quote customer record.");
			}

			var visitor = new Entity("contact");

			visitor["firstname"] = "Anonymous Portal User";
			visitor["lastname"] = visitorId;
			visitor["adx_username"] = "anonymous:{0}".FormatWith(visitorId);

			serviceContext.AddObject(visitor);
			serviceContext.SaveChanges();

			return visitor.ToEntityReference();
		}

		public static string GetQuoteInstructions(IEnumerable<LineItem> lineItems)
		{
			return lineItems.Aggregate(
				new StringBuilder(),
				(sb, lineItem) => lineItem.Instructions == null
					? sb
					: sb.AppendFormat("{0}: {1}\n\n", lineItem.Product.GetAttributeValue<string>("name"), lineItem.Instructions))
				.ToString();
		}

		public static string GetQuoteName(OrganizationServiceContext serviceContext, Entity purchaseMetadata,  string webformName = null)
		{
			if (purchaseMetadata != null)
			{
				var quoteName = purchaseMetadata.GetAttributeValue<string>("adx_purchasequotename");

				if (!string.IsNullOrEmpty(quoteName))
				{
					return quoteName;
				}
			}

			if (!string.IsNullOrEmpty(webformName))
			{
				return webformName;
			}

			return "Commerce Quote Saved from Cart";
		}

		public static IEnumerable<LineItem> GetValidLineItems(IEnumerable<LineItem> lineItems)
		{
			return lineItems.Where(e =>
			{
				var state = e.Product.GetAttributeValue<OptionSetValue>("statecode");

				return state != null && state.Value == 0;
			});
		}

		public static EntityReference CreateQuote(IEnumerable<LineItem> lineItems, EntityReference purchaseEntity, OrganizationServiceContext context, 
			OrganizationServiceContext serviceContextForWrite,  EntityReference user, EntityReference priceList, string visitorId = null, EntityReference target = null, 
			Entity purchaseMetadata = null)
		{
			lineItems = lineItems.ToArray();

			// Filter line items to only valid (active) products, that are in the current price list. If there are
			// no line items remaining at that point either return null or throw an exception?

			var validLineItems = GetValidLineItems(lineItems).ToArray();

			if (!validLineItems.Any()) { return null; }

			var productIds = validLineItems.Select(e => e.Product.Id).ToArray();

			var priceListItems = context.CreateQuery("productpricelevel")
				.Where(e => e.GetAttributeValue<EntityReference>("pricelevelid") == priceList)
				.WhereIn(e => e.GetAttributeValue<Guid>("productid"), productIds)
				.ToArray()
				.ToLookup(e => e.GetAttributeValue<EntityReference>("productid").Id);

			// Determine the UOM for each line item. If there is only one price list item for the product in the price list,
			// use the UOM from that. If there is more than one, favour them in this order: explict UOM on line item, default
			// product UOM... then random, basically?

			var lineItemsWithUom = validLineItems
				.Select(e => new { LineItem = e, UnitOfMeasure = e.GetUnitOfMeasure(priceListItems[e.Product.Id]) })
				.Where(e => e.UnitOfMeasure != null)
				.OrderBy(e => e.LineItem.Optional)
				.ThenBy(e => e.LineItem.Order)
				.ThenBy(e => e.LineItem.Product.GetAttributeValue<string>("adx_name"))
				.ToArray();

			if (!lineItemsWithUom.Any()) { return null; }

			// Set quote customer to current portal user. If there is no current portal user, create a contact on the fly
			// using the web form session anonymous visitor ID.

			var quote = new Entity("quote");

			var customer = GetQuoteCustomer(context, user, visitorId);

			quote["pricelevelid"] = priceList;

			quote["transactioncurrencyid"] = GetPriceListCurrency(context, priceList);

			quote["customerid"] = customer;

			quote["adx_specialinstructions"] = GetQuoteInstructions(lineItems);

			quote["name"] = GetQuoteName(context, purchaseMetadata);

			// If the purchase entity or target entity is a shopping cart, set that on the quote.
			if (purchaseEntity.LogicalName == "adx_shoppingcart")
			{
				quote["adx_shoppingcartid"] = purchaseEntity;
			}
			else if (target != null && target.LogicalName == "adx_shoppingcart")
			{
				quote["adx_shoppingcartid"] = target;
			}

			if (user != null)
			{
				var contact = context.CreateQuery("contact")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("contactid") == user.Id);

				if (contact != null)
				{
					quote["billto_city"] = contact.GetAttributeValue<string>("address1_city");
					quote["billto_country"] = contact.GetAttributeValue<string>("address1_country");
					quote["billto_line1"] = contact.GetAttributeValue<string>("address1_line1");
					quote["billto_line2"] = contact.GetAttributeValue<string>("address1_line2");
					quote["billto_line3"] = contact.GetAttributeValue<string>("address1_line3");
					quote["billto_name"] = contact.GetAttributeValue<string>("fullname");
					quote["billto_postalcode"] = contact.GetAttributeValue<string>("address1_postalcode");
					quote["billto_stateorprovince"] = contact.GetAttributeValue<string>("address1_stateorprovince");

					quote["shipto_city"] = contact.GetAttributeValue<string>("address1_city");
					quote["shipto_country"] = contact.GetAttributeValue<string>("address1_country");
					quote["shipto_line1"] = contact.GetAttributeValue<string>("address1_line1");
					quote["shipto_line2"] = contact.GetAttributeValue<string>("address1_line2");
					quote["shipto_line3"] = contact.GetAttributeValue<string>("address1_line3");
					quote["shipto_name"] = contact.GetAttributeValue<string>("fullname");
					quote["shipto_postalcode"] = contact.GetAttributeValue<string>("address1_postalcode");
					quote["shipto_stateorprovince"] = contact.GetAttributeValue<string>("address1_stateorprovince");
				}
			}

			if (serviceContextForWrite == null) serviceContextForWrite = context;

			serviceContextForWrite.AddObject(quote);
			serviceContextForWrite.SaveChanges();

			var quoteReference = quote.ToEntityReference();

			var lineItemNumber = 1;

			foreach (var lineItem in lineItemsWithUom)
			{
				var quoteProduct = new Entity("quotedetail");

				quoteProduct["quoteid"] = quoteReference;
				quoteProduct["productid"] = lineItem.LineItem.Product.ToEntityReference();
				quoteProduct["quantity"] = lineItem.LineItem.Quantity;
				quoteProduct["uomid"] = lineItem.UnitOfMeasure;
				quoteProduct["description"] = lineItem.LineItem.Description;
				quoteProduct["lineitemnumber"] = lineItemNumber;
				quoteProduct["adx_isrequired"] = !lineItem.LineItem.Optional;

				serviceContextForWrite.AddObject(quoteProduct);

				lineItemNumber++;
			}

			serviceContextForWrite.SaveChanges();

			return quoteReference;
		}
	}
}
