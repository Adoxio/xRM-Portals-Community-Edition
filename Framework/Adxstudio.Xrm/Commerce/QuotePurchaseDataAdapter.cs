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
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Commerce
{
	public class QuotePurchaseDataAdapter : IPurchaseDataAdapter
	{
		public QuotePurchaseDataAdapter(EntityReference quote, IDataAdapterDependencies dependencies, bool requiresShipping = false)
		{
			if (quote == null) throw new ArgumentNullException("quote");
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Quote = quote;
			Dependencies = dependencies;
			RequiresShipping = requiresShipping;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		protected EntityReference Quote { get; private set; }

		protected bool RequiresShipping { get; private set; }

		public void CompletePurchase(bool fulfillOrder = false, bool createInvoice = false)
		{
			if (!(fulfillOrder || createInvoice))
			{
				return;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var order = serviceContext.CreateQuery("salesorder")
				.Where(e => e.GetAttributeValue<EntityReference>("quoteid") == Quote)
				.OrderByDescending(e => e.GetAttributeValue<DateTime>("createdon"))
				.FirstOrDefault();

			if (order == null)
			{
				throw new InvalidOperationException("Unable to retrieve associated order for quote {0}.".FormatWith(Quote.Id));
			}

			if (fulfillOrder)
			{
				var orderClose = new Entity("orderclose");

				orderClose["salesorderid"] = order.ToEntityReference();

				serviceContext.Execute(new FulfillSalesOrderRequest
				{
					OrderClose = orderClose,
					Status = new OptionSetValue(-1),
				});
			}
			
			if (createInvoice)
			{
                var convertOrderRequest = new ConvertSalesOrderToInvoiceRequest()
                {
                    ColumnSet = new ColumnSet("invoiceid"),
                    SalesOrderId = order.Id
                };

                var convertOrderResponse = (ConvertSalesOrderToInvoiceResponse)serviceContext.Execute(convertOrderRequest);

                var invoice = convertOrderResponse.Entity;

                var setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = invoice.ToEntityReference(),
                    State = new OptionSetValue(2),
                    Status = new OptionSetValue(100001)
                };

                var setStateResponse = (SetStateResponse)serviceContext.Execute(setStateRequest);

            }
		}

		public IPurchasable Select()
		{
			return Select(Enumerable.Empty<IPurchasableItemOptions>());
		}

		public IPurchasable Select(IEnumerable<IPurchasableItemOptions> options)
		{
			Update(options);

			var serviceContext = Dependencies.GetServiceContext();

			var quote = serviceContext.CreateQuery("quote")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("quoteid") == Quote.Id);

			if (quote == null)
			{
				return null;
			}

			var quoteProducts = serviceContext.CreateQuery("quotedetail")
				.Where(e => e.GetAttributeValue<EntityReference>("quoteid") == quote.ToEntityReference())
				.ToArray();

			var productIds = quoteProducts
				.Select(e => e.GetAttributeValue<EntityReference>("productid"))
				.Where(product => product != null)
				.Select(product => product.Id);

			var products = serviceContext.CreateQuery("product")
				.WhereIn(e => e.GetAttributeValue<Guid>("productid"), productIds)
				.ToDictionary(e => e.Id, e => e);

			var items = quoteProducts
				.Select(e => GetPurchasableItemFromQuoteProduct(serviceContext, e, products))
				.Where(e => e != null)
				.OrderBy(e => e.IsOptional)
				.ThenBy(e => e.LineItemNumber)
				.ThenBy(e => e.Name);

			serviceContext.LoadProperty(quote, new Relationship("adx_discount_quote"));

			var discounts = quote.GetRelatedEntities<Entity>(new Relationship("adx_discount_quote"))
							.Select(e => new Discount(e.Id, e.GetAttributeValue<string>("adx_code"), e.GetAttributeValue<string>("adx_name"), 
								e.GetAttributeValue<OptionSetValue>("adx_scope") == null ? 0 : e.GetAttributeValue<OptionSetValue>("adx_scope").Value,
								e.GetAttributeValue<OptionSetValue>("adx_type") == null ? 0 : e.GetAttributeValue<OptionSetValue>("adx_type").Value,
								e.GetAttributeValue<OptionSetValue>("adx_type") == null ? 0 : (e.GetAttributeValue<OptionSetValue>("adx_type").Value == (int)DiscountType.Amount) ? QuoteFunctions.GetDecimalFromMoney(e, "adx_amount") : e.GetAttributeValue<decimal?>("adx_percentage") ?? 0))
							.ToArray();

			return new Purchasable(quote, items, discounts, RequiresShipping);
		}

		public void UpdateShipToAddress(IPurchaseAddress address)
		{
			if (address == null) throw new ArgumentNullException("address");

			var update = new Entity("quote")
			{
				Id = Quote.Id
			};

			update["shipto_city"] = address.City;
			update["shipto_country"] = address.Country;
			update["shipto_line1"] = address.Line1;
			update["shipto_line2"] = address.Line2;
			update["shipto_line3"] = address.Line3;
			update["shipto_name"] = address.Name;
			update["shipto_postalcode"] = address.PostalCode;
			update["shipto_stateorprovince"] = address.StateOrProvince;

			var serviceContext = Dependencies.GetServiceContextForWrite();

			serviceContext.Attach(update);
			serviceContext.UpdateObject(update);
			serviceContext.SaveChanges();
		}

		public void Update(IEnumerable<IPurchasableItemOptions> options)
		{
			options = options.ToArray();

			if (!options.Any())
			{
				return;
			}

			var quoteProductIds = options
				.Where(o => o.QuoteProduct != null)
				.Select(o => o.QuoteProduct.Id)
				.ToArray();

			if (!quoteProductIds.Any())
			{
				return;
			}

			var serviceContext = Dependencies.GetServiceContext();

			var quoteProducts = serviceContext.CreateQuery("quotedetail")
				.Where(e => e.GetAttributeValue<EntityReference>("quoteid") == Quote)
				.WhereIn(e => e.GetAttributeValue<Guid>("quotedetailid"), quoteProductIds)
				.ToArray();

			if (!quoteProducts.Any())
			{
				return;
			}

			var serviceContextForWrite = Dependencies.GetServiceContextForWrite();

			var specialInstructions = new StringBuilder();

			foreach (var quoteProduct in quoteProducts)
			{
				var option = options.FirstOrDefault(o => o.QuoteProduct != null && o.QuoteProduct.Id == quoteProduct.Id);

				if (option == null)
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(option.Instructions))
				{
					var product = quoteProduct.GetAttributeValue<EntityReference>("productid");
					var productName = product == null ? quoteProduct.Id.ToString() : product.Name;
					specialInstructions.AppendFormat("{0}: {1}\n\n", productName, option.Instructions);
				}

				var quantity = quoteProduct.GetAttributeValue<decimal?>("quantity").GetValueOrDefault(0);

				var update = new Entity("quotedetail")
				{
					Id = quoteProduct.Id
				};

				if (option.IsSelected.HasValue)
				{
					if (option.IsSelected.Value && quantity == 0)
					{
						update["quantity"] = option.Quantity.GetValueOrDefault(1);
					}
					else if (!option.IsSelected.Value && quantity != 0)
					{
						update["quantity"] = new decimal(0);
					}
				}

				if (option.Quantity.HasValue)
				{
					update["quantity"] = option.Quantity.Value;
				}

				if (update.Attributes.Any())
				{
					serviceContextForWrite.Attach(update);
					serviceContextForWrite.UpdateObject(update);
				}
			}

			var quoteUpdate = new Entity("quote")
			{
				Id = Quote.Id
			};

			quoteUpdate["adx_specialinstructions"] = specialInstructions.ToString();

			serviceContextForWrite.Attach(quoteUpdate);
			serviceContextForWrite.UpdateObject(quoteUpdate);

			serviceContextForWrite.SaveChanges();
		}

		private static IPurchasableItem GetPurchasableItemFromQuoteProduct(OrganizationServiceContext serviceContext, Entity quoteProduct, IDictionary<Guid, Entity> products)
		{
			var productReference = quoteProduct.GetAttributeValue<EntityReference>("productid");

			if (productReference == null)
			{
				return null;
			}

			Entity product;

			if (!products.TryGetValue(productReference.Id, out product))
			{
				return null;
			}

			serviceContext.LoadProperty(quoteProduct, new Relationship("adx_discount_quotedetail"));

			var discounts = quoteProduct.GetRelatedEntities<Entity>(new Relationship("adx_discount_quotedetail"))
							.Select(e => new Discount(e.Id, e.GetAttributeValue<string>("adx_code"), e.GetAttributeValue<string>("adx_name"),
								e.GetAttributeValue<OptionSetValue>("adx_scope") == null ? 0 : e.GetAttributeValue<OptionSetValue>("adx_scope").Value,
								e.GetAttributeValue<OptionSetValue>("adx_type") == null ? 0 : e.GetAttributeValue<OptionSetValue>("adx_type").Value,
								e.GetAttributeValue<OptionSetValue>("adx_type") == null ? 0 : (e.GetAttributeValue<OptionSetValue>("adx_type").Value == (int)DiscountType.Amount) ? QuoteFunctions.GetDecimalFromMoney(e, "adx_amount") : e.GetAttributeValue<decimal?>("adx_percentage") ?? 0))
							.ToArray();

			return new PurchaseableItem(
				quoteProduct,
				product.GetAttributeValue<string>("name"),
				productReference,
				discounts,
				product.GetAttributeValue<decimal?>("stockweight").GetValueOrDefault(0) > 0);
		}

		
	}
}
