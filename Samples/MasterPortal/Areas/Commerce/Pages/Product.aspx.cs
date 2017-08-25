/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Core;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Commerce.Pages
{
	public partial class ProductPage : PortalPage
	{
		protected string VisitorID
		{
			get { return Context.Profile.UserName; }
		}

		private ShoppingCart _cart;

		public ShoppingCart Cart
		{
			get
			{
				if (_cart != null)
				{
					return _cart;
				}

				var user = Contact;

				Entity baseCart = null;

				if (user != null)
				{
					baseCart = XrmContext.GetCartsForContact(user, Website).FirstOrDefault();
				}
				else if (!string.IsNullOrEmpty(VisitorID))
				{
					baseCart = XrmContext.GetCartsForVisitor(VisitorID, Website).FirstOrDefault();
				}

				_cart = baseCart == null ? null : new ShoppingCart(baseCart, XrmContext);

				return _cart;
			}
		}

		public string PriceListName
		{
			get
			{
				var priceListName = ServiceContext.GetDefaultPriceListName(Website.Id);

				return string.IsNullOrWhiteSpace(priceListName) ? "Web" : priceListName;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
			{
				return;
			}

			var context = ServiceContext;

			var webPage = Entity;

			if (webPage == null || webPage.GetAttributeValue<EntityReference>("adx_subjectid") == null)
			{
				return;
			}

			var products =
				from product in context.GetProductsBySubject(webPage.GetAttributeValue<EntityReference>("adx_subjectid").Id)
				select new
				{
					Name = product.GetAttributeValue<string>("name"),
					ProductId = product.GetAttributeValue<Guid>("productid"),
					ProductNumber = product.GetAttributeValue<string>("productnumber"),
					Price = (context.GetProductPriceByPriceListName(product, Website, PriceListName) ?? new Money(0)).Value.ToString("C2")
				};

			Products.DataSource = products;
			Products.DataBind();
		}

		protected void ProductItemCommand(object sender, CommandEventArgs e)
		{
			if (e.CommandName != "AddToCart")
			{
				return;
			}

			if (Cart == null)
			{
				const string nameFormat = "Cart for {0}";

				var cart = new Entity("adx_shoppingcart");
				cart.SetAttributeValue("adx_websiteid", Website.ToEntityReference());

				if (Contact == null)
				{
					cart.SetAttributeValue("adx_name", string.Format(nameFormat, VisitorID));
					cart.SetAttributeValue("adx_visitorid", VisitorID);
				}
				else
				{
					cart.SetAttributeValue("adx_name", string.Format(nameFormat, Contact.GetAttributeValue<string>("fullname")));
					cart.SetAttributeValue("adx_contactid", Contact.ToEntityReference());
				}
				
				XrmContext.AddObject(cart);
				XrmContext.SaveChanges();
			}

			var productId = new Guid(e.CommandArgument.ToString());

			if (Cart == null)
			{
				throw new Exception("Error Processing Cart.");
			}

			Cart.AddProductToCart(productId, PriceListName);

			Response.Redirect(Html.SiteMarkerUrl("Shopping Cart") ?? Request.Url.PathAndQuery);
		}
	}
}
