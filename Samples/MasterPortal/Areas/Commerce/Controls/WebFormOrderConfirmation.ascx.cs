/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Site.Areas.Commerce.Controls
{
	public partial class WebFormOrderConfirmation : WebFormCommercePortalUserControl
	{
		protected void Page_Load(object sender, EventArgs args)
		{
			var quoteId = WebForm.CurrentSessionHistory.QuoteId;

			if (quoteId == Guid.Empty)
			{
				GeneralErrorMessage.Visible = true;
				Order.Visible = false;
				Invoice.Visible = false;

				return;
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			Entity order;

			if (!TryGetOrder(serviceContext, quoteId, out order))
			{
				GeneralErrorMessage.Visible = true;
				Order.Visible = false;
				Invoice.Visible = false;

				return;
			}

			Entity invoice;

			if (TryGetInvoice(serviceContext, order, out invoice))
			{
				ShowInvoice(serviceContext, invoice);

				return;
			}

			ShowOrder(serviceContext, order);

			GeneralErrorMessage.Visible = false;
			Order.Visible = true;
			Invoice.Visible = false;
		}

		protected void Page_PreRender(object sender, EventArgs args)
		{
			if (WebForm.CurrentSessionHistory == null)
			{
				return;
			}

			var currentStepId = WebForm.CurrentSessionHistory.CurrentStepId;

			if (currentStepId == Guid.Empty)
			{
				return;
			}

			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(PortalName, Request.RequestContext);
			var serviceContext = dataAdapterDependencies.GetServiceContext();

			var step = serviceContext.CreateQuery("adx_webformstep")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webformstepid") == currentStepId);

			if (step == null)
			{
				return;
			}

			if (step.GetAttributeValue<EntityReference>("adx_nextstep") == null)
			{
				WebForm.ShowHideNextButton(false);
			}
		}

		private void ShowInvoice(OrganizationServiceContext serviceContext, Entity invoice)
		{
			var invoiceProducts = serviceContext.CreateQuery("invoicedetail")
				.Where(e => e.GetAttributeValue<EntityReference>("invoiceid") == invoice.ToEntityReference())
				.Where(e => e.GetAttributeValue<decimal>("quantity") > 0)
				.ToArray();

			var productIds = invoiceProducts
				.Select(e => e.GetAttributeValue<EntityReference>("productid"))
				.Where(product => product != null)
				.Select(product => product.Id);

			var products = serviceContext.CreateQuery("product")
				.WhereIn(e => e.GetAttributeValue<Guid>("productid"), productIds)
				.ToDictionary(e => e.Id, e => e);

			var items = invoiceProducts
				.Select(e => GetLineItemFromInvoiceProduct(e, products))
				.Where(e => e != null)
				.OrderBy(e => e.Number)
				.ThenBy(e => e.Name);

			InvoiceItems.DataSource = items;
			InvoiceItems.DataBind();

			InvoiceNumber.Text = invoice.GetAttributeValue<string>("invoicenumber");

			var tax = invoice.GetAttributeValue<Money>("totaltax") ?? new Money(0);

			InvoiceTotalTax.Visible = tax.Value > 0;
			InvoiceTotalTaxAmount.Text = tax.Value.ToString("C2");

			var shipping = invoice.GetAttributeValue<Money>("freightamount") ?? new Money(0);

			InvoiceTotalShipping.Visible = shipping.Value > 0;
			InvoiceTotalShippingAmount.Text = shipping.Value.ToString("C2");

			var discount = invoice.GetAttributeValue<Money>("totaldiscountamount") ?? new Money(0);

			InvoiceTotalDiscount.Visible = discount.Value > 0;
			InvoiceTotalDiscountAmount.Text = discount.Value.ToString("C2");

			var total = invoice.GetAttributeValue<Money>("totalamount") ?? new Money(0);

			InvoiceTotal.Visible = total.Value > 0;
			InvoiceTotalAmount.Text = total.Value.ToString("C2");

			GeneralErrorMessage.Visible = false;
			Order.Visible = false;
			Invoice.Visible = true;
		}

		private void ShowOrder(OrganizationServiceContext serviceContext, Entity order)
		{
			var orderProducts = serviceContext.CreateQuery("salesorderdetail")
				.Where(e => e.GetAttributeValue<EntityReference>("salesorderid") == order.ToEntityReference())
				.Where(e => e.GetAttributeValue<decimal>("quantity") > 0)
				.ToArray();

			var productIds = orderProducts
				.Select(e => e.GetAttributeValue<EntityReference>("productid"))
				.Where(product => product != null)
				.Select(product => product.Id);

			var products = serviceContext.CreateQuery("product")
				.WhereIn(e => e.GetAttributeValue<Guid>("productid"), productIds)
				.ToDictionary(e => e.Id, e => e);

			var items = orderProducts
				.Select(e => GetLineItemFromOrderProduct(e, products))
				.Where(e => e != null)
				.OrderBy(e => e.Number)
				.ThenBy(e => e.Name);

			OrderItems.DataSource = items;
			OrderItems.DataBind();

			OrderNumber.Text = order.GetAttributeValue<string>("ordernumber");

			var tax = order.GetAttributeValue<Money>("totaltax") ?? new Money(0);

			OrderTotalTax.Visible = tax.Value > 0;
			OrderTotalTaxAmount.Text = tax.Value.ToString("C2");

			var shipping = order.GetAttributeValue<Money>("freightamount") ?? new Money(0);

			OrderTotalShipping.Visible = shipping.Value > 0;
			OrderTotalShippingAmount.Text = shipping.Value.ToString("C2");

			var discount = order.GetAttributeValue<Money>("totaldiscountamount") ?? new Money(0);

			OrderTotalDiscount.Visible = discount.Value > 0;
			OrderTotalDiscountAmount.Text = discount.Value.ToString("C2");

			var total = order.GetAttributeValue<Money>("totalamount") ?? new Money(0);

			OrderTotal.Visible = total.Value > 0;
			OrderTotalAmount.Text = total.Value.ToString("C2");

			GeneralErrorMessage.Visible = false;
			Order.Visible = true;
			Invoice.Visible = false;
		}

		private static LineItem GetLineItemFromInvoiceProduct(Entity invoiceProduct, IDictionary<Guid, Entity> products)
		{
			var productReference = invoiceProduct.GetAttributeValue<EntityReference>("productid");

			if (productReference == null)
			{
				return null;
			}

			Entity product;

			if (!products.TryGetValue(productReference.Id, out product))
			{
				return null;
			}

			var quantity = invoiceProduct.GetAttributeValue<decimal?>("quantity").GetValueOrDefault();

			if (quantity <= 0)
			{
				return null;
			}

			var pricePerUnit = invoiceProduct.GetAttributeValue<Money>("priceperunit");

			return new LineItem(
				product.GetAttributeValue<string>("name"),
				invoiceProduct.GetAttributeValue<int?>("lineitemnumber").GetValueOrDefault(0),
				quantity,
				pricePerUnit == null ? new decimal(0) : pricePerUnit.Value);
		}

		private static LineItem GetLineItemFromOrderProduct(Entity orderProduct, IDictionary<Guid, Entity> products)
		{
			var productReference = orderProduct.GetAttributeValue<EntityReference>("productid");

			if (productReference == null)
			{
				return null;
			}

			Entity product;

			if (!products.TryGetValue(productReference.Id, out product))
			{
				return null;
			}

			var quantity = orderProduct.GetAttributeValue<decimal?>("quantity").GetValueOrDefault();

			if (quantity <= 0)
			{
				return null;
			}

			var pricePerUnit = orderProduct.GetAttributeValue<Money>("priceperunit");

			return new LineItem(
				product.GetAttributeValue<string>("name"),
				orderProduct.GetAttributeValue<int?>("lineitemnumber").GetValueOrDefault(0),
				quantity,
				pricePerUnit == null ? new decimal(0) : pricePerUnit.Value);
		}

		private static bool TryGetInvoice(OrganizationServiceContext serviceContext, Entity order, out Entity invoice)
		{
			invoice = null;

			if (order == null)
			{
				return false;
			}

			var orderState = order.GetAttributeValue<OptionSetValue>("statecode");

			if (orderState == null || orderState.Value == 0)
			{
				return false;
			}

			invoice = serviceContext.CreateQuery("invoice")
				.Where(e => e.GetAttributeValue<EntityReference>("salesorderid") == order.ToEntityReference())
				.Where(e => e.GetAttributeValue<OptionSetValue>("statecode") != null && (e.GetAttributeValue<OptionSetValue>("statecode").Value == 0 || e.GetAttributeValue<OptionSetValue>("statecode").Value == 2))
				.OrderByDescending(e => e.GetAttributeValue<DateTime>("createdon"))
				.FirstOrDefault();

			return invoice != null;
		}

		private static bool TryGetOrder(OrganizationServiceContext serviceContext, Guid quoteId, out Entity order)
		{
			var quote = new EntityReference("quote", quoteId);

			order = serviceContext.CreateQuery("salesorder")
				.Where(e => e.GetAttributeValue<EntityReference>("quoteid") == quote)
				.Where(e => e.GetAttributeValue<OptionSetValue>("statecode") != null && e.GetAttributeValue<OptionSetValue>("statecode").Value != 2)
				.OrderByDescending(e => e.GetAttributeValue<DateTime>("createdon"))
				.FirstOrDefault();

			return order != null;
		}

		protected class LineItem : Tuple<string, int, decimal, decimal>
		{
			public LineItem(string name, int lineItemNumber, decimal quantity, decimal pricePerUnit)
				: base(name, lineItemNumber, quantity, pricePerUnit) { }

			public string Name
			{
				get { return Item1; }
			}

			public int Number
			{
				get { return Item2; }
			}

			public decimal Quantity
			{
				get { return Item3; }
			}

			public decimal PricePerUnit
			{
				get { return Item4; }
			}
		}
	}
}
