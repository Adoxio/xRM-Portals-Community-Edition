/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Adxstudio.Xrm.Resources;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Commerce
{
	public class CommerceOrder
	{
		private OrganizationServiceContext _context;

		public Entity Entity { get; private set; }

		public Entity InvoiceEntity { get; private set; }

		public Guid Id { get; private set; }

		public CommerceOrder(Entity order, OrganizationServiceContext context)
		{
			if (order == null)
			{
				throw new ArgumentNullException("order");
			}

			if (order.LogicalName != "salesorder")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", order.LogicalName), "order");
			}

			Entity = order;
			Id = order.Id;
			_context = context;
		}

		public CommerceOrder(Dictionary<string, string> values,
			IPortalContext xrm,
			string paymentProvider,
			bool getCreateInvoiceSettingValue,
			Entity account = null,
			string tombstoneEntityLogicalName = null,
			string tombstoneEntityPrimaryKeyName = null)
		{
			if (paymentProvider == "PayPal")
			{
				System.Diagnostics.Debug.Write("we are creating the order and have determined we are using paypal...");
				CreateOrderPayPal(values, xrm, getCreateInvoiceSettingValue);
			}
			else if (paymentProvider == "Authorize.Net" || paymentProvider == "Demo")
			{
				CreateOrderAuthorizeNet(values, xrm, getCreateInvoiceSettingValue, account, tombstoneEntityLogicalName, tombstoneEntityPrimaryKeyName);
			}
		}

		public T GetAttributeValue<T>(string attributeName)
		{
			return Entity.GetAttributeValue<T>(attributeName);
		}

		private void CreateOrderPayPal(Dictionary<string, string> values, IPortalContext xrm, bool getCreateInvoiceSettingValue)
		{
			System.Diagnostics.Debug.Write("A commerce order is being created...");

			_context = xrm.ServiceContext;

			if (!values.ContainsKey("invoice"))
			{
				throw new Exception("no invoice found");
			}

			var shoppingCart =
				_context.CreateQuery("adx_shoppingcart").FirstOrDefault(
					q => q.GetAttributeValue<Guid>("adx_shoppingcartid") == Guid.Parse(values["invoice"]));

			var order = new Entity("salesorder");

			var orderGuid = Guid.NewGuid();

			order.Attributes["salesorderid"] = orderGuid;
			order.Id = orderGuid;

			order.Attributes["name"] = "order created by: " + shoppingCart.GetAttributeValue<string>("adx_name");

			order.Attributes["adx_shoppingcartid"] = shoppingCart.ToEntityReference();

			System.Diagnostics.Debug.Write(string.Format("shopping cart ID:{0}", shoppingCart.Id.ToString()));

			var supportRequest = _context.CreateQuery("adx_supportrequest")
					.FirstOrDefault(sr => sr.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id == shoppingCart.Id);
			if (supportRequest != null)
			{
				System.Diagnostics.Debug.Write(string.Format("Support Request ID:{0}", supportRequest.Id.ToString()));

				var supportPlanReference = supportRequest.GetAttributeValue<EntityReference>("adx_supportplan");

				System.Diagnostics.Debug.Write(string.Format("Support Reference:{0}", supportPlanReference));

				var supportPlan = _context.CreateQuery("adx_supportplan").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_supportplanid")
							== supportPlanReference.Id);

				order.Attributes["adx_supportplanid"] = supportPlan.ToEntityReference();
			}
			else
			{
				System.Diagnostics.Debug.Write("support request is null");
			}

			//Ensure that there is a customer
			var customer = GetOrderCustomer(values, _context, shoppingCart);

			if (!_context.IsAttached(shoppingCart))
			{
				_context.Attach(shoppingCart);
			}

			shoppingCart.Attributes["adx_contactid"] = customer.ToEntityReference();

			var parentCustomer = customer.GetAttributeValue<EntityReference>("parentcustomerid");

			var parentCustomerEntity =
				_context.CreateQuery("account").FirstOrDefault(pce => pce.GetAttributeValue<Guid>("accountid") == parentCustomer.Id);

			order.Attributes["customerid"] = (parentCustomerEntity != null) ? parentCustomerEntity.ToEntityReference() : customer.ToEntityReference();

			var priceLevel =
				_context.CreateQuery("pricelevel").FirstOrDefault(pl => pl.GetAttributeValue<string>("name") == "Web");

			if (priceLevel == null)
			{
				throw new Exception("price level null");
			}

			//Set the price level
			var priceLevelReference = priceLevel.ToEntityReference();

			order.Attributes["pricelevelid"] = priceLevelReference;

			//Set the address for the order
			SetOrderAddresses(values, order, customer);

			//order.Attributes["adx_confirmationnumber"] = shoppingCart.GetAttributeValue<string>("adx_confirmationnumber");
			order.Attributes["adx_receiptnumber"] = values.ContainsKey("ipn_trac_id") ? values["ipn_track_id"] : null;

			_context.AddObject(order);

			_context.UpdateObject(shoppingCart);

			_context.SaveChanges();

			//Set the products of the order
			SetOrderProducts(shoppingCart, _context, orderGuid, null);


			//Time to associate order with support plan

			//sw.WriteLine("ok, we are at the weird part!");


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

			//At this point we want to Create an Invoice, if that is indeed what we are doing.

			if (getCreateInvoiceSettingValue)
			{
				InvoiceEntity = CreateInvoiceFromOrder(_context, orderGuid);
			}

			Entity = _context.CreateQuery("salesorder").FirstOrDefault(o => o.GetAttributeValue<Guid>("salesorderid") == orderGuid);
			Id = Entity.Id;
		}

		private void CreateOrderAuthorizeNet(Dictionary<string, string> values,
			IPortalContext xrm,
			bool getCreateInvoiceSettingValue,
			Entity account = null,
			string tombstoneEntityLogicalName = null,
			string tombstoneEntityPrimaryKeyName = null)
		{
			_context = xrm.ServiceContext;

			if (!values.ContainsKey("order_id"))
			{
				throw new Exception("no order_id found");
			}

			var orderId = values["order_id"];

			string[] guids = orderId.Split('&');

			var tombstoneEntityId = new Guid(guids[0]);

			Entity tombstoneEntity = null; Entity shoppingCart = null; Entity supportPlan = null;



			if (tombstoneEntityLogicalName != null)
			{
				tombstoneEntity = _context.CreateQuery(tombstoneEntityLogicalName)
					.FirstOrDefault(sr => sr.GetAttributeValue<Guid>(tombstoneEntityPrimaryKeyName) == tombstoneEntityId);

				shoppingCart = _context.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid") ==
							tombstoneEntity.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id);

			}
			else
			{
				shoppingCart = _context.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid") ==
							tombstoneEntityId);
			}

			if (tombstoneEntityLogicalName == "adx_supportrequest")
			{
				supportPlan = _context.CreateQuery("adx_supportplan").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_supportplanid")
								== tombstoneEntity.GetAttributeValue<EntityReference>("adx_supportplan").Id);
			}


			var orderGuid = Guid.NewGuid();

			var order = new Entity("salesorder") { Id = orderGuid };

			order.Attributes["salesorderid"] = orderGuid;
			order.Id = orderGuid;

			order.Attributes["name"] = "order created by: " + shoppingCart.GetAttributeValue<string>("adx_name");

			order.Attributes["adx_shoppingcartid"] = shoppingCart.ToEntityReference();

			if (tombstoneEntityLogicalName == "adx_supportrequest")
			{
				order.Attributes["adx_supportplanid"] = supportPlan.ToEntityReference();
			}

			//Ensure that there is a customer
			var customer = GetOrderCustomer(values, _context, shoppingCart);

			if (!_context.IsAttached(shoppingCart))
			{
				_context.Attach(shoppingCart);
			}

			shoppingCart.Attributes["adx_contactid"] = customer.ToEntityReference();

			if (account == null)
			{
				var parentCustomer = customer.GetAttributeValue<EntityReference>("parentcustomerid");

				Entity parentCustomerEntity = null;

				if (parentCustomer != null)
				{
					parentCustomerEntity =
					_context.CreateQuery("account").FirstOrDefault(pce => pce.GetAttributeValue<Guid>("accountid") == parentCustomer.Id);
				}

				order.Attributes["customerid"] = (parentCustomerEntity != null) ? parentCustomerEntity.ToEntityReference() : customer.ToEntityReference();
			}
			else
			{
				order.Attributes["customerid"] = account.ToEntityReference();
			}

			var priceLevel =
				_context.CreateQuery("pricelevel").FirstOrDefault(pl => pl.GetAttributeValue<string>("name") == "Web");

			if (priceLevel == null)
			{
				throw new Exception("price level null");
			}

			//Set the price level
			var priceLevelReference = priceLevel.ToEntityReference();

			order.Attributes["pricelevelid"] = priceLevelReference;

			//Set the address for the order
			SetOrderAddresses(values, order, customer);

			//order.Attributes["adx_confirmationnumber"] = shoppingCart.GetAttributeValue<string>("adx_confirmationnumber");
			order.Attributes["adx_receiptnumber"] = values.ContainsKey("x_trans_id") ? values["x_trans_id"] : null;

			//Set the tax 
			//order.Attributes["totaltax"] = values.ContainsKey("x_tax")	? new Money(decimal.Parse(values["x_tax"]))	: null;
			var tax = values.ContainsKey("x_tax") ? new Money(decimal.Parse(values["x_tax"])) : null;

			_context.AddObject(order);

			_context.UpdateObject(shoppingCart);

			_context.SaveChanges();

			//Set the products of the order
			SetOrderProducts(shoppingCart, _context, orderGuid, tax);

			tombstoneEntity = _context.CreateQuery(tombstoneEntityLogicalName)
					.FirstOrDefault(sr => sr.GetAttributeValue<Guid>(tombstoneEntityPrimaryKeyName) == tombstoneEntityId);

			shoppingCart = _context.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid")
							== tombstoneEntity.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id);

			//Deactivate the shopping Cart

			try
			{
				_context.SetState(1, 2, shoppingCart);
				_context.SaveChanges();
			}
			catch
			{
				//Unlikely that there is an issue, most likely it has already been deactiveated.
			}

			//At this point we want to Create an Invoice, if that is indeed what we are doing.

			if (getCreateInvoiceSettingValue)
			{
				InvoiceEntity = CreateInvoiceFromOrder(_context, orderGuid);
			}

			Entity = _context.CreateQuery("salesorder").FirstOrDefault(o => o.GetAttributeValue<Guid>("salesorderid") == orderGuid);
			Id = Entity.Id;

			//writer.Close();
		}

		private static Entity CreateInvoiceFromOrder(OrganizationServiceContext context, Guid salesOrderId)
		{
			ColumnSet invoiceColumns = new ColumnSet("invoiceid", "totalamount");

			var convertOrderRequest = new ConvertSalesOrderToInvoiceRequest()
			{
				SalesOrderId = salesOrderId,
				ColumnSet = invoiceColumns
			};

			var convertOrderResponse = (ConvertSalesOrderToInvoiceResponse)context.Execute(convertOrderRequest);

			var invoice = convertOrderResponse.Entity;

            var setStateRequest = new SetStateRequest()
            {
                EntityMoniker = invoice.ToEntityReference(),
                State = new OptionSetValue(2),
                Status = new OptionSetValue(100001)
            };

            var setStateResponse = (SetStateResponse)context.Execute(setStateRequest);

            invoice = context.CreateQuery("invoice").Where(i => i.GetAttributeValue<Guid>("invoiceid") == convertOrderResponse.Entity.Id).FirstOrDefault();

            return invoice;
		}

		private static void SetOrderProducts(Entity shoppingCart, OrganizationServiceContext context, Guid salesOrderGuid, Money tax)
		{
			bool first = true;

			var cartItems = context.CreateQuery("adx_shoppingcartitem")
				.Where(
					qp =>
					qp.GetAttributeValue<EntityReference>("adx_shoppingcartid").Id ==
					shoppingCart.GetAttributeValue<Guid>("adx_shoppingcartid")).ToList();

			foreach (var item in cartItems)
			{
				var invoiceOrder =
					context.CreateQuery("salesorder").FirstOrDefault(o => o.GetAttributeValue<Guid>("salesorderid") == salesOrderGuid);

				var orderProduct = new Entity("salesorderdetail");

				var detailGuid = Guid.NewGuid();

				orderProduct.Attributes["salesorderdetailid"] = detailGuid;
				orderProduct.Id = detailGuid;

				var product = context.CreateQuery("product")
					.FirstOrDefault(
						p => p.GetAttributeValue<Guid>("productid") == item.GetAttributeValue<EntityReference>("adx_productid").Id);
				var unit = context.CreateQuery("uom")
					.FirstOrDefault(
						uom => uom.GetAttributeValue<Guid>("uomid") == item.GetAttributeValue<EntityReference>("adx_uomid").Id) ??
						context.CreateQuery("uom").FirstOrDefault(uom => uom.GetAttributeValue<Guid>("uomid")
							== product.GetAttributeValue<EntityReference>("defaultuomid").Id);
				/*var unit = context.CreateQuery("uom")
					.FirstOrDefault(
						uom => uom.GetAttributeValue<Guid>("uomid") == item.GetAttributeValue<EntityReference>("adx_uomid").Id);*/

				orderProduct.Attributes["productid"] = product.ToEntityReference();
				orderProduct.Attributes["uomid"] = unit.ToEntityReference();
				orderProduct.Attributes["ispriceoverridden"] = true;
				orderProduct.Attributes["priceperunit"] = item.GetAttributeValue<Money>("adx_quotedprice");
				orderProduct.Attributes["quantity"] = item.GetAttributeValue<decimal>("adx_quantity");
				orderProduct.Attributes["salesorderid"] = invoiceOrder.ToEntityReference();

				//We only place our tax on the first item
				if (first)
				{
					first = false;
					orderProduct.Attributes["tax"] = tax;
				}

				context.AddObject(orderProduct);
				//context.UpdateObject(invoiceOrder);
				context.SaveChanges();

				var detail =
					context.CreateQuery("salesorderdetail").FirstOrDefault(sod => sod.GetAttributeValue<Guid>("salesorderdetailid") == detailGuid);

			}
		}

		private static void SetOrderAddresses(Dictionary<string, string> values, Entity order, Entity customer)
		{
			order.Attributes["billto_line1"] = customer.GetAttributeValue<string>("address1_line1");
			order.Attributes["billto_city"] = customer.GetAttributeValue<string>("address1_city");
			order.Attributes["billto_country"] = customer.GetAttributeValue<string>("address1_country");
			order.Attributes["billto_stateorprovince"] = customer.GetAttributeValue<string>("address1_stateorprovince");
			order.Attributes["billto_postalcode"] = customer.GetAttributeValue<string>("address1_postalcode");

			order.Attributes["shipto_line1"] = (values.ContainsKey("address_street") ? values["address_street"] : null) ??
				(values.ContainsKey("x_address") ? values["x_address"] : null) ??
				(values.ContainsKey("address1") ? values["address1"] : null) ?? customer.GetAttributeValue<string>("address1_line1");
			order.Attributes["shipto_city"] = (values.ContainsKey("address_city") ? values["address_city"] : null) ??
				(values.ContainsKey("x_city") ? values["x_city"] : null) ??
				(values.ContainsKey("city") ? values["city"] : null) ?? customer.GetAttributeValue<string>("address1_city");
			order.Attributes["shipto_country"] = (values.ContainsKey("address_country") ? values["address_country"] : null) ??
				(values.ContainsKey("x_country") ? values["x_country"] : null) ??
				(values.ContainsKey("country") ? values["country"] : null) ?? customer.GetAttributeValue<string>("address1_country");
			order.Attributes["shipto_stateorprovince"] = (values.ContainsKey("address_state") ? values["address_state"] : null) ??
				(values.ContainsKey("x_state") ? values["x_state"] : null) ??
				(values.ContainsKey("state") ? values["state"] : null) ?? customer.GetAttributeValue<string>("address1_stateorprovince");
			order.Attributes["shipto_postalcode"] = (values.ContainsKey("address_zip") ? values["address_zip"] : null) ??
				(values.ContainsKey("x_zip") ? values["x_zip"] : null) ??
				(values.ContainsKey("zip") ? values["zip"] : null) ?? customer.GetAttributeValue<string>("address1_postalcode");
		}

		private static Entity GetOrderCustomer(Dictionary<string, string> values, OrganizationServiceContext context, Entity shoppingCart)
		{
			Guid customerID;

			Entity customer;

			if (shoppingCart.GetAttributeValue<EntityReference>("adx_contactid") != null)
			{
				customerID = shoppingCart.GetAttributeValue<EntityReference>("adx_contactid").Id;
			}
			else
			{
				customerID = Guid.NewGuid();
				customer = new Entity("contact") { Id = customerID };

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

	}
}
