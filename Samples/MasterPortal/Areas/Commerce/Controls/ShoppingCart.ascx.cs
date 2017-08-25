/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Controls;
using IDataAdapterDependencies = Adxstudio.Xrm.Commerce.IDataAdapterDependencies;
using Adxstudio.Xrm.Data;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Commerce.PortalConfigurationDataAdapterDependencies;

namespace Site.Areas.Commerce.Controls
{
	public partial class ShoppingCart : PortalUserControl
	{
		protected const string ShoppingCartIdQueryStringParameterName = "cartid";

		public IShoppingCart Cart { get; set; }

		public string PriceListName
		{
			get
			{
				var priceListName = ServiceContext.GetDefaultPriceListName(Website.Id);

				return string.IsNullOrWhiteSpace(priceListName) ? "Web" : priceListName;
			}
		}

		public bool SaveToQuoteEnabled
		{
			get { return Html.BooleanSetting("Ecommerce/SaveToQuoteEnabled").GetValueOrDefault(false); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}
			
			if (Cart == null)
			{
				ShoppingCartPanel.Visible = false;
				ShoppingCartEmptyPanel.Visible = true;

				return;
			}

			var cartItems = Cart.GetCartItems().Select(sci => sci.Entity).ToArray();

			if (!cartItems.Any())
			{
				ShoppingCartPanel.Visible = false;
				ShoppingCartEmptyPanel.Visible = true;

				return;
			}

			ShoppingCartEmptyPanel.Visible = false;

			CartItems.DataSource = cartItems.Select(item =>
			{
				var product = ServiceContext.CreateQuery("product").First(p => p.GetAttributeValue<Guid>("productid") == (item.GetAttributeValue<EntityReference>("adx_productid") == null ? Guid.Empty : item.GetAttributeValue<EntityReference>("adx_productid").Id));

				return new
				{
					Id = item.GetAttributeValue<Guid>("adx_shoppingcartitemid"),
					Description = product == null ? item.GetAttributeValue<string>("adx_name") : product.GetAttributeValue<string>("name"),
					Price = item.GetAttributeValue<Money>("adx_quotedprice") != null ? item.GetAttributeValue<Money>("adx_quotedprice").Value : 0,
					Quantity = item.GetAttributeValue<decimal?>("adx_quantity").GetValueOrDefault(1),
					Total = item.GetAttributeValue<decimal?>("adx_quantity").GetValueOrDefault(1) * (item.GetAttributeValue<Money>("adx_quotedprice") != null ? item.GetAttributeValue<Money>("adx_quotedprice").Value : 0),
					Url = GetProductUrl(product)
				};
			});

			CartItems.DataBind();

			Total.Text = Cart.GetCartTotal().ToString("C2");

			SaveToQuote.Visible = SaveToQuoteEnabled;
		}

		protected void OnUpdateCart(object sender, EventArgs e)
		{
			UpdateCartItems();

			Response.Redirect(Request.Url.PathAndQuery);
		}

		protected void OnCheckOut(object sender, EventArgs e)
		{
			UpdateCartItems();

			var checkoutUrl = GetCheckoutUrl(Cart.Id);

			Response.Redirect(checkoutUrl);
		}

		protected void OnSaveToQuote(object sender, EventArgs e)
		{
			UpdateCartItems();

			var dependencies = new PortalConfigurationDataAdapterDependencies(requestContext: Request.RequestContext);

			var purchasable = GetPurchaseableQuote(Cart, dependencies, Request.RequestContext.HttpContext.Profile.UserName);

			if (purchasable == null)
			{
				throw new ApplicationException("Purchase could not be determined.");
			}

			var quote = purchasable.Quote;

			if (quote == null)
			{
				throw new ApplicationException("Quote could not be determined.");
			}

			var quoteId = quote.Id;

			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Quote History");

			if (page == null)
			{
				throw new ApplicationException(string.Format("A page couldn't be found for the site marker named {0}.", "Quote History"));
			}

			var url = ServiceContext.GetUrl(page);

			if (string.IsNullOrWhiteSpace(url))
			{
				throw new ApplicationException(string.Format("A URL couldn't be determined for the site marker named {0}.", "Quote History"));
			}

			var urlBuilder = new UrlBuilder(url);
			
			urlBuilder.QueryString.Add("QuoteID", quoteId.ToString());

			Cart.DeactivateCart();

			Response.Redirect(urlBuilder.PathWithQueryString);
		}
		
		protected void UpdateCartItems()
		{
			foreach (var item in CartItems.Items)
			{
				var cartItemIdTextBox = item.FindControl("CartItemID") as TextBox;

				if (cartItemIdTextBox == null)
				{
					continue;
				}

				var quantityTextBox = item.FindControl("Quantity") as TextBox;

				if (quantityTextBox == null)
				{
					continue;
				}

				int quantity;

				if (!int.TryParse(quantityTextBox.Text, out quantity))
				{
					throw new InvalidOperationException("Couldn't parse quantity.");
				}

				try
				{
					var cartItemId = new Guid(cartItemIdTextBox.Text);
					var cartItem = Cart.GetCartItemByID(cartItemId) as ShoppingCartItem;

					if (cartItem == null)
					{
						throw new InvalidOperationException("Unable to find cart item corresponding to CartItemID.");
					}

					if (cartItem.Quantity != quantity)
					{
						cartItem.Quantity = quantity;
					}

					cartItem.UpdateItemPrice(PriceListName);

				}
				catch (FormatException)
				{
					throw new InvalidOperationException("Unable to parse Guid from CartItemID value.");
				}
			}

			XrmContext.SaveChanges();
		}

		protected void CartItems_ItemCommand(object source, CommandEventArgs e)
		{
			if (e.CommandName == "Remove" && e.CommandArgument != null)
			{
				Cart.RemoveItemFromCart(new Guid(e.CommandArgument.ToString()));
			}

			Response.Redirect(Request.Url.PathAndQuery);
		}

		private string GetProductUrl(Entity product)
		{
			if (product == null || product.GetAttributeValue<EntityReference>("subjectid") == null)
			{
				return null;
			}

			var productPage = XrmContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<EntityReference>("adx_subjectid") == product.GetAttributeValue<EntityReference>("subjectid")
					&& e.GetAttributeValue<EntityReference>("adx_websiteid") == Website.ToEntityReference());

			return productPage == null ? null : XrmContext.GetUrl(productPage);
		}

		protected string GetCheckoutUrl(Guid shoppingCartId)
		{
			var page = ServiceContext.GetPageBySiteMarkerName(Website, "Checkout");

			if (page == null)
			{
				throw new ApplicationException(string.Format("A page couldn't be found for the site marker named {0}.", "Checkout"));
			}

			var url = ServiceContext.GetUrl(page);

			if (string.IsNullOrWhiteSpace(url))
			{
				throw new ApplicationException(string.Format("A URL couldn't be determined for the site marker named {0}.", "Checkout"));
			}

			var urlBuilder = new UrlBuilder(url);

			urlBuilder.QueryString.Add(ShoppingCartIdQueryStringParameterName, shoppingCartId.ToString());

			return urlBuilder.PathWithQueryString;
		}

		protected IPurchasable GetPurchaseableQuote(IShoppingCart cart, IDataAdapterDependencies dependencies, string visitorId)
		{
			var entities = cart.GetCartItems().Select(i => i.Entity).ToArray();

			if (!entities.Any()) { return null; }

			var productIds = entities
				.Select(e => e.GetAttributeValue<EntityReference>("adx_productid"))
				.Where(product => product != null)
				.Select(product => product.Id)
				.ToArray();

			if (!productIds.Any()) { return null; }

			var products = XrmContext.CreateQuery("product")
				.WhereIn(e => e.GetAttributeValue<Guid>("productid"), productIds)
				.ToDictionary(e => e.Id, e => e);

			var lineItems = entities
				.Select(e => LineItem.GetLineItemFromLineItemEntity(e, "adx_productid", null, "adx_specialinstructions", null, null, "adx_quantity", "adx_uomid", products))
				.Where(lineItem => lineItem != null);

			var quote = Adxstudio.Xrm.Commerce.QuoteFunctions.CreateQuote(lineItems, Entity.ToEntityReference(), XrmContext, null, dependencies.GetPortalUser(), dependencies.GetPriceList(), visitorId);

			var dataAdapter = new QuotePurchaseDataAdapter(quote, dependencies);

			var purchasable = quote == null
				? null
				: dataAdapter.Select();

			return purchasable;
		}
	}
}
