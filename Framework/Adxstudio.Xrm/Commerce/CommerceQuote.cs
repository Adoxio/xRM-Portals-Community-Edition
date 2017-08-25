/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Commerce
{
	public class CommerceQuote
	{
		private OrganizationServiceContext _context;

		public Entity Entity { get; private set; }

		public Guid Id { get; private set; }

		public CommerceQuote(Entity quote, OrganizationServiceContext context)
		{
			if (quote == null)
			{
				throw new ArgumentNullException("quote");
			}

			if (quote.LogicalName != "quote")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", quote.LogicalName), "quote");
			}

			Entity = quote;
			Id = quote.Id;
			_context = context;
		}

		public CommerceQuote(Dictionary<string, string> values, IPortalContext xrm, string paymentProvider)
		{
			if (paymentProvider == "PayPal")
			{
				CreateQuotePayPal(values, xrm);
			}
		}

		public T GetAttributeValue<T>(string attributeName)
		{
			return Entity.GetAttributeValue<T>(attributeName);
		}

		private void CreateQuotePayPal(Dictionary<string, string> values, IPortalContext xrm)
		{
			_context = xrm.ServiceContext;

			if (!values.ContainsKey("invoice"))
			{
				throw new Exception("no invoice found");
			}

			var shoppingCart =
				_context.CreateQuery("adx_shoppingcart").FirstOrDefault(
					q => q.GetAttributeValue<Guid>("adx_shoppingcartid") == Guid.Parse(values["invoice"]));

			var quote = new Entity("quote");

			var orderGuid = Guid.NewGuid();

			quote.Attributes["quoteid"] = orderGuid;
			quote.Id = orderGuid;

			quote.Attributes["name"] = "quote created by: " + shoppingCart.GetAttributeValue<string>("adx_name");

			quote.Attributes["adx_shoppingcartid"] = shoppingCart.ToEntityReference();

			//Ensure that there is a customer
			var customer = GetQuoteCustomer(values, _context, shoppingCart);

			if (!_context.IsAttached(shoppingCart))
			{
				_context.Attach(shoppingCart);
			}

			shoppingCart.Attributes["adx_contactid"] = customer.ToEntityReference();

			quote.Attributes["customerid"] = customer.ToEntityReference();

			var priceLevel =
				_context.CreateQuery("pricelevel").FirstOrDefault(pl => pl.GetAttributeValue<string>("name") == "Web");

			if (priceLevel == null)
			{
                throw new Exception("price level null");
			}

			//Set the price level
			var priceLevelReference = priceLevel.ToEntityReference();

			quote.Attributes["pricelevelid"] = priceLevelReference;

			//Set the address for the order
			SetQuoteAddresses(values, quote, customer);

			//order.Attributes["adx_confirmationnumber"] = shoppingCart.GetAttributeValue<string>("adx_confirmationnumber");
			//order.Attributes["adx_receiptnumber"] = values.ContainsKey("ipn_trac_id") ? values["ipn_track_id"] : null;

			_context.AddObject(quote);

			_context.UpdateObject(shoppingCart);

			_context.SaveChanges();

			//Set the products of the order
			SetQuoteProducts(shoppingCart, _context, orderGuid);


			//Deactivate the shopping Cart
			shoppingCart =
				_context.CreateQuery("adx_shoppingcart").FirstOrDefault(
					q => q.GetAttributeValue<Guid>("adx_shoppingcartid") == Guid.Parse(values["invoice"]));

			try
			{
				_context.SetState(1, 2, shoppingCart);
				_context.SaveChanges();
			}
			catch
			{
				//Unlikely that there is an issue, most likely it has already been deactiveated.
			}

			Entity = _context.CreateQuery("quote").FirstOrDefault(o => o.GetAttributeValue<Guid>("quoteid") == orderGuid);
			Id = Entity.Id;
		}

		private static void SetQuoteProducts(Entity shoppingCart, OrganizationServiceContext context, Guid quoteGuid)
		{
			var cartItems = context.CreateQuery("adx_shoppingcartitem")
				.Where(
					qp =>
					qp.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id ==
					shoppingCart.GetAttributeValue<Guid>("adx_shoppingcartid")).ToList();

			foreach (var item in cartItems)
			{
				var invoiceOrder =
					context.CreateQuery("quote").FirstOrDefault(o => o.GetAttributeValue<Guid>("quoteid") == quoteGuid);

				var orderProduct = new Entity("quotedetail");

				var detailGuid = Guid.NewGuid();

				orderProduct.Attributes["quotedetailid"] = detailGuid;
				orderProduct.Id = detailGuid;

				var product = context.CreateQuery("product")
					.FirstOrDefault(
						p => p.GetAttributeValue<Guid>("productid") == item.GetAttributeValue<EntityReference>("adx_productid").Id);
				var unit = context.CreateQuery("uom")
					.FirstOrDefault(
						uom => uom.GetAttributeValue<Guid>("uomid") == product.GetAttributeValue<EntityReference>("defaultuomid").Id);
				/*var unit = context.CreateQuery("uom")
					.FirstOrDefault(
						uom => uom.GetAttributeValue<Guid>("uomid") == item.GetAttributeValue<EntityReference>("adx_uomid").Id);*/

				orderProduct.Attributes["productid"] = product.ToEntityReference();
				orderProduct.Attributes["uomid"] = unit.ToEntityReference();
				orderProduct.Attributes["ispriceoverridden"] = true;
				orderProduct.Attributes["priceperunit"] = item.GetAttributeValue<Money>("adx_quotedprice");
				orderProduct.Attributes["quantity"] = item.GetAttributeValue<decimal>("adx_quantity");
				orderProduct.Attributes["quoteid"] = invoiceOrder.ToEntityReference();

				context.AddObject(orderProduct);
				//context.UpdateObject(invoiceOrder);
				context.SaveChanges();

				var detail =
					context.CreateQuery("quotedetail").FirstOrDefault(sod => sod.GetAttributeValue<Guid>("quotedetailid") == detailGuid);

			}
		}

		private static void SetQuoteAddresses(Dictionary<string, string> values, Entity quote, Entity customer)
		{
			quote.Attributes["billto_line1"] = customer.GetAttributeValue<string>("address1_line1");
			quote.Attributes["billto_city"] = customer.GetAttributeValue<string>("address1_city");
			quote.Attributes["billto_country"] = customer.GetAttributeValue<string>("address1_country");
			quote.Attributes["billto_stateorprovince"] = customer.GetAttributeValue<string>("address1_stateorprovince");
			quote.Attributes["billto_postalcode"] = customer.GetAttributeValue<string>("address1_postalcode");

			quote.Attributes["shipto_line1"] = (values.ContainsKey("address_street") ? values["address_street"] : null) ??
				(values.ContainsKey("address1") ? values["address1"] : null) ?? customer.GetAttributeValue<string>("address1_line1");
			quote.Attributes["shipto_city"] = (values.ContainsKey("address_street") ? values["address_city"] : null) ??
				(values.ContainsKey("city") ? values["city"] : null) ?? customer.GetAttributeValue<string>("address1_city");
			quote.Attributes["shipto_country"] = (values.ContainsKey("address_street") ? values["address_country"] : null) ??
				(values.ContainsKey("country") ? values["country"] : null) ?? customer.GetAttributeValue<string>("address1_country");
			quote.Attributes["shipto_stateorprovince"] = (values.ContainsKey("address_street") ? values["address_state"] : null) ??
				(values.ContainsKey("state") ? values["state"] : null) ?? customer.GetAttributeValue<string>("address1_stateorprovince");
			quote.Attributes["shipto_postalcode"] = (values.ContainsKey("address_street") ? values["address_zip"] : null) ??
				(values.ContainsKey("zip") ? values["zip"] : null) ?? customer.GetAttributeValue<string>("address1_postalcode");
		}

		private static Entity GetQuoteCustomer(Dictionary<string, string> values, OrganizationServiceContext context, Entity shoppingCart)
		{
			Guid customerID;

			Entity customer;

			if (shoppingCart.GetAttributeValue<EntityReference>("adx_contactid") != null)
			{
				customerID = shoppingCart.GetAttributeValue<EntityReference>("adx_contactid").Id;
			}
			else //Probably will not be used
			{
				customer = new Entity("contact");

				customerID = Guid.NewGuid();
				customer.Attributes["contactid"] = customerID;
				customer.Id = customerID;

				var firstName = (values.ContainsKey("first_name") ? values["first_name"] : null) ?? "Tim";

				customer.Attributes["firstname"] = firstName;
				customer.Attributes["lastname"] = (values.ContainsKey("last_name") ? values["last_name"] : null) ?? "Sample";
				customer.Attributes["telephone1"] = (values.ContainsKey("contact_phone") ? values["contact_phone"] : null) ??
					((string.IsNullOrEmpty(firstName)) ? "555-9765" : null);
				customer.Attributes["address1_line1"] = (values.ContainsKey("address_street") ? values["address_street"] : null) ??
					(values.ContainsKey("address1") ? values["address1"] : null) ?? ((string.IsNullOrEmpty(firstName)) ? "123 easy street" : null);
				customer.Attributes["address1_city"] = (values.ContainsKey("address_city") ? values["address_city"] : null) ??
					(values.ContainsKey("city") ? values["city"] : null) ?? ((string.IsNullOrEmpty(firstName)) ? "Anytown" : null);
				customer.Attributes["address1_country"] = (values.ContainsKey("address_country") ? values["address_country"] : null) ??
					(values.ContainsKey("country") ? values["country"] : null) ?? ((string.IsNullOrEmpty(firstName)) ? "USA" : null);
				customer.Attributes["address1_stateorprovince"] = (values.ContainsKey("address_state") ? values["address_state"] : null) ??
					(values.ContainsKey("state") ? values["state"] : null) ?? ((string.IsNullOrEmpty(firstName)) ? "NY" : null);
				customer.Attributes["address1_postalcode"] = (values.ContainsKey("address_zip") ? values["address_zip"] : null) ??
					(values.ContainsKey("zip") ? values["zip"] : null) ?? ((string.IsNullOrEmpty(firstName)) ? "91210" : null);

				context.AddObject(customer);
				context.SaveChanges();
			}

			customer = context.CreateQuery("contact").FirstOrDefault(c => c.GetAttributeValue<Guid>("contactid") == customerID);

			return customer;
		}

		public Entity CreateOrder()
		{
			var shoppingCartReference = GetAttributeValue<EntityReference>("adx_shoppingcartid");

			ConvertQuoteToOrder(_context, Entity);

			var orderCreated = _context.CreateQuery("salesorder").FirstOrDefault(so => so.GetAttributeValue<EntityReference>("quoteid").Id
				== Entity.GetAttributeValue<Guid>("quoteid"));

			orderCreated.Attributes["adx_shoppingcartid"] = shoppingCartReference;

			_context.UpdateObject(orderCreated);

			_context.SaveChanges();

			orderCreated = _context.CreateQuery("salesorder").FirstOrDefault(so => so.GetAttributeValue<Guid>("salesorderid")
				== orderCreated.GetAttributeValue<Guid>("salesorderid"));

			return orderCreated;
		}
		

		/*****
		Currently unable to get this function working.
		 * Soemthing about the WinQuoteRequest and CloseQuoteRequest does not fuction as expected
		 * error: "cannot close the entity because it is not in the correct state" always occurs
		 * ******************/
		private static void ConvertQuoteToOrder(Microsoft.Xrm.Sdk.Client.OrganizationServiceContext context, Microsoft.Xrm.Sdk.Entity myQuote)
		{

			// Activate the quote
			SetStateRequest activateQuote = new SetStateRequest()
			{
				EntityMoniker = myQuote.ToEntityReference(),
				State = new OptionSetValue(1),
				Status = new OptionSetValue(2)
			};
			context.Execute(activateQuote);

			//Console.WriteLine("Quote activated.");

			Guid quoteId = myQuote.GetAttributeValue<Guid>("quoteid");

			var quoteClose = new Entity("quoteclose");

			quoteClose.Attributes["quoteid"] = myQuote.ToEntityReference();
			quoteClose.Attributes["subject"] = "Won The Quote";

			WinQuoteRequest winQuoteRequest = new WinQuoteRequest()
			{
				QuoteClose = quoteClose,
				Status = new OptionSetValue(-1)  //2?  -1??
			};

			var winQuoteResponse = (WinQuoteResponse)context.Execute(winQuoteRequest);

			ColumnSet salesOrderColumns = new ColumnSet("salesorderid", "totalamount");

			var convertOrderRequest = new ConvertQuoteToSalesOrderRequest()
			{
				QuoteId = quoteId,
				ColumnSet = salesOrderColumns
			};

			var convertOrderResponse = (ConvertQuoteToSalesOrderResponse)context.Execute(convertOrderRequest);
		}

	}
}

