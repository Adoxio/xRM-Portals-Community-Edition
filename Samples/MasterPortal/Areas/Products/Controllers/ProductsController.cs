/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Adxstudio.Xrm;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Products;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Commerce;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Site.Areas.Products.ViewModels;
using PortalConfigurationDataAdapterDependencies = Adxstudio.Xrm.Commerce.PortalConfigurationDataAdapterDependencies;

namespace Site.Areas.Products.Controllers
{
	[PortalView]
	public class ProductsController : Controller
	{
		private const int DefaultPageSize = 10;
		private const int DefaultMaxPageSize = 25;

		public class Review
		{
			public Review(Guid id, string content, Guid productId, double rating, double ratingRationalValue, 
				int ratingMaximumValue, bool recommend, Guid reviewerContactId, string reviewerEmail, string reviewerLocation,
				string reviewerName, DateTime submittedOn, string title)
			{
				Id = id;
				Content = string.IsNullOrWhiteSpace(content) ? string.Empty : content.Replace("\r\n", "<br />");
				ProductId = productId;
				Rating = rating;
				RatingRationalValue = ratingRationalValue;
				RatingMaximumValue = ratingMaximumValue;
				Recommend = recommend;
				ReviewerContactId = reviewerContactId;
				ReviewerEmail = reviewerEmail;
				ReviewerLocation = reviewerLocation;
				ReviewerName = reviewerName;
				SubmittedOn = submittedOn;
				Title = title;
			}

			public Guid Id { get; private set; }
			public string Content { get; private set; }
			public Guid ProductId { get; private set; }
			public double Rating { get; private set; }
			public double RatingRationalValue { get; private set; }
			public int RatingMaximumValue { get; private set; }
			public bool Recommend { get; private set; }
			public Guid ReviewerContactId { get; private set; }
			public string ReviewerEmail { get; private set; }
			public string ReviewerName { get; private set; }
			public string ReviewerLocation { get; private set; }
			public DateTime SubmittedOn { get; private set; }
			public string Title { get; private set; }
		}

		public class ReviewPaginatedViewData
		{
			public ReviewPaginatedViewData(Review[] reviews, int itemCount, int pageSize, int startIndex)
			{
				Reviews = reviews;
				ItemCount = itemCount;
				PageCount = itemCount > 0 ? (int)Math.Ceiling(itemCount / (double)pageSize) : 0;
				PageNumber = startIndex > 0 ? (int)Math.Ceiling(startIndex / (double)pageSize) + 1 : 1;
				PageSize = pageSize;
			}

			public Review[] Reviews { get; set; }
			public int ItemCount { get; set; }
			public int PageCount { get; set; }
			public int PageNumber { get; set; }
			public int PageSize { get; set; }
		}

        [HttpGet]
		public ActionResult Product(string productIdentifier)
		{
			if (string.IsNullOrWhiteSpace(productIdentifier))
			{
				return RedirectToPageNotFound();
			}

			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));

			var product = GetActiveProduct(productIdentifier, context);

			return product == null ? RedirectToPageNotFound() : GetProductView(product);
		}

		[AcceptVerbs(HttpVerbs.Post), AjaxValidateAntiForgeryToken]
		public ActionResult GetProductReviews(string productid, string sortExpression, int startRowIndex = 0, int pageSize = DefaultPageSize)
		{
			Guid productID;
			if (string.IsNullOrEmpty(productid) || !Guid.TryParse(productid, out productID))
			{
				throw new ArgumentException("Please provide a valid product ID.");
			}
			if (pageSize < 0)
			{
				pageSize = DefaultPageSize;
			}
			if (pageSize > DefaultMaxPageSize)
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("pageSize={0} is greater than the allowed maximum page size of {1}. Page size has been constrained to {1}.", pageSize, DefaultMaxPageSize));
				pageSize = DefaultMaxPageSize;
			}
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var product = GetActiveProduct(productID, context);
			var productReviewDataAdapter = new ProductReviewAggregationDataAdapter(product.ToEntityReference(), new Adxstudio.Xrm.Products.PortalContextDataAdapterDependencies(portal, null, Request.RequestContext));
			var reviews =
				productReviewDataAdapter.SelectReviews(startRowIndex, pageSize, sortExpression)
										.Select(
											r =>
											new Review(r.EntityReference != null ? r.EntityReference.Id : Guid.Empty, r.Content,
														r.Product != null ? r.Product.Id : Guid.Empty, r.Rating, r.RatingRationalValue, r.RatingMaximumValue,
														r.Recommend, r.ReviewerContact != null ? r.ReviewerContact.Id : Guid.Empty, r.ReviewerEmail,
														r.ReviewerLocation, r.ReviewerName, r.SubmittedOn, r.Title))
										.ToArray();
			var count = productReviewDataAdapter.SelectReviewCount();
			var data = new ReviewPaginatedViewData(reviews, count, pageSize, startRowIndex);
			var json = Json(data);
			return json;
		}

		[HttpPost, ValidateInput(false), ValidateAntiForgeryToken]
		public ActionResult CreateReview(string productid, string title, string content, double rating, 
			int maximumRatingValue, string reviewerName, string reviewerLocation, string reviewerEmail, bool recommend)
		{
			Guid productID;
			if (string.IsNullOrEmpty(productid) || !Guid.TryParse(productid, out productID))
			{
				throw new ArgumentException("Please provide a valid product ID.");
			}
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = new OrganizationServiceContext(new OrganizationService("Xrm"));
			var product = GetActiveProduct(productID, context);
			var productDataAdapter = new ProductDataAdapter(product, new Adxstudio.Xrm.Products.PortalContextDataAdapterDependencies(portal, null, Request.RequestContext));
			TryCreateReview(productDataAdapter, title, content, rating, maximumRatingValue, reviewerName, reviewerLocation, reviewerEmail, recommend);
			return PartialView("CreateReview", productDataAdapter.Select());
		}

        [HttpGet]
        public ActionResult AddProductToCart(Guid productid, int quantity)
		{
			var visitorID = HttpContext.Profile.UserName;
			ShoppingCart cart;
			var portal = PortalCrmConfigurationManager.CreatePortalContext();
			var context = portal.ServiceContext;
			var product = context.CreateQuery("product").FirstOrDefault(p => p.GetAttributeValue<Guid>("productid") == productid);
			var productDataAdapter = new ProductDataAdapter(product, new Adxstudio.Xrm.Products.PortalContextDataAdapterDependencies(portal, null, Request.RequestContext));
			
			// Check for an existing cart

			var shoppingCartDataAdapter = new ShoppingCartDataAdapter(new PortalConfigurationDataAdapterDependencies(null, Request.RequestContext), visitorID);

			var existingShoppingCart = shoppingCartDataAdapter.SelectCart();
			
			// Create cart if it does not already exist.

			if (existingShoppingCart == null || existingShoppingCart.Entity == null)
			{
				cart = shoppingCartDataAdapter.CreateCart() as ShoppingCart;
			}
			else
			{
				cart = existingShoppingCart as ShoppingCart;
			}

			// Add product to cart and redirect to shopping cart page

			if (cart != null) cart.AddProductToCart(productid, productDataAdapter.Select().PricingInfo.PriceListName, quantity);

			return RedirectToShoppingCart();
		}

		private static Entity GetActiveProduct(string productIdentifier, OrganizationServiceContext context)
		{
			var product = context.CreateQuery("product")
						.FirstOrDefault(p => p.GetAttributeValue<OptionSetValue>("statecode") != null && p.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ProductState.Active &&
							(p.GetAttributeValue<string>("adx_partialurl") == productIdentifier ||
							 p.GetAttributeValue<string>("productnumber") == productIdentifier));

			return product;
		}

		private static Entity GetActiveProduct(Guid productId, OrganizationServiceContext context)
		{
			var product = context.CreateQuery("product")
						.FirstOrDefault(p => p.GetAttributeValue<OptionSetValue>("statecode") != null && p.GetAttributeValue<OptionSetValue>("statecode").Value == (int)ProductState.Active &&
							p.GetAttributeValue<Guid>("productid") == productId);

			return product;
		}

		private ActionResult GetProductView(Entity product)
		{
			var context = PortalCrmConfigurationManager.CreatePortalContext();

			var productDataAdapter = new ProductDataAdapter(product.ToEntityReference(), new Adxstudio.Xrm.Products.PortalContextDataAdapterDependencies(context, null, Request.RequestContext));

			var imageGalleryNodes = new List<IProductImageGalleryNode>(productDataAdapter.SelectImageGalleryNodes());
			var iproduct = productDataAdapter.Select();
			
			var productViewModel = new ProductViewModel
			{
				Product = iproduct,
				ImageGalleryNodes = new ProductImageGalleryViewModel { ImageGalleryNodes = imageGalleryNodes, Product = iproduct },
				UserHasReviewed = HttpContext.Request.IsAuthenticated ? productDataAdapter.HasReview(context.User.ToEntityReference()) : productDataAdapter.HasReview(HttpContext.Request.AnonymousID)
			};

			return View("Product", productViewModel);
		}

		private ActionResult RedirectToPageNotFound()
		{
			const string pageNotFoundSiteMarker = "Page Not Found";

			var context = PortalCrmConfigurationManager.CreatePortalContext();

			var website = context.ServiceContext.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == context.Website.Id);

			var page = context.ServiceContext.GetPageBySiteMarkerName(website, pageNotFoundSiteMarker);

			if (page == null)
			{
				throw new Exception("Please contact your system administrator. The required site marker {0} is missing.".FormatWith(pageNotFoundSiteMarker));
			}

			var path = context.ServiceContext.GetUrl(page);

			if (path == null)
			{
                throw new Exception("Please contact your System Administrator. Unable to build URL for Site Marker {0}.".FormatWith(pageNotFoundSiteMarker));
			}

			return Redirect(path);
		}

		private ActionResult RedirectToShoppingCart()
		{
			const string shoppingCartSiteMarker = "Shopping Cart";

			var context = PortalCrmConfigurationManager.CreatePortalContext();

			var website = context.ServiceContext.CreateQuery("adx_website").FirstOrDefault(w => w.GetAttributeValue<Guid>("adx_websiteid") == context.Website.Id);

			var page = context.ServiceContext.GetPageBySiteMarkerName(website, shoppingCartSiteMarker);

			if (page == null)
			{
				throw new Exception("Please contact your system administrator. The required site marker {0} is missing.".FormatWith(shoppingCartSiteMarker));
			}

			var path = context.ServiceContext.GetUrl(page);

			if (path == null)
			{
                throw new Exception("Please contact your System Administrator. Unable to build URL for Site Marker {0}.".FormatWith(shoppingCartSiteMarker));
			}

			return Redirect(path);
		}

		private bool TryCreateReview(IProductDataAdapter productDataAdapter, string title, string content, double rating, 
			int maximumRatingValue, string reviewerName, string reviewerLocation, string reviewerEmail, bool recommend)
		{
			if (string.IsNullOrWhiteSpace(reviewerName))
			{
				ModelState.AddModelError("reviewerName", ResourceManager.GetString("Required_Nick_Name"));
			}

			if (!Request.IsAuthenticated)
			{
				if (string.IsNullOrWhiteSpace(reviewerEmail))
				{
					ModelState.AddModelError("reviewerEmail", ResourceManager.GetString("Required_Email"));
				}
			}

			if (string.IsNullOrWhiteSpace(title))
			{
				ModelState.AddModelError("title", ResourceManager.GetString("Required_Title"));
			}

			if (rating < 1)
			{
				ModelState.AddModelError("rating", ResourceManager.GetString("Required_Rating"));
			}

			if (!ModelState.IsValid)
			{
				return false;
			}

			productDataAdapter.CreateReview(title, content, rating, maximumRatingValue, reviewerName, reviewerLocation, reviewerEmail, recommend);

			return true;
		}
	}
}
