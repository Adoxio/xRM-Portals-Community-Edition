/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Products
{
	/// <summary>
	/// Represents a node for a given product sales attachment
	/// </summary>
	public class ProductImageGalleryNode : IProductImageGalleryNode
	{
		public ProductImageGalleryNode(ISalesAttachment imageSalesAttachment, ISalesAttachment thumbnailImageSalesAttachment, string imageURL, string thumbnailImageURL)
		{
			ImageSalesAttachment = imageSalesAttachment;
			ImageURL = imageURL;
			ThumbnailImageSalesAttachment = thumbnailImageSalesAttachment;
			ThumbnailImageURL = thumbnailImageURL;
		}

		public ISalesAttachment ImageSalesAttachment { get; set; }
		public string ImageURL { get; set; }
		public ISalesAttachment ThumbnailImageSalesAttachment { get; set; }
		public string ThumbnailImageURL { get; set; }
		public string Title
		{
			get { return ImageSalesAttachment.Title; }
		}
	}
}
