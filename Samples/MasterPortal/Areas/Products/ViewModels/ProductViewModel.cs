/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Products;

namespace Site.Areas.Products.ViewModels
{
	public class ProductViewModel
	{
		public ProductImageGalleryViewModel ImageGalleryNodes { get; set; }

		public IProduct Product { get; set; }

		public bool UserHasReviewed { get; set; }
	}
}
