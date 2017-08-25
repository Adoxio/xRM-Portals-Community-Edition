/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.Mvc;

namespace Site.Areas.Products
{
	public class ProductsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Products"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute("Product", "products/product/{productIdentifier}", new { controller = "Products", action = "Product", productIdentifier = UrlParameter.Optional });

			context.MapRoute("AddProductToCart", "products/AddProductToCart/{productid}/{quantity}", new { controller = "Products", action = "AddProductToCart", productid = UrlParameter.Optional, quantity = UrlParameter.Optional });

			context.MapRoute("GetProductReviews", "products/GetProductReviews/{productid}", new { controller = "Products", action = "GetProductReviews", productid = UrlParameter.Optional });

			context.MapRoute("CreateProductReview", "products/CreateProductReview/{productid}", new { controller = "Products", action = "CreateReview", productid = UrlParameter.Optional });

			context.MapRoute("ReviewReportAbuse", "products/reviews/ReportAbuse/{reviewid}", new { controller = "Review", action = "ReportAbuse", reviewid = UrlParameter.Optional });
		}
	}
}
