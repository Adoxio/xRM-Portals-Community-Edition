/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Products;

namespace Site.Areas.Products.ViewModels
{
	public class ProductImageGalleryViewModel
	{
		public List<IProductImageGalleryNode> ImageGalleryNodes { get; set; }

		public IProduct Product { get; set; }
	}
}
