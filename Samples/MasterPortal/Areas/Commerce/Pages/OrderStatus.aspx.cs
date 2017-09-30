/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Text;
using Adxstudio.Xrm.Web.UI.WebControls;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Pages;
using ShoppingCart = Adxstudio.Xrm.Commerce.ShoppingCart;

namespace Site.Areas.Commerce.Pages
{
	public partial class OrderStatus : PortalPage
	{
		private Entity _order;

		public Entity OrderToEdit
		{
			get
			{
				if (_order != null)
				{
					return _order;
				}

				Guid orderId;

				if (!Guid.TryParse(Request["OrderID"], out orderId))
				{
					return null;
				}

				_order = XrmContext.CreateQuery("salesorder").FirstOrDefault(c => c.GetAttributeValue<Guid>("salesorderid") == orderId);

				return _order;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			RedirectToLoginIfAnonymous();

			if (OrderToEdit == null || (OrderToEdit.GetAttributeValue<EntityReference>("customerid") != null && !OrderToEdit.GetAttributeValue<EntityReference>("customerid").Equals(Contact.ToEntityReference())))
			{
				PageBreadcrumbs.Visible = true;
				GenericError.Visible = true;
				OrderHeader.Visible = false;
				OrderDetails.Visible = false;
				OrderInfo.Visible = false;
				OrderBreadcrumbs.Visible = false;
				OrderHeader.Visible = false;

				return;
			}
			
			var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext, portalName: PortalName);
			var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
			var annotations = dataAdapter.GetAnnotations(OrderToEdit.ToEntityReference(),
				new List<Order> { new Order("createdon") }, respectPermissions: false);

			NotesList.DataSource = annotations;
			NotesList.DataBind();

			var formViewDataSource = new CrmDataSource { ID = "WebFormDataSource", CrmDataContextName = FormView.ContextName };

			var fetchXml = string.Format("<fetch mapping='logical'><entity name='{0}'><all-attributes /><filter type='and'><condition attribute = '{1}' operator='eq' value='{{{2}}}'/></filter></entity></fetch>", "salesorder", "salesorderid", OrderToEdit.GetAttributeValue<Guid>("salesorderid"));

			formViewDataSource.FetchXml = fetchXml;

			OrderForm.Controls.Add(formViewDataSource);

			FormView.DataSourceID = "WebFormDataSource";

			var baseCartReference = OrderToEdit.GetAttributeValue<EntityReference>("adx_shoppingcartid");

			if (baseCartReference == null)
			{
				ShoppingCartSummary.Visible = false;

				Entity invoice;

				if (TryGetInvoice(XrmContext, OrderToEdit, out invoice))
				{
					ShowInvoice(XrmContext, invoice);

					return;
				}

				ShowOrder(XrmContext, OrderToEdit);

				Order.Visible = true;
				Invoice.Visible = false;

				return;
			}

			// legacy code for displaying summary of ordered items.

			var baseCart = XrmContext.CreateQuery("adx_shoppingcart").FirstOrDefault(sc => sc.GetAttributeValue<Guid>("adx_shoppingcartid") == baseCartReference.Id);

			var cartRecord = baseCart == null ? null : new ShoppingCart(baseCart, XrmContext);

			if (cartRecord == null)
			{
				ShoppingCartSummary.Visible = false;

				return;
			}

			var cartItems = cartRecord.GetCartItems().Select(sci => sci.Entity);

			if (!cartItems.Any())
			{
				ShoppingCartSummary.Visible = false;

				return;
			}

			CartRepeater.DataSource = cartItems;
			CartRepeater.DataBind();

			Total.Text = cartRecord.GetCartTotal().ToString("C2");
		}

		protected string GetCartItemTitle(OrganizationServiceContext context, Entity item)
		{
			var product = item.GetRelatedEntity(context, new Relationship("adx_product_shoppingcartitem"));

			return product.GetAttributeValue<string>("name");
		}

		protected void OnItemUpdating(object sender, CrmEntityFormViewUpdatingEventArgs e)
		{
		}

		protected void OnItemUpdated(object sender, CrmEntityFormViewUpdatedEventArgs e)
		{
			UpdateSuccessMessage.Visible = true;
		}

		protected void AddNote_Click(object sender, EventArgs e)
		{
			if (OrderToEdit == null || (OrderToEdit.GetAttributeValue<EntityReference>("customerid") != null && !OrderToEdit.GetAttributeValue<EntityReference>("customerid").Equals(Contact.ToEntityReference())))
			{
				throw new InvalidOperationException("Unable to retrieve the order.");
			}

			if (!string.IsNullOrEmpty(NewNoteText.Text) ||
				(NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0))
			{
				var dataAdapterDependencies = new PortalConfigurationDataAdapterDependencies(
					requestContext: Request.RequestContext, portalName: PortalName);

				var dataAdapter = new AnnotationDataAdapter(dataAdapterDependencies);
				
				var annotation = new Annotation
				{
					NoteText = string.Format("{0}{1}", AnnotationHelper.WebAnnotationPrefix, NewNoteText.Text),
					Subject = AnnotationHelper.BuildNoteSubject(dataAdapterDependencies),
					Regarding = OrderToEdit.ToEntityReference()
				};
				if (NewNoteAttachment.PostedFile != null && NewNoteAttachment.PostedFile.ContentLength > 0)
				{
					annotation.FileAttachment = AnnotationDataAdapter.CreateFileAttachment(new HttpPostedFileWrapper(NewNoteAttachment.PostedFile));
				}
				dataAdapter.CreateAnnotation(annotation);
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected string GetLabelClassForOrder(Entity order)
		{
			if (order.GetAttributeValue<OptionSetValue>("statuscode") == null)
			{
				return null;
			}

			switch (order.GetAttributeValue<OptionSetValue>("statuscode").Value)
			{
				case (int)Enums.SalesOrderStatusCode.Canceled:
					return "label-danger";
				case (int)Enums.SalesOrderStatusCode.New:
				case (int)Enums.SalesOrderStatusCode.Partial:
				case (int)Enums.SalesOrderStatusCode.Processesing:
					return "label-info";
				case (int)Enums.SalesOrderStatusCode.Shipped:
				case (int)Enums.SalesOrderStatusCode.Invoiced:
					return "label-success";
				case (int)Enums.SalesOrderStatusCode.Pending:
					return "label-warning";
				default:
					return "label-default";
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
